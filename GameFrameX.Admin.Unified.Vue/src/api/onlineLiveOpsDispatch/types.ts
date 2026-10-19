/* eslint-disable */
/* tslint:disable */

import type { LiveOpsSourceKind, OnlineLiveOpsDispatchResponse } from "../_shared/data-contracts";

/**
 * LiveOps 统一下发输入（来源 change C343 / vault:C8 S7.7）。
 *
 * 五类来源硬枚举 + 业务主键 + Online 作用域三元组的 AppId/ServerId（TenantId 由鉴权上下文强制注入）；
 * `Reason` 必填进审计；`SegmentId` 可选定向分群。越枚举值 / 越作用域在入口即拒绝。
 */
export interface OnlineLiveOpsDispatchRequest {
  source: LiveOpsSourceKind;
  /**
   * 来源域业务主键（对应来源实体的 Id，如远程配置记录 Id）。
   * @format int64
   * @min 1
   * @max 9223372036854776000
   */
  businessKey: number;
  /**
   * 目标应用Id（经 Online 作用域三元组租户归属校验）。
   * @min 1
   * @max 9223372036854776000
   */
  appId: string;
  /**
   * 目标区服Id（经 Online 作用域三元组租户归属校验；Online 命令按区服路由到对应 Runtime）。
   * @min 1
   * @max 9223372036854776000
   */
  serverId: string;
  /**
   * 可选定向分群Id（引用玩家分群行；下发只对分群内玩家生效，语义对照见 OnlineRolloutSemanticMap）。
   * @min 1
   * @max 9223372036854776000
   */
  segmentId?: string | null;
  /**
   * 下发理由（必填，随 MonitorLogging 全链路审计）。
   * @minLength 1
   */
  reason: string;
}

export interface OnlineLiveOpsDispatchDispatchPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /**
   * LiveOps 统一下发回执（来源 change C343 / vault:C8 S7.7）。
   *
   * 透传 Online 确认版本 / 状态供前端回显；`Merged` 标记邮件来源「同目标合并返回首次回执」路径。
   */
  data: OnlineLiveOpsDispatchResponse;
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
