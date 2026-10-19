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
  AddTenantSmsConfigRequest,
  PageTenantSmsConfigRequest,
  TenantSmsConfigEntityAddPostData,
  TenantSmsConfigEntityDeletePostData,
  TenantSmsConfigEntityDetailGetData,
  TenantSmsConfigEntityPagePostData,
  TenantSmsConfigEntitySendTestPostData,
  TenantSmsConfigEntityUpdatePostData,
  TenantSmsConfigTestSendRequest,
  UpdateTenantSmsConfigRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantSmsConfigEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantSmsConfigEntity
   * @name TenantSmsConfigEntityAddPost
   * @summary 新增租户 SMS 配置。 Aliyun Provider 必须提交真实 AccessKeyId / AccessKeySecret / SignName，禁止提交 `******` 脱敏占位。
   * @request POST:/api/tenantSmsConfigEntity/add
   */
  tenantSmsConfigEntityAddPost = (
    data: AddTenantSmsConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantSmsConfigEntityAddPostData, any>({
      path: `/api/tenantSmsConfigEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantSmsConfigEntity
   * @name TenantSmsConfigEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantSmsConfigEntity/delete
   */
  tenantSmsConfigEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantSmsConfigEntityDeletePostData, any>({
      path: `/api/tenantSmsConfigEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantSmsConfigEntity
   * @name TenantSmsConfigEntityDetailGet
   * @summary 详情（敏感字段脱敏）。
   * @request GET:/api/tenantSmsConfigEntity/detail
   */
  tenantSmsConfigEntityDetailGet = (
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
    this.http.request<TenantSmsConfigEntityDetailGetData, any>({
      path: `/api/tenantSmsConfigEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantSmsConfigEntity
   * @name TenantSmsConfigEntityPagePost
   * @summary 分页查询（敏感字段脱敏）。
   * @request POST:/api/tenantSmsConfigEntity/page
   */
  tenantSmsConfigEntityPagePost = (
    data: PageTenantSmsConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantSmsConfigEntityPagePostData, any>({
      path: `/api/tenantSmsConfigEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantSmsConfigEntity
   * @name TenantSmsConfigEntitySendTestPost
   * @summary 受权限控制的测试发送接口。 按 GameFrameX.Application.Game.Tenant.Integration.TenantSmsConfig.Dto.TenantSmsConfigTestSendRequest.Id 加载当前租户配置，缺凭据、禁用、不属于当前租户、模板为空时返回明确错误。 禁止 fallback 到静态配置或其它租户配置；异常与日志不得打印 AccessKey 明文。
   * @request POST:/api/tenantSmsConfigEntity/sendTest
   */
  tenantSmsConfigEntitySendTestPost = (
    data: TenantSmsConfigTestSendRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantSmsConfigEntitySendTestPostData, any>({
      path: `/api/tenantSmsConfigEntity/sendTest`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantSmsConfigEntity
   * @name TenantSmsConfigEntityUpdatePost
   * @summary 更新租户 SMS 配置。 AccessKeyId / AccessKeySecret 回传 `******` 时保留数据库原值。
   * @request POST:/api/tenantSmsConfigEntity/update
   */
  tenantSmsConfigEntityUpdatePost = (
    data: UpdateTenantSmsConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantSmsConfigEntityUpdatePostData, any>({
      path: `/api/tenantSmsConfigEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantSmsConfigEntityAddPost = (...args: Parameters<InstanceType<typeof TenantSmsConfigEntity>['tenantSmsConfigEntityAddPost']>) => getAPI(TenantSmsConfigEntity).tenantSmsConfigEntityAddPost(...args);
export const tenantSmsConfigEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantSmsConfigEntity>['tenantSmsConfigEntityDeletePost']>) => getAPI(TenantSmsConfigEntity).tenantSmsConfigEntityDeletePost(...args);
export const tenantSmsConfigEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantSmsConfigEntity>['tenantSmsConfigEntityDetailGet']>) => getAPI(TenantSmsConfigEntity).tenantSmsConfigEntityDetailGet(...args);
export const tenantSmsConfigEntityPagePost = (...args: Parameters<InstanceType<typeof TenantSmsConfigEntity>['tenantSmsConfigEntityPagePost']>) => getAPI(TenantSmsConfigEntity).tenantSmsConfigEntityPagePost(...args);
export const tenantSmsConfigEntitySendTestPost = (...args: Parameters<InstanceType<typeof TenantSmsConfigEntity>['tenantSmsConfigEntitySendTestPost']>) => getAPI(TenantSmsConfigEntity).tenantSmsConfigEntitySendTestPost(...args);
export const tenantSmsConfigEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantSmsConfigEntity>['tenantSmsConfigEntityUpdatePost']>) => getAPI(TenantSmsConfigEntity).tenantSmsConfigEntityUpdatePost(...args);
