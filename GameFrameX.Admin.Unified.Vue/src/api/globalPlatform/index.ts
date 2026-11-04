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
  GlobalPlatformSelectPostData,
  GlobalPlatformSelectStringPostData,
} from "./types";
import {
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GlobalPlatform<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags globalPlatform
   * @name GlobalPlatformSelectPost
   * @summary 获取平台选择器列表
   * @request POST:/api/globalPlatform/select
   */
  globalPlatformSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalPlatformSelectPostData, any>({
      path: `/api/globalPlatform/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalPlatform
   * @name GlobalPlatformSelectStringPost
   * @summary 获取平台选择器列表（字符串 ID）
   * @request POST:/api/globalPlatform/selectString
   */
  globalPlatformSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalPlatformSelectStringPostData, any>({
      path: `/api/globalPlatform/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const globalPlatformSelectPost = (...args: Parameters<InstanceType<typeof GlobalPlatform>['globalPlatformSelectPost']>) => getAPI(GlobalPlatform).globalPlatformSelectPost(...args);
export const globalPlatformSelectStringPost = (...args: Parameters<InstanceType<typeof GlobalPlatform>['globalPlatformSelectStringPost']>) => getAPI(GlobalPlatform).globalPlatformSelectStringPost(...args);
