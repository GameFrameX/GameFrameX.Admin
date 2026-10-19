using GameFrameX.Core.Job;

namespace GameFrameX.Application.Game.Job;

/// <summary>
/// 游戏业务模块作业调度注册入口。
/// </summary>
public static class GameScheduleSetup
{
    /// <summary>
    /// 注册游戏业务模块依赖的调度服务。
    /// </summary>
    public static void AddGameSchedule(this IServiceCollection services)
    {
        services.AddSchedule();
    }
}