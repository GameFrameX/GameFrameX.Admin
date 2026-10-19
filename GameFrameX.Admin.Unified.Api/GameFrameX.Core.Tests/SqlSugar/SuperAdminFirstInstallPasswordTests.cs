using GameFrameX.Application.SeedData.System;
using GameFrameX.Core.Cache;
using GameFrameX.Core.Option;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.System;
using GameFrameX.Core.Utility;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.System;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

/// <summary>
/// superadmin 首装密码初始化守护（C322 随机打印机制的首装向导/env 注入替代）。
/// <para>锁定四项语义：①env 未设置时首装 seed 出 PasswordConfigured=false + 空密码（等待登录页向导）；
/// ②env 设置时 seed 出 Argon2id 哈希 + PasswordConfigured=true（无头部署路径，且两次构建哈希不同——随机盐）；
/// ③二次 InitDatabase 后密码与配置状态不被重置（insert-only 幂等）；④首装打印初始化指引且指引与路径匹配，二次不打印。</para>
/// <para>env 操作全部集中在本类（同类测试串行），避免与并行测试类互染进程级环境变量。</para>
/// </summary>
[Collection("MenuSeedCatalog")]
public class SuperAdminFirstInstallPasswordTests : IDisposable
{
    private const long SuperAdminUserId = 1300000000101;

    // GlobalMenu 表/种子必须随建：InitDatabase main 分支末尾 MenuSeedCatalog.LoadFromDb 固定查询 global_menu
    // （与 SeedInsertOnlyTests.MainSeedTypes 同构）
    private static readonly Type[] MainSeedTypes =
    {
        typeof(SystemUserEntity),
        typeof(SystemUserSeedData),
        typeof(GlobalMenuEntity),
        typeof(GlobalMenuSeedData),
    };

    private readonly List<string> _dbPaths = new();

    public SuperAdminFirstInstallPasswordTests()
    {
        // 种子 insert 触发二级缓存清理（IsAutoRemoveDataCache），静态运行期缓存须先就位
        SqlSugarCache.Configure(new FakeCache());
    }

    [Fact]
    public void InitDatabase_FirstInstallWithoutEnv_SuperAdminAwaitingWizard()
    {
        using (new TemporaryEnvironment(SystemUserSeedData.InitialPasswordEnvironmentVariable, null))
        using (var scope = CreateScope("main"))
        {
            var config = CreateConfig("main");

            SqlSugarSetup.InitDatabase(scope, config, MainSeedTypes);

            var mainDb = scope.GetConnectionScope("main");
            var superAdmin = mainDb.Queryable<SystemUserEntity>().First(u => u.Id == SuperAdminUserId);
            Assert.NotNull(superAdmin);
            // 向导路径：密码留空、未配置，等待 SetupSuperAdminPassword 端点初始化
            Assert.False(superAdmin.PasswordConfigured);
            Assert.Equal(string.Empty, superAdmin.Password);
        }
    }

    [Fact]
    public void HasData_EnvironmentVariableSet_SuperAdminConfiguredWithArgon2id()
    {
        using (new TemporaryEnvironment(SystemUserSeedData.InitialPasswordEnvironmentVariable, "Headless@2026"))
        {
            var firstUser = new SystemUserSeedData().HasData().First(u => u.Id == SuperAdminUserId);
            var secondUser = new SystemUserSeedData().HasData().First(u => u.Id == SuperAdminUserId);

            // env 注入路径：Argon2id 哈希 + 已配置，明文可验过；两次构建随机盐哈希必然不同
            Assert.True(firstUser.PasswordConfigured);
            Assert.StartsWith("$argon2id$", firstUser.Password);
            Assert.True(CryptogramUtility.VerifyPassword("Headless@2026", firstUser.Password));
            Assert.NotEqual(firstUser.Password, secondUser.Password);
        }
    }

    [Fact]
    public void InitDatabase_SecondRun_SuperAdminPasswordAndStateUnchanged()
    {
        using (new TemporaryEnvironment(SystemUserSeedData.InitialPasswordEnvironmentVariable, null))
        using (var scope = CreateScope("main"))
        {
            var config = CreateConfig("main");

            SqlSugarSetup.InitDatabase(scope, config, MainSeedTypes);

            // 模拟用户已通过向导设置密码：直接改库值 + 配置状态，二次初始化后不得被重置
            var mainDb = scope.GetConnectionScope("main");
            mainDb.Updateable<SystemUserEntity>()
                  .SetColumns(u => new SystemUserEntity { Password = "$argon2id$modified-by-user", PasswordConfigured = true })
                  .Where(u => u.Id == SuperAdminUserId)
                  .ExecuteCommand();

            SqlSugarSetup.InitDatabase(scope, config, MainSeedTypes);

            var secondSuperAdmin = mainDb.Queryable<SystemUserEntity>().First(u => u.Id == SuperAdminUserId);
            Assert.Equal("$argon2id$modified-by-user", secondSuperAdmin.Password);
            Assert.True(secondSuperAdmin.PasswordConfigured);
        }
    }

    [Fact]
    public void InitDatabase_FirstInstallPrintsWizardGuide_SecondRunPrintsNothing()
    {
        using (new TemporaryEnvironment(SystemUserSeedData.InitialPasswordEnvironmentVariable, null))
        using (var scope = CreateScope("main"))
        {
            var config = CreateConfig("main");

            var firstRunOutput = CaptureConsole(() => SqlSugarSetup.InitDatabase(scope, config, MainSeedTypes));

            // 首装打印：账号 + 向导指引（不再打印任何明文密码）
            Assert.Contains("superadmin", firstRunOutput);
            Assert.Contains("向导", firstRunOutput);
            Assert.DoesNotContain("初始密码（随机生成", firstRunOutput);

            // 二次初始化（重启语义）：行已存在不插入，不再打印任何初始化指引
            var secondRunOutput = CaptureConsole(() => SqlSugarSetup.InitDatabase(scope, config, MainSeedTypes));
            Assert.DoesNotContain("首次安装", secondRunOutput);
        }
    }

    [Fact]
    public void InitDatabase_FirstInstallWithEnv_PrintsEnvironmentGuidance()
    {
        using (new TemporaryEnvironment(SystemUserSeedData.InitialPasswordEnvironmentVariable, "Headless@2026"))
        using (var scope = CreateScope("main"))
        {
            var config = CreateConfig("main");

            var firstRunOutput = CaptureConsole(() => SqlSugarSetup.InitDatabase(scope, config, MainSeedTypes));

            // env 路径打印：指引密码来自环境变量，不回显明文
            Assert.Contains(SystemUserSeedData.InitialPasswordEnvironmentVariable, firstRunOutput);
            Assert.DoesNotContain("Headless@2026", firstRunOutput);
        }
    }

    private static string CaptureConsole(Action action)
    {
        var originalOutput = Console.Out;
        using (var stringWriter = new StringWriter())
        {
            Console.SetOut(stringWriter);
            try
            {
                action();
            }
            finally
            {
                Console.SetOut(originalOutput);
            }

            return stringWriter.ToString();
        }
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

    private SqlSugarScope CreateScope(params string[] configIds)
    {
        return new SqlSugarScope(configIds.Select(configId =>
        {
            var config = CreateConfig(configId);
            SqlSugarSetup.SetDbConfig(config);
            return (ConnectionConfig)config;
        }).ToList());
    }

    private DbConnectionConfig CreateConfig(string configId)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"gfx_admin_superadmin_seed_{configId}_{Guid.NewGuid():N}.db");
        _dbPaths.Add(dbPath);

        var config = new DbConnectionConfig
        {
            ConfigId = configId,
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={dbPath}",
            DbSettings = new DbSettings
            {
                EnableInitDb = true,
                EnableDiffLog = false,
            },
            TableSettings = new TableSettings
            {
                EnableInitTable = true,
                EnableIncreTable = false,
            },
            SeedSettings = new SeedSettings
            {
                EnableInitSeed = true,
                EnableIncreSeed = false,
            },
        };
        SqlSugarSetup.SetDbConfig(config);
        return config;
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
            if (_hadValue)
            {
                Environment.SetEnvironmentVariable(_name, _originalValue);
            }
            else
            {
                Environment.SetEnvironmentVariable(_name, null);
            }
        }
    }
}
