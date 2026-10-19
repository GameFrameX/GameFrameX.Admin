// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目进行危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的行为！

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameFrameX.Client.Api.GameUser.Dto;

/// <summary>
/// 匿名设备登录输入参数（C12 A1：承接旧 SSO Lookup 语义——按设备标识找号或建号）
/// </summary>
public sealed class LoginDeviceRequest
{
    /// <summary>
    /// 设备标识（主绑定键，对应客户端 DeviceInfo.GetDeviceID）
    /// </summary>
    [Required(ErrorMessage = "设备标识不能为空")]
    public string DeviceId { get; set; } = null!;

    /// <summary>
    /// 厂商标识（记录用，不参与查找）
    /// </summary>
    public string VendorId { get; set; } = string.Empty;

    /// <summary>
    /// 安装实例标识
    /// </summary>
    public string InstallId { get; set; } = string.Empty;

    /// <summary>
    /// 平台标识（OS 名，如 "Mac OS X 26.5.2"）
    /// </summary>
    public string Platform { get; set; } = string.Empty;

    /// <summary>
    /// 客户端版本
    /// </summary>
    public string ClientVersion { get; set; } = string.Empty;

    /// <summary>
    /// 渠道号（string 契约，对齐 GameAppVersion/RemoteConfig 先例；客户端 CURRENT_PLATFORM 数字须 tostring）
    /// </summary>
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// 未声明字段，用于拒绝历史上下文字段覆盖 Header 或 token claims。
    /// <para>可空注解必要：non-nullable 属性会被 MVC 隐式 [Required] 拦截（裸声明字段
    /// body 无未声明键时字典为 null → 400 ExtraFields field is required）。</para>
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtraFields { get; set; }
}
