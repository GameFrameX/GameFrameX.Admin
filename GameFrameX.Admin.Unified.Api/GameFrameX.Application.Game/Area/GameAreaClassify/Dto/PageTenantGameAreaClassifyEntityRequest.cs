namespace GameFrameX.Application.Game.Area.GameAreaClassify.Dto;

/// <summary>
/// 游戏大区分类分页查询输入参数
/// </summary>
public class PageTenantGameAreaClassifyEntityRequest : BasePageRequest
{
    /// <summary>
    /// 应用Id
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;
}