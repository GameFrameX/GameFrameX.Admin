namespace GameFrameX.Client.Api.GameWebShop;

/// <summary>
/// 游戏角色输出参数
/// </summary>
public sealed class GameWebConfig
{
    /// <summary>
    /// 邮件副标题
    /// </summary>
    public string WebShopSubTitle { get; set; } = null!;

    /// <summary>
    /// 邮件内容
    /// </summary>
    public string WebShopContent { get; set; } = null!;

    /// <summary>
    /// 邮件标题
    /// </summary>
    public string WebShopTitle { get; set; } = null!;

    /// <summary>
    /// 支付返回地址
    /// </summary>
    public string ReturnUrl { get; set; } = null!;

    /// <summary>
    /// md5加密秘钥
    /// </summary>
    public string Sign { get; set; } = null!;

    /// <summary>
    /// 支付类型
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// 商户id
    /// </summary>
    public string Pid { get; set; } = null!;

    /// <summary>
    /// 回调地址
    /// </summary>
    public string NotifyUrl { get; set; } = null!;

    /// <summary>
    /// 支付回调验签模式：log / enforce。
    /// <para>log（缺省）：重算 MD5 并记录 expected/received 签名但不拒绝，用于灰度观测 zlpay notify 签名口径；非白名单取值一律按 log 处理（灰度安全侧）。</para>
    /// <para>enforce：验签失败直接拒绝发货。切换前置条件：log 模式下观察到一次真实合法回调验签通过（口径确认）后再切 enforce，否则口径不符会打断真实收款。</para>
    /// </summary>
    public string VerifySignMode { get; set; } = "log";
}