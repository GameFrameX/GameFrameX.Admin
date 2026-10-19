namespace GameFrameX.Core.Service.Role.Dto;

public sealed class RoleRequest : BaseIdRequest
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}