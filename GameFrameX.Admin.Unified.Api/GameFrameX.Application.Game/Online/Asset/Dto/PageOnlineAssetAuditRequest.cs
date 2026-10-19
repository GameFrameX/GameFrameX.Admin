namespace GameFrameX.Application.Game.Online.Asset.Dto;

/// <summary>
/// 资产审计分页查询输入参数（操作者/时间/作用域/命令类型 + 目标玩家；Cursor 翻页，vault:C2 S1.2）。
/// <para>AppId/ServerId 必填（EXECUTE 期回写：C339 作用域三元组为每次 Online 调用硬前置，审计查询无跨区服聚合端点）。</para>
/// </summary>
public class PageOnlineAssetAuditRequest
{
    /// <summary>
    /// 应用作用域（必填，经 OnlineScopeContextFactory 校验租户归属）。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "应用Id必须大于0")]
    public long AppId { get; set; }

    /// <summary>
    /// 区服作用域（必填，经 OnlineScopeContextFactory 校验租户与应用归属）。
    /// </summary>
    [Required(ErrorMessage = "区服Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "区服Id必须大于0")]
    public long ServerId { get; set; }

    /// <summary>
    /// 操作者 Id 过滤（null = 不过滤）。
    /// </summary>
    [MaxLength(EntityStringLength.Length256)]
    public string? OperatorId { get; set; }

    /// <summary>
    /// 目标玩家 Id 过滤（null = 不过滤）。
    /// </summary>
    [MaxLength(EntityStringLength.Length256)]
    public string? PlayerId { get; set; }

    /// <summary>
    /// 命令类型过滤（grant / revoke 等；null = 不过滤）。
    /// </summary>
    [MaxLength(EntityStringLength.Length256)]
    public string? CommandType { get; set; }

    /// <summary>
    /// 发生时间窗起点（Unix 秒；null = 不限）。
    /// </summary>
    public long? StartTime { get; set; }

    /// <summary>
    /// 发生时间窗终点（Unix 秒；null = 不限）。
    /// </summary>
    public long? EndTime { get; set; }

    /// <summary>
    /// 游标（上一页返回的 NextCursor；首页不传）。
    /// </summary>
    public string? Cursor { get; set; }

    /// <summary>
    /// 页大小（1～100，默认 20）。
    /// </summary>
    [Range(1, 100, ErrorMessage = "页大小必须在1~100之间")]
    public int PageSize { get; set; } = 20;
}
