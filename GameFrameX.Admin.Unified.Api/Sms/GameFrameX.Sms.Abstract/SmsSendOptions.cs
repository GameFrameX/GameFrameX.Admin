namespace GameFrameX.Sms.Abstract;

/// <summary>
/// 短信发送输入参数。
/// <para>屏蔽各 Provider 私有 SDK 类型（Aliyun <c>SendSmsRequest</c>、后续 TencentCloud/Huawei 等），
/// 所有调用方通过本模型与 <see cref="ISmsSender"/> 交互。</para>
/// <para>字段按渠道需求提供，Provider 实现按需读取自身关心的子集，忽略无关字段。</para>
/// </summary>
public sealed class SmsSendOptions
{
    /// <summary>
    /// 接收方手机号（必填，多个用半角逗号分隔，由具体渠道 SDK 自行分隔）。
    /// </summary>
    public string PhoneNumbers { get; set; } = string.Empty;

    /// <summary>
    /// 短信签名（必填）。
    /// </summary>
    public string SignName { get; set; } = string.Empty;

    /// <summary>
    /// 短信模板 Code（必填；阿里云以 <c>SMS_</c> 开头）。
    /// </summary>
    public string TemplateCode { get; set; } = string.Empty;

    /// <summary>
    /// 模板变量 JSON 字符串（必填；阿里云 SDK 直接序列化进 <c>TemplateParam</c>）。
    /// </summary>
    public string TemplateParamJson { get; set; } = string.Empty;

    /// <summary>
    /// Provider 凭据 AccessKeyId（Aliyun 必填；其它渠道按需扩展）。
    /// </summary>
    public string AccessKeyId { get; set; } = string.Empty;

    /// <summary>
    /// Provider 凭据 AccessKeySecret（Aliyun 必填；其它渠道按需扩展）。
    /// </summary>
    public string AccessKeySecret { get; set; } = string.Empty;

    /// <summary>
    /// Provider Endpoint（可选；缺省为各渠道的固定默认 Endpoint，如 <c>dysmsapi.aliyuncs.com</c>）。
    /// </summary>
    public string? Endpoint { get; set; }
}