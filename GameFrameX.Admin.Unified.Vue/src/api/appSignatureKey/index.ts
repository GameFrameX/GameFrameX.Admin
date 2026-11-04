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
  AppSignatureKeyAddPostData,
  AppSignatureKeyAddRequest,
  AppSignatureKeyDeletePostData,
  AppSignatureKeyDisablePostData,
  AppSignatureKeyEnablePostData,
  AppSignatureKeyPagePostData,
  AppSignatureKeyRenewPostData,
  AppSignatureKeyRenewRequest,
  AppSignatureKeyRotatePostData,
  AppSignatureKeyRotateRequest,
  PageTenantAppSignatureKeyRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseIdRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class AppSignatureKey<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags appSignatureKey
   * @name AppSignatureKeyAddPost
   * @summary 新增签名密钥：服务端随机生成 Kid + HMAC 密钥明文，响应一次性回显。 适合为同一应用并行放置多把 key（多版本客户端各持一把互不影响）；多版本客户端共用主库密钥环。
   * @request POST:/api/appSignatureKey/add
   */
  appSignatureKeyAddPost = (
    data: AppSignatureKeyAddRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<AppSignatureKeyAddPostData, any>({
      path: `/api/appSignatureKey/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags appSignatureKey
   * @name AppSignatureKeyDeletePost
   * @summary 禁止删除签名密钥：历史 key 是在途请求与未升级客户端的验签依据，删除会直接 60102 打断存量流量； 请改用禁用（立即拒签）或到期退场（租约自然排除）。
   * @request POST:/api/appSignatureKey/delete
   */
  appSignatureKeyDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<AppSignatureKeyDeletePostData, any>({
      path: `/api/appSignatureKey/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags appSignatureKey
   * @name AppSignatureKeyDisablePost
   * @summary 禁用密钥：立即拒签该 key 的请求（60103），不影响同应用其余 key（单 key 失效不殃及全应用，R5）。
   * @request POST:/api/appSignatureKey/disable
   */
  appSignatureKeyDisablePost = (
    data: BaseIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<AppSignatureKeyDisablePostData, any>({
      path: `/api/appSignatureKey/disable`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags appSignatureKey
   * @name AppSignatureKeyEnablePost
   * @summary 启用密钥。
   * @request POST:/api/appSignatureKey/enable
   */
  appSignatureKeyEnablePost = (
    data: BaseIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<AppSignatureKeyEnablePostData, any>({
      path: `/api/appSignatureKey/enable`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags appSignatureKey
   * @name AppSignatureKeyPagePost
   * @summary 分页查询签名密钥（元数据，密钥固定脱敏 ******，明文永不回显）。
   * @request POST:/api/appSignatureKey/page
   */
  appSignatureKeyPagePost = (
    data: PageTenantAppSignatureKeyRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<AppSignatureKeyPagePostData, any>({
      path: `/api/appSignatureKey/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags appSignatureKey
   * @name AppSignatureKeyRenewPost
   * @summary 续租：仅更新到期时间（null = 改为永不过期），不产生新密钥、不改动 Kid 与明文。 典型用途：RotateAsync 后客户端切换进度不及预期，延长旧 key 双活窗口。
   * @request POST:/api/appSignatureKey/renew
   */
  appSignatureKeyRenewPost = (
    data: AppSignatureKeyRenewRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<AppSignatureKeyRenewPostData, any>({
      path: `/api/appSignatureKey/renew`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags appSignatureKey
   * @name AppSignatureKeyRotatePost
   * @summary 轮换签名密钥（双活语义）：新插一把立即生效的 key，旧 key 不禁用不删除 —— 新旧在旧 key 到期/禁用前同时可验签，客户端可在双活窗口内任意时刻切换，切换零中断。 当前最新生效 key 会被打 RotatedTime 标记（纯元数据，不影响验签）； 可选同时收窄旧 key 到期时间（OldKeyExpireTime）以控制双活窗口长度。
   * @request POST:/api/appSignatureKey/rotate
   */
  appSignatureKeyRotatePost = (
    data: AppSignatureKeyRotateRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<AppSignatureKeyRotatePostData, any>({
      path: `/api/appSignatureKey/rotate`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const appSignatureKeyAddPost = (...args: Parameters<InstanceType<typeof AppSignatureKey>['appSignatureKeyAddPost']>) => getAPI(AppSignatureKey).appSignatureKeyAddPost(...args);
export const appSignatureKeyDeletePost = (...args: Parameters<InstanceType<typeof AppSignatureKey>['appSignatureKeyDeletePost']>) => getAPI(AppSignatureKey).appSignatureKeyDeletePost(...args);
export const appSignatureKeyDisablePost = (...args: Parameters<InstanceType<typeof AppSignatureKey>['appSignatureKeyDisablePost']>) => getAPI(AppSignatureKey).appSignatureKeyDisablePost(...args);
export const appSignatureKeyEnablePost = (...args: Parameters<InstanceType<typeof AppSignatureKey>['appSignatureKeyEnablePost']>) => getAPI(AppSignatureKey).appSignatureKeyEnablePost(...args);
export const appSignatureKeyPagePost = (...args: Parameters<InstanceType<typeof AppSignatureKey>['appSignatureKeyPagePost']>) => getAPI(AppSignatureKey).appSignatureKeyPagePost(...args);
export const appSignatureKeyRenewPost = (...args: Parameters<InstanceType<typeof AppSignatureKey>['appSignatureKeyRenewPost']>) => getAPI(AppSignatureKey).appSignatureKeyRenewPost(...args);
export const appSignatureKeyRotatePost = (...args: Parameters<InstanceType<typeof AppSignatureKey>['appSignatureKeyRotatePost']>) => getAPI(AppSignatureKey).appSignatureKeyRotatePost(...args);
