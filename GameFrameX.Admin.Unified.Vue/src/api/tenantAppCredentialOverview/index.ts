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
  TenantAppCredentialOverviewGetGetData,
} from "./types";
import { HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantAppCredentialOverview<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantAppCredentialOverview
   * @name TenantAppCredentialOverviewGetGet
   * @summary 应用凭证概览：① 应用身份 ② 传输签名 ③ 票据与令牌 ④ 出站对接四类卡片 + 用途矩阵（后端常量单一事实来源）。
   * @request GET:/api/tenantAppCredentialOverview/get
   */
  tenantAppCredentialOverviewGetGet = (
    query?: {
      /**
       * 目标应用 Id（须属于当前租户）。
       * @format int64
       */
      appId?: number;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<TenantAppCredentialOverviewGetGetData, any>({
      path: `/api/tenantAppCredentialOverview/get`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
}

export const tenantAppCredentialOverviewGetGet = (...args: Parameters<InstanceType<typeof TenantAppCredentialOverview>['tenantAppCredentialOverviewGetGet']>) => getAPI(TenantAppCredentialOverview).tenantAppCredentialOverviewGetGet(...args);
