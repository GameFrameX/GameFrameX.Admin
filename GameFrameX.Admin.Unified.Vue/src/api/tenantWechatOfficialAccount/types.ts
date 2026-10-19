/* eslint-disable */
/* tslint:disable */

export interface DataItem {
  value?: string | null;
}

/** 删除公众号模板（C23：必须显式传入 AppId）。 */
export interface WechatDeleteMessageTemplateRequest {
  /**
   * 订阅模板Id
   * @minLength 1
   */
  templateId: string;
  /** 应用 ID。0/null 表示租户默认公众号配置；> 0 表示应用级覆盖。 */
  appId?: string | null;
}

/** 公众号模板删除结果 */
export interface WechatDeleteResultResponse {
  /**
   * 微信接口返回的错误码（0 表示成功）
   * @format int32
   */
  errorCode?: number | null;
  /** 微信接口返回的错误信息 */
  errorMessage?: string | null;
}

export interface WechatDeleteResultResponseHttpJsonResultData {
  /** @format int32 */
  code?: number;
  message?: string | null;
  /** 公众号模板删除结果 */
  data?: WechatDeleteResultResponse;
  trackId?: string | null;
  errorCode?: string | null;
  type?: string | null;
  /** @format int64 */
  time?: number;
  extras?: any | null;
}

/** 微信 JS-SDK wx.config 签名参数 */
export interface WechatJsConfigResponse {
  /** 公众号 AppId */
  appId?: string | null;
  /** 生成签名用时间戳 */
  timestamp?: string | null;
  /** 生成签名用随机字符串 */
  nonceStr?: string | null;
  /** JS-SDK 签名 */
  signature?: string | null;
}

export interface WechatJsConfigResponseHttpJsonResultData {
  /** @format int32 */
  code?: number;
  message?: string | null;
  /** 微信 JS-SDK wx.config 签名参数 */
  data?: WechatJsConfigResponse;
  trackId?: string | null;
  errorCode?: string | null;
  type?: string | null;
  /** @format int64 */
  time?: number;
  extras?: any | null;
}

/** 发送公众号模板消息（C23：必须显式传入 AppId）。 */
export interface WechatMessageTemplateSendRequest {
  /**
   * 订阅模板Id
   * @minLength 1
   */
  templateId: string;
  /**
   * 接收者的OpenId
   * @minLength 1
   */
  toUserOpenId: string;
  /** 模板数据，格式形如 { "key1": { "value": any }, "key2": { "value": any } } */
  data: Record<string, DataItem>;
  /** 模板跳转链接 */
  url?: string | null;
  /** 所需跳转到小程序的具体页面路径，支持带参数,（示例index?foo=bar） */
  miniProgramPagePath?: string | null;
  /** 应用 ID。0/null 表示租户默认公众号配置；> 0 表示应用级覆盖。 */
  appId?: string | null;
}

/** 公众号模板消息发送结果 */
export interface WechatSendResultResponse {
  /** 消息 ID（发送成功时返回的数字标识） */
  messageId?: string | null;
  /**
   * 微信接口返回的错误码（0 表示成功）
   * @format int32
   */
  errorCode?: number | null;
  /** 微信接口返回的错误信息 */
  errorMessage?: string | null;
}

export interface WechatSendResultResponseHttpJsonResultData {
  /** @format int32 */
  code?: number;
  message?: string | null;
  /** 公众号模板消息发送结果 */
  data?: WechatSendResultResponse;
  trackId?: string | null;
  errorCode?: string | null;
  type?: string | null;
  /** @format int64 */
  time?: number;
  extras?: any | null;
}

/** 获取 wx.config 签名参数（C23：必须显式传入 AppId）。 */
export interface WechatSignatureRequest {
  /** 应用 ID。0/null 表示租户默认公众号配置；> 0 表示应用级覆盖。 */
  appId?: string | null;
  /** 当前页面 URL（用于 wx.config 签名）。 */
  url?: string | null;
}

/** 公众号消息模板信息 */
export interface WechatTemplateResponse {
  /** 模板 ID */
  templateId?: string | null;
  /** 模板标题 */
  title?: string | null;
  /** 模板所属一级行业 */
  primaryIndustry?: string | null;
  /** 模板所属二级行业 */
  secondaryIndustry?: string | null;
  /** 模板内容 */
  content?: string | null;
  /** 模板内容示例 */
  example?: string | null;
}

export interface WechatTemplateResponseListHttpJsonResultData {
  /** @format int32 */
  code?: number;
  message?: string | null;
  data?: WechatTemplateResponse[] | null;
  trackId?: string | null;
  errorCode?: string | null;
  type?: string | null;
  /** @format int64 */
  time?: number;
  extras?: any | null;
}

export type TenantWechatOfficialAccountDeleteMessageTemplatePostData =
  WechatDeleteResultResponseHttpJsonResultData;

export type TenantWechatOfficialAccountGenConfigParaPostData =
  WechatJsConfigResponseHttpJsonResultData;

export type TenantWechatOfficialAccountMessageTemplateListGetData =
  WechatTemplateResponseListHttpJsonResultData;

export type TenantWechatOfficialAccountSendTemplateMessagePostData =
  WechatSendResultResponseHttpJsonResultData;
