// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text.Json.Serialization;

namespace GameFrameX.Client.Api.Integrations.Payment.Dto.Google;

/// <summary>
/// Google Pay WebHook 的输入数据
/// </summary>
public sealed class GoogleWebHookRequest
{
    /// <summary>
    /// 消息内容
    /// </summary>
    [JsonPropertyName("message")]
    public GoogleWebHookInputMessage Message { get; set; } = null!;

    /// <summary>
    /// 项目ID，在Google Play 开发者控制台中的应用信息中配置的主题名称
    /// </summary>
    [JsonPropertyName("subscription")]
    public string Subscription { get; set; } = null!;

    /// <summary>Returns a string that represents the current object.</summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        return JsonHelper.Serialize(this);
    }
}