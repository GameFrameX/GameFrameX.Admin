using System.Runtime.CompilerServices;
using GameFrameX.Core.Extension;

namespace GameFrameX.Core.Tests.Bootstrap;

/// <summary>
/// 测试进程紧凑雪花布局先行应用（2026-09-15 决策，C360 / GFX-803）：
/// 在任何测试（含并行测试类）触发 <c>SnowFlakeIdHelper.Instance</c> 之前应用布局，规避用例顺序耦合。
/// 生产路径由三 host 启动装配（HostBootstrapSnowflake.Init / AddSnowFlakeIdGenerator）应用，不经本类。
/// </summary>
internal static class CompactSnowflakeLayoutInitializer
{
    [ModuleInitializer]
    internal static void Apply()
    {
        SnowFlakeExtension.ApplyCompactLayout();
    }
}
