namespace GameFrameX.Core.Job;

public sealed class ProjectScheduleOptions
{
    private readonly List<Type> _persistenceTypes = new();

    public void AddPersistence<TPersistence>() where TPersistence : class, IJobPersistence
    {
        _persistenceTypes.Add(typeof(TPersistence));
    }

    internal void Register(IServiceCollection services)
    {
        foreach (var persistenceType in _persistenceTypes)
        {
            services.AddSingleton(typeof(IJobPersistence), persistenceType);
        }
    }
}