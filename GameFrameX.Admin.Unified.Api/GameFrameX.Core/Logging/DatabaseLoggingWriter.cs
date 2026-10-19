using System.Text.Json.Nodes;
using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Service.PlatformLogging;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Log;
using IPTools.Core;

namespace GameFrameX.Core.Logging;

/// <summary>
/// 数据库日志写入器。C156 起：日志不再写入独立日志库，而是按当前运行上下文分流——
/// 平台上下文（SuperAdmin / 平台启动 / 无租户上下文）写 <see cref="SystemLogOperationEntity"/> 等平台日志到主库，
/// 租户上下文（租户管理员 / 租户业务接口）写 <see cref="TenantLogOperationEntity"/> 等租户日志到当前租户库。
/// 写入器运行在独立 <see cref="IServiceScope"/>，无 HTTP 租户上下文，因此租户库连接通过
/// <see cref="TenantDbConnectionProvider.GetTenantDbConnectionScope"/> 显式按日志中解析到的租户 Id 解析。
/// </summary>
public class DatabaseLoggingWriter : IDatabaseLoggingWriter, IDisposable
{
    private readonly IServiceScope _serviceScope;
    private readonly SqlSugarRepository<SystemTenantEntity> _systemTenantRepository; // 租户
    private readonly SystemConfigService _systemConfigService; // 参数配置服务
    private readonly IEventPublisher _eventPublisher;

    public DatabaseLoggingWriter(IServiceScopeFactory scopeFactory)
    {
        _serviceScope = scopeFactory.CreateScope();
        _systemTenantRepository = _serviceScope.ServiceProvider.GetRequiredService<SqlSugarRepository<SystemTenantEntity>>();
        _systemConfigService = _serviceScope.ServiceProvider.GetRequiredService<SystemConfigService>();
        _eventPublisher = _serviceScope.ServiceProvider.GetRequiredService<IEventPublisher>();
    }

    public async Task WriteAsync(IReadOnlyList<LogMessage> logMsgs, bool flush)
    {
        // C171：同 Host 动态关停——保存后快照 database.enabled=false 时，写路径立即停止落库。
        var loggingConfig = _serviceScope.ServiceProvider.GetService<PlatformLoggingConfigProvider>();
        if (loggingConfig != null && !loggingConfig.GetSnapshot().Database.IsEnabled)
        {
            return;
        }

        foreach (var logMsg in logMsgs)
        {
            await WriteSingleAsync(logMsg);
        }
    }

    private async Task WriteSingleAsync(LogMessage logMsg)
    {
        var jsonStr = logMsg.Context?.Get("loggingMonitor")?.ToString();
        if (string.IsNullOrWhiteSpace(jsonStr))
        {
            return;
        }

        var loggingMonitor = JsonNode.Parse(jsonStr)!.AsObject();

        // 不记录数据校验日志
        if (loggingMonitor["validation"] != null)
        {
            return;
        }

        // 获取当前操作者
        string account = "", realName = "", userId = "", tenantId = "", accountType = "";
        var authorizationClaims = loggingMonitor["authorizationClaims"]?.AsArray();
        if (authorizationClaims != null)
        {
            foreach (var item in authorizationClaims)
            {
                if (item == null)
                {
                    continue;
                }

                var claimType = (string?)item["type"];
                if (claimType == ClaimConst.Account)
                {
                    account = (string?)item["value"] ?? string.Empty;
                }

                if (claimType == ClaimConst.RealName)
                {
                    realName = (string?)item["value"] ?? string.Empty;
                }

                if (claimType == ClaimConst.TenantId)
                {
                    tenantId = (string?)item["value"] ?? string.Empty;
                }

                if (claimType == ClaimConst.UserId)
                {
                    userId = (string?)item["value"] ?? string.Empty;
                }

                if (claimType == ClaimConst.AccountType)
                {
                    accountType = (string?)item["value"] ?? string.Empty;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            TryFillTenantFromSuccessfulLoginParameters(
                (string?)loggingMonitor["actionTypeName"],
                loggingMonitor["parameters"]?.ToJsonString(),
                loggingMonitor["returnInformation"]?.ToJsonString(),
                ref account,
                ref tenantId);
        }

        var remoteIPv4 = (string?)loggingMonitor["remoteIPv4"] ?? string.Empty;
        var ipAddressInfo = GetIpAddress(remoteIPv4);
        var ipLocation = ipAddressInfo.IpLocation;
        var longitude = ipAddressInfo.Longitude;
        var latitude = ipAddressInfo.Latitude;

        var client = Parser.GetDefault().Parse((string?)loggingMonitor["userAgent"] ?? string.Empty);
        var browser = $"{client.UA.Family} {client.UA.Major}.{client.UA.Minor} / {client.Device.Family}";
        var os = $"{client.OS.Family} {client.OS.Major} {client.OS.Minor}";

        var ctx = new LogContext
        {
            Monitor = loggingMonitor,
            LogMsg = logMsg,
            Account = account,
            RealName = realName,
            UserId = userId,
            TenantId = tenantId,
            RemoteIp = remoteIPv4,
            IpLocation = ipLocation,
            Longitude = longitude,
            Latitude = latitude,
            Browser = browser,
            Os = os,
        };

        var target = ResolveLogContext(tenantId, accountType);
        var db = target.Db;
        var usePlatformEntity = target.UsePlatformEntity;

        // 记录异常日志并发送邮件
        if (logMsg.Exception != null || loggingMonitor["exception"] != null)
        {
            if (usePlatformEntity)
            {
                var entity = new SystemLogExceptionEntity();
                FillOperationFields(entity, ctx);
                entity.Exception = loggingMonitor["exception"]?.ToJsonString();
                await db.Insertable(entity).ExecuteCommandAsync();
            }
            else
            {
                var entity = new TenantLogExceptionEntity();
                FillOperationFields(entity, ctx);
                entity.Exception = loggingMonitor["exception"]?.ToJsonString();
                await db.Insertable(entity).ExecuteCommandAsync();
            }

            try
            {
                await _eventPublisher.PublishAsync("Send:ErrorMail", loggingMonitor["exception"]);
            }
            catch
            {
            }

            return;
        }

        // 记录访问日志-登录登出
        if ((string?)loggingMonitor["actionName"] == "userInfo" || (string?)loggingMonitor["actionName"] == "logout")
        {
            if (usePlatformEntity)
            {
                var entity = new SystemLogVisitEntity();
                FillVisitFields(entity, ctx);
                await db.Insertable(entity).ExecuteCommandAsync();
            }
            else
            {
                var entity = new TenantLogVisitEntity();
                FillVisitFields(entity, ctx);
                await db.Insertable(entity).ExecuteCommandAsync();
            }

            return;
        }

        // 记录操作日志
        var systemOpLogEnabled = await _systemConfigService.GetConfigValueAsync<bool>(CommonConst.SysOpLog);
        if (!systemOpLogEnabled)
        {
            return;
        }

        if (usePlatformEntity)
        {
            var entity = new SystemLogOperationEntity();
            FillOperationFields(entity, ctx);
            entity.Exception = loggingMonitor["exception"]?.ToJsonString();
            await db.Insertable(entity).ExecuteCommandAsync();
        }
        else
        {
            var entity = new TenantLogOperationEntity();
            FillOperationFields(entity, ctx);
            entity.Exception = loggingMonitor["exception"]?.ToJsonString();
            await db.Insertable(entity).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 解析日志写入目标库与实体归属。平台上下文（SuperAdmin 或无租户上下文）写入主库并使用 SystemLog* 实体；
    /// 租户上下文写入当前租户库并使用 TenantLog* 实体。租户库未就绪时归类为平台日志，避免日志写入失败影响业务线程。
    /// </summary>
    /// <param name="tenantId">从日志 claims/登录参数解析到的租户 Id 字符串（可能为空）。</param>
    /// <param name="accountType">从日志 claims 解析到的账号类型（SuperAdmin 判定依据）。</param>
    /// <returns>写入目标连接与是否使用平台日志实体的标志。</returns>
    private LogWriteTarget ResolveLogContext(string tenantId, string accountType)
    {
        var sqlSugarClient = _serviceScope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
        var tenant = sqlSugarClient.AsTenant();
        var isSuperAdmin = accountType == ((int)AccountTypeEnum.SuperAdmin).ToString();
        var tenantIdValue = string.IsNullOrWhiteSpace(tenantId) ? 0 : long.Parse(tenantId);
        var mainDb = tenant.GetConnectionScope(SqlSugarConst.MainConfigId);

        if (isSuperAdmin || tenantIdValue == 0)
        {
            return new LogWriteTarget { Db = mainDb, UsePlatformEntity = true, };
        }

        var provider = _serviceScope.ServiceProvider.GetRequiredService<TenantDbConnectionProvider>();
        var tenantDb = provider.GetTenantDbConnectionScope(tenantIdValue);
        if (tenantDb != null)
        {
            return new LogWriteTarget { Db = tenantDb, UsePlatformEntity = false, };
        }

        // 租户库未就绪：归类为平台日志写入主库，不静默兜底到任意默认租户库（C156 边界要求）。
        return new LogWriteTarget { Db = mainDb, UsePlatformEntity = true, };
    }

    /// <summary>
    /// 填充访问日志公共字段（SystemLogVisitEntity / TenantLogVisitEntity 共享 LogVisitFields）。
    /// </summary>
    private static void FillVisitFields(LogVisitFields target, LogContext ctx)
    {
        var monitor = ctx.Monitor;
        target.ControllerName = (string?)monitor["controllerName"];
        target.ActionName = (string?)monitor["actionTypeName"];
        target.DisplayTitle = (string?)monitor["displayTitle"];
        target.Status = monitor["returnInformation"]?["httpStatusCode"]?.ToJsonString();
        target.RemoteIp = ctx.RemoteIp;
        target.Location = ctx.IpLocation;
        target.Longitude = ctx.Longitude;
        target.Latitude = ctx.Latitude;
        target.Browser = ctx.Browser;
        target.Os = ctx.Os;
        target.Elapsed = monitor["timeOperationElapsedMilliseconds"]?.GetValue<long>();
        target.LogDateTime = TimerHelper.DateTimeToSeconds(ctx.LogMsg.LogDateTime.ToUniversalTime(), true);
        target.Account = ctx.Account;
        target.RealName = ctx.RealName;
        target.CreatedId = string.IsNullOrWhiteSpace(ctx.UserId) ? 0 : long.Parse(ctx.UserId);
        target.TenantId = string.IsNullOrWhiteSpace(ctx.TenantId) ? 0 : long.Parse(ctx.TenantId);
        target.LogLevel = ctx.LogMsg.LogLevel;
    }

    /// <summary>
    /// 填充操作/异常日志公共字段（SystemLogOperationEntity/Exception、TenantLogOperationEntity/Exception 共享 LogOperationFields）。
    /// Exception 字段由调用方按日志种类决定（异常日志始终序列化，操作日志空值保持 null）。
    /// </summary>
    private static void FillOperationFields(LogOperationFields target, LogContext ctx)
    {
        FillVisitFields(target, ctx);
        var monitor = ctx.Monitor;
        target.HttpMethod = (string?)monitor["httpMethod"];
        target.RequestUrl = (string?)monitor["requestUrl"];
        var parameters = monitor["parameters"]?.AsArray();
        target.RequestParam = parameters == null || parameters.Count == 0 ? null : parameters[0]?["value"]?.ToJsonString();
        target.ReturnResult = monitor["returnInformation"]?.ToJsonString();
        target.EventId = ctx.LogMsg.EventId.Id;
        target.ThreadId = ctx.LogMsg.ThreadId;
        target.TraceId = ctx.LogMsg.TraceId;
        target.Message = ctx.LogMsg.Message;
    }

    private void TryFillTenantFromSuccessfulLoginParameters(
        string? actionTypeName,
        string? parametersJson,
        string? returnInformationJson,
        ref string account,
        ref string tenantId)
    {
        if (!string.Equals(actionTypeName, "Login", StringComparison.Ordinal))
        {
            return;
        }

        if (!IsSuccessfulUnifiedReturn(returnInformationJson))
        {
            return;
        }

        var scope = TryReadLoginScope(parametersJson);
        if (scope == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(account) && !string.IsNullOrWhiteSpace(scope.Account))
        {
            account = scope.Account;
        }

        if (scope.TenantId > 0)
        {
            tenantId = scope.TenantId.ToString();
            return;
        }

        if (string.IsNullOrWhiteSpace(scope.TenantHost))
        {
            return;
        }

        var resolvedTenantId = _systemTenantRepository.AsQueryable()
                                               .Filter(null, true)
                                               .Where(u => u.Host == scope.TenantHost)
                                               .Select(u => (long?)u.Id)
                                               .First();
        if (resolvedTenantId.HasValue && resolvedTenantId.Value > 0)
        {
            tenantId = resolvedTenantId.Value.ToString();
        }
    }

    /// <summary>
    /// 判断统一返回值是否为成功结果。
    /// </summary>
    /// <param name="returnInformationJson">日志监控记录的返回信息 JSON。</param>
    /// <returns>是否成功。</returns>
    internal static bool IsSuccessfulUnifiedReturn(string? returnInformationJson)
    {
        if (string.IsNullOrWhiteSpace(returnInformationJson))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(returnInformationJson);
            var root = document.RootElement;
            return TryGetProperty(root, "httpStatusCode", out var statusCode)
                   && statusCode.TryGetInt32(out var httpStatusCode)
                   && httpStatusCode == StatusCodes.Status200OK;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 从登录参数中读取租户范围。
    /// </summary>
    /// <param name="parametersJson">日志监控记录的参数 JSON。</param>
    /// <returns>登录范围。</returns>
    internal static LoginScope? TryReadLoginScope(string? parametersJson)
    {
        if (string.IsNullOrWhiteSpace(parametersJson))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(parametersJson);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            foreach (var parameter in document.RootElement.EnumerateArray())
            {
                if (!TryGetProperty(parameter, "value", out var value)
                    || value.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                var scope = new LoginScope();
                if (TryGetProperty(value, "account", out var account) && account.ValueKind == JsonValueKind.String)
                {
                    scope.Account = account.GetString();
                }

                if (TryGetProperty(value, "tenantId", out var tenantId)
                    && tenantId.ValueKind == JsonValueKind.Number
                    && tenantId.TryGetInt64(out var id))
                {
                    scope.TenantId = id;
                }

                if (TryGetProperty(value, "tenantHost", out var tenantHost) && tenantHost.ValueKind == JsonValueKind.String)
                {
                    scope.TenantHost = tenantHost.GetString();
                }

                if (!string.IsNullOrWhiteSpace(scope.Account) || scope.TenantId > 0 || !string.IsNullOrWhiteSpace(scope.TenantHost))
                {
                    return scope;
                }
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    internal sealed class LoginScope
    {
        public string? Account { get; set; }

        public long TenantId { get; set; }

        public string? TenantHost { get; set; }
    }

    /// <summary>
    /// 日志写入目标（连接 + 是否使用平台日志实体），由 <see cref="ResolveLogContext"/> 按运行上下文解析。
    /// 用具名类型承载多值返回，避免 C# 元组语法（项目架构约束禁用元组）。
    /// </summary>
    private sealed class LogWriteTarget
    {
        public SqlSugarScopeProvider Db { get; set; } = null!;

        public bool UsePlatformEntity { get; set; }
    }

    /// <summary>
    /// 日志字段填充上下文，封装从 loggingMonitor 与 claims 解析出的写入源数据。
    /// </summary>
    private sealed class LogContext
    {
        public JsonObject Monitor { get; set; } = null!;

        public LogMessage LogMsg { get; set; } = null!;

        public string Account { get; set; } = null!;

        public string RealName { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public string TenantId { get; set; } = null!;

        public string RemoteIp { get; set; } = null!;

        public string IpLocation { get; set; } = null!;

        public double? Longitude { get; set; }

        public double? Latitude { get; set; }

        public string Browser { get; set; } = null!;

        public string Os { get; set; } = null!;
    }

    private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    /// <summary>
    /// 解析IP地址
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    private static IpAddressInfo GetIpAddress(string ip)
    {
        try
        {
            var ipInfo = IpTool.Search(ip);
            var addressList = new List<string> { ipInfo.Country, ipInfo.Province, ipInfo.City, ipInfo.NetworkOperator, };
            return new IpAddressInfo(string.Join("|", addressList.Where(it => it != "0").ToList()), ipInfo.Longitude, ipInfo.Latitude); // 去掉0并用|连接
        }
        catch
        {
        }

        return new IpAddressInfo("未知", 0, 0);
    }

    /// <summary>
    /// 释放服务作用域
    /// </summary>
    public void Dispose()
    {
        _serviceScope.Dispose();
    }
}