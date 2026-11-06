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
  AddGameMailCampaignEntityRequest,
  GameMailCampaignAddPostData,
  GameMailCampaignDeletePostData,
  GameMailCampaignDetailGetData,
  GameMailCampaignDispatchOnlinePostData,
  GameMailCampaignPagePostData,
  GameMailCampaignPreviewPostData,
  GameMailCampaignPublishPostData,
  GameMailCampaignRevokePostData,
  GameMailCampaignUpdatePostData,
  PageGameMailCampaignEntityRequest,
  PreviewMailCampaignEntityRequest,
  PublishMailCampaignEntityRequest,
  RevokeMailCampaignEntityRequest,
  UpdateGameMailCampaignEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  OnlineLiveOpsSourceDispatchRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameMailCampaign<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameMailCampaign
   * @name GameMailCampaignAddPost
   * @summary 新增
   * @request POST:/api/gameMailCampaign/add
   */
  gameMailCampaignAddPost = (
    data: AddGameMailCampaignEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameMailCampaignAddPostData, any>({
      path: `/api/gameMailCampaign/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMailCampaign
   * @name GameMailCampaignDeletePost
   * @summary 删除：仅 Draft/Failed 可删除（Published/Revoked 主体不可删 B1）。
   * @request POST:/api/gameMailCampaign/delete
   */
  gameMailCampaignDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameMailCampaignDeletePostData, any>({
      path: `/api/gameMailCampaign/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMailCampaign
   * @name GameMailCampaignDetailGet
   * @summary 详情
   * @request GET:/api/gameMailCampaign/detail
   */
  gameMailCampaignDetailGet = (
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
    this.http.request<GameMailCampaignDetailGetData, any>({
      path: `/api/gameMailCampaign/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMailCampaign
   * @name GameMailCampaignDispatchOnlinePost
   * @summary 经 Online 统一下发当前 Campaign（来源 change C343 / vault:C8 S7.7，方案C）：转调 OnlineLiveOpsDispatchService 单一入口（publish_mail_campaign 通道）。 邮件不重复下发：单一入口只收 Published（同目标 ServerId==AreaId 合并返回首次发布回执）；Draft/Failed/Revoked 拒绝并指引走本服务 publish 链路。
   * @request POST:/api/gameMailCampaign/dispatchOnline
   */
  gameMailCampaignDispatchOnlinePost = (
    data: OnlineLiveOpsSourceDispatchRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameMailCampaignDispatchOnlinePostData, any>({
      path: `/api/gameMailCampaign/dispatchOnline`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMailCampaign
   * @name GameMailCampaignPagePost
   * @summary 分页查询
   * @request POST:/api/gameMailCampaign/page
   */
  gameMailCampaignPagePost = (
    data: PageGameMailCampaignEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameMailCampaignPagePostData, any>({
      path: `/api/gameMailCampaign/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMailCampaign
   * @name GameMailCampaignPreviewPost
   * @summary 预览 / 校验 Campaign：调用 Server preview，不落库、不改状态。预估命中数一期为 -1（未知）。
   * @request POST:/api/gameMailCampaign/preview
   */
  gameMailCampaignPreviewPost = (
    data: PreviewMailCampaignEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameMailCampaignPreviewPostData, any>({
      path: `/api/gameMailCampaign/preview`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMailCampaign
   * @name GameMailCampaignPublishPost
   * @summary 发布 Campaign：调用 Server publish。Ok 时回写 ServerCampaignId/PublishVersion/PublishedTime 并置 Published；非 Ok 时置 Failed 并记录 LastServerCode/Message，抛出友好错误。 仅 Draft/Failed 可发布（Published/Revoked 主体不可重复发布 B1）。
   * @request POST:/api/gameMailCampaign/publish
   */
  gameMailCampaignPublishPost = (
    data: PublishMailCampaignEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameMailCampaignPublishPostData, any>({
      path: `/api/gameMailCampaign/publish`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMailCampaign
   * @name GameMailCampaignRevokePost
   * @summary 撤回 Campaign：调用 Server revoke。仅 Published 可撤回（撤回不可逆 B3，已发放资产不回滚）。Ok 时回写 RevokedTime/RevokeOperator 并置 Revoked。
   * @request POST:/api/gameMailCampaign/revoke
   */
  gameMailCampaignRevokePost = (
    data: RevokeMailCampaignEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameMailCampaignRevokePostData, any>({
      path: `/api/gameMailCampaign/revoke`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMailCampaign
   * @name GameMailCampaignUpdatePost
   * @summary 更新
   * @request POST:/api/gameMailCampaign/update
   */
  gameMailCampaignUpdatePost = (
    data: UpdateGameMailCampaignEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameMailCampaignUpdatePostData, any>({
      path: `/api/gameMailCampaign/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameMailCampaignAddPost = (...args: Parameters<InstanceType<typeof GameMailCampaign>['gameMailCampaignAddPost']>) => getAPI(GameMailCampaign).gameMailCampaignAddPost(...args);
export const gameMailCampaignDeletePost = (...args: Parameters<InstanceType<typeof GameMailCampaign>['gameMailCampaignDeletePost']>) => getAPI(GameMailCampaign).gameMailCampaignDeletePost(...args);
export const gameMailCampaignDetailGet = (...args: Parameters<InstanceType<typeof GameMailCampaign>['gameMailCampaignDetailGet']>) => getAPI(GameMailCampaign).gameMailCampaignDetailGet(...args);
export const gameMailCampaignDispatchOnlinePost = (...args: Parameters<InstanceType<typeof GameMailCampaign>['gameMailCampaignDispatchOnlinePost']>) => getAPI(GameMailCampaign).gameMailCampaignDispatchOnlinePost(...args);
export const gameMailCampaignPagePost = (...args: Parameters<InstanceType<typeof GameMailCampaign>['gameMailCampaignPagePost']>) => getAPI(GameMailCampaign).gameMailCampaignPagePost(...args);
export const gameMailCampaignPreviewPost = (...args: Parameters<InstanceType<typeof GameMailCampaign>['gameMailCampaignPreviewPost']>) => getAPI(GameMailCampaign).gameMailCampaignPreviewPost(...args);
export const gameMailCampaignPublishPost = (...args: Parameters<InstanceType<typeof GameMailCampaign>['gameMailCampaignPublishPost']>) => getAPI(GameMailCampaign).gameMailCampaignPublishPost(...args);
export const gameMailCampaignRevokePost = (...args: Parameters<InstanceType<typeof GameMailCampaign>['gameMailCampaignRevokePost']>) => getAPI(GameMailCampaign).gameMailCampaignRevokePost(...args);
export const gameMailCampaignUpdatePost = (...args: Parameters<InstanceType<typeof GameMailCampaign>['gameMailCampaignUpdatePost']>) => getAPI(GameMailCampaign).gameMailCampaignUpdatePost(...args);
