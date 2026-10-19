// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯用户合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

using System.ComponentModel;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Entity.Game.Online;

/// <summary>
/// 灰度发布（Rollout）版本行状态（来源 change C343 / vault:C8 S7.8 / VC-7.12）。
/// <para>回滚 = 版本化新下发：不修改、不删除任何历史行，被回退的版本行置 <see cref="RolledBack"/> 后不可再作为发布基线；
/// 回滚动作本身产生携带新 <c>ConfigVersion</c> 与 <c>TargetVersion</c>（指向目标历史版本）的新行。</para>
/// </summary>
[Description("灰度发布状态")]
[AdminEnum]
public enum OnlineConfigRolloutStatus
{
    /// <summary>
    /// 已发布：Online 已接受本次灰度下发并回执确认版本；当前生效中的版本。
    /// </summary>
    [Description("已发布")] Published = 1,

    /// <summary>
    /// 已回退：被后续发布或回滚替代的历史版本，主体不可变、不可再作为发布基线（VC-7.12 审计追溯依据）。
    /// </summary>
    [Description("已回退")] RolledBack = 2,
}
