namespace GameFrameX.Application.ServerCenter.GameAreaMerge.Dto;

/// <summary>取消合服计划请求。</summary>
public class CancelGameAreaMergePlanRequest
{
    public long Id { get; set; }

    [Required]
    public string Reason { get; set; } = string.Empty;
}
