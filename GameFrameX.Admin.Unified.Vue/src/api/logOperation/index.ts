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
  LogOperationClearPostData,
  LogOperationExportPostData,
  LogOperationPagePostData,
} from "./types";
import {
  LogRequest,
  PageLogRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class LogOperation<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags logOperation
   * @name LogOperationClearPost
   * @summary 清空日志。平台上下文清空主库平台日志，租户上下文清空当前租户库的租户日志。
   * @request POST:/api/logOperation/clear
   */
  logOperationClearPost = (params: RequestParams = {}) =>
    this.http.request<LogOperationClearPostData, any>({
      path: `/api/logOperation/clear`,
      method: "POST",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags logOperation
   * @name LogOperationExportPost
   * @summary 导出操作日志
   * @request POST:/api/logOperation/export
   */
  logOperationExportPost = (data: LogRequest, params: RequestParams = {}) =>
    this.http.request<LogOperationExportPostData, any>({
      path: `/api/logOperation/export`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags logOperation
   * @name LogOperationPagePost
   * @summary 获取日志分页列表
   * @request POST:/api/logOperation/page
   */
  logOperationPagePost = (data: PageLogRequest, params: RequestParams = {}) =>
    this.http.request<LogOperationPagePostData, any>({
      path: `/api/logOperation/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const logOperationClearPost = (...args: Parameters<InstanceType<typeof LogOperation>['logOperationClearPost']>) => getAPI(LogOperation).logOperationClearPost(...args);
export const logOperationExportPost = (...args: Parameters<InstanceType<typeof LogOperation>['logOperationExportPost']>) => getAPI(LogOperation).logOperationExportPost(...args);
export const logOperationPagePost = (...args: Parameters<InstanceType<typeof LogOperation>['logOperationPagePost']>) => getAPI(LogOperation).logOperationPagePost(...args);
