using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;

namespace GameFrameX.Core.Service.User.Dto;

/// <summary>
/// 新增/编辑用户输入参数。
/// 显式声明前端表单可填字段，不再继承 <see cref="SystemUserBase"/>，避免整表字段（含登录审计、导航属性、电子签名等）暴露给前端批量赋值。
/// </summary>
public class AddUserRequest
{
    /// <summary>
    /// 主键Id（新增为 0，编辑由前端回填）
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 乐观锁版本号
    /// </summary>
    public long? RowVersion { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    [Required(ErrorMessage = "账号不能为空")]
    public string Account { get; set; } = null!;

    /// <summary>
    /// 真实姓名
    /// </summary>
    [Required(ErrorMessage = "真实姓名不能为空")]
    public string RealName { get; set; } = null!;

    /// <summary>
    /// 昵称
    /// </summary>
    public string NickName { get; set; } = null!;

    /// <summary>
    /// 头像
    /// </summary>
    public string Avatar { get; set; } = null!;

    /// <summary>
    /// 性别-男_1、女_2
    /// </summary>
    public GenderEnum Sex { get; set; } = GenderEnum.Male;

    /// <summary>
    /// 年龄
    /// </summary>
    public int Age { get; set; }

    /// <summary>
    /// 出生日期
    /// </summary>
    public long? BirthdayTime { get; set; }

    /// <summary>
    /// 民族
    /// </summary>
    public string Nation { get; set; } = null!;

    /// <summary>
    /// 手机号码
    /// </summary>
    public string Phone { get; set; } = null!;

    /// <summary>
    /// 证件类型
    /// </summary>
    public CardTypeEnum CardType { get; set; }

    /// <summary>
    /// 身份证号
    /// </summary>
    public string IdCardNum { get; set; } = null!;

    /// <summary>
    /// 邮箱
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// 地址
    /// </summary>
    public string Address { get; set; } = null!;

    /// <summary>
    /// 文化程度
    /// </summary>
    public CultureLevelEnum CultureLevel { get; set; }

    /// <summary>
    /// 政治面貌
    /// </summary>
    public string PoliticalOutlook { get; set; } = null!;

    /// <summary>
    /// 毕业院校
    /// </summary>
    public string College { get; set; } = null!;

    /// <summary>
    /// 办公电话
    /// </summary>
    public string OfficePhone { get; set; } = null!;

    /// <summary>
    /// 紧急联系人
    /// </summary>
    public string EmergencyContact { get; set; } = null!;

    /// <summary>
    /// 紧急联系人电话
    /// </summary>
    public string EmergencyPhone { get; set; } = null!;

    /// <summary>
    /// 紧急联系人地址
    /// </summary>
    public string EmergencyAddress { get; set; } = null!;

    /// <summary>
    /// 排序
    /// </summary>
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; } = null!;

    /// <summary>
    /// 账号类型
    /// </summary>
    public AccountTypeEnum AccountType { get; set; } = AccountTypeEnum.NormalUser;

    /// <summary>
    /// 直属机构Id
    /// </summary>
    public long OrganizationId { get; set; }

    /// <summary>
    /// 职位Id
    /// </summary>
    public long PosId { get; set; }

    /// <summary>
    /// 工号
    /// </summary>
    public string JobNum { get; set; } = null!;

    /// <summary>
    /// 入职日期
    /// </summary>
    public long? JoinTime { get; set; }

    /// <summary>
    /// 角色集合
    /// </summary>
    public List<long> RoleIdList { get; set; } = null!;

    /// <summary>
    /// 扩展机构集合
    /// </summary>
    public List<TenantUserExtensionOrganizationEntity> ExtOrganizationIdList { get; set; } = null!;
}