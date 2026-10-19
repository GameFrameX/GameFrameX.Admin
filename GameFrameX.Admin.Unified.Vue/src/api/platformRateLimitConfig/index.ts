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
  PlatformRateLimitConfigDetailPostData,
  PlatformRateLimitConfigUpdatePostData,
  PlatformRateLimitConfigUpdateRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class PlatformRateLimitConfig<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags platformRateLimitConfig
   * @name PlatformRateLimitConfigDetailPost
   * @summary 获取平台限流配置详情。
   * @request POST:/api/platformRateLimitConfig/detail
   */
  platformRateLimitConfigDetailPost = (params: RequestParams = {}) =>
    this.http.request<PlatformRateLimitConfigDetailPostData, any>({
      path: `/api/platformRateLimitConfig/detail`,
      method: "POST",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags platformRateLimitConfig
   * @name PlatformRateLimitConfigUpdatePost
   * @summary 保存平台限流配置。每次成功保存都会递增版本，使后续请求切换到新的 limiter 分区。
   * @request POST:/api/platformRateLimitConfig/update
   */
  platformRateLimitConfigUpdatePost = (
    data: PlatformRateLimitConfigUpdateRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<PlatformRateLimitConfigUpdatePostData, any>({
      path: `/api/platformRateLimitConfig/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const platformRateLimitConfigDetailPost = (...args: Parameters<InstanceType<typeof PlatformRateLimitConfig>['platformRateLimitConfigDetailPost']>) => getAPI(PlatformRateLimitConfig).platformRateLimitConfigDetailPost(...args);
export const platformRateLimitConfigUpdatePost = (...args: Parameters<InstanceType<typeof PlatformRateLimitConfig>['platformRateLimitConfigUpdatePost']>) => getAPI(PlatformRateLimitConfig).platformRateLimitConfigUpdatePost(...args);
