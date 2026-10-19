/* eslint-disable */
/* tslint:disable */

import type { OnlinePenaltyCommandKind, OnlinePenaltyLinkResponse } from "../_shared/data-contracts";

/**
 * 处罚联动命令输入参数（高危端点，权限码 onlinePenaltyLinkCommand:execute 仅同步 TenantAdmin）。
 *
 * 作用域从处罚事实源行推导（AppId 必有；AreaId 有值 = 单区服联动，null = App 下全部区服逐一联动）。
 */
export interface OnlinePenaltyLinkRequest {
  /**
   * 处罚事实源行 Id（必须处于生效中）。
   * @min 1
   * @max 9223372036854776000
   */
  penaltyId: string;
  command: OnlinePenaltyCommandKind;
}

export interface OnlinePenaltyLinkCommandExecutePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 处罚联动命令聚合响应（按联动目标区服集逐区服执行后的聚合结果）。 */
  data: OnlinePenaltyLinkResponse;
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
