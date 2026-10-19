/* eslint-disable */
/* tslint:disable */

/** ① 应用身份凭证卡片：AppSecret 配置态与最近轮换时间。 */
export interface IdentityCardResponse {
  /** AppSecret 是否已配置（以 AppSecretHash 非空为准；C371 后明文列清零，仅 hash 验证）。 */
  appSecretConfigured?: boolean;
  /**
   * AppSecret 最近一次重置时间（未重置过为 null）。
   * @format int64
   */
  appSecretLastRotatedTime?: number | null;
  /** 是否强制请求签名（SignatureRequired 开关态；enforcement 口径见 client-api spec）。 */
  signatureRequired?: boolean;
}

/** ④ 出站对接凭证条目：作用域命中态 + 启用态 + 优先级 + 配置页路由（供前端跳转深度配置页）。 */
export interface OutboundConfigResponse {
  /** 凭证成员标识：payment / email / sms / wechat / adCallback / objectStorage。 */
  kind?: string | null;
  /** 展示标题（支付 / 邮件 / SMS / 微信 / 广告回调 / 对象存储）。 */
  title?: string | null;
  /** 作用域命中态：app / tenant-default / tenant（租户级配置）/ none。 */
  scope?: string | null;
  /**
   * 命中配置行数（0 = 未配置）。
   * @format int32
   */
  configuredCount?: number;
  /** 命中行中是否存在启用行。 */
  enabled?: boolean;
  /**
   * 命中行最小 Priority（值小者优先；实体无优先级字段时为 null）。
   * @format int32
   */
  priority?: number | null;
  /** 深度配置页前端路由（保留为深度配置页，凭证中心仅展示状态 + 跳转）。 */
  routePath?: string | null;
}

/**
 * 凭证中心聚合概览响应（C374，U14 四类凭证分类法的 per-app 状态面）。
 *
 * <b>零密钥材料不变量</b>：全图仅承载状态（bool）/ 计数（int）/ 时间戳（long?）/ 描述性元数据（路由、口径说明），
 * 不含任何密钥明文、密文、hash 或 PEM 片段；`TenantAppCredentialOverviewTests` 以反射扫描守护本不变量
 * （字符串属性名不得命中敏感词，任何属性不得为 byte[]）。
 * 用途矩阵为后端常量输出（M:GameFrameX.Application.Game.App.CredentialCenter.Dto.TenantAppCredentialOverviewResponse.BuildUsageMatrix，单一事实来源，前端不再硬编码分类口径）。
 */
export interface TenantAppCredentialOverviewResponse {
  /** 目标应用 Id。 */
  appId?: string;
  /** 目标应用名称。 */
  appName?: string | null;
  /** ① 应用身份凭证卡片：AppSecret 配置态与最近轮换时间。 */
  identity?: IdentityCardResponse;
  /**
   * ② 传输签名凭证卡片：HMAC 密钥环 + 应用开放访问凭证。
   *
   * 「可用」口径与验签一致：启用 + 未删 + 已生效（NotBefore ≤ now）+ 未到期（Expire 为空或 > now）；
   * RotatedTime 为纯元数据（C373 租约模型：已标记旧钥在退场时刻前仍可验签），不参与排除。
   */
  transport?: TransportCardResponse;
  /** ③ 票据与令牌密钥卡片：RSA 票据密钥 + Client API 策略（C378 合并表）+ 会话密钥。 */
  ticketToken?: TicketTokenCardResponse;
  /** ④ 出站对接凭证条目（支付/邮件/SMS/微信/广告回调/对象存储各一行）。 */
  outbound?: OutboundConfigResponse[] | null;
  /** 用途矩阵（类别 → 形态 → 消费方 → 操作集），后端常量输出。 */
  usageMatrix?: UsageMatrixResponse[] | null;
}

/** ③ 票据与令牌密钥卡片：RSA 票据密钥 + Client API 策略（C378 合并表）+ 会话密钥。 */
export interface TicketTokenCardResponse {
  /**
   * 票据密钥可用数（口径同传输签名：启用 + 已生效 + 未到期）。
   * @format int32
   */
  ticketKeyActiveCount?: number;
  /**
   * 票据密钥最近到期时间（全部永不过期为 null）。
   * @format int64
   */
  ticketKeyNearestExpireTime?: number | null;
  /** 票据密钥自动建钥策略态（Client API 策略 EnableKeyAutoCreate）。 */
  ticketKeyAutoCreateEnabled?: boolean;
  /**
   * 令牌有效期分钟数（Client API 策略 LifetimeMinutes；未配置策略为 0）。
   * @format int32
   */
  tokenLifetimeMinutes?: number;
  /** 令牌签名密钥是否已配置（策略 SigningKey 非空即视为已配置，不出密钥本体）。 */
  tokenSigningKeyConfigured?: boolean;
  /** 命中的 Client API 策略作用域：app（应用行）/ tenant-default（租户默认行）/ none（未配置）。 */
  policyScope?: string | null;
  /**
   * 会话密钥活跃数（口径同传输签名：启用 + 已生效 + 未到期）。
   * @format int32
   */
  sessionKeyActiveCount?: number;
}

/**
 * ② 传输签名凭证卡片：HMAC 密钥环 + 应用开放访问凭证。
 *
 * 「可用」口径与验签一致：启用 + 未删 + 已生效（NotBefore ≤ now）+ 未到期（Expire 为空或 > now）；
 * RotatedTime 为纯元数据（C373 租约模型：已标记旧钥在退场时刻前仍可验签），不参与排除。
 */
export interface TransportCardResponse {
  /** HMAC 密钥环最新生效钥的 kid（无生效钥为 null）。 */
  latestHmacKid?: string | null;
  /**
   * HMAC 密钥环可用钥数。
   * @format int32
   */
  hmacActiveKeyCount?: number;
  /**
   * HMAC 密钥环最近到期时间（全部永不过期为 null；临期提示由前端基于本值展示）。
   * @format int64
   */
  hmacNearestExpireTime?: number | null;
  /**
   * 应用开放访问凭证活跃数（未吊销 + 未到期 + 启用）。
   * @format int32
   */
  openAccessActiveCount?: number;
  /**
   * 开放访问凭证最近轮换时间（Refresh 就地更新 AccessSecret 取最近 UpdateTime；无凭证为 null）。
   * @format int64
   */
  openAccessLastRotatedTime?: number | null;
}

/** 用途矩阵条目（U14 §1 四类凭证分类法，后端常量单一事实来源）。 */
export interface UsageMatrixResponse {
  /** 类别标识：identity / transport / ticketToken / outbound。 */
  category?: string | null;
  /** 类别标题（① 应用身份凭证 … ④ 出站对接凭证）。 */
  categoryTitle?: string | null;
  /** 成员标识（appSecret / hmacKeyRing / openAccess / ticketKey / tokenSigningKey / sessionKey / payment / email / sms / wechat / adCallback / objectStorage）。 */
  member?: string | null;
  /** 成员标题。 */
  memberTitle?: string | null;
  /** 形态/算法（对称 hash / HMAC 对称 / RSA 非对称 / 出站凭证 / 入站验签）。 */
  algorithmForm?: string | null;
  /** 验证/消费方。 */
  consumers?: string | null;
  /** 所需操作（生命周期操作集 / 深度配置页指引）。 */
  operations?: string | null;
}

export interface TenantAppCredentialOverviewGetGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /**
   * 凭证中心聚合概览响应（C374，U14 四类凭证分类法的 per-app 状态面）。
   *
   * <b>零密钥材料不变量</b>：全图仅承载状态（bool）/ 计数（int）/ 时间戳（long?）/ 描述性元数据（路由、口径说明），
   * 不含任何密钥明文、密文、hash 或 PEM 片段；`TenantAppCredentialOverviewTests` 以反射扫描守护本不变量
   * （字符串属性名不得命中敏感词，任何属性不得为 byte[]）。
   * 用途矩阵为后端常量输出（M:GameFrameX.Application.Game.App.CredentialCenter.Dto.TenantAppCredentialOverviewResponse.BuildUsageMatrix，单一事实来源，前端不再硬编码分类口径）。
   */
  data: TenantAppCredentialOverviewResponse;
  /** 链路追踪标识，未启用追踪时为 null */
  trackId: string | null;
  /** 已退役字段：统一错误码体系（C265）下恒为 null，数字业务错误码承载在 code 字段 */
  errorCode: string | null;
  /** 响应类型（success/warning/error） */
  type: string | null;
  /**
   * Unix 时间戳（秒）
   * @format int64
   */
  time: number;
  extras: object | null;
}
