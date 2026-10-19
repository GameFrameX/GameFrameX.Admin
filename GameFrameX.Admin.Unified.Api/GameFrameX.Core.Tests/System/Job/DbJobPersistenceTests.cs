using GameFrameX.Core.Job;
using GameFrameX.Application.System.Job;

namespace GameFrameX.Core.Tests.System.Job;

public sealed class DbJobPersistenceTests
{
    [Theory]
    [InlineData(typeof(LogJob))]
    [InlineData(typeof(OnlineUserJob))]
    [InlineData(typeof(TenantExpirationNotifyJob))]
    public void DisableAnnotationTriggerRescan_KeepsScannedTriggersButDisablesSecondScan(Type jobType)
    {
        var schedulerBuilder = jobType.ScanToBuilder();
        var triggerCount = schedulerBuilder.GetEnumerable().Count();
        var jobBuilder = schedulerBuilder.GetJobBuilder().SetIncludeAnnotations(true);

        DbJobPersistence.DisableAnnotationTriggerRescan(jobBuilder);

        Assert.False(jobBuilder.IncludeAnnotations);
        Assert.Equal(triggerCount, schedulerBuilder.GetEnumerable().Count());
        Assert.True(triggerCount > 0);
    }
}