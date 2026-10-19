// GameFrameX 组织下的以及组织衍生的项目的版权、商标和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。

using GameFrameX.Application.Game.Online.LiveOps.PlayerSegment;
using GameFrameX.Entity.Game.Online;

namespace GameFrameX.Application.Game.Online.LiveOps.PlayerSegment.Dto;

/// <summary>
/// 玩家分群实体输入基类（Add / Update 公共字段；来源 change C343 / vault:C8 S7.5）。
/// <para>条件以 JSON 字符串承载（List&lt;<see cref="OnlineSegmentCondition"/>&gt;），字段名与操作符受
/// <c>OnlineRolloutSemanticMap</c> 白名单校验（Admin 只校验不解释，语义解释在 Online 侧）。</para>
/// </summary>
public abstract class TenantGameOnlinePlayerSegmentBaseRequest : BaseUpdateRequest
{
    /// <summary>
    /// 应用Id。事实数据应用作用域，必须 &gt; 0。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    public long AppId { get; set; }

    /// <summary>
    /// 分群名称。
    /// </summary>
    [Required(ErrorMessage = "分群名称不能为空")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 分群描述。
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 分群条件 JSON（List&lt;<see cref="OnlineSegmentCondition"/>&gt;；空 = 不限条件）。
    /// </summary>
    public string? ConditionsJson { get; set; }
}
