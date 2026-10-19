using SqlSugar;

namespace GameFrameX.Core.Tests.Bootstrap;

/// <summary>
/// SqlSugar 雪花 ID 进程级接线守卫（C361 / GFX-804）。
/// <para>SqlSugar 主键回填取号必须经 <see cref="StaticConfig.CustomSnowFlakeFunc"/> 走 Foundation 紧凑布局——
/// 该 func 由 Core.Extension 的 SqlSugarSnowflakeFallbackInitializer（ModuleInitializer）在模块加载时安装，
/// 覆盖 Hub / Client（AddSqlSugarClientService 不装配 SqlSugarSetup）与一切未装配路径。
/// 为空即 SqlSugar 退回内置 SnowFlakeSingle 18-19 位长布局，是 C361 要杜绝的退化路径。</para>
/// </summary>
public sealed class SqlSugarSnowflakeFallbackWireTests
{
    private const long JsSafeIntegerLimit = 1L << 53;

    [Fact]
    public void CustomSnowFlakeFunc_InstalledProcessWide()
    {
        // ModuleInitializer 在 Core.Extension 模块加载时安装；本测试进程经
        // CompactSnowflakeLayoutInitializer（同为 ModuleInitializer，其 body 触发 Core.Extension 加载）
        // 已加载 Core.Extension——此处断言生产接线在无 AddSqlSugar 装配的进程同样生效。
        Assert.NotNull(StaticConfig.CustomSnowFlakeFunc);
    }

    [Fact]
    public void CustomSnowFlakeFunc_ProducesCompactUniqueIds()
    {
        var snowFlakeFunc = StaticConfig.CustomSnowFlakeFunc!;

        var ids = new List<long>(1000);
        for (var i = 0; i < 1000; i++)
        {
            var id = snowFlakeFunc();
            // 紧凑量级：13-14 位数字（epoch 2026-09-01）；18-19 位即兜底未生效退内置长布局
            Assert.True(id >= 1_000_000_000_000L && id < 100_000_000_000_000L, $"Id 不在 13-14 位量级：{id}");
            // 2^53 JS 安全域（决策核心目标：Admin 页面主键展示不丢精度）
            Assert.True(id < JsSafeIntegerLimit, $"Id 超出 2^53 JS 安全域：{id}");
            ids.Add(id);
        }

        // 基础唯一性：func 指向 Foundation 单例（已初始化复用 / 未初始化兜底创建），连续取号不得重复
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }
}
