using GameFrameX.Entity.Enum;
using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Entity.System;

/// <summary>
/// 用户字段基类，仅提供 system_user 与 tenant_user 共享字段，不直接映射物理表。
/// </summary>
public abstract class SystemUserBase : EntityTenantBase
{
    /// <summary>
    /// 账号
    /// </summary>
    [SugarColumn(ColumnDescription = "账号", Length = EntityStringLength.Length256)]
    public virtual string Account { get; set; } = null!;

    /// <summary>
    /// 密码
    /// </summary>
    [SugarColumn(ColumnDescription = "密码", Length = EntityStringLength.Length1024)]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public virtual string Password { get; set; } = null!;

    /// <summary>
    /// 真实姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "真实姓名", Length = EntityStringLength.Length256)]
    public virtual string RealName { get; set; } = null!;

    /// <summary>
    /// 昵称
    /// </summary>
    [SugarColumn(ColumnDescription = "昵称", Length = EntityStringLength.Length256)]
    public string NickName { get; set; } = null!;

    /// <summary>
    /// 头像
    /// </summary>
    [SugarColumn(ColumnDescription = "头像", Length = EntityStringLength.Length1024)]
    public string Avatar { get; set; } = null!;

    /// <summary>
    /// 性别-男_1、女_2
    /// </summary>
    [SugarColumn(ColumnDescription = "性别", DefaultValue = "0")]
    public GenderEnum Sex { get; set; } = GenderEnum.Male;

    /// <summary>
    /// 年龄
    /// </summary>
    [SugarColumn(ColumnDescription = "年龄", DefaultValue = "0")]
    public int Age { get; set; }

    /// <summary>
    /// 出生日期
    /// </summary>
    [SugarColumn(ColumnDescription = "出生日期")]
    public long? BirthdayTime { get; set; }

    /// <summary>
    /// 民族
    /// </summary>
    [SugarColumn(ColumnDescription = "民族", Length = EntityStringLength.Length256)]
    public string Nation { get; set; } = null!;

    /// <summary>
    /// 手机号码
    /// </summary>
    [SugarColumn(ColumnDescription = "手机号码", Length = EntityStringLength.Length256)]
    public string Phone { get; set; } = null!;

    /// <summary>
    /// 证件类型
    /// </summary>
    [SugarColumn(ColumnDescription = "证件类型", DefaultValue = "0")]
    public CardTypeEnum CardType { get; set; }

    /// <summary>
    /// 身份证号
    /// </summary>
    [SugarColumn(ColumnDescription = "身份证号", Length = EntityStringLength.Length256)]
    public string IdCardNum { get; set; } = null!;

    /// <summary>
    /// 邮箱
    /// </summary>
    [SugarColumn(ColumnDescription = "邮箱", Length = EntityStringLength.Length256)]
    public string Email { get; set; } = null!;

    /// <summary>
    /// 地址
    /// </summary>
    [SugarColumn(ColumnDescription = "地址", Length = EntityStringLength.Length512)]
    public string Address { get; set; } = null!;

    /// <summary>
    /// 文化程度
    /// </summary>
    [SugarColumn(ColumnDescription = "文化程度", DefaultValue = "0")]
    public CultureLevelEnum CultureLevel { get; set; }

    /// <summary>
    /// 政治面貌
    /// </summary>
    [SugarColumn(ColumnDescription = "政治面貌", Length = EntityStringLength.Length256)]
    public string PoliticalOutlook { get; set; } = null!;

    /// <summary>
    /// 毕业院校
    /// </summary>COLLEGE
    [SugarColumn(ColumnDescription = "毕业院校", Length = EntityStringLength.Length256)]
    public string College { get; set; } = null!;

    /// <summary>
    /// 办公电话
    /// </summary>
    [SugarColumn(ColumnDescription = "办公电话", Length = EntityStringLength.Length256)]
    public string OfficePhone { get; set; } = null!;

    /// <summary>
    /// 紧急联系人
    /// </summary>
    [SugarColumn(ColumnDescription = "紧急联系人", Length = EntityStringLength.Length256)]
    public string EmergencyContact { get; set; } = null!;

    /// <summary>
    /// 紧急联系人电话
    /// </summary>
    [SugarColumn(ColumnDescription = "紧急联系人电话", Length = EntityStringLength.Length256)]
    public string EmergencyPhone { get; set; } = null!;

    /// <summary>
    /// 紧急联系人地址
    /// </summary>
    [SugarColumn(ColumnDescription = "紧急联系人地址", Length = EntityStringLength.Length512)]
    public string EmergencyAddress { get; set; } = null!;

    /// <summary>
    /// 个人简介
    /// </summary>
    [SugarColumn(ColumnDescription = "个人简介", Length = EntityStringLength.Length1024)]
    public string Introduction { get; set; } = null!;

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序", DefaultValue = "0")]
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = EntityStringLength.Length512)]
    public string Remark { get; set; } = null!;

    /// <summary>
    /// 账号类型
    /// </summary>
    [SugarColumn(ColumnDescription = "账号类型", DefaultValue = "0")]
    public AccountTypeEnum AccountType { get; set; } = AccountTypeEnum.NormalUser;

    /// <summary>
    /// 直属机构Id
    /// </summary>
    [SugarColumn(ColumnDescription = "直属机构Id", DefaultValue = "0")]
    public long OrganizationId { get; set; }

    /// <summary>
    /// 直属机构
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(OrganizationId))]
    public TenantOrganizationEntity TenantOrganizationEntity { get; set; } = null!;

    /// <summary>
    /// 直属主管Id
    /// </summary>
    [SugarColumn(ColumnDescription = "直属主管Id")]
    public long? ManagerUserId { get; set; }

    /// <summary>
    /// 直属主管
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(ManagerUserId))]
    public TenantUserEntity ManagerUser { get; set; } = null!;

    /// <summary>
    /// 职位Id
    /// </summary>
    [SugarColumn(ColumnDescription = "职位Id", DefaultValue = "0")]
    public long PosId { get; set; }

    /// <summary>
    /// 职位
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(PosId))]
    public TenantPositionEntity TenantPositionEntity { get; set; } = null!;

    /// <summary>
    /// 工号
    /// </summary>
    [SugarColumn(ColumnDescription = "工号", Length = EntityStringLength.Length256)]
    public string JobNum { get; set; } = null!;

    /// <summary>
    /// 职级
    /// </summary>
    [SugarColumn(ColumnDescription = "职级", Length = EntityStringLength.Length256)]
    public string PosLevel { get; set; } = null!;

    /// <summary>
    /// 职称
    /// </summary>
    [SugarColumn(ColumnDescription = "职称", Length = EntityStringLength.Length256)]
    public string PosTitle { get; set; } = null!;

    /// <summary>
    /// 擅长领域
    /// </summary>
    [SugarColumn(ColumnDescription = "擅长领域", Length = EntityStringLength.Length256)]
    public string Expertise { get; set; } = null!;

    /// <summary>
    /// 办公区域
    /// </summary>
    [SugarColumn(ColumnDescription = "办公区域", Length = EntityStringLength.Length256)]
    public string OfficeZone { get; set; } = null!;

    /// <summary>
    /// 办公室
    /// </summary>
    [SugarColumn(ColumnDescription = "办公室", Length = EntityStringLength.Length256)]
    public string Office { get; set; } = null!;

    /// <summary>
    /// 入职日期
    /// </summary>
    [SugarColumn(ColumnDescription = "入职日期")]
    public long? JoinTime { get; set; }

    /// <summary>
    /// 最新登录Ip
    /// </summary>
    [SugarColumn(ColumnDescription = "最新登录Ip", Length = EntityStringLength.Length512)]
    public string LastLoginIp { get; set; } = null!;

    /// <summary>
    /// 最新登录地点
    /// </summary>
    [SugarColumn(ColumnDescription = "最新登录地点", Length = EntityStringLength.Length256)]
    public string LastLoginAddress { get; set; } = null!;

    /// <summary>
    /// 最新登录时间
    /// </summary>
    [SugarColumn(ColumnDescription = "最新登录时间")]
    public long? LastLoginTime { get; set; }

    /// <summary>
    /// 最新登录设备
    /// </summary>
    [SugarColumn(ColumnDescription = "最新登录设备", Length = EntityStringLength.Length256)]
    public string LastLoginDevice { get; set; } = null!;

    /// <summary>
    /// 电子签名
    /// </summary>
    [SugarColumn(ColumnDescription = "电子签名", Length = EntityStringLength.Length1024)]
    public string Signature { get; set; } = null!;
}