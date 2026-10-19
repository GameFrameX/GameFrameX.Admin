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
  OnlineReportCaseHandlePostData,
  OnlineReportCasePagePostData,
  OnlineReportHandleRequest,
  PageOnlineReportCaseRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class OnlineReportCase<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags onlineReportCase
   * @name OnlineReportCaseHandlePost
   * @summary 处置举报案件：按决策分派三步流（事实源 → 联动 → 通知/回写），任一步失败不静默——落审计并在响应中呈现。 决策语义：Dismiss（驳回，不写事实源）/ Warn（警告通知，不写事实源）/ Mute / Ban（写处罚事实源并联动 Online 运行时）/ ForwardPenalty（转交处罚流程人工跟进，案件转 Handling，不自动写事实源）。
   * @request POST:/api/onlineReportCase/handle
   */
  onlineReportCaseHandlePost = (
    data: OnlineReportHandleRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<OnlineReportCaseHandlePostData, any>({
      path: `/api/onlineReportCase/handle`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags onlineReportCase
   * @name OnlineReportCasePagePost
   * @summary 举报案件分页查询：过滤条件透传 Online，游标分页往返。
   * @request POST:/api/onlineReportCase/page
   */
  onlineReportCasePagePost = (
    data: PageOnlineReportCaseRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<OnlineReportCasePagePostData, any>({
      path: `/api/onlineReportCase/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const onlineReportCaseHandlePost = (...args: Parameters<InstanceType<typeof OnlineReportCase>['onlineReportCaseHandlePost']>) => getAPI(OnlineReportCase).onlineReportCaseHandlePost(...args);
export const onlineReportCasePagePost = (...args: Parameters<InstanceType<typeof OnlineReportCase>['onlineReportCasePagePost']>) => getAPI(OnlineReportCase).onlineReportCasePagePost(...args);
