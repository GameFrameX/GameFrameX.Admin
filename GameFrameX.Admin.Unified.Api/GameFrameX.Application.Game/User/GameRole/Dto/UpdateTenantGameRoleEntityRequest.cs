namespace GameFrameX.Application.Game.User.GameRole.Dto;

/// <summary>
/// 游戏角色更新输入参数
/// </summary>
public class UpdateTenantGameRoleEntityRequest : TenantGameRoleEntityBaseRequest, ICrudUpdateRequest
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long Id { get; set; }

    /// <summary>
    /// 客户端读取记录时的版本号。
    /// </summary>
    [Required(ErrorMessage = "版本号不能为空")]
    [Range(0, long.MaxValue, ErrorMessage = "版本号不能小于0")]
    public long? RowVersion { get; set; }

    /// <summary>
    /// 角色Id
    /// </summary>
    public new string RoleId { get; set; } = null!;
}