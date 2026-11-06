using System.Globalization;
using System.Threading;
using GameFrameX.Application.ServerCenter.GameArea;
using GameFrameX.Application.ServerCenter.GameAreaProvision.Dto;
using GameFrameX.Application.ServerCenter.Online;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.ServerCenter.GameAreaProvision;

/// <summary>开服向导：禁用行落库、创建容器、登记 Online 开服锚点，最后启用。</summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 110)]
public sealed class GameAreaProvisionService : IAdminApiController
{
    // 单宿主内串行编排；跨宿主以主键唯一、稳定容器名称、ServerId 注册幂等和行版本兜底。
    private static readonly SemaphoreSlim ExecutionGate = new(1, 1);
    private readonly SqlSugarRepository<GameAreaEntity> _repository;
    private readonly GameAreaService _areaService;
    private readonly OnlineScopeContextFactory _scopeFactory;
    private readonly IOnlineServerClient _onlineClient;

    public GameAreaProvisionService(SqlSugarRepository<GameAreaEntity> repository,
        GameAreaService areaService, OnlineScopeContextFactory scopeFactory, IOnlineServerClient onlineClient)
    {
        _repository = repository;
        _areaService = areaService;
        _scopeFactory = scopeFactory;
        _onlineClient = onlineClient;
    }

    /// <summary>获取可选模板（包含禁用行，但不包含已软删行）。</summary>
    [HttpGet("templates")]
    [DisplayName("开服模板")]
    public async Task<List<GameAreaProvisionTemplateResponse>> TemplatesAsync()
    {
        return await _repository.AsQueryable().Filter(null, true).Where(area => area.IsDeleted == false)
            .OrderBy(area => area.Id).Select(area => new GameAreaProvisionTemplateResponse { Id = area.Id, Name = area.Name }).ToListAsync();
    }

    /// <summary>预览显式克隆字段及必须填写的差异字段，不复制状态与运行时标识。</summary>
    [HttpGet("preview")]
    [DisplayName("预览开服配置")]
    public async Task<GameAreaProvisionPreviewResponse> PreviewAsync([FromQuery] BaseIdRequest input)
    {
        ValidateIdentifier(input);
        var template = await FindAsync(input.Id) ?? throw BusinessError.Oh(AreaProvisionErrorCode.TemplateNotFound);
        var clone = CloneConfiguration(template);
        var fields = typeof(GameAreaEntity).GetProperties()
            .Where(property => ClonedFieldNames.Contains(property.Name))
            .ToDictionary(property => property.Name, property => Convert.ToString(property.GetValue(clone), CultureInfo.InvariantCulture));
        fields[nameof(GameAreaProvisionCreateRequest.ServerParameters)] = template.ServerParams;
        return new GameAreaProvisionPreviewResponse
        {
            TemplateId = template.Id,
            ClonedFields = fields,
            RequiredFields = new[] { "ServerId", "Name", "HttpManageUrl", "TcpSocketUrl", "WebSocketUrl", "ServerParameters" },
        };
    }

    /// <summary>以新区服编号为幂等键创建禁用行；同编号但参数不同的请求拒绝覆盖。</summary>
    [HttpPost("create")]
    [DisplayName("创建开服区服")]
    public async Task<GameAreaProvisionStatusResponse> CreateAsync(GameAreaProvisionCreateRequest input)
    {
        ValidateCreate(input);
        await ExecutionGate.WaitAsync();
        try
        {
            var existing = await _repository.AsQueryable().Filter(null, true).FirstAsync(area => area.Id == input.ServerId);
            if (existing != null)
            {
                if (existing.IsDeleted == true || !MatchesRequest(existing, input))
                {
                    throw BusinessError.Oh(AreaProvisionErrorCode.ServerConflict);
                }

                return BuildStatus(existing);
            }

            var template = await FindAsync(input.TemplateId) ?? throw BusinessError.Oh(AreaProvisionErrorCode.TemplateNotFound);
            var area = CloneConfiguration(template);
            area.Id = input.ServerId;
            area.Name = input.Name.Trim();
            area.HttpManageUrl = input.HttpManageUrl.Trim().TrimEnd('/');
            area.TcpSocketUrl = input.TcpSocketUrl.Trim();
            area.WebSocketUrl = input.WebSocketUrl.Trim();
            area.ServerParams = MergeServerParameters(template.ServerParams, input.ServerParameters);
            area.IsEnabled = false;
            area.ServerStatus = GameServerStatus.Offline;
            area.ServerContainerId = string.Empty;
            area.CreationTime = 0;
            area.CurrentLoad = 0;
            area.ServerEvents = "[]";
            area.IsDefault = false;
            area.IsRecommend = false;
            area.EnableClientGmApi = false;
            area.IsDeleted = false;
            area.RowVersion = 1;
            area.CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            try
            {
                await _repository.Context.Insertable(area).ExecuteCommandAsync();
            }
            catch (Exception exception)
            {
                // 另一个宿主可能已写入同一幂等键，只有完全相同的请求才可继续。
                existing = await FindAsync(input.ServerId);
                if (existing != null && MatchesRequest(existing, input))
                {
                    return BuildStatus(existing);
                }

                throw BusinessError.Oh(AreaProvisionErrorCode.PersistenceFailed, new { serverId = input.ServerId, reason = exception.Message });
            }

            return BuildStatus(area);
        }
        finally
        {
            ExecutionGate.Release();
        }
    }

    /// <summary>从尚未完成的步骤继续，失败保留已有事实，绝不提前启用。</summary>
    [HttpPost("execute")]
    [DisplayName("执行开服")]
    public async Task<GameAreaProvisionStatusResponse> ExecuteAsync(BaseIdRequest input)
    {
        ValidateIdentifier(input);
        await ExecutionGate.WaitAsync();
        try
        {
            var area = await RequireAreaAsync(input.Id);
            if (area.IsEnabled == true)
            {
                return BuildStatus(area, await QueryCalendarAsync(area));
            }

            if (area.CreationTime > 0)
            {
                throw BusinessError.Oh(AreaProvisionErrorCode.ServerAlreadyClosed);
            }

            if (string.IsNullOrWhiteSpace(area.ServerContainerId))
            {
                try
                {
                    await _areaService.OpenAsync(input);
                    area = await RequireAreaAsync(input.Id);
                    if (string.IsNullOrWhiteSpace(area.ServerContainerId))
                    {
                        throw BusinessError.Oh(AreaProvisionErrorCode.ContainerFailed);
                    }
                }
                catch (Exception exception)
                {
                    throw BusinessError.Oh(AreaProvisionErrorCode.ContainerFailed, new { serverId = input.Id, reason = exception.Message });
                }
            }

            var calendar = await QueryCalendarAsync(area);
            if (calendar.MergedIntoServerId > 0)
            {
                throw BusinessError.Oh(AreaProvisionErrorCode.ServerAlreadyClosed);
            }

            if (!calendar.Registered)
            {
                try
                {
                    var scope = await _scopeFactory.CreateForProvisioningAsync(area.Id, "register_server_open");
                    var registration = await _onlineClient.PostAsync<GameAreaProvisionRegistrationResponse>(
                        "register_server_open", new { }, scope);
                    if (registration == null || registration.ServerId != area.Id || registration.OpenedAtUnixMilliseconds <= 0)
                    {
                        throw BusinessError.Oh(AreaProvisionErrorCode.RegistrationFailed);
                    }

                    calendar = new GameAreaProvisionCalendarResponse
                    {
                        ServerId = area.Id, Registered = true, OpenedAtUnixMilliseconds = registration.OpenedAtUnixMilliseconds,
                    };
                }
                catch (Exception exception)
                {
                    throw BusinessError.Oh(AreaProvisionErrorCode.RegistrationFailed, new { serverId = input.Id, reason = exception.Message });
                }
            }

            // 同一个乐观锁更新原子落下锚点与启用；外部修改行版本时拒绝覆盖。
            area.CreationTime = calendar.OpenedAtUnixMilliseconds / 1000;
            area.ServerStatus = GameServerStatus.Online;
            area.IsEnabled = true;
            try
            {
                await _repository.Context.UpdateWithOptimisticLockAsync(area);
            }
            catch (Exception exception)
            {
                throw BusinessError.Oh(AreaProvisionErrorCode.PersistenceFailed, new { serverId = input.Id, reason = exception.Message });
            }

            return BuildStatus(area, calendar);
        }
        finally
        {
            ExecutionGate.Release();
        }
    }

    /// <summary>从区服记录和 Online 日历查询四个步骤的事实状态。</summary>
    [HttpGet("status")]
    [DisplayName("查询开服状态")]
    public async Task<GameAreaProvisionStatusResponse> StatusAsync([FromQuery] BaseIdRequest input)
    {
        ValidateIdentifier(input);
        var area = await RequireAreaAsync(input.Id);
        return BuildStatus(area, string.IsNullOrWhiteSpace(area.ServerContainerId) ? null : await QueryCalendarAsync(area));
    }

    private async Task<GameAreaProvisionCalendarResponse> QueryCalendarAsync(GameAreaEntity area)
    {
        try
        {
            var scope = area.IsEnabled == true
                ? await _scopeFactory.CreateAsync(area.Id, "query_server_calendar")
                : await _scopeFactory.CreateForProvisioningAsync(area.Id, "query_server_calendar");
            var calendar = await _onlineClient.PostAsync<GameAreaProvisionCalendarResponse>("query_server_calendar", new { }, scope);
            if (calendar == null || calendar.ServerId != area.Id || (calendar.Registered && calendar.OpenedAtUnixMilliseconds <= 0))
            {
                throw BusinessError.Oh(AreaProvisionErrorCode.CalendarFailed);
            }

            return calendar;
        }
        catch (Exception exception)
        {
            throw BusinessError.Oh(AreaProvisionErrorCode.CalendarFailed, new { serverId = area.Id, reason = exception.Message });
        }
    }

    private Task<GameAreaEntity> FindAsync(long identifier)
    {
        return _repository.AsQueryable().Filter(null, true).Where(area => area.IsDeleted == false).FirstAsync(area => area.Id == identifier);
    }

    private async Task<GameAreaEntity> RequireAreaAsync(long identifier)
    {
        return await FindAsync(identifier) ?? throw BusinessError.Oh(AreaProvisionErrorCode.ServerNotFound);
    }

    private static GameAreaProvisionStatusResponse BuildStatus(GameAreaEntity area, GameAreaProvisionCalendarResponse? calendar = null)
    {
        return new GameAreaProvisionStatusResponse
        {
            ServerId = area.Id, Name = area.Name, RecordCreated = true,
            ContainerCreated = !string.IsNullOrWhiteSpace(area.ServerContainerId),
            Registered = calendar?.Registered ?? area.CreationTime > 0,
            OpenedAtUnixMilliseconds = calendar?.OpenedAtUnixMilliseconds ?? area.CreationTime * 1000,
            MergedIntoServerId = calendar?.MergedIntoServerId ?? 0,
            IsEnabled = area.IsEnabled == true,
        };
    }

    private static void ValidateIdentifier(BaseIdRequest input)
    {
        if (input == null || input.Id <= 0 || input.Id > 9007199254740991L)
        {
            throw BusinessError.Oh(AreaProvisionErrorCode.InvalidRequest);
        }
    }

    private static void ValidateCreate(GameAreaProvisionCreateRequest input)
    {
        if (input == null || input.TemplateId <= 0 || input.ServerId <= 0 || input.ServerId > 9007199254740991L
            || input.ServerId == input.TemplateId || string.IsNullOrWhiteSpace(input.Name) || input.Name.Trim().Length > 256
            || !IsAddress(input.HttpManageUrl, "http", "https") || !IsAddress(input.TcpSocketUrl, "tcp", "tcps")
            || !IsAddress(input.WebSocketUrl, "ws", "wss") || string.IsNullOrWhiteSpace(input.ServerParameters))
        {
            throw BusinessError.Oh(AreaProvisionErrorCode.InvalidRequest);
        }

        ParseServerParameters(input.ServerParameters);
    }

    private static bool IsAddress(string value, params string[] schemes)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var address) && schemes.Contains(address.Scheme) && !string.IsNullOrEmpty(address.Host);
    }

    private static bool MatchesRequest(GameAreaEntity area, GameAreaProvisionCreateRequest input)
    {
        return area.Name == input.Name.Trim() && area.HttpManageUrl == input.HttpManageUrl.Trim().TrimEnd('/')
            && area.TcpSocketUrl == input.TcpSocketUrl.Trim() && area.WebSocketUrl == input.WebSocketUrl.Trim()
            && MatchesServerParameters(area.ServerParams, input.ServerParameters);
    }

    // DTO 使用完整单词 ServerParameters，显式写入实体的存量 ServerParams；不依赖按名映射。
    private static string MergeServerParameters(string templateParameters, string overrides)
    {
        var parameters = ParseServerParameters(templateParameters);
        foreach (var pair in ParseServerParameters(overrides))
        {
            parameters[pair.Key] = pair.Value;
        }

        return JsonSerializer.Serialize(parameters);
    }

    private static bool MatchesServerParameters(string persistedParameters, string overrides)
    {
        var parameters = ParseServerParameters(persistedParameters);
        return ParseServerParameters(overrides).All(pair => parameters.TryGetValue(pair.Key, out var value) && value == pair.Value);
    }

    private static Dictionary<string, string> ParseServerParameters(string parameters)
    {
        try
        {
            var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(string.IsNullOrWhiteSpace(parameters) ? "{}" : parameters);
            if (parsed == null || parsed.Any(pair => string.IsNullOrWhiteSpace(pair.Key) || pair.Value == null))
            {
                throw BusinessError.Oh(AreaProvisionErrorCode.InvalidRequest);
            }

            return parsed;
        }
        catch (JsonException)
        {
            throw BusinessError.Oh(AreaProvisionErrorCode.InvalidRequest);
        }
    }

    private static readonly string[] ClonedFieldNames =
    {
        "Description", "ClassifyId", "Capacity", "ServerVersion", "ServerType", "LocalizationKey", "Language",
        "AllowedPlayerProfile", "AccessControls", "ChannelId", "PlatformId", "PackageId", "AppVersion",
        "ClientDispatchApiPrefix", "FulfillmentApiPrefix", "GmApiPrefix", "ControlApiPrefix", "OnlineAdminApiPrefix",
    };

    private static GameAreaEntity CloneConfiguration(GameAreaEntity template)
    {
        return new GameAreaEntity
        {
            Description = template.Description, ClassifyId = template.ClassifyId, Capacity = template.Capacity,
            ServerParams = template.ServerParams,
            ServerVersion = template.ServerVersion, ServerType = template.ServerType, LocalizationKey = template.LocalizationKey,
            Language = template.Language, AllowedPlayerProfile = template.AllowedPlayerProfile, AccessControls = template.AccessControls,
            ChannelId = template.ChannelId, PlatformId = template.PlatformId, PackageId = template.PackageId, AppVersion = template.AppVersion,
            ClientDispatchApiPrefix = template.ClientDispatchApiPrefix, FulfillmentApiPrefix = template.FulfillmentApiPrefix,
            GmApiPrefix = template.GmApiPrefix, ControlApiPrefix = template.ControlApiPrefix, OnlineAdminApiPrefix = template.OnlineAdminApiPrefix,
        };
    }
}
