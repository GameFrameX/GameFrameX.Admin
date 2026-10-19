using GameFrameX.Core.Cache;
using GameFrameX.Core.Enum;
using GameFrameX.Core.Option;
using GameFrameX.Core.Service.Auth;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Config;
using GameFrameX.Core.Service.User;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Utility;
using GameFrameX.Entity;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// SystemAuthService 端到端测试宿主（C217）：真实 SqlSugar 主库（superadmin seed）+ 租户库（tenant_user seed）+
/// 手写 fake（3 接口）+ CurrentUserContext 真实 + SystemConfigService 真实（FakeCache）+ ICaptcha/IServiceProvider null!（captcha IsEnabled=false 路径不调用）。
/// <para>C218 扩展：主库 + 租户库双连接 scope（configId=main/1001），FakeTenantDbConnectionProvider 返回租户 scope；</para>
/// <para>DefaultHttpContext 供 CreateTokenAsync 写响应头；FakePlatformJwtTokenService 捕获 payload 供 Claim 校验。</para>
/// </summary>
internal sealed class AuthTestHost : IDisposable
{
    private const string MainConfigId = "main";
    private const string TenantConfigId = "1001";
    private const long TenantUserId = 2001L;

    /// <summary>
    /// 租户库 seed 用的租户 ID（与 <see cref="TenantConfigId"/> 对应）。
    /// </summary>
    public const long SeedTenantId = 1001L;

    private readonly SqlSugarScope _client;
    private readonly List<string> _dbPaths = new();

    public AuthTestHost()
    {
        SqlSugarCache.Configure(new FakeCache());
        var mainConfig = CreateConfig(MainConfigId);
        var tenantConfig = CreateConfig(TenantConfigId);
        _client = new SqlSugarScope(new List<ConnectionConfig> { mainConfig, tenantConfig, });
        SqlSugarSetup.InitDatabase(_client, mainConfig, new[]
        {
            typeof(SystemUserEntity),
            typeof(SystemConfigEntity),
            typeof(SystemTenantEntity),
        });
        SqlSugarSetup.InitDatabase(_client, tenantConfig, new[]
        {
            typeof(TenantUserEntity),
            typeof(TenantOrganizationEntity),
        });
    }

    /// <summary>
    /// Fake JWT 服务实例；暴露捕获的 payload 供场景 ③ Claim 校验。
    /// </summary>
    public FakePlatformJwtTokenService Jwt { get; } = new();

    /// <summary>
    /// BuildService 使用的 HTTP 上下文；setup 限流等按 Connection.RemoteIpAddress 取客户端键，
    /// 测试可按需覆写以隔离进程级静态限流字典。
    /// </summary>
    public DefaultHttpContext HttpContext { get; } = new();

    public SystemAuthService BuildService()
    {
        var httpContextAccessor = new HttpContextAccessor { HttpContext = HttpContext, };
#pragma warning disable CS8604 // SystemUserEntity/SystemConfigEntity 是 platform table，构造不实际使用 tenantDbConnectionProvider；ICaptcha 在 captcha IsEnabled=false 路径不调用
        var systemUserRepository = new SqlSugarRepository<SystemUserEntity>(_client, null!);
        var systemConfigRepository = new SqlSugarRepository<SystemConfigEntity>(_client, null!);
        var captcha = (Lazy.Captcha.Core.ICaptcha)null!;
#pragma warning restore CS8604
        var systemCacheService = new SystemCacheService(new FakeCache(), Options.Create(new CacheOptions()));
        var systemConfigService = new SystemConfigService(systemConfigRepository, systemCacheService);
        var currentUserContext = new CurrentUserContext(httpContextAccessor);
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        return new SystemAuthService(
            currentUserContext,
            systemUserRepository,
            httpContextAccessor,
            serviceProvider,
            systemConfigService,
            captcha,
            new FakeTenantDbConnectionProvider(_client, TenantConfigId),
            Jwt,
            new FakePlatformCaptchaConfigProvider());
    }

