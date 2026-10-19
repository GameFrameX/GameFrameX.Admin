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
  OnlineAssetTransactionQueryDetailGetData,
} from "./types";
import { HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class OnlineAssetTransactionQuery<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags onlineAssetTransactionQuery
   * @name OnlineAssetTransactionQueryDetailGet
   * @summary 交易详情查询：按 TransactionId 返回交易/Grant 全字段（变更来源完整可查的 Admin 展示半边）。
   * @request GET:/api/onlineAssetTransactionQuery/detail
   */
  onlineAssetTransactionQueryDetailGet = (
    query: {
      /**
       * 应用作用域（必填，经 OnlineScopeContextFactory 校验租户归属）。
       * @format int64
       * @min 1
       * @max 9223372036854776000
       */
      appId: number;
      /**
       * 区服作用域（必填，经 OnlineScopeContextFactory 校验租户与应用归属）。
       * @format int64
       * @min 1
       * @max 9223372036854776000
       */
      serverId: number;
      /**
       * 交易 Id（必填；流水行反查交易详情的唯一键）。
       * @maxLength 256
       */
      transactionId: string;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<OnlineAssetTransactionQueryDetailGetData, any>({
      path: `/api/onlineAssetTransactionQuery/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
}

export const onlineAssetTransactionQueryDetailGet = (...args: Parameters<InstanceType<typeof OnlineAssetTransactionQuery>['onlineAssetTransactionQueryDetailGet']>) => getAPI(OnlineAssetTransactionQuery).onlineAssetTransactionQueryDetailGet(...args);
