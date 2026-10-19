namespace GameFrameX.Application.System.Tenant.Position.Dto;

public class PosRequest
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 编码
    /// </summary>
    public string Code { get; set; } = string.Empty;
}