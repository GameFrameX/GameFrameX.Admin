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
  AddTenantEmailConfigRequest,
  PageTenantEmailConfigRequest,
  TenantEmailConfigEntityAddPostData,
  TenantEmailConfigEntityDeletePostData,
  TenantEmailConfigEntityDetailGetData,
  TenantEmailConfigEntityPagePostData,
  TenantEmailConfigEntitySendTestPostData,
  TenantEmailConfigEntityUpdatePostData,
  TenantEmailConfigTestSendRequest,
  UpdateTenantEmailConfigRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantEmailConfigEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantEmailConfigEntity
   * @name TenantEmailConfigEntityAddPost
   * @summary 新增租户业务邮件配置。 AppId 与 Purpose 共同决定匹配层；同层允许多条配置，由 Priority 决定选择顺序。
   * @request POST:/api/tenantEmailConfigEntity/add
   */
  tenantEmailConfigEntityAddPost = (
    data: AddTenantEmailConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantEmailConfigEntityAddPostData, any>({
      path: `/api/tenantEmailConfigEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantEmailConfigEntity
   * @name TenantEmailConfigEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantEmailConfigEntity/delete
   */
  tenantEmailConfigEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantEmailConfigEntityDeletePostData, any>({
      path: `/api/tenantEmailConfigEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantEmailConfigEntity
   * @name TenantEmailConfigEntityDetailGet
   * @summary 详情（敏感字段脱敏）。
   * @request GET:/api/tenantEmailConfigEntity/detail
   */
  tenantEmailConfigEntityDetailGet = (
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
    this.http.request<TenantEmailConfigEntityDetailGetData, any>({
      path: `/api/tenantEmailConfigEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantEmailConfigEntity
   * @name TenantEmailConfigEntityPagePost
   * @summary 分页查询（敏感字段脱敏）。
   * @request POST:/api/tenantEmailConfigEntity/page
   */
  tenantEmailConfigEntityPagePost = (
    data: PageTenantEmailConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantEmailConfigEntityPagePostData, any>({
      path: `/api/tenantEmailConfigEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantEmailConfigEntity
   * @name TenantEmailConfigEntitySendTestPost
   * @summary 受权限控制的测试发送接口。 按 GameFrameX.Application.Game.Tenant.Integration.TenantEmailConfig.Dto.TenantEmailConfigTestSendRequest.Id 直接读取对应租户业务邮件配置，缺租户上下文 / 缺配置 / 缺凭据返回明确错误，禁止 fallback 到平台系统邮件。 调用方传入 `Id` 时跳过作用域自动选择；模板变量通过 GameFrameX.Email.TencentSes.SendEmailTemplateDataMapper 与真实发送保持一致。
   * @request POST:/api/tenantEmailConfigEntity/sendTest
   */
  tenantEmailConfigEntitySendTestPost = (
    data: TenantEmailConfigTestSendRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantEmailConfigEntitySendTestPostData, any>({
      path: `/api/tenantEmailConfigEntity/sendTest`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantEmailConfigEntity
   * @name TenantEmailConfigEntityUpdatePost
   * @summary 更新租户业务邮件配置。 SecretId / SecretKey 回传 `******` 时保留数据库原值，避免脱敏回写覆盖真实密钥。
   * @request POST:/api/tenantEmailConfigEntity/update
   */
  tenantEmailConfigEntityUpdatePost = (
    data: UpdateTenantEmailConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantEmailConfigEntityUpdatePostData, any>({
      path: `/api/tenantEmailConfigEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantEmailConfigEntityAddPost = (...args: Parameters<InstanceType<typeof TenantEmailConfigEntity>['tenantEmailConfigEntityAddPost']>) => getAPI(TenantEmailConfigEntity).tenantEmailConfigEntityAddPost(...args);
export const tenantEmailConfigEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantEmailConfigEntity>['tenantEmailConfigEntityDeletePost']>) => getAPI(TenantEmailConfigEntity).tenantEmailConfigEntityDeletePost(...args);
export const tenantEmailConfigEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantEmailConfigEntity>['tenantEmailConfigEntityDetailGet']>) => getAPI(TenantEmailConfigEntity).tenantEmailConfigEntityDetailGet(...args);
export const tenantEmailConfigEntityPagePost = (...args: Parameters<InstanceType<typeof TenantEmailConfigEntity>['tenantEmailConfigEntityPagePost']>) => getAPI(TenantEmailConfigEntity).tenantEmailConfigEntityPagePost(...args);
export const tenantEmailConfigEntitySendTestPost = (...args: Parameters<InstanceType<typeof TenantEmailConfigEntity>['tenantEmailConfigEntitySendTestPost']>) => getAPI(TenantEmailConfigEntity).tenantEmailConfigEntitySendTestPost(...args);
export const tenantEmailConfigEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantEmailConfigEntity>['tenantEmailConfigEntityUpdatePost']>) => getAPI(TenantEmailConfigEntity).tenantEmailConfigEntityUpdatePost(...args);
