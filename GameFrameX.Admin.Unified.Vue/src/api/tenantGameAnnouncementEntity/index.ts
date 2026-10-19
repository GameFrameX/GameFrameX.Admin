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
  AddTenantGameAnnouncementEntityRequest,
  PageTenantGameAnnouncementEntityRequest,
  TenantGameAnnouncementEntityAddPostData,
  TenantGameAnnouncementEntityDeletePostData,
  TenantGameAnnouncementEntityDetailGetData,
  TenantGameAnnouncementEntityDispatchOnlinePostData,
  TenantGameAnnouncementEntityPagePostData,
  TenantGameAnnouncementEntityUpdatePostData,
  UpdateTenantGameAnnouncementEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  OnlineLiveOpsSourceDispatchRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameAnnouncementEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameAnnouncementEntity
   * @name TenantGameAnnouncementEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameAnnouncementEntity/add
   */
  tenantGameAnnouncementEntityAddPost = (
    data: AddTenantGameAnnouncementEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAnnouncementEntityAddPostData, any>({
      path: `/api/tenantGameAnnouncementEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAnnouncementEntity
   * @name TenantGameAnnouncementEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameAnnouncementEntity/delete
   */
  tenantGameAnnouncementEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAnnouncementEntityDeletePostData, any>({
      path: `/api/tenantGameAnnouncementEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAnnouncementEntity
   * @name TenantGameAnnouncementEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameAnnouncementEntity/detail
   */
  tenantGameAnnouncementEntityDetailGet = (
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
    this.http.request<TenantGameAnnouncementEntityDetailGetData, any>({
      path: `/api/tenantGameAnnouncementEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAnnouncementEntity
   * @name TenantGameAnnouncementEntityDispatchOnlinePost
   * @summary 经 Online 统一下发当前公告（来源 change C343 / vault:C8 S7.7）：转调 OnlineLiveOpsDispatchService 单一入口（pushAnnouncement 通道）， 来源类型固定注入 Announcement，存在性 / 应用归属 / 作用域校验在单一入口完成。
   * @request POST:/api/tenantGameAnnouncementEntity/dispatchOnline
   */
  tenantGameAnnouncementEntityDispatchOnlinePost = (
    data: OnlineLiveOpsSourceDispatchRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAnnouncementEntityDispatchOnlinePostData, any>({
      path: `/api/tenantGameAnnouncementEntity/dispatchOnline`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAnnouncementEntity
   * @name TenantGameAnnouncementEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameAnnouncementEntity/page
   */
  tenantGameAnnouncementEntityPagePost = (
    data: PageTenantGameAnnouncementEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAnnouncementEntityPagePostData, any>({
      path: `/api/tenantGameAnnouncementEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAnnouncementEntity
   * @name TenantGameAnnouncementEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameAnnouncementEntity/update
   */
  tenantGameAnnouncementEntityUpdatePost = (
    data: UpdateTenantGameAnnouncementEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAnnouncementEntityUpdatePostData, any>({
      path: `/api/tenantGameAnnouncementEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameAnnouncementEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameAnnouncementEntity>['tenantGameAnnouncementEntityAddPost']>) => getAPI(TenantGameAnnouncementEntity).tenantGameAnnouncementEntityAddPost(...args);
export const tenantGameAnnouncementEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameAnnouncementEntity>['tenantGameAnnouncementEntityDeletePost']>) => getAPI(TenantGameAnnouncementEntity).tenantGameAnnouncementEntityDeletePost(...args);
export const tenantGameAnnouncementEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameAnnouncementEntity>['tenantGameAnnouncementEntityDetailGet']>) => getAPI(TenantGameAnnouncementEntity).tenantGameAnnouncementEntityDetailGet(...args);
export const tenantGameAnnouncementEntityDispatchOnlinePost = (...args: Parameters<InstanceType<typeof TenantGameAnnouncementEntity>['tenantGameAnnouncementEntityDispatchOnlinePost']>) => getAPI(TenantGameAnnouncementEntity).tenantGameAnnouncementEntityDispatchOnlinePost(...args);
export const tenantGameAnnouncementEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameAnnouncementEntity>['tenantGameAnnouncementEntityPagePost']>) => getAPI(TenantGameAnnouncementEntity).tenantGameAnnouncementEntityPagePost(...args);
export const tenantGameAnnouncementEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameAnnouncementEntity>['tenantGameAnnouncementEntityUpdatePost']>) => getAPI(TenantGameAnnouncementEntity).tenantGameAnnouncementEntityUpdatePost(...args);
