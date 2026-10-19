using GameFrameX.Entity.Base;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏广告奖励发放账本。
/// <para>广告奖励发放权威事实源（C114 / GFX-110），覆盖 TopOn / GoogleAdMob / UnityAds 服务端回调。</para>
/// <para>作用域：事实数据应用作用域 <see cref="EntityBasic"/> 的 <c>TenantId</c> + <see cref="AppId"/>；AppId 跟随角色 AppId，由 <c>AppScopeRules.EnsureFactAppId</c> 守卫。</para>
/// <para>幂等：<see cref="IdempotencyKey"/> = <c>adreward:{NormalizedProvider}:{TransactionId}</c>，数据库唯一约束兜底，同一广告 transaction 重放不重复发奖。</para>
/// </summary>
[SugarTable(null, "游戏广告奖励发放账本")]
[SugarIndex("ux_ad_reward_idempotency", nameof(IdempotencyKey), OrderByType.Asc, true)]
[SugarIndex("idx_ad_reward_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameAdvertisementRewardEntity : EntityBasic, IAppScoped
{
    /// <summary>
    /// 应用 Id。事实数据应用作用域，跟随角色 <c>AppId</c>，必须 &gt; 0。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 广告平台 Provider。标准化值：<c>TopOn</c> / <c>GoogleAdMob</c> / <c>UnityAds</c>。
    /// </summary>
    [SugarColumn(ColumnDescription = "广告平台Provider", Length = EntityStringLength.Length256)]
    public string Provider { get; set; } = null!;

    /// <summary>
    /// 平台交易号。TopOn <c>trans_id</c> / AdMob <c>transaction_id</c> / UnityAds <c>oid</c>，由 Provider Option 的 <c>*Param</c> 从回调 query 取。
    /// </summary>
    [SugarColumn(ColumnDescription = "平台交易号", Length = EntityStringLength.Length256)]
    public string TransactionId { get; set; } = null!;

    /// <summary>
    /// 广告位/单元。TopOn <c>placement_id</c> / AdMob <c>ad_unit</c>，schema 取；UnityAds 无则空。
    /// </summary>
    [SugarColumn(ColumnDescription = "广告位", Length = EntityStringLength.Length256, IsNullable = true)]
    public string AdvertisementUnit { get; set; } = null!;

    /// <summary>
    /// 角色 Id。反查自 <c>TenantGameRoleEntity</c>。
    /// </summary>
    [SugarColumn(ColumnDescription = "角色Id", Length = EntityStringLength.Length256)]
    public string RoleId { get; set; } = null!;

    /// <summary>
    /// 用户 Id。来自平台 userId 参数。
    /// </summary>
    [SugarColumn(ColumnDescription = "用户Id", DefaultValue = "0")]
    public long UserId { get; set; }

    /// <summary>
    /// 区服 Id。来自角色 <c>AreaId</c>。
    /// </summary>
    [SugarColumn(ColumnDescription = "区服Id", DefaultValue = "0")]
    public long AreaId { get; set; }

    /// <summary>
    /// 奖励项名。TopOn <c>reward_name</c> / AdMob <c>reward_item</c>，schema 取。
    /// </summary>
    [SugarColumn(ColumnDescription = "奖励项", Length = EntityStringLength.Length256, IsNullable = true)]
    public string RewardItem { get; set; } = null!;

    /// <summary>
    /// 奖励数量原文（字符串保留，平台可能传小数或字符串）。TopOn/AdMob <c>reward_amount</c>。
    /// </summary>
    [SugarColumn(ColumnDescription = "奖励数量", Length = EntityStringLength.Length256, IsNullable = true)]
    public string RewardAmount { get; set; } = null!;

    /// <summary>
    /// 发放状态。
    /// </summary>
    [SugarColumn(ColumnDescription = "发放状态", DefaultValue = "0")]
    public AdvertisementRewardStatusEnum Status { get; set; }

    /// <summary>
    /// 幂等键。组成规则：<c>adreward:{NormalizedProvider}:{TransactionId}</c>，数据库唯一约束兜底。
    /// </summary>
    [SugarColumn(ColumnDescription = "幂等键", Length = EntityStringLength.Length512)]
    public string IdempotencyKey { get; set; } = null!;

    /// <summary>
    /// 失败原因。仅 <see cref="AdvertisementRewardStatusEnum.Failed"/> 写，游戏服返回 message 或异常摘要。
    /// </summary>
    [SugarColumn(ColumnDescription = "失败原因", Length = EntityStringLength.Length2048, IsNullable = true)]
    public string FailureReason { get; set; } = null!;

    /// <summary>
    /// 是否沙箱/测试回调。TopOn <c>is_test&gt;0</c> / AdMob custom_data.IsSandbox / UnityAds 默认 false。
    /// </summary>
    [SugarColumn(ColumnDescription = "是否沙箱", DefaultValue = "false")]
    public bool IsSandbox { get; set; }
}