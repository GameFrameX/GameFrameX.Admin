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
  OnlineAssetLedgerQueryPagePostData,
  PageOnlineAssetLedgerRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class OnlineAssetLedgerQuery<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags onlineAssetLedgerQuery
   * @name OnlineAssetLedgerQueryPagePost
   * @summary 资产流水分页查询：玩家 + 货币/道具类型 + 时间窗过滤，游标翻页往返透传 Online。
   * @request POST:/api/onlineAssetLedgerQuery/page
   */
  onlineAssetLedgerQueryPagePost = (
    data: PageOnlineAssetLedgerRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<OnlineAssetLedgerQueryPagePostData, any>({
      path: `/api/onlineAssetLedgerQuery/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const onlineAssetLedgerQueryPagePost = (...args: Parameters<InstanceType<typeof OnlineAssetLedgerQuery>['onlineAssetLedgerQueryPagePost']>) => getAPI(OnlineAssetLedgerQuery).onlineAssetLedgerQueryPagePost(...args);
