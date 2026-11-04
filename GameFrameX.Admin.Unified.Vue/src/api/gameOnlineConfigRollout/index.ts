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
  GameOnlineConfigRolloutPagePostData,
  GameOnlineConfigRolloutPublishPostData,
  GameOnlineConfigRolloutRollbackPostData,
  PageTenantGameOnlineConfigRolloutRequest,
  PublishConfigRolloutRequest,
  RollbackConfigRolloutRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameOnlineConfigRollout<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameOnlineConfigRollout
   * @name GameOnlineConfigRolloutPagePost
   * @summary 分页查询版本行：按 ConfigKind / ConfigKey / Status / ServerId 过滤，新版本在前。
   * @request POST:/api/gameOnlineConfigRollout/page
   */
  gameOnlineConfigRolloutPagePost = (
    data: PageTenantGameOnlineConfigRolloutRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameOnlineConfigRolloutPagePostData, any>({
      path: `/api/gameOnlineConfigRollout/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameOnlineConfigRollout
   * @name GameOnlineConfigRolloutPublishPost
   * @summary 灰度发布：校验（Percent 边界 / 来源存在 / 分群已同步）→ 分配 ConfigVersion → 落版本行 → 经单一入口下发 Online → 回写确认版本并替代旧生效行。 下发失败（Online 非 0 协议码 / 作用域拒绝）时删除刚插入的版本行后原样抛出——Online 未确认即无生效事实，不留半生效审计行，可整重试。
   * @request POST:/api/gameOnlineConfigRollout/publish
   */
  gameOnlineConfigRolloutPublishPost = (
    data: PublishConfigRolloutRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameOnlineConfigRolloutPublishPostData, any>({
      path: `/api/gameOnlineConfigRollout/publish`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameOnlineConfigRollout
   * @name GameOnlineConfigRolloutRollbackPost
   * @summary 灰度回滚：版本化新下发——目标版本语义（SegmentId/Percent）恢复到新行，新行 TargetVersion 指向目标，经单一入口 rollbackConfigRollout 通道生效。 目标版本必须存在于该配置的版本序列（无论当前是否生效——恢复任一历史版本行为均合法）；成功后旧生效行置 RolledBack。
   * @request POST:/api/gameOnlineConfigRollout/rollback
   */
  gameOnlineConfigRolloutRollbackPost = (
    data: RollbackConfigRolloutRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameOnlineConfigRolloutRollbackPostData, any>({
      path: `/api/gameOnlineConfigRollout/rollback`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const gameOnlineConfigRolloutPagePost = (...args: Parameters<InstanceType<typeof GameOnlineConfigRollout>['gameOnlineConfigRolloutPagePost']>) => getAPI(GameOnlineConfigRollout).gameOnlineConfigRolloutPagePost(...args);
export const gameOnlineConfigRolloutPublishPost = (...args: Parameters<InstanceType<typeof GameOnlineConfigRollout>['gameOnlineConfigRolloutPublishPost']>) => getAPI(GameOnlineConfigRollout).gameOnlineConfigRolloutPublishPost(...args);
export const gameOnlineConfigRolloutRollbackPost = (...args: Parameters<InstanceType<typeof GameOnlineConfigRollout>['gameOnlineConfigRolloutRollbackPost']>) => getAPI(GameOnlineConfigRollout).gameOnlineConfigRolloutRollbackPost(...args);
