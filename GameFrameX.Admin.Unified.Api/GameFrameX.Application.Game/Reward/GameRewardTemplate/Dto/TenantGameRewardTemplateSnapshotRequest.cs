namespace GameFrameX.Application.Game.Reward.GameRewardTemplate.Dto;

/// <summary>
/// 奖励模板快照输入。
/// </summary>
public class TenantGameRewardTemplateSnapshotRequest : BaseIdRequest
{
    /// <summary>
    /// 当前应用Id。快照复制必须限定到当前应用，禁止 AppId=0。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    public long? AppId { get; set; }
}