using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Client.Api.GameUser.Dto;

/// <summary>
/// 账号重新激活
/// </summary>
public class GameUserReActiveRequest
{
    /// <summary>
    /// 账号ID
    /// </summary>
    [Required(ErrorMessage = "账号ID不能为空")]
    public long Id { get; set; }
}