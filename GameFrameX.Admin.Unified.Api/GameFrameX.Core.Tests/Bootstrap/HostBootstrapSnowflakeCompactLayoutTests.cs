using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.Extension;
using GameFrameX.Foundation.Utility.DistributedSystem.Snowflake;
using Microsoft.Extensions.Configuration;

namespace GameFrameX.Core.Tests.Bootstrap;

/// <summary>
/// 紧凑短 ID 布局冻结守卫（2026-09-15 决策，C360 / GFX-803）。
/// <para>epoch / 位宽一旦有数据落库即不可更改：改一次 = 与历史 Id 空间重叠。
/// 布局先经 <see cref="CompactSnowflakeLayoutInitializer"/>（ModuleInitializer）应用于测试进程。</para>
/// </summary>
public sealed class HostBootstrapSnowflakeCompactLayoutTests
{
    private const long JsSafeIntegerLimit = 1L << 53;

    [Fact]
    public void LayoutConstants_FrozenByDecision()
    {
        // 决策冻结值（gfx-doc/decisions/2026-09-15-compact-snowflake-id.md）：
        // epoch 2026-09-01T00:00:00Z = 1788220800000；Worker 3 位（0-7）；DataCenter 0 位；序列 10 位。
        // 改动任一常量前必须先推翻该决策（存量 Id 空间重叠 = 数据完整性事故）。
        Assert.Equal(1788220800000L, SnowFlakeExtension.CompactEpochMilliseconds);
        Assert.Equal(3, SnowFlakeExtension.CompactWorkerIdBits);
        Assert.Equal(0, SnowFlakeExtension.CompactDatacenterIdBits);
        Assert.Equal(10, SnowFlakeExtension.CompactSequenceBits);
        Assert.Equal(8, SnowFlakeExtension.WorkerIdCount);
    }

    [Fact]
    public void SingletonIds_CompactRange_Unique_Monotonic()
    {
        // 单例经 ModuleInitializer 已应用紧凑布局；此处首次（或后续）访问触发生成。
        var ids = new List<long>(2000);
        for (var i = 0; i < 2000; i++)
        {
            ids.Add(SnowFlakeIdHelper.Instance.NextId());
        }

        Assert.All(ids, id => Assert.True(id > 0, $"Id 应为正数：{id}"));
        // 2^53 JS 安全域（决策核心目标：Admin 页面主键展示不丢精度）
        Assert.All(ids, id => Assert.True(id < JsSafeIntegerLimit, $"Id 超出 2^53 JS 安全域：{id}"));
        // 紧凑量级：当前 13-14 位数字（epoch 2026-09-01，2030 年前 ≤15 位）
        Assert.All(ids, id => Assert.True(id >= 1_000_000_000_000L && id < 100_000_000_000_000L, $"Id 不在 13-14 位量级：{id}"));
        // 同进程内严格单调（时间戳左移占高位 + 序列低位自增）
        for (var i = 1; i < ids.Count; i++)
        {
            Assert.True(ids[i] > ids[i - 1], $"Id 非单调递增：{ids[i - 1]} -> {ids[i]}");
        }

        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void DistinctWorkerIds_DisjointIdSpaces()
    {
        // 双实例同库写的唯一性断言：紧凑布局下实例区分仅 Worker 位——
        // 两个不同 WorkerId 的生成器（同 epoch / 位宽）产出集合必须完全不相交。
        var instanceA = new IdWorker(2, 0, SnowFlakeExtension.CompactEpochMilliseconds, 0, SnowFlakeExtension.CompactWorkerIdBits, SnowFlakeExtension.CompactDatacenterIdBits, SnowFlakeExtension.CompactSequenceBits);
        var instanceB = new IdWorker(3, 0, SnowFlakeExtension.CompactEpochMilliseconds, 0, SnowFlakeExtension.CompactWorkerIdBits, SnowFlakeExtension.CompactDatacenterIdBits, SnowFlakeExtension.CompactSequenceBits);

        var idsA = new HashSet<long>();
        var idsB = new HashSet<long>();
        for (var i = 0; i < 2000; i++)
        {
            idsA.Add(instanceA.NextId());
            idsB.Add(instanceB.NextId());
        }

        Assert.Equal(2000, idsA.Count);
        Assert.Equal(2000, idsB.Count);
        Assert.Empty(idsA.Intersect(idsB));

        // Worker 位段编码正确：序列 10 位之上 3 位为 WorkerId
        Assert.All(idsA, id => Assert.Equal(2, (int)((id >> 10) & 7)));
        Assert.All(idsB, id => Assert.Equal(3, (int)((id >> 10) & 7)));
    }

    [Fact]
    public void Id_ComposedOfTimestampWorkerSequence_OnlyWorkerDiscriminatesInstances()
    {
        // 防撞必要性断言（布局恒等式）：Id 可由 (时间戳, WorkerId, 序列) 完全重建，
        // 即除 Worker 位外无任何实例区分维度——WorkerId 相同的两个实例在同 (ts, seq) 坐标上必然产出相同 Id，
        // 故同库写入实例必须互斥配置 WorkerId（GFX-803 的存在理由）。
        var instance = new IdWorker(5, 0, SnowFlakeExtension.CompactEpochMilliseconds, 0, SnowFlakeExtension.CompactWorkerIdBits, SnowFlakeExtension.CompactDatacenterIdBits, SnowFlakeExtension.CompactSequenceBits);
        var id = instance.NextId();

        var timestamp = id >> 13;
        var sequence = id & 1023;
        var recomposed = (timestamp << 13) | (5L << 10) | sequence;

        Assert.Equal(id, recomposed);
    }

    [Fact]
    public void ApplyCompactLayout_AfterInstanceCreated_Throws()
    {
        // 数据完整性防御：单例创建后再应用布局将静默失效（statics 赋值不影响已建实例），
        // 本进程会以旧布局生成 18 位长 Id 并与紧凑布局进程的 Id 空间重叠，必须显式失败。
        _ = SnowFlakeIdHelper.Instance;

        Assert.Throws<InvalidOperationException>(() => SnowFlakeExtension.ApplyCompactLayout());
    }

    [Fact]
    public void SeedIds_UniqueWithinValueDomain()
    {
        // ponytail: 真实全量菜单/角色菜单种子键来自 DB（MenuSeedCatalog.LoadFromDb 重建），单测不接库；
        // 此处用 1 万合成键（≥ 真实规模 ~8800 键）验证 46 位掩码同量级唯一性与值域。
        var seen = new HashSet<long>();
        for (var i = 0; i < 10_000; i++)
        {
            var menuId = StableSeedId.Menu($"route:guard:{i}");
            var roleMenuId = StableSeedId.RoleMenu($"admin", $"route:guard:{i}");

            Assert.True(menuId > 0 && menuId < (1L << 46), $"菜单种子 Id 超出 46 位值域：{menuId}");
            Assert.True(roleMenuId > 0 && roleMenuId < (1L << 46), $"角色菜单种子 Id 超出 46 位值域：{roleMenuId}");
            Assert.True(seen.Add(menuId), $"菜单种子 Id 碰撞：{menuId}");
            Assert.True(seen.Add(roleMenuId), $"角色菜单种子 Id 碰撞：{roleMenuId}");
        }
    }

    [Theory]
    [InlineData("0", 9, 0)]        // 配置命中：显式 0 覆盖兜底
    [InlineData("7", 9, 7)]        // 配置命中：上边界
    [InlineData(null, 2, 2)]       // 缺省：回落 host 兜底
    [InlineData("", 3, 3)]         // 空串：视为未配置回落兜底
    [InlineData("   ", 3, 3)]      // 纯空白：视为未配置回落兜底
    public void ResolveWorkerId_ConfigurationOverridesFallback(string? configured, int fallback, int expected)
    {
        var builder = new ConfigurationBuilder();
        if (configured is not null)
        {
            builder.AddInMemoryCollection(new Dictionary<string, string?> { ["SnowFlake:WorkerId"] = configured });
        }

        Assert.Equal(expected, SnowFlakeExtension.ResolveWorkerId(builder.Build(), fallback));
    }

    [Theory]
    [InlineData("abc")]   // 非数字：配置错误
    [InlineData("-1")]    // 负号：NumberStyles.None 不容忍符号
    [InlineData(" 5")]    // 带空白：环境变量注入须为纯数字
    [InlineData("1.0")]   // 小数点：非整数
    public void ResolveWorkerId_MalformedValue_FailsFastInsteadOfSilentFallback(string configured)
    {
        // 配置错误若静默回退兜底值，会以「看似正常」的 WorkerId 与其它同库实例撞库——必须显式失败。
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["SnowFlake:WorkerId"] = configured })
            .Build();

        Assert.Throws<InvalidOperationException>(() => SnowFlakeExtension.ResolveWorkerId(config, 2));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void ValidateWorkerId_AcceptsCompactRange(int workerId)
    {
        SnowFlakeExtension.ValidateWorkerId(workerId);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(8)]
    [InlineData(31)]
    [InlineData(int.MaxValue)]
    public void ValidateWorkerId_RejectsOutOfRange_FailsFast(int workerId)
    {
        // 超过 8 个同库实例无法互斥配置 → 启动期显式失败（fail fast），不允许带病运行。
        var exception = Assert.Throws<InvalidOperationException>(() => SnowFlakeExtension.ValidateWorkerId(workerId));
        Assert.Contains("SnowFlake WorkerId", exception.Message, StringComparison.Ordinal);
        Assert.Contains("0", exception.Message, StringComparison.Ordinal);
        Assert.Contains("7", exception.Message, StringComparison.Ordinal);
    }
}
