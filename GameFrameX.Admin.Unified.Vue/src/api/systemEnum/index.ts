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
  SystemEnumEnumDataListByFieldGetData,
  SystemEnumEnumDataListGetData,
  SystemEnumEnumTypeListGetData,
} from "./types";
import { HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class SystemEnum<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags systemEnum
   * @name SystemEnumEnumDataListGet
   * @summary 通过枚举类型获取枚举值集合。仅允许读取带 GameFrameX.Entity.Enum.AdminEnumAttribute 标记的枚举， 未标记的枚举按 GameFrameX.Core.Enum.Errors.ProtocolErrorCode.NoEnumType 拒绝，避免按名绕过公开边界。
   * @request GET:/api/systemEnum/enumDataList
   */
  systemEnumEnumDataListGet = (
    query: {
      /**
       * 枚举类型名称
       * @example "AccountTypeEnum"
       */
      enumName: string;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<SystemEnumEnumDataListGetData, any>({
      path: `/api/systemEnum/enumDataList`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemEnum
   * @name SystemEnumEnumDataListByFieldGet
   * @summary 通过实体的字段名获取相关枚举值集合（目前仅支持枚举类型）。 字段枚举类型必须带 GameFrameX.Entity.Enum.AdminEnumAttribute 标记，否则按 GameFrameX.Core.Enum.Errors.ProtocolErrorCode.NoEnumType 拒绝。
   * @request GET:/api/systemEnum/enumDataListByField
   */
  systemEnumEnumDataListByFieldGet = (
    query: {
      /**
       * 实体名称
       * @example "SystemUserEntity"
       */
      entityName: string;
      /**
       * 字段名称
       * @example "AccountType"
       */
      fieldName: string;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<SystemEnumEnumDataListByFieldGetData, any>({
      path: `/api/systemEnum/enumDataListByField`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemEnum
   * @name SystemEnumEnumTypeListGet
   * @summary 获取所有枚举类型（仅返回带 GameFrameX.Entity.Enum.AdminEnumAttribute 标记的枚举）。
   * @request GET:/api/systemEnum/enumTypeList
   */
  systemEnumEnumTypeListGet = (params: RequestParams = {}) =>
    this.http.request<SystemEnumEnumTypeListGetData, any>({
      path: `/api/systemEnum/enumTypeList`,
      method: "GET",
      format: "json",
      ...params,
    });
}

export const systemEnumEnumDataListGet = (...args: Parameters<InstanceType<typeof SystemEnum>['systemEnumEnumDataListGet']>) => getAPI(SystemEnum).systemEnumEnumDataListGet(...args);
export const systemEnumEnumDataListByFieldGet = (...args: Parameters<InstanceType<typeof SystemEnum>['systemEnumEnumDataListByFieldGet']>) => getAPI(SystemEnum).systemEnumEnumDataListByFieldGet(...args);
export const systemEnumEnumTypeListGet = (...args: Parameters<InstanceType<typeof SystemEnum>['systemEnumEnumTypeListGet']>) => getAPI(SystemEnum).systemEnumEnumTypeListGet(...args);
