// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text.Json.Serialization;

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The success or error information the App Store server records when it attempts to send a server notification to your server.
/// <para>App Store 服务器尝试向您的服务器发送服务器通知时记录的成功或错误信息。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/sendattemptresult</para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SendAttemptResult
{
    /// <summary>
    /// Success.
    /// <para>成功。</para>
    /// </summary>
    SUCCESS,

    /// <summary>
    /// Timed out.
    /// <para>超时。</para>
    /// </summary>
    TIMED_OUT,

    /// <summary>
    /// TLS issue.
    /// <para>TLS 问题。</para>
    /// </summary>
    TLS_ISSUE,

    /// <summary>
    /// Circular redirect.
    /// <para>循环重定向。</para>
    /// </summary>
    CIRCULAR_REDIRECT,

    /// <summary>
    /// No response.
    /// <para>无响应。</para>
    /// </summary>
    NO_RESPONSE,

    /// <summary>
    /// Socket issue.
    /// <para>Socket 问题。</para>
    /// </summary>
    SOCKET_ISSUE,

    /// <summary>
    /// Unsupported charset.
    /// <para>不支持的字符集。</para>
    /// </summary>
    UNSUPPORTED_CHARSET,

    /// <summary>
    /// Invalid response.
    /// <para>无效响应。</para>
    /// </summary>
    INVALID_RESPONSE,

    /// <summary>
    /// Premature close.
    /// <para>连接过早关闭。</para>
    /// </summary>
    PREMATURE_CLOSE,

    /// <summary>
    /// Unsuccessful HTTP response code.
    /// <para>不成功的 HTTP 响应代码。</para>
    /// </summary>
    UNSUCCESSFUL_HTTP_RESPONSE_CODE,

    /// <summary>
    /// Other.
    /// <para>其他。</para>
    /// </summary>
    OTHER,
}