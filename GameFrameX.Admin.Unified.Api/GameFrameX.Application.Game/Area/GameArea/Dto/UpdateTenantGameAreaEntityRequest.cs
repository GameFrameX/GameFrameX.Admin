namespace GameFrameX.Application.Game.Area.GameArea.Dto;

/// <summary>
/// 游戏区服更新输入参数
/// </summary>
public class UpdateTenantGameAreaEntityRequest : TenantGameAreaEntityBaseRequest
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 渠道id
    /// </summary>
    public long ChannelId { get; set; }

    /// <summary>
    /// 平台id
    /// </summary>
    public long PlatformId { get; set; }

    /// <summary>
    /// 包id
    /// </summary>
    public long PackageId { get; set; }

    // Id、ServerParams 及其余公共字段已从 TenantGameAreaEntityBaseRequest 继承
}