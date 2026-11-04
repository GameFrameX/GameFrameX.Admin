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
  AddTenantGameRedeemCodeUseRecordEntityRequest,
  GameRedeemCodeUseRecordAddPostData,
  GameRedeemCodeUseRecordDeletePostData,
  GameRedeemCodeUseRecordDetailGetData,
  GameRedeemCodeUseRecordPagePostData,
  GameRedeemCodeUseRecordTenantGameAreaEntityAreaIdDropdownGetData,
  GameRedeemCodeUseRecordUpdatePostData,
  PageTenantGameRedeemCodeUseRecordEntityRequest,
  UpdateTenantGameRedeemCodeUseRecordEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameRedeemCodeUseRecord<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameRedeemCodeUseRecord
   * @name GameRedeemCodeUseRecordAddPost
   * @summary 新增
   * @request POST:/api/gameRedeemCodeUseRecord/add
   */
  gameRedeemCodeUseRecordAddPost = (
    data: AddTenantGameRedeemCodeUseRecordEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRedeemCodeUseRecordAddPostData, any>({
      path: `/api/gameRedeemCodeUseRecord/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRedeemCodeUseRecord
   * @name GameRedeemCodeUseRecordDeletePost
   * @summary 删除
   * @request POST:/api/gameRedeemCodeUseRecord/delete
   */
  gameRedeemCodeUseRecordDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRedeemCodeUseRecordDeletePostData, any>({
      path: `/api/gameRedeemCodeUseRecord/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRedeemCodeUseRecord
   * @name GameRedeemCodeUseRecordDetailGet
   * @summary 详情
   * @request GET:/api/gameRedeemCodeUseRecord/detail
   */
  gameRedeemCodeUseRecordDetailGet = (
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
    this.http.request<GameRedeemCodeUseRecordDetailGetData, any>({
      path: `/api/gameRedeemCodeUseRecord/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRedeemCodeUseRecord
   * @name GameRedeemCodeUseRecordPagePost
   * @summary 分页查询
   * @request POST:/api/gameRedeemCodeUseRecord/page
   */
  gameRedeemCodeUseRecordPagePost = (
    data: PageTenantGameRedeemCodeUseRecordEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRedeemCodeUseRecordPagePostData, any>({
      path: `/api/gameRedeemCodeUseRecord/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRedeemCodeUseRecord
   * @name GameRedeemCodeUseRecordTenantGameAreaEntityAreaIdDropdownGet
   * @summary 获取区服下拉列表（AreaId 外键关联）
   * @request GET:/api/gameRedeemCodeUseRecord/tenantGameAreaEntityAreaIdDropdown
   */
  gameRedeemCodeUseRecordTenantGameAreaEntityAreaIdDropdownGet = (
    params: RequestParams = {},
  ) =>
    this.http.request<
      GameRedeemCodeUseRecordTenantGameAreaEntityAreaIdDropdownGetData,
      any
    >({
      path: `/api/gameRedeemCodeUseRecord/tenantGameAreaEntityAreaIdDropdown`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRedeemCodeUseRecord
   * @name GameRedeemCodeUseRecordUpdatePost
   * @summary 更新
   * @request POST:/api/gameRedeemCodeUseRecord/update
   */
  gameRedeemCodeUseRecordUpdatePost = (
    data: UpdateTenantGameRedeemCodeUseRecordEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRedeemCodeUseRecordUpdatePostData, any>({
      path: `/api/gameRedeemCodeUseRecord/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameRedeemCodeUseRecordAddPost = (...args: Parameters<InstanceType<typeof GameRedeemCodeUseRecord>['gameRedeemCodeUseRecordAddPost']>) => getAPI(GameRedeemCodeUseRecord).gameRedeemCodeUseRecordAddPost(...args);
export const gameRedeemCodeUseRecordDeletePost = (...args: Parameters<InstanceType<typeof GameRedeemCodeUseRecord>['gameRedeemCodeUseRecordDeletePost']>) => getAPI(GameRedeemCodeUseRecord).gameRedeemCodeUseRecordDeletePost(...args);
export const gameRedeemCodeUseRecordDetailGet = (...args: Parameters<InstanceType<typeof GameRedeemCodeUseRecord>['gameRedeemCodeUseRecordDetailGet']>) => getAPI(GameRedeemCodeUseRecord).gameRedeemCodeUseRecordDetailGet(...args);
export const gameRedeemCodeUseRecordPagePost = (...args: Parameters<InstanceType<typeof GameRedeemCodeUseRecord>['gameRedeemCodeUseRecordPagePost']>) => getAPI(GameRedeemCodeUseRecord).gameRedeemCodeUseRecordPagePost(...args);
export const gameRedeemCodeUseRecordTenantGameAreaEntityAreaIdDropdownGet = (...args: Parameters<InstanceType<typeof GameRedeemCodeUseRecord>['gameRedeemCodeUseRecordTenantGameAreaEntityAreaIdDropdownGet']>) => getAPI(GameRedeemCodeUseRecord).gameRedeemCodeUseRecordTenantGameAreaEntityAreaIdDropdownGet(...args);
export const gameRedeemCodeUseRecordUpdatePost = (...args: Parameters<InstanceType<typeof GameRedeemCodeUseRecord>['gameRedeemCodeUseRecordUpdatePost']>) => getAPI(GameRedeemCodeUseRecord).gameRedeemCodeUseRecordUpdatePost(...args);
