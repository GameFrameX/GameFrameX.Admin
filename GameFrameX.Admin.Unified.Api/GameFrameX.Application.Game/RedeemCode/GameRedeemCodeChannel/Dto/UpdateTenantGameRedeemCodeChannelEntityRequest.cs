namespace GameFrameX.Application.Game.RedeemCode.GameRedeemCodeChannel.Dto;

/// <summary>
/// 兑换码渠道更新输入参数
/// </summary>
public class UpdateTenantGameRedeemCodeChannelEntityRequest : BaseUpdateRequest
{
    /// <summary>
    /// 应用Id。事实数据应用作用域，必须 &gt; 0。
    /// <para>过渡期：后台运营当前应用上下文尚未建立，由前端手填。</para>
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    public long AppId { get; set; }

    /// <summary>
    /// 名称。此前 UpdateRequest 缺此声明：编辑提交的 name 被绑定丢弃，Adapt 后实体 Name=null，
    /// 又被 IgnoreColumns(true)（忽略 null 列）跳过 —— update 200 但名称静默不更新。
    /// </summary>
    [Required(ErrorMessage = "名称不能为空")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    [Required(ErrorMessage = "详细描述不能为空")]
    public string Description { get; set; } = null!;

    /// <summary>
    /// 每角色可用次数（兑换码分组角色限制）。0 = 不限制，1 = 每角色一次，N = 每角色最多 N 次。
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "每角色可用次数必须大于等于0")]
    public int RoleUsageLimit { get; set; }
}