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
  MessageRequest,
  SystemMessageSendAllUserPostData,
  SystemMessageSendEmailPostData,
  SystemMessageSendOtherUserPostData,
  SystemMessageSendUserPostData,
  SystemMessageSendUsersPostData,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class SystemMessage<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags systemMessage
   * @name SystemMessageSendAllUserPost
   * @summary 发送消息给所有人
   * @request POST:/api/systemMessage/sendAllUser
   */
  systemMessageSendAllUserPost = (
    data: MessageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemMessageSendAllUserPostData, any>({
      path: `/api/systemMessage/sendAllUser`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemMessage
   * @name SystemMessageSendEmailPost
   * @summary 发送平台系统邮件。 从主库 `SystemEmailProviderEntity` 读取配置（按 `SystemEmailPurpose.PlatformOps` 默认）；缺配置返回 `false`，禁止静默成功或 fallback 到租户玩家邮件。 缺省收件人：使用当前命中的 `SystemEmailProviderEntity` 配置的 `DefaultToEmail`；若配置也未提供则发送失败。
   * @request POST:/api/systemMessage/sendEmail
   */
  systemMessageSendEmailPost = (
    query: {
      /** 邮件正文。 */
      message: string;
      /**
       * 邮件主题。
       * @default "系统邮件"
       */
      title?: string;
      /**
       * 是否 HTML 邮件；当前 `ISystemEmailSender` 实现按纯文本发送，HTML 标志位为预留扩展点。
       * @default false
       */
      isHtml?: boolean;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<SystemMessageSendEmailPostData, any>({
      path: `/api/systemMessage/sendEmail`,
      method: "POST",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemMessage
   * @name SystemMessageSendOtherUserPost
   * @summary 发送消息给除了发送人的其他人
   * @request POST:/api/systemMessage/sendOtherUser
   */
  systemMessageSendOtherUserPost = (
    data: MessageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemMessageSendOtherUserPostData, any>({
      path: `/api/systemMessage/sendOtherUser`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemMessage
   * @name SystemMessageSendUserPost
   * @summary 发送消息给某个人
   * @request POST:/api/systemMessage/sendUser
   */
  systemMessageSendUserPost = (
    data: MessageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemMessageSendUserPostData, any>({
      path: `/api/systemMessage/sendUser`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemMessage
   * @name SystemMessageSendUsersPost
   * @summary 发送消息给某些人
   * @request POST:/api/systemMessage/sendUsers
   */
  systemMessageSendUsersPost = (
    data: MessageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemMessageSendUsersPostData, any>({
      path: `/api/systemMessage/sendUsers`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const systemMessageSendAllUserPost = (...args: Parameters<InstanceType<typeof SystemMessage>['systemMessageSendAllUserPost']>) => getAPI(SystemMessage).systemMessageSendAllUserPost(...args);
export const systemMessageSendEmailPost = (...args: Parameters<InstanceType<typeof SystemMessage>['systemMessageSendEmailPost']>) => getAPI(SystemMessage).systemMessageSendEmailPost(...args);
export const systemMessageSendOtherUserPost = (...args: Parameters<InstanceType<typeof SystemMessage>['systemMessageSendOtherUserPost']>) => getAPI(SystemMessage).systemMessageSendOtherUserPost(...args);
export const systemMessageSendUserPost = (...args: Parameters<InstanceType<typeof SystemMessage>['systemMessageSendUserPost']>) => getAPI(SystemMessage).systemMessageSendUserPost(...args);
export const systemMessageSendUsersPost = (...args: Parameters<InstanceType<typeof SystemMessage>['systemMessageSendUsersPost']>) => getAPI(SystemMessage).systemMessageSendUsersPost(...args);