    public async Task SeedSuperAdminAsync(string account, string password, bool? isEnabled = null, bool passwordConfigured = true)
    {
        var user = BuildUser<SystemUserEntity>(1, account, password, AccountTypeEnum.SuperAdmin);
        if (isEnabled.HasValue)
        {
            user.IsEnabled = isEnabled.Value;
        }

        // PasswordConfigured 是严格 bool：CLR 默认 false=「未配置」，须双向显式赋值，
        // 不能依赖 nullable 时代「默认 null=已配置」的隐式语义
        user.PasswordConfigured = passwordConfigured;
        if (!passwordConfigured)
        {
            user.Password = string.Empty;
        }

        var mainDb = _client.GetConnectionScope(MainConfigId);
        await mainDb.Insertable(user).ExecuteCommandAsync();
    }

    /// <summary>
    /// 查询主库超管行（AccountType=SuperAdmin），供 setup/login 守护测试断言落库结果。
    /// </summary>
    public async Task<SystemUserEntity?> GetSuperAdminAsync()
    {
        var mainDb = _client.GetConnectionScope(MainConfigId);
        return await mainDb.Queryable<SystemUserEntity>().FirstAsync(u => u.AccountType == AccountTypeEnum.SuperAdmin);
    }

    /// <summary>
    /// 在主库 seed 一条 <see cref="SystemTenantEntity"/>（场景 ⑤ 用 IsEnabled=false 触发 Z1003）。
    /// </summary>
    public async Task SeedTenantAsync(long tenantId, bool isEnabled)
    {
        var mainDb = _client.GetConnectionScope(MainConfigId);
        await mainDb.Insertable(new SystemTenantEntity
        {
            Id = tenantId,
            TenantType = TenantTypeEnum.Db,
            DbType = DbType.Sqlite,
            Connection = string.Empty,
            ConfigId = tenantId.ToString(),
            IsEnabled = isEnabled,
        }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 在租户库 seed 一条 <see cref="TenantUserEntity"/>（AccountType=NormalUser，IsEnabled=true，以通过冻结检查进入租户禁用分支）。
    /// </summary>
    public async Task SeedTenantUserAsync(long tenantId, string account, string password)
    {
        var user = BuildUser<TenantUserEntity>(TenantUserId, account, password, AccountTypeEnum.NormalUser);
        user.TenantId = tenantId;
        user.IsEnabled = true;

        var tenantDb = _client.GetConnectionScope(TenantConfigId);
        await tenantDb.Insertable(user).ExecuteCommandAsync();
    }

    public void Dispose()
    {
        foreach (var dbPath in _dbPaths)
        {
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
    }

    /// <summary>
    /// 填充 <see cref="SystemUserBase"/> 共享字段（system_user / tenant_user 通用），调用方再按需覆盖 Id/TenantId/IsEnabled/AccountType。
    /// </summary>
    private static T BuildUser<T>(long id, string account, string password, AccountTypeEnum accountType) where T : SystemUserBase, new()
    {
        return new T
        {
            Id = id,
            Account = account,
            Password = CryptogramUtility.HashPassword(password),
            AccountType = accountType,
            RealName = account,
            NickName = account,
            Avatar = string.Empty,
            Nation = string.Empty,
            Phone = string.Empty,
            IdCardNum = string.Empty,
            Email = string.Empty,
            Address = string.Empty,
            PoliticalOutlook = string.Empty,
            College = string.Empty,
            OfficePhone = string.Empty,
            EmergencyContact = string.Empty,
            EmergencyPhone = string.Empty,
            EmergencyAddress = string.Empty,
            Introduction = string.Empty,
            Remark = string.Empty,
            JobNum = string.Empty,
            PosLevel = string.Empty,
            PosTitle = string.Empty,
            Expertise = string.Empty,
            OfficeZone = string.Empty,
            Office = string.Empty,
            LastLoginIp = string.Empty,
            LastLoginAddress = string.Empty,
            LastLoginDevice = string.Empty,
            Signature = string.Empty,
        };
    }

    private DbConnectionConfig CreateConfig(string configId)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"gfx_auth_{configId}_{Guid.NewGuid():N}.db");
        _dbPaths.Add(dbPath);
        var config = new DbConnectionConfig
        {
            ConfigId = configId,
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={dbPath}",
            IsAutoCloseConnection = true,
            DbSettings = new DbSettings { EnableInitDb = true, },
            TableSettings = new TableSettings { EnableInitTable = true, EnableIncreTable = false, },
            SeedSettings = new SeedSettings { EnableInitSeed = false, EnableIncreSeed = false, },
        };
        SqlSugarSetup.SetDbConfig(config);
        return config;
    }
}