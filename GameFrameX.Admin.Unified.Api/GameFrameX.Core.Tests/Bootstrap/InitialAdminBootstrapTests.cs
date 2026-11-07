using GameFrameX.Application.SeedData.System;
using GameFrameX.Core.Bootstrap;
using GameFrameX.Core.Cache;
using GameFrameX.Core.Const;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.Option;
using GameFrameX.Core.Tests.System;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Utility;
using GameFrameX.Entity.Business;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.System;
using SqlSugar;

namespace GameFrameX.Core.Tests.Bootstrap;

/// <summary>
/// 新装首个业务管理员种子引导测试：
/// ①新库 + 环境变量：创建 1 个账号绑定 sys_admin，角色菜单 = 全部非超管种子菜单（超管专属子树 0），
/// 密码 Argon2id 可验；②二次启动幂等：账号唯一、密码不覆盖、被删的种子授权补回、手工收窄的额外授权不删；
/// ③不设变量：不建账号并输出警告；④弱密码：fail-closed 启动报错且错误信息不含密码。
/// <para>env 操作与密码策略静态属性均进程级，集中本类并挂 MenuSeedCatalog 串行集合避免互染。</para>
/// </summary>
[Collection("MenuSeedCatalog")]
public sealed class InitialAdminBootstrapTests : IDisposable
{
    private const string InitialAccount = "first-admin";
    private const string InitialPassword = "{{SECRET_p5xv2w8n}}";

    private static readonly Type[] MainSeedTypes =
    {
        typeof(SystemUserEntity),
        typeof(SystemUserSeedData),
        typeof(GlobalMenuEntity),
        typeof(GlobalMenuSeedData),
        typeof(UserEntity),
        typeof(RoleEntity),
        typeof(RoleMenuEntity),
        typeof(UserRoleEntity),
    };

    private readonly List<string> _dbPaths = new();

    public InitialAdminBootstrapTests()
    {
        SqlSugarCache.Configure(new FakeCache());
    }

    private DbConnectionConfig _config = null!;

