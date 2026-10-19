namespace GameFrameX.Core.Logging;

public sealed class ProjectLoggingWriteError
{
    public string CurrentFileName { get; set; } = string.Empty;

    public void UseRollbackFileName(string fileName)
    {
        CurrentFileName = fileName;
    }
}