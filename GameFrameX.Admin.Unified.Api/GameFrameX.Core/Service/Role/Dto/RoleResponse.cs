namespace GameFrameX.Core.Service.Role.Dto;

/// <summary>
/// 角色列表输出参数
/// </summary>
public class RoleResponse
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 编码
    /// </summary>
    public string Code { get; set; } = null!;
}