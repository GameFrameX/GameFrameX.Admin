using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GameFrameX.Core.Job;

public sealed class ProjectScheduleOptions
{
    private readonly List<Type> _persistenceTypes = new();

    public void AddPersistence<TPersistence>() where TPersistence : class, IJobPersistence
    {
        // 同一持久化类型只登记一次：AddSchedule 双入口重复调用时不产生重复实例（否则 Preload/写入翻倍）。
        if (!_persistenceTypes.Contains(typeof(TPersistence)))
        {
            _persistenceTypes.Add(typeof(TPersistence));
        }
    }

    internal void Register(IServiceCollection services)
    {
        foreach (var persistenceType in _persistenceTypes)
        {
            services.TryAddSingleton(typeof(IJobPersistence), persistenceType);
        }
    }
}