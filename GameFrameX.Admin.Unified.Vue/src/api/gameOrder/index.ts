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
  AddTenantGameOrderEntityRequest,
  GameOrderAddPostData,
  GameOrderDeletePostData,
  GameOrderDetailGetData,
  GameOrderPagePostData,
  GameOrderSendPostData,
  GameOrderSummaryPostData,
  GameOrderUpdatePostData,
  PageTenantGameOrderEntityRequest,
  UpdateTenantGameOrderEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameOrder<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameOrder
   * @name GameOrderAddPost
   * @summary 新增
   * @request POST:/api/gameOrder/add
   */
  gameOrderAddPost = (
    data: AddTenantGameOrderEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameOrderAddPostData, any>({
      path: `/api/gameOrder/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameOrder
   * @name GameOrderDeletePost
   * @summary 删除
   * @request POST:/api/gameOrder/delete
   */
  gameOrderDeletePost = (data: BaseDeleteRequest, params: RequestParams = {}) =>
    this.http.request<GameOrderDeletePostData, any>({
      path: `/api/gameOrder/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameOrder
   * @name GameOrderDetailGet
   * @summary 详情
   * @request GET:/api/gameOrder/detail
   */
  gameOrderDetailGet = (
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
    this.http.request<GameOrderDetailGetData, any>({
      path: `/api/gameOrder/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameOrder
   * @name GameOrderPagePost
   * @summary 分页查询
   * @request POST:/api/gameOrder/page
   */
  gameOrderPagePost = (
    data: PageTenantGameOrderEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameOrderPagePostData, any>({
      path: `/api/gameOrder/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameOrder
   * @name GameOrderSendPost
   * @summary 游戏支付订单人工补发（高风险）。走 M:GameFrameX.Application.CommerceCenter.GameOrder.GameOrderShipService.CompensateAsync(System.Int64,System.String,System.Int64) 审计路径： 必须带操作人（当前登录 Admin），独立幂等键 + OperatorId 落账，不改动订单状态、不重复累加角色累充。
   * @request POST:/api/gameOrder/send
   */
  gameOrderSendPost = (
    data: AddTenantGameOrderEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameOrderSendPostData, any>({
      path: `/api/gameOrder/send`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameOrder
   * @name GameOrderSummaryPost
   * @summary 订单统计汇总
   * @request POST:/api/gameOrder/summary
   */
  gameOrderSummaryPost = (
    data: PageTenantGameOrderEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameOrderSummaryPostData, any>({
      path: `/api/gameOrder/summary`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameOrder
   * @name GameOrderUpdatePost
   * @summary 更新
   * @request POST:/api/gameOrder/update
   */
  gameOrderUpdatePost = (
    data: UpdateTenantGameOrderEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameOrderUpdatePostData, any>({
      path: `/api/gameOrder/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameOrderAddPost = (...args: Parameters<InstanceType<typeof GameOrder>['gameOrderAddPost']>) => getAPI(GameOrder).gameOrderAddPost(...args);
export const gameOrderDeletePost = (...args: Parameters<InstanceType<typeof GameOrder>['gameOrderDeletePost']>) => getAPI(GameOrder).gameOrderDeletePost(...args);
export const gameOrderDetailGet = (...args: Parameters<InstanceType<typeof GameOrder>['gameOrderDetailGet']>) => getAPI(GameOrder).gameOrderDetailGet(...args);
export const gameOrderPagePost = (...args: Parameters<InstanceType<typeof GameOrder>['gameOrderPagePost']>) => getAPI(GameOrder).gameOrderPagePost(...args);
export const gameOrderSendPost = (...args: Parameters<InstanceType<typeof GameOrder>['gameOrderSendPost']>) => getAPI(GameOrder).gameOrderSendPost(...args);
export const gameOrderSummaryPost = (...args: Parameters<InstanceType<typeof GameOrder>['gameOrderSummaryPost']>) => getAPI(GameOrder).gameOrderSummaryPost(...args);
export const gameOrderUpdatePost = (...args: Parameters<InstanceType<typeof GameOrder>['gameOrderUpdatePost']>) => getAPI(GameOrder).gameOrderUpdatePost(...args);
