using System.ComponentModel.DataAnnotations;
using GameFrameX.Client.Api.Services;
using GameFrameX.Client.Api.Services.PackageVersion;

namespace GameFrameX.Client.Api.GameFeatureSwitch.Dto;

/// <summary>
/// 活动/功能开关聚合下发请求（C347 客户端通道）。
/// <para>请求经 <c>RequireTenantAppContext</c> 强制租户 + 应用上下文（缺失 fail-fast，C314 语义）；
/// 应用作用域由服务端从应用上下文注入，客户端勿依赖请求体 AppId。</para>
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
    /// 应用作用域；由服务端从应用上下文注入（请求体声明不作为授权来源）
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 设备稳定标识，用于记录级灰度取模与白名单旁路；缺失时仅命中全量投放（与远程配置投放同语义）
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// 上次获取到的投放版本号（md5 hex）。与当前版本相同 → 响应 <c>Unchanged=true</c> 且 <c>Switches</c> 为空数组。
    /// </summary>
    public string? Version { get; set; }
}
