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
  AddOrganizationRequest,
  DeleteOrganizationRequest,
  TenantOrganizationAddPostData,
  TenantOrganizationDeletePostData,
  TenantOrganizationListGetData,
  TenantOrganizationUpdatePostData,
  UpdateOrganizationRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantOrganization<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantOrganization
   * @name TenantOrganizationAddPost
   * @summary 增加运营组织
   * @request POST:/api/tenantOrganization/add
   */
  tenantOrganizationAddPost = (
    data: AddOrganizationRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantOrganizationAddPostData, any>({
      path: `/api/tenantOrganization/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantOrganization
   * @name TenantOrganizationDeletePost
   * @summary 删除运营组织
   * @request POST:/api/tenantOrganization/delete
   */
  tenantOrganizationDeletePost = (
    data: DeleteOrganizationRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantOrganizationDeletePostData, any>({
      path: `/api/tenantOrganization/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantOrganization
   * @name TenantOrganizationListGet
   * @summary 获取运营组织列表
   * @request GET:/api/tenantOrganization/list
   */
  tenantOrganizationListGet = (
    query: {
      /** 名称 */
      name?: string;
      /** 编码 */
      code?: string;
      /** 组织类型 */
      type?: string;
      /**
       * @format int64
       * @min 1
       * @max 9223372036854776000
       */
      id: number;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<TenantOrganizationListGetData, any>({
      path: `/api/tenantOrganization/list`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantOrganization
   * @name TenantOrganizationUpdatePost
   * @summary 更新运营组织
   * @request POST:/api/tenantOrganization/update
   */
  tenantOrganizationUpdatePost = (
    data: UpdateOrganizationRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantOrganizationUpdatePostData, any>({
      path: `/api/tenantOrganization/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantOrganizationAddPost = (...args: Parameters<InstanceType<typeof TenantOrganization>['tenantOrganizationAddPost']>) => getAPI(TenantOrganization).tenantOrganizationAddPost(...args);
export const tenantOrganizationDeletePost = (...args: Parameters<InstanceType<typeof TenantOrganization>['tenantOrganizationDeletePost']>) => getAPI(TenantOrganization).tenantOrganizationDeletePost(...args);
export const tenantOrganizationListGet = (...args: Parameters<InstanceType<typeof TenantOrganization>['tenantOrganizationListGet']>) => getAPI(TenantOrganization).tenantOrganizationListGet(...args);
export const tenantOrganizationUpdatePost = (...args: Parameters<InstanceType<typeof TenantOrganization>['tenantOrganizationUpdatePost']>) => getAPI(TenantOrganization).tenantOrganizationUpdatePost(...args);
