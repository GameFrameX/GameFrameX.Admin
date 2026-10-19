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
  PlatformPasswordPolicyConfigDetailPostData,
  PlatformPasswordPolicyConfigUpdatePostData,
  PlatformPasswordPolicyConfigUpdateRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class PlatformPasswordPolicyConfig<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags platformPasswordPolicyConfig
   * @name PlatformPasswordPolicyConfigDetailPost
   * @summary 获取平台密码策略配置详情。仅返回 strongPassword/passwordStrengthValidation/passwordStrengthValidationMsg， 不包含任何 Cryptogram 密钥。
   * @request POST:/api/platformPasswordPolicyConfig/detail
   */
  platformPasswordPolicyConfigDetailPost = (params: RequestParams = {}) =>
    this.http.request<PlatformPasswordPolicyConfigDetailPostData, any>({
      path: `/api/platformPasswordPolicyConfig/detail`,
      method: "POST",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags platformPasswordPolicyConfig
   * @name PlatformPasswordPolicyConfigUpdatePost
   * @summary 保存平台密码策略配置。CAS 乐观锁冲突重试；成功后刷新同 Host 快照并镜像同步 CryptogramUtility 密码策略静态属性， 后续 TenantUserService.ChangePwdAsync 立即按新规则校验。
   * @request POST:/api/platformPasswordPolicyConfig/update
   */
  platformPasswordPolicyConfigUpdatePost = (
    data: PlatformPasswordPolicyConfigUpdateRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<PlatformPasswordPolicyConfigUpdatePostData, any>({
      path: `/api/platformPasswordPolicyConfig/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const platformPasswordPolicyConfigDetailPost = (...args: Parameters<InstanceType<typeof PlatformPasswordPolicyConfig>['platformPasswordPolicyConfigDetailPost']>) => getAPI(PlatformPasswordPolicyConfig).platformPasswordPolicyConfigDetailPost(...args);
export const platformPasswordPolicyConfigUpdatePost = (...args: Parameters<InstanceType<typeof PlatformPasswordPolicyConfig>['platformPasswordPolicyConfigUpdatePost']>) => getAPI(PlatformPasswordPolicyConfig).platformPasswordPolicyConfigUpdatePost(...args);
