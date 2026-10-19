namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// 错误码段登记表：ADR 2026-08-20「统一错误码体系」分段表的可执行形态。
/// 新模块必须先在此登记段，再写错误码成员（ErrorCodeContractTests 强制守护）。
/// </summary>
public static class ErrorSegmentRegistry
{
    public static readonly IReadOnlyList<ErrorSegment> Entries = new ErrorSegment[]
    {
        new(0, ErrorDomain.None, "Envelope", null, false, "信封保留：成功码恒为 0，任何枚举成员不得使用"),
        new(100, ErrorDomain.System, "System", nameof(SystemErrorCode), false, "系统域：内部错误"),
        new(101, ErrorDomain.System, "RateLimit", null, false, "系统域预留：限流降级"),
        new(200, ErrorDomain.Protocol, "Common", nameof(CommonErrorCode), false, "协议域：通用错误（资源不存在、参数校验失败）"),
        new(201, ErrorDomain.Protocol, "Auth", nameof(AuthErrorCode), false, "协议域：认证授权（令牌、未授权、签名验证）"),
        new(202, ErrorDomain.Protocol, "Protocol", nameof(ProtocolErrorCode), false, "协议域：协议解析错误"),
        // Admin 业务域 3xx-5xx（C265 / GFX-671 自旧体系 121 个 D 码按服务域归段平移，一段一模块）。
        new(300, ErrorDomain.AdminBusiness, "SystemUser", nameof(SystemUserErrorCode), false, "Admin 系统用户与认证"),
        new(301, ErrorDomain.AdminBusiness, "RoleMenu", nameof(RoleMenuErrorCode), false, "Admin 角色与菜单"),
        new(302, ErrorDomain.AdminBusiness, "Dictionary", nameof(DictionaryErrorCode), false, "Admin 字典"),
        new(303, ErrorDomain.AdminBusiness, "PayConfig", null, false, "Admin 支付配置（预留：暂无错误码成员）"),
        new(304, ErrorDomain.AdminBusiness, "Wechat", null, false, "Admin 微信生态配置（预留：暂无错误码成员）"),
        new(305, ErrorDomain.AdminBusiness, "PlatformConfig", nameof(PlatformConfigErrorCode), false, "Admin 平台配置"),
        new(306, ErrorDomain.AdminBusiness, "Tenant", nameof(TenantErrorCode), false, "Admin 租户管理"),
        new(307, ErrorDomain.AdminBusiness, "Organization", nameof(OrganizationErrorCode), false, "Admin 组织架构"),
        new(308, ErrorDomain.AdminBusiness, "Region", nameof(RegionErrorCode), false, "Admin 区域"),
        new(309, ErrorDomain.AdminBusiness, "Position", nameof(PositionErrorCode), false, "Admin 职位"),
        new(310, ErrorDomain.AdminBusiness, "File", nameof(FileErrorCode), false, "Admin 文件"),
        new(311, ErrorDomain.AdminBusiness, "Job", nameof(JobErrorCode), false, "Admin 任务调度"),
        new(312, ErrorDomain.AdminBusiness, "Database", nameof(DatabaseErrorCode), false, "Admin 代码生成数据库表"),
        new(313, ErrorDomain.AdminBusiness, "OpenAccess", nameof(OpenAccessErrorCode), false, "Admin 开放访问"),
        new(314, ErrorDomain.AdminBusiness, "MailCampaign", nameof(MailCampaignErrorCode), false, "Admin 运营邮件 Campaign"),
        new(315, ErrorDomain.AdminBusiness, "Print", nameof(PrintErrorCode), false, "Admin 打印模板"),
        new(316, ErrorDomain.AdminBusiness, "Plugin", nameof(PluginErrorCode), false, "Admin 动态插件"),
        new(317, ErrorDomain.AdminBusiness, "OnlineModeration", nameof(OnlineModerationErrorCode), false, "Admin Online 社交治理（消息审计/举报处置/处罚联动，C350）"),
        // Online 平台 8 子段（C339 / vault:C2 S1.3，一一镜像 vault 1xxx～8xxx 八段；协议段 Nxxx ↔ 镜像段 32(N-1)）。
        new(320, ErrorDomain.AdminBusiness, "OnlineSystem", nameof(OnlineErrorCode), false, "Online 镜像：系统与网络（vault 1xxx）"),
        new(321, ErrorDomain.AdminBusiness, "OnlineSession", nameof(OnlineErrorCode), false, "Online 镜像：鉴权与 Session（vault 2xxx）"),
        new(322, ErrorDomain.AdminBusiness, "OnlineScope", nameof(OnlineErrorCode), false, "Online 镜像：租户/App/Server 作用域（vault 3xxx）"),
        new(323, ErrorDomain.AdminBusiness, "OnlineParam", nameof(OnlineErrorCode), false, "Online 镜像：参数与资源（vault 4xxx）"),
        new(324, ErrorDomain.AdminBusiness, "OnlineBusiness", nameof(OnlineErrorCode), false, "Online 镜像：业务状态（vault 5xxx）"),
        new(325, ErrorDomain.AdminBusiness, "OnlineIdempotency", nameof(OnlineErrorCode), false, "Online 镜像：幂等与冲突（vault 6xxx）"),
        new(326, ErrorDomain.AdminBusiness, "OnlineRateLimit", nameof(OnlineErrorCode), false, "Online 镜像：限流与风控（vault 7xxx）"),
        new(327, ErrorDomain.AdminBusiness, "OnlineTransient", nameof(OnlineErrorCode), false, "Online 镜像：临时失败与可重试（vault 8xxx）"),
        new(601, ErrorDomain.GameData, "Account", nameof(AccountErrorCode), false, "游戏数据域：账号"),
        new(602, ErrorDomain.GameData, "GameRole", nameof(GameRoleErrorCode), false, "游戏数据域：角色"),
        new(603, ErrorDomain.GameData, "Area", nameof(AreaErrorCode), false, "游戏数据域：区服"),
        new(604, ErrorDomain.GameData, "Archive", nameof(ArchiveErrorCode), false, "游戏数据域：存档"),
        new(605, ErrorDomain.GameData, "GameHome", nameof(GameHomeErrorCode), false, "游戏数据域：首页运营驾驶舱"),
        new(701, ErrorDomain.GameTrade, "Order", nameof(OrderErrorCode), false, "游戏交易域：订单与商城"),
        new(703, ErrorDomain.GameTrade, "RedeemCode", nameof(RedeemCodeErrorCode), false, "游戏交易域：兑换码"),
        new(704, ErrorDomain.GameTrade, "Payment", nameof(PaymentErrorCode), false, "游戏交易域：支付渠道"),
    };

    /// <summary>
    /// 按段号查找登记条目；未登记返回 null（契约测试据此判定违规）。
    /// </summary>
    public static ErrorSegment? Find(int segment)
    {
        foreach (var entry in Entries)
        {
            if (entry.Segment == segment)
            {
                return entry;
            }
        }

        return null;
    }

    /// <summary>
    /// 判断段号首位数字是否落在指定大域的合法首位区间。
    /// </summary>
    public static bool IsLeadingDigitInDomain(int segment, ErrorDomain domain)
    {
        var leading = segment / 100;
        return domain switch
        {
            ErrorDomain.None          => leading == 0,
            ErrorDomain.System        => leading == 1,
            ErrorDomain.Protocol      => leading == 2,
            ErrorDomain.AdminBusiness => leading >= 3 && leading <= 5,
            ErrorDomain.GameData      => leading == 6,
            ErrorDomain.GameTrade     => leading == 7,
            ErrorDomain.Reserved      => leading == 8 || leading == 9,
            _                         => false,
        };
    }
}