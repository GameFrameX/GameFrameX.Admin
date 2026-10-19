/* eslint-disable */
/* tslint:disable */

/**
 * Online 控制台受控操作命令枚举（严格四值，来源 change C344 / vault:C9 S8.1）。
 *
 * 只承载本 change 新增的四条运行时干预命令；踢下线/封禁/禁言与发放/撤销资产等既有命令
 * 由 C350 `OnlinePenaltyLinkCommandService`、C340 `OnlineAssetGrantCommandService` 既有端点承载
 * （复用矩阵，禁平行实现——守护测试 `OnlineOverviewIsolationTests` 锁定枚举四值与 Console 域零既有命令 token）。
 * 新增命令必须回 vault 评审并同步守护测试。
 * @format int32
 */
export enum OnlineControlledCommandKind {
  /** CancelTicket */
  CancelTicket = 1,
  /** EndAbnormalMatch */
  EndAbnormalMatch = 2,
  /** SuspendResumePlayer */
  SuspendResumePlayer = 3,
  /** MatchIsolation */
  MatchIsolation = 4,
}

/**
 * 开关型受控命令的方向（来源 change C344 A0 补充锁定）。
 *
 * SuspendResumePlayer（暂停/恢复）与 MatchIsolation（隔离/解除）是开关型命令，必须携带方向；
 * CancelTicket / EndAbnormalMatch 是单向动作，禁止携带方向（携带即参数校验拒绝）。
 * 方向参与幂等键——同一目标「暂停」与「恢复」是两次独立命令意图，各自幂等。
 * @format int32
 */
export enum ControlledCommandDirection {
  /** Apply */
  Apply = 1,
  /** Lift */
  Lift = 2,
}

/**
 * 受控操作命令回执（线缆/Admin 出参共用；vault:C9 S8.1 契约，mock 先行）。
 *
 * 透传 Online 首次执行结果——同 IdempotencyKey 的重复调用拿到的是首次回执（VC-8.4），不产生第二次副作用。
 */
export interface OnlineControlledCommandAckResponse {
  /** 命令标识（回显）。 */
  command?: string | null;
  /** 目标标识（TicketId / MatchId / PlayerId，回显）。 */
  targetIdentifier?: string | null;
  /** 幂等键（回执即本键；同键重发返回首次结果）。 */
  idempotencyKey?: string | null;
  /** 执行状态（Online 侧原值，如 Accepted / Executed）。 */
  status?: string | null;
  /**
   * 执行时间（Unix 秒；幂等重发时为首次执行时间）。
   * @format int64
   */
  executedAt?: number;
  /** 执行摘要（结果概述；幂等重发时说明返回首次结果）。 */
  summary?: string | null;
}

/**
 * 受控操作命令输入参数（高危端点；change C344 / vault:C9 S8.1）。
 *
 * 高危复核口径（A0 锁定）：本端点整体高危（权限码仅同步 TenantAdmin），`Reason` 与
 * `ReviewerOperatorId` 对全部四命令必填——双人复核字段落地 vault「审批（如需）」，不做通用审批流。
 * 目标标识按命令取值：CancelTicket → TicketId；EndAbnormalMatch → MatchId；SuspendResumePlayer / MatchIsolation → PlayerId。
 */
export interface OnlineControlledCommandRequest {
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
   * Online 控制台受控操作命令枚举（严格四值，来源 change C344 / vault:C9 S8.1）。
   *
   * 只承载本 change 新增的四条运行时干预命令；踢下线/封禁/禁言与发放/撤销资产等既有命令
   * 由 C350 `OnlinePenaltyLinkCommandService`、C340 `OnlineAssetGrantCommandService` 既有端点承载
   * （复用矩阵，禁平行实现——守护测试 `OnlineOverviewIsolationTests` 锁定枚举四值与 Console 域零既有命令 token）。
   * 新增命令必须回 vault 评审并同步守护测试。
   */
  command: OnlineControlledCommandKind;
  /**
   * 目标 Ticket Id（CancelTicket 必填；其余命令必须为空）。
   * @maxLength 256
   */
  ticketId?: string | null;
  /**
   * 目标 Match Id（EndAbnormalMatch 必填；其余命令必须为空）。
   * @maxLength 256
   */
  matchId?: string | null;
  /**
   * 目标玩家 Id（SuspendResumePlayer / MatchIsolation 必填；其余命令必须为空）。
   * @maxLength 256
   */
  playerId?: string | null;
  /**
   * 开关型受控命令的方向（来源 change C344 A0 补充锁定）。
   *
   * SuspendResumePlayer（暂停/恢复）与 MatchIsolation（隔离/解除）是开关型命令，必须携带方向；
   * CancelTicket / EndAbnormalMatch 是单向动作，禁止携带方向（携带即参数校验拒绝）。
   * 方向参与幂等键——同一目标「暂停」与「恢复」是两次独立命令意图，各自幂等。
   */
  direction?: ControlledCommandDirection;
  /**
   * 操作原因（必填，进审计）。
   * @minLength 1
   * @maxLength 512
   */
  reason: string;
  /**
   * 复核人操作者 Id（高危双人复核，必填，进审计；不得等于操作者本人由前端提示、后端只做非空校验）。
   * @minLength 1
   * @maxLength 256
   */
  reviewerOperatorId: string;
}

export interface OnlineControlledCommandExecutePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /**
   * 受控操作命令回执（线缆/Admin 出参共用；vault:C9 S8.1 契约，mock 先行）。
   *
   * 透传 Online 首次执行结果——同 IdempotencyKey 的重复调用拿到的是首次回执（VC-8.4），不产生第二次副作用。
   */
  data: OnlineControlledCommandAckResponse;
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
