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
  AddPrintRequest,
  DeletePrintRequest,
  PagePrintRequest,
  TenantPrintAddPostData,
  TenantPrintDeletePostData,
  TenantPrintPagePostData,
  TenantPrintUpdatePostData,
  UpdatePrintRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantPrint<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantPrint
   * @name TenantPrintAddPost
   * @summary 增加打印模板
   * @request POST:/api/tenantPrint/add
   */
  tenantPrintAddPost = (data: AddPrintRequest, params: RequestParams = {}) =>
    this.http.request<TenantPrintAddPostData, any>({
      path: `/api/tenantPrint/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantPrint
   * @name TenantPrintDeletePost
   * @summary 删除打印模板
   * @request POST:/api/tenantPrint/delete
   */
  tenantPrintDeletePost = (
    data: DeletePrintRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantPrintDeletePostData, any>({
      path: `/api/tenantPrint/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantPrint
   * @name TenantPrintPagePost
   * @summary 获取打印模板列表
   * @request POST:/api/tenantPrint/page
   */
  tenantPrintPagePost = (data: PagePrintRequest, params: RequestParams = {}) =>
    this.http.request<TenantPrintPagePostData, any>({
      path: `/api/tenantPrint/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantPrint
   * @name TenantPrintUpdatePost
   * @summary 更新打印模板
   * @request POST:/api/tenantPrint/update
   */
  tenantPrintUpdatePost = (
    data: UpdatePrintRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantPrintUpdatePostData, any>({
      path: `/api/tenantPrint/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantPrintAddPost = (...args: Parameters<InstanceType<typeof TenantPrint>['tenantPrintAddPost']>) => getAPI(TenantPrint).tenantPrintAddPost(...args);
export const tenantPrintDeletePost = (...args: Parameters<InstanceType<typeof TenantPrint>['tenantPrintDeletePost']>) => getAPI(TenantPrint).tenantPrintDeletePost(...args);
export const tenantPrintPagePost = (...args: Parameters<InstanceType<typeof TenantPrint>['tenantPrintPagePost']>) => getAPI(TenantPrint).tenantPrintPagePost(...args);
export const tenantPrintUpdatePost = (...args: Parameters<InstanceType<typeof TenantPrint>['tenantPrintUpdatePost']>) => getAPI(TenantPrint).tenantPrintUpdatePost(...args);
