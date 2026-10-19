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
  PageTenantGameLoginTicketKeyRequest,
  TenantGameLoginTicketKeyEntityDeletePostData,
  TenantGameLoginTicketKeyEntityDetailGetData,
  TenantGameLoginTicketKeyEntityDisablePostData,
  TenantGameLoginTicketKeyEntityEnablePostData,
  TenantGameLoginTicketKeyEntityPagePostData,
  TenantGameLoginTicketKeyEntityRotatePostData,
  TenantGameLoginTicketKeyRotateRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseIdRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameLoginTicketKeyEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameLoginTicketKeyEntity
   * @name TenantGameLoginTicketKeyEntityDeletePost
   * @summary 禁止删除签名密钥：历史 key 是 Server 验签在途票据的依据，删除会导致验签失败；请改用禁用或轮换。
   * @request POST:/api/tenantGameLoginTicketKeyEntity/delete
   */
  tenantGameLoginTicketKeyEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameLoginTicketKeyEntityDeletePostData, any>({
      path: `/api/tenantGameLoginTicketKeyEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameLoginTicketKeyEntity
   * @name TenantGameLoginTicketKeyEntityDetailGet
   * @summary 密钥详情（仅公钥与元数据，私钥永不回显）。
   * @request GET:/api/tenantGameLoginTicketKeyEntity/detail
   */
  tenantGameLoginTicketKeyEntityDetailGet = (
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
    this.http.request<TenantGameLoginTicketKeyEntityDetailGetData, any>({
      path: `/api/tenantGameLoginTicketKeyEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameLoginTicketKeyEntity
   * @name TenantGameLoginTicketKeyEntityDisablePost
   * @summary 禁用密钥。
   * @request POST:/api/tenantGameLoginTicketKeyEntity/disable
   */
  tenantGameLoginTicketKeyEntityDisablePost = (
    data: BaseIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameLoginTicketKeyEntityDisablePostData, any>({
      path: `/api/tenantGameLoginTicketKeyEntity/disable`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameLoginTicketKeyEntity
   * @name TenantGameLoginTicketKeyEntityEnablePost
   * @summary 启用密钥。
   * @request POST:/api/tenantGameLoginTicketKeyEntity/enable
   */
  tenantGameLoginTicketKeyEntityEnablePost = (
    data: BaseIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameLoginTicketKeyEntityEnablePostData, any>({
      path: `/api/tenantGameLoginTicketKeyEntity/enable`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameLoginTicketKeyEntity
   * @name TenantGameLoginTicketKeyEntityPagePost
   * @summary 分页查询签名密钥（仅公钥与元数据，私钥永不回显）。
   * @request POST:/api/tenantGameLoginTicketKeyEntity/page
   */
  tenantGameLoginTicketKeyEntityPagePost = (
    data: PageTenantGameLoginTicketKeyRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameLoginTicketKeyEntityPagePostData, any>({
      path: `/api/tenantGameLoginTicketKeyEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameLoginTicketKeyEntity
   * @name TenantGameLoginTicketKeyEntityRotatePost
   * @summary 手动轮换（C373）：立即为指定应用生成新 key（ExpireTime 为 null 则永不过期）； 旧 key 打 RotatedTime 标记并收窄到期时间到退场时刻（默认 now + 7 天、上限 now + 30 天）， 到点被生效查询自然排除——轮换后旧公钥在退场窗口内仍可验签在途票据，窗口外自然退场。
   * @request POST:/api/tenantGameLoginTicketKeyEntity/rotate
   */
  tenantGameLoginTicketKeyEntityRotatePost = (
    data: TenantGameLoginTicketKeyRotateRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameLoginTicketKeyEntityRotatePostData, any>({
      path: `/api/tenantGameLoginTicketKeyEntity/rotate`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameLoginTicketKeyEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameLoginTicketKeyEntity>['tenantGameLoginTicketKeyEntityDeletePost']>) => getAPI(TenantGameLoginTicketKeyEntity).tenantGameLoginTicketKeyEntityDeletePost(...args);
export const tenantGameLoginTicketKeyEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameLoginTicketKeyEntity>['tenantGameLoginTicketKeyEntityDetailGet']>) => getAPI(TenantGameLoginTicketKeyEntity).tenantGameLoginTicketKeyEntityDetailGet(...args);
export const tenantGameLoginTicketKeyEntityDisablePost = (...args: Parameters<InstanceType<typeof TenantGameLoginTicketKeyEntity>['tenantGameLoginTicketKeyEntityDisablePost']>) => getAPI(TenantGameLoginTicketKeyEntity).tenantGameLoginTicketKeyEntityDisablePost(...args);
export const tenantGameLoginTicketKeyEntityEnablePost = (...args: Parameters<InstanceType<typeof TenantGameLoginTicketKeyEntity>['tenantGameLoginTicketKeyEntityEnablePost']>) => getAPI(TenantGameLoginTicketKeyEntity).tenantGameLoginTicketKeyEntityEnablePost(...args);
export const tenantGameLoginTicketKeyEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameLoginTicketKeyEntity>['tenantGameLoginTicketKeyEntityPagePost']>) => getAPI(TenantGameLoginTicketKeyEntity).tenantGameLoginTicketKeyEntityPagePost(...args);
export const tenantGameLoginTicketKeyEntityRotatePost = (...args: Parameters<InstanceType<typeof TenantGameLoginTicketKeyEntity>['tenantGameLoginTicketKeyEntityRotatePost']>) => getAPI(TenantGameLoginTicketKeyEntity).tenantGameLoginTicketKeyEntityRotatePost(...args);
