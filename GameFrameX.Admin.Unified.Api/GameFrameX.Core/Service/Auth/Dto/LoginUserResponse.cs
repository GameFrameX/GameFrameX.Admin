using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Service.Auth.Dto;

/// <summary>
/// 用户登录信息
/// </summary>
public class LoginUserResponse
{
    /// <summary>
    /// 用户id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 账号名称
    /// </summary>
    public string Account { get; set; } = null!;

    /// <summary>
    /// 真实姓名
    /// </summary>
    public string RealName { get; set; } = null!;

    /// <summary>
    /// 账号类型
    /// </summary>
    public AccountTypeEnum AccountType { get; set; } = AccountTypeEnum.NormalUser;

    /// <summary>
    /// 头像
    /// </summary>
    public string Avatar { get; set; } = null!;

    /// <summary>
    /// 个人简介
    /// </summary>
    public string Introduction { get; set; } = null!;

    /// <summary>
    /// 地址
    /// </summary>
    public string Address { get; set; } = null!;

    /// <summary>
    /// 电子签名
    /// </summary>
    public string Signature { get; set; } = null!;

    /// <summary>
    /// 机构Id
    /// </summary>
    public long OrganizationId { get; set; }

    /// <summary>
    /// 机构名称
    /// </summary>
    public string OrganizationName { get; set; } = null!;

    /// <summary>
    /// 机构类型
    /// </summary>
    public string OrganizationType { get; set; } = null!;

    /// <summary>
    /// 职位名称
    /// </summary>
    public string PosName { get; set; } = null!;

    /// <summary>
    /// 按钮权限集合
    /// </summary>
    public List<string> Buttons { get; set; } = null!;
}