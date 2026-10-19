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
  AddTenantGameAdvertisementRewardEntityRequest,
  PageTenantGameAdvertisementRewardEntityRequest,
  TenantGameAdvertisementRewardEntityAddPostData,
  TenantGameAdvertisementRewardEntityDeletePostData,
  TenantGameAdvertisementRewardEntityDetailGetData,
  TenantGameAdvertisementRewardEntityExportPostData,
  TenantGameAdvertisementRewardEntityPagePostData,
  TenantGameAdvertisementRewardEntityUpdatePostData,
  UpdateTenantGameAdvertisementRewardEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameAdvertisementRewardEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameAdvertisementRewardEntity
   * @name TenantGameAdvertisementRewardEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameAdvertisementRewardEntity/add
   */
  tenantGameAdvertisementRewardEntityAddPost = (
    data: AddTenantGameAdvertisementRewardEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAdvertisementRewardEntityAddPostData, any>({
      path: `/api/tenantGameAdvertisementRewardEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAdvertisementRewardEntity
   * @name TenantGameAdvertisementRewardEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameAdvertisementRewardEntity/delete
   */
  tenantGameAdvertisementRewardEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAdvertisementRewardEntityDeletePostData, any>({
      path: `/api/tenantGameAdvertisementRewardEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAdvertisementRewardEntity
   * @name TenantGameAdvertisementRewardEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameAdvertisementRewardEntity/detail
   */
  tenantGameAdvertisementRewardEntityDetailGet = (
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
    this.http.request<TenantGameAdvertisementRewardEntityDetailGetData, any>({
      path: `/api/tenantGameAdvertisementRewardEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAdvertisementRewardEntity
   * @name TenantGameAdvertisementRewardEntityExportPost
   * @summary 导出当前过滤条件下的账本列表（上限 1000 行；文件化由前端处理）。
   * @request POST:/api/tenantGameAdvertisementRewardEntity/export
   */
  tenantGameAdvertisementRewardEntityExportPost = (
    data: PageTenantGameAdvertisementRewardEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAdvertisementRewardEntityExportPostData, any>({
      path: `/api/tenantGameAdvertisementRewardEntity/export`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAdvertisementRewardEntity
   * @name TenantGameAdvertisementRewardEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameAdvertisementRewardEntity/page
   */
  tenantGameAdvertisementRewardEntityPagePost = (
    data: PageTenantGameAdvertisementRewardEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAdvertisementRewardEntityPagePostData, any>({
      path: `/api/tenantGameAdvertisementRewardEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAdvertisementRewardEntity
   * @name TenantGameAdvertisementRewardEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameAdvertisementRewardEntity/update
   */
  tenantGameAdvertisementRewardEntityUpdatePost = (
    data: UpdateTenantGameAdvertisementRewardEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAdvertisementRewardEntityUpdatePostData, any>({
      path: `/api/tenantGameAdvertisementRewardEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameAdvertisementRewardEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameAdvertisementRewardEntity>['tenantGameAdvertisementRewardEntityAddPost']>) => getAPI(TenantGameAdvertisementRewardEntity).tenantGameAdvertisementRewardEntityAddPost(...args);
export const tenantGameAdvertisementRewardEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameAdvertisementRewardEntity>['tenantGameAdvertisementRewardEntityDeletePost']>) => getAPI(TenantGameAdvertisementRewardEntity).tenantGameAdvertisementRewardEntityDeletePost(...args);
export const tenantGameAdvertisementRewardEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameAdvertisementRewardEntity>['tenantGameAdvertisementRewardEntityDetailGet']>) => getAPI(TenantGameAdvertisementRewardEntity).tenantGameAdvertisementRewardEntityDetailGet(...args);
export const tenantGameAdvertisementRewardEntityExportPost = (...args: Parameters<InstanceType<typeof TenantGameAdvertisementRewardEntity>['tenantGameAdvertisementRewardEntityExportPost']>) => getAPI(TenantGameAdvertisementRewardEntity).tenantGameAdvertisementRewardEntityExportPost(...args);
export const tenantGameAdvertisementRewardEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameAdvertisementRewardEntity>['tenantGameAdvertisementRewardEntityPagePost']>) => getAPI(TenantGameAdvertisementRewardEntity).tenantGameAdvertisementRewardEntityPagePost(...args);
export const tenantGameAdvertisementRewardEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameAdvertisementRewardEntity>['tenantGameAdvertisementRewardEntityUpdatePost']>) => getAPI(TenantGameAdvertisementRewardEntity).tenantGameAdvertisementRewardEntityUpdatePost(...args);
