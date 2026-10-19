// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Application.Game.ServerManager;

/// <summary>
/// 网络配置
/// </summary>
public sealed class NetworkConfig
{
    /// <summary>
    /// 获取或设置网络的名称
    /// </summary>
    [Required]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 获取或设置网络的子网地址
    /// </summary>
    [Required]
    public string Subnet { get; set; } = null!;

    /// <summary>
    /// 获取或设置网络的网关地址
    /// </summary>
    [Required]
    public string Gateway { get; set; } = null!;

    /// <summary>
    /// 获取或设置网络的IP地址
    /// </summary>
    public string IpAddress { get; set; } = null!;

    /// <summary>
    /// 是否强制重新创建网络
    /// </summary>
    public bool IsRecreate { get; set; }
}