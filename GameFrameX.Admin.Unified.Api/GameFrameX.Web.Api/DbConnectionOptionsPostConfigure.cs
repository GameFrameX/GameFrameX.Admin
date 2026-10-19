using GameFrameX.Core.Option;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace GameFrameX.Web.Api;

/// <summary>
/// Options 管线的 <see cref="DbConnectionOptions"/> 后置配置（GFX-682）。
/// <para>环境变量覆盖（GFX_MAIN_DB_*）与默认租户库推导此前只在 SqlSugarSetup 的手工实例上执行；
/// 注入 <c>IOptions&lt;DbConnectionOptions&gt;</c> 的消费方（SystemTenantService 等）拿到的是
/// 未推导的空实例，<c>ConnectionConfigs</c> 为 null 导致 AddTenantAsync 抛 ArgumentNullException(source)。
/// 本类让 Options 管线实例走与 SqlSugarSetup 相同的覆盖/推导/校验管线。</para>
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