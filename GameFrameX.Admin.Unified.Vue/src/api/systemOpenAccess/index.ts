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
  AddOpenAccessRequest,
  DeleteOpenAccessRequest,
  PageOpenAccessRequest,
  SystemOpenAccessAddPostData,
  SystemOpenAccessCreateSecretPostData,
  SystemOpenAccessDeletePostData,
  SystemOpenAccessPagePostData,
  SystemOpenAccessUpdatePostData,
  UpdateOpenAccessRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class SystemOpenAccess<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags systemOpenAccess
   * @name SystemOpenAccessAddPost
   * @summary 增加开放接口身份
   * @request POST:/api/systemOpenAccess/add
   */
  systemOpenAccessAddPost = (
    data: AddOpenAccessRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemOpenAccessAddPostData, any>({
      path: `/api/systemOpenAccess/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemOpenAccess
   * @name SystemOpenAccessCreateSecretPost
   * @summary 创建密钥
   * @request POST:/api/systemOpenAccess/createSecret
   */
  systemOpenAccessCreateSecretPost = (params: RequestParams = {}) =>
    this.http.request<SystemOpenAccessCreateSecretPostData, any>({
      path: `/api/systemOpenAccess/createSecret`,
      method: "POST",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemOpenAccess
   * @name SystemOpenAccessDeletePost
   * @summary 删除开放接口身份
   * @request POST:/api/systemOpenAccess/delete
   */
  systemOpenAccessDeletePost = (
    data: DeleteOpenAccessRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemOpenAccessDeletePostData, any>({
      path: `/api/systemOpenAccess/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemOpenAccess
   * @name SystemOpenAccessPagePost
   * @summary 获取开放接口身份分页列表
   * @request POST:/api/systemOpenAccess/page
   */
  systemOpenAccessPagePost = (
    data: PageOpenAccessRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemOpenAccessPagePostData, any>({
      path: `/api/systemOpenAccess/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemOpenAccess
   * @name SystemOpenAccessUpdatePost
   * @summary 更新开放接口身份
   * @request POST:/api/systemOpenAccess/update
   */
  systemOpenAccessUpdatePost = (
    data: UpdateOpenAccessRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemOpenAccessUpdatePostData, any>({
      path: `/api/systemOpenAccess/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const systemOpenAccessAddPost = (...args: Parameters<InstanceType<typeof SystemOpenAccess>['systemOpenAccessAddPost']>) => getAPI(SystemOpenAccess).systemOpenAccessAddPost(...args);
export const systemOpenAccessCreateSecretPost = (...args: Parameters<InstanceType<typeof SystemOpenAccess>['systemOpenAccessCreateSecretPost']>) => getAPI(SystemOpenAccess).systemOpenAccessCreateSecretPost(...args);
export const systemOpenAccessDeletePost = (...args: Parameters<InstanceType<typeof SystemOpenAccess>['systemOpenAccessDeletePost']>) => getAPI(SystemOpenAccess).systemOpenAccessDeletePost(...args);
export const systemOpenAccessPagePost = (...args: Parameters<InstanceType<typeof SystemOpenAccess>['systemOpenAccessPagePost']>) => getAPI(SystemOpenAccess).systemOpenAccessPagePost(...args);
export const systemOpenAccessUpdatePost = (...args: Parameters<InstanceType<typeof SystemOpenAccess>['systemOpenAccessUpdatePost']>) => getAPI(SystemOpenAccess).systemOpenAccessUpdatePost(...args);
