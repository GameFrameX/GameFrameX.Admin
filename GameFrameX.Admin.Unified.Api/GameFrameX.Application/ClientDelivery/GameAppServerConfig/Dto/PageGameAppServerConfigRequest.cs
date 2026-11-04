namespace GameFrameX.Application.ClientDelivery.GameAppServerConfig.Dto;

/// <summary>
/// 游戏运营配置管理分页查询输入参数
/// </summary>
public class PageGameAppServerConfigRequest : BaseAppFilterPageRequest
{
    /// <summary>
    /// 配置键精确筛选
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// 键级启停筛选
    /// </summary>
    public bool? Enabled { get; set; }
}
