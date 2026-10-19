namespace GameFrameX.Application.System.Logging.Dto;

public class LogRequest
{
    /// <summary>
    /// 开始时间
    /// </summary>
    public long? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public long? EndTime { get; set; }
}