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
  LogVisitClearPostData,
  LogVisitPagePostData,
} from "./types";
import {
  PageLogRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class LogVisit<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags logVisit
   * @name LogVisitClearPost
   * @summary 清空日志。平台上下文清空主库平台日志，租户上下文清空当前租户库的租户日志。
   * @request POST:/api/logVisit/clear
   */
  logVisitClearPost = (params: RequestParams = {}) =>
    this.http.request<LogVisitClearPostData, any>({
      path: `/api/logVisit/clear`,
      method: "POST",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags logVisit
   * @name LogVisitPagePost
   * @summary 获取日志分页列表
   * @request POST:/api/logVisit/page
   */
  logVisitPagePost = (data: PageLogRequest, params: RequestParams = {}) =>
    this.http.request<LogVisitPagePostData, any>({
      path: `/api/logVisit/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const logVisitClearPost = (...args: Parameters<InstanceType<typeof LogVisit>['logVisitClearPost']>) => getAPI(LogVisit).logVisitClearPost(...args);
export const logVisitPagePost = (...args: Parameters<InstanceType<typeof LogVisit>['logVisitPagePost']>) => getAPI(LogVisit).logVisitPagePost(...args);
