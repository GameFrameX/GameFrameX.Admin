namespace GameFrameX.Application.Game.Order.GameOrderFulfillment.Dto;

/// <summary>
/// 权益发货账本高风险动作输入参数（C120）：RefundAsync / CompensateAsync / ResendAsync 共用。
/// 兼作 GenericCrudService 的 TAddRequest（AddAsync 端点已由 OnAddingAsync 抛错关闭，与 TenantGameOrderEntityService 复用 AddInput 于 SendAsync 同款）。
/// </summary>
public class FulfillmentActionRequest : BaseAddRequest
{
    /// <summary>
    /// 账本主键 Id（动作作用于账本行对应的订单）。
    /// </summary>
    [Required(ErrorMessage = "账本Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "账本Id必须大于0")]
    public long Id { get; set; }

    /// <summary>
    /// 操作原因。RefundAsync 并入账本 FailureReason；CompensateAsync/ResendAsync 失败时落账，成功不落。
    /// </summary>
    public string Reason { get; set; } = null!;
}