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
  AddDictionaryDataRequest,
  DeleteDictionaryDataRequest,
  DictionaryDataRequest,
  GlobalDictionaryDataAddPostData,
  GlobalDictionaryDataDataListGet2Data,
  GlobalDictionaryDataDataListGetData,
  GlobalDictionaryDataDeletePostData,
  GlobalDictionaryDataDetailGetData,
  GlobalDictionaryDataListGetData,
  GlobalDictionaryDataPagePostData,
  GlobalDictionaryDataSetStatusPostData,
  GlobalDictionaryDataUpdatePostData,
  PageDictionaryDataRequest,
  UpdateDictionaryDataRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GlobalDictionaryData<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags globalDictionaryData
   * @name GlobalDictionaryDataAddPost
   * @summary 增加字典值
   * @request POST:/api/globalDictionaryData/add
   */
  globalDictionaryDataAddPost = (
    data: AddDictionaryDataRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalDictionaryDataAddPostData, any>({
      path: `/api/globalDictionaryData/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags globalDictionaryData
   * @name GlobalDictionaryDataDataListGet
   * @summary 根据查询条件获取字典值集合
   * @request GET:/api/globalDictionaryData/dataList
   */
  globalDictionaryDataDataListGet = (
    query: {
      /** 编码 */
      code: string;
      isEnabled?: boolean;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalDictionaryDataDataListGetData, any>({
      path: `/api/globalDictionaryData/dataList`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalDictionaryData
   * @name GlobalDictionaryDataDataListGet2
   * @summary 根据字典类型编码获取字典值集合
   * @request GET:/api/globalDictionaryData/dataList/{code}
   * @originalName globalDictionaryDataDataListGet
   * @duplicate
   */
  globalDictionaryDataDataListGet2 = (
    code: string,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalDictionaryDataDataListGet2Data, any>({
      path: `/api/globalDictionaryData/dataList/${code}`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalDictionaryData
   * @name GlobalDictionaryDataDeletePost
   * @summary 删除字典值
   * @request POST:/api/globalDictionaryData/delete
   */
  globalDictionaryDataDeletePost = (
    data: DeleteDictionaryDataRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalDictionaryDataDeletePostData, any>({
      path: `/api/globalDictionaryData/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags globalDictionaryData
   * @name GlobalDictionaryDataDetailGet
   * @summary 获取字典值详情
   * @request GET:/api/globalDictionaryData/detail
   */
  globalDictionaryDataDetailGet = (
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
    this.http.request<GlobalDictionaryDataDetailGetData, any>({
      path: `/api/globalDictionaryData/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalDictionaryData
   * @name GlobalDictionaryDataListGet
   * @summary 获取字典值列表
   * @request GET:/api/globalDictionaryData/list
   */
  globalDictionaryDataListGet = (
    query: {
      /**
       * 字典类型Id
       * @format int64
       * @min 1
       * @max 9223372036854776000
       */
      dictionaryTypeId: number;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalDictionaryDataListGetData, any>({
      path: `/api/globalDictionaryData/list`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalDictionaryData
   * @name GlobalDictionaryDataPagePost
   * @summary 获取字典值分页列表
   * @request POST:/api/globalDictionaryData/page
   */
  globalDictionaryDataPagePost = (
    data: PageDictionaryDataRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalDictionaryDataPagePostData, any>({
      path: `/api/globalDictionaryData/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalDictionaryData
   * @name GlobalDictionaryDataSetStatusPost
   * @summary 修改字典值状态
   * @request POST:/api/globalDictionaryData/setStatus
   */
  globalDictionaryDataSetStatusPost = (
    data: DictionaryDataRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalDictionaryDataSetStatusPostData, any>({
      path: `/api/globalDictionaryData/setStatus`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags globalDictionaryData
   * @name GlobalDictionaryDataUpdatePost
   * @summary 更新字典值
   * @request POST:/api/globalDictionaryData/update
   */
  globalDictionaryDataUpdatePost = (
    data: UpdateDictionaryDataRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalDictionaryDataUpdatePostData, any>({
      path: `/api/globalDictionaryData/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const globalDictionaryDataAddPost = (...args: Parameters<InstanceType<typeof GlobalDictionaryData>['globalDictionaryDataAddPost']>) => getAPI(GlobalDictionaryData).globalDictionaryDataAddPost(...args);
export const globalDictionaryDataDataListGet = (...args: Parameters<InstanceType<typeof GlobalDictionaryData>['globalDictionaryDataDataListGet']>) => getAPI(GlobalDictionaryData).globalDictionaryDataDataListGet(...args);
export const globalDictionaryDataDataListGet2 = (...args: Parameters<InstanceType<typeof GlobalDictionaryData>['globalDictionaryDataDataListGet2']>) => getAPI(GlobalDictionaryData).globalDictionaryDataDataListGet2(...args);
export const globalDictionaryDataDeletePost = (...args: Parameters<InstanceType<typeof GlobalDictionaryData>['globalDictionaryDataDeletePost']>) => getAPI(GlobalDictionaryData).globalDictionaryDataDeletePost(...args);
export const globalDictionaryDataDetailGet = (...args: Parameters<InstanceType<typeof GlobalDictionaryData>['globalDictionaryDataDetailGet']>) => getAPI(GlobalDictionaryData).globalDictionaryDataDetailGet(...args);
export const globalDictionaryDataListGet = (...args: Parameters<InstanceType<typeof GlobalDictionaryData>['globalDictionaryDataListGet']>) => getAPI(GlobalDictionaryData).globalDictionaryDataListGet(...args);
export const globalDictionaryDataPagePost = (...args: Parameters<InstanceType<typeof GlobalDictionaryData>['globalDictionaryDataPagePost']>) => getAPI(GlobalDictionaryData).globalDictionaryDataPagePost(...args);
export const globalDictionaryDataSetStatusPost = (...args: Parameters<InstanceType<typeof GlobalDictionaryData>['globalDictionaryDataSetStatusPost']>) => getAPI(GlobalDictionaryData).globalDictionaryDataSetStatusPost(...args);
export const globalDictionaryDataUpdatePost = (...args: Parameters<InstanceType<typeof GlobalDictionaryData>['globalDictionaryDataUpdatePost']>) => getAPI(GlobalDictionaryData).globalDictionaryDataUpdatePost(...args);
