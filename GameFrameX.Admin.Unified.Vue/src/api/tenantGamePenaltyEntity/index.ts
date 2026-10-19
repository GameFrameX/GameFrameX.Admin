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
  AddTenantGamePenaltyEntityRequest,
  PageTenantGamePenaltyEntityRequest,
  RevokeTenantGamePenaltyEntityRequest,
  SyncOnlineRuntimeTenantGamePenaltyRequest,
  TenantGamePenaltyEntityAddPostData,
  TenantGamePenaltyEntityDeletePostData,
  TenantGamePenaltyEntityDetailGetData,
  TenantGamePenaltyEntityPagePostData,
  TenantGamePenaltyEntityRevokePostData,
  TenantGamePenaltyEntitySyncOnlineRuntimePostData,
  TenantGamePenaltyEntityUpdatePostData,
  UpdateTenantGamePenaltyEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGamePenaltyEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGamePenaltyEntity
   * @name TenantGamePenaltyEntityAddPost
   * @summary 新增处罚。AppId 必填且必须大于 0；审计字段服务端强制。
   * @request POST:/api/tenantGamePenaltyEntity/add
   */
  tenantGamePenaltyEntityAddPost = (
    data: AddTenantGamePenaltyEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGamePenaltyEntityAddPostData, any>({
      path: `/api/tenantGamePenaltyEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGamePenaltyEntity
   * @name TenantGamePenaltyEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGamePenaltyEntity/delete
   */
  tenantGamePenaltyEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGamePenaltyEntityDeletePostData, any>({
      path: `/api/tenantGamePenaltyEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGamePenaltyEntity
   * @name TenantGamePenaltyEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGamePenaltyEntity/detail
   */
  tenantGamePenaltyEntityDetailGet = (
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
    this.http.request<TenantGamePenaltyEntityDetailGetData, any>({
      path: `/api/tenantGamePenaltyEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGamePenaltyEntity
   * @name TenantGamePenaltyEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGamePenaltyEntity/page
   */
  tenantGamePenaltyEntityPagePost = (
    data: PageTenantGamePenaltyEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGamePenaltyEntityPagePostData, any>({
      path: `/api/tenantGamePenaltyEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGamePenaltyEntity
   * @name TenantGamePenaltyEntityRevokePost
   * @summary 撤销生效中处罚。仅 Active 可撤销；写撤销审计三元组。Enforcement 直读库无缓存，撤销后即时生效。
   * @request POST:/api/tenantGamePenaltyEntity/revoke
   */
  tenantGamePenaltyEntityRevokePost = (
    data: RevokeTenantGamePenaltyEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGamePenaltyEntityRevokePostData, any>({
      path: `/api/tenantGamePenaltyEntity/revoke`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGamePenaltyEntity
   * @name TenantGamePenaltyEntitySyncOnlineRuntimePost
   * @summary 把生效中处罚同步到 Online 运行时（手动补发联动；Add/Revoke 语义不变）。联动命令按 ActionType 服务端推导， 经 GameFrameX.Application.Game.Online.Social.OnlinePenaltyLinkCommandService 唯一入口下发——事实源先行、联动可重试（同 IdempotencyKey 重试幂等）。
   * @request POST:/api/tenantGamePenaltyEntity/syncOnlineRuntime
   */
  tenantGamePenaltyEntitySyncOnlineRuntimePost = (
    data: SyncOnlineRuntimeTenantGamePenaltyRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGamePenaltyEntitySyncOnlineRuntimePostData, any>({
      path: `/api/tenantGamePenaltyEntity/syncOnlineRuntime`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGamePenaltyEntity
   * @name TenantGamePenaltyEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGamePenaltyEntity/update
   */
  tenantGamePenaltyEntityUpdatePost = (
    data: UpdateTenantGamePenaltyEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGamePenaltyEntityUpdatePostData, any>({
      path: `/api/tenantGamePenaltyEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGamePenaltyEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGamePenaltyEntity>['tenantGamePenaltyEntityAddPost']>) => getAPI(TenantGamePenaltyEntity).tenantGamePenaltyEntityAddPost(...args);
export const tenantGamePenaltyEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGamePenaltyEntity>['tenantGamePenaltyEntityDeletePost']>) => getAPI(TenantGamePenaltyEntity).tenantGamePenaltyEntityDeletePost(...args);
export const tenantGamePenaltyEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGamePenaltyEntity>['tenantGamePenaltyEntityDetailGet']>) => getAPI(TenantGamePenaltyEntity).tenantGamePenaltyEntityDetailGet(...args);
export const tenantGamePenaltyEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGamePenaltyEntity>['tenantGamePenaltyEntityPagePost']>) => getAPI(TenantGamePenaltyEntity).tenantGamePenaltyEntityPagePost(...args);
export const tenantGamePenaltyEntityRevokePost = (...args: Parameters<InstanceType<typeof TenantGamePenaltyEntity>['tenantGamePenaltyEntityRevokePost']>) => getAPI(TenantGamePenaltyEntity).tenantGamePenaltyEntityRevokePost(...args);
export const tenantGamePenaltyEntitySyncOnlineRuntimePost = (...args: Parameters<InstanceType<typeof TenantGamePenaltyEntity>['tenantGamePenaltyEntitySyncOnlineRuntimePost']>) => getAPI(TenantGamePenaltyEntity).tenantGamePenaltyEntitySyncOnlineRuntimePost(...args);
export const tenantGamePenaltyEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGamePenaltyEntity>['tenantGamePenaltyEntityUpdatePost']>) => getAPI(TenantGamePenaltyEntity).tenantGamePenaltyEntityUpdatePost(...args);
