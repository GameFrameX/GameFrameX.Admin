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
  AddTenantGameReportEntityRequest,
  LinkPenaltyTenantGameReportEntityRequest,
  PageTenantGameReportEntityRequest,
  TenantGameReportEntityAddPostData,
  TenantGameReportEntityDeletePostData,
  TenantGameReportEntityDetailGetData,
  TenantGameReportEntityLinkPenaltyPostData,
  TenantGameReportEntityPagePostData,
  TenantGameReportEntityUpdatePostData,
  UpdateTenantGameReportEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameReportEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameReportEntity
   * @name TenantGameReportEntityAddPost
   * @summary 录入举报：AppId 必填且必须大于 0；Status/HandlerId 服务端强制。
   * @request POST:/api/tenantGameReportEntity/add
   */
  tenantGameReportEntityAddPost = (
    data: AddTenantGameReportEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameReportEntityAddPostData, any>({
      path: `/api/tenantGameReportEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameReportEntity
   * @name TenantGameReportEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameReportEntity/delete
   */
  tenantGameReportEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameReportEntityDeletePostData, any>({
      path: `/api/tenantGameReportEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameReportEntity
   * @name TenantGameReportEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameReportEntity/detail
   */
  tenantGameReportEntityDetailGet = (
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
    this.http.request<TenantGameReportEntityDetailGetData, any>({
      path: `/api/tenantGameReportEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameReportEntity
   * @name TenantGameReportEntityLinkPenaltyPost
   * @summary 关联处罚：仅 Confirmed 举报可关联；处罚必须存在；写 ResultPenaltyId + HandlerId，不自动创建处罚。
   * @request POST:/api/tenantGameReportEntity/linkPenalty
   */
  tenantGameReportEntityLinkPenaltyPost = (
    data: LinkPenaltyTenantGameReportEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameReportEntityLinkPenaltyPostData, any>({
      path: `/api/tenantGameReportEntity/linkPenalty`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameReportEntity
   * @name TenantGameReportEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameReportEntity/page
   */
  tenantGameReportEntityPagePost = (
    data: PageTenantGameReportEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameReportEntityPagePostData, any>({
      path: `/api/tenantGameReportEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameReportEntity
   * @name TenantGameReportEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameReportEntity/update
   */
  tenantGameReportEntityUpdatePost = (
    data: UpdateTenantGameReportEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameReportEntityUpdatePostData, any>({
      path: `/api/tenantGameReportEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameReportEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameReportEntity>['tenantGameReportEntityAddPost']>) => getAPI(TenantGameReportEntity).tenantGameReportEntityAddPost(...args);
export const tenantGameReportEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameReportEntity>['tenantGameReportEntityDeletePost']>) => getAPI(TenantGameReportEntity).tenantGameReportEntityDeletePost(...args);
export const tenantGameReportEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameReportEntity>['tenantGameReportEntityDetailGet']>) => getAPI(TenantGameReportEntity).tenantGameReportEntityDetailGet(...args);
export const tenantGameReportEntityLinkPenaltyPost = (...args: Parameters<InstanceType<typeof TenantGameReportEntity>['tenantGameReportEntityLinkPenaltyPost']>) => getAPI(TenantGameReportEntity).tenantGameReportEntityLinkPenaltyPost(...args);
export const tenantGameReportEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameReportEntity>['tenantGameReportEntityPagePost']>) => getAPI(TenantGameReportEntity).tenantGameReportEntityPagePost(...args);
export const tenantGameReportEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameReportEntity>['tenantGameReportEntityUpdatePost']>) => getAPI(TenantGameReportEntity).tenantGameReportEntityUpdatePost(...args);
