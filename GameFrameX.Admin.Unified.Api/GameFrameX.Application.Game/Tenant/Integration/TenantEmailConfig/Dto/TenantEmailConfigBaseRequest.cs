namespace GameFrameX.Application.Game.Tenant.Integration.TenantEmailConfig.Dto;

/// <summary>
/// 租户业务邮件配置新增/更新共有输入参数。
/// </summary>
public class TenantEmailConfigBaseRequest : BaseAddRequest
{
    /// <summary>
    /// 应用 ID。0 表示租户默认邮件配置；大于 0 表示应用级覆盖。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 租户业务邮件用途。0 表示通配该租户 / 应用下所有租户业务邮件。
    /// </summary>
    public int Purpose { get; set; }

    /// <summary>
    /// Provider 类型。当前仅支持 <c>TencentCloudSes</c>。
    /// </summary>
    [Required(ErrorMessage = "Provider类型不能为空")]
    public string Provider { get; set; } = "TencentCloudSes";

    /// <summary>
    /// Provider 区域。
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// 腾讯云邮件模板组 / 模板集标识。
    /// </summary>
    public string? TemplateSet { get; set; }

    /// <summary>
    /// 发件人邮箱地址。
    /// </summary>
    [Required(ErrorMessage = "发件人邮箱不能为空")]
    public string FromEmailAddress { get; set; } = null!;

    /// <summary>
    /// 默认发件人显示名。
    /// </summary>
    public string? DefaultFromName { get; set; }

    /// <summary>
    /// 默认邮件主题前缀。
    /// </summary>
    public string? DefaultSubjectPrefix { get; set; }

    /// <summary>
    /// 配置选择优先级（数值越大越优先）。仅用于配置选择，不代表邮件投递队列优先级。
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// 是否启用。禁用配置不参与租户业务邮件选择和测试发送。
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 备注。
    /// </summary>
    public string Remark { get; set; } = null!;
}