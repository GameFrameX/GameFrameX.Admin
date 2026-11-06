namespace GameFrameX.Application.ServerCenter.GameAreaClassify.Dto;

/// <summary>
/// 游戏大区分类分页查询输入参数
/// </summary>
public class PageGameAreaClassifyEntityRequest : BasePageRequest
{

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;
}