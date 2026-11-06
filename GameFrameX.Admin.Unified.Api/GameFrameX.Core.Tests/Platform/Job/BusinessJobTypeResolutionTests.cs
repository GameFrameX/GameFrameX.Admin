using GameFrameX.Application.Platform.Job;
using GameFrameX.Core.Job;

namespace GameFrameX.Core.Tests.Platform.Job;

/// <summary>
/// 默认业务任务类型解析器回归测试：按 Name 解析 IScheduleJob 实现必须真实命中
/// （旧引擎用无程序集限定的 Type.GetType 解析，实测恒为 null——业务定时任务在合一前从未执行过）。
/// 与 GameTimeJobDispatchJobTests 共享静态钩子，同 Collection 禁止并行。
/// </summary>
[Collection(nameof(GameTimeJobDispatchJobTests))]
public sealed class BusinessJobTypeResolutionTests
{
    [Fact]
    public void DefaultResolver_ResolvesEmailTaskAsScheduleJob()
    {
        var resolved = GameTimeJobDispatchJob.BusinessJobTypeResolver("EmailTask");

        Assert.NotNull(resolved);
        Assert.Equal("GameFrameX.Application.Platform.Job.EmailTask", resolved.FullName);
        Assert.True(typeof(IScheduleJob).IsAssignableFrom(resolved));
    }

    [Fact]
    public void DefaultResolver_IgnoresNonScheduleJobTypes()
    {
        // GameHomeOnlineSampleJob 实现 IJob 而非 IScheduleJob：与旧引擎口径一致，不派发。
        Assert.Null(GameTimeJobDispatchJob.BusinessJobTypeResolver("GameHomeOnlineSampleJob"));
    }

    [Fact]
    public void DefaultResolver_UnknownName_ReturnsNull()
    {
        Assert.Null(GameTimeJobDispatchJob.BusinessJobTypeResolver("MissingJobName"));
    }
}
