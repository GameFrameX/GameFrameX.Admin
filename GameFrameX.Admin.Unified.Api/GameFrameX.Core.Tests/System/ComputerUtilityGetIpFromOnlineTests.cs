using GameFrameX.Core.Utility;
using GameFrameX.Payment.Apple.AppStoreServer;
using GameFrameX.Payment.Apple.AppStoreServer.Models;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// ComputerUtility.GetIpFromOnlineAsync（C200 异步化 + 内存缓存）与 AppStoreServerApiClient 兜底 HttpClient 校验的单元测试。
/// </summary>
public class ComputerUtilityGetIpFromOnlineTests
{
    public ComputerUtilityGetIpFromOnlineTests()
    {
        // 静态缓存隔离：每个用例重置，避免用例间相互污染。
        ComputerUtility.ResetRemoteIpCacheForTests();
    }

    [Fact]
    public async Task GetIpFromOnlineAsync_ParsesRemoteIpFromResponse()
    {
        var handler = new StubHandler("当前 IP：1.2.3.4 来自于：中国");
        var client = new HttpClient(handler);

        var ip = await ComputerUtility.GetIpFromOnlineAsync(client);

        Assert.Equal("1.2.3.4 中国", ip);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetIpFromOnlineAsync_ServesFromCacheOnSecondCall()
    {
        var handler = new StubHandler("当前 IP：10.0.0.1 来自于：CN");
        var client = new HttpClient(handler);

        await ComputerUtility.GetIpFromOnlineAsync(client);
        var ip = await ComputerUtility.GetIpFromOnlineAsync(client);

        Assert.Equal("10.0.0.1 CN", ip);
        // 第二次命中缓存，不应再次发起 HTTP 请求
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetIpFromOnlineAsync_ReturnsUnknownWhenResponseHasNoMarker()
    {
        var handler = new StubHandler("garbage without marker");
        var client = new HttpClient(handler);

        var ip = await ComputerUtility.GetIpFromOnlineAsync(client);

        Assert.Equal("未知", ip);
    }

    [Fact]
    public async Task GetIpFromOnlineAsync_ReturnsUnknownOnTransportFailure()
    {
        var handler = new StubHandler(throwOnSend: true);
        var client = new HttpClient(handler);

        var ip = await ComputerUtility.GetIpFromOnlineAsync(client);

        Assert.Equal("未知", ip);
    }

    [Fact]
    public async Task GetIpFromOnlineAsync_ThrowsWhenHttpClientNull()
    {
#pragma warning disable CS8625 // 测试意图：传入 null HttpClient 验证 ArgumentNullException
        await Assert.ThrowsAsync<ArgumentNullException>(() => ComputerUtility.GetIpFromOnlineAsync(null));
#pragma warning restore CS8625
    }

    [Fact]
    public void AppStoreServerApiClient_ThrowsWhenHttpClientNull()
    {
        // C200：移除 new HttpClient() 兜底，未注入 HttpClient 必须显式失败而非静默新建。
#pragma warning disable CS8625 // 测试意图：传入 null HttpClient 验证 ArgumentNullException
        Assert.Throws<ArgumentNullException>(() =>
                                                 new AppStoreServerApiClient("key", "keyId", "issuerId", "bundleId", ApplePayEnvironment.Production, null));
#pragma warning restore CS8625
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly string? _response;
        private readonly bool _throwOnSend;
        private int _callCount;

        public StubHandler(string? response = null, bool throwOnSend = false)
        {
            _response = response;
            _throwOnSend = throwOnSend;
        }

        public int CallCount
        {
            get { return _callCount; }
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _callCount);
            if (_throwOnSend)
            {
                return Task.FromException<HttpResponseMessage>(new HttpRequestException("simulated transport failure"));
            }

            return Task.FromResult(new HttpResponseMessage
            {
                Content = new StringContent(_response ?? string.Empty),
            });
        }
    }
}