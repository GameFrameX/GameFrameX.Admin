namespace GameFrameX.Core.Service.User.Dto;

public class PageUserRequest : BasePageRequest
{
    /// <summary>
    /// 账号
    /// </summary>
    public string Account { get; set; } = null!;

    /// <summary>
    /// 姓名
    /// </summary>
    public string RealName { get; set; } = null!;

    /// <summary>
    /// 手机号
    /// </summary>
    public string Phone { get; set; } = null!;

    /// <summary>
    /// 查询时所选机构Id
    /// </summary>
    public long OrganizationId { get; set; }
}