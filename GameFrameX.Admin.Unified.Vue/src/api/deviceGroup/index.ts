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
  AddDeviceGroupRequest,
  DeviceGroupAddPostData,
  DeviceGroupDeletePostData,
  DeviceGroupDetailGetData,
  DeviceGroupPagePostData,
  DeviceGroupRequest,
  DeviceGroupSelectPostData,
  DeviceGroupSelectStringPostData,
  DeviceGroupUpdatePostData,
  UpdateDeviceGroupRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class DeviceGroup<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags deviceGroup
   * @name DeviceGroupAddPost
   * @summary 增加设备码分组
   * @request POST:/api/deviceGroup/add
   */
  deviceGroupAddPost = (
    data: AddDeviceGroupRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<DeviceGroupAddPostData, any>({
      path: `/api/deviceGroup/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags deviceGroup
   * @name DeviceGroupDeletePost
   * @summary 删除
   * @request POST:/api/deviceGroup/delete
   */
  deviceGroupDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<DeviceGroupDeletePostData, any>({
      path: `/api/deviceGroup/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags deviceGroup
   * @name DeviceGroupDetailGet
   * @summary 详情
   * @request GET:/api/deviceGroup/detail
   */
  deviceGroupDetailGet = (
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
    this.http.request<DeviceGroupDetailGetData, any>({
      path: `/api/deviceGroup/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags deviceGroup
   * @name DeviceGroupPagePost
   * @summary 分页查询
   * @request POST:/api/deviceGroup/page
   */
  deviceGroupPagePost = (
    data: DeviceGroupRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<DeviceGroupPagePostData, any>({
      path: `/api/deviceGroup/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags deviceGroup
   * @name DeviceGroupSelectPost
   * @summary 获取主库选择器列表。
   * @request POST:/api/deviceGroup/select
   */
  deviceGroupSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<DeviceGroupSelectPostData, any>({
      path: `/api/deviceGroup/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags deviceGroup
   * @name DeviceGroupSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/deviceGroup/selectString
   */
  deviceGroupSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<DeviceGroupSelectStringPostData, any>({
      path: `/api/deviceGroup/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags deviceGroup
   * @name DeviceGroupUpdatePost
   * @summary 更新
   * @request POST:/api/deviceGroup/update
   */
  deviceGroupUpdatePost = (
    data: UpdateDeviceGroupRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<DeviceGroupUpdatePostData, any>({
      path: `/api/deviceGroup/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const deviceGroupAddPost = (...args: Parameters<InstanceType<typeof DeviceGroup>['deviceGroupAddPost']>) => getAPI(DeviceGroup).deviceGroupAddPost(...args);
export const deviceGroupDeletePost = (...args: Parameters<InstanceType<typeof DeviceGroup>['deviceGroupDeletePost']>) => getAPI(DeviceGroup).deviceGroupDeletePost(...args);
export const deviceGroupDetailGet = (...args: Parameters<InstanceType<typeof DeviceGroup>['deviceGroupDetailGet']>) => getAPI(DeviceGroup).deviceGroupDetailGet(...args);
export const deviceGroupPagePost = (...args: Parameters<InstanceType<typeof DeviceGroup>['deviceGroupPagePost']>) => getAPI(DeviceGroup).deviceGroupPagePost(...args);
export const deviceGroupSelectPost = (...args: Parameters<InstanceType<typeof DeviceGroup>['deviceGroupSelectPost']>) => getAPI(DeviceGroup).deviceGroupSelectPost(...args);
export const deviceGroupSelectStringPost = (...args: Parameters<InstanceType<typeof DeviceGroup>['deviceGroupSelectStringPost']>) => getAPI(DeviceGroup).deviceGroupSelectStringPost(...args);
export const deviceGroupUpdatePost = (...args: Parameters<InstanceType<typeof DeviceGroup>['deviceGroupUpdatePost']>) => getAPI(DeviceGroup).deviceGroupUpdatePost(...args);
