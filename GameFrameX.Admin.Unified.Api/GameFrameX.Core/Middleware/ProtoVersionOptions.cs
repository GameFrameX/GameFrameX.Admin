namespace GameFrameX.Core.Middleware;

/// <summary>
/// 协议版本白名单配置（change C5 server-envelope-middleware）。
/// </summary>
public sealed class ProtoVersionOptions
{
    /// <summary>
    /// 允许的协议版本（格式 = PROJECT_ID + "/" + PROTO_VER，如 cola/1.0.0）。
    /// <para>不兼容协议变更提 major、加字段提 minor（客户端 semver 约定）；
    /// 当前白名单仅起步版本，配置可扩展。</para>
    /// </summary>
    public List<string> AllowedVersions { get; set; } = new() { "cola/1.0.0", };
}