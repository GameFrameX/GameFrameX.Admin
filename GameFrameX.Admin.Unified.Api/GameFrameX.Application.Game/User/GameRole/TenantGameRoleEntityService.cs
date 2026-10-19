using GameFrameX.Application.Game.ServerManager;
using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.User.GameRole.Dto;
using GameFrameX.Entity.Enum;
using GameFrameX.Foundation.Http.Extension;
using GameFrameX.Foundation.Http.Normalization;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.User.GameRole;

/// <summary>
/// 游戏角色服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameRoleEntityService : GenericCrudSelectService<Entity.Tenant.Game.TenantGameRoleEntity, AddTenantGameRoleEntityRequest, UpdateTenantGameRoleEntityRequest, TenantGameRoleEntityResponse, PageTenantGameRoleEntityRequest>
{
    private readonly HttpClient _httpClient;
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> _gameAppRepository;

    public TenantGameRoleEntityService(SqlSugarRepository<Entity.Tenant.Game.TenantGameRoleEntity> repository, SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> gameAppRepository, HttpClient httpClient) : base(repository)
    {
        _httpClient = httpClient;
        _gameAppRepository = gameAppRepository;
    }

    /// <summary>
    /// 构建分页查询条件
    /// </summary>
    protected override ISugarQueryable<TenantGameRoleEntityResponse> BuildPageQuery(PageTenantGameRoleEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "游戏角色");
        return Repository.AsQueryable()
                         .WhereIF(input.Id > 0, u => u.Id == input.Id)
                         .WhereIF(input.UserId > 0, u => u.UserId == input.UserId)
                         .WhereIF(input.AreaId > 0, u => u.AreaId == input.AreaId)
                         .Where(u => u.AppId == appId)
                         .WhereIF(input.IsSandbox == true, u => u.IsSandbox == true)
                         .WhereIF(input.IsSandbox == false, u => u.IsSandbox == false || u.IsSandbox == null)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .WhereIF(input.RegisterTime != null && input.RegisterTime.Count == 2 && input.RegisterTime[0] < input.RegisterTime[1],
                                  u => u.CreatedTime >= input.RegisterTime[0]
                                       && u.CreatedTime <= input.RegisterTime[1])
                         .WhereIF(input.LogoutTime != null && input.LogoutTime.Count == 2 && input.LogoutTime[0] < input.LogoutTime[1],
                                  u => u.LastLoginTime >= input.LogoutTime[0]
                                       && u.LastLoginTime <= input.LogoutTime[1])
                         .WhereIF(input.RechargeMin != null && input.RechargeMax != null && input.RechargeMin <= input.RechargeMax,
                                  u => u.AllRecharge >= input.RechargeMin
                                       && u.AllRecharge <= input.RechargeMax)
                         .Select<TenantGameRoleEntityResponse>();
    }

    /// <summary>
    /// 新增前校验：AppId 必填且必须属于当前租户（事实数据，禁止 AppId=0）。
    /// </summary>
    protected override async Task OnAddingAsync(AddTenantGameRoleEntityRequest input)
    {
        AppScopeRules.EnsureFactAppId(input.AppId, "游戏角色");
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
    }

    /// <summary>
    /// 更新前：AppId 不可变（从既有实体回填，禁止角色跨应用迁移）并校验应用归属。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameRoleEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == long.Parse(input.RoleId));
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        input.AppId = existing.AppId;
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
    }

    /// <summary>
    /// 分页查询游戏角色（含 RoleId 回填）
    /// </summary>
    [HttpPost(CrudActionRoutes.Page)]
    [ApiDescriptionSettings(Name = "Page")]
    public override async Task<SqlSugarPagedList<TenantGameRoleEntityResponse>> PageAsync(PageTenantGameRoleEntityRequest input)
    {
        var query = BuildPageQuery(input);
        query = ApplyOrdering(query, input);
        var result = await query.ToPagedListAsync(input.Page, input.PageSize);
        foreach (var item in result.Items)
        {
            item.RoleId = item.Id.ToString();
        }

        return result;
    }

    /// <summary>
    /// 更新游戏角色（AppId 不可变，从既有实体回填，禁止跨应用迁移）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost(CrudActionRoutes.Update)]
    [ApiDescriptionSettings(Name = "Update")]
    public override async Task UpdateAsync(UpdateTenantGameRoleEntityRequest input)
    {
        if (string.IsNullOrEmpty(input.RoleId))
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        // 应用归属不可变：从既有实体回填 AppId，禁止角色跨应用迁移
        var existing = await Repository.GetFirstAsync(u => u.Id == long.Parse(input.RoleId));
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        input.AppId = existing.AppId;
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);

        var entity = input.Adapt<Entity.Tenant.Game.TenantGameRoleEntity>();
        entity.Id = long.Parse(input.RoleId);
        await Repository.AsUpdateable(entity).IgnoreColumns(true).ExecuteCommandWithOptLockAsync(true);
    }

    /// <summary>
    /// 更新游戏角色为沙盒用户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "UpdateToSandBox")]
    public async Task UpdateToSandBoxAsync(UpdateTenantGameRoleEntityRequest input)
    {
        var gameRoleEntity = await Repository.GetFirstAsync(u => u.Id == long.Parse(input.RoleId));
        var gameAreaEntity = await Repository.AsSugarClient().Queryable<Entity.Tenant.Game.TenantGameAreaEntity>().Filter(null, true).Where(u => u.IsDeleted == false).SingleAsync(m => m.Id == gameRoleEntity.AreaId);
        if (gameAreaEntity == null)
        {
            throw new Exception("区服信息不存在");
        }

        var request = new PlayerSetSandBoxRequest
        {
            IsSandbox = true,
            PlayerId = long.Parse(input.RoleId),
        };
        var areaAddress = GameServerEndpointUrlBuilder.Build(gameAreaEntity, GameServerEndpointKindEnum.Fulfillment, "req_player_set_sand_box");
        var content = await _httpClient.PostJsonToStringAsync(areaAddress, request);
        var respPlayerSetSandBoxResponse = content.ToHttpJsonResult<PlayerSetSandBoxResponse>();
        // 设置成功
        if (respPlayerSetSandBoxResponse.IsSuccess)
        {
            gameRoleEntity.IsSandbox = true;
            await Repository.AsUpdateable(gameRoleEntity).IgnoreColumns(true).ExecuteCommandWithOptLockAsync(true);
            return;
        }

        throw new Exception("游戏角色更新为沙盒用户失败:" + respPlayerSetSandBoxResponse.Code);
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class PlayerSetSandBoxRequest
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public long PlayerId { get; set; }

        /// <summary>
        /// 是否沙盒
        /// </summary>
        public bool IsSandbox { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class PlayerSetSandBoxResponse
    {
    }

    /// <summary>
    /// 更新游戏角色为非沙盒用户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "UpdateToUnSandBox")]
    public async Task UpdateToUnSandBoxAsync(UpdateTenantGameRoleEntityRequest input)
    {
        var gameRoleEntity = await Repository.GetFirstAsync(u => u.Id == long.Parse(input.RoleId));
        var gameAreaEntity = await Repository.AsSugarClient().Queryable<Entity.Tenant.Game.TenantGameAreaEntity>().Filter(null, true).Where(u => u.IsDeleted == false).SingleAsync(m => m.Id == gameRoleEntity.AreaId);
        if (gameAreaEntity == null)
        {
            throw new Exception("区服信息不存在");
        }

        var request = new PlayerSetSandBoxRequest
        {
            IsSandbox = false,
            PlayerId = long.Parse(input.RoleId),
        };
        var areaAddress = GameServerEndpointUrlBuilder.Build(gameAreaEntity, GameServerEndpointKindEnum.Fulfillment, "req_player_set_sand_box");
        var content = await _httpClient.PostJsonToStringAsync(areaAddress, request);
        var respPlayerSetSandBoxResponse = content.ToHttpJsonResult<PlayerSetSandBoxResponse>();
        // 设置成功
        if (respPlayerSetSandBoxResponse.IsSuccess)
        {
            gameRoleEntity.IsSandbox = false;
            await Repository.AsUpdateable(gameRoleEntity).IgnoreColumns(true).ExecuteCommandWithOptLockAsync(true);
            return;
        }

        throw new Exception("游戏角色更新为非沙盒用户失败:" + respPlayerSetSandBoxResponse.Code);
    }

    /// <summary>
    /// 同步游戏角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Sync")]
    public async Task SyncAsync(UpdateTenantGameRoleEntityRequest input)
    {
        var gameRoleEntity = await Repository.GetFirstAsync(u => u.Id == long.Parse(input.RoleId));
        var gameAreaEntity = await Repository.AsSugarClient().Queryable<Entity.Tenant.Game.TenantGameAreaEntity>().Filter(null, true).Where(u => u.IsDeleted == false).SingleAsync(m => m.Id == gameRoleEntity.AreaId);
        if (gameAreaEntity == null)
        {
            throw new Exception("区服信息不存在");
        }


        var request = new PlayerGetInfoRequest
        {
            PlayerId = long.Parse(input.RoleId),
        };
        var areaAddress = GameServerEndpointUrlBuilder.Build(gameAreaEntity, GameServerEndpointKindEnum.Fulfillment, "req_player_get_info");
        var content = await _httpClient.PostJsonToStringAsync(areaAddress, request);
        var respPlayerSetSandBoxResponse = content.ToHttpJsonResult<PlayerGetInfoResponse>();
        // 设置成功
        if (respPlayerSetSandBoxResponse.IsSuccess)
        {
            gameRoleEntity.Level = (int)respPlayerSetSandBoxResponse.Data.PlayerInfo.Level;
            gameRoleEntity.Avatar = respPlayerSetSandBoxResponse.Data.PlayerInfo.Avatar;
            gameRoleEntity.State = respPlayerSetSandBoxResponse.Data.PlayerInfo.State;
            gameRoleEntity.Gender = respPlayerSetSandBoxResponse.Data.PlayerInfo.Gender;
            gameRoleEntity.AvatarFrame = respPlayerSetSandBoxResponse.Data.PlayerInfo.AvatarFrame;
            gameRoleEntity.CurrentLevelConfigId = respPlayerSetSandBoxResponse.Data.PlayerInfo.CurrentLevelConfigId;
            gameRoleEntity.CurrentExp = respPlayerSetSandBoxResponse.Data.PlayerInfo.CurrentExp;
            //在线时长,转化为分钟
            // gameRoleEntity.OnlineTime = respPlayerSetSandBoxResponse.Data.OnlineTime / 60;
            await Repository.AsUpdateable(gameRoleEntity).IgnoreColumns(true).ExecuteCommandWithOptLockAsync(true);
            return;
        }

        throw new Exception("游戏角色同步失败:" + respPlayerSetSandBoxResponse.Code);
    }

    /// <summary>
    /// 获取游戏角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet(CrudActionRoutes.Detail)]
    [ApiDescriptionSettings(Name = "Detail")]
    public override async Task<TenantGameRoleEntityResponse> GetAsync([FromQuery] BaseIdRequest input)
    {
        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id);
        return entity == null ? default : entity.Adapt<TenantGameRoleEntityResponse>();
    }
}