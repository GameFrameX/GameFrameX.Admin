namespace GameFrameX.Application.Game.App.GameAppServerConfig.Dto;

/// <summary>
/// 游戏运营配置管理分页查询输入参数
/// </summary>
public class PageGameAppServerConfigRequest : BaseAppFilterPageRequest
{
    /// <summary>
    /// 应用作用域筛选。游戏运营配置是应用工作台页面，分页查询必须提供具体应用，值必须大于 0
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 配置键精确筛选
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// 键级启停筛选
    /// </summary>
    public bool? Enabled { get; set; }
}
