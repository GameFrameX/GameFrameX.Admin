using System.Text.RegularExpressions;
using GameFrameX.Application.Game.Online.Social.Dto;

namespace GameFrameX.Application.Game.Online.Social;

/// <summary>
/// 消息审计展示映射 + 兜底脱敏（唯一落点，change C350；vault:C2 S1.6）。
/// <para>
/// 双保险语义：Online 侧按其脱敏规则先行处理；Admin 侧在此对常见 PII（身份证 / 银行卡 / 手机号 / 邮箱）做兜底打码——
/// Online 侧规则升级滞后或漏匹配时，Admin 展示不裸奔。vault S1.6 详表落盘后按表回写替换兜底规则集。
/// </para>
/// <para>所有消息展示路径必须经 <see cref="MapToItemResponse"/>（守护测试断言零旁路直拷）；映射规则集中本文件，禁止散落 Service。</para>
/// </summary>
public static class OnlineMessageAuditDisplayMapper
{
    /// <summary>
    /// 身份证号：15 或 18 位（18 位末位可为 X/x），保留前 4 后 3（先于银行卡 / 手机号匹配，避免长号段被短规则截胡）。
    /// </summary>
    private static readonly Regex ChinaIdCardRegex = new(@"(?<!\d)\d{17}[\dXx](?!\d)|(?<!\d)\d{15}(?!\d)", RegexOptions.Compiled);

    /// <summary>
    /// 银行卡号：16～19 位数字，保留前 4 后 4。
    /// </summary>
    private static readonly Regex BankCardRegex = new(@"(?<!\d)\d{16,19}(?!\d)", RegexOptions.Compiled);

    /// <summary>
    /// 大陆手机号：11 位数字（1[3-9] 开头），保留前 3 后 4。
    /// </summary>
    private static readonly Regex ChinaMobileRegex = new(@"(?<!\d)1[3-9]\d{9}(?!\d)", RegexOptions.Compiled);

    /// <summary>
    /// 邮箱地址：保留首字符与域名。
    /// </summary>
    private static readonly Regex EmailRegex = new(@"(?<![\w.])[\w.]+@[\w-]+(\.[\w-]+)+", RegexOptions.Compiled);

    /// <summary>
    /// 消息行映射 + 兜底脱敏：Content 打码，其余字段原样。
    /// </summary>
    public static OnlineMessageAuditItemResponse MapToItemResponse(OnlineMessageAuditMessageResponse message)
    {
        return new OnlineMessageAuditItemResponse
        {
            MessageId = message.MessageId,
            ChannelKind = message.ChannelKind,
            ParticipantPlayerIds = message.ParticipantPlayerIds,
            Content = MaskSensitiveContent(message.Content ?? string.Empty),
            OccurredAt = message.OccurredAt,
        };
    }

    /// <summary>
    /// 兜底脱敏：身份证保留前 4 后 3；银行卡保留前 4 后 4；手机号保留前 3 后 4；邮箱保留首字符与域名。
    /// </summary>
    private static string MaskSensitiveContent(string content)
    {
        var masked = ChinaIdCardRegex.Replace(content, match => KeepAround(match.Value, 4, 3));
        masked = BankCardRegex.Replace(masked, match => KeepAround(match.Value, 4, 4));
        masked = ChinaMobileRegex.Replace(masked, match => KeepAround(match.Value, 3, 4));
        masked = EmailRegex.Replace(masked, MaskEmail);
        return masked;
    }

    /// <summary>
    /// 数字串打码：保留前 keepPrefix 后 keepSuffix，中间以 * 等长替代。
    /// </summary>
    private static string KeepAround(string value, int keepPrefix, int keepSuffix)
    {
        if (value.Length <= keepPrefix + keepSuffix)
        {
            return new string('*', value.Length);
        }

        return value[..keepPrefix] + new string('*', value.Length - keepPrefix - keepSuffix) + value[^keepSuffix..];
    }

    /// <summary>
    /// 邮箱打码：本地部分保留首字符 + *** + @域名。
    /// </summary>
    private static string MaskEmail(global::System.Text.RegularExpressions.Match match)
    {
        var value = match.Value;
        var atIndex = value.IndexOf('@');
        if (atIndex <= 1)
        {
            return value;
        }

        return value[0] + "***" + value[atIndex..];
    }
}
