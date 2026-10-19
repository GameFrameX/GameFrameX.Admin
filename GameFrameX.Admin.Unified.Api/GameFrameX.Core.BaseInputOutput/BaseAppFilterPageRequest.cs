namespace GameFrameX.Core.BaseInputOutput;

/// <summary>
/// 按渠道/平台/版本/包名/语言筛选的分页查询输入参数
/// </summary>
public class BaseAppFilterPageRequest : BasePageRequest
{
    /// <summary>
    /// 渠道
    /// </summary>
    public string? Channel { get; set; }

    /// <summary>
    /// 平台
    /// </summary>
    public string? Platform { get; set; }

    /// <summary>
    /// 游戏版本
    /// </summary>
    public string? AppVersion { get; set; }

    /// <summary>
    /// 包名
    /// </summary>
    public string? Package { get; set; }

    /// <summary>
    /// 语言
    /// </summary>
    public string? Language { get; set; }
}