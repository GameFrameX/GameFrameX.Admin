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
  PlatformJwtConfigDetailPostData,
  PlatformJwtConfigUpdatePostData,
  PlatformJwtConfigUpdateRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class PlatformJwtConfig<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags platformJwtConfig
   * @name PlatformJwtConfigDetailPost
   * @summary 获取平台 JWT 配置详情。签名密钥只返回掩码，不返回明文。
   * @request POST:/api/platformJwtConfig/detail
   */
  platformJwtConfigDetailPost = (params: RequestParams = {}) =>
    this.http.request<PlatformJwtConfigDetailPostData, any>({
      path: `/api/platformJwtConfig/detail`,
      method: "POST",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags platformJwtConfig
   * @name PlatformJwtConfigUpdatePost
   * @summary 保存平台 JWT 配置。每次成功保存都会递增全局令牌版本，使所有旧 AccessToken 和 RefreshToken 立即失效。
   * @request POST:/api/platformJwtConfig/update
   */
  platformJwtConfigUpdatePost = (
    data: PlatformJwtConfigUpdateRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<PlatformJwtConfigUpdatePostData, any>({
      path: `/api/platformJwtConfig/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const platformJwtConfigDetailPost = (...args: Parameters<InstanceType<typeof PlatformJwtConfig>['platformJwtConfigDetailPost']>) => getAPI(PlatformJwtConfig).platformJwtConfigDetailPost(...args);
export const platformJwtConfigUpdatePost = (...args: Parameters<InstanceType<typeof PlatformJwtConfig>['platformJwtConfigUpdatePost']>) => getAPI(PlatformJwtConfig).platformJwtConfigUpdatePost(...args);
