using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏角色
/// </summary>
[SugarTable(null, "游戏角色")]
[SugarIndex("idx_tenant_game_role_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameRoleEntity : EntityBasicSelect, IAppScoped
{
    /// <summary>
    /// 应用Id。事实数据应用作用域（TenantId + AppId），必须 &gt; 0。
    /// <para>来源：Client API 建角由 <c>AppResolutionMiddleware.AppIdKey</c> 写入；Admin 由后台当前应用上下文写入。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    [SugarColumn(ColumnDescription = "用户ID", DefaultValue = "0")]
    public long UserId { get; set; }

    /// <summary>
    /// 区服Id
    /// </summary>
    [SugarColumn(ColumnDescription = "区服Id", DefaultValue = "0")]
    public long AreaId { get; set; }

    /// <summary>
    /// 等级
    /// </summary>
    [SugarColumn(ColumnDescription = "等级")]
    public int? Level { get; set; }

    /// <summary>
    /// 是否是沙盒账户
    /// </summary>
    [SugarColumn(ColumnDescription = "是否是沙盒账户")]
    public bool? IsSandbox { get; set; }

    /// <summary>
    /// 最后一次登录时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "最后一次登录时间", DefaultValue = "0")]
    public long LastLoginTime { get; set; }

    /// <summary>
    /// 角色状态
    /// </summary>
    [SugarColumn(ColumnDescription = "角色状态")]
    public int? State { get; set; }

    /// <summary>
    /// 角色头像
    /// </summary>
    [SugarColumn(ColumnDescription = "角色头像")]
    public uint? Avatar { get; set; }

    /// <summary>
    /// 角色当前经验
    /// </summary>
    [SugarColumn(ColumnDescription = "角色当前经验")]
    public ulong? CurrentExp { get; set; }

    /// <summary>
    /// 当前关卡配置id
    /// </summary>
    [SugarColumn(ColumnDescription = "当前关卡配置id")]
    public int? CurrentLevelConfigId { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    [SugarColumn(ColumnDescription = "性别")]
    public bool? Gender { get; set; }

    /// <summary>
    /// 角色头像框
    /// </summary>
    [SugarColumn(ColumnDescription = "角色头像框")]
    public uint? AvatarFrame { get; set; }

    /// <summary>
    /// 累充金额(分)
    /// </summary>
    [SugarColumn(ColumnDescription = "累充金额")]
    public int? AllRecharge { get; set; }

    /// <summary>
    /// 在线时间分
    /// </summary>
    [SugarColumn(ColumnDescription = "在线总时长")]
    public long? OnlineTime { get; set; }
}