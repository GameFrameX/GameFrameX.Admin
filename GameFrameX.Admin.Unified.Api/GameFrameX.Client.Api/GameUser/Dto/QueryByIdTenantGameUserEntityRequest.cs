using System.ComponentModel.DataAnnotations;
using GameFrameX.Core.BaseInputOutput;

namespace GameFrameX.Client.Api.GameUser.Dto;

/// <summary>
/// 游戏账号主键查询输入参数
/// </summary>
public class QueryByIdTenantGameUserEntityRequest : BaseIdRequest
{
    /// <summary>
    /// Client API 访问凭证
    /// </summary>
    [Required(ErrorMessage = "授权无效")]
    public string AccessToken { get; set; } = null!;
}