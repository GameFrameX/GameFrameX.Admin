using GameFrameX.Application.Game.App.GameAppRemoteConfigMode.Dto;

namespace GameFrameX.Application.Game.App.GameAppRemoteConfigMode;

/// <summary>
/// 远程配置模式定义管理服务
/// </summary>
[ApiDescriptionSettings(ClientConst.GroupName, Order = 100)]
public class TenantGameAppRemoteConfigModeService : GenericCrudService<Entity.Tenant.Game.RemoteConfig.TenantGameAppRemoteConfigModeEntity, AddGameAppRemoteConfigModeRequest, UpdateGameAppRemoteConfigModeRequest, GameAppRemoteConfigModeResponse, PageGameAppRemoteConfigModeRequest>
{
    public TenantGameAppRemoteConfigModeService(SqlSugarRepository<Entity.Tenant.Game.RemoteConfig.TenantGameAppRemoteConfigModeEntity> repository) : base(repository)
    {
    }

    /// <summary>
    /// 构建分页查询条件
    /// </summary>
    protected override ISugarQueryable<GameAppRemoteConfigModeResponse> BuildPageQuery(PageGameAppRemoteConfigModeRequest input)
    {
        var modeCode = input.ModeCode?.Trim() ?? string.Empty;
        var displayName = input.DisplayName?.Trim() ?? string.Empty;
        var isEnabled = input.IsEnabled.GetValueOrDefault();

        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrEmpty(modeCode), x => x.ModeCode.Contains(modeCode))
                         .WhereIF(!string.IsNullOrEmpty(displayName), x => x.DisplayName.Contains(displayName))
                         .WhereIF(input.IsEnabled.HasValue, x => x.IsEnabled == isEnabled)
                         .Select<GameAppRemoteConfigModeResponse>();
    }

    /// <summary>
    /// 新增模式定义。
    /// 显式声明 AddAsync 方法，确保 Admin API 约定以强类型 <see cref="AddGameAppRemoteConfigModeRequest"/> 绑定参数，
    /// 避免继承的泛型 AddAsync 方法被解析为 dynamic/ExpandoObject 后在基类 (BaseAddRequest) 强转时抛 InvalidCastException。
    /// </summary>
    [HttpPost(CrudActionRoutes.Add)]
    [ApiDescriptionSettings(Name = "Add")]
    public override async Task<GameAppRemoteConfigModeResponse> AddAsync(AddGameAppRemoteConfigModeRequest input)
    {
        return await base.AddAsync(input);
    }

    /// <summary>
    /// 新增前的编码唯一性校验
    /// </summary>
    protected override async Task OnAddingAsync(AddGameAppRemoteConfigModeRequest input)
    {
        var modeCode = input.ModeCode?.Trim() ?? string.Empty;
        var exists = await Repository.AsQueryable()
                                     .Where(x => x.ModeCode == modeCode)
                                     .AnyAsync();
        if (exists)
        {
            throw BusinessError.Oh($"模式编码 '{modeCode}' 已存在");
        }
    }

    /// <summary>
    /// 更新前的编码唯一性校验
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateGameAppRemoteConfigModeRequest input)
    {
        var modeCode = input.ModeCode?.Trim() ?? string.Empty;
        var exists = await Repository.AsQueryable()
                                     .Where(x => x.ModeCode == modeCode && x.Id != input.Id)
                                     .AnyAsync();
        if (exists)
        {
            throw BusinessError.Oh($"模式编码 '{modeCode}' 已存在");
        }
    }
}