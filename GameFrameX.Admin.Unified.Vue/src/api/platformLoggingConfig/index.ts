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
  PlatformLoggingConfigDetailPostData,
  PlatformLoggingConfigUpdatePostData,
  PlatformLoggingConfigUpdateRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class PlatformLoggingConfig<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags platformLoggingConfig
   * @name PlatformLoggingConfigDetailPost
   * @summary 获取平台日志配置详情。ES ServerUris 元素统一掩码为 `"***"`，仅保留数量。
   * @request POST:/api/platformLoggingConfig/detail
   */
  platformLoggingConfigDetailPost = (params: RequestParams = {}) =>
    this.http.request<PlatformLoggingConfigDetailPostData, any>({
      path: `/api/platformLoggingConfig/detail`,
      method: "POST",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags platformLoggingConfig
   * @name PlatformLoggingConfigUpdatePost
   * @summary 保存平台日志配置。ES ServerUris 全为掩码占位时保留库内原值；CAS 乐观锁冲突重试；成功后刷新同 Host 快照。
   * @request POST:/api/platformLoggingConfig/update
   */
  platformLoggingConfigUpdatePost = (
    data: PlatformLoggingConfigUpdateRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<PlatformLoggingConfigUpdatePostData, any>({
      path: `/api/platformLoggingConfig/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const platformLoggingConfigDetailPost = (...args: Parameters<InstanceType<typeof PlatformLoggingConfig>['platformLoggingConfigDetailPost']>) => getAPI(PlatformLoggingConfig).platformLoggingConfigDetailPost(...args);
export const platformLoggingConfigUpdatePost = (...args: Parameters<InstanceType<typeof PlatformLoggingConfig>['platformLoggingConfigUpdatePost']>) => getAPI(PlatformLoggingConfig).platformLoggingConfigUpdatePost(...args);
