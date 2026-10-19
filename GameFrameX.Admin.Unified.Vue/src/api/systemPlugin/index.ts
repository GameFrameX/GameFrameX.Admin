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
  AddPluginRequest,
  CompileAssemblyRequest,
  DeletePluginRequest,
  PagePluginRequest,
  SystemPluginAddPostData,
  SystemPluginCompileAssemblyPostData,
  SystemPluginDeletePostData,
  SystemPluginPagePostData,
  SystemPluginRemoveAssemblyPostData,
  SystemPluginUpdatePostData,
  UpdatePluginRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class SystemPlugin<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags systemPlugin
   * @name SystemPluginAddPost
   * @summary 增加动态插件
   * @request POST:/api/systemPlugin/add
   */
  systemPluginAddPost = (data: AddPluginRequest, params: RequestParams = {}) =>
    this.http.request<SystemPluginAddPostData, any>({
      path: `/api/systemPlugin/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemPlugin
   * @name SystemPluginCompileAssemblyPost
   * @summary 添加动态程序集/接口
   * @request POST:/api/systemPlugin/compileAssembly
   */
  systemPluginCompileAssemblyPost = (
    data: CompileAssemblyRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemPluginCompileAssemblyPostData, any>({
      path: `/api/systemPlugin/compileAssembly`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemPlugin
   * @name SystemPluginDeletePost
   * @summary 删除动态插件
   * @request POST:/api/systemPlugin/delete
   */
  systemPluginDeletePost = (
    data: DeletePluginRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemPluginDeletePostData, any>({
      path: `/api/systemPlugin/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemPlugin
   * @name SystemPluginPagePost
   * @summary 获取动态插件列表
   * @request POST:/api/systemPlugin/page
   */
  systemPluginPagePost = (
    data: PagePluginRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemPluginPagePostData, any>({
      path: `/api/systemPlugin/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemPlugin
   * @name SystemPluginRemoveAssemblyPost
   * @summary 移除动态程序集/接口
   * @request POST:/api/systemPlugin/removeAssembly
   */
  systemPluginRemoveAssemblyPost = (
    query?: {
      assemblyName?: string;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<SystemPluginRemoveAssemblyPostData, any>({
      path: `/api/systemPlugin/removeAssembly`,
      method: "POST",
      query: query,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemPlugin
   * @name SystemPluginUpdatePost
   * @summary 更新动态插件
   * @request POST:/api/systemPlugin/update
   */
  systemPluginUpdatePost = (
    data: UpdatePluginRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemPluginUpdatePostData, any>({
      path: `/api/systemPlugin/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const systemPluginAddPost = (...args: Parameters<InstanceType<typeof SystemPlugin>['systemPluginAddPost']>) => getAPI(SystemPlugin).systemPluginAddPost(...args);
export const systemPluginCompileAssemblyPost = (...args: Parameters<InstanceType<typeof SystemPlugin>['systemPluginCompileAssemblyPost']>) => getAPI(SystemPlugin).systemPluginCompileAssemblyPost(...args);
export const systemPluginDeletePost = (...args: Parameters<InstanceType<typeof SystemPlugin>['systemPluginDeletePost']>) => getAPI(SystemPlugin).systemPluginDeletePost(...args);
export const systemPluginPagePost = (...args: Parameters<InstanceType<typeof SystemPlugin>['systemPluginPagePost']>) => getAPI(SystemPlugin).systemPluginPagePost(...args);
export const systemPluginRemoveAssemblyPost = (...args: Parameters<InstanceType<typeof SystemPlugin>['systemPluginRemoveAssemblyPost']>) => getAPI(SystemPlugin).systemPluginRemoveAssemblyPost(...args);
export const systemPluginUpdatePost = (...args: Parameters<InstanceType<typeof SystemPlugin>['systemPluginUpdatePost']>) => getAPI(SystemPlugin).systemPluginUpdatePost(...args);
