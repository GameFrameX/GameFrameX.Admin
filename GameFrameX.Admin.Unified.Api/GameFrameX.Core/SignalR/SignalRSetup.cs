namespace GameFrameX.Core.SignalR;

public static class SignalRSetup
{
    /// <summary>
    /// 即时消息 SignalR 注册（默认内存 backplane，不启用 Redis / Cluster 适配）
    /// </summary>
    /// <param name="services"></param>
    public static void AddProjectSignalR(this IServiceCollection services)
    {
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
            // 心跳改 15s：确保 WebSocket 帧频繁流动，避免被 Nginx/反向代理 60s 空闲超时切断
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.MaximumReceiveMessageSize = 1024 * 1024 * 10; // 数据包大小10M，默认最大为32K
        }).AddJsonProtocol(options => JsonSerializerOptionsProvider.Configure(options.PayloadSerializerOptions));
    }
}