using GameFrameX.Core.Middleware;

namespace GameFrameX.Client.Api.Services.PublicCallback;

/// <summary>
/// 标记公开 provider 回调入口。
/// <para>provider 与回调类型由 action 自己声明，路由解析层只读取 endpoint metadata，不维护 controller/action 名称表。</para>
/// <para>实现 <see cref="IProtoVersionExempt"/>：外部平台 S2S 回调不携带 X-Proto-Ver，豁免协议版本缺头拒绝。</para>
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class PublicCallbackEndpointAttribute : Attribute, IProtoVersionExempt
{
    /// <summary>
    /// 创建公开回调入口标记。
    /// </summary>
    public PublicCallbackEndpointAttribute(string provider, string callbackKind)
    {
        Provider = provider;
        CallbackKind = callbackKind;
    }

    /// <summary>
    /// 第三方 provider 标识。
    /// </summary>
    public string Provider { get; }

    /// <summary>
    /// 回调类型，例如 payment 或 ad。
    /// </summary>
    public string CallbackKind { get; }
}