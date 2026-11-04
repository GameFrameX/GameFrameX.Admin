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
  AddTenantGameWebShopOrderEntityRequest,
  GameWebShopOrderAddPostData,
  GameWebShopOrderDeletePostData,
  GameWebShopOrderDetailGetData,
  GameWebShopOrderPagePostData,
  GameWebShopOrderRedoPostData,
  GameWebShopOrderSelectPostData,
  GameWebShopOrderSelectStringPostData,
  GameWebShopOrderUpdatePostData,
  PageTenantGameWebShopOrderEntityRequest,
  UpdateTenantGameWebShopOrderEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
  BaseUpdateRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameWebShopOrder<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameWebShopOrder
   * @name GameWebShopOrderAddPost
   * @summary 新增
   * @request POST:/api/gameWebShopOrder/add
   */
  gameWebShopOrderAddPost = (
    data: AddTenantGameWebShopOrderEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameWebShopOrderAddPostData, any>({
      path: `/api/gameWebShopOrder/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameWebShopOrder
   * @name GameWebShopOrderDeletePost
   * @summary 删除
   * @request POST:/api/gameWebShopOrder/delete
   */
  gameWebShopOrderDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameWebShopOrderDeletePostData, any>({
      path: `/api/gameWebShopOrder/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameWebShopOrder
   * @name GameWebShopOrderDetailGet
   * @summary 详情
   * @request GET:/api/gameWebShopOrder/detail
   */
  gameWebShopOrderDetailGet = (
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
    this.http.request<GameWebShopOrderDetailGetData, any>({
      path: `/api/gameWebShopOrder/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameWebShopOrder
   * @name GameWebShopOrderPagePost
   * @summary 分页查询
   * @request POST:/api/gameWebShopOrder/page
   */
  gameWebShopOrderPagePost = (
    data: PageTenantGameWebShopOrderEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameWebShopOrderPagePostData, any>({
      path: `/api/gameWebShopOrder/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameWebShopOrder
   * @name GameWebShopOrderRedoPost
   * @summary 重新发送订单
   * @request POST:/api/gameWebShopOrder/redo
   */
  gameWebShopOrderRedoPost = (
    data: BaseUpdateRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameWebShopOrderRedoPostData, any>({
      path: `/api/gameWebShopOrder/redo`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameWebShopOrder
   * @name GameWebShopOrderSelectPost
   * @summary 获取选择器列表
   * @request POST:/api/gameWebShopOrder/select
   */
  gameWebShopOrderSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameWebShopOrderSelectPostData, any>({
      path: `/api/gameWebShopOrder/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameWebShopOrder
   * @name GameWebShopOrderSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/gameWebShopOrder/selectString
   */
  gameWebShopOrderSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameWebShopOrderSelectStringPostData, any>({
      path: `/api/gameWebShopOrder/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameWebShopOrder
   * @name GameWebShopOrderUpdatePost
   * @summary 更新
   * @request POST:/api/gameWebShopOrder/update
   */
  gameWebShopOrderUpdatePost = (
    data: UpdateTenantGameWebShopOrderEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameWebShopOrderUpdatePostData, any>({
      path: `/api/gameWebShopOrder/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameWebShopOrderAddPost = (...args: Parameters<InstanceType<typeof GameWebShopOrder>['gameWebShopOrderAddPost']>) => getAPI(GameWebShopOrder).gameWebShopOrderAddPost(...args);
export const gameWebShopOrderDeletePost = (...args: Parameters<InstanceType<typeof GameWebShopOrder>['gameWebShopOrderDeletePost']>) => getAPI(GameWebShopOrder).gameWebShopOrderDeletePost(...args);
export const gameWebShopOrderDetailGet = (...args: Parameters<InstanceType<typeof GameWebShopOrder>['gameWebShopOrderDetailGet']>) => getAPI(GameWebShopOrder).gameWebShopOrderDetailGet(...args);
export const gameWebShopOrderPagePost = (...args: Parameters<InstanceType<typeof GameWebShopOrder>['gameWebShopOrderPagePost']>) => getAPI(GameWebShopOrder).gameWebShopOrderPagePost(...args);
export const gameWebShopOrderRedoPost = (...args: Parameters<InstanceType<typeof GameWebShopOrder>['gameWebShopOrderRedoPost']>) => getAPI(GameWebShopOrder).gameWebShopOrderRedoPost(...args);
export const gameWebShopOrderSelectPost = (...args: Parameters<InstanceType<typeof GameWebShopOrder>['gameWebShopOrderSelectPost']>) => getAPI(GameWebShopOrder).gameWebShopOrderSelectPost(...args);
export const gameWebShopOrderSelectStringPost = (...args: Parameters<InstanceType<typeof GameWebShopOrder>['gameWebShopOrderSelectStringPost']>) => getAPI(GameWebShopOrder).gameWebShopOrderSelectStringPost(...args);
export const gameWebShopOrderUpdatePost = (...args: Parameters<InstanceType<typeof GameWebShopOrder>['gameWebShopOrderUpdatePost']>) => getAPI(GameWebShopOrder).gameWebShopOrderUpdatePost(...args);
