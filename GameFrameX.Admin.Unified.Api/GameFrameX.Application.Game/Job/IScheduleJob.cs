// Jobs/IScheduleJob.cs

using GameFrameX.Core.Job;

namespace GameFrameX.Application.Game.Job;

public interface IScheduleJob : IJob
{
    Task<Task> ExecuteAsync(CancellationToken cancellationToken);
}