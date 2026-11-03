using GameFrameX.Core.Option;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace GameFrameX.Web.Api;

/// <summary>
/// 让 Options 管线与手工初始化使用同一套主库环境变量覆盖和连接配置校验。
/// </summary>
public sealed class DbConnectionOptionsPostConfigure : IPostConfigureOptions<DbConnectionOptions>
{
    private readonly IConfiguration _configuration;

    public DbConnectionOptionsPostConfigure(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void PostConfigure(string? name, DbConnectionOptions options)
    {
        options.PostConfigure(options, _configuration);
    }
}