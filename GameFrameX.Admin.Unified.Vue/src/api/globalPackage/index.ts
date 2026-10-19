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
  AddTenantPackageRequest,
  DeleteTenantPackageRequest,
  GlobalPackageAddPostData,
  GlobalPackageCurrentTenantPackageGetData,
  GlobalPackageDeletePostData,
  GlobalPackageDetailGetData,
  GlobalPackagePagePostData,
  GlobalPackageTenantExpirationStatusGetData,
  GlobalPackageUpdatePostData,
  PageTenantPackageRequest,
  UpdateTenantPackageRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GlobalPackage<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags globalPackage
   * @name GlobalPackageAddPost
   * @summary 增加套餐
   * @request POST:/api/globalPackage/add
   */
  globalPackageAddPost = (
    data: AddTenantPackageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalPackageAddPostData, any>({
      path: `/api/globalPackage/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags globalPackage
   * @name GlobalPackageCurrentTenantPackageGet
   * @summary 获取当前租户的套餐信息（只读，租户管理员可查看）
   * @request GET:/api/globalPackage/currentTenantPackage
   */
  globalPackageCurrentTenantPackageGet = (params: RequestParams = {}) =>
    this.http.request<GlobalPackageCurrentTenantPackageGetData, any>({
      path: `/api/globalPackage/currentTenantPackage`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalPackage
   * @name GlobalPackageDeletePost
   * @summary 删除套餐
   * @request POST:/api/globalPackage/delete
   */
  globalPackageDeletePost = (
    data: DeleteTenantPackageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalPackageDeletePostData, any>({
      path: `/api/globalPackage/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags globalPackage
   * @name GlobalPackageDetailGet
   * @summary 获取套餐详情（含关联菜单）
   * @request GET:/api/globalPackage/detail
   */
  globalPackageDetailGet = (
    query?: {
      /** @format int64 */
      id?: number;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalPackageDetailGetData, any>({
      path: `/api/globalPackage/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalPackage
   * @name GlobalPackagePagePost
   * @summary 获取套餐分页列表
   * @request POST:/api/globalPackage/page
   */
  globalPackagePagePost = (
    data: PageTenantPackageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalPackagePagePostData, any>({
      path: `/api/globalPackage/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalPackage
   * @name GlobalPackageTenantExpirationStatusGet
   * @summary 获取当前租户的到期状态（前端横幅用）
   * @request GET:/api/globalPackage/tenantExpirationStatus
   */
  globalPackageTenantExpirationStatusGet = (params: RequestParams = {}) =>
    this.http.request<GlobalPackageTenantExpirationStatusGetData, any>({
      path: `/api/globalPackage/tenantExpirationStatus`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalPackage
   * @name GlobalPackageUpdatePost
   * @summary 更新套餐
   * @request POST:/api/globalPackage/update
   */
  globalPackageUpdatePost = (
    data: UpdateTenantPackageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalPackageUpdatePostData, any>({
      path: `/api/globalPackage/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const globalPackageAddPost = (...args: Parameters<InstanceType<typeof GlobalPackage>['globalPackageAddPost']>) => getAPI(GlobalPackage).globalPackageAddPost(...args);
export const globalPackageCurrentTenantPackageGet = (...args: Parameters<InstanceType<typeof GlobalPackage>['globalPackageCurrentTenantPackageGet']>) => getAPI(GlobalPackage).globalPackageCurrentTenantPackageGet(...args);
export const globalPackageDeletePost = (...args: Parameters<InstanceType<typeof GlobalPackage>['globalPackageDeletePost']>) => getAPI(GlobalPackage).globalPackageDeletePost(...args);
export const globalPackageDetailGet = (...args: Parameters<InstanceType<typeof GlobalPackage>['globalPackageDetailGet']>) => getAPI(GlobalPackage).globalPackageDetailGet(...args);
export const globalPackagePagePost = (...args: Parameters<InstanceType<typeof GlobalPackage>['globalPackagePagePost']>) => getAPI(GlobalPackage).globalPackagePagePost(...args);
export const globalPackageTenantExpirationStatusGet = (...args: Parameters<InstanceType<typeof GlobalPackage>['globalPackageTenantExpirationStatusGet']>) => getAPI(GlobalPackage).globalPackageTenantExpirationStatusGet(...args);
export const globalPackageUpdatePost = (...args: Parameters<InstanceType<typeof GlobalPackage>['globalPackageUpdatePost']>) => getAPI(GlobalPackage).globalPackageUpdatePost(...args);
