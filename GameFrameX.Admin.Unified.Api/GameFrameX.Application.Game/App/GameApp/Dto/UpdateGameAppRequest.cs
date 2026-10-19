namespace GameFrameX.Application.Game.App.GameApp.Dto;

/// <summary>
/// 游戏应用更新输入参数
/// </summary>
public class UpdateGameAppRequest : BaseUpdateRequest
{
    /// <summary>
    /// 应用名称
    /// </summary>
    [Required(ErrorMessage = "应用名称不能为空")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 应用描述
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// 应用图标
    /// </summary>
    public string Icon { get; set; } = null!;

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; } = null!;

    /// <summary>
    /// 是否要求请求签名（首版默认 false，落地开关供后续灰度启用）
    /// </summary>
    public bool SignatureRequired { get; set; }

    /// <summary>
    /// 是否要求设备证明（首版默认 false，落地开关供后续灰度启用）
    /// </summary>
    public bool DeviceProofRequired { get; set; }

    /// <summary>
    /// 风控策略 opaque 字符串，由 C10 / C11 风控与内容运营消费
    /// </summary>
    public string RiskPolicy { get; set; } = null!;
}