using System.ComponentModel.DataAnnotations;
using GameFrameX.Client.Api.Services;
using GameFrameX.Client.Api.Services.PackageVersion;

namespace GameFrameX.Client.Api.GameFeatureSwitch.Dto;

/// <summary>
/// 活动/功能开关聚合下发请求，按分包与投放条件匹配主库配置。
/// </summary>
public class FeatureSwitchRequest : IPackageRequest
{
    /// <summary>
    /// 语言
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Language { get; set; } = null!;

    /// <summary>
    /// 包名（服务端经母包解析器展开为候选集合后参与作用域匹配）
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string PackageName { get; set; } = null!;

    /// <summary>
    /// 平台
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Platform { get; set; } = null!;

    /// <summary>
    /// 主渠道名称
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Channel { get; set; } = null!;

    /// <summary>
    /// 程序版本。开关投放无程序版本维度，本字段仅对齐既有包请求形态，不参与解析。
    /// </summary>
    public string AppVersion { get; set; } = null!;



    /// <summary>
    /// 设备稳定标识，用于记录级灰度取模与白名单旁路；缺失时仅命中全量投放（与远程配置投放同语义）
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// 上次获取到的投放版本号（md5 hex）。与当前版本相同 → 响应 <c>Unchanged=true</c> 且 <c>Switches</c> 为空数组。
    /// </summary>
    public string? Version { get; set; }
}
