/* eslint-disable */
/* tslint:disable */

/** @format int32 */
export enum MessageTypeEnum {
  /** 消息 */
  Info = 0,
  /** 成功 */
  Success = 1,
  /** 警告 */
  Warning = 2,
  /** 错误 */
  Error = 3,
}

export interface MessageRequest {
  /** 用户ID */
  userId?: string;
  /** 用户ID列表 */
  userIds?: number[] | null;
  /** 消息标题 */
  title?: string | null;
  messageType?: MessageTypeEnum;
  /** 消息内容 */
  message?: string | null;
}

export type SystemMessageSendAllUserPostData = any;

export interface SystemMessageSendEmailPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: boolean;
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

export type SystemMessageSendOtherUserPostData = any;

export type SystemMessageSendUserPostData = any;

export type SystemMessageSendUsersPostData = any;
