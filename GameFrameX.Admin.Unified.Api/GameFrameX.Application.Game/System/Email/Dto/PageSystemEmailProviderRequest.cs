namespace GameFrameX.Application.Game.System.Email.Dto;

/// <summary>
/// 平台系统邮件 Provider 配置分页查询输入参数。
/// </summary>
public class PageSystemEmailProviderRequest : BasePageRequest
{
    /// <summary>
    /// 平台系统邮件用途（精确过滤）。空表示不限。
    /// </summary>
    public int? Purpose { get; set; }

    /// <summary>
    /// Provider 类型（精确过滤）。空表示不限。
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool? IsActive { get; set; }
}