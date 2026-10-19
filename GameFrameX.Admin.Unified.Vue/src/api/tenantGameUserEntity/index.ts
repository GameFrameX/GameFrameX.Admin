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
  AddTenantGameUserEntityByMailRequest,
  DeleteTenantGameUserEntityRequest,
  PageTenantGameUserEntityRequest,
  TenantGameUserEntityAddByMailPostData,
  TenantGameUserEntityAddPostData,
  TenantGameUserEntityBanPostData,
  TenantGameUserEntityDeletePostData,
  TenantGameUserEntityDetailGetData,
  TenantGameUserEntityPagePostData,
  TenantGameUserEntitySelectPostData,
  TenantGameUserEntitySelectStringPostData,
  TenantGameUserEntityUnsealPostData,
  TenantGameUserEntityUpdatePostData,
  UpdateTenantGameUserEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameUserEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameUserEntity
   * @name TenantGameUserEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameUserEntity/add
   */
  tenantGameUserEntityAddPost = (
    data: AddTenantGameUserEntityByMailRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameUserEntityAddPostData, any>({
      path: `/api/tenantGameUserEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameUserEntity
   * @name TenantGameUserEntityAddByMailPost
   * @summary 通过邮箱增加账号
   * @request POST:/api/tenantGameUserEntity/addByMail
   */
  tenantGameUserEntityAddByMailPost = (
    data: AddTenantGameUserEntityByMailRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameUserEntityAddByMailPostData, any>({
      path: `/api/tenantGameUserEntity/addByMail`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameUserEntity
   * @name TenantGameUserEntityBanPost
   * @summary 封禁游戏账号
   * @request POST:/api/tenantGameUserEntity/ban
   */
  tenantGameUserEntityBanPost = (
    data: DeleteTenantGameUserEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameUserEntityBanPostData, any>({
      path: `/api/tenantGameUserEntity/ban`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameUserEntity
   * @name TenantGameUserEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameUserEntity/delete
   */
  tenantGameUserEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameUserEntityDeletePostData, any>({
      path: `/api/tenantGameUserEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameUserEntity
   * @name TenantGameUserEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameUserEntity/detail
   */
  tenantGameUserEntityDetailGet = (
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
    this.http.request<TenantGameUserEntityDetailGetData, any>({
      path: `/api/tenantGameUserEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameUserEntity
   * @name TenantGameUserEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameUserEntity/page
   */
  tenantGameUserEntityPagePost = (
    data: PageTenantGameUserEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameUserEntityPagePostData, any>({
      path: `/api/tenantGameUserEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameUserEntity
   * @name TenantGameUserEntitySelectPost
   * @summary 获取选择器列表
   * @request POST:/api/tenantGameUserEntity/select
   */
  tenantGameUserEntitySelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameUserEntitySelectPostData, any>({
      path: `/api/tenantGameUserEntity/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameUserEntity
   * @name TenantGameUserEntitySelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/tenantGameUserEntity/selectString
   */
  tenantGameUserEntitySelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameUserEntitySelectStringPostData, any>({
      path: `/api/tenantGameUserEntity/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameUserEntity
   * @name TenantGameUserEntityUnsealPost
   * @summary 解除封禁游戏账号
   * @request POST:/api/tenantGameUserEntity/unseal
   */
  tenantGameUserEntityUnsealPost = (
    data: DeleteTenantGameUserEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameUserEntityUnsealPostData, any>({
      path: `/api/tenantGameUserEntity/unseal`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameUserEntity
   * @name TenantGameUserEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameUserEntity/update
   */
  tenantGameUserEntityUpdatePost = (
    data: UpdateTenantGameUserEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameUserEntityUpdatePostData, any>({
      path: `/api/tenantGameUserEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameUserEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameUserEntity>['tenantGameUserEntityAddPost']>) => getAPI(TenantGameUserEntity).tenantGameUserEntityAddPost(...args);
export const tenantGameUserEntityAddByMailPost = (...args: Parameters<InstanceType<typeof TenantGameUserEntity>['tenantGameUserEntityAddByMailPost']>) => getAPI(TenantGameUserEntity).tenantGameUserEntityAddByMailPost(...args);
export const tenantGameUserEntityBanPost = (...args: Parameters<InstanceType<typeof TenantGameUserEntity>['tenantGameUserEntityBanPost']>) => getAPI(TenantGameUserEntity).tenantGameUserEntityBanPost(...args);
export const tenantGameUserEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameUserEntity>['tenantGameUserEntityDeletePost']>) => getAPI(TenantGameUserEntity).tenantGameUserEntityDeletePost(...args);
export const tenantGameUserEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameUserEntity>['tenantGameUserEntityDetailGet']>) => getAPI(TenantGameUserEntity).tenantGameUserEntityDetailGet(...args);
export const tenantGameUserEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameUserEntity>['tenantGameUserEntityPagePost']>) => getAPI(TenantGameUserEntity).tenantGameUserEntityPagePost(...args);
export const tenantGameUserEntitySelectPost = (...args: Parameters<InstanceType<typeof TenantGameUserEntity>['tenantGameUserEntitySelectPost']>) => getAPI(TenantGameUserEntity).tenantGameUserEntitySelectPost(...args);
export const tenantGameUserEntitySelectStringPost = (...args: Parameters<InstanceType<typeof TenantGameUserEntity>['tenantGameUserEntitySelectStringPost']>) => getAPI(TenantGameUserEntity).tenantGameUserEntitySelectStringPost(...args);
export const tenantGameUserEntityUnsealPost = (...args: Parameters<InstanceType<typeof TenantGameUserEntity>['tenantGameUserEntityUnsealPost']>) => getAPI(TenantGameUserEntity).tenantGameUserEntityUnsealPost(...args);
export const tenantGameUserEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameUserEntity>['tenantGameUserEntityUpdatePost']>) => getAPI(TenantGameUserEntity).tenantGameUserEntityUpdatePost(...args);
