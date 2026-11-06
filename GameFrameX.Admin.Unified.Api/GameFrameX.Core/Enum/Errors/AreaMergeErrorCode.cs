namespace GameFrameX.Core.Enum.Errors;

/// <summary>合服编排错误码，607 号段。</summary>
public enum AreaMergeErrorCode
{
    /// <summary>合服计划不存在。</summary>
    [ErrorText("合服计划不存在", "Merge plan not found")]
    PlanNotFound = 60700001,

    /// <summary>合服计划状态不允许此操作。</summary>
    [ErrorText("合服计划状态不允许此操作", "Merge plan state does not allow this operation")]
    InvalidState = 60700002,

    /// <summary>目标服和源服必须存在且互不相同。</summary>
    [ErrorText("目标服和源服必须存在且互不相同", "Target and source servers must exist and be distinct")]
    InvalidServers = 60700003,

    /// <summary>区服已参与未完成的合服计划。</summary>
    [ErrorText("区服已参与未完成的合服计划", "Server is reserved by an unfinished merge plan")]
    SourceReserved = 60700004,

    /// <summary>需要另一位操作者填写原因并复核。</summary>
    [ErrorText("需要另一位操作者填写原因并复核", "Another operator must review and provide a reason")]
    ReviewRequired = 60700005,

    /// <summary>合服报告触及全局域，禁止批准。</summary>
    [ErrorText("合服报告触及全局域，禁止批准", "Merge report touches the global domain; approval forbidden")]
    GlobalDomainTouched = 60700006,

    /// <summary>执行前必须确认备份并填写备份标识。</summary>
    [ErrorText("执行前必须确认备份并填写备份标识", "Confirm backup and provide its identifier before execution")]
    BackupRequired = 60700007,

    /// <summary>合服报告与计划不匹配。</summary>
    [ErrorText("合服报告与计划不匹配", "Merge report does not match the plan")]
    InvalidReport = 60700008,

    /// <summary>合服调用失败，请核查后重新预演。</summary>
    [ErrorText("合服调用失败，请核查后重新预演", "Merge call failed; verify and rehearse again")]
    DispatchFailed = 60700009,

    /// <summary>合服计划不允许取消。</summary>
    [ErrorText("合服计划不允许取消", "Merge plan cannot be cancelled")]
    CancellationNotAllowed = 60700010,

}
