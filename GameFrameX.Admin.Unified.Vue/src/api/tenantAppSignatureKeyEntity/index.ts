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
  PageTenantAppSignatureKeyRequest,
  TenantAppSignatureKeyAddRequest,
  TenantAppSignatureKeyEntityAddPostData,
  TenantAppSignatureKeyEntityDeletePostData,
  TenantAppSignatureKeyEntityDisablePostData,
  TenantAppSignatureKeyEntityEnablePostData,
  TenantAppSignatureKeyEntityPagePostData,
  TenantAppSignatureKeyEntityRenewPostData,
  TenantAppSignatureKeyEntityRotatePostData,
  TenantAppSignatureKeyRenewRequest,
  TenantAppSignatureKeyRotateRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseIdRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantAppSignatureKeyEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantAppSignatureKeyEntity
   * @name TenantAppSignatureKeyEntityAddPost
   * @summary 新增签名密钥：服务端随机生成 Kid + HMAC 密钥明文，响应一次性回显。 适合为同一应用并行放置多把 key（多版本客户端各持一把互不影响）；密钥域归属 AppId。
   * @request POST:/api/tenantAppSignatureKeyEntity/add
   */
  tenantAppSignatureKeyEntityAddPost = (
    data: TenantAppSignatureKeyAddRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantAppSignatureKeyEntityAddPostData, any>({
      path: `/api/tenantAppSignatureKeyEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantAppSignatureKeyEntity
   * @name TenantAppSignatureKeyEntityDeletePost
   * @summary 禁止删除签名密钥：历史 key 是在途请求与未升级客户端的验签依据，删除会直接 60102 打断存量流量； 请改用禁用（立即拒签）或到期退场（租约自然排除）。
   * @request POST:/api/tenantAppSignatureKeyEntity/delete
   */
  tenantAppSignatureKeyEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantAppSignatureKeyEntityDeletePostData, any>({
      path: `/api/tenantAppSignatureKeyEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantAppSignatureKeyEntity
   * @name TenantAppSignatureKeyEntityDisablePost
   * @summary 禁用密钥：立即拒签该 key 的请求（60103），不影响同应用其余 key（单 key 失效不殃及全应用，R5）。
   * @request POST:/api/tenantAppSignatureKeyEntity/disable
   */
  tenantAppSignatureKeyEntityDisablePost = (
    data: BaseIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantAppSignatureKeyEntityDisablePostData, any>({
      path: `/api/tenantAppSignatureKeyEntity/disable`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantAppSignatureKeyEntity
   * @name TenantAppSignatureKeyEntityEnablePost
   * @summary 启用密钥。
   * @request POST:/api/tenantAppSignatureKeyEntity/enable
   */
  tenantAppSignatureKeyEntityEnablePost = (
    data: BaseIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantAppSignatureKeyEntityEnablePostData, any>({
      path: `/api/tenantAppSignatureKeyEntity/enable`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantAppSignatureKeyEntity
   * @name TenantAppSignatureKeyEntityPagePost
   * @summary 分页查询签名密钥（元数据，密钥固定脱敏 ******，明文永不回显）。
   * @request POST:/api/tenantAppSignatureKeyEntity/page
   */
  tenantAppSignatureKeyEntityPagePost = (
    data: PageTenantAppSignatureKeyRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantAppSignatureKeyEntityPagePostData, any>({
      path: `/api/tenantAppSignatureKeyEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantAppSignatureKeyEntity
   * @name TenantAppSignatureKeyEntityRenewPost
   * @summary 续租：仅更新到期时间（null = 改为永不过期），不产生新密钥、不改动 Kid 与明文。 典型用途：RotateAsync 后客户端切换进度不及预期，延长旧 key 双活窗口。
   * @request POST:/api/tenantAppSignatureKeyEntity/renew
   */
  tenantAppSignatureKeyEntityRenewPost = (
    data: TenantAppSignatureKeyRenewRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantAppSignatureKeyEntityRenewPostData, any>({
      path: `/api/tenantAppSignatureKeyEntity/renew`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantAppSignatureKeyEntity
   * @name TenantAppSignatureKeyEntityRotatePost
   * @summary 轮换签名密钥（双活语义）：新插一把立即生效的 key，旧 key 不禁用不删除 —— 新旧在旧 key 到期/禁用前同时可验签，客户端可在双活窗口内任意时刻切换，切换零中断。 当前最新生效 key 会被打 RotatedTime 标记（纯元数据，不影响验签）； 可选同时收窄旧 key 到期时间（OldKeyExpireTime）以控制双活窗口长度。
   * @request POST:/api/tenantAppSignatureKeyEntity/rotate
   */
  tenantAppSignatureKeyEntityRotatePost = (
    data: TenantAppSignatureKeyRotateRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantAppSignatureKeyEntityRotatePostData, any>({
      path: `/api/tenantAppSignatureKeyEntity/rotate`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const tenantAppSignatureKeyEntityAddPost = (...args: Parameters<InstanceType<typeof TenantAppSignatureKeyEntity>['tenantAppSignatureKeyEntityAddPost']>) => getAPI(TenantAppSignatureKeyEntity).tenantAppSignatureKeyEntityAddPost(...args);
export const tenantAppSignatureKeyEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantAppSignatureKeyEntity>['tenantAppSignatureKeyEntityDeletePost']>) => getAPI(TenantAppSignatureKeyEntity).tenantAppSignatureKeyEntityDeletePost(...args);
export const tenantAppSignatureKeyEntityDisablePost = (...args: Parameters<InstanceType<typeof TenantAppSignatureKeyEntity>['tenantAppSignatureKeyEntityDisablePost']>) => getAPI(TenantAppSignatureKeyEntity).tenantAppSignatureKeyEntityDisablePost(...args);
export const tenantAppSignatureKeyEntityEnablePost = (...args: Parameters<InstanceType<typeof TenantAppSignatureKeyEntity>['tenantAppSignatureKeyEntityEnablePost']>) => getAPI(TenantAppSignatureKeyEntity).tenantAppSignatureKeyEntityEnablePost(...args);
export const tenantAppSignatureKeyEntityPagePost = (...args: Parameters<InstanceType<typeof TenantAppSignatureKeyEntity>['tenantAppSignatureKeyEntityPagePost']>) => getAPI(TenantAppSignatureKeyEntity).tenantAppSignatureKeyEntityPagePost(...args);
export const tenantAppSignatureKeyEntityRenewPost = (...args: Parameters<InstanceType<typeof TenantAppSignatureKeyEntity>['tenantAppSignatureKeyEntityRenewPost']>) => getAPI(TenantAppSignatureKeyEntity).tenantAppSignatureKeyEntityRenewPost(...args);
export const tenantAppSignatureKeyEntityRotatePost = (...args: Parameters<InstanceType<typeof TenantAppSignatureKeyEntity>['tenantAppSignatureKeyEntityRotatePost']>) => getAPI(TenantAppSignatureKeyEntity).tenantAppSignatureKeyEntityRotatePost(...args);
