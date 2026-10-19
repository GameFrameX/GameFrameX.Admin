using GameFrameX.Advertisement.Abstract;
using GameFrameX.Client.Api.Services.ClientApiTenantOptions;

namespace GameFrameX.Core.Tests.ClientApi;

/// <summary>
/// 测试用 <see cref="IClientApiTenantOptionsProvider"/> 桩，按预设策略返回配置，供 Issuer / 邮件等单测使用。
/// </summary>
internal sealed class FakeOptionsProvider : IClientApiTenantOptionsProvider
{
    private TenantEmailOptions _emailOptions = null!;
    private TenantAdvertisementCallbackOptions _adOptions = new();
    private TenantClientApiTokenPolicyOptions _tokenPolicy = null!;
    private TenantGameLoginTicketPolicyOptions _ticketPolicy = new() { TicketExpireTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60, };
    private TenantOrderPolicyOptions _orderPolicy = new();
    private TenantAreaPolicyOptions _areaPolicy = new();

    public ClientApiTenantResolution ResolveContext()
    {
        return new ClientApiTenantResolution { TenantId = 10, AppId = 20, };
    }

    public Task<TenantEmailOptions> GetEmailOptionsAsync()
    {
        return Task.FromResult(_emailOptions);
    }

    public Task<TenantAdvertisementCallbackOptions> GetAdvertisementCallbackOptionsAsync()
    {
        return Task.FromResult(_adOptions);
    }

    public Task<TenantClientApiTokenPolicyOptions> GetTokenPolicyAsync()
    {
        return Task.FromResult(_tokenPolicy);
    }

    public Task<TenantGameLoginTicketPolicyOptions> GetGameLoginTicketPolicyAsync()
    {
        return Task.FromResult(_ticketPolicy);
    }

    public Task<TenantOrderPolicyOptions> GetOrderPolicyAsync()
    {
        return Task.FromResult(_orderPolicy);
    }

    public Task<TenantAreaPolicyOptions> GetAreaPolicyAsync()
    {
        return Task.FromResult(_areaPolicy);
    }

    public static FakeOptionsProvider WithTokenPolicy(string signingKey, int lifetimeMinutes)
    {
        return new FakeOptionsProvider
        {
            _tokenPolicy = new TenantClientApiTokenPolicyOptions { SigningKey = signingKey, LifetimeMinutes = lifetimeMinutes, },
        };
    }

    public static FakeOptionsProvider WithoutTokenPolicy()
    {
        return new FakeOptionsProvider
        {
            _tokenPolicy = new TenantClientApiTokenPolicyOptions { SigningKey = null, LifetimeMinutes = 60, },
        };
    }

    /// <summary>票据到期时间（Unix 秒）；运行时会按平台默认值和 30 天上限归一化。</summary>
    public static FakeOptionsProvider WithTicketPolicy(long ticketExpireTime)
    {
        return new FakeOptionsProvider
        {
            _tokenPolicy = new TenantClientApiTokenPolicyOptions { SigningKey = "unit-test-signing-key", LifetimeMinutes = 60, },
            _ticketPolicy = new TenantGameLoginTicketPolicyOptions { TicketExpireTime = ticketExpireTime, EnableKeyAutoCreate = true, },
        };
    }

    public static FakeOptionsProvider WithEmailOptions(string fromAddress)
    {
        return new FakeOptionsProvider
        {
            _emailOptions = new TenantEmailOptions
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
            _tokenPolicy = new TenantClientApiTokenPolicyOptions { SigningKey = "unit-test-signing-key", LifetimeMinutes = 60, },
            _areaPolicy = new TenantAreaPolicyOptions { EnableReturnHttpManager = enableReturnHttpManager, },
        };
    }
}