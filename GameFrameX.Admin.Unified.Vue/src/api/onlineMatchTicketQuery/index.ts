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
  OnlineMatchTicketQueryAbnormalPagePostData,
  OnlineMatchTicketQueryFailurePagePostData,
  PageOnlineMatchAbnormalTicketRequest,
  PageOnlineMatchFailureRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class OnlineMatchTicketQuery<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags onlineMatchTicketQuery
   * @name OnlineMatchTicketQueryAbnormalPagePost
   * @summary 异常 Ticket 分页查询：按异常类别（长时间未分配/重复取消/孤儿）检索，游标翻页往返透传 Online。
   * @request POST:/api/onlineMatchTicketQuery/abnormalPage
   */
  onlineMatchTicketQueryAbnormalPagePost = (
    data: PageOnlineMatchAbnormalTicketRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<OnlineMatchTicketQueryAbnormalPagePostData, any>({
      path: `/api/onlineMatchTicketQuery/abnormalPage`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags onlineMatchTicketQuery
   * @name OnlineMatchTicketQueryFailurePagePost
   * @summary 匹配失败原因分页查询：原因码/时间窗过滤 + 按原因码聚合摘要，游标翻页往返透传 Online。
   * @request POST:/api/onlineMatchTicketQuery/failurePage
   */
  onlineMatchTicketQueryFailurePagePost = (
    data: PageOnlineMatchFailureRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<OnlineMatchTicketQueryFailurePagePostData, any>({
      path: `/api/onlineMatchTicketQuery/failurePage`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const onlineMatchTicketQueryAbnormalPagePost = (...args: Parameters<InstanceType<typeof OnlineMatchTicketQuery>['onlineMatchTicketQueryAbnormalPagePost']>) => getAPI(OnlineMatchTicketQuery).onlineMatchTicketQueryAbnormalPagePost(...args);
export const onlineMatchTicketQueryFailurePagePost = (...args: Parameters<InstanceType<typeof OnlineMatchTicketQuery>['onlineMatchTicketQueryFailurePagePost']>) => getAPI(OnlineMatchTicketQuery).onlineMatchTicketQueryFailurePagePost(...args);
