namespace GameFrameX.Application.Game.RedeemCode.GameRedeemCode.Dto;

/// <summary>
/// 兑换码更新输入参数
/// </summary>
public class UpdateTenantGameRedeemCodeEntityRequest : BaseUpdateRequest
{
    /// <summary>
    /// 应用Id。不可变（服务端从既有实体回填，禁止跨应用迁移），仅在 Output 投影展示。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [Required(ErrorMessage = "描述不能为空")]
    public string Description { get; set; } = null!;
}