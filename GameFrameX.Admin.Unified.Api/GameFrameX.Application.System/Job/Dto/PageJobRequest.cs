namespace GameFrameX.Application.System.Job.Dto;

public class PageJobRequest : BasePageRequest
{
    /// <summary>
    /// 作业Id
    /// </summary>
    public string JobId { get; set; } = string.Empty;

    /// <summary>
    /// 描述信息
    /// </summary>
    public string Description { get; set; } = string.Empty;
}