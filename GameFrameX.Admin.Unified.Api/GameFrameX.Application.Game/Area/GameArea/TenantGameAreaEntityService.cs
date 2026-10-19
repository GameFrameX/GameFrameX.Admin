using System.ComponentModel;
using GameFrameX.Application.Game.ServerManager;
using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Area.GameArea.Dto;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Foundation.Http.Extension;
using GameFrameX.Foundation.Json;

#pragma warning disable CS8602, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Area.GameArea;

/// <summary>
/// 游戏区服服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameAreaEntityService : GenericCrudSelectService<Entity.Tenant.Game.TenantGameAreaEntity, AddTenantGameAreaEntityRequest, UpdateTenantGameAreaEntityRequest, TenantGameAreaEntityResponse, PageTenantGameAreaEntityRequest>
{
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameAreaClassifyEntity> _classifyRepository;
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> _gameAppRepository;
    private readonly HttpClient _httpClient;

    /// <summary>
    ///
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="classifyRepository"></param>
    /// <param name="gameAppRepository"></param>
    /// <param name="httpClient"></param>
    public TenantGameAreaEntityService(SqlSugarRepository<Entity.Tenant.Game.TenantGameAreaEntity> repository, SqlSugarRepository<Entity.Tenant.Game.TenantGameAreaClassifyEntity> classifyRepository, SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> gameAppRepository, HttpClient httpClient) : base(repository)
    {
        _classifyRepository = classifyRepository;
        _gameAppRepository = gameAppRepository;
        _httpClient = httpClient;
    }

    /// <summary>
    /// 构建分页查询条件（含外键关联）
    /// </summary>
    protected override ISugarQueryable<TenantGameAreaEntityResponse> BuildPageQuery(PageTenantGameAreaEntityRequest input)
    {
        var gameAppId = AppScopeRules.EnsureFactAppId(input.AppId, "游戏区服");
        var tenantId = TenantOperationContext.GetEffectiveTenantId();
        // 区服启用标记 C301 起归一基类 IsEnabled，会被全局 ISafeEnabledFilter 隐藏禁用行；
        // 管理列表必须能看到禁用区服（否则无法重新启用，参照 TenantGameAppPackageService 的 C67 FixPattern），
        // 故绕过全部过滤器并手动补软删与租户条件。
        // 注意：Filter(null, true) 必须放在 LeftJoin 之后——SqlSugar 的 Filter 状态不随 join 链传递，
        // 放 join 前会被 join 重置（实测禁用行被 ISafeEnabledFilter 过滤），放 join 后才对整个查询生效。
        return Repository.AsQueryable()
                         .Filter(null, true)
                         .Where(u => u.IsDeleted == false)
                         .WhereIF(tenantId.HasValue, u => u.TenantId == null || u.TenantId == tenantId.Value)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .Where(u => u.GameAppId == gameAppId)
                         .WhereIF(input.IsDefault.HasValue, u => u.IsDefault == input.IsDefault.Value)
                         .WhereIF(input.IsRecommend.HasValue, u => u.IsRecommend == input.IsRecommend.Value)
                         .WhereIF(input.IsEnabled.HasValue, u => u.IsEnabled == input.IsEnabled.Value)
                         .WhereIF(input.Language.HasValue, u => u.Language == input.Language.Value)
                         // C301：分類名改用子查询取值。原 LeftJoin 形态下 SqlSugar 的 Filter/ClearFilter 均不生效
                         //（join 查询不吃 queryable 级过滤器开关，实测禁用行被 ISafeEnabledFilter 隐藏），
                         // 改单表 + SqlFunc.Subqueryable 后 Filter(null, true) 才能对主查询生效。
                         .Select(u => new TenantGameAreaEntityResponse
                         {
                             Id = u.Id,
                             // 乐观锁版本必须下发：编辑回传缺 RowVersion 会让乐观锁以 null 参数比较（Postgres bigint = text）500
                             RowVersion = u.RowVersion,
                             Name = u.Name,
                             Description = u.Description,
                             IsDefault = u.IsDefault,
                             IsRecommend = u.IsRecommend,
                             ClassifyId = u.ClassifyId,
                             ClassifyIdName = SqlFunc.Subqueryable<Entity.Tenant.Game.TenantGameAreaClassifyEntity>().Where(classifyid => classifyid.Id == u.ClassifyId).Select(classifyid => classifyid.Name),
                             GameAppId = u.GameAppId,
                             IsEnabled = u.IsEnabled,
                             ServerStatus = u.ServerStatus,
                             HttpManageUrl = u.HttpManageUrl,
                             TcpSocketUrl = u.TcpSocketUrl,
                             WebSocketUrl = u.WebSocketUrl,
                             Capacity = u.Capacity,
                             CurrentLoad = u.CurrentLoad,
                             CreationTime = u.CreationTime,
                             LastMaintenanceTime = u.LastMaintenanceTime,
                             ServerVersion = u.ServerVersion,
                             ServerContainerId = u.ServerContainerId,
                             ServerType = u.ServerType,
                             Language = u.Language,
                             AllowedPlayerProfile = u.AllowedPlayerProfile,
                             ServerEvents = u.ServerEvents,
                             AccessControls = u.AccessControls,
                             CreatedTime = u.CreatedTime,
                             UpdateTime = u.UpdateTime,
                             CreatedName = u.CreatedName,
                             UpdatedName = u.UpdatedName,
                             LocalizationKey = u.LocalizationKey,
                             AppVersion = u.AppVersion,
                             ChannelId = u.ChannelId,
                             PackageId = u.PackageId,
                             PlatformId = u.PlatformId,
                             ServerParams = u.ServerParams,
                             ClientDispatchApiPrefix = u.ClientDispatchApiPrefix,
                             FulfillmentApiPrefix = u.FulfillmentApiPrefix,
                             GmApiPrefix = u.GmApiPrefix,
                             ControlApiPrefix = u.ControlApiPrefix,
                             EnableClientGmApi = u.EnableClientGmApi,
                         });
    }


    /// <summary>
    /// 按 Id 取区服（含禁用行）。启用标记在基类 IsEnabled（C301 归一）后受全局 ISafeEnabledFilter 过滤，
    /// 管理/转发操作必须触达禁用区服，故按 FixPattern 绕过全部过滤器并手动补软删与租户条件。
    /// </summary>
    private async Task<Entity.Tenant.Game.TenantGameAreaEntity> GetAreaForManagementAsync(long id)
    {
        var tenantId = TenantOperationContext.GetEffectiveTenantId();
        return await Repository.AsQueryable()
                               .Filter(null, true)
                               .Where(u => u.IsDeleted == false)
                               .WhereIF(tenantId.HasValue, u => u.TenantId == null || u.TenantId == tenantId.Value)
                               .FirstAsync(u => u.Id == id);
    }

    /// <summary>
    /// 详情：绕过全局过滤器读取（禁用区服 IsEnabled=false 会被 ISafeEnabledFilter 隐藏，详情页必须可见，C301）。
    /// </summary>
    [HttpGet(CrudActionRoutes.Detail)]
    [DisplayName("区服详情")]
    public override async Task<TenantGameAreaEntityResponse> GetAsync([FromQuery] BaseIdRequest input)
    {
        var entity = await GetAreaForManagementAsync(input.Id);
        return entity == null ? default : entity.Adapt<TenantGameAreaEntityResponse>();
    }

    /// <summary>
    /// 新增前校验：GameAppId 必填且必须属于当前租户（多租户过滤仓储，null 即抛错）。
    /// </summary>
    protected override async Task OnAddingAsync(AddTenantGameAreaEntityRequest input)
    {
        NormalizeEndpointPrefixes(input);
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
    }

    /// <summary>
    /// 更新前：GameAppId 不可变（从既有实体回填，禁止区服跨应用迁移）并校验应用归属。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameAreaEntityRequest input)
    {
        NormalizeEndpointPrefixes(input);
        var existing = await GetAreaForManagementAsync(input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        input.AppId = existing.GameAppId;
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
    }

    /// <summary>
    /// 归一化区服 Server HTTP 接口前缀（来源 change C175）：Trim 空白与首尾斜杠；
    /// <para>ClientDispatch/Fulfillment 空值回填默认 <c>game/api</c>（保证历史路径不回归）；Gm/Control 允许空（空表示不构造对应能力 URL）。</para>
    /// </summary>
    private static void NormalizeEndpointPrefixes(TenantGameAreaEntityBaseRequest input)
    {
        input.ClientDispatchApiPrefix = NormalizeApiPrefix(input.ClientDispatchApiPrefix, GameServerEndpointUrlBuilder.DefaultApiPrefix);
        input.FulfillmentApiPrefix = NormalizeApiPrefix(input.FulfillmentApiPrefix, GameServerEndpointUrlBuilder.DefaultApiPrefix);
#pragma warning disable CS8625 // null 作为 fallback 表示允许保留空（Gm/Control 前缀允许空，与 ClientDispatch/Fulfillment 必填不同）
        input.GmApiPrefix = NormalizeApiPrefix(input.GmApiPrefix, null);
        input.ControlApiPrefix = NormalizeApiPrefix(input.ControlApiPrefix, null);
#pragma warning restore CS8625
    }

    private static string NormalizeApiPrefix(string value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var trimmed = value.Trim().Trim('/');
        return trimmed.Length == 0 ? fallback : trimmed;
    }

    /// <summary>
    /// 删除游戏区服
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "DisableDelete")]
    public async Task DisableDeleteAsync(BaseIdRequest input)
    {
        var entity = await GetAreaForManagementAsync(input.Id);
        if (entity == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }


        var gameAreaClassify = await _classifyRepository.GetByIdAsync(entity.ClassifyId);
        var url = gameAreaClassify.HttpManagerUrl;

        if (!string.IsNullOrEmpty(url))
        {
            // 执行停止
            await _httpClient.PostJsonToStringAsync(url + "api/Containers/Stop", new BaseRequest { Id = entity.ServerContainerId, });
        }

        await Repository.FakeDeleteAsync(entity); //假删除
        //await Repository.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 游戏区服开服
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Open")]
    public async Task OpenAsync(BaseIdRequest input)
    {
        //获取区服信息
        var entity = await GetAreaForManagementAsync(input.Id);
        if (entity == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        var gameAreaClassify = await _classifyRepository.GetByIdAsync(entity.ClassifyId);
        var url = gameAreaClassify.HttpManagerUrl;

        var createDockerContainer = new CreateDockerContainer
        {
            ImageName = gameAreaClassify.ImageName,
            Tag = entity.ServerVersion,
            ContainerName = entity.Id.ToString(),
            IsRecreate = false,
            Auth = new CreateDockerContainerAuth
            {
                Registry = gameAreaClassify.DockerRegister,
                Username = gameAreaClassify.DockerUserName,
                Password = gameAreaClassify.DockerPassword,
            },
            NetworkConfig = new CreateDockerContainerNet
            {
                Name = gameAreaClassify.ContainerNetName,
                Gateway = gameAreaClassify.ContainerGateway,
                Subnet = gameAreaClassify.ContainerSubnet,
            },
            Environments = JsonHelper.Deserialize<Dictionary<string, string>>(entity.ServerParams),
        };


        if (!string.IsNullOrEmpty(url))
        {
            // 执行创建服务
            var content = await _httpClient.PostJsonToStringAsync(url + "api/Containers/CreateAndStart", createDockerContainer);
            try
            {
                var resultData = JsonHelper.Deserialize<RespCreateDockerContainer>(content);
                if (resultData.Code != 0)
                {
                    Console.WriteLine(content);
                    return;
                }

                entity.ServerContainerId = resultData.Data;
                await Repository.Context.UpdateWithOptimisticLockAsync(entity);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        //
        // await Repository.FakeDeleteAsync(entity); //假删除
        // //await Repository.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 停止游戏区服
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Stop")]
    public async Task StopAsync(BaseIdRequest input)
    {
        var entity = await GetAreaForManagementAsync(input.Id);
        if (entity == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }


        var gameAreaClassify = await _classifyRepository.GetByIdAsync(entity.ClassifyId);
        var url = gameAreaClassify.HttpManagerUrl;
        if (string.IsNullOrEmpty(url))
        {
            throw new Exception("游戏区服的管理地址未配置");
        }

        // 执行停止
        await _httpClient.PostJsonToStringAsync(url + "api/Containers/Stop", new BaseRequest { Id = entity.ServerContainerId, });
    }

    /// <summary>
    /// 重启游戏区服
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Restart")]
    public async Task RestartAsync(BaseIdRequest input)
    {
        var gameAreaEntity = await GetAreaForManagementAsync(input.Id);
        if (gameAreaEntity == null)
        {
            throw new Exception("游戏区服不存在");
        }

        if (string.IsNullOrEmpty(gameAreaEntity.ServerContainerId))
        {
            throw new Exception("游戏区服的容器ID不存在");
        }

        var gameAreaClassify = await _classifyRepository.GetByIdAsync(gameAreaEntity.ClassifyId);
        var url = gameAreaClassify.HttpManagerUrl;

        if (string.IsNullOrEmpty(url))
        {
            throw new Exception("游戏区服的管理地址未配置");
        }

        // 执行重启
        await _httpClient.PostJsonToStringAsync(url + "api/Containers/Restart", new BaseRequest { Id = gameAreaEntity.ServerContainerId, });
    }

    /// <summary>
    /// 镜像仓库登录信息
    /// </summary>
    private sealed class CreateDockerContainerAuth
    {
        /// <summary>
        /// 镜像仓库地址
        /// </summary>
        public string Registry { get; set; } = null!;

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; } = null!;

        /// <summary>
        /// 可选的密码
        /// </summary>
        public string Password { get; set; } = null!;
    }

    /// <summary>
    /// 镜像仓库登录信息
    /// </summary>
    private sealed class CreateDockerContainerNet
    {
        /// <summary>
        /// 网络的名称
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// 网络的子网地址
        /// </summary>
        public string Subnet { get; set; } = null!;

        /// <summary>
        /// 网关地址
        /// </summary>
        public string Gateway { get; set; } = null!;
    }

    private sealed class CreateDockerContainer
    {
        /// <summary>
        /// docker镜像名称
        /// </summary>
        public string ImageName { get; set; } = null!;

        /// <summary>
        /// 镜像标签
        /// </summary>
        public string Tag { get; set; } = null!;

        /// <summary>
        /// 容器名称
        /// </summary>
        public string ContainerName { get; set; } = null!;

        /// <summary>
        /// 是否强制重新创建false
        /// </summary>
        public bool IsRecreate { get; set; }

        /// <summary>
        /// docker镜像名称
        /// </summary>
        public CreateDockerContainerAuth Auth { get; set; } = null!;

        /// <summary>
        /// docker镜像名称
        /// </summary>
        public CreateDockerContainerNet NetworkConfig { get; set; } = null!;

        /// <summary>
        /// 端口,可填可随机
        /// </summary>
        public string Ports { get; set; } = null!;

        /// <summary>
        /// 命令
        /// </summary>
        public string Command { get; set; } = null!;

        public Dictionary<string, string> Environments { get; set; } = null!;
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class RespCreateDockerContainer
    {
        /// <summary>
        ///
        /// </summary>
        [Description("返回的错误码")]
        public int Code { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Description("返回的错误信息")]
        public string Message { get; set; } = null!;

        /// <summary>
        ///
        /// </summary>
        [Description("返回的错误码")]
        public string Data { get; set; } = null!;
    }
}
#pragma warning restore CS8602, CS8629