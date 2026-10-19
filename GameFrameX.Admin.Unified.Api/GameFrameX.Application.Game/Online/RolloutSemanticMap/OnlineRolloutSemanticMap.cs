namespace GameFrameX.Application.Game.Online.RolloutSemanticMap;

/// <summary>
/// 灰度 / 分群语义三方对照表（来源 change C343 / vault:C8 S7.5+S7.8；A0 契约对齐评审产物，禁散落实现）。
/// <para>
/// 唯一职责：集中承载「Admin 编排语义 ↔ Online 侧解释 ↔ 既有 <c>RemoteConfigRolloutMatcher</c> 语义」的对照关系、
/// Segment 条件字段白名单、灰度百分比边界常量。Admin 侧只编排不解释——不在 Admin 内复算 Online 的分群命中或百分比桶展开；
/// 各 Service 引用本类常量与白名单做校验，不得内联第二套数值或字段表。
/// </para>
/// <list type="table">
/// <listheader><term>维度</term><term>Admin 编排语义（本表登记）</term><term>Online 侧解释（vault S7.8）</term><term>既有 RolloutMatcher（Client.Api）</term></listheader>
/// <item><term>灰度维度</term><description>SegmentId（玩家分群行）+ Percent</description><description>玩家 / 事件数据维度命中 + 百分比桶稳定展开，命中记录可查（VC-7.11）</description><description>设备维度（deviceId）</description></item>
/// <item><term>百分比语义</term><description>PublishPercentMin=1 ~ PublishPercentMax=100（发布语义：0 无意义即拒绝，100 全量）</description><description>桶序 &lt; percent 即命中，分群未命中直接不生效</description><description>记录级 RolloutPercent 0-100：0=关闭、100=全量（Math.Clamp 防漂移）</description></item>
/// <item><term>短路优先级</term><description>不设白名单（Admin 不编排个体豁免）</description><description>分群命中是前置条件，百分比只在分群内展开</description><description>DeviceWhitelistJson 白名单命中即生效（先于百分比）</description></item>
/// <item><term>稳定性依据</term><description>不承诺（Admin 不计算桶）</description><description>同一玩家对同一版本的桶归属稳定（重复判定结果一致，可回溯命中记录）</description><description>稳定桶 = SHA256(tenantId:appId:deviceId:recordId) % 100，同键恒定</description></item>
/// <item><term>回滚语义</term><description>回滚 = 版本化新下发（新 ConfigVersion 行 + TargetVersion 指向目标历史版本；旧行置 RolledBack 不可变）</description><description>按新版本命令生效，Online 侧确认版本回执（VC-7.12：回滚后行为恢复 + 审计完整）</description><description>无版本概念（记录级覆盖），不适用于本对照行</description></item>
/// </list>
/// </summary>
public static class OnlineRolloutSemanticMap
{
    /// <summary>
    /// 灰度发布百分比下界（发布语义：1 = 最小有效灰度；0 无意义，入口即拒绝——区别于记录级 RolloutPercent 的 0=关闭）。
    /// </summary>
    public const int PublishPercentMin = 1;

    /// <summary>
    /// 灰度发布百分比上界（100 = 分群内全量生效）。
    /// </summary>
    public const int PublishPercentMax = 100;

    /// <summary>
    /// Segment 条件字段白名单（第一版三族起步，锚定 vault S7.5 标准事件族；联调后扩充须回本表登记，禁散落）。
    /// <list type="bullet">
    /// <item><description><c>activeDays</c>：近 N 日活跃天数（锚定 Login / SessionStart 事件族）。</description></item>
    /// <item><description><c>retentionDays</c>：自注册起的留存天数（锚定 Install / Register 事件族）。</description></item>
    /// <item><description><c>totalPaymentAmount</c>：累计付费金额（锚定 Purchase 事件族）。</description></item>
    /// </list>
    /// Online 侧按 S7.5 事件数据解释字段语义；Admin 只校验字段名在白名单内，不解释、不复算命中。
    /// </summary>
    public static readonly IReadOnlySet<string> SegmentConditionFields = new HashSet<string>(StringComparer.Ordinal)
    {
        "activeDays",
        "retentionDays",
        "totalPaymentAmount",
    };

    /// <summary>
    /// Segment 条件操作符白名单（gte=大于等于 / lte=小于等于 / eq=等于；闭区间与等值三操作符起步）。
    /// </summary>
    public static readonly IReadOnlySet<string> SegmentConditionOperators = new HashSet<string>(StringComparer.Ordinal)
    {
        "gte",
        "lte",
        "eq",
    };
}
