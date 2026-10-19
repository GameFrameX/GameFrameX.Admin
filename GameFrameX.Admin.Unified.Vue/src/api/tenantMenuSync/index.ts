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
  SyncTenantMenuByPackageRequest,
  SyncTenantMenuRequest,
  TenantMenuSyncSyncAllPostData,
  TenantMenuSyncSyncPackagePostData,
  TenantMenuSyncSyncTenantPostData,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantMenuSync<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantMenuSync
   * @name TenantMenuSyncSyncAllPost
   * @summary 手动同步全部租户菜单（SuperAdmin 逃生通道；含套餐悬空租户，由 Applier 置 failed 不删不补）
   * @request POST:/api/tenantMenuSync/syncAll
   */
  tenantMenuSyncSyncAllPost = (params: RequestParams = {}) =>
    this.http.request<TenantMenuSyncSyncAllPostData, any>({
      path: `/api/tenantMenuSync/syncAll`,
      method: "POST",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantMenuSync
   * @name TenantMenuSyncSyncPackagePost
   * @summary 按套餐手动同步其全部租户菜单（SuperAdmin 逃生通道）
   * @request POST:/api/tenantMenuSync/syncPackage
   */
  tenantMenuSyncSyncPackagePost = (
    data: SyncTenantMenuByPackageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantMenuSyncSyncPackagePostData, any>({
      path: `/api/tenantMenuSync/syncPackage`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantMenuSync
   * @name TenantMenuSyncSyncTenantPost
   * @summary 手动同步单个租户菜单（SuperAdmin 逃生通道）
   * @request POST:/api/tenantMenuSync/syncTenant
   */
  tenantMenuSyncSyncTenantPost = (
    data: SyncTenantMenuRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantMenuSyncSyncTenantPostData, any>({
      path: `/api/tenantMenuSync/syncTenant`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const tenantMenuSyncSyncAllPost = (...args: Parameters<InstanceType<typeof TenantMenuSync>['tenantMenuSyncSyncAllPost']>) => getAPI(TenantMenuSync).tenantMenuSyncSyncAllPost(...args);
export const tenantMenuSyncSyncPackagePost = (...args: Parameters<InstanceType<typeof TenantMenuSync>['tenantMenuSyncSyncPackagePost']>) => getAPI(TenantMenuSync).tenantMenuSyncSyncPackagePost(...args);
export const tenantMenuSyncSyncTenantPost = (...args: Parameters<InstanceType<typeof TenantMenuSync>['tenantMenuSyncSyncTenantPost']>) => getAPI(TenantMenuSync).tenantMenuSyncSyncTenantPost(...args);
