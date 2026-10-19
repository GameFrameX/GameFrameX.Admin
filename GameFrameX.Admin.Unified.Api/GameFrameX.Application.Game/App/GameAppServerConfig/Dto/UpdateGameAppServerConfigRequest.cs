namespace GameFrameX.Application.Game.App.GameAppServerConfig.Dto;

/// <summary>
/// 游戏运营配置管理更新输入参数
/// </summary>
public class UpdateGameAppServerConfigRequest : GameAppServerConfigBaseRequest, ICrudUpdateRequest
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
