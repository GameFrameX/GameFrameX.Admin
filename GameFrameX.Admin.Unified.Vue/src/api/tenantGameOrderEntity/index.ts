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
  PageTenantGameOrderEntityRequest,
  TenantGameOrderEntityAddPostData,
  TenantGameOrderEntityDeletePostData,
  TenantGameOrderEntityDetailGetData,
  TenantGameOrderEntityPagePostData,
  TenantGameOrderEntitySendPostData,
  TenantGameOrderEntitySummaryPostData,
  TenantGameOrderEntityUpdatePostData,
  UpdateTenantGameOrderEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameOrderEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameOrderEntity
   * @name TenantGameOrderEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameOrderEntity/add
   */
  tenantGameOrderEntityAddPost = (
    data: AddTenantGameOrderEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameOrderEntityAddPostData, any>({
      path: `/api/tenantGameOrderEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameOrderEntity
   * @name TenantGameOrderEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameOrderEntity/delete
   */
  tenantGameOrderEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameOrderEntityDeletePostData, any>({
      path: `/api/tenantGameOrderEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameOrderEntity
   * @name TenantGameOrderEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameOrderEntity/detail
   */
  tenantGameOrderEntityDetailGet = (
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
    this.http.request<TenantGameOrderEntityDetailGetData, any>({
      path: `/api/tenantGameOrderEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameOrderEntity
   * @name TenantGameOrderEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameOrderEntity/page
   */
  tenantGameOrderEntityPagePost = (
    data: PageTenantGameOrderEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameOrderEntityPagePostData, any>({
      path: `/api/tenantGameOrderEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameOrderEntity
   * @name TenantGameOrderEntitySendPost
   * @summary 游戏支付订单人工补发（高风险）。走 M:GameFrameX.Application.Game.Order.GameOrder.TenantGameOrderShipService.CompensateAsync(System.Int64,System.String,System.Int64) 审计路径： 必须带操作人（当前登录 Admin），独立幂等键 + OperatorId 落账，不改动订单状态、不重复累加角色累充。
   * @request POST:/api/tenantGameOrderEntity/send
   */
  tenantGameOrderEntitySendPost = (
    data: AddTenantGameOrderEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameOrderEntitySendPostData, any>({
      path: `/api/tenantGameOrderEntity/send`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameOrderEntity
   * @name TenantGameOrderEntitySummaryPost
   * @summary 订单统计汇总
   * @request POST:/api/tenantGameOrderEntity/summary
   */
  tenantGameOrderEntitySummaryPost = (
    data: PageTenantGameOrderEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameOrderEntitySummaryPostData, any>({
      path: `/api/tenantGameOrderEntity/summary`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameOrderEntity
   * @name TenantGameOrderEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameOrderEntity/update
   */
  tenantGameOrderEntityUpdatePost = (
    data: UpdateTenantGameOrderEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameOrderEntityUpdatePostData, any>({
      path: `/api/tenantGameOrderEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameOrderEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameOrderEntity>['tenantGameOrderEntityAddPost']>) => getAPI(TenantGameOrderEntity).tenantGameOrderEntityAddPost(...args);
export const tenantGameOrderEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameOrderEntity>['tenantGameOrderEntityDeletePost']>) => getAPI(TenantGameOrderEntity).tenantGameOrderEntityDeletePost(...args);
export const tenantGameOrderEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameOrderEntity>['tenantGameOrderEntityDetailGet']>) => getAPI(TenantGameOrderEntity).tenantGameOrderEntityDetailGet(...args);
export const tenantGameOrderEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameOrderEntity>['tenantGameOrderEntityPagePost']>) => getAPI(TenantGameOrderEntity).tenantGameOrderEntityPagePost(...args);
export const tenantGameOrderEntitySendPost = (...args: Parameters<InstanceType<typeof TenantGameOrderEntity>['tenantGameOrderEntitySendPost']>) => getAPI(TenantGameOrderEntity).tenantGameOrderEntitySendPost(...args);
export const tenantGameOrderEntitySummaryPost = (...args: Parameters<InstanceType<typeof TenantGameOrderEntity>['tenantGameOrderEntitySummaryPost']>) => getAPI(TenantGameOrderEntity).tenantGameOrderEntitySummaryPost(...args);
export const tenantGameOrderEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameOrderEntity>['tenantGameOrderEntityUpdatePost']>) => getAPI(TenantGameOrderEntity).tenantGameOrderEntityUpdatePost(...args);
