using GameFrameX.Client.Api.Services.PayVerification;
using GameFrameX.Payment.Abstract;
using GameFrameX.Payment.Xsolla;
using GameFrameX.Payment.Google;
using GameFrameX.Payment.Apple;
using GameFrameX.Payment.AliPay;
using GameFrameX.Payment.Wechat;
using GameFrameX.Application.Game.Order.GamePaymentConfig;
using GameFrameX.Client.Api.Services;

namespace GameFrameX.Client.Api.Integrations.Payment;

/// <summary>
/// 支付渠道服务扩展
/// </summary>
public static class PaymentChannelServiceExtensions
{
    /// <summary>
    /// 添加支付渠道服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddPaymentChannels(this IServiceCollection services)
    {
        // 注册所有支付渠道处理器
        services.AddScoped<IPaymentChannelHandler, XsollaPaymentHandler>();
        services.AddScoped<IPaymentChannelHandler, ApplePaymentHandler>();
        services.AddScoped<IPaymentChannelHandler, GooglePaymentHandler>();
        services.AddScoped<IPaymentChannelHandler, AliPayPaymentHandler>();
        services.AddScoped<IPaymentChannelHandler, WechatPaymentHandler>();

        // 注册配置提供器
        services.AddScoped<IPaymentOptionSettingProvider, PaymentOptionSettingProvider>();

        // 注册支付配置 schema 提供器
        services.AddScoped<IPaymentConfigSchemaProvider, PaymentConfigSchemaProvider>();

        // 注册支付渠道工厂和选择器
        services.AddScoped<IPaymentChannelFactory, PaymentChannelFactory>();
        services.AddScoped<IPaymentChannelSelector, PaymentChannelSelector>();
        services.AddScoped<IGooglePubSubPushAuthenticator, GooglePubSubPushAuthenticator>();
        services.AddScoped<IGooglePlayPurchaseService, GooglePlayPurchaseService>();

        return services;
    }
}