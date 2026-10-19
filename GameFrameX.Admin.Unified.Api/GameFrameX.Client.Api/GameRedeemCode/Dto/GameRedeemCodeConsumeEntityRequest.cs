namespace GameFrameX.Client.Api.GameRedeemCode.Dto;

/// <summary>
///     兑换码消费输入参数
/// </summary>
public class GameRedeemCodeConsumeEntityRequest
{
    /// <summary>
    ///     区服ID
    /// </summary>
    public long AreaId { get; set; }

    /// <summary>
    ///     兑换码
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    ///     角色ID
    /// </summary>
    public long RoleId { get; set; }
}