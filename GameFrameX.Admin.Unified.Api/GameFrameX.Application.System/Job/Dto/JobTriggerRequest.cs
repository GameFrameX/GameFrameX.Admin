namespace GameFrameX.Application.System.Job.Dto;

public class JobTriggerRequest
{
    /// <summary>
    /// 作业Id
    /// </summary>
    public string JobId { get; set; } = string.Empty;

    /// <summary>
    /// 触发器Id
    /// </summary>
    public string TriggerId { get; set; } = string.Empty;
}