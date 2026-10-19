using System.Text.Json.Serialization;

namespace GameFrameX.Application.Game.Goods.GameCommodity.Dto;

/// <summary>
/// 游戏商品交易分页查询输入参数
/// </summary>
public class PageTenantGameCommodityEntityRequest : BasePageRequest
{
    /// <summary>
    /// 应用Id（可选过滤，按当前应用筛选商品交易）
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// 类别Id（可选过滤，等值匹配）
    /// </summary>
    public long? CategoryId { get; set; }

    /// <summary>
    /// 多少rmb
    /// </summary>
    public bool? Cost { get; set; }

    /// <summary>
    ///  数量（搜索表单为文本输入，宽容读取字符串提交——C391）
    /// </summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? Number { get; set; }

    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string CreatedName { get; set; } = null!;

    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string UpdatedName { get; set; } = null!;
}
