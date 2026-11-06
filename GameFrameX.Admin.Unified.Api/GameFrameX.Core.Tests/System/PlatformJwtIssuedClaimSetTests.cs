using GameFrameX.Core.Cache;
using GameFrameX.Core.Const;
using GameFrameX.Core.Option;
using GameFrameX.Core.Service.PlatformJwt;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;
using Microsoft.IdentityModel.JsonWebTokens;
using SqlSugar;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// 平台 JWT 签发 claim 集合精确断言（C406 身份收口）：
/// 真实 <see cref="PlatformJwtTokenService"/> 签发的 AccessToken claim 全集必须恰好为
/// 身份四项（UserId/Account/RealName/AccountType）+ 签发框架项（iat/nbf/exp/iss/aud/platform_jwt_version），
/// 不得出现 TenantId、TenantHost、AppId、组织或租户角色等已删除作用域 claim。 // c406-keep（有意引用旧 claim 名作禁止说明）
/// </summary>
public sealed class PlatformJwtIssuedClaimSetTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"gfx_jwt_claim_set_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _client;

    public PlatformJwtIssuedClaimSetTests()
    {
        var config = new DbConnectionConfig
        {
            ConfigId = "main",
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
            IsAutoCloseConnection = true,
        };
        SqlSugarSetup.SetDbConfig(config);
        SqlSugarCache.Configure(new FakeCache());
        _client = new SqlSugarScope(new List<ConnectionConfig> { config, });
        _client.GetConnectionScope("main").CodeFirst.InitTables(typeof(SystemConfigEntity));
    }

    [Fact]
    public async Task CreateAccessTokenAsync_IssuedClaimSet_ExactlyIdentityAndFrameworkClaims()
    {
        // 与 SystemAuthService.CreateTokenAsync 相同的 payload 构造（身份四项）
        var payload = new Dictionary<string, object>
        {
            { ClaimConst.UserId, 1001L },
            { ClaimConst.Account, "operator" },
            { ClaimConst.RealName, "运营管理员" },
            { ClaimConst.AccountType, AccountTypeEnum.NormalUser },
        };

        var repository = new SqlSugarRepository<SystemConfigEntity>(_client);
        var runtimeProvider = new PlatformJwtRuntimeProvider(repository);
        await runtimeProvider.EnsureDefaultSettingsAsync();
        var tokenService = new PlatformJwtTokenService(runtimeProvider);

        var token = await tokenService.CreateAccessTokenAsync(payload, expiredMinutes: 20);

        var jwtToken = new JsonWebTokenHandler().ReadJsonWebToken(token);
        var claimTypes = jwtToken.Claims.Select(claim => claim.Type).Distinct().OrderBy(type => type, StringComparer.Ordinal).ToArray();

        Assert.Equal(
            new[]
            {
                "Account",
                "AccountType",
                "RealName",
                "UserId",
                "aud",
                "exp",
                "iat",
                "iss",
                "nbf",
                PlatformJwtConfigConstants.TokenVersionClaim,
            },
            claimTypes);
    }

    public void Dispose()
    {
        _client.Dispose();
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }
}
