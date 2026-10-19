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
  OnlineAssetAuditQueryPagePostData,
  PageOnlineAssetAuditRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class OnlineAssetAuditQuery<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags onlineAssetAuditQuery
   * @name OnlineAssetAuditQueryPagePost
   * @summary 资产审计分页查询：操作者 / 命令类型 / 目标玩家 / 时间窗过滤，游标翻页往返透传 Online。
   * @request POST:/api/onlineAssetAuditQuery/page
   */
  onlineAssetAuditQueryPagePost = (
    data: PageOnlineAssetAuditRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<OnlineAssetAuditQueryPagePostData, any>({
      path: `/api/onlineAssetAuditQuery/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const onlineAssetAuditQueryPagePost = (...args: Parameters<InstanceType<typeof OnlineAssetAuditQuery>['onlineAssetAuditQueryPagePost']>) => getAPI(OnlineAssetAuditQuery).onlineAssetAuditQueryPagePost(...args);
