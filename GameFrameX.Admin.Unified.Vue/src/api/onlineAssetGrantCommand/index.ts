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
  OnlineAssetGrantCommandGrantPostData,
  OnlineAssetGrantCommandRevokePostData,
  OnlineAssetGrantRequest,
  OnlineAssetRevokeRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class OnlineAssetGrantCommand<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags onlineAssetGrantCommand
   * @name OnlineAssetGrantCommandGrantPost
   * @summary 人工发放资产（高危端点）：明细 + 必填理由经 Online Grant API 执行，响应透传 Online 首次结果。
   * @request POST:/api/onlineAssetGrantCommand/grant
   */
  onlineAssetGrantCommandGrantPost = (
    data: OnlineAssetGrantRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<OnlineAssetGrantCommandGrantPostData, any>({
      path: `/api/onlineAssetGrantCommand/grant`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags onlineAssetGrantCommand
   * @name OnlineAssetGrantCommandRevokePost
   * @summary 人工撤销资产（高危端点）：按目标交易 Id 撤销已生效变更，经 Online Grant/Transaction API 执行。
   * @request POST:/api/onlineAssetGrantCommand/revoke
   */
  onlineAssetGrantCommandRevokePost = (
    data: OnlineAssetRevokeRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<OnlineAssetGrantCommandRevokePostData, any>({
      path: `/api/onlineAssetGrantCommand/revoke`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const onlineAssetGrantCommandGrantPost = (...args: Parameters<InstanceType<typeof OnlineAssetGrantCommand>['onlineAssetGrantCommandGrantPost']>) => getAPI(OnlineAssetGrantCommand).onlineAssetGrantCommandGrantPost(...args);
export const onlineAssetGrantCommandRevokePost = (...args: Parameters<InstanceType<typeof OnlineAssetGrantCommand>['onlineAssetGrantCommandRevokePost']>) => getAPI(OnlineAssetGrantCommand).onlineAssetGrantCommandRevokePost(...args);
