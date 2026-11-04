namespace GameFrameX.Application.Platform.OpenAccess.Dto;

/// <summary>
/// 开放接口身份输入参数
/// </summary>
public class PageOpenAccessRequest : BasePageRequest
{
    /// <summary>
    /// 身份标识
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;
}