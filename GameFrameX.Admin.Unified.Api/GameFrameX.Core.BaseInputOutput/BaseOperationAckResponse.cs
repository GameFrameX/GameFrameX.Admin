namespace GameFrameX.Core.BaseInputOutput;

/// <summary>
/// 操作确认输出参数。用于成功无业务数据的端点（回调 ack、写操作确认等）：
/// 成功确认统一由信封 code==0 承载，data 恒为 null，本类型仅作泛型静态 schema 声明。
/// </summary>
public class BaseOperationAckResponse
{
}