namespace GameFrameX.Application.Game.System.Email.Dto;

/// <summary>
/// 平台系统邮件 Provider 配置更新输入参数。
/// <para>敏感字段（UserName / Password / SecretId / SecretKey）回传 <c>******</c> 时保留数据库原值，避免脱敏回写覆盖真实凭据。</para>
/// </summary>
public class UpdateSystemEmailProviderRequest : SystemEmailProviderBaseRequest, ICrudUpdateRequest
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "主键Id必须大于0")]
    public long Id { get; set; }

    /// <summary>
    /// 客户端读取记录时的版本号，用于阻止过期数据覆盖较新的修改。
    /// </summary>
    [Required(ErrorMessage = "版本号不能为空")]
    [Range(0, long.MaxValue, ErrorMessage = "版本号不能小于0")]
    public long? RowVersion { get; set; }
}