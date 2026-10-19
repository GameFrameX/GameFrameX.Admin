namespace GameFrameX.Application.System.Region.Dto;

public class PageRegionRequest : BasePageRequest
{
    /// <summary>
    /// 父节点Id
    /// </summary>
    public long Pid { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 编码
    /// </summary>
    public string Code { get; set; } = string.Empty;
}