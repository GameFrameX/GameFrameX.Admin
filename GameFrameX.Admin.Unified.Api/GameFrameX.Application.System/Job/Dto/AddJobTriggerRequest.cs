namespace GameFrameX.Application.System.Job.Dto;

/// <summary>
/// 新增/编辑作业触发器输入参数。
/// 显式声明前端表单可填字段，不再继承 <see cref="SystemJobTriggerEntity"/>，避免运行时状态（Status、NumberOfRuns、NumberOfErrors、LastRunTime、NextRunTime、UpdatedTime、AssemblyName）被前端批量赋值。
/// </summary>
public class AddJobTriggerRequest
{
    /// <summary>
    /// 主键Id（新增为 0，编辑由前端回填）
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 乐观锁版本号
    /// </summary>
    public long? RowVersion { get; set; }

    /// <summary>
    /// 触发器Id
    /// </summary>
    [Required(ErrorMessage = "触发器Id不能为空")]
    [MinLength(2, ErrorMessage = "触发器Id不能少于2个字符")]
    public string TriggerId { get; set; } = string.Empty;

    /// <summary>
    /// 作业Id
    /// </summary>
    [Required(ErrorMessage = "作业Id不能为空")]
    [MinLength(2, ErrorMessage = "作业Id不能少于2个字符")]
    public string JobId { get; set; } = string.Empty;

    /// <summary>
    /// 触发器类型FullName
    /// </summary>
    public string TriggerType { get; set; } = null!;

    /// <summary>
    /// 程序集名称
    /// </summary>
    public string AssemblyName { get; set; } = null!;

    /// <summary>
    /// 参数
    /// </summary>
    public string Args { get; set; } = null!;

    /// <summary>
    /// 描述信息
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// 起始时间
    /// </summary>
    public long? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public long? EndTime { get; set; }

    /// <summary>
    /// 最大触发次数（0:不限制，n:N次）
    /// </summary>
    public long MaxNumberOfRuns { get; set; }

    /// <summary>
    /// 最大出错次数（0:不限制，n:N次）
    /// </summary>
    public long MaxNumberOfErrors { get; set; }

    /// <summary>
    /// 重试次数
    /// </summary>
    public int NumRetries { get; set; }

    /// <summary>
    /// 重试间隔时间（ms）
    /// </summary>
    public int RetryTimeout { get; set; } = 1000;

    /// <summary>
    /// 是否立即启动
    /// </summary>
    public bool StartNow { get; set; } = true;

    /// <summary>
    /// 是否启动时执行一次
    /// </summary>
    public bool RunOnStart { get; set; } = false;

    /// <summary>
    /// 是否在启动时重置最大触发次数等于一次的作业
    /// </summary>
    public bool ResetOnlyOnce { get; set; } = true;
}