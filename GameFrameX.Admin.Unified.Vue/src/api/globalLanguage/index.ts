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
  GlobalLanguageSelectPostData,
  GlobalLanguageSelectStringPostData,
} from "./types";
import {
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GlobalLanguage<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags globalLanguage
   * @name GlobalLanguageSelectPost
   * @summary 获取语言选择器列表
   * @request POST:/api/globalLanguage/select
   */
  globalLanguageSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalLanguageSelectPostData, any>({
      path: `/api/globalLanguage/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalLanguage
   * @name GlobalLanguageSelectStringPost
   * @summary 获取语言选择器列表（字符串 ID）
   * @request POST:/api/globalLanguage/selectString
   */
  globalLanguageSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalLanguageSelectStringPostData, any>({
      path: `/api/globalLanguage/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const globalLanguageSelectPost = (...args: Parameters<InstanceType<typeof GlobalLanguage>['globalLanguageSelectPost']>) => getAPI(GlobalLanguage).globalLanguageSelectPost(...args);
export const globalLanguageSelectStringPost = (...args: Parameters<InstanceType<typeof GlobalLanguage>['globalLanguageSelectStringPost']>) => getAPI(GlobalLanguage).globalLanguageSelectStringPost(...args);
