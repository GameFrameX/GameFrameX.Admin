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
  AddGameAnnouncementEntityRequest,
  GameAnnouncementAddPostData,
  GameAnnouncementDeletePostData,
  GameAnnouncementDetailGetData,
  GameAnnouncementDispatchOnlinePostData,
  GameAnnouncementPagePostData,
  GameAnnouncementUpdatePostData,
  PageGameAnnouncementEntityRequest,
  UpdateGameAnnouncementEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  OnlineLiveOpsSourceDispatchRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameAnnouncement<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameAnnouncement
   * @name GameAnnouncementAddPost
   * @summary 新增
   * @request POST:/api/gameAnnouncement/add
   */
  gameAnnouncementAddPost = (
    data: AddGameAnnouncementEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAnnouncementAddPostData, any>({
      path: `/api/gameAnnouncement/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAnnouncement
   * @name GameAnnouncementDeletePost
   * @summary 删除
   * @request POST:/api/gameAnnouncement/delete
   */
  gameAnnouncementDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAnnouncementDeletePostData, any>({
      path: `/api/gameAnnouncement/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAnnouncement
   * @name GameAnnouncementDetailGet
   * @summary 详情
   * @request GET:/api/gameAnnouncement/detail
   */
  gameAnnouncementDetailGet = (
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
    this.http.request<GameAnnouncementDetailGetData, any>({
      path: `/api/gameAnnouncement/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAnnouncement
   * @name GameAnnouncementDispatchOnlinePost
   * @summary 经 Online 统一下发当前公告（来源 change C343 / vault:C8 S7.7）：转调 OnlineLiveOpsDispatchService 单一入口（pushAnnouncement 通道）， 来源类型固定注入 Announcement，存在性 / 应用归属 / 作用域校验在单一入口完成。
   * @request POST:/api/gameAnnouncement/dispatchOnline
   */
  gameAnnouncementDispatchOnlinePost = (
    data: OnlineLiveOpsSourceDispatchRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAnnouncementDispatchOnlinePostData, any>({
      path: `/api/gameAnnouncement/dispatchOnline`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAnnouncement
   * @name GameAnnouncementPagePost
   * @summary 分页查询
   * @request POST:/api/gameAnnouncement/page
   */
  gameAnnouncementPagePost = (
    data: PageGameAnnouncementEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAnnouncementPagePostData, any>({
      path: `/api/gameAnnouncement/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAnnouncement
   * @name GameAnnouncementUpdatePost
   * @summary 更新
   * @request POST:/api/gameAnnouncement/update
   */
  gameAnnouncementUpdatePost = (
    data: UpdateGameAnnouncementEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAnnouncementUpdatePostData, any>({
      path: `/api/gameAnnouncement/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameAnnouncementAddPost = (...args: Parameters<InstanceType<typeof GameAnnouncement>['gameAnnouncementAddPost']>) => getAPI(GameAnnouncement).gameAnnouncementAddPost(...args);
export const gameAnnouncementDeletePost = (...args: Parameters<InstanceType<typeof GameAnnouncement>['gameAnnouncementDeletePost']>) => getAPI(GameAnnouncement).gameAnnouncementDeletePost(...args);
export const gameAnnouncementDetailGet = (...args: Parameters<InstanceType<typeof GameAnnouncement>['gameAnnouncementDetailGet']>) => getAPI(GameAnnouncement).gameAnnouncementDetailGet(...args);
export const gameAnnouncementDispatchOnlinePost = (...args: Parameters<InstanceType<typeof GameAnnouncement>['gameAnnouncementDispatchOnlinePost']>) => getAPI(GameAnnouncement).gameAnnouncementDispatchOnlinePost(...args);
export const gameAnnouncementPagePost = (...args: Parameters<InstanceType<typeof GameAnnouncement>['gameAnnouncementPagePost']>) => getAPI(GameAnnouncement).gameAnnouncementPagePost(...args);
export const gameAnnouncementUpdatePost = (...args: Parameters<InstanceType<typeof GameAnnouncement>['gameAnnouncementUpdatePost']>) => getAPI(GameAnnouncement).gameAnnouncementUpdatePost(...args);
