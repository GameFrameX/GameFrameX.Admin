namespace GameFrameX.Application.System.Const.Dto;

public class ConstResponse
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 编码
    /// </summary>
    public dynamic Code { get; set; } = null!;

    /// <summary>
    /// 扩展字段
    /// </summary>
    public dynamic Data { get; set; } = null!;
}