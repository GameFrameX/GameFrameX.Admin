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
  AddMenuRequest,
  DeleteMenuRequest,
  GlobalMenuAddPostData,
  GlobalMenuDeletePostData,
  GlobalMenuEndpointPermissionMapAsyncGetData,
  GlobalMenuListGetData,
  GlobalMenuLoginMenuTreeGetData,
  GlobalMenuOwnButtonPermListGetData,
  GlobalMenuOwnMenuIdListGetData,
  GlobalMenuUpdatePostData,
  MenuTypeEnum,
  UpdateMenuRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GlobalMenu<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags globalMenu
   * @name GlobalMenuAddPost
   * @summary 增加菜单
   * @request POST:/api/globalMenu/add
   */
  globalMenuAddPost = (data: AddMenuRequest, params: RequestParams = {}) =>
    this.http.request<GlobalMenuAddPostData, any>({
      path: `/api/globalMenu/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags globalMenu
   * @name GlobalMenuDeletePost
   * @summary 删除菜单
   * @request POST:/api/globalMenu/delete
   */
  globalMenuDeletePost = (
    data: DeleteMenuRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalMenuDeletePostData, any>({
      path: `/api/globalMenu/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags globalMenu
   * @name GlobalMenuEndpointPermissionMapAsyncGet
   * @summary 获取端点权限目录（U6/C42）：(HttpMethod, RoutePath) → GameFrameX.Core.Authorization.EndpointPermissionEntry。 由 `ApiEndpointCatalogSync` 启动时构建并预热；此处请求期按需返回缓存，缓存缺失则即时重建。
   * @request GET:/api/globalMenu/endpointPermissionMapAsync
   */
  globalMenuEndpointPermissionMapAsyncGet = (params: RequestParams = {}) =>
    this.http.request<GlobalMenuEndpointPermissionMapAsyncGetData, any>({
      path: `/api/globalMenu/endpointPermissionMapAsync`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalMenu
   * @name GlobalMenuListGet
   * @summary 获取菜单列表
   * @request GET:/api/globalMenu/list
   */
  globalMenuListGet = (
    query?: {
      /** 标题 */
      title?: string;
      /** 菜单类型（1目录 2菜单 3按钮） */
      type?: MenuTypeEnum;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalMenuListGetData, any>({
      path: `/api/globalMenu/list`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalMenu
   * @name GlobalMenuLoginMenuTreeGet
   * @summary 获取登录菜单树
   * @request GET:/api/globalMenu/loginMenuTree
   */
  globalMenuLoginMenuTreeGet = (params: RequestParams = {}) =>
    this.http.request<GlobalMenuLoginMenuTreeGetData, any>({
      path: `/api/globalMenu/loginMenuTree`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalMenu
   * @name GlobalMenuOwnButtonPermListGet
   * @summary 获取用户拥有按钮权限集合（缓存）
   * @request GET:/api/globalMenu/ownButtonPermList
   */
  globalMenuOwnButtonPermListGet = (params: RequestParams = {}) =>
    this.http.request<GlobalMenuOwnButtonPermListGetData, any>({
      path: `/api/globalMenu/ownButtonPermList`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalMenu
   * @name GlobalMenuOwnMenuIdListGet
   * @summary 获取当前用户已授权的菜单Id集合（缓存，含目录/菜单/按钮，来自 TenantRoleMenuEntity）。 供基于稳定身份（Key）的鉴权使用：M:GameFrameX.Core.Const.MenuSeed.StableSeedId.Menu(System.String) 由 Key 派生的 Id 与本集合比对。
   * @request GET:/api/globalMenu/ownMenuIdList
   */
  globalMenuOwnMenuIdListGet = (params: RequestParams = {}) =>
    this.http.request<GlobalMenuOwnMenuIdListGetData, any>({
      path: `/api/globalMenu/ownMenuIdList`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalMenu
   * @name GlobalMenuUpdatePost
   * @summary 更新菜单
   * @request POST:/api/globalMenu/update
   */
  globalMenuUpdatePost = (
    data: UpdateMenuRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalMenuUpdatePostData, any>({
      path: `/api/globalMenu/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const globalMenuAddPost = (...args: Parameters<InstanceType<typeof GlobalMenu>['globalMenuAddPost']>) => getAPI(GlobalMenu).globalMenuAddPost(...args);
export const globalMenuDeletePost = (...args: Parameters<InstanceType<typeof GlobalMenu>['globalMenuDeletePost']>) => getAPI(GlobalMenu).globalMenuDeletePost(...args);
export const globalMenuEndpointPermissionMapAsyncGet = (...args: Parameters<InstanceType<typeof GlobalMenu>['globalMenuEndpointPermissionMapAsyncGet']>) => getAPI(GlobalMenu).globalMenuEndpointPermissionMapAsyncGet(...args);
export const globalMenuListGet = (...args: Parameters<InstanceType<typeof GlobalMenu>['globalMenuListGet']>) => getAPI(GlobalMenu).globalMenuListGet(...args);
export const globalMenuLoginMenuTreeGet = (...args: Parameters<InstanceType<typeof GlobalMenu>['globalMenuLoginMenuTreeGet']>) => getAPI(GlobalMenu).globalMenuLoginMenuTreeGet(...args);
export const globalMenuOwnButtonPermListGet = (...args: Parameters<InstanceType<typeof GlobalMenu>['globalMenuOwnButtonPermListGet']>) => getAPI(GlobalMenu).globalMenuOwnButtonPermListGet(...args);
export const globalMenuOwnMenuIdListGet = (...args: Parameters<InstanceType<typeof GlobalMenu>['globalMenuOwnMenuIdListGet']>) => getAPI(GlobalMenu).globalMenuOwnMenuIdListGet(...args);
export const globalMenuUpdatePost = (...args: Parameters<InstanceType<typeof GlobalMenu>['globalMenuUpdatePost']>) => getAPI(GlobalMenu).globalMenuUpdatePost(...args);
