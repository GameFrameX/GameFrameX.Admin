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
  AddTenantGamePenaltyAppealEntityRequest,
  PageTenantGamePenaltyAppealEntityRequest,
  ReviewTenantGamePenaltyAppealEntityRequest,
  TenantGamePenaltyAppealEntityAddPostData,
  TenantGamePenaltyAppealEntityDeletePostData,
  TenantGamePenaltyAppealEntityDetailGetData,
  TenantGamePenaltyAppealEntityPagePostData,
  TenantGamePenaltyAppealEntityReviewPostData,
  TenantGamePenaltyAppealEntityUpdatePostData,
  UpdateTenantGamePenaltyAppealEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGamePenaltyAppealEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGamePenaltyAppealEntity
   * @name TenantGamePenaltyAppealEntityAddPost
   * @summary 提交申诉：校验处罚生效中、同一处罚无待审申诉；AppId 跟随处罚；Status 服务端强制为 Pending。
   * @request POST:/api/tenantGamePenaltyAppealEntity/add
   */
  tenantGamePenaltyAppealEntityAddPost = (
    data: AddTenantGamePenaltyAppealEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGamePenaltyAppealEntityAddPostData, any>({
      path: `/api/tenantGamePenaltyAppealEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGamePenaltyAppealEntity
   * @name TenantGamePenaltyAppealEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGamePenaltyAppealEntity/delete
   */
  tenantGamePenaltyAppealEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGamePenaltyAppealEntityDeletePostData, any>({
      path: `/api/tenantGamePenaltyAppealEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGamePenaltyAppealEntity
   * @name TenantGamePenaltyAppealEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGamePenaltyAppealEntity/detail
   */
  tenantGamePenaltyAppealEntityDetailGet = (
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
    this.http.request<TenantGamePenaltyAppealEntityDetailGetData, any>({
      path: `/api/tenantGamePenaltyAppealEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGamePenaltyAppealEntity
   * @name TenantGamePenaltyAppealEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGamePenaltyAppealEntity/page
   */
  tenantGamePenaltyAppealEntityPagePost = (
    data: PageTenantGamePenaltyAppealEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGamePenaltyAppealEntityPagePostData, any>({
      path: `/api/tenantGamePenaltyAppealEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGamePenaltyAppealEntity
   * @name TenantGamePenaltyAppealEntityReviewPost
   * @summary 审核申诉。仅 Pending 可审核。通过→撤销关联处罚并写 SourceAppealId 审计；驳回→Rejected。审核人/备注/时间服务端写。
   * @request POST:/api/tenantGamePenaltyAppealEntity/review
   */
  tenantGamePenaltyAppealEntityReviewPost = (
    data: ReviewTenantGamePenaltyAppealEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGamePenaltyAppealEntityReviewPostData, any>({
      path: `/api/tenantGamePenaltyAppealEntity/review`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGamePenaltyAppealEntity
   * @name TenantGamePenaltyAppealEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGamePenaltyAppealEntity/update
   */
  tenantGamePenaltyAppealEntityUpdatePost = (
    data: UpdateTenantGamePenaltyAppealEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGamePenaltyAppealEntityUpdatePostData, any>({
      path: `/api/tenantGamePenaltyAppealEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGamePenaltyAppealEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGamePenaltyAppealEntity>['tenantGamePenaltyAppealEntityAddPost']>) => getAPI(TenantGamePenaltyAppealEntity).tenantGamePenaltyAppealEntityAddPost(...args);
export const tenantGamePenaltyAppealEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGamePenaltyAppealEntity>['tenantGamePenaltyAppealEntityDeletePost']>) => getAPI(TenantGamePenaltyAppealEntity).tenantGamePenaltyAppealEntityDeletePost(...args);
export const tenantGamePenaltyAppealEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGamePenaltyAppealEntity>['tenantGamePenaltyAppealEntityDetailGet']>) => getAPI(TenantGamePenaltyAppealEntity).tenantGamePenaltyAppealEntityDetailGet(...args);
export const tenantGamePenaltyAppealEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGamePenaltyAppealEntity>['tenantGamePenaltyAppealEntityPagePost']>) => getAPI(TenantGamePenaltyAppealEntity).tenantGamePenaltyAppealEntityPagePost(...args);
export const tenantGamePenaltyAppealEntityReviewPost = (...args: Parameters<InstanceType<typeof TenantGamePenaltyAppealEntity>['tenantGamePenaltyAppealEntityReviewPost']>) => getAPI(TenantGamePenaltyAppealEntity).tenantGamePenaltyAppealEntityReviewPost(...args);
export const tenantGamePenaltyAppealEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGamePenaltyAppealEntity>['tenantGamePenaltyAppealEntityUpdatePost']>) => getAPI(TenantGamePenaltyAppealEntity).tenantGamePenaltyAppealEntityUpdatePost(...args);
