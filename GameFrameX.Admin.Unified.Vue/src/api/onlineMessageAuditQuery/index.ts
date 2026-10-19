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
  OnlineMessageAuditQueryPagePostData,
  PageOnlineMessageAuditRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class OnlineMessageAuditQuery<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags onlineMessageAuditQuery
   * @name OnlineMessageAuditQueryPagePost
   * @summary 消息审计分页查询：过滤条件透传 Online，游标分页往返，展示前集中脱敏。
   * @request POST:/api/onlineMessageAuditQuery/page
   */
  onlineMessageAuditQueryPagePost = (
    data: PageOnlineMessageAuditRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<OnlineMessageAuditQueryPagePostData, any>({
      path: `/api/onlineMessageAuditQuery/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const onlineMessageAuditQueryPagePost = (...args: Parameters<InstanceType<typeof OnlineMessageAuditQuery>['onlineMessageAuditQueryPagePost']>) => getAPI(OnlineMessageAuditQuery).onlineMessageAuditQueryPagePost(...args);
