using GameFrameX.Application.CommerceCenter.GameRedeemCodeChannel.Dto;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.CommerceCenter.GameRedeemCodeChannel;

/// <summary>
/// 兑换码渠道服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class GameRedeemCodeChannelService
    : GenericCrudSelectService<Entity.Business.Game.GameRedeemCodeChannelEntity,
        AddGameRedeemCodeChannelEntityRequest, UpdateGameRedeemCodeChannelEntityRequest,
        GameRedeemCodeChannelResponse, PageGameRedeemCodeChannelEntityRequest>
{
    public GameRedeemCodeChannelService(SqlSugarRepository<Entity.Business.Game.GameRedeemCodeChannelEntity> repository)
        : base(repository)
    {
    }

    /// <summary>
    /// 新增前校验每角色可用次数。
    /// </summary>
    protected override Task OnAddingAsync(AddGameRedeemCodeChannelEntityRequest input)
    {
        EnsureValidRoleUsageLimit(input.RoleUsageLimit);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 更新前验证记录存在。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateGameRedeemCodeChannelEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        EnsureValidRoleUsageLimit(input.RoleUsageLimit);
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

    protected override ISugarQueryable<GameRedeemCodeChannelResponse> BuildPageQuery(PageGameRedeemCodeChannelEntityRequest input)
    {
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .Select<GameRedeemCodeChannelResponse>();
    }
}
#pragma warning restore CS8602, CS8604, CS8629