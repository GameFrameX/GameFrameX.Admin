using System.Text.Json;
using GameFrameX.Payment.Abstract;
using GameFrameX.Payment.Abstract.Models;
using GameFrameX.Payment.AliPay.Refund;
using GameFrameX.Payment.Apple.Refund;
using GameFrameX.Payment.Google.Refund;
using GameFrameX.Payment.Xsolla.Refund;

namespace GameFrameX.Core.Tests.Payment;

/// <summary>
/// C310 / GFX-718 渠道退款契约测试：四渠道 IRefundManager 实现齐全、ChannelName 对齐 PaymentChannelRegistry、
/// 主动退款能力矩阵锁定、不支持主动退款的渠道（Apple / Google）与不支持消费上报的渠道显式失败（不静默成功）。
/// </summary>
public class RefundManagerContractTests
{
    [Fact]
    public void AllFourChannels_HaveRefundManagerImplementation_AndChannelNamesAlignWithRegistry()
    {
        var managers = new IRefundManager[]
        {
            new XsollaRefundManager(),
            new AppleRefundManager(),
            new GoogleRefundManager(),
            new AliPayRefundManager(),
        };

        var channelNames = PaymentChannelRegistry.GetChannelNames();
        foreach (var manager in managers)
        {
            Assert.Contains(manager.ChannelName, channelNames);
        }

        Assert.Equal(4, managers.Select(m => m.ChannelName).Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void SupportsProactiveRefund_MatrixIsLocked()
    {
        // Xsolla / 支付宝：官方支持商家主动退款；Apple（用户发起 + App Store 裁决）/ Google（无主动退款 API）：不支持
        Assert.True(new XsollaRefundManager().SupportsProactiveRefund);
        Assert.True(new AliPayRefundManager().SupportsProactiveRefund);
        Assert.False(new AppleRefundManager().SupportsProactiveRefund);
        Assert.False(new GoogleRefundManager().SupportsProactiveRefund);
    }

    [Fact]
    public async Task Apple_RefundAsync_FailsExplicitly_BecauseMerchantRefundIsNotSupported()
    {
        var response = await new AppleRefundManager().RefundAsync(new RefundOrderContext(), new TestOptionSetting(), new HttpClient());

        Assert.False(response.IsSuccess);
        Assert.Contains("不支持商家主动发起退款", response.ErrorMessage);
    }

    [Fact]
    public async Task Google_RefundAsync_FailsExplicitly_BecauseMerchantRefundIsNotSupported()
    {
        var response = await new GoogleRefundManager().RefundAsync(new RefundOrderContext(), new TestOptionSetting(), new HttpClient());

        Assert.False(response.IsSuccess);
        Assert.Contains("不支持商家主动发起退款", response.ErrorMessage);
    }

    [Fact]
    public async Task NonAppleChannels_SendConsumptionInformation_FailsExplicitly()
    {
        var managers = new IRefundManager[]
        {
            new XsollaRefundManager(),
            new GoogleRefundManager(),
            new AliPayRefundManager(),
        };

        foreach (var manager in managers)
        {
            var response = await manager.SendConsumptionInformationAsync(new RefundOrderContext(), new TestOptionSetting(), new HttpClient());
            Assert.False(response.IsSuccess, $"{manager.ChannelName} 消费信息上报应显式失败");
        }
    }

    [Fact]
    public async Task Apple_SendConsumptionInformation_WithWrongOptionType_FailsWithoutNetwork()
    {
        var response = await new AppleRefundManager().SendConsumptionInformationAsync(new RefundOrderContext(), new TestOptionSetting(), new HttpClient());

        Assert.False(response.IsSuccess);
        Assert.Contains("配置类型错误", response.ErrorMessage);
    }

    [Fact]
    public async Task Xsolla_RefundAsync_WithWrongOptionType_Fails()
    {
        var response = await new XsollaRefundManager().RefundAsync(new RefundOrderContext(), new TestOptionSetting(), new HttpClient());

        Assert.False(response.IsSuccess);
        Assert.Contains("配置类型错误", response.ErrorMessage);
    }

    [Fact]
    public async Task Xsolla_RefundAsync_WithoutTransactionId_Fails()
    {
        var settings = new GameFrameX.Payment.Xsolla.Option.XsollaPayOptionSetting();
        var context = new RefundOrderContext { TransactionId = string.Empty, };
        var response = await new XsollaRefundManager().RefundAsync(context, settings, new HttpClient());

        Assert.False(response.IsSuccess);
        Assert.Contains("transactionId", response.ErrorMessage);
    }

    [Fact]
    public async Task AliPay_RefundAsync_WithWrongOptionType_Fails()
    {
        var response = await new AliPayRefundManager().RefundAsync(new RefundOrderContext(), new TestOptionSetting(), new HttpClient());

        Assert.False(response.IsSuccess);
        Assert.Contains("配置类型错误", response.ErrorMessage);
    }

    [Fact]
    public async Task AliPay_RefundAsync_WithoutOutTradeNo_Fails()
    {
        var settings = new GameFrameX.Payment.AliPay.AliPayOptionSetting();
        var context = new RefundOrderContext { OutTradeNo = string.Empty, };
        var response = await new AliPayRefundManager().RefundAsync(context, settings, new HttpClient());

        Assert.False(response.IsSuccess);
        Assert.Contains("outTradeNo", response.ErrorMessage);
    }

    [Fact]
    public void RefundRequest_SerializesDescriptionField()
    {
        var refundRequest = new RefundRequest("管理后台发起退款");

        var json = refundRequest.ToString();

        using (var document = JsonDocument.Parse(json))
        {
            Assert.Equal("管理后台发起退款", document.RootElement.GetProperty("description").GetString());
        }
    }

    /// <summary>
    /// 测试用渠道配置占位（不属于任何渠道的 Option 类型）。
    /// </summary>
    private sealed class TestOptionSetting : IPaymentOptionSetting
    {
    }
}
