/* eslint-disable */
/* tslint:disable */

/** @format int32 */
export enum OnlineChatChannelKind {
  /** 私聊 */
  Direct = 1,
  /** 队伍频道 */
  Party = 2,
  /** 群组频道 */
  Group = 3,
  /** 世界频道 */
  Global = 4,
}

/** 消息审计条目展示（脱敏后）。字段经 `OnlineMessageAuditDisplayMapper` 从 Online 消息对象映射。 */
export interface OnlineMessageAuditItemResponse {
  /** 消息 Id（Online 侧标识，原样透传）。 */
  messageId?: string | null;
  channelKind?: OnlineChatChannelKind;
  /** 参与者玩家 Id 列表（Direct=双方、Party/Group=成员、Global=发送者）。 */
  participantPlayerIds?: string[] | null;
  /** 消息内容（展示层已按 Online 脱敏规则处理 + Admin 兜底脱敏，见 DisplayMapper）。 */
  content?: string | null;
  /**
   * 发生时间（Unix 秒）。
   * @format int64
   */
  occurredAt?: number;
}

/** 消息审计分页响应（Cursor + HasMore，vault:C2 S1.2；透传 Online 游标语义）。 */
export interface OnlineMessageAuditPageResponse {
  /** 当前页消息条目（脱敏后）。 */
  items?: OnlineMessageAuditItemResponse[] | null;
  /** 下一页游标（null = 没有更多数据）。 */
  nextCursor?: string | null;
  /** 是否还有更多数据。 */
  hasMore?: boolean;
}

/**
 * 消息审计分页查询输入参数（vault:C7 四类频道；Cursor 翻页，vault:C2 S1.2 定稿）。
 *
 * 查询实时透传 Online，Admin 不落消息镜像。
 */
export interface PageOnlineMessageAuditRequest {
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
  channelKind?: OnlineChatChannelKind;
  /** 参与者玩家 Id 过滤（null = 不过滤）。 */
  participantPlayerId?: string | null;
  /** 内容关键词过滤（null = 不过滤）。 */
  keyword?: string | null;
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

export interface OnlineMessageAuditQueryPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 消息审计分页响应（Cursor + HasMore，vault:C2 S1.2；透传 Online 游标语义）。 */
  data: OnlineMessageAuditPageResponse;
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
