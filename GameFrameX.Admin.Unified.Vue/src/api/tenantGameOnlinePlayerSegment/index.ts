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
  AddTenantGameOnlinePlayerSegmentRequest,
  PageTenantGameOnlinePlayerSegmentRequest,
  SyncPlayerSegmentOnlineRequest,
  TenantGameOnlinePlayerSegmentAddPostData,
  TenantGameOnlinePlayerSegmentDeletePostData,
  TenantGameOnlinePlayerSegmentDetailGetData,
  TenantGameOnlinePlayerSegmentPagePostData,
  TenantGameOnlinePlayerSegmentSyncOnlinePostData,
  TenantGameOnlinePlayerSegmentUpdatePostData,
  UpdateTenantGameOnlinePlayerSegmentRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameOnlinePlayerSegment<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameOnlinePlayerSegment
   * @name TenantGameOnlinePlayerSegmentAddPost
   * @summary 新增
   * @request POST:/api/tenantGameOnlinePlayerSegment/add
   */
  tenantGameOnlinePlayerSegmentAddPost = (
    data: AddTenantGameOnlinePlayerSegmentRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameOnlinePlayerSegmentAddPostData, any>({
      path: `/api/tenantGameOnlinePlayerSegment/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameOnlinePlayerSegment
   * @name TenantGameOnlinePlayerSegmentDeletePost
   * @summary 删除：已被灰度版本行引用（任一状态）的分群不可删除——版本行是不可变审计历史，引用悬空会破坏回溯。
   * @request POST:/api/tenantGameOnlinePlayerSegment/delete
   */
  tenantGameOnlinePlayerSegmentDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameOnlinePlayerSegmentDeletePostData, any>({
      path: `/api/tenantGameOnlinePlayerSegment/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameOnlinePlayerSegment
   * @name TenantGameOnlinePlayerSegmentDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameOnlinePlayerSegment/detail
   */
  tenantGameOnlinePlayerSegmentDetailGet = (
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
    this.http.request<TenantGameOnlinePlayerSegmentDetailGetData, any>({
      path: `/api/tenantGameOnlinePlayerSegment/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameOnlinePlayerSegment
   * @name TenantGameOnlinePlayerSegmentPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameOnlinePlayerSegment/page
   */
  tenantGameOnlinePlayerSegmentPagePost = (
    data: PageTenantGameOnlinePlayerSegmentRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameOnlinePlayerSegmentPagePostData, any>({
      path: `/api/tenantGameOnlinePlayerSegment/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameOnlinePlayerSegment
   * @name TenantGameOnlinePlayerSegmentSyncOnlinePost
   * @summary 分群同步 Online：定义快照经统一下发入口（syncPlayerSegment 通道）送达 Online，Online 分配分群标识回写本行。 成功置 Synced 并回写回执；失败（含 Online 非 0 协议码镜像异常）置 Failed 落库可见后原样抛出，可重试。
   * @request POST:/api/tenantGameOnlinePlayerSegment/syncOnline
   */
  tenantGameOnlinePlayerSegmentSyncOnlinePost = (
    data: SyncPlayerSegmentOnlineRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameOnlinePlayerSegmentSyncOnlinePostData, any>({
      path: `/api/tenantGameOnlinePlayerSegment/syncOnline`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameOnlinePlayerSegment
   * @name TenantGameOnlinePlayerSegmentUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameOnlinePlayerSegment/update
   */
  tenantGameOnlinePlayerSegmentUpdatePost = (
    data: UpdateTenantGameOnlinePlayerSegmentRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameOnlinePlayerSegmentUpdatePostData, any>({
      path: `/api/tenantGameOnlinePlayerSegment/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameOnlinePlayerSegmentAddPost = (...args: Parameters<InstanceType<typeof TenantGameOnlinePlayerSegment>['tenantGameOnlinePlayerSegmentAddPost']>) => getAPI(TenantGameOnlinePlayerSegment).tenantGameOnlinePlayerSegmentAddPost(...args);
export const tenantGameOnlinePlayerSegmentDeletePost = (...args: Parameters<InstanceType<typeof TenantGameOnlinePlayerSegment>['tenantGameOnlinePlayerSegmentDeletePost']>) => getAPI(TenantGameOnlinePlayerSegment).tenantGameOnlinePlayerSegmentDeletePost(...args);
export const tenantGameOnlinePlayerSegmentDetailGet = (...args: Parameters<InstanceType<typeof TenantGameOnlinePlayerSegment>['tenantGameOnlinePlayerSegmentDetailGet']>) => getAPI(TenantGameOnlinePlayerSegment).tenantGameOnlinePlayerSegmentDetailGet(...args);
export const tenantGameOnlinePlayerSegmentPagePost = (...args: Parameters<InstanceType<typeof TenantGameOnlinePlayerSegment>['tenantGameOnlinePlayerSegmentPagePost']>) => getAPI(TenantGameOnlinePlayerSegment).tenantGameOnlinePlayerSegmentPagePost(...args);
export const tenantGameOnlinePlayerSegmentSyncOnlinePost = (...args: Parameters<InstanceType<typeof TenantGameOnlinePlayerSegment>['tenantGameOnlinePlayerSegmentSyncOnlinePost']>) => getAPI(TenantGameOnlinePlayerSegment).tenantGameOnlinePlayerSegmentSyncOnlinePost(...args);
export const tenantGameOnlinePlayerSegmentUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameOnlinePlayerSegment>['tenantGameOnlinePlayerSegmentUpdatePost']>) => getAPI(TenantGameOnlinePlayerSegment).tenantGameOnlinePlayerSegmentUpdatePost(...args);
