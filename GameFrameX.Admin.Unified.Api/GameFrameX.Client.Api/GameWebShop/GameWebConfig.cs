﻿namespace GameFrameX.Client.Api.GameWebShop;

/// <summary>
/// 网页商城支付配置（系统配置 code=web_shop_params）
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
    /// 回调地址（站点根地址或完整回调路由地址，下单前由 PublicCallbackNotifyUrlBuilder 规范化为
    /// 能命中 api/GameWebShop/NotifyWebShop/{packageId} 路由的完整地址，无法识别的地址拒绝下单）
    /// </summary>
    public string NotifyUrl { get; set; } = null!;
}