// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

namespace GameFrameX.Entity.Game.Mail;

/// <summary>
/// 运营邮件 Campaign 邮件类型。值域逐字对齐 Server <c>MailType</c>（跨仓库契约）。
/// <para><b>不复用</b> Admin 既有 <c>GameMailType</c>（PersonalMail/SystemMail/...）——两者值域不同（既有 SystemMail=2，Server Operation=1），混用会导致 wire 格式错位。</para>
/// </summary>
public enum MailCampaignMailType
{
    /// <summary>
    /// 系统邮件：系统自动触发（如维护补偿），无运营 Campaign 概念时也走相同展示通道。
    /// </summary>
    System = 0,

    /// <summary>
    /// 运营邮件：由 Admin 发布接口创建 Campaign，按过滤条件命中懒创建到玩家邮箱。
    /// </summary>
    Operation = 1,

    /// <summary>
    /// 补偿邮件：针对单角色或小范围的定向补偿，复用 Campaign 落库但不走全服过滤命中。
    /// </summary>
    Compensation = 2,
}