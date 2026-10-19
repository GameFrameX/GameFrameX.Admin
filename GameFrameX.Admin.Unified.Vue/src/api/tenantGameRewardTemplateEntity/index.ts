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
  AddTenantGameRewardTemplateEntityRequest,
  PageTenantGameRewardTemplateEntityRequest,
  TenantGameRewardTemplateEntityAddPostData,
  TenantGameRewardTemplateEntityCopySnapshotPostData,
  TenantGameRewardTemplateEntityDeletePostData,
  TenantGameRewardTemplateEntityDetailGetData,
  TenantGameRewardTemplateEntityPagePostData,
  TenantGameRewardTemplateEntitySelectPostData,
  TenantGameRewardTemplateEntitySelectStringPostData,
  TenantGameRewardTemplateEntityUpdatePostData,
  TenantGameRewardTemplateSnapshotRequest,
  UpdateTenantGameRewardTemplateEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameRewardTemplateEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameRewardTemplateEntity
   * @name TenantGameRewardTemplateEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameRewardTemplateEntity/add
   */
  tenantGameRewardTemplateEntityAddPost = (
    data: AddTenantGameRewardTemplateEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRewardTemplateEntityAddPostData, any>({
      path: `/api/tenantGameRewardTemplateEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRewardTemplateEntity
   * @name TenantGameRewardTemplateEntityCopySnapshotPost
   * @summary 复制当前模板奖励快照。业务侧应保存返回的 JSON 快照，而不是运行时引用模板。
   * @request POST:/api/tenantGameRewardTemplateEntity/copySnapshot
   */
  tenantGameRewardTemplateEntityCopySnapshotPost = (
    data: TenantGameRewardTemplateSnapshotRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRewardTemplateEntityCopySnapshotPostData, any>({
      path: `/api/tenantGameRewardTemplateEntity/copySnapshot`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRewardTemplateEntity
   * @name TenantGameRewardTemplateEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameRewardTemplateEntity/delete
   */
  tenantGameRewardTemplateEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRewardTemplateEntityDeletePostData, any>({
      path: `/api/tenantGameRewardTemplateEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRewardTemplateEntity
   * @name TenantGameRewardTemplateEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameRewardTemplateEntity/detail
   */
  tenantGameRewardTemplateEntityDetailGet = (
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
    this.http.request<TenantGameRewardTemplateEntityDetailGetData, any>({
      path: `/api/tenantGameRewardTemplateEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRewardTemplateEntity
   * @name TenantGameRewardTemplateEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameRewardTemplateEntity/page
   */
  tenantGameRewardTemplateEntityPagePost = (
    data: PageTenantGameRewardTemplateEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRewardTemplateEntityPagePostData, any>({
      path: `/api/tenantGameRewardTemplateEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRewardTemplateEntity
   * @name TenantGameRewardTemplateEntitySelectPost
   * @summary 获取当前应用下启用的奖励模板。
   * @request POST:/api/tenantGameRewardTemplateEntity/select
   */
  tenantGameRewardTemplateEntitySelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRewardTemplateEntitySelectPostData, any>({
      path: `/api/tenantGameRewardTemplateEntity/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRewardTemplateEntity
   * @name TenantGameRewardTemplateEntitySelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/tenantGameRewardTemplateEntity/selectString
   */
  tenantGameRewardTemplateEntitySelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRewardTemplateEntitySelectStringPostData, any>({
      path: `/api/tenantGameRewardTemplateEntity/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRewardTemplateEntity
   * @name TenantGameRewardTemplateEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameRewardTemplateEntity/update
   */
  tenantGameRewardTemplateEntityUpdatePost = (
    data: UpdateTenantGameRewardTemplateEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRewardTemplateEntityUpdatePostData, any>({
      path: `/api/tenantGameRewardTemplateEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameRewardTemplateEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameRewardTemplateEntity>['tenantGameRewardTemplateEntityAddPost']>) => getAPI(TenantGameRewardTemplateEntity).tenantGameRewardTemplateEntityAddPost(...args);
export const tenantGameRewardTemplateEntityCopySnapshotPost = (...args: Parameters<InstanceType<typeof TenantGameRewardTemplateEntity>['tenantGameRewardTemplateEntityCopySnapshotPost']>) => getAPI(TenantGameRewardTemplateEntity).tenantGameRewardTemplateEntityCopySnapshotPost(...args);
export const tenantGameRewardTemplateEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameRewardTemplateEntity>['tenantGameRewardTemplateEntityDeletePost']>) => getAPI(TenantGameRewardTemplateEntity).tenantGameRewardTemplateEntityDeletePost(...args);
export const tenantGameRewardTemplateEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameRewardTemplateEntity>['tenantGameRewardTemplateEntityDetailGet']>) => getAPI(TenantGameRewardTemplateEntity).tenantGameRewardTemplateEntityDetailGet(...args);
export const tenantGameRewardTemplateEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameRewardTemplateEntity>['tenantGameRewardTemplateEntityPagePost']>) => getAPI(TenantGameRewardTemplateEntity).tenantGameRewardTemplateEntityPagePost(...args);
export const tenantGameRewardTemplateEntitySelectPost = (...args: Parameters<InstanceType<typeof TenantGameRewardTemplateEntity>['tenantGameRewardTemplateEntitySelectPost']>) => getAPI(TenantGameRewardTemplateEntity).tenantGameRewardTemplateEntitySelectPost(...args);
export const tenantGameRewardTemplateEntitySelectStringPost = (...args: Parameters<InstanceType<typeof TenantGameRewardTemplateEntity>['tenantGameRewardTemplateEntitySelectStringPost']>) => getAPI(TenantGameRewardTemplateEntity).tenantGameRewardTemplateEntitySelectStringPost(...args);
export const tenantGameRewardTemplateEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameRewardTemplateEntity>['tenantGameRewardTemplateEntityUpdatePost']>) => getAPI(TenantGameRewardTemplateEntity).tenantGameRewardTemplateEntityUpdatePost(...args);
