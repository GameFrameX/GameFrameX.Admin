using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace GameFrameX.Core.Bootstrap;

/// <summary>
/// 静态 JWT Bearer 鉴权注册扩展（C261，GFX-668）：收敛 Hub Program.cs 内联的
/// TokenValidationParameters 手写装配（约 35 行）。Admin host 走动态平台 JWT（<c>AddPlatformJwt</c>），不适用本扩展。
/// </summary>
public static class HostBootstrapJwtExtensions
{
    /// <summary>
    /// 从配置节 <c>JWTSettings</c> 绑定 <see cref="HostJwtSettings"/> 并注册 JWT Bearer 鉴权与授权。
    /// <para>配置缺失时启动即抛异常（fail-fast）：静态 JWT host 必须显式配置，避免静默放行或全量 401。</para>
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <param name="configuration">host 配置，需含 JWTSettings 节（与 Admin 平台 JWT 配置保持一致）。</param>
    public static void AddHostJwtBearer(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JWTSettings").Get<HostJwtSettings>()
                          ?? throw new InvalidOperationException("未配置 JWTSettings，请在 appsettings.json 中配置与 Admin 一致的 JWTSettings 节点");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(bearerOptions =>
                {
                    bearerOptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.IssuerSigningKey)),
                        ValidateIssuer = jwtSettings.ValidateIssuer,
                        ValidIssuer = jwtSettings.ValidIssuer,
                        ValidateAudience = jwtSettings.ValidateAudience,
                        ValidAudience = jwtSettings.ValidAudience,
                        ValidateLifetime = jwtSettings.ValidateLifetime,
                        ClockSkew = TimeSpan.FromSeconds(jwtSettings.ClockSkew),
                    };
                });
        services.AddAuthorization();
    }
}