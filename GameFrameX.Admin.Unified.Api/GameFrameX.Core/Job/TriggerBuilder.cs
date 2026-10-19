using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;

namespace GameFrameX.Core.Job;

public sealed class TriggerBuilder
{
    private readonly Trigger _trigger = new();

    private TriggerBuilder(string? triggerId)
    {
        _trigger.TriggerId = triggerId;
    }

    public string? TriggerId
    {
        get { return _trigger.TriggerId; }
    }

    public string? JobId
    {
        get { return _trigger.JobId; }
    }

    public Trigger Trigger
    {
        get { return _trigger; }
    }

    public static TriggerBuilder Create(string? triggerId)
    {
        return new TriggerBuilder(triggerId);
    }

    public TriggerBuilder LoadFrom(SystemJobTriggerEntity source)
    {
        _trigger.TriggerId = source.TriggerId;
        _trigger.JobId = source.JobId;
        _trigger.TriggerType = source.TriggerType;
        _trigger.AssemblyName = source.AssemblyName;
        _trigger.Args = source.Args;
        _trigger.Description = source.Description;
        _trigger.Status = source.Status;
        _trigger.StartTime = ScheduleTimeBridge.FromUnixSeconds(source.StartTime);
        _trigger.EndTime = ScheduleTimeBridge.FromUnixSeconds(source.EndTime);
        _trigger.LastRunTime = ScheduleTimeBridge.FromUnixSeconds(source.LastRunTime);
        _trigger.NextRunTime = ScheduleTimeBridge.FromUnixSeconds(source.NextRunTime);
        _trigger.NumberOfRuns = source.NumberOfRuns;
        _trigger.MaxNumberOfRuns = source.MaxNumberOfRuns;
        _trigger.NumberOfErrors = source.NumberOfErrors;
        _trigger.MaxNumberOfErrors = source.MaxNumberOfErrors;
        _trigger.NumRetries = source.NumRetries;
        _trigger.RetryTimeout = source.RetryTimeout;
        _trigger.StartNow = source.StartNow;
        _trigger.RunOnStart = source.RunOnStart;
        _trigger.ResetOnlyOnce = source.ResetOnlyOnce;
        _trigger.UpdatedTime = ScheduleTimeBridge.FromUnixSeconds(source.UpdatedTime);
        return this;
    }

    public TriggerBuilder SetTriggerType(string assemblyName, string triggerType)
    {
        _trigger.AssemblyName = assemblyName;
        _trigger.TriggerType = triggerType;
        return this;
    }

    public TriggerBuilder SetArgs(string? value)
    {
        _trigger.Args = value;
        return this;
    }

    public TriggerBuilder SetDescription(string? value)
    {
        _trigger.Description = value;
        return this;
    }

    public TriggerBuilder SetStatus(TriggerStatus value)
    {
        _trigger.Status = value;
        return this;
    }

    public TriggerBuilder SetStartTime(DateTime? value)
    {
        _trigger.StartTime = value;
        return this;
    }

    public TriggerBuilder SetEndTime(DateTime? value)
    {
        _trigger.EndTime = value;
        return this;
    }

    public TriggerBuilder SetLastRunTime(DateTime? value)
    {
        _trigger.LastRunTime = value;
        return this;
    }

    public TriggerBuilder SetNextRunTime(DateTime? value)
    {
        _trigger.NextRunTime = value;
        return this;
    }

    public TriggerBuilder SetNumberOfRuns(long value)
    {
        _trigger.NumberOfRuns = value;
        return this;
    }

    public TriggerBuilder SetMaxNumberOfRuns(long value)
    {
        _trigger.MaxNumberOfRuns = value;
        return this;
    }

    public TriggerBuilder SetNumberOfErrors(long value)
    {
        _trigger.NumberOfErrors = value;
        return this;
    }

    public TriggerBuilder SetMaxNumberOfErrors(long value)
    {
        _trigger.MaxNumberOfErrors = value;
        return this;
    }

    public TriggerBuilder SetNumRetries(int value)
    {
        _trigger.NumRetries = value;
        return this;
    }

    public TriggerBuilder SetRetryTimeout(int value)
    {
        _trigger.RetryTimeout = value;
        return this;
    }

    public TriggerBuilder SetStartNow(bool value)
    {
        _trigger.StartNow = value;
        return this;
    }

    public TriggerBuilder SetRunOnStart(bool value)
    {
        _trigger.RunOnStart = value;
        return this;
    }

    public TriggerBuilder SetResetOnlyOnce(bool value)
    {
        _trigger.ResetOnlyOnce = value;
        return this;
    }

    public TriggerBuilder Updated()
    {
        _trigger.UpdatedTime = DateTime.Now;
        return this;
    }
}