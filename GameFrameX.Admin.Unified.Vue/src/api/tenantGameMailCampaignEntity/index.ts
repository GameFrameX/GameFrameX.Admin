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
  AddTenantGameMailCampaignEntityRequest,
  PageTenantGameMailCampaignEntityRequest,
  PreviewMailCampaignEntityRequest,
  PublishMailCampaignEntityRequest,
  RevokeMailCampaignEntityRequest,
  TenantGameMailCampaignEntityAddPostData,
  TenantGameMailCampaignEntityDeletePostData,
  TenantGameMailCampaignEntityDetailGetData,
  TenantGameMailCampaignEntityDispatchOnlinePostData,
  TenantGameMailCampaignEntityPagePostData,
  TenantGameMailCampaignEntityPreviewPostData,
  TenantGameMailCampaignEntityPublishPostData,
  TenantGameMailCampaignEntityRevokePostData,
  TenantGameMailCampaignEntityUpdatePostData,
  UpdateTenantGameMailCampaignEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  OnlineLiveOpsSourceDispatchRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameMailCampaignEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameMailCampaignEntity
   * @name TenantGameMailCampaignEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameMailCampaignEntity/add
   */
  tenantGameMailCampaignEntityAddPost = (
    data: AddTenantGameMailCampaignEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameMailCampaignEntityAddPostData, any>({
      path: `/api/tenantGameMailCampaignEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailCampaignEntity
   * @name TenantGameMailCampaignEntityDeletePost
   * @summary 删除：仅 Draft/Failed 可删除（Published/Revoked 主体不可删 B1）。
   * @request POST:/api/tenantGameMailCampaignEntity/delete
   */
  tenantGameMailCampaignEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameMailCampaignEntityDeletePostData, any>({
      path: `/api/tenantGameMailCampaignEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailCampaignEntity
   * @name TenantGameMailCampaignEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameMailCampaignEntity/detail
   */
  tenantGameMailCampaignEntityDetailGet = (
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
    this.http.request<TenantGameMailCampaignEntityDetailGetData, any>({
      path: `/api/tenantGameMailCampaignEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailCampaignEntity
   * @name TenantGameMailCampaignEntityDispatchOnlinePost
   * @summary 经 Online 统一下发当前 Campaign（来源 change C343 / vault:C8 S7.7，方案C）：转调 OnlineLiveOpsDispatchService 单一入口（publish_mail_campaign 通道）。 邮件不重复下发：单一入口只收 Published（同目标 ServerId==AreaId 合并返回首次发布回执）；Draft/Failed/Revoked 拒绝并指引走本服务 publish 链路。
   * @request POST:/api/tenantGameMailCampaignEntity/dispatchOnline
   */
  tenantGameMailCampaignEntityDispatchOnlinePost = (
    data: OnlineLiveOpsSourceDispatchRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameMailCampaignEntityDispatchOnlinePostData, any>({
      path: `/api/tenantGameMailCampaignEntity/dispatchOnline`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailCampaignEntity
   * @name TenantGameMailCampaignEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameMailCampaignEntity/page
   */
  tenantGameMailCampaignEntityPagePost = (
    data: PageTenantGameMailCampaignEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameMailCampaignEntityPagePostData, any>({
      path: `/api/tenantGameMailCampaignEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailCampaignEntity
   * @name TenantGameMailCampaignEntityPreviewPost
   * @summary 预览 / 校验 Campaign：调用 Server preview，不落库、不改状态。预估命中数一期为 -1（未知）。
   * @request POST:/api/tenantGameMailCampaignEntity/preview
   */
  tenantGameMailCampaignEntityPreviewPost = (
    data: PreviewMailCampaignEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameMailCampaignEntityPreviewPostData, any>({
      path: `/api/tenantGameMailCampaignEntity/preview`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailCampaignEntity
   * @name TenantGameMailCampaignEntityPublishPost
   * @summary 发布 Campaign：调用 Server publish。Ok 时回写 ServerCampaignId/PublishVersion/PublishedTime 并置 Published；非 Ok 时置 Failed 并记录 LastServerCode/Message，抛出友好错误。 仅 Draft/Failed 可发布（Published/Revoked 主体不可重复发布 B1）。
   * @request POST:/api/tenantGameMailCampaignEntity/publish
   */
  tenantGameMailCampaignEntityPublishPost = (
    data: PublishMailCampaignEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameMailCampaignEntityPublishPostData, any>({
      path: `/api/tenantGameMailCampaignEntity/publish`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailCampaignEntity
   * @name TenantGameMailCampaignEntityRevokePost
   * @summary 撤回 Campaign：调用 Server revoke。仅 Published 可撤回（撤回不可逆 B3，已发放资产不回滚）。Ok 时回写 RevokedTime/RevokeOperator 并置 Revoked。
   * @request POST:/api/tenantGameMailCampaignEntity/revoke
   */
  tenantGameMailCampaignEntityRevokePost = (
    data: RevokeMailCampaignEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameMailCampaignEntityRevokePostData, any>({
      path: `/api/tenantGameMailCampaignEntity/revoke`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailCampaignEntity
   * @name TenantGameMailCampaignEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameMailCampaignEntity/update
   */
  tenantGameMailCampaignEntityUpdatePost = (
    data: UpdateTenantGameMailCampaignEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameMailCampaignEntityUpdatePostData, any>({
      path: `/api/tenantGameMailCampaignEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameMailCampaignEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameMailCampaignEntity>['tenantGameMailCampaignEntityAddPost']>) => getAPI(TenantGameMailCampaignEntity).tenantGameMailCampaignEntityAddPost(...args);
export const tenantGameMailCampaignEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameMailCampaignEntity>['tenantGameMailCampaignEntityDeletePost']>) => getAPI(TenantGameMailCampaignEntity).tenantGameMailCampaignEntityDeletePost(...args);
export const tenantGameMailCampaignEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameMailCampaignEntity>['tenantGameMailCampaignEntityDetailGet']>) => getAPI(TenantGameMailCampaignEntity).tenantGameMailCampaignEntityDetailGet(...args);
export const tenantGameMailCampaignEntityDispatchOnlinePost = (...args: Parameters<InstanceType<typeof TenantGameMailCampaignEntity>['tenantGameMailCampaignEntityDispatchOnlinePost']>) => getAPI(TenantGameMailCampaignEntity).tenantGameMailCampaignEntityDispatchOnlinePost(...args);
export const tenantGameMailCampaignEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameMailCampaignEntity>['tenantGameMailCampaignEntityPagePost']>) => getAPI(TenantGameMailCampaignEntity).tenantGameMailCampaignEntityPagePost(...args);
export const tenantGameMailCampaignEntityPreviewPost = (...args: Parameters<InstanceType<typeof TenantGameMailCampaignEntity>['tenantGameMailCampaignEntityPreviewPost']>) => getAPI(TenantGameMailCampaignEntity).tenantGameMailCampaignEntityPreviewPost(...args);
export const tenantGameMailCampaignEntityPublishPost = (...args: Parameters<InstanceType<typeof TenantGameMailCampaignEntity>['tenantGameMailCampaignEntityPublishPost']>) => getAPI(TenantGameMailCampaignEntity).tenantGameMailCampaignEntityPublishPost(...args);
export const tenantGameMailCampaignEntityRevokePost = (...args: Parameters<InstanceType<typeof TenantGameMailCampaignEntity>['tenantGameMailCampaignEntityRevokePost']>) => getAPI(TenantGameMailCampaignEntity).tenantGameMailCampaignEntityRevokePost(...args);
export const tenantGameMailCampaignEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameMailCampaignEntity>['tenantGameMailCampaignEntityUpdatePost']>) => getAPI(TenantGameMailCampaignEntity).tenantGameMailCampaignEntityUpdatePost(...args);
