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
  AddTenantGameMailEntityRequest,
  PageTenantGameMailEntityRequest,
  TenantGameMailEntityAddPostData,
  TenantGameMailEntityDeletePostData,
  TenantGameMailEntityDetailGetData,
  TenantGameMailEntityPagePostData,
  TenantGameMailEntitySendPostData,
  TenantGameMailEntityUpdatePostData,
  UpdateTenantGameMailEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseIdRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameMailEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameMailEntity
   * @name TenantGameMailEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameMailEntity/add
   */
  tenantGameMailEntityAddPost = (
    data: AddTenantGameMailEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameMailEntityAddPostData, any>({
      path: `/api/tenantGameMailEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailEntity
   * @name TenantGameMailEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameMailEntity/delete
   */
  tenantGameMailEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameMailEntityDeletePostData, any>({
      path: `/api/tenantGameMailEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailEntity
   * @name TenantGameMailEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameMailEntity/detail
   */
  tenantGameMailEntityDetailGet = (
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
    this.http.request<TenantGameMailEntityDetailGetData, any>({
      path: `/api/tenantGameMailEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailEntity
   * @name TenantGameMailEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameMailEntity/page
   */
  tenantGameMailEntityPagePost = (
    data: PageTenantGameMailEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameMailEntityPagePostData, any>({
      path: `/api/tenantGameMailEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailEntity
   * @name TenantGameMailEntitySendPost
   * @summary 发送游戏邮件
   * @request POST:/api/tenantGameMailEntity/send
   */
  tenantGameMailEntitySendPost = (
    data: BaseIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameMailEntitySendPostData, any>({
      path: `/api/tenantGameMailEntity/send`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailEntity
   * @name TenantGameMailEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameMailEntity/update
   */
  tenantGameMailEntityUpdatePost = (
    data: UpdateTenantGameMailEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameMailEntityUpdatePostData, any>({
      path: `/api/tenantGameMailEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameMailEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameMailEntity>['tenantGameMailEntityAddPost']>) => getAPI(TenantGameMailEntity).tenantGameMailEntityAddPost(...args);
export const tenantGameMailEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameMailEntity>['tenantGameMailEntityDeletePost']>) => getAPI(TenantGameMailEntity).tenantGameMailEntityDeletePost(...args);
export const tenantGameMailEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameMailEntity>['tenantGameMailEntityDetailGet']>) => getAPI(TenantGameMailEntity).tenantGameMailEntityDetailGet(...args);
export const tenantGameMailEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameMailEntity>['tenantGameMailEntityPagePost']>) => getAPI(TenantGameMailEntity).tenantGameMailEntityPagePost(...args);
export const tenantGameMailEntitySendPost = (...args: Parameters<InstanceType<typeof TenantGameMailEntity>['tenantGameMailEntitySendPost']>) => getAPI(TenantGameMailEntity).tenantGameMailEntitySendPost(...args);
export const tenantGameMailEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameMailEntity>['tenantGameMailEntityUpdatePost']>) => getAPI(TenantGameMailEntity).tenantGameMailEntityUpdatePost(...args);
