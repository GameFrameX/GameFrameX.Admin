// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text.Json.Serialization;

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The request body for uploading a message, which includes the message text and an optional image reference.
/// <para>用于上传消息的请求体，其中包括消息文本和可选图像引用。</para>
/// <para>https://developer.apple.com/documentation/retentionmessaging/uploadmessagerequestbody</para>
/// </summary>
public class UploadMessageRequestBody
{
    /// <summary>
    /// The header text of the retention message that the system displays to customers.
    /// Maximum length: 66 characters
    /// <para>系统向客户显示的保留消息的标题文本。</para>
    /// <para>最大长度：66 个字符</para>
    /// <para>https://developer.apple.com/documentation/retentionmessaging/header</para>
    /// </summary>
    [JsonPropertyName("header")]
    public string Header { get; set; } = null!;

    /// <summary>
    /// The body text of the retention message that the system displays to customers.
    /// Maximum length: 144 characters
    /// <para>系统向客户显示的保留消息的正文文本。</para>
    /// <para>最大长度：144 个字符</para>
    /// <para>https://developer.apple.com/documentation/retentionmessaging/body</para>
    /// </summary>
    [JsonPropertyName("body")]
    public string Body { get; set; } = null!;

    /// <summary>
    /// The optional image identifier and its alternative text to appear as part of a text-based message with an image.
    /// <para>可选图像标识符及其替代文本，作为带有图像的基于文本的消息的一部分出现。</para>
    /// <para>https://developer.apple.com/documentation/retentionmessaging/uploadmessageimage</para>
    /// </summary>
    [JsonPropertyName("image")]
    public UploadMessageImage? Image { get; set; }
}