namespace GameFrameX.Application.Game.App.FeatureSwitch.Dto;

/// <summary>
/// 活动/功能开关管理更新输入参数
/// </summary>
public class UpdateGameAppFeatureSwitchRequest : GameAppFeatureSwitchBaseRequest, ICrudUpdateRequest
{
    /// <summary>
    /// 主键
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 乐观锁版本（编辑回显 RowVersion 原样回传）
    /// </summary>
    public long? RowVersion { get; set; }
}
