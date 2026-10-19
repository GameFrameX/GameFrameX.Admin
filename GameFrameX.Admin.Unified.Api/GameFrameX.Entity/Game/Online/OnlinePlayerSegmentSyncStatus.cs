// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯用户合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

using System.ComponentModel;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Entity.Game.Online;

/// <summary>
/// 玩家分群定义的 Online 侧同步状态（来源 change C343 / vault:C8 S7.8）。
/// <para>Admin 是分群定义的事实源（编排），Online 是解释与命中判定方；本状态只反映最近一次
/// <c>syncOnline</c> 动作的结果，同步失败（<c>Failed</c>）可修正后重试。</para>
/// </summary>
[Description("玩家分群同步状态")]
[AdminEnum]
public enum OnlinePlayerSegmentSyncStatus
{
    /// <summary>
    /// 未同步：新建或编辑后尚未推送 Online，Online 侧不存在该分群定义。
    /// </summary>
    [Description("未同步")] NotSynced = 0,

    /// <summary>
    /// 已同步：Online 已接受分群定义并回执 OnlineSegmentId；可供 Rollout 灰度引用。
    /// </summary>
    [Description("已同步")] Synced = 1,

    /// <summary>
    /// 同步失败：Online 拒绝（条件字段非法等），<c>LastServerCode/LastServerMessage</c> 记录原因，修正后可重试。
    /// </summary>
    [Description("同步失败")] Failed = 2,
}
