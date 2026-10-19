using System.Text.Json.Serialization;

namespace GameFrameX.Application.Game.Goods.GameCommodity.Dto;

/// <summary>
/// 游戏商品交易增加输入参数
/// </summary>
// C391（GFX-923 / Api#291）：AddRequest 继承空基类 BaseAddRequest，不携带主键 Id（对齐
// AddTenantGameGoodsCatalogueEntityRequest 的既定修正注释）。此前 Id:long 带 [Required] 属新增语义错误。
// 契约：CategoryId 统一 long（Id 后缀契约兼容数字/字符串提交）；Number 为数量、保持 long，
// 前端 TextField 提交 JS string，加 AllowReadingFromString 宽容读取，防整个 body 反序列化失败 → input null → NRE。
public class AddTenantGameCommodityEntityRequest : BaseAddRequest
{
    /// <summary>
    /// 应用Id。事实数据应用作用域，必须 &gt; 0。
    /// <para>过渡期：后台运营当前应用上下文尚未建立，由前端手填。</para>
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    public long AppId { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [Required(ErrorMessage = "状态不能为空")]
    public string Status { get; set; } = null!;

    /// <summary>
    /// 类别Id。引用 TenantGameGoodsCatalogueEntity.Id；Id 后缀契约兼容数字/字符串提交。
    /// </summary>
    [Required(ErrorMessage = "类别不能为空")]
    public long CategoryId { get; set; }

    /// <summary>
    /// 多少rmb
    /// </summary>
    [Required(ErrorMessage = "多少rmb不能为空")]
    public bool Cost { get; set; }

    /// <summary>
    ///  数量
    /// </summary>
    [Required(ErrorMessage = " 数量不能为空")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long Number { get; set; }
}
