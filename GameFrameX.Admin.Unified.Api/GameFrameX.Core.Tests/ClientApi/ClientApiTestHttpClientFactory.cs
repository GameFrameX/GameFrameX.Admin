namespace GameFrameX.Core.Tests.ClientApi;

internal sealed class ClientApiTestHttpClientFactory : IHttpClientFactory
{
    private readonly HttpMessageHandler _handler;

    public ClientApiTestHttpClientFactory(HttpMessageHandler handler)
    {
        _handler = handler;
    }

    public HttpClient CreateClient(string name)
    {
        return new HttpClient(_handler, false);
    }
}