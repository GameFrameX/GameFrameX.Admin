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
  LogDiffClearPostData,
  LogDiffPagePostData,
} from "./types";
import {
  PageLogRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class LogDiff<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags logDiff
   * @name LogDiffClearPost
   * @summary 清空日志。平台上下文清空主库平台日志，租户上下文清空当前租户库的租户日志。
   * @request POST:/api/logDiff/clear
   */
  logDiffClearPost = (params: RequestParams = {}) =>
    this.http.request<LogDiffClearPostData, any>({
      path: `/api/logDiff/clear`,
      method: "POST",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags logDiff
   * @name LogDiffPagePost
   * @summary 获取日志分页列表
   * @request POST:/api/logDiff/page
   */
  logDiffPagePost = (data: PageLogRequest, params: RequestParams = {}) =>
    this.http.request<LogDiffPagePostData, any>({
      path: `/api/logDiff/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const logDiffClearPost = (...args: Parameters<InstanceType<typeof LogDiff>['logDiffClearPost']>) => getAPI(LogDiff).logDiffClearPost(...args);
export const logDiffPagePost = (...args: Parameters<InstanceType<typeof LogDiff>['logDiffPagePost']>) => getAPI(LogDiff).logDiffPagePost(...args);
