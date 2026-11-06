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
  AddGameBannerAnnouncementEntityRequest,
  GameBannerAnnouncementAddPostData,
  GameBannerAnnouncementDeletePostData,
  GameBannerAnnouncementDetailGetData,
  GameBannerAnnouncementPagePostData,
  GameBannerAnnouncementUpdatePostData,
  PageGameBannerAnnouncementEntityRequest,
  UpdateGameBannerAnnouncementEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameBannerAnnouncement<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameBannerAnnouncement
   * @name GameBannerAnnouncementAddPost
   * @summary 新增
   * @request POST:/api/gameBannerAnnouncement/add
   */
  gameBannerAnnouncementAddPost = (
    data: AddGameBannerAnnouncementEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameBannerAnnouncementAddPostData, any>({
      path: `/api/gameBannerAnnouncement/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameBannerAnnouncement
   * @name GameBannerAnnouncementDeletePost
   * @summary 删除
   * @request POST:/api/gameBannerAnnouncement/delete
   */
  gameBannerAnnouncementDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameBannerAnnouncementDeletePostData, any>({
      path: `/api/gameBannerAnnouncement/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameBannerAnnouncement
   * @name GameBannerAnnouncementDetailGet
   * @summary 详情
   * @request GET:/api/gameBannerAnnouncement/detail
   */
  gameBannerAnnouncementDetailGet = (
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
    this.http.request<GameBannerAnnouncementDetailGetData, any>({
      path: `/api/gameBannerAnnouncement/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameBannerAnnouncement
   * @name GameBannerAnnouncementPagePost
   * @summary 分页查询
   * @request POST:/api/gameBannerAnnouncement/page
   */
  gameBannerAnnouncementPagePost = (
    data: PageGameBannerAnnouncementEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameBannerAnnouncementPagePostData, any>({
      path: `/api/gameBannerAnnouncement/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameBannerAnnouncement
   * @name GameBannerAnnouncementUpdatePost
   * @summary 更新
   * @request POST:/api/gameBannerAnnouncement/update
   */
  gameBannerAnnouncementUpdatePost = (
    data: UpdateGameBannerAnnouncementEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameBannerAnnouncementUpdatePostData, any>({
      path: `/api/gameBannerAnnouncement/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameBannerAnnouncementAddPost = (...args: Parameters<InstanceType<typeof GameBannerAnnouncement>['gameBannerAnnouncementAddPost']>) => getAPI(GameBannerAnnouncement).gameBannerAnnouncementAddPost(...args);
export const gameBannerAnnouncementDeletePost = (...args: Parameters<InstanceType<typeof GameBannerAnnouncement>['gameBannerAnnouncementDeletePost']>) => getAPI(GameBannerAnnouncement).gameBannerAnnouncementDeletePost(...args);
export const gameBannerAnnouncementDetailGet = (...args: Parameters<InstanceType<typeof GameBannerAnnouncement>['gameBannerAnnouncementDetailGet']>) => getAPI(GameBannerAnnouncement).gameBannerAnnouncementDetailGet(...args);
export const gameBannerAnnouncementPagePost = (...args: Parameters<InstanceType<typeof GameBannerAnnouncement>['gameBannerAnnouncementPagePost']>) => getAPI(GameBannerAnnouncement).gameBannerAnnouncementPagePost(...args);
export const gameBannerAnnouncementUpdatePost = (...args: Parameters<InstanceType<typeof GameBannerAnnouncement>['gameBannerAnnouncementUpdatePost']>) => getAPI(GameBannerAnnouncement).gameBannerAnnouncementUpdatePost(...args);
