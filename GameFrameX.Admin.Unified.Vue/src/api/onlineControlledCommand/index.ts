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
  OnlineControlledCommandExecutePostData,
  OnlineControlledCommandRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class OnlineControlledCommand<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags onlineControlledCommand
   * @name OnlineControlledCommandExecutePost
   * @summary 执行受控操作命令（高危端点）：四值命令路由 + 目标/方向校验 + 双人复核字段必填。 幂等语义（vault:C9 VC-8.4 / vault:C2 S1.5）：每次调用按 M:GameFrameX.Application.Game.Online.Console.OnlineControlledCommandService.BuildIdempotencyKey(System.String,System.Nullable{GameFrameX.Application.Game.Online.Console.Dto.ControlledCommandDirection},System.String,System.Int64) 规则派生 Online IdempotencyKey 下发——同一（命令, 方向, 目标, 区服） 组合的重复调用携带相同幂等键，Online 侧按契约返回首次执行结果，不产生第二次副作用； 因此网络重试 / 运营重复点击天然安全，失败可直接重试。
   * @request POST:/api/onlineControlledCommand/execute
   */
  onlineControlledCommandExecutePost = (
    data: OnlineControlledCommandRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<OnlineControlledCommandExecutePostData, any>({
      path: `/api/onlineControlledCommand/execute`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const onlineControlledCommandExecutePost = (...args: Parameters<InstanceType<typeof OnlineControlledCommand>['onlineControlledCommandExecutePost']>) => getAPI(OnlineControlledCommand).onlineControlledCommandExecutePost(...args);
