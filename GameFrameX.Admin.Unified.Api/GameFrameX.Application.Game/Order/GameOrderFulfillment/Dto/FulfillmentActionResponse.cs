using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Order.GameOrderFulfillment.Dto;

/// <summary>
/// 权益发货账本高风险动作输出（C223 / GFX-644，U9 信封统一后为三 host 通用）：RefundAsync / CompensateAsync / ResendAsync 成功响应的 data 对象契约。
/// Admin 端点返回本对象交 AdminResultFilter 自动包装 HttpJsonResultData，失败走 BusinessException 统一错误路径。
/// </summary>
public class FulfillmentActionResponse
{
    /// <summary>
    /// 动作作用的订单 Id（取自账本行对应订单）。
    /// </summary>
    public long OrderId { get; set; }

    /// <summary>
    /// 动作落账的发放类型：RefundAsync→RefundAsync，CompensateAsync/ResendAsync→CompensateAsync（C113 三类：Purchase/RefundAsync/CompensateAsync）。
    /// </summary>
    public FulfillmentTypeEnum Type { get; set; }
}