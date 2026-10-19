using GameFrameX.Core.Extension;

namespace GameFrameX.Core.Tests.Extension;

// 紧凑短 ID 布局（2026-09-15 决策，C360 / GFX-803）：Redis 租约机制已随 Redis/Cluster 退役（C170）
// 失去分布式底座而退役，GetAvailableWorkIds 用例随之删除；布局冻结与双实例唯一性守卫见
// GameFrameX.Core.Tests/Bootstrap/HostBootstrapSnowflakeCompactLayoutTests.cs。
public sealed class SnowFlakeExtensionTests
{
    [Fact]
    public void WorkerIdCount_Is8_CompactLayoutSingleDatacenterCap()
    {
        // 紧凑布局 WorkerId 3 位：0-7，共 8 个同库写入实例；超过 8 个实例无法互斥配置，启动期显式失败
        Assert.Equal(8, SnowFlakeExtension.WorkerIdCount);
    }
}
