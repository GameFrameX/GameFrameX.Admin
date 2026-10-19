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
  AddTenantWechatConfigRequest,
  PageTenantWechatConfigRequest,
  TenantWechatConfigEntityAddPostData,
  TenantWechatConfigEntityDeletePostData,
  TenantWechatConfigEntityDetailGetData,
  TenantWechatConfigEntityPagePostData,
  TenantWechatConfigEntitySendTestPostData,
  TenantWechatConfigEntityUpdatePostData,
  TenantWechatConfigTestSendRequest,
  UpdateTenantWechatConfigRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantWechatConfigEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantWechatConfigEntity
   * @name TenantWechatConfigEntityAddPost
   * @summary 新增租户 Wechat 配置。 公众号 / 小程序凭证对至少配置一组且每组成对完整。
   * @request POST:/api/tenantWechatConfigEntity/add
   */
  tenantWechatConfigEntityAddPost = (
    data: AddTenantWechatConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantWechatConfigEntityAddPostData, any>({
      path: `/api/tenantWechatConfigEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantWechatConfigEntity
   * @name TenantWechatConfigEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantWechatConfigEntity/delete
   */
  tenantWechatConfigEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantWechatConfigEntityDeletePostData, any>({
      path: `/api/tenantWechatConfigEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantWechatConfigEntity
   * @name TenantWechatConfigEntityDetailGet
   * @summary 详情（敏感字段脱敏）。
   * @request GET:/api/tenantWechatConfigEntity/detail
   */
  tenantWechatConfigEntityDetailGet = (
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
    this.http.request<TenantWechatConfigEntityDetailGetData, any>({
      path: `/api/tenantWechatConfigEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantWechatConfigEntity
   * @name TenantWechatConfigEntityPagePost
   * @summary 分页查询（敏感字段脱敏）。
   * @request POST:/api/tenantWechatConfigEntity/page
   */
  tenantWechatConfigEntityPagePost = (
    data: PageTenantWechatConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantWechatConfigEntityPagePostData, any>({
      path: `/api/tenantWechatConfigEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantWechatConfigEntity
   * @name TenantWechatConfigEntitySendTestPost
   * @summary 受权限控制的测试发送接口。 按当前配置 GameFrameX.Application.Game.Tenant.Integration.TenantWechatConfig.Dto.TenantWechatConfigTestSendRequest.Id 读取公众号 / 小程序凭证，缺租户上下文 / 缺配置 / 禁用 / 字段不完整返回明确错误，禁止 fallback 到静态配置或其它租户配置。 本接口复用租户配置的公众号凭证对发送模板消息，验证凭证有效性 + 模板可达性。小程序订阅消息测试请走 `SystemWxOpenService`。 执行过程不得向日志 / 异常消息输出 Secret 明文。
   * @request POST:/api/tenantWechatConfigEntity/sendTest
   */
  tenantWechatConfigEntitySendTestPost = (
    data: TenantWechatConfigTestSendRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantWechatConfigEntitySendTestPostData, any>({
      path: `/api/tenantWechatConfigEntity/sendTest`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantWechatConfigEntity
   * @name TenantWechatConfigEntityUpdatePost
   * @summary 更新租户 Wechat 配置。 敏感字段回传 `******` 时保留数据库原值。
   * @request POST:/api/tenantWechatConfigEntity/update
   */
  tenantWechatConfigEntityUpdatePost = (
    data: UpdateTenantWechatConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantWechatConfigEntityUpdatePostData, any>({
      path: `/api/tenantWechatConfigEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantWechatConfigEntityAddPost = (...args: Parameters<InstanceType<typeof TenantWechatConfigEntity>['tenantWechatConfigEntityAddPost']>) => getAPI(TenantWechatConfigEntity).tenantWechatConfigEntityAddPost(...args);
export const tenantWechatConfigEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantWechatConfigEntity>['tenantWechatConfigEntityDeletePost']>) => getAPI(TenantWechatConfigEntity).tenantWechatConfigEntityDeletePost(...args);
export const tenantWechatConfigEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantWechatConfigEntity>['tenantWechatConfigEntityDetailGet']>) => getAPI(TenantWechatConfigEntity).tenantWechatConfigEntityDetailGet(...args);
export const tenantWechatConfigEntityPagePost = (...args: Parameters<InstanceType<typeof TenantWechatConfigEntity>['tenantWechatConfigEntityPagePost']>) => getAPI(TenantWechatConfigEntity).tenantWechatConfigEntityPagePost(...args);
export const tenantWechatConfigEntitySendTestPost = (...args: Parameters<InstanceType<typeof TenantWechatConfigEntity>['tenantWechatConfigEntitySendTestPost']>) => getAPI(TenantWechatConfigEntity).tenantWechatConfigEntitySendTestPost(...args);
export const tenantWechatConfigEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantWechatConfigEntity>['tenantWechatConfigEntityUpdatePost']>) => getAPI(TenantWechatConfigEntity).tenantWechatConfigEntityUpdatePost(...args);
