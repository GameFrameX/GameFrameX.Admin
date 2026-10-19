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
  LogExceptionClearPostData,
  LogExceptionExportPostData,
  LogExceptionPagePostData,
} from "./types";
import {
  LogRequest,
  PageLogRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class LogException<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags logException
   * @name LogExceptionClearPost
   * @summary 清空日志。平台上下文清空主库平台日志，租户上下文清空当前租户库的租户日志。
   * @request POST:/api/logException/clear
   */
  logExceptionClearPost = (params: RequestParams = {}) =>
    this.http.request<LogExceptionClearPostData, any>({
      path: `/api/logException/clear`,
      method: "POST",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags logException
   * @name LogExceptionExportPost
   * @summary 导出异常日志
   * @request POST:/api/logException/export
   */
  logExceptionExportPost = (data: LogRequest, params: RequestParams = {}) =>
    this.http.request<LogExceptionExportPostData, any>({
      path: `/api/logException/export`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags logException
   * @name LogExceptionPagePost
   * @summary 获取日志分页列表
   * @request POST:/api/logException/page
   */
  logExceptionPagePost = (data: PageLogRequest, params: RequestParams = {}) =>
    this.http.request<LogExceptionPagePostData, any>({
      path: `/api/logException/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const logExceptionClearPost = (...args: Parameters<InstanceType<typeof LogException>['logExceptionClearPost']>) => getAPI(LogException).logExceptionClearPost(...args);
export const logExceptionExportPost = (...args: Parameters<InstanceType<typeof LogException>['logExceptionExportPost']>) => getAPI(LogException).logExceptionExportPost(...args);
export const logExceptionPagePost = (...args: Parameters<InstanceType<typeof LogException>['logExceptionPagePost']>) => getAPI(LogException).logExceptionPagePost(...args);
