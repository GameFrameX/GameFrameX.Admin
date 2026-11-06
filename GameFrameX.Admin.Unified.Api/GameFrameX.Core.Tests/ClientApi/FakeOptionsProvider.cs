using GameFrameX.Advertisement.Abstract;
using GameFrameX.Client.Api.Services.ClientApiOptions;

namespace GameFrameX.Core.Tests.ClientApi;

/// <summary>
/// 测试用 <see cref="IClientApiOptionsProvider"/> 桩，按预设策略返回配置，供 Issuer / 邮件等单测使用。
/// </summary>
internal sealed class FakeOptionsProvider : IClientApiOptionsProvider
{
    private EmailOptions _emailOptions = null!;
    private AdvertisementCallbackOptions _adOptions = new();
    private ClientApiTokenPolicyOptions _tokenPolicy = null!;
    private GameLoginTicketPolicyOptions _ticketPolicy = new() { TicketExpireTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60, };
    private OrderPolicyOptions _orderPolicy = new();
    private AreaPolicyOptions _areaPolicy = new();

    public Task<EmailOptions> GetEmailOptionsAsync()
    {
        return Task.FromResult(_emailOptions);
    }

    public Task<AdvertisementCallbackOptions> GetAdvertisementCallbackOptionsAsync()
    {
        return Task.FromResult(_adOptions);
    }

    public Task<ClientApiTokenPolicyOptions> GetTokenPolicyAsync()
    {
        return Task.FromResult(_tokenPolicy);
    }

    public Task<GameLoginTicketPolicyOptions> GetGameLoginTicketPolicyAsync()
    {
        return Task.FromResult(_ticketPolicy);
    }

    public Task<OrderPolicyOptions> GetOrderPolicyAsync()
    {
        return Task.FromResult(_orderPolicy);
    }

    public Task<AreaPolicyOptions> GetAreaPolicyAsync()
    {
        return Task.FromResult(_areaPolicy);
    }

    public static FakeOptionsProvider WithTokenPolicy(string signingKey, int lifetimeMinutes)
    {
        return new FakeOptionsProvider
        {
            _tokenPolicy = new ClientApiTokenPolicyOptions { SigningKey = signingKey, LifetimeMinutes = lifetimeMinutes, },
        };
    }

    public static FakeOptionsProvider WithoutTokenPolicy()
    {
        return new FakeOptionsProvider
        {
            _tokenPolicy = new ClientApiTokenPolicyOptions { SigningKey = null, LifetimeMinutes = 60, },
        };
    }

    /// <summary>票据到期时间（Unix 秒）；运行时会按平台默认值和 30 天上限归一化。</summary>
    public static FakeOptionsProvider WithTicketPolicy(long ticketExpireTime)
    {
        return new FakeOptionsProvider
        {
            _tokenPolicy = new ClientApiTokenPolicyOptions { SigningKey = "unit-test-signing-key", LifetimeMinutes = 60, },
            _ticketPolicy = new GameLoginTicketPolicyOptions { TicketExpireTime = ticketExpireTime, EnableKeyAutoCreate = true, },
        };
    }

    public static FakeOptionsProvider WithEmailOptions(string fromAddress)
    {
        return new FakeOptionsProvider
        {
            _emailOptions = new EmailOptions
            {
                FromEmailAddress = fromAddress,
                FromEmailSecretId = "secret-id",
                FromEmailSecretKey = "secret-key",
            },
        };
    }

    public static FakeOptionsProvider WithoutEmailOptions()
    {
#pragma warning disable CS8625 // 测试桩：模拟租户未配置邮件选项，接口契约允许返回 null
        return new FakeOptionsProvider { _emailOptions = null, };
#pragma warning restore CS8625
    }

    public static FakeOptionsProvider WithAreaPolicy(bool enableReturnHttpManager)
    {
        return new FakeOptionsProvider
        {
            _tokenPolicy = new ClientApiTokenPolicyOptions { SigningKey = "unit-test-signing-key", LifetimeMinutes = 60, },
            _areaPolicy = new AreaPolicyOptions { EnableReturnHttpManager = enableReturnHttpManager, },
        };
    }
}