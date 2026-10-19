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
  OnlineLiveOpsDispatchDispatchPostData,
  OnlineLiveOpsDispatchRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class OnlineLiveOpsDispatch<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags onlineLiveOpsDispatch
   * @name OnlineLiveOpsDispatchDispatchPost
   * @summary 统一下发运营事件（五类来源单一入口）。校验序与来源路由表见类注释。
   * @request POST:/api/onlineLiveOpsDispatch/dispatch
   */
  onlineLiveOpsDispatchDispatchPost = (
    data: OnlineLiveOpsDispatchRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<OnlineLiveOpsDispatchDispatchPostData, any>({
      path: `/api/onlineLiveOpsDispatch/dispatch`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const onlineLiveOpsDispatchDispatchPost = (...args: Parameters<InstanceType<typeof OnlineLiveOpsDispatch>['onlineLiveOpsDispatchDispatchPost']>) => getAPI(OnlineLiveOpsDispatch).onlineLiveOpsDispatchDispatchPost(...args);
