namespace GameFrameX.Application.Game.Mail.GameMailTemplate.Dto;

/// <summary>
/// 邮件模板增加输入参数
/// </summary>
public class AddTenantGameMailTemplateEntityRequest : TenantGameMailTemplateEntityBaseRequest
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long Id { get; set; }
}