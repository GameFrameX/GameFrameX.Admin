namespace GameFrameX.Application.ServerCenter.GameAreaMerge.Dto;

/// <summary>合服流程数据。</summary>
public class ExecuteGameAreaMergePlanRequest
{
    public long Id { get; set; }
    [Required] public string BackupIdentifier { get; set; } = string.Empty;
    public bool BackupConfirmed { get; set; }
}
