using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Client.Api.GameUser.Dto;

/// <summary>
/// 游戏账号更新输入参数
/// </summary>
public class UpdateTenantGameUserEntityRequest : TenantGameUserEntityBaseRequest
{
    /// <summary>
    /// Id
    /// </summary>
    [Required(ErrorMessage = "Id不能为空")]
    public long Id { get; set; }
}