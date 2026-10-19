namespace GameFrameX.Application.Game.App.CredentialCenter.Dto;

/// <summary>
/// 凭证中心聚合概览响应（C374，U14 四类凭证分类法的 per-app 状态面）。
/// <para>
/// <b>零密钥材料不变量</b>：全图仅承载状态（bool）/ 计数（int）/ 时间戳（long?）/ 描述性元数据（路由、口径说明），
/// 不含任何密钥明文、密文、hash 或 PEM 片段；<c>TenantAppCredentialOverviewTests</c> 以反射扫描守护本不变量
/// （字符串属性名不得命中敏感词，任何属性不得为 byte[]）。
/// </para>
/// <para>用途矩阵为后端常量输出（<see cref="BuildUsageMatrix"/>，单一事实来源，前端不再硬编码分类口径）。</para>
/// </summary>
public class TenantAppCredentialOverviewResponse
{
    /// <summary>作用域常量：应用级行命中。</summary>
    public const string ScopeApp = "app";

    /// <summary>作用域常量：租户默认行（AppId=0）命中。</summary>
    public const string ScopeTenantDefault = "tenant-default";

    /// <summary>作用域常量：租户级配置（无应用维度，如对象存储）。</summary>
    public const string ScopeTenant = "tenant";

    /// <summary>作用域常量：未配置。</summary>
    public const string ScopeNone = "none";

    /// <summary>目标应用 Id。</summary>
    public long AppId { get; set; }

    /// <summary>目标应用名称。</summary>
    public string AppName { get; set; } = string.Empty;

    /// <summary>① 应用身份凭证卡片。</summary>
    public IdentityCardResponse Identity { get; set; } = new();

    /// <summary>② 传输签名凭证卡片。</summary>
    public TransportCardResponse Transport { get; set; } = new();

    /// <summary>③ 票据与令牌密钥卡片。</summary>
    public TicketTokenCardResponse TicketToken { get; set; } = new();

    /// <summary>④ 出站对接凭证条目（支付/邮件/SMS/微信/广告回调/对象存储各一行）。</summary>
    public List<OutboundConfigResponse> Outbound { get; set; } = new();

    /// <summary>用途矩阵（类别 → 形态 → 消费方 → 操作集），后端常量输出。</summary>
    public List<UsageMatrixResponse> UsageMatrix { get; set; } = new();

    /// <summary>
    /// 构建用途矩阵（U14 §1 四类凭证分类法，12 个成员；系统开放访问凭证属主库系统级，不在 per-app 概览范围）。
    /// </summary>
    public static List<UsageMatrixResponse> BuildUsageMatrix()
    {
        return new List<UsageMatrixResponse>
        {
            new() { Category = "identity", CategoryTitle = "应用身份凭证", Member = "appSecret",
                    MemberTitle = "AppSecret（App-Id + App-Secret 双头）", AlgorithmForm = "对称 hash（PBKDF2 懒升级）",
                    Consumers = "Client.Api AppResolutionMiddleware", Operations = "重置（应用管理页，旧值即刻作废）", },
            new() { Category = "transport", CategoryTitle = "传输签名凭证", Member = "hmacKeyRing",
                    MemberTitle = "HMAC 传输签名密钥环", AlgorithmForm = "HMAC 对称（timestamp+nonce+sign，防篡改+防重放）",
                    Consumers = "Client.Api SignatureValidationMiddleware", Operations = "轮换（旧钥租约退场）/ 续期 / 启用 / 禁用 / 一次性回显", },
            new() { Category = "transport", CategoryTitle = "传输签名凭证", Member = "openAccess",
                    MemberTitle = "应用开放访问凭证（AccessKey/AccessSecret）", AlgorithmForm = "HMAC 请求签名",
                    Consumers = "Hub.Api GameAppOpenAccessAuthMiddleware", Operations = "轮换（Refresh）/ 启用 / 禁用 / 吊销", },
            new() { Category = "ticketToken", CategoryTitle = "票据与令牌密钥", Member = "ticketKey",
                    MemberTitle = "登录票据密钥（RSA，Server 仅持公钥）", AlgorithmForm = "RSA 非对称",
                    Consumers = "GameFrameX.Server 公钥拉取", Operations = "轮换（旧钥租约退场）/ 启用 / 禁用", },
            new() { Category = "ticketToken", CategoryTitle = "票据与令牌密钥", Member = "tokenSigningKey",
                    MemberTitle = "令牌签名密钥（Client API 策略）", AlgorithmForm = "HMAC 对称（服务端内部）",
                    Consumers = "ClientApiTokenIssuer", Operations = "轮换（旧值即刻作废，响应不回显）", },
            new() { Category = "ticketToken", CategoryTitle = "票据与令牌密钥", Member = "sessionKey",
                    MemberTitle = "会话密钥", AlgorithmForm = "HMAC 对称",
                    Consumers = "游戏会话", Operations = "轮换 / 续期 / 启用 / 禁用", },
            new() { Category = "outbound", CategoryTitle = "出站对接凭证", Member = "payment",
                    MemberTitle = "支付凭证", AlgorithmForm = "出站凭证（Provider ConfigJson）",
                    Consumers = "Payment Provider 适配层", Operations = "支付配置页维护", },
            new() { Category = "outbound", CategoryTitle = "出站对接凭证", Member = "email",
                    MemberTitle = "邮件凭证", AlgorithmForm = "出站凭证",
                    Consumers = "邮件 Sender", Operations = "邮件配置页维护", },
            new() { Category = "outbound", CategoryTitle = "出站对接凭证", Member = "sms",
                    MemberTitle = "SMS 凭证", AlgorithmForm = "出站凭证",
                    Consumers = "SMS Sender", Operations = "SMS 配置页维护", },
            new() { Category = "outbound", CategoryTitle = "出站对接凭证", Member = "wechat",
                    MemberTitle = "微信凭证", AlgorithmForm = "出站凭证",
                    Consumers = "微信对接", Operations = "微信配置页维护", },
            new() { Category = "outbound", CategoryTitle = "出站对接凭证", Member = "adCallback",
                    MemberTitle = "广告回调验签", AlgorithmForm = "入站验签",
                    Consumers = "广告回调验签器", Operations = "广告回调配置页维护", },
            new() { Category = "outbound", CategoryTitle = "出站对接凭证", Member = "objectStorage",
                    MemberTitle = "对象存储凭证", AlgorithmForm = "出站凭证",
                    Consumers = "对象存储上传", Operations = "对象存储配置页维护", },
        };
    }

    /// <summary>① 应用身份凭证卡片：AppSecret 配置态与最近轮换时间。</summary>
    public class IdentityCardResponse
    {
        /// <summary>AppSecret 是否已配置（以 AppSecretHash 非空为准；C371 后明文列清零，仅 hash 验证）。</summary>
        public bool AppSecretConfigured { get; set; }

        /// <summary>AppSecret 最近一次重置时间（未重置过为 null）。</summary>
        public long? AppSecretLastRotatedTime { get; set; }

        /// <summary>是否强制请求签名（SignatureRequired 开关态；enforcement 口径见 client-api spec）。</summary>
        public bool SignatureRequired { get; set; }
    }

    /// <summary>
    /// ② 传输签名凭证卡片：HMAC 密钥环 + 应用开放访问凭证。
    /// <para>「可用」口径与验签一致：启用 + 未删 + 已生效（NotBefore ≤ now）+ 未到期（Expire 为空或 &gt; now）；
    /// RotatedTime 为纯元数据（C373 租约模型：已标记旧钥在退场时刻前仍可验签），不参与排除。</para>
    /// </summary>
    public class TransportCardResponse
    {
        /// <summary>HMAC 密钥环最新生效钥的 kid（无生效钥为 null）。</summary>
        public string? LatestHmacKid { get; set; }

        /// <summary>HMAC 密钥环可用钥数。</summary>
        public int HmacActiveKeyCount { get; set; }

        /// <summary>HMAC 密钥环最近到期时间（全部永不过期为 null；临期提示由前端基于本值展示）。</summary>
        public long? HmacNearestExpireTime { get; set; }

        /// <summary>应用开放访问凭证活跃数（未吊销 + 未到期 + 启用）。</summary>
        public int OpenAccessActiveCount { get; set; }

        /// <summary>开放访问凭证最近轮换时间（Refresh 就地更新 AccessSecret 取最近 UpdateTime；无凭证为 null）。</summary>
        public long? OpenAccessLastRotatedTime { get; set; }
    }

    /// <summary>③ 票据与令牌密钥卡片：RSA 票据密钥 + Client API 策略（C378 合并表）+ 会话密钥。</summary>
    public class TicketTokenCardResponse
    {
        /// <summary>票据密钥可用数（口径同传输签名：启用 + 已生效 + 未到期）。</summary>
        public int TicketKeyActiveCount { get; set; }

        /// <summary>票据密钥最近到期时间（全部永不过期为 null）。</summary>
        public long? TicketKeyNearestExpireTime { get; set; }

        /// <summary>票据密钥自动建钥策略态（Client API 策略 EnableKeyAutoCreate）。</summary>
        public bool TicketKeyAutoCreateEnabled { get; set; }

        /// <summary>令牌有效期分钟数（Client API 策略 LifetimeMinutes；未配置策略为 0）。</summary>
        public int TokenLifetimeMinutes { get; set; }

        /// <summary>令牌签名密钥是否已配置（策略 SigningKey 非空即视为已配置，不出密钥本体）。</summary>
        public bool TokenSigningKeyConfigured { get; set; }

        /// <summary>命中的 Client API 策略作用域：app（应用行）/ tenant-default（租户默认行）/ none（未配置）。</summary>
        public string PolicyScope { get; set; } = ScopeNone;

        /// <summary>会话密钥活跃数（口径同传输签名：启用 + 已生效 + 未到期）。</summary>
        public int SessionKeyActiveCount { get; set; }
    }

    /// <summary>
    /// ④ 出站对接凭证条目：作用域命中态 + 启用态 + 优先级 + 配置页路由（供前端跳转深度配置页）。
    /// </summary>
    public class OutboundConfigResponse
    {
        /// <summary>凭证成员标识：payment / email / sms / wechat / adCallback / objectStorage。</summary>
        public string Kind { get; set; } = string.Empty;

        /// <summary>展示标题（支付 / 邮件 / SMS / 微信 / 广告回调 / 对象存储）。</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>作用域命中态：app / tenant-default / tenant（租户级配置）/ none。</summary>
        public string Scope { get; set; } = ScopeNone;

        /// <summary>命中配置行数（0 = 未配置）。</summary>
        public int ConfiguredCount { get; set; }

        /// <summary>命中行中是否存在启用行。</summary>
        public bool Enabled { get; set; }

        /// <summary>命中行最小 Priority（值小者优先；实体无优先级字段时为 null）。</summary>
        public int? Priority { get; set; }

        /// <summary>深度配置页前端路由（保留为深度配置页，凭证中心仅展示状态 + 跳转）。</summary>
        public string RoutePath { get; set; } = string.Empty;
    }

    /// <summary>用途矩阵条目（U14 §1 四类凭证分类法，后端常量单一事实来源）。</summary>
    public class UsageMatrixResponse
    {
        /// <summary>类别标识：identity / transport / ticketToken / outbound。</summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>类别标题（① 应用身份凭证 … ④ 出站对接凭证）。</summary>
        public string CategoryTitle { get; set; } = string.Empty;

        /// <summary>成员标识（appSecret / hmacKeyRing / openAccess / ticketKey / tokenSigningKey / sessionKey / payment / email / sms / wechat / adCallback / objectStorage）。</summary>
        public string Member { get; set; } = string.Empty;

        /// <summary>成员标题。</summary>
        public string MemberTitle { get; set; } = string.Empty;

        /// <summary>形态/算法（对称 hash / HMAC 对称 / RSA 非对称 / 出站凭证 / 入站验签）。</summary>
        public string AlgorithmForm { get; set; } = string.Empty;

        /// <summary>验证/消费方。</summary>
        public string Consumers { get; set; } = string.Empty;

        /// <summary>所需操作（生命周期操作集 / 深度配置页指引）。</summary>
        public string Operations { get; set; } = string.Empty;
    }
}
