namespace GameFrameX.Entity.Enum;

/// <summary>合服计划状态。</summary>
[Description("合服计划状态")]
public enum GameAreaMergePlanStatus
{
    [Description("草稿")] Draft = 0,
    [Description("已预演")] DryRunCompleted = 1,
    [Description("已批准")] Approved = 2,
    [Description("执行中")] Executing = 3,
    [Description("已完成")] Completed = 4,
    [Description("失败")] Failed = 5,
}
