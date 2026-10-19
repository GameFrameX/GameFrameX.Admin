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
  PlatformCaptchaConfigDetailPostData,
  PlatformCaptchaConfigUpdatePostData,
  PlatformCaptchaConfigUpdateRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class PlatformCaptchaConfig<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags platformCaptchaConfig
   * @name PlatformCaptchaConfigDetailPost
   * @summary 获取平台图形验证码配置详情。
   * @request POST:/api/platformCaptchaConfig/detail
   */
  platformCaptchaConfigDetailPost = (params: RequestParams = {}) =>
    this.http.request<PlatformCaptchaConfigDetailPostData, any>({
      path: `/api/platformCaptchaConfig/detail`,
      method: "POST",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags platformCaptchaConfig
   * @name PlatformCaptchaConfigUpdatePost
   * @summary 保存平台图形验证码配置。每次保存原子递增版本，使旧验证码立即失效。 采用乐观并发：以读取时的完整 !:SystemConfigEntity.Value 作为比较基准执行条件更新， 受影响行数为 0 表示期间有并发写入，重读后重算版本并重试，保证版本号在并发下严格单调递增。
   * @request POST:/api/platformCaptchaConfig/update
   */
  platformCaptchaConfigUpdatePost = (
    data: PlatformCaptchaConfigUpdateRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<PlatformCaptchaConfigUpdatePostData, any>({
      path: `/api/platformCaptchaConfig/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const platformCaptchaConfigDetailPost = (...args: Parameters<InstanceType<typeof PlatformCaptchaConfig>['platformCaptchaConfigDetailPost']>) => getAPI(PlatformCaptchaConfig).platformCaptchaConfigDetailPost(...args);
export const platformCaptchaConfigUpdatePost = (...args: Parameters<InstanceType<typeof PlatformCaptchaConfig>['platformCaptchaConfigUpdatePost']>) => getAPI(PlatformCaptchaConfig).platformCaptchaConfigUpdatePost(...args);
