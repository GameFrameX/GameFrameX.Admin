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
  AddGlobalLanguageRequest,
  GlobalLanguageAddPostData,
  GlobalLanguageDeletePostData,
  GlobalLanguageDetailGetData,
  GlobalLanguagePagePostData,
  GlobalLanguageSelectPostData,
  GlobalLanguageSelectStringPostData,
  GlobalLanguageUpdatePostData,
  PageGlobalLanguageRequest,
  UpdateGlobalLanguageRequest,
} from "./types";
import {
  BaseDeleteRequest,
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
   * @name GlobalLanguageAddPost
   * @summary 新增
   * @request POST:/api/globalLanguage/add
   */
  globalLanguageAddPost = (
    data: AddGlobalLanguageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalLanguageAddPostData, any>({
      path: `/api/globalLanguage/add`,
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
   * @name GlobalLanguageDeletePost
   * @summary 删除
   * @request POST:/api/globalLanguage/delete
   */
  globalLanguageDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalLanguageDeletePostData, any>({
      path: `/api/globalLanguage/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags globalLanguage
   * @name GlobalLanguageDetailGet
   * @summary 详情
   * @request GET:/api/globalLanguage/detail
   */
  globalLanguageDetailGet = (
    query: {
      /**
       * @format int64
       * @min 1
       * @max 9223372036854776000
       */
      id: number;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalLanguageDetailGetData, any>({
      path: `/api/globalLanguage/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalLanguage
   * @name GlobalLanguagePagePost
   * @summary 分页查询
   * @request POST:/api/globalLanguage/page
   */
  globalLanguagePagePost = (
    data: PageGlobalLanguageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalLanguagePagePostData, any>({
      path: `/api/globalLanguage/page`,
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
   * @summary 获取选择器列表（字符串 ID）
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
  /**
   * No description
   *
   * @tags globalLanguage
   * @name GlobalLanguageUpdatePost
   * @summary 更新
   * @request POST:/api/globalLanguage/update
   */
  globalLanguageUpdatePost = (
    data: UpdateGlobalLanguageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalLanguageUpdatePostData, any>({
      path: `/api/globalLanguage/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const globalLanguageAddPost = (...args: Parameters<InstanceType<typeof GlobalLanguage>['globalLanguageAddPost']>) => getAPI(GlobalLanguage).globalLanguageAddPost(...args);
export const globalLanguageDeletePost = (...args: Parameters<InstanceType<typeof GlobalLanguage>['globalLanguageDeletePost']>) => getAPI(GlobalLanguage).globalLanguageDeletePost(...args);
export const globalLanguageDetailGet = (...args: Parameters<InstanceType<typeof GlobalLanguage>['globalLanguageDetailGet']>) => getAPI(GlobalLanguage).globalLanguageDetailGet(...args);
export const globalLanguagePagePost = (...args: Parameters<InstanceType<typeof GlobalLanguage>['globalLanguagePagePost']>) => getAPI(GlobalLanguage).globalLanguagePagePost(...args);
export const globalLanguageSelectPost = (...args: Parameters<InstanceType<typeof GlobalLanguage>['globalLanguageSelectPost']>) => getAPI(GlobalLanguage).globalLanguageSelectPost(...args);
export const globalLanguageSelectStringPost = (...args: Parameters<InstanceType<typeof GlobalLanguage>['globalLanguageSelectStringPost']>) => getAPI(GlobalLanguage).globalLanguageSelectStringPost(...args);
export const globalLanguageUpdatePost = (...args: Parameters<InstanceType<typeof GlobalLanguage>['globalLanguageUpdatePost']>) => getAPI(GlobalLanguage).globalLanguageUpdatePost(...args);
