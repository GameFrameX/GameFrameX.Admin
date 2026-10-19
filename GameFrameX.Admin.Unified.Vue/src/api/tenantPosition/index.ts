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
  AddPosRequest,
  DeletePosRequest,
  TenantPositionAddPostData,
  TenantPositionDeletePostData,
  TenantPositionListGetData,
  TenantPositionUpdatePostData,
  UpdatePosRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantPosition<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantPosition
   * @name TenantPositionAddPost
   * @summary 增加职位
   * @request POST:/api/tenantPosition/add
   */
  tenantPositionAddPost = (data: AddPosRequest, params: RequestParams = {}) =>
    this.http.request<TenantPositionAddPostData, any>({
      path: `/api/tenantPosition/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantPosition
   * @name TenantPositionDeletePost
   * @summary 删除职位
   * @request POST:/api/tenantPosition/delete
   */
  tenantPositionDeletePost = (
    data: DeletePosRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantPositionDeletePostData, any>({
      path: `/api/tenantPosition/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantPosition
   * @name TenantPositionListGet
   * @summary 获取职位列表
   * @request GET:/api/tenantPosition/list
   */
  tenantPositionListGet = (
    query?: {
      /** 名称 */
      name?: string;
      /** 编码 */
      code?: string;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<TenantPositionListGetData, any>({
      path: `/api/tenantPosition/list`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantPosition
   * @name TenantPositionUpdatePost
   * @summary 更新职位
   * @request POST:/api/tenantPosition/update
   */
  tenantPositionUpdatePost = (
    data: UpdatePosRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantPositionUpdatePostData, any>({
      path: `/api/tenantPosition/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantPositionAddPost = (...args: Parameters<InstanceType<typeof TenantPosition>['tenantPositionAddPost']>) => getAPI(TenantPosition).tenantPositionAddPost(...args);
export const tenantPositionDeletePost = (...args: Parameters<InstanceType<typeof TenantPosition>['tenantPositionDeletePost']>) => getAPI(TenantPosition).tenantPositionDeletePost(...args);
export const tenantPositionListGet = (...args: Parameters<InstanceType<typeof TenantPosition>['tenantPositionListGet']>) => getAPI(TenantPosition).tenantPositionListGet(...args);
export const tenantPositionUpdatePost = (...args: Parameters<InstanceType<typeof TenantPosition>['tenantPositionUpdatePost']>) => getAPI(TenantPosition).tenantPositionUpdatePost(...args);
