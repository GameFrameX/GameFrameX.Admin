/* eslint-disable */
/* tslint:disable */
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

import {
  TenantWechatOfficialAccountDeleteMessageTemplatePostData,
  TenantWechatOfficialAccountGenConfigParaPostData,
  TenantWechatOfficialAccountMessageTemplateListGetData,
  TenantWechatOfficialAccountSendTemplateMessagePostData,
  WechatDeleteMessageTemplateRequest,
  WechatMessageTemplateSendRequest,
  WechatSignatureRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantWechatOfficialAccount<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantWechatOfficialAccount
   * @name TenantWechatOfficialAccountDeleteMessageTemplatePost
   * @summary 删除模板。
   * @request POST:/api/tenantWechatOfficialAccount/deleteMessageTemplate
   */
  tenantWechatOfficialAccountDeleteMessageTemplatePost = (
    data: WechatDeleteMessageTemplateRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<
      TenantWechatOfficialAccountDeleteMessageTemplatePostData,
      any
    >({
      path: `/api/tenantWechatOfficialAccount/deleteMessageTemplate`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantWechatOfficialAccount
   * @name TenantWechatOfficialAccountGenConfigParaPost
   * @summary 获取配置签名参数(wx.config)。 Admin 会话本身无隐式 AppId；调用方必须显式传入 `appId`，resolver 仅在 `appId=0` 时选择租户默认公众号配置。
   * @request POST:/api/tenantWechatOfficialAccount/genConfigPara
   */
  tenantWechatOfficialAccountGenConfigParaPost = (
    data: WechatSignatureRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantWechatOfficialAccountGenConfigParaPostData, any>({
      path: `/api/tenantWechatOfficialAccount/genConfigPara`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantWechatOfficialAccount
   * @name TenantWechatOfficialAccountMessageTemplateListGet
   * @summary 获取模板列表。
   * @request GET:/api/tenantWechatOfficialAccount/messageTemplateList
   */
  tenantWechatOfficialAccountMessageTemplateListGet = (
    query?: {
      /**
       * 应用 ID。0/null 表示租户默认公众号配置；> 0 表示应用级覆盖。
       * @format int64
       */
      appId?: number;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<
      TenantWechatOfficialAccountMessageTemplateListGetData,
      any
    >({
      path: `/api/tenantWechatOfficialAccount/messageTemplateList`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantWechatOfficialAccount
   * @name TenantWechatOfficialAccountSendTemplateMessagePost
   * @summary 发送模板消息。 使用当前租户公众号凭证创建客户端；跳转小程序的 AppId 从同一条配置读取，不复用其他租户配置。
   * @request POST:/api/tenantWechatOfficialAccount/sendTemplateMessage
   */
  tenantWechatOfficialAccountSendTemplateMessagePost = (
    data: WechatMessageTemplateSendRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<
      TenantWechatOfficialAccountSendTemplateMessagePostData,
      any
    >({
      path: `/api/tenantWechatOfficialAccount/sendTemplateMessage`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const tenantWechatOfficialAccountDeleteMessageTemplatePost = (...args: Parameters<InstanceType<typeof TenantWechatOfficialAccount>['tenantWechatOfficialAccountDeleteMessageTemplatePost']>) => getAPI(TenantWechatOfficialAccount).tenantWechatOfficialAccountDeleteMessageTemplatePost(...args);
export const tenantWechatOfficialAccountGenConfigParaPost = (...args: Parameters<InstanceType<typeof TenantWechatOfficialAccount>['tenantWechatOfficialAccountGenConfigParaPost']>) => getAPI(TenantWechatOfficialAccount).tenantWechatOfficialAccountGenConfigParaPost(...args);
export const tenantWechatOfficialAccountMessageTemplateListGet = (...args: Parameters<InstanceType<typeof TenantWechatOfficialAccount>['tenantWechatOfficialAccountMessageTemplateListGet']>) => getAPI(TenantWechatOfficialAccount).tenantWechatOfficialAccountMessageTemplateListGet(...args);
export const tenantWechatOfficialAccountSendTemplateMessagePost = (...args: Parameters<InstanceType<typeof TenantWechatOfficialAccount>['tenantWechatOfficialAccountSendTemplateMessagePost']>) => getAPI(TenantWechatOfficialAccount).tenantWechatOfficialAccountSendTemplateMessagePost(...args);
