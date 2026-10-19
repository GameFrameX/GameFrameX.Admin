using System.ComponentModel.DataAnnotations;
using GameFrameX.Core.BaseInputOutput;

namespace GameFrameX.Hub.Api.Dto.GameArea;

/// <summary>
/// 游戏区服删除输入参数
/// </summary>
public class RemoveTenantGameAreaEntityRequest : BaseAddRequest
{
    /// <summary>
    /// 分支Id
    /// </summary>
    [Required(ErrorMessage = "分支Id不能为空")]
    public long Id { get; set; }
}