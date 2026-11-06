namespace GameFrameX.Core.Enum.Errors;

/// <summary>开服向导错误码，606 号段。</summary>
public enum AreaProvisionErrorCode
{
    /// <summary>开服参数无效。</summary>
    [ErrorText("开服参数无效", "Invalid provisioning parameters")]
    InvalidRequest = 60600001,

    /// <summary>模板区服不存在。</summary>
    [ErrorText("模板区服不存在", "Template server not found")]
    TemplateNotFound = 60600002,

    /// <summary>区服编号已占用或重试参数不一致。</summary>
    [ErrorText("区服编号已占用或重试参数不一致", "Server identifier is occupied or retry parameters differ")]
    ServerConflict = 60600003,

    /// <summary>容器创建失败，请检查容器管理器后重试。</summary>
    [ErrorText("容器创建失败，请检查容器管理器后重试", "Container creation failed; check the container manager and retry")]
    ContainerFailed = 60600004,

    /// <summary>开服登记失败，请等待游戏服就绪后重试。</summary>
    [ErrorText("开服登记失败，请等待游戏服就绪后重试", "Opening registration failed; wait for the game server and retry")]
    RegistrationFailed = 60600005,

    /// <summary>开服状态查询失败，请稍后重试。</summary>
    [ErrorText("开服状态查询失败，请稍后重试", "Opening status query failed; retry later")]
    CalendarFailed = 60600006,

    /// <summary>开服数据保存失败，请查询状态后重试。</summary>
    [ErrorText("开服数据保存失败，请查询状态后重试", "Provisioning data could not be saved; query status and retry")]
    PersistenceFailed = 60600007,

    /// <summary>开服区服不存在。</summary>
    [ErrorText("开服区服不存在", "Provisioning server not found")]
    ServerNotFound = 60600008,

    /// <summary>区服已被停用或合服，不能通过开服向导重新启用。</summary>
    [ErrorText("区服已被停用或合服，不能通过开服向导重新启用", "Server has been disabled or merged; provisioning cannot enable it again")]
    ServerAlreadyClosed = 60600009,

}
