using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Service.User.Dto;

/// <summary>
/// 租户用户输出参数
/// </summary>
public class TenantUserResponse
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    public string Account { get; set; } = null!;

    /// <summary>
    /// 真实姓名
    /// </summary>
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
    /// 个人简介
    /// </summary>
    public string Introduction { get; set; } = null!;

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
    /// 直属主管Id
    /// </summary>
    public long? ManagerUserId { get; set; }

    /// <summary>
    /// 职位Id
    /// </summary>
    public long PosId { get; set; }

    /// <summary>
    /// 工号
    /// </summary>
    public string JobNum { get; set; } = null!;

    /// <summary>
    /// 职级
    /// </summary>
    public string PosLevel { get; set; } = null!;

    /// <summary>
    /// 职称
    /// </summary>
    public string PosTitle { get; set; } = null!;

    /// <summary>
    /// 擅长领域
    /// </summary>
    public string Expertise { get; set; } = null!;

    /// <summary>
    /// 办公区域
    /// </summary>
    public string OfficeZone { get; set; } = null!;

    /// <summary>
    /// 办公室
    /// </summary>
    public string Office { get; set; } = null!;

    /// <summary>
    /// 入职日期
    /// </summary>
    public long? JoinTime { get; set; }

    /// <summary>
    /// 最新登录Ip
    /// </summary>
    public string LastLoginIp { get; set; } = null!;

    /// <summary>
    /// 最新登录地点
    /// </summary>
    public string LastLoginAddress { get; set; } = null!;

    /// <summary>
    /// 最新登录时间
    /// </summary>
    public long? LastLoginTime { get; set; }

    /// <summary>
    /// 最新登录设备
    /// </summary>
    public string LastLoginDevice { get; set; } = null!;

    /// <summary>
    /// 电子签名
    /// </summary>
    public string Signature { get; set; } = null!;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreatedName { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedName { get; set; }

    /// <summary>
    /// 行版本号（乐观锁）
    /// </summary>
    public long? RowVersion { get; set; }
}