namespace GameFrameX.Core.Job;

/// <summary>
/// HTTP 请求作业
/// </summary>
[JobDetail("job_http")]
public class HttpJob : IJob
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HttpJob(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        var properties = context.JobDetail?.Properties;
        if (string.IsNullOrWhiteSpace(properties))
        {
            return;
        }

        var dict = Json.Deserialize<Dictionary<string, string>>(properties);
        if (dict == null || !dict.TryGetValue("HttpUrl", out var httpUrl) || string.IsNullOrWhiteSpace(httpUrl))
        {
            return;
        }

        dict.TryGetValue("HttpMethod", out var httpMethod);
        httpMethod ??= "GET";

        using (var httpClient = _httpClientFactory.CreateClient())
        {
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            HttpResponseMessage response;
            if (httpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                dict.TryGetValue("HttpBody", out var httpBody);
                var content = new StringContent(httpBody ?? "", Encoding.UTF8, "application/json");
                response = await httpClient.PostAsync(httpUrl, content, stoppingToken);
            }
            else
            {
                response = await httpClient.GetAsync(httpUrl, stoppingToken);
            }

            context.Result = $"StatusCode: {(int)response.StatusCode}";
        }
    }
}