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
  AddTenantGameBannerAnnouncementEntityRequest,
  PageTenantGameBannerAnnouncementEntityRequest,
  TenantGameBannerAnnouncementEntityAddPostData,
  TenantGameBannerAnnouncementEntityDeletePostData,
  TenantGameBannerAnnouncementEntityDetailGetData,
  TenantGameBannerAnnouncementEntityPagePostData,
  TenantGameBannerAnnouncementEntityUpdatePostData,
  UpdateTenantGameBannerAnnouncementEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameBannerAnnouncementEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameBannerAnnouncementEntity
   * @name TenantGameBannerAnnouncementEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameBannerAnnouncementEntity/add
   */
  tenantGameBannerAnnouncementEntityAddPost = (
    data: AddTenantGameBannerAnnouncementEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameBannerAnnouncementEntityAddPostData, any>({
      path: `/api/tenantGameBannerAnnouncementEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameBannerAnnouncementEntity
   * @name TenantGameBannerAnnouncementEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameBannerAnnouncementEntity/delete
   */
  tenantGameBannerAnnouncementEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameBannerAnnouncementEntityDeletePostData, any>({
      path: `/api/tenantGameBannerAnnouncementEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameBannerAnnouncementEntity
   * @name TenantGameBannerAnnouncementEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameBannerAnnouncementEntity/detail
   */
  tenantGameBannerAnnouncementEntityDetailGet = (
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
    this.http.request<TenantGameBannerAnnouncementEntityDetailGetData, any>({
      path: `/api/tenantGameBannerAnnouncementEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameBannerAnnouncementEntity
   * @name TenantGameBannerAnnouncementEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameBannerAnnouncementEntity/page
   */
  tenantGameBannerAnnouncementEntityPagePost = (
    data: PageTenantGameBannerAnnouncementEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameBannerAnnouncementEntityPagePostData, any>({
      path: `/api/tenantGameBannerAnnouncementEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameBannerAnnouncementEntity
   * @name TenantGameBannerAnnouncementEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameBannerAnnouncementEntity/update
   */
  tenantGameBannerAnnouncementEntityUpdatePost = (
    data: UpdateTenantGameBannerAnnouncementEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameBannerAnnouncementEntityUpdatePostData, any>({
      path: `/api/tenantGameBannerAnnouncementEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameBannerAnnouncementEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameBannerAnnouncementEntity>['tenantGameBannerAnnouncementEntityAddPost']>) => getAPI(TenantGameBannerAnnouncementEntity).tenantGameBannerAnnouncementEntityAddPost(...args);
export const tenantGameBannerAnnouncementEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameBannerAnnouncementEntity>['tenantGameBannerAnnouncementEntityDeletePost']>) => getAPI(TenantGameBannerAnnouncementEntity).tenantGameBannerAnnouncementEntityDeletePost(...args);
export const tenantGameBannerAnnouncementEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameBannerAnnouncementEntity>['tenantGameBannerAnnouncementEntityDetailGet']>) => getAPI(TenantGameBannerAnnouncementEntity).tenantGameBannerAnnouncementEntityDetailGet(...args);
export const tenantGameBannerAnnouncementEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameBannerAnnouncementEntity>['tenantGameBannerAnnouncementEntityPagePost']>) => getAPI(TenantGameBannerAnnouncementEntity).tenantGameBannerAnnouncementEntityPagePost(...args);
export const tenantGameBannerAnnouncementEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameBannerAnnouncementEntity>['tenantGameBannerAnnouncementEntityUpdatePost']>) => getAPI(TenantGameBannerAnnouncementEntity).tenantGameBannerAnnouncementEntityUpdatePost(...args);
