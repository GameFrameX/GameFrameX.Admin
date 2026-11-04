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
  AddTenantGameReportEntityRequest,
  GameReportAddPostData,
  GameReportDeletePostData,
  GameReportDetailGetData,
  GameReportLinkPenaltyPostData,
  GameReportPagePostData,
  GameReportUpdatePostData,
  LinkPenaltyTenantGameReportEntityRequest,
  PageTenantGameReportEntityRequest,
  UpdateTenantGameReportEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameReport<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameReport
   * @name GameReportAddPost
   * @summary 录入举报：Status/HandlerId 服务端强制。
   * @request POST:/api/gameReport/add
   */
  gameReportAddPost = (
    data: AddTenantGameReportEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameReportAddPostData, any>({
      path: `/api/gameReport/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameReport
   * @name GameReportDeletePost
   * @summary 删除
   * @request POST:/api/gameReport/delete
   */
  gameReportDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameReportDeletePostData, any>({
      path: `/api/gameReport/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameReport
   * @name GameReportDetailGet
   * @summary 详情
   * @request GET:/api/gameReport/detail
   */
  gameReportDetailGet = (
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
    this.http.request<GameReportDetailGetData, any>({
      path: `/api/gameReport/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameReport
   * @name GameReportLinkPenaltyPost
   * @summary 关联处罚：仅 Confirmed 举报可关联；处罚必须存在；写 ResultPenaltyId + HandlerId，不自动创建处罚。
   * @request POST:/api/gameReport/linkPenalty
   */
  gameReportLinkPenaltyPost = (
    data: LinkPenaltyTenantGameReportEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameReportLinkPenaltyPostData, any>({
      path: `/api/gameReport/linkPenalty`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameReport
   * @name GameReportPagePost
   * @summary 分页查询
   * @request POST:/api/gameReport/page
   */
  gameReportPagePost = (
    data: PageTenantGameReportEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameReportPagePostData, any>({
      path: `/api/gameReport/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameReport
   * @name GameReportUpdatePost
   * @summary 更新
   * @request POST:/api/gameReport/update
   */
  gameReportUpdatePost = (
    data: UpdateTenantGameReportEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameReportUpdatePostData, any>({
      path: `/api/gameReport/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameReportAddPost = (...args: Parameters<InstanceType<typeof GameReport>['gameReportAddPost']>) => getAPI(GameReport).gameReportAddPost(...args);
export const gameReportDeletePost = (...args: Parameters<InstanceType<typeof GameReport>['gameReportDeletePost']>) => getAPI(GameReport).gameReportDeletePost(...args);
export const gameReportDetailGet = (...args: Parameters<InstanceType<typeof GameReport>['gameReportDetailGet']>) => getAPI(GameReport).gameReportDetailGet(...args);
export const gameReportLinkPenaltyPost = (...args: Parameters<InstanceType<typeof GameReport>['gameReportLinkPenaltyPost']>) => getAPI(GameReport).gameReportLinkPenaltyPost(...args);
export const gameReportPagePost = (...args: Parameters<InstanceType<typeof GameReport>['gameReportPagePost']>) => getAPI(GameReport).gameReportPagePost(...args);
export const gameReportUpdatePost = (...args: Parameters<InstanceType<typeof GameReport>['gameReportUpdatePost']>) => getAPI(GameReport).gameReportUpdatePost(...args);