    private SqlSugarScope CreateScope()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"gfx_admin_initial_admin_{Guid.NewGuid():N}.db");
        _dbPaths.Add(dbPath);
        var config = new DbConnectionConfig
        {
            ConfigId = "main",
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={dbPath}",
            DbSettings = new DbSettings { EnableInitDb = true, EnableDiffLog = false, },
            TableSettings = new TableSettings { EnableInitTable = true, EnableIncreTable = false, },
            SeedSettings = new SeedSettings { EnableInitSeed = true, EnableIncreSeed = false, },
        };
        SqlSugarSetup.SetDbConfig(config);
        _config = config;
        return new SqlSugarScope(new List<ConnectionConfig> { config });
    }

    private static HashSet<long> ExpectedGrantableMenuIds()
    {
        var reservedMenuIds = MenuSeedCatalog.SuperAdminOnlySubtreeMenuIds;
        var readableMenuIds = MenuSeedCatalog.AdminReadableSuperAdminOnlyMenuIds;
        return MenuSeedCatalog.AllDefinitions
                              .Select(u => StableSeedId.Menu(u.Key))
                              .Where(id => !reservedMenuIds.Contains(id) || readableMenuIds.Contains(id))
                              .ToHashSet();
    }

    [Fact]
    public async Task FirstInstallWithEnv_CreatesSingleAdminWithNonSuperAdminMenus()
    {
        using (new TemporaryEnvironment(InitialAdminBootstrap.InitialAccountEnvironmentVariable, InitialAccount))
        using (new TemporaryEnvironment(InitialAdminBootstrap.InitialPasswordEnvironmentVariable, InitialPassword))
        using (var scope = CreateScope())
        {
            SqlSugarSetup.InitDatabase(scope, _config, MainSeedTypes);
            await InitialAdminBootstrap.RunAsync(scope);

            var db = scope.GetConnectionScope("main");
            var user = Assert.Single(db.Queryable<UserEntity>().Filter(null, true).ToList());
            Assert.Equal(InitialAccount, user.Account);
            Assert.True(CryptogramUtility.VerifyPassword(InitialPassword, user.Password));
            Assert.Equal(AccountTypeEnum.SystemAdmin, user.AccountType);
            Assert.True(user.IsEnabled);

            var role = db.Queryable<RoleEntity>().First(u => u.Code == CommonConst.SysAdminRole);
            Assert.NotNull(role);
            Assert.Contains(db.Queryable<UserRoleEntity>().ToList(), binding => binding.UserId == user.Id && binding.RoleId == role.Id);

            var grantedMenuIds = db.Queryable<RoleMenuEntity>().Where(u => u.RoleId == role.Id).Select(u => u.MenuId).ToList();
            Assert.Equal(ExpectedGrantableMenuIds(), grantedMenuIds.ToHashSet());
            Assert.Empty(grantedMenuIds.Where(MenuSeedCatalog.SuperAdminOnlySubtreeMenuIds.Contains)
                                       .Except(MenuSeedCatalog.AdminReadableSuperAdminOnlyMenuIds));
        }
    }

    [Fact]
    public async Task SecondRun_IdempotentAndOnlyAddsMissingSeedMenus()
    {
        using (new TemporaryEnvironment(InitialAdminBootstrap.InitialAccountEnvironmentVariable, InitialAccount))
        using (new TemporaryEnvironment(InitialAdminBootstrap.InitialPasswordEnvironmentVariable, InitialPassword))
        using (var scope = CreateScope())
        {
            SqlSugarSetup.InitDatabase(scope, _config, MainSeedTypes);
            await InitialAdminBootstrap.RunAsync(scope);

            var db = scope.GetConnectionScope("main");
            var firstUser = Assert.Single(db.Queryable<UserEntity>().Filter(null, true).ToList());
            var role = db.Queryable<RoleEntity>().First(u => u.Code == CommonConst.SysAdminRole);

            // 模拟运营手工收窄（删一条种子授权）+ 手工额外授权（非种子菜单）
            db.Deleteable<RoleMenuEntity>(u => u.RoleId == role.Id && u.MenuId == ExpectedGrantableMenuIds().First()).ExecuteCommand();
            db.Insertable(new RoleMenuEntity { RoleId = role.Id, MenuId = 987654321L, IsEnabled = true }).ExecuteCommand();

            await InitialAdminBootstrap.RunAsync(scope);

            var secondUser = Assert.Single(db.Queryable<UserEntity>().Filter(null, true).ToList());
            Assert.Equal(firstUser.Id, secondUser.Id);
            Assert.Equal(firstUser.Password, secondUser.Password);

            var grantedMenuIds = db.Queryable<RoleMenuEntity>().Where(u => u.RoleId == role.Id).Select(u => u.MenuId).ToList();
            // 被删的种子授权补回；手工额外授权保留（只补不删）
            Assert.Superset(ExpectedGrantableMenuIds(), grantedMenuIds.ToHashSet());
            Assert.Contains(987654321L, grantedMenuIds);
            Assert.Single(db.Queryable<UserEntity>().Filter(null, true).ToList());
        }
    }

    [Fact]
    public async Task WithoutEnv_CreatesNoAccountAndWarns()
    {
        using (new TemporaryEnvironment(InitialAdminBootstrap.InitialAccountEnvironmentVariable, null))
        using (new TemporaryEnvironment(InitialAdminBootstrap.InitialPasswordEnvironmentVariable, null))
        using (var scope = CreateScope())
        {
            SqlSugarSetup.InitDatabase(scope, _config, MainSeedTypes);
            var output = string.Empty;
            output = await CaptureConsoleAsync(() => InitialAdminBootstrap.RunAsync(scope));

            var db = scope.GetConnectionScope("main");
            Assert.Empty(db.Queryable<UserEntity>().Filter(null, true).ToList());
            // 角色与授权仍会建立（首装即有管理员角色可分配），仅账号不创建
            Assert.NotNull(db.Queryable<RoleEntity>().First(u => u.Code == CommonConst.SysAdminRole));
            Assert.Contains("尚未初始化业务管理员", output);
            Assert.Contains(InitialAdminBootstrap.InitialAccountEnvironmentVariable, output);
        }
    }

    [Fact]
    public async Task PartialEnv_WarnsAndCreatesNoAccount()
    {
        using (new TemporaryEnvironment(InitialAdminBootstrap.InitialAccountEnvironmentVariable, InitialAccount))
        using (new TemporaryEnvironment(InitialAdminBootstrap.InitialPasswordEnvironmentVariable, null))
        using (var scope = CreateScope())
        {
            SqlSugarSetup.InitDatabase(scope, _config, MainSeedTypes);
            var output = await CaptureConsoleAsync(() => InitialAdminBootstrap.RunAsync(scope));

            var db = scope.GetConnectionScope("main");
            Assert.Empty(db.Queryable<UserEntity>().Filter(null, true).ToList());
            Assert.Contains("必须成对设置", output);
        }
    }

    [Fact]
    public async Task WeakPassword_FailsClosedWithoutLeakingPassword()
    {
        using (new TemporaryEnvironment(InitialAdminBootstrap.InitialAccountEnvironmentVariable, InitialAccount))
        using (new TemporaryEnvironment(InitialAdminBootstrap.InitialPasswordEnvironmentVariable, "weak"))
        using (var scope = CreateScope())
        {
            SqlSugarSetup.InitDatabase(scope, _config, MainSeedTypes);

            CryptogramUtility.ApplyPasswordPolicy(true, "^(?=.*[A-Z]).{8,}$", "密码须至少 8 位且含大写字母");
            try
            {
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => InitialAdminBootstrap.RunAsync(scope));
                Assert.Contains(InitialAdminBootstrap.InitialPasswordEnvironmentVariable, exception.Message);
                Assert.DoesNotContain("weak", exception.Message);
            }
            finally
            {
                CryptogramUtility.ApplyPasswordPolicy(false, CryptogramUtility.PasswordStrengthValidation, CryptogramUtility.PasswordStrengthValidationMsg);
            }

            var db = scope.GetConnectionScope("main");
            Assert.Empty(db.Queryable<UserEntity>().Filter(null, true).ToList());
        }
    }

    private static async Task<string> CaptureConsoleAsync(Func<Task> action)
    {
        var originalOutput = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        try
        {
            await action();
        }
        finally
        {
            Console.SetOut(originalOutput);
        }

        return stringWriter.ToString();
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
    /// 测试作用域环境变量：构造时设置（value 为 null 时清除），Dispose 时恢复原值。
    /// </summary>
    private sealed class TemporaryEnvironment : IDisposable
    {
        private readonly string _name;
        private readonly string? _originalValue;
        private readonly bool _hadValue;

        public TemporaryEnvironment(string name, string? value)
        {
            _name = name;
            _originalValue = Environment.GetEnvironmentVariable(name);
            _hadValue = _originalValue != null;
            Environment.SetEnvironmentVariable(name, value);
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable(_name, _hadValue ? _originalValue : null);
        }
    }
}
