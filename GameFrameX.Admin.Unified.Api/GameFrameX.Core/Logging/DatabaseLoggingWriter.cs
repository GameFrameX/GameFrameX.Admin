using System.Text.Json.Nodes;
using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Service.PlatformLogging;
using GameFrameX.Core.Service.Business;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Log;
using IPTools.Core;

namespace GameFrameX.Core.Logging;

/// <summary>
/// 主库日志写入器。访问、操作和异常分别写入 SystemLog 表，历史日志表继续供审计查询。
/// </summary>
public class DatabaseLoggingWriter : IDatabaseLoggingWriter, IDisposable
{
    private readonly IServiceScope _serviceScope;
    private readonly SystemConfigService _systemConfigService; // 参数配置服务
    private readonly IEventPublisher _eventPublisher;

    public DatabaseLoggingWriter(IServiceScopeFactory scopeFactory)
    {
        _serviceScope = scopeFactory.CreateScope();
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

        var loggingMonitor = JsonNode.Parse(SensitiveLogRedactor.Redact(jsonStr)!)!.AsObject();

        // 不记录数据校验日志
        if (loggingMonitor["validation"] != null)
        {
            return;
        }

        // 获取当前操作者
        string account = "", realName = "", userId = "";
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

                if (claimType == ClaimConst.UserId)
                {
                    userId = (string?)item["value"] ?? string.Empty;
                }

            }
        }

        if (string.IsNullOrWhiteSpace(account))
        {
            TryFillAccountFromSuccessfulLoginParameters(
                (string?)loggingMonitor["actionTypeName"],
                loggingMonitor["parameters"]?.ToJsonString(),
                loggingMonitor["returnInformation"]?.ToJsonString(),
                ref account);
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
            RemoteIp = remoteIPv4,
            IpLocation = ipLocation,
            Longitude = longitude,
            Latitude = latitude,
            Browser = browser,
            Os = os,
        };

        var sqlSugarClient = _serviceScope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
        var db = sqlSugarClient.AsTenant().GetConnectionScope(SqlSugarConst.MainConfigId);

        // 异常日志优先记录，并继续发布错误邮件事件。
        if (logMsg.Exception != null || loggingMonitor["exception"] != null)
        {
            var entity = new SystemLogExceptionEntity();
            FillOperationFields(entity, ctx);
            entity.Exception = loggingMonitor["exception"]?.ToJsonString();
            await db.Insertable(entity).ExecuteCommandAsync();
            try
            {
                await _eventPublisher.PublishAsync("Send:ErrorMail", loggingMonitor["exception"]);
            }
            catch
            {
            }

            return;
        }

        if ((string?)loggingMonitor["actionName"] == "userInfo" || (string?)loggingMonitor["actionName"] == "logout")
        {
            var entity = new SystemLogVisitEntity();
            FillVisitFields(entity, ctx);
            await db.Insertable(entity).ExecuteCommandAsync();
            return;
        }

        if (!await _systemConfigService.GetConfigValueAsync<bool>(CommonConst.SysOpLog))
        {
            return;
        }

        var operation = new SystemLogOperationEntity();
        FillOperationFields(operation, ctx);
        operation.Exception = loggingMonitor["exception"]?.ToJsonString();
        await db.Insertable(operation).ExecuteCommandAsync();
    }

    /// <summary>
    /// 填充访问日志公共字段（SystemLogVisitEntity / LogVisitEntity 共享 LogVisitFields）。
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
        target.LogLevel = ctx.LogMsg.LogLevel;
    }

    /// <summary>
    /// 填充操作/异常日志公共字段（SystemLogOperationEntity/Exception、LogOperationEntity/Exception 共享 LogOperationFields）。
    /// Exception 字段由调用方按日志种类决定（异常日志始终序列化，操作日志空值保持 null）。
    /// </summary>
    private static void FillOperationFields(LogOperationFields target, LogContext ctx)
    {
        FillVisitFields(target, ctx);
        var monitor = ctx.Monitor;
        target.HttpMethod = (string?)monitor["httpMethod"];
        target.RequestUrl = SensitiveLogRedactor.Redact((string?)monitor["requestUrl"]);
        var parameters = monitor["parameters"]?.AsArray();
        target.RequestParam = parameters == null || parameters.Count == 0 ? null : parameters[0]?["value"]?.ToJsonString();
        target.ReturnResult = monitor["returnInformation"]?.ToJsonString();
        target.EventId = ctx.LogMsg.EventId.Id;
        target.ThreadId = ctx.LogMsg.ThreadId;
        target.TraceId = ctx.LogMsg.TraceId;
        target.Message = SensitiveLogRedactor.Redact(ctx.LogMsg.Message);
    }

    private void TryFillAccountFromSuccessfulLoginParameters(
        string? actionTypeName,
        string? parametersJson,
        string? returnInformationJson,
        ref string account)
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
            using (var document = JsonDocument.Parse(returnInformationJson))
            {
                var root = document.RootElement;
                return TryGetProperty(root, "httpStatusCode", out var statusCode)
                       && statusCode.TryGetInt32(out var httpStatusCode)
                       && httpStatusCode == StatusCodes.Status200OK;
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 从登录参数中读取操作账号。
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
            using (var document = JsonDocument.Parse(parametersJson))
            {
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

                    if (!string.IsNullOrWhiteSpace(scope.Account))
                    {
                        return scope;
                    }
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