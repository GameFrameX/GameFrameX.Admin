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
  GameAreaProvisionCreatePostData,
  GameAreaProvisionCreateRequest,
  GameAreaProvisionExecutePostData,
  GameAreaProvisionPreviewGetData,
  GameAreaProvisionStatusGetData,
  GameAreaProvisionTemplatesGetData,
} from "./types";
import {
  BaseIdRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameAreaProvision<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameAreaProvision
   * @name GameAreaProvisionCreatePost
   * @summary 以新区服编号为幂等键创建禁用行；同编号但参数不同的请求拒绝覆盖。
   * @request POST:/api/gameAreaProvision/create
   */
  gameAreaProvisionCreatePost = (
    data: GameAreaProvisionCreateRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAreaProvisionCreatePostData, any>({
      path: `/api/gameAreaProvision/create`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAreaProvision
   * @name GameAreaProvisionExecutePost
   * @summary 从尚未完成的步骤继续，失败保留已有事实，绝不提前启用。
   * @request POST:/api/gameAreaProvision/execute
   */
  gameAreaProvisionExecutePost = (
    data: BaseIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAreaProvisionExecutePostData, any>({
      path: `/api/gameAreaProvision/execute`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAreaProvision
   * @name GameAreaProvisionPreviewGet
   * @summary 预览显式克隆字段及必须填写的差异字段，不复制状态与运行时标识。
   * @request GET:/api/gameAreaProvision/preview
   */
  gameAreaProvisionPreviewGet = (
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
    this.http.request<GameAreaProvisionPreviewGetData, any>({
      path: `/api/gameAreaProvision/preview`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAreaProvision
   * @name GameAreaProvisionStatusGet
   * @summary 从区服记录和 Online 日历查询四个步骤的事实状态。
   * @request GET:/api/gameAreaProvision/status
   */
  gameAreaProvisionStatusGet = (
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
    this.http.request<GameAreaProvisionStatusGetData, any>({
      path: `/api/gameAreaProvision/status`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAreaProvision
   * @name GameAreaProvisionTemplatesGet
   * @summary 获取可选模板（包含禁用行，但不包含已软删行）。
   * @request GET:/api/gameAreaProvision/templates
   */
  gameAreaProvisionTemplatesGet = (params: RequestParams = {}) =>
    this.http.request<GameAreaProvisionTemplatesGetData, any>({
      path: `/api/gameAreaProvision/templates`,
      method: "GET",
      format: "json",
      ...params,
    });
}

export const gameAreaProvisionCreatePost = (...args: Parameters<InstanceType<typeof GameAreaProvision>['gameAreaProvisionCreatePost']>) => getAPI(GameAreaProvision).gameAreaProvisionCreatePost(...args);
export const gameAreaProvisionExecutePost = (...args: Parameters<InstanceType<typeof GameAreaProvision>['gameAreaProvisionExecutePost']>) => getAPI(GameAreaProvision).gameAreaProvisionExecutePost(...args);
export const gameAreaProvisionPreviewGet = (...args: Parameters<InstanceType<typeof GameAreaProvision>['gameAreaProvisionPreviewGet']>) => getAPI(GameAreaProvision).gameAreaProvisionPreviewGet(...args);
export const gameAreaProvisionStatusGet = (...args: Parameters<InstanceType<typeof GameAreaProvision>['gameAreaProvisionStatusGet']>) => getAPI(GameAreaProvision).gameAreaProvisionStatusGet(...args);
export const gameAreaProvisionTemplatesGet = (...args: Parameters<InstanceType<typeof GameAreaProvision>['gameAreaProvisionTemplatesGet']>) => getAPI(GameAreaProvision).gameAreaProvisionTemplatesGet(...args);
