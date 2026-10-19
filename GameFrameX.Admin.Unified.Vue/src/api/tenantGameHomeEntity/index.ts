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
  TenantGameHomeEntityGetOnlinePostData,
  TenantGameHomeEntityOnlineTrendPostData,
  TenantGameHomeEntityOverviewPostData,
} from "./types";
import { HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameHomeEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameHomeEntity
   * @name TenantGameHomeEntityGetOnlinePost
   * @summary 获取在线玩家信息
   * @request POST:/api/tenantGameHomeEntity/getOnline
   */
  tenantGameHomeEntityGetOnlinePost = (params: RequestParams = {}) =>
    this.http.request<TenantGameHomeEntityGetOnlinePostData, any>({
      path: `/api/tenantGameHomeEntity/getOnline`,
      method: "POST",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameHomeEntity
   * @name TenantGameHomeEntityOnlineTrendPost
   * @summary 首页 24h 在线人数时序：最近 24 个整点桶回读采样表，桶内取最后一次采样，缺桶为 null（C321 / GFX-730）
   * @request POST:/api/tenantGameHomeEntity/onlineTrend
   */
  tenantGameHomeEntityOnlineTrendPost = (params: RequestParams = {}) =>
    this.http.request<TenantGameHomeEntityOnlineTrendPostData, any>({
      path: `/api/tenantGameHomeEntity/onlineTrend`,
      method: "POST",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameHomeEntity
   * @name TenantGameHomeEntityOverviewPost
   * @summary 首页运营驾驶舱聚合读数：在线人数、今日订单（待发货/退款中细分）、玩家总数/今日新增、区服计数（C321 / GFX-730）
   * @request POST:/api/tenantGameHomeEntity/overview
   */
  tenantGameHomeEntityOverviewPost = (params: RequestParams = {}) =>
    this.http.request<TenantGameHomeEntityOverviewPostData, any>({
      path: `/api/tenantGameHomeEntity/overview`,
      method: "POST",
      format: "json",
      ...params,
    });
}

export const tenantGameHomeEntityGetOnlinePost = (...args: Parameters<InstanceType<typeof TenantGameHomeEntity>['tenantGameHomeEntityGetOnlinePost']>) => getAPI(TenantGameHomeEntity).tenantGameHomeEntityGetOnlinePost(...args);
export const tenantGameHomeEntityOnlineTrendPost = (...args: Parameters<InstanceType<typeof TenantGameHomeEntity>['tenantGameHomeEntityOnlineTrendPost']>) => getAPI(TenantGameHomeEntity).tenantGameHomeEntityOnlineTrendPost(...args);
export const tenantGameHomeEntityOverviewPost = (...args: Parameters<InstanceType<typeof TenantGameHomeEntity>['tenantGameHomeEntityOverviewPost']>) => getAPI(TenantGameHomeEntity).tenantGameHomeEntityOverviewPost(...args);
