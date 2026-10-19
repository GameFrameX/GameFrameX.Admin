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
  TenantSessionKeyAddRequest,
  TenantSessionKeyEntityAddPostData,
  TenantSessionKeyEntityDeletePostData,
  TenantSessionKeyEntityDetailGetData,
  TenantSessionKeyEntityDisablePostData,
  TenantSessionKeyEntityEnablePostData,
  TenantSessionKeyEntityPagePostData,
  TenantSessionKeyEntityRenewPostData,
  TenantSessionKeyEntityRotatePostData,
  TenantSessionKeyRenewRequest,
  TenantSessionKeyRotateRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseIdRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantSessionKeyEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantSessionKeyEntity
   * @name TenantSessionKeyEntityAddPost
   * @summary 新增会话密钥：服务端随机生成 Kid + AES 密钥明文，响应一次性回显。 适合为同一应用并行放置多把 key（多版本客户端各持一把互不影响）；密钥域归属 AppId。
   * @request POST:/api/tenantSessionKeyEntity/add
   */
  tenantSessionKeyEntityAddPost = (
    data: TenantSessionKeyAddRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantSessionKeyEntityAddPostData, any>({
      path: `/api/tenantSessionKeyEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantSessionKeyEntity
   * @name TenantSessionKeyEntityDeletePost
   * @summary 禁止删除会话密钥：历史 key 是在途加密信封与未升级客户端的解密依据，删除会直接打断存量流量； 请改用禁用（立即拒用）或到期退场（租约自然排除）。
   * @request POST:/api/tenantSessionKeyEntity/delete
   */
  tenantSessionKeyEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantSessionKeyEntityDeletePostData, any>({
      path: `/api/tenantSessionKeyEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantSessionKeyEntity
   * @name TenantSessionKeyEntityDetailGet
   * @summary 查询单条会话密钥元数据（密钥脱敏 ******）。
   * @request GET:/api/tenantSessionKeyEntity/detail
   */
  tenantSessionKeyEntityDetailGet = (
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
    this.http.request<TenantSessionKeyEntityDetailGetData, any>({
      path: `/api/tenantSessionKeyEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantSessionKeyEntity
   * @name TenantSessionKeyEntityDisablePost
   * @summary 禁用密钥：立即拒用该 key 的信封（60105），不影响同应用其余 key（单 key 失效不殃及全应用）。
   * @request POST:/api/tenantSessionKeyEntity/disable
   */
  tenantSessionKeyEntityDisablePost = (
    data: BaseIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantSessionKeyEntityDisablePostData, any>({
      path: `/api/tenantSessionKeyEntity/disable`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantSessionKeyEntity
   * @name TenantSessionKeyEntityEnablePost
   * @summary 启用密钥。
   * @request POST:/api/tenantSessionKeyEntity/enable
   */
  tenantSessionKeyEntityEnablePost = (
    data: BaseIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantSessionKeyEntityEnablePostData, any>({
      path: `/api/tenantSessionKeyEntity/enable`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantSessionKeyEntity
   * @name TenantSessionKeyEntityPagePost
   * @summary 分页查询会话密钥（元数据，密钥固定脱敏 ******，明文永不回显）。
   * @request POST:/api/tenantSessionKeyEntity/page
   */
  tenantSessionKeyEntityPagePost = (
    data: PageTenantSessionKeyRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantSessionKeyEntityPagePostData, any>({
      path: `/api/tenantSessionKeyEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantSessionKeyEntity
   * @name TenantSessionKeyEntityRenewPost
   * @summary 续租：仅更新到期时间（null = 改为永不过期），不产生新密钥、不改动 Kid 与明文。 典型用途：RotateAsync 后客户端切换进度不及预期，延长旧 key 双活窗口。
   * @request POST:/api/tenantSessionKeyEntity/renew
   */
  tenantSessionKeyEntityRenewPost = (
    data: TenantSessionKeyRenewRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantSessionKeyEntityRenewPostData, any>({
      path: `/api/tenantSessionKeyEntity/renew`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantSessionKeyEntity
   * @name TenantSessionKeyEntityRotatePost
   * @summary 轮换会话密钥（双活语义）：新插一把立即生效的 key，旧 key 不禁用不删除 —— 新旧在旧 key 到期/禁用前同时可解密，客户端可在双活窗口内任意时刻切换，切换零中断。 当前最新生效 key 会被打 RotatedTime 标记（纯元数据，不影响解密）； 可选同时收窄旧 key 到期时间（OldKeyExpireTime）以控制双活窗口长度。
   * @request POST:/api/tenantSessionKeyEntity/rotate
   */
  tenantSessionKeyEntityRotatePost = (
    data: TenantSessionKeyRotateRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantSessionKeyEntityRotatePostData, any>({
      path: `/api/tenantSessionKeyEntity/rotate`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const tenantSessionKeyEntityAddPost = (...args: Parameters<InstanceType<typeof TenantSessionKeyEntity>['tenantSessionKeyEntityAddPost']>) => getAPI(TenantSessionKeyEntity).tenantSessionKeyEntityAddPost(...args);
export const tenantSessionKeyEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantSessionKeyEntity>['tenantSessionKeyEntityDeletePost']>) => getAPI(TenantSessionKeyEntity).tenantSessionKeyEntityDeletePost(...args);
export const tenantSessionKeyEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantSessionKeyEntity>['tenantSessionKeyEntityDetailGet']>) => getAPI(TenantSessionKeyEntity).tenantSessionKeyEntityDetailGet(...args);
export const tenantSessionKeyEntityDisablePost = (...args: Parameters<InstanceType<typeof TenantSessionKeyEntity>['tenantSessionKeyEntityDisablePost']>) => getAPI(TenantSessionKeyEntity).tenantSessionKeyEntityDisablePost(...args);
export const tenantSessionKeyEntityEnablePost = (...args: Parameters<InstanceType<typeof TenantSessionKeyEntity>['tenantSessionKeyEntityEnablePost']>) => getAPI(TenantSessionKeyEntity).tenantSessionKeyEntityEnablePost(...args);
export const tenantSessionKeyEntityPagePost = (...args: Parameters<InstanceType<typeof TenantSessionKeyEntity>['tenantSessionKeyEntityPagePost']>) => getAPI(TenantSessionKeyEntity).tenantSessionKeyEntityPagePost(...args);
export const tenantSessionKeyEntityRenewPost = (...args: Parameters<InstanceType<typeof TenantSessionKeyEntity>['tenantSessionKeyEntityRenewPost']>) => getAPI(TenantSessionKeyEntity).tenantSessionKeyEntityRenewPost(...args);
export const tenantSessionKeyEntityRotatePost = (...args: Parameters<InstanceType<typeof TenantSessionKeyEntity>['tenantSessionKeyEntityRotatePost']>) => getAPI(TenantSessionKeyEntity).tenantSessionKeyEntityRotatePost(...args);
