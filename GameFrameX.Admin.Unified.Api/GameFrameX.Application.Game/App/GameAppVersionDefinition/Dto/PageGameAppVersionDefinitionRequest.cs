namespace GameFrameX.Application.Game.App.GameAppVersionDefinition.Dto;

/// <summary>
/// 版本管理分页查询输入参数
/// </summary>
public class PageGameAppVersionDefinitionRequest : BasePageRequest
{
    /// <summary>
    /// 游戏应用Id筛选；null 不筛选。
    /// </summary>
    public long? AppId { get; set; }
}