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
  AddNoticeRequest,
  DeleteNoticeRequest,
  NoticeRequest,
  NoticeTypeEnum,
  PageNoticeRequest,
  TenantNoticeAddPostData,
  TenantNoticeDeletePostData,
  TenantNoticePagePostData,
  TenantNoticePageReceivedGetData,
  TenantNoticePublishPostData,
  TenantNoticeSetReadPostData,
  TenantNoticeUnReadListGetData,
  TenantNoticeUpdatePostData,
  UpdateNoticeRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantNotice<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantNotice
   * @name TenantNoticeAddPost
   * @summary 增加通知公告
   * @request POST:/api/tenantNotice/add
   */
  tenantNoticeAddPost = (data: AddNoticeRequest, params: RequestParams = {}) =>
    this.http.request<TenantNoticeAddPostData, any>({
      path: `/api/tenantNotice/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantNotice
   * @name TenantNoticeDeletePost
   * @summary 删除通知公告
   * @request POST:/api/tenantNotice/delete
   */
  tenantNoticeDeletePost = (
    data: DeleteNoticeRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantNoticeDeletePostData, any>({
      path: `/api/tenantNotice/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantNotice
   * @name TenantNoticePagePost
   * @summary 获取通知公告分页列表
   * @request POST:/api/tenantNotice/page
   */
  tenantNoticePagePost = (
    data: PageNoticeRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantNoticePagePostData, any>({
      path: `/api/tenantNotice/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantNotice
   * @name TenantNoticePageReceivedGet
   * @summary 获取接收的通知公告
   * @request GET:/api/tenantNotice/pageReceived
   */
  tenantNoticePageReceivedGet = (
    query?: {
      /** 标题 */
      title?: string;
      /** 类型（1通知 2公告） */
      type?: NoticeTypeEnum;
      searchKey?: string;
      /**
       * @format int32
       * @min 1
       * @max 2147483647
       */
      page?: number;
      /**
       * @format int32
       * @min 1
       * @max 100
       */
      pageSize?: number;
      orderBy?: string;
      isDescending?: boolean;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<TenantNoticePageReceivedGetData, any>({
      path: `/api/tenantNotice/pageReceived`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantNotice
   * @name TenantNoticePublishPost
   * @summary 发布通知公告
   * @request POST:/api/tenantNotice/publish
   */
  tenantNoticePublishPost = (data: NoticeRequest, params: RequestParams = {}) =>
    this.http.request<TenantNoticePublishPostData, any>({
      path: `/api/tenantNotice/publish`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantNotice
   * @name TenantNoticeSetReadPost
   * @summary 设置通知公告已读状态
   * @request POST:/api/tenantNotice/setRead
   */
  tenantNoticeSetReadPost = (data: NoticeRequest, params: RequestParams = {}) =>
    this.http.request<TenantNoticeSetReadPostData, any>({
      path: `/api/tenantNotice/setRead`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantNotice
   * @name TenantNoticeUnReadListGet
   * @summary 获取未读的通知公告
   * @request GET:/api/tenantNotice/unReadList
   */
  tenantNoticeUnReadListGet = (params: RequestParams = {}) =>
    this.http.request<TenantNoticeUnReadListGetData, any>({
      path: `/api/tenantNotice/unReadList`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantNotice
   * @name TenantNoticeUpdatePost
   * @summary 更新通知公告
   * @request POST:/api/tenantNotice/update
   */
  tenantNoticeUpdatePost = (
    data: UpdateNoticeRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantNoticeUpdatePostData, any>({
      path: `/api/tenantNotice/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantNoticeAddPost = (...args: Parameters<InstanceType<typeof TenantNotice>['tenantNoticeAddPost']>) => getAPI(TenantNotice).tenantNoticeAddPost(...args);
export const tenantNoticeDeletePost = (...args: Parameters<InstanceType<typeof TenantNotice>['tenantNoticeDeletePost']>) => getAPI(TenantNotice).tenantNoticeDeletePost(...args);
export const tenantNoticePagePost = (...args: Parameters<InstanceType<typeof TenantNotice>['tenantNoticePagePost']>) => getAPI(TenantNotice).tenantNoticePagePost(...args);
export const tenantNoticePageReceivedGet = (...args: Parameters<InstanceType<typeof TenantNotice>['tenantNoticePageReceivedGet']>) => getAPI(TenantNotice).tenantNoticePageReceivedGet(...args);
export const tenantNoticePublishPost = (...args: Parameters<InstanceType<typeof TenantNotice>['tenantNoticePublishPost']>) => getAPI(TenantNotice).tenantNoticePublishPost(...args);
export const tenantNoticeSetReadPost = (...args: Parameters<InstanceType<typeof TenantNotice>['tenantNoticeSetReadPost']>) => getAPI(TenantNotice).tenantNoticeSetReadPost(...args);
export const tenantNoticeUnReadListGet = (...args: Parameters<InstanceType<typeof TenantNotice>['tenantNoticeUnReadListGet']>) => getAPI(TenantNotice).tenantNoticeUnReadListGet(...args);
export const tenantNoticeUpdatePost = (...args: Parameters<InstanceType<typeof TenantNotice>['tenantNoticeUpdatePost']>) => getAPI(TenantNotice).tenantNoticeUpdatePost(...args);
