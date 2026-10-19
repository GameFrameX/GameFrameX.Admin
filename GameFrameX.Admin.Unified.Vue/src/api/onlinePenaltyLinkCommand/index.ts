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
  OnlinePenaltyLinkCommandExecutePostData,
  OnlinePenaltyLinkRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class OnlinePenaltyLinkCommand<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags onlinePenaltyLinkCommand
   * @name OnlinePenaltyLinkCommandExecutePost
   * @summary 执行处罚联动命令（高危端点）：按处罚事实源行推导作用域并下发 Online 命令。
   * @request POST:/api/onlinePenaltyLinkCommand/execute
   */
  onlinePenaltyLinkCommandExecutePost = (
    data: OnlinePenaltyLinkRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<OnlinePenaltyLinkCommandExecutePostData, any>({
      path: `/api/onlinePenaltyLinkCommand/execute`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const onlinePenaltyLinkCommandExecutePost = (...args: Parameters<InstanceType<typeof OnlinePenaltyLinkCommand>['onlinePenaltyLinkCommandExecutePost']>) => getAPI(OnlinePenaltyLinkCommand).onlinePenaltyLinkCommandExecutePost(...args);
