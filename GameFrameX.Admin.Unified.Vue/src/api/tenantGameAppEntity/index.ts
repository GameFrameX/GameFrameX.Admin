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
  AddGameAppRequest,
  PageGameAppRequest,
  TenantGameAppEntityAddPostData,
  TenantGameAppEntityDeletePostData,
  TenantGameAppEntityDetailGetData,
  TenantGameAppEntityListGetData,
  TenantGameAppEntityPagePostData,
  TenantGameAppEntityResetSecretPostData,
  TenantGameAppEntitySelectPostData,
  TenantGameAppEntitySelectStringPostData,
  TenantGameAppEntityUpdatePostData,
  UpdateGameAppRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseIdRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameAppEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameAppEntity
   * @name TenantGameAppEntityAddPost
   * @summary 重写基类 AddAsync：校验租户应用配额后创建应用。 密钥采用 hash-first：明文永不落库，仅在返回实体上一次性回传（M:GameFrameX.Application.Game.App.GameApp.TenantGameAppEntityService.CreateEntityAsync(GameFrameX.Application.Game.App.GameApp.Dto.AddGameAppRequest)）。
   * @request POST:/api/tenantGameAppEntity/add
   */
  tenantGameAppEntityAddPost = (
    data: AddGameAppRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppEntityAddPostData, any>({
      path: `/api/tenantGameAppEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppEntity
   * @name TenantGameAppEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameAppEntity/delete
   */
  tenantGameAppEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppEntityDeletePostData, any>({
      path: `/api/tenantGameAppEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppEntity
   * @name TenantGameAppEntityDetailGet
   * @summary 详情（AppSecret 脱敏）
   * @request GET:/api/tenantGameAppEntity/detail
   */
  tenantGameAppEntityDetailGet = (
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
    this.http.request<TenantGameAppEntityDetailGetData, any>({
      path: `/api/tenantGameAppEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppEntity
   * @name TenantGameAppEntityListGet
   * @summary 获取当前租户启用应用列表
   * @request GET:/api/tenantGameAppEntity/list
   */
  tenantGameAppEntityListGet = (params: RequestParams = {}) =>
    this.http.request<TenantGameAppEntityListGetData, any>({
      path: `/api/tenantGameAppEntity/list`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppEntity
   * @name TenantGameAppEntityPagePost
   * @summary 分页查询（AppSecret 脱敏）
   * @request POST:/api/tenantGameAppEntity/page
   */
  tenantGameAppEntityPagePost = (
    data: PageGameAppRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppEntityPagePostData, any>({
      path: `/api/tenantGameAppEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppEntity
   * @name TenantGameAppEntityResetSecretPost
   * @summary 重置应用密钥：生成新明文 → 写 PBKDF2-SHA256 hash + 算法标识 + 轮换时间，清空遗留明文。 明文仅在返回 Output 上一次性回传（供前端展示一次），不落库；Hash 字段不在 Output，天然不出 wire。
   * @request POST:/api/tenantGameAppEntity/resetSecret
   */
  tenantGameAppEntityResetSecretPost = (
    data: BaseIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppEntityResetSecretPostData, any>({
      path: `/api/tenantGameAppEntity/resetSecret`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppEntity
   * @name TenantGameAppEntitySelectPost
   * @summary 获取选择器列表
   * @request POST:/api/tenantGameAppEntity/select
   */
  tenantGameAppEntitySelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppEntitySelectPostData, any>({
      path: `/api/tenantGameAppEntity/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppEntity
   * @name TenantGameAppEntitySelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/tenantGameAppEntity/selectString
   */
  tenantGameAppEntitySelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppEntitySelectStringPostData, any>({
      path: `/api/tenantGameAppEntity/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppEntity
   * @name TenantGameAppEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameAppEntity/update
   */
  tenantGameAppEntityUpdatePost = (
    data: UpdateGameAppRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppEntityUpdatePostData, any>({
      path: `/api/tenantGameAppEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameAppEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameAppEntity>['tenantGameAppEntityAddPost']>) => getAPI(TenantGameAppEntity).tenantGameAppEntityAddPost(...args);
export const tenantGameAppEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameAppEntity>['tenantGameAppEntityDeletePost']>) => getAPI(TenantGameAppEntity).tenantGameAppEntityDeletePost(...args);
export const tenantGameAppEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameAppEntity>['tenantGameAppEntityDetailGet']>) => getAPI(TenantGameAppEntity).tenantGameAppEntityDetailGet(...args);
export const tenantGameAppEntityListGet = (...args: Parameters<InstanceType<typeof TenantGameAppEntity>['tenantGameAppEntityListGet']>) => getAPI(TenantGameAppEntity).tenantGameAppEntityListGet(...args);
export const tenantGameAppEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameAppEntity>['tenantGameAppEntityPagePost']>) => getAPI(TenantGameAppEntity).tenantGameAppEntityPagePost(...args);
export const tenantGameAppEntityResetSecretPost = (...args: Parameters<InstanceType<typeof TenantGameAppEntity>['tenantGameAppEntityResetSecretPost']>) => getAPI(TenantGameAppEntity).tenantGameAppEntityResetSecretPost(...args);
export const tenantGameAppEntitySelectPost = (...args: Parameters<InstanceType<typeof TenantGameAppEntity>['tenantGameAppEntitySelectPost']>) => getAPI(TenantGameAppEntity).tenantGameAppEntitySelectPost(...args);
export const tenantGameAppEntitySelectStringPost = (...args: Parameters<InstanceType<typeof TenantGameAppEntity>['tenantGameAppEntitySelectStringPost']>) => getAPI(TenantGameAppEntity).tenantGameAppEntitySelectStringPost(...args);
export const tenantGameAppEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameAppEntity>['tenantGameAppEntityUpdatePost']>) => getAPI(TenantGameAppEntity).tenantGameAppEntityUpdatePost(...args);
