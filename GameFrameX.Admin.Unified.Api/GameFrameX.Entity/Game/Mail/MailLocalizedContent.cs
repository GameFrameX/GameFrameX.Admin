// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

namespace GameFrameX.Entity.Game.Mail;

/// <summary>
/// 运营邮件多语言内容项。字段名逐字对齐 Server <c>MailLocalizedContent</c>（跨仓库契约）。
/// <para>至少含一种语言；缺失语言由客户端回退到首项。</para>
/// </summary>
public sealed class MailLocalizedContent
{
    /// <summary>
    /// 语言代码（小写 BCP 47 tag，如 <c>zh-cn</c>、<c>en-us</c>）。
    /// </summary>
    public string Language { get; set; } = null!;

    /// <summary>
    /// 该语言下的展示文案。允许内嵌参数占位符（如 <c>{0}</c>），由客户端按上下文替换。
    /// </summary>
    public string Text { get; set; } = null!;
}