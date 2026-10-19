namespace GameFrameX.Application.System.Job;

/// <summary>
/// Admin Host 作业调度注册入口。
/// </summary>
public static class AdminScheduleSetup
{
    /// <summary>
    /// 注册 Admin 作业调度；启用时接入数据库持久化，禁用时仅保留调度服务依赖占位，避免其它服务解析失败。
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <param name="enablePersistence">是否启用数据库持久化和实际调度。</param>
    public static void AddAdminSchedule(this IServiceCollection services, bool enablePersistence)
    {
        if (enablePersistence)
        {
            services.AddSchedule(options => { options.AddPersistence<DbJobPersistence>(); });
            return;
        }

        services.AddSchedule();
    }
}