/* eslint-disable */
/* tslint:disable */

/**
 * 资产流水行（vault:C4 账本不变量字段：TransactionId / Player / App / Server / 来源 / 原因 / 业务单号 / 操作者 / 前后值 / 时间；
 * App/Server 由查询作用域隐含，不在行内重复）。线缆与 Admin 响应共用形态（账本行无脱敏面）。
 */
export interface OnlineAssetLedgerEntryResponse {
  /** 交易 Id（一笔业务操作 = 一个 TransactionId，账本行经其反查交易详情）。 */
  transactionId?: string | null;
  /** 玩家 Id。 */
  playerId?: string | null;
  /** 货币或道具类型（如 gold / diamond / item:1001）。 */
  currencyOrItemType?: string | null;
  /**
   * 变更前值（该资产维度的余额/库存快照）。
   * @format int64
   */
  beforeValue?: number;
  /**
   * 变更后值。
   * @format int64
   */
  afterValue?: number;
  /** 来源（Admin 人工 / 支付 / 邮件 / 兑换码 / 对局 / 活动，vault:C4 六类）。 */
  source?: string | null;
  /** 原因（进审计的操作理由原文）。 */
  reason?: string | null;
  /** 业务单号（幂等键载体，同单号重试只生效一次）。 */
  businessOrderNumber?: string | null;
  /** 操作者 Id（Admin 人工来源时为 Admin 操作者；系统来源为系统标识）。 */
  operatorId?: string | null;
  /**
   * 发生时间（Unix 秒）。
   * @format int64
   */
  occurredAt?: number;
}

/** 资产流水分页响应（Cursor + HasMore，vault:C2 S1.2；透传 Online 游标语义，不套页码偏移）。 */
export interface OnlineAssetLedgerPageResponse {
  /** 当前页流水行。 */
  items?: OnlineAssetLedgerEntryResponse[] | null;
  /** 下一页游标（null = 没有更多数据）。 */
  nextCursor?: string | null;
  /** 是否还有更多数据。 */
  hasMore?: boolean;
}

/**
 * 资产流水分页查询输入参数（vault:C4 LedgerEntry 查询；Cursor 翻页，vault:C2 S1.2 定稿）。
 *
 * 查询实时透传 Online，Admin 不落资产镜像。
 */
export interface PageOnlineAssetLedgerRequest {
  /**
   * 应用作用域（必填，经 OnlineScopeContextFactory 校验租户归属）。
   * @min 1
   * @max 9223372036854776000
   */
  appId: string;
  /**
   * 区服作用域（必填，经 OnlineScopeContextFactory 校验租户与应用归属）。
   * @min 1
   * @max 9223372036854776000
   */
  serverId: string;
  /**
   * 目标玩家 Id（必填；流水页按玩家查询）。
   * @minLength 1
   * @maxLength 256
   */
  playerId: string;
  /**
   * 货币或道具类型过滤（null = 不过滤，返回该玩家全部资产维度流水）。
   * @maxLength 256
   */
  currencyOrItemType?: string | null;
  /**
   * 发生时间窗起点（Unix 秒；null = 不限）。
   * @format int64
   */
  startTime?: number | null;
  /**
   * 发生时间窗终点（Unix 秒；null = 不限）。
   * @format int64
   */
  endTime?: number | null;
  /** 游标（上一页返回的 NextCursor；首页不传）。 */
  cursor?: string | null;
  /**
   * 页大小（1～100，默认 20）。
   * @format int32
   * @min 1
   * @max 100
   */
  pageSize?: number;
}

export interface OnlineAssetLedgerQueryPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 资产流水分页响应（Cursor + HasMore，vault:C2 S1.2；透传 Online 游标语义，不套页码偏移）。 */
  data: OnlineAssetLedgerPageResponse;
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
