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
  AddSystemEmailProviderRequest,
  PageSystemEmailProviderRequest,
  SystemEmailProviderAddPostData,
  SystemEmailProviderDeletePostData,
  SystemEmailProviderDetailGetData,
  SystemEmailProviderPagePostData,
  SystemEmailProviderSendTestPostData,
  SystemEmailProviderTestSendRequest,
  SystemEmailProviderUpdatePostData,
  UpdateSystemEmailProviderRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class SystemEmailProvider<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags systemEmailProvider
   * @name SystemEmailProviderAddPost
   * @summary 新增平台系统邮件 Provider 配置。 缺 Provider / 发件人邮箱 / Provider 必填凭据会直接拒绝；同一 Purpose 可保留多条配置并交给 Priority 决定选择顺序。
   * @request POST:/api/systemEmailProvider/add
   */
  systemEmailProviderAddPost = (
    data: AddSystemEmailProviderRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemEmailProviderAddPostData, any>({
      path: `/api/systemEmailProvider/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemEmailProvider
   * @name SystemEmailProviderDeletePost
   * @summary 删除
   * @request POST:/api/systemEmailProvider/delete
   */
  systemEmailProviderDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemEmailProviderDeletePostData, any>({
      path: `/api/systemEmailProvider/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemEmailProvider
   * @name SystemEmailProviderDetailGet
   * @summary 详情（敏感字段脱敏）。
   * @request GET:/api/systemEmailProvider/detail
   */
  systemEmailProviderDetailGet = (
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
    this.http.request<SystemEmailProviderDetailGetData, any>({
      path: `/api/systemEmailProvider/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemEmailProvider
   * @name SystemEmailProviderPagePost
   * @summary 分页查询（敏感字段脱敏）。
   * @request POST:/api/systemEmailProvider/page
   */
  systemEmailProviderPagePost = (
    data: PageSystemEmailProviderRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemEmailProviderPagePostData, any>({
      path: `/api/systemEmailProvider/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemEmailProvider
   * @name SystemEmailProviderSendTestPost
   * @summary 受权限控制的测试发送接口。 按当前 `Id` 读取配置，缺凭据或发送失败返回明确错误，不向 API 输出敏感字段。
   * @request POST:/api/systemEmailProvider/sendTest
   */
  systemEmailProviderSendTestPost = (
    data: SystemEmailProviderTestSendRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemEmailProviderSendTestPostData, any>({
      path: `/api/systemEmailProvider/sendTest`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemEmailProvider
   * @name SystemEmailProviderUpdatePost
   * @summary 更新平台系统邮件 Provider 配置。 敏感字段回传 `******` 时保留数据库原值。
   * @request POST:/api/systemEmailProvider/update
   */
  systemEmailProviderUpdatePost = (
    data: UpdateSystemEmailProviderRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemEmailProviderUpdatePostData, any>({
      path: `/api/systemEmailProvider/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const systemEmailProviderAddPost = (...args: Parameters<InstanceType<typeof SystemEmailProvider>['systemEmailProviderAddPost']>) => getAPI(SystemEmailProvider).systemEmailProviderAddPost(...args);
export const systemEmailProviderDeletePost = (...args: Parameters<InstanceType<typeof SystemEmailProvider>['systemEmailProviderDeletePost']>) => getAPI(SystemEmailProvider).systemEmailProviderDeletePost(...args);
export const systemEmailProviderDetailGet = (...args: Parameters<InstanceType<typeof SystemEmailProvider>['systemEmailProviderDetailGet']>) => getAPI(SystemEmailProvider).systemEmailProviderDetailGet(...args);
export const systemEmailProviderPagePost = (...args: Parameters<InstanceType<typeof SystemEmailProvider>['systemEmailProviderPagePost']>) => getAPI(SystemEmailProvider).systemEmailProviderPagePost(...args);
export const systemEmailProviderSendTestPost = (...args: Parameters<InstanceType<typeof SystemEmailProvider>['systemEmailProviderSendTestPost']>) => getAPI(SystemEmailProvider).systemEmailProviderSendTestPost(...args);
export const systemEmailProviderUpdatePost = (...args: Parameters<InstanceType<typeof SystemEmailProvider>['systemEmailProviderUpdatePost']>) => getAPI(SystemEmailProvider).systemEmailProviderUpdatePost(...args);
