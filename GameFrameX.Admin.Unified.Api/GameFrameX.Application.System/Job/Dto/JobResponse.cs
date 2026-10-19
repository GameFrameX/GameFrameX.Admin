namespace GameFrameX.Application.System.Job.Dto;

public class JobResponse
{
    /// <summary>
    /// 作业信息
    /// </summary>
    public SystemJobDetailEntity JobDetail { get; set; } = new();

    /// <summary>
    /// 触发器集合
    /// </summary>
    public List<SystemJobTriggerEntity> JobTriggers { get; set; } = new();
}