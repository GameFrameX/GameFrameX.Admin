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
  AddTenantGameRedeemCodeUseRecordEntityRequest,
  PageTenantGameRedeemCodeUseRecordEntityRequest,
  TenantGameRedeemCodeUseRecordEntityAddPostData,
  TenantGameRedeemCodeUseRecordEntityDeletePostData,
  TenantGameRedeemCodeUseRecordEntityDetailGetData,
  TenantGameRedeemCodeUseRecordEntityPagePostData,
  TenantGameRedeemCodeUseRecordEntityTenantGameAreaEntityAreaIdDropdownGetData,
  TenantGameRedeemCodeUseRecordEntityUpdatePostData,
  UpdateTenantGameRedeemCodeUseRecordEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameRedeemCodeUseRecordEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameRedeemCodeUseRecordEntity
   * @name TenantGameRedeemCodeUseRecordEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameRedeemCodeUseRecordEntity/add
   */
  tenantGameRedeemCodeUseRecordEntityAddPost = (
    data: AddTenantGameRedeemCodeUseRecordEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRedeemCodeUseRecordEntityAddPostData, any>({
      path: `/api/tenantGameRedeemCodeUseRecordEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRedeemCodeUseRecordEntity
   * @name TenantGameRedeemCodeUseRecordEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameRedeemCodeUseRecordEntity/delete
   */
  tenantGameRedeemCodeUseRecordEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRedeemCodeUseRecordEntityDeletePostData, any>({
      path: `/api/tenantGameRedeemCodeUseRecordEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRedeemCodeUseRecordEntity
   * @name TenantGameRedeemCodeUseRecordEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameRedeemCodeUseRecordEntity/detail
   */
  tenantGameRedeemCodeUseRecordEntityDetailGet = (
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
    this.http.request<TenantGameRedeemCodeUseRecordEntityDetailGetData, any>({
      path: `/api/tenantGameRedeemCodeUseRecordEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRedeemCodeUseRecordEntity
   * @name TenantGameRedeemCodeUseRecordEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameRedeemCodeUseRecordEntity/page
   */
  tenantGameRedeemCodeUseRecordEntityPagePost = (
    data: PageTenantGameRedeemCodeUseRecordEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRedeemCodeUseRecordEntityPagePostData, any>({
      path: `/api/tenantGameRedeemCodeUseRecordEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRedeemCodeUseRecordEntity
   * @name TenantGameRedeemCodeUseRecordEntityTenantGameAreaEntityAreaIdDropdownGet
   * @summary 获取区服下拉列表（AreaId 外键关联）
   * @request GET:/api/tenantGameRedeemCodeUseRecordEntity/tenantGameAreaEntityAreaIdDropdown
   */
  tenantGameRedeemCodeUseRecordEntityTenantGameAreaEntityAreaIdDropdownGet = (
    params: RequestParams = {},
  ) =>
    this.http.request<
      TenantGameRedeemCodeUseRecordEntityTenantGameAreaEntityAreaIdDropdownGetData,
      any
    >({
      path: `/api/tenantGameRedeemCodeUseRecordEntity/tenantGameAreaEntityAreaIdDropdown`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRedeemCodeUseRecordEntity
   * @name TenantGameRedeemCodeUseRecordEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameRedeemCodeUseRecordEntity/update
   */
  tenantGameRedeemCodeUseRecordEntityUpdatePost = (
    data: UpdateTenantGameRedeemCodeUseRecordEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRedeemCodeUseRecordEntityUpdatePostData, any>({
      path: `/api/tenantGameRedeemCodeUseRecordEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameRedeemCodeUseRecordEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameRedeemCodeUseRecordEntity>['tenantGameRedeemCodeUseRecordEntityAddPost']>) => getAPI(TenantGameRedeemCodeUseRecordEntity).tenantGameRedeemCodeUseRecordEntityAddPost(...args);
export const tenantGameRedeemCodeUseRecordEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameRedeemCodeUseRecordEntity>['tenantGameRedeemCodeUseRecordEntityDeletePost']>) => getAPI(TenantGameRedeemCodeUseRecordEntity).tenantGameRedeemCodeUseRecordEntityDeletePost(...args);
export const tenantGameRedeemCodeUseRecordEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameRedeemCodeUseRecordEntity>['tenantGameRedeemCodeUseRecordEntityDetailGet']>) => getAPI(TenantGameRedeemCodeUseRecordEntity).tenantGameRedeemCodeUseRecordEntityDetailGet(...args);
export const tenantGameRedeemCodeUseRecordEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameRedeemCodeUseRecordEntity>['tenantGameRedeemCodeUseRecordEntityPagePost']>) => getAPI(TenantGameRedeemCodeUseRecordEntity).tenantGameRedeemCodeUseRecordEntityPagePost(...args);
export const tenantGameRedeemCodeUseRecordEntityTenantGameAreaEntityAreaIdDropdownGet = (...args: Parameters<InstanceType<typeof TenantGameRedeemCodeUseRecordEntity>['tenantGameRedeemCodeUseRecordEntityTenantGameAreaEntityAreaIdDropdownGet']>) => getAPI(TenantGameRedeemCodeUseRecordEntity).tenantGameRedeemCodeUseRecordEntityTenantGameAreaEntityAreaIdDropdownGet(...args);
export const tenantGameRedeemCodeUseRecordEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameRedeemCodeUseRecordEntity>['tenantGameRedeemCodeUseRecordEntityUpdatePost']>) => getAPI(TenantGameRedeemCodeUseRecordEntity).tenantGameRedeemCodeUseRecordEntityUpdatePost(...args);
