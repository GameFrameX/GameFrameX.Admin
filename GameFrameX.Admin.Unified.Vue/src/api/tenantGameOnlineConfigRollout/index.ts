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
  PageTenantGameOnlineConfigRolloutRequest,
  PublishConfigRolloutRequest,
  RollbackConfigRolloutRequest,
  TenantGameOnlineConfigRolloutPagePostData,
  TenantGameOnlineConfigRolloutPublishPostData,
  TenantGameOnlineConfigRolloutRollbackPostData,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameOnlineConfigRollout<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameOnlineConfigRollout
   * @name TenantGameOnlineConfigRolloutPagePost
   * @summary 分页查询版本行：按 AppId（必填事实作用域）/ ConfigKind / ConfigKey / Status / ServerId 过滤，新版本在前。
   * @request POST:/api/tenantGameOnlineConfigRollout/page
   */
  tenantGameOnlineConfigRolloutPagePost = (
    data: PageTenantGameOnlineConfigRolloutRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameOnlineConfigRolloutPagePostData, any>({
      path: `/api/tenantGameOnlineConfigRollout/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameOnlineConfigRollout
   * @name TenantGameOnlineConfigRolloutPublishPost
   * @summary 灰度发布：校验（Percent 边界 / 来源存在归属 / 分群已同步）→ 分配 ConfigVersion → 落版本行 → 经单一入口下发 Online → 回写确认版本并替代旧生效行。 下发失败（Online 非 0 协议码 / 作用域拒绝）时删除刚插入的版本行后原样抛出——Online 未确认即无生效事实，不留半生效审计行，可整重试。
   * @request POST:/api/tenantGameOnlineConfigRollout/publish
   */
  tenantGameOnlineConfigRolloutPublishPost = (
    data: PublishConfigRolloutRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameOnlineConfigRolloutPublishPostData, any>({
      path: `/api/tenantGameOnlineConfigRollout/publish`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameOnlineConfigRollout
   * @name TenantGameOnlineConfigRolloutRollbackPost
   * @summary 灰度回滚：版本化新下发——目标版本语义（SegmentId/Percent）恢复到新行，新行 TargetVersion 指向目标，经单一入口 rollbackConfigRollout 通道生效。 目标版本必须存在于该配置的版本序列（无论当前是否生效——恢复任一历史版本行为均合法）；成功后旧生效行置 RolledBack。
   * @request POST:/api/tenantGameOnlineConfigRollout/rollback
   */
  tenantGameOnlineConfigRolloutRollbackPost = (
    data: RollbackConfigRolloutRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameOnlineConfigRolloutRollbackPostData, any>({
      path: `/api/tenantGameOnlineConfigRollout/rollback`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const tenantGameOnlineConfigRolloutPagePost = (...args: Parameters<InstanceType<typeof TenantGameOnlineConfigRollout>['tenantGameOnlineConfigRolloutPagePost']>) => getAPI(TenantGameOnlineConfigRollout).tenantGameOnlineConfigRolloutPagePost(...args);
export const tenantGameOnlineConfigRolloutPublishPost = (...args: Parameters<InstanceType<typeof TenantGameOnlineConfigRollout>['tenantGameOnlineConfigRolloutPublishPost']>) => getAPI(TenantGameOnlineConfigRollout).tenantGameOnlineConfigRolloutPublishPost(...args);
export const tenantGameOnlineConfigRolloutRollbackPost = (...args: Parameters<InstanceType<typeof TenantGameOnlineConfigRollout>['tenantGameOnlineConfigRolloutRollbackPost']>) => getAPI(TenantGameOnlineConfigRollout).tenantGameOnlineConfigRolloutRollbackPost(...args);
