using GameFrameX.Entity.System;

namespace GameFrameX.Core.Job;

public sealed class JobBuilder
{
    private readonly Type? _jobType;
    private readonly JobDetail _detail = new();

    private JobBuilder(Type? jobType)
    {
        _jobType = jobType;
        if (jobType == null)
        {
            return;
        }

        var attribute = jobType.GetCustomAttributes(typeof(JobDetailAttribute), true).OfType<JobDetailAttribute>().FirstOrDefault();
        _detail.JobId = attribute?.JobId ?? jobType.Name;
        _detail.GroupName = attribute?.GroupName ?? "default";
        _detail.Description = attribute?.Description;
        _detail.Concurrent = attribute?.Concurrent ?? true;
        SetJobType(jobType);
    }

    public string? JobId
    {
        get { return _detail.JobId; }
    }

    public JobDetail Detail
    {
        get { return _detail; }
    }

    public Type? JobType
    {
        get { return _jobType ?? ResolveType(_detail.AssemblyName, _detail.JobType); }
    }

    public bool IncludeAnnotations
    {
        get { return _detail.IncludeAnnotations; }
    }

    public static JobBuilder Create(Type jobType)
    {
        return new JobBuilder(jobType);
    }

    public JobBuilder LoadFrom(SystemJobDetailEntity source)
    {
        _detail.JobId = source.JobId;
        _detail.GroupName = source.GroupName;
        _detail.JobType = source.JobType;
        _detail.AssemblyName = source.AssemblyName;
        _detail.Description = source.Description;
        _detail.Concurrent = source.Concurrent;
        _detail.IncludeAnnotations = source.IncludeAnnotations;
        _detail.Properties = source.Properties;
        _detail.UpdatedTime = ScheduleTimeBridge.FromUnixSeconds(source.UpdatedTime);
        return this;
    }

    public JobBuilder SetGroupName(string? value)
    {
        _detail.GroupName = value;
        return this;
    }

    public JobBuilder SetJobType(string? assemblyName, string? jobType)
    {
        _detail.AssemblyName = assemblyName;
        _detail.JobType = jobType;
        return this;
    }

    public JobBuilder SetJobType(Type jobType)
    {
        _detail.AssemblyName = jobType.Assembly.GetName().Name;
        _detail.JobType = jobType.FullName;
        return this;
    }

    public JobBuilder SetDescription(string? value)
    {
        _detail.Description = value;
        return this;
    }

    public JobBuilder SetConcurrent(bool value)
    {
        _detail.Concurrent = value;
        return this;
    }

    public JobBuilder SetIncludeAnnotations(bool value)
    {
        _detail.IncludeAnnotations = value;
        return this;
    }

    public JobBuilder SetProperties(string value)
    {
        _detail.Properties = value;
        return this;
    }

    private static Type? ResolveType(string? assemblyName, string? typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return null;
        }

        return ProjectTypeCatalog.EffectiveTypes.FirstOrDefault(type => type.FullName == typeName)
               ?? Type.GetType($"{typeName}, {assemblyName}", false);
    }
}