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
  SystemConstDataGetData,
  SystemConstListGetData,
} from "./types";
import { HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class SystemConst<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags systemConst
   * @name SystemConstDataGet
   * @summary 根据类名获取常量数据
   * @request GET:/api/systemConst/data/{typeName}
   */
  systemConstDataGet = (typeName: string, params: RequestParams = {}) =>
    this.http.request<SystemConstDataGetData, any>({
      path: `/api/systemConst/data/${typeName}`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemConst
   * @name SystemConstListGet
   * @summary 获取所有常量列表
   * @request GET:/api/systemConst/list
   */
  systemConstListGet = (params: RequestParams = {}) =>
    this.http.request<SystemConstListGetData, any>({
      path: `/api/systemConst/list`,
      method: "GET",
      format: "json",
      ...params,
    });
}

export const systemConstDataGet = (...args: Parameters<InstanceType<typeof SystemConst>['systemConstDataGet']>) => getAPI(SystemConst).systemConstDataGet(...args);
export const systemConstListGet = (...args: Parameters<InstanceType<typeof SystemConst>['systemConstListGet']>) => getAPI(SystemConst).systemConstListGet(...args);
