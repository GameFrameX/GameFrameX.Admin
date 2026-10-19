using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Client.Api.GameRole.Dto;

/// <summary>
/// 游戏角色更新输入参数
/// </summary>
public class UpdateTenantGameRoleEntityRequest : TenantGameRoleEntityBaseRequest
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long Id { get; set; }
}