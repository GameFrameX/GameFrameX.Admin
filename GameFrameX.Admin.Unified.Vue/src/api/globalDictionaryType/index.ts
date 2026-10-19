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
  AddDictionaryTypeRequest,
  DeleteDictionaryTypeRequest,
  DictionaryTypeRequest,
  GlobalDictionaryTypeAddPostData,
  GlobalDictionaryTypeAllDictionaryListGetData,
  GlobalDictionaryTypeDataListGetData,
  GlobalDictionaryTypeDeletePostData,
  GlobalDictionaryTypeDetailGetData,
  GlobalDictionaryTypeListGetData,
  GlobalDictionaryTypePagePostData,
  GlobalDictionaryTypeSetStatusPostData,
  GlobalDictionaryTypeUpdatePostData,
  PageDictionaryTypeRequest,
  UpdateDictionaryTypeRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GlobalDictionaryType<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags globalDictionaryType
   * @name GlobalDictionaryTypeAddPost
   * @summary 添加字典类型
   * @request POST:/api/globalDictionaryType/add
   */
  globalDictionaryTypeAddPost = (
    data: AddDictionaryTypeRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalDictionaryTypeAddPostData, any>({
      path: `/api/globalDictionaryType/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags globalDictionaryType
   * @name GlobalDictionaryTypeAllDictionaryListGet
   * @summary 获取所有字典集合
   * @request GET:/api/globalDictionaryType/allDictionaryList
   */
  globalDictionaryTypeAllDictionaryListGet = (params: RequestParams = {}) =>
    this.http.request<GlobalDictionaryTypeAllDictionaryListGetData, any>({
      path: `/api/globalDictionaryType/allDictionaryList`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalDictionaryType
   * @name GlobalDictionaryTypeDataListGet
   * @summary 获取字典类型-值列表
   * @request GET:/api/globalDictionaryType/dataList
   */
  globalDictionaryTypeDataListGet = (
    query: {
      /** 编码 */
      code: string;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalDictionaryTypeDataListGetData, any>({
      path: `/api/globalDictionaryType/dataList`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalDictionaryType
   * @name GlobalDictionaryTypeDeletePost
   * @summary 删除字典类型
   * @request POST:/api/globalDictionaryType/delete
   */
  globalDictionaryTypeDeletePost = (
    data: DeleteDictionaryTypeRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalDictionaryTypeDeletePostData, any>({
      path: `/api/globalDictionaryType/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags globalDictionaryType
   * @name GlobalDictionaryTypeDetailGet
   * @summary 获取字典类型详情
   * @request GET:/api/globalDictionaryType/detail
   */
  globalDictionaryTypeDetailGet = (
    query: {
      /** 是否启用 */
      isEnabled?: boolean;
      /**
       * @format int64
       * @min 1
       * @max 9223372036854776000
       */
      id: number;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalDictionaryTypeDetailGetData, any>({
      path: `/api/globalDictionaryType/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalDictionaryType
   * @name GlobalDictionaryTypeListGet
   * @summary 获取字典类型列表
   * @request GET:/api/globalDictionaryType/list
   */
  globalDictionaryTypeListGet = (params: RequestParams = {}) =>
    this.http.request<GlobalDictionaryTypeListGetData, any>({
      path: `/api/globalDictionaryType/list`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalDictionaryType
   * @name GlobalDictionaryTypePagePost
   * @summary 获取字典类型分页列表
   * @request POST:/api/globalDictionaryType/page
   */
  globalDictionaryTypePagePost = (
    data: PageDictionaryTypeRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalDictionaryTypePagePostData, any>({
      path: `/api/globalDictionaryType/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalDictionaryType
   * @name GlobalDictionaryTypeSetStatusPost
   * @summary 修改字典类型状态
   * @request POST:/api/globalDictionaryType/setStatus
   */
  globalDictionaryTypeSetStatusPost = (
    data: DictionaryTypeRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalDictionaryTypeSetStatusPostData, any>({
      path: `/api/globalDictionaryType/setStatus`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags globalDictionaryType
   * @name GlobalDictionaryTypeUpdatePost
   * @summary 更新字典类型
   * @request POST:/api/globalDictionaryType/update
   */
  globalDictionaryTypeUpdatePost = (
    data: UpdateDictionaryTypeRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalDictionaryTypeUpdatePostData, any>({
      path: `/api/globalDictionaryType/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const globalDictionaryTypeAddPost = (...args: Parameters<InstanceType<typeof GlobalDictionaryType>['globalDictionaryTypeAddPost']>) => getAPI(GlobalDictionaryType).globalDictionaryTypeAddPost(...args);
export const globalDictionaryTypeAllDictionaryListGet = (...args: Parameters<InstanceType<typeof GlobalDictionaryType>['globalDictionaryTypeAllDictionaryListGet']>) => getAPI(GlobalDictionaryType).globalDictionaryTypeAllDictionaryListGet(...args);
export const globalDictionaryTypeDataListGet = (...args: Parameters<InstanceType<typeof GlobalDictionaryType>['globalDictionaryTypeDataListGet']>) => getAPI(GlobalDictionaryType).globalDictionaryTypeDataListGet(...args);
export const globalDictionaryTypeDeletePost = (...args: Parameters<InstanceType<typeof GlobalDictionaryType>['globalDictionaryTypeDeletePost']>) => getAPI(GlobalDictionaryType).globalDictionaryTypeDeletePost(...args);
export const globalDictionaryTypeDetailGet = (...args: Parameters<InstanceType<typeof GlobalDictionaryType>['globalDictionaryTypeDetailGet']>) => getAPI(GlobalDictionaryType).globalDictionaryTypeDetailGet(...args);
export const globalDictionaryTypeListGet = (...args: Parameters<InstanceType<typeof GlobalDictionaryType>['globalDictionaryTypeListGet']>) => getAPI(GlobalDictionaryType).globalDictionaryTypeListGet(...args);
export const globalDictionaryTypePagePost = (...args: Parameters<InstanceType<typeof GlobalDictionaryType>['globalDictionaryTypePagePost']>) => getAPI(GlobalDictionaryType).globalDictionaryTypePagePost(...args);
export const globalDictionaryTypeSetStatusPost = (...args: Parameters<InstanceType<typeof GlobalDictionaryType>['globalDictionaryTypeSetStatusPost']>) => getAPI(GlobalDictionaryType).globalDictionaryTypeSetStatusPost(...args);
export const globalDictionaryTypeUpdatePost = (...args: Parameters<InstanceType<typeof GlobalDictionaryType>['globalDictionaryTypeUpdatePost']>) => getAPI(GlobalDictionaryType).globalDictionaryTypeUpdatePost(...args);
