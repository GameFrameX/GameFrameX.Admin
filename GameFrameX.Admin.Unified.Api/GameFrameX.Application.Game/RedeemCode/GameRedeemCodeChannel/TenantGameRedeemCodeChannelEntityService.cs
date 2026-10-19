using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.RedeemCode.GameRedeemCodeChannel.Dto;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.RedeemCode.GameRedeemCodeChannel;

/// <summary>
/// 兑换码渠道服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameRedeemCodeChannelEntityService
    : GenericCrudSelectService<Entity.Tenant.Game.TenantGameRedeemCodeChannelEntity,
        AddTenantGameRedeemCodeChannelEntityRequest, UpdateTenantGameRedeemCodeChannelEntityRequest,
        TenantGameRedeemCodeChannelEntityResponse, PageTenantGameRedeemCodeChannelEntityRequest>
{
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> _gameAppRepository;

    public TenantGameRedeemCodeChannelEntityService(SqlSugarRepository<Entity.Tenant.Game.TenantGameRedeemCodeChannelEntity> repository, SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> gameAppRepository)
        : base(repository)
    {
        _gameAppRepository = gameAppRepository;
    }

    /// <summary>
    /// 新增前校验：AppId 必填且必须属于当前租户（运营内容，禁止 AppId=0）；RoleUsageLimit 必须 &gt;= 0。
    /// </summary>
    protected override async Task OnAddingAsync(AddTenantGameRedeemCodeChannelEntityRequest input)
    {
        AppScopeRules.EnsureFactAppId(input.AppId, "兑换码渠道");
        EnsureValidRoleUsageLimit(input.RoleUsageLimit);
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
    }

    /// <summary>
    /// 更新前：AppId 不可变（从既有实体回填）并校验应用归属；RoleUsageLimit 必须 &gt;= 0。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameRedeemCodeChannelEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        input.AppId = existing.AppId;
        AppScopeRules.EnsureFactAppId(input.AppId, "兑换码渠道");
        EnsureValidRoleUsageLimit(input.RoleUsageLimit);
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
    }

    /// <summary>
    /// 校验每角色可用次数合法：必须 &gt;= 0（0=不限制，1=每角色一次，N=每角色最多 N 次；负数非法）。
    /// </summary>
    private static void EnsureValidRoleUsageLimit(int roleUsageLimit)
    {
        if (roleUsageLimit < 0)
        {
            throw BusinessError.Oh("每角色可用次数必须大于等于0");
        }
    }

    protected override ISugarQueryable<TenantGameRedeemCodeChannelEntityResponse> BuildPageQuery(PageTenantGameRedeemCodeChannelEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "兑换码渠道");
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .Where(u => u.AppId == appId)
                         .Select<TenantGameRedeemCodeChannelEntityResponse>();
    }
}
#pragma warning restore CS8602, CS8604, CS8629