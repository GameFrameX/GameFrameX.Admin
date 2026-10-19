// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

namespace GameFrameX.Entity.Game.Mail;

/// <summary>
/// Server 运营邮件 Campaign 接口业务码。值域逐字对齐 Server <c>OperationStatusCode</c>（跨仓库契约）。
/// <para>Admin 收到 Server 响应后按此码映射到 Admin <c>MailCampaignErrorCode</c>（C119）并回写 <c>LastServerCode</c>。</para>
/// </summary>
public enum MailCampaignServerCode
{
    /// <summary>
    /// 成功。
    /// </summary>
    Ok = 0,

    /// <summary>
    /// Campaign 已发布或已撤回，主体字段不可修改（B1）。
    /// </summary>
    HasExist = 1,

    /// <summary>
    /// Campaign 参数非法。
    /// </summary>
    InvalidCampaignParameter = 2,

    /// <summary>
    /// Campaign 不存在。
    /// </summary>
    CampaignNotFound = 3,

    /// <summary>
    /// Campaign 已撤回，不可重复撤回。
    /// </summary>
    CampaignAlreadyRevoked = 4,

    /// <summary>
    /// 附件奖励非法（数量非正等）。
    /// </summary>
    InvalidReward = 5,

    /// <summary>
    /// 附件奖励类型暂不支持（一期仅 NormalItem 可发放）。
    /// </summary>
    Unsupported = 6,

    /// <summary>
    /// Server 调用网络级失败（不可达/超时/信封异常），非 Server 返回的业务码。
    /// </summary>
    NetworkError = 7,
}