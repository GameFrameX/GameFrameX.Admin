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
  PageTenantSessionKeyRequest,
  SessionKeyAddPostData,
  SessionKeyAddRequest,
  SessionKeyDeletePostData,
  SessionKeyDetailGetData,
  SessionKeyDisablePostData,
  SessionKeyEnablePostData,
  SessionKeyPagePostData,
  SessionKeyRenewPostData,
  SessionKeyRenewRequest,
  SessionKeyRotatePostData,
  SessionKeyRotateRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseIdRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class SessionKey<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags sessionKey
   * @name SessionKeyAddPost
   * @summary 新增会话密钥：服务端随机生成 Kid + AES 密钥明文，响应一次性回显。 适合为同一应用并行放置多把 key（多版本客户端各持一把互不影响）；多版本客户端共用主库密钥环。
   * @request POST:/api/sessionKey/add
   */
  sessionKeyAddPost = (
    data: SessionKeyAddRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SessionKeyAddPostData, any>({
      path: `/api/sessionKey/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags sessionKey
   * @name SessionKeyDeletePost
   * @summary 禁止删除会话密钥：历史 key 是在途加密信封与未升级客户端的解密依据，删除会直接打断存量流量； 请改用禁用（立即拒用）或到期退场（租约自然排除）。
   * @request POST:/api/sessionKey/delete
   */
  sessionKeyDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SessionKeyDeletePostData, any>({
      path: `/api/sessionKey/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags sessionKey
   * @name SessionKeyDetailGet
   * @summary 查询单条会话密钥元数据（密钥脱敏 ******）。
   * @request GET:/api/sessionKey/detail
   */
  sessionKeyDetailGet = (
    query: {
      /**
       * @format int64
       * @min 1
       * @max 9223372036854776000
       */
      id: number;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<SessionKeyDetailGetData, any>({
      path: `/api/sessionKey/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags sessionKey
   * @name SessionKeyDisablePost
   * @summary 禁用密钥：立即拒用该 key 的信封（60105），不影响同应用其余 key（单 key 失效不殃及全应用）。
   * @request POST:/api/sessionKey/disable
   */
  sessionKeyDisablePost = (data: BaseIdRequest, params: RequestParams = {}) =>
    this.http.request<SessionKeyDisablePostData, any>({
      path: `/api/sessionKey/disable`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags sessionKey
   * @name SessionKeyEnablePost
   * @summary 启用密钥。
   * @request POST:/api/sessionKey/enable
   */
  sessionKeyEnablePost = (data: BaseIdRequest, params: RequestParams = {}) =>
    this.http.request<SessionKeyEnablePostData, any>({
      path: `/api/sessionKey/enable`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags sessionKey
   * @name SessionKeyPagePost
   * @summary 分页查询会话密钥（元数据，密钥固定脱敏 ******，明文永不回显）。
   * @request POST:/api/sessionKey/page
   */
  sessionKeyPagePost = (
    data: PageTenantSessionKeyRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SessionKeyPagePostData, any>({
      path: `/api/sessionKey/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags sessionKey
   * @name SessionKeyRenewPost
   * @summary 续租：仅更新到期时间（null = 改为永不过期），不产生新密钥、不改动 Kid 与明文。 典型用途：RotateAsync 后客户端切换进度不及预期，延长旧 key 双活窗口。
   * @request POST:/api/sessionKey/renew
   */
  sessionKeyRenewPost = (
    data: SessionKeyRenewRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SessionKeyRenewPostData, any>({
      path: `/api/sessionKey/renew`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags sessionKey
   * @name SessionKeyRotatePost
   * @summary 轮换会话密钥（双活语义）：新插一把立即生效的 key，旧 key 不禁用不删除 —— 新旧在旧 key 到期/禁用前同时可解密，客户端可在双活窗口内任意时刻切换，切换零中断。 当前最新生效 key 会被打 RotatedTime 标记（纯元数据，不影响解密）； 可选同时收窄旧 key 到期时间（OldKeyExpireTime）以控制双活窗口长度。
   * @request POST:/api/sessionKey/rotate
   */
  sessionKeyRotatePost = (
    data: SessionKeyRotateRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SessionKeyRotatePostData, any>({
      path: `/api/sessionKey/rotate`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const sessionKeyAddPost = (...args: Parameters<InstanceType<typeof SessionKey>['sessionKeyAddPost']>) => getAPI(SessionKey).sessionKeyAddPost(...args);
export const sessionKeyDeletePost = (...args: Parameters<InstanceType<typeof SessionKey>['sessionKeyDeletePost']>) => getAPI(SessionKey).sessionKeyDeletePost(...args);
export const sessionKeyDetailGet = (...args: Parameters<InstanceType<typeof SessionKey>['sessionKeyDetailGet']>) => getAPI(SessionKey).sessionKeyDetailGet(...args);
export const sessionKeyDisablePost = (...args: Parameters<InstanceType<typeof SessionKey>['sessionKeyDisablePost']>) => getAPI(SessionKey).sessionKeyDisablePost(...args);
export const sessionKeyEnablePost = (...args: Parameters<InstanceType<typeof SessionKey>['sessionKeyEnablePost']>) => getAPI(SessionKey).sessionKeyEnablePost(...args);
export const sessionKeyPagePost = (...args: Parameters<InstanceType<typeof SessionKey>['sessionKeyPagePost']>) => getAPI(SessionKey).sessionKeyPagePost(...args);
export const sessionKeyRenewPost = (...args: Parameters<InstanceType<typeof SessionKey>['sessionKeyRenewPost']>) => getAPI(SessionKey).sessionKeyRenewPost(...args);
export const sessionKeyRotatePost = (...args: Parameters<InstanceType<typeof SessionKey>['sessionKeyRotatePost']>) => getAPI(SessionKey).sessionKeyRotatePost(...args);
