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
  OnlineAuditSearchPagePostData,
  PageOnlineAuditSearchRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class OnlineAuditSearch<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags onlineAuditSearch
   * @name OnlineAuditSearchPagePost
   * @summary 统一审计检索分页：三来源各取时间倒序 top（PageSize+1）→ 统一映射 → 五维内存过滤 → 跨源 OccurredAt 倒序归并取一页。 分页语义（时间倒序 keyset）：NextCursor = 本页末行 OccurredAt Unix 秒，下一页各来源只取 OccurredAt 严格小于 cursor 的行。<b>同秒截断边界</b>：Unix 秒粒度下同一秒内多行被页边界截断时，下一页会跳过同秒的后续行—— 秒级时间戳无法区分同秒行序，属已知精度限制，非数据丢失（重放检索用时间窗过滤代替深翻页可达同区域）。 深翻页近似边界：每来源只翻 top（PageSize+1）即参与归并——若某来源的命中行稀疏分散在时间深处（五维过滤 命中率低），该来源可能在未翻尽时提前不再贡献行，HasMore 随之为 false。缓解：作用域/时间/对象维度尽量 SQL 预筛下推（见各来源查询），稀疏过滤请优先收窄时间窗与作用域，而非无限深翻页。
   * @request POST:/api/onlineAuditSearch/page
   */
  onlineAuditSearchPagePost = (
    data: PageOnlineAuditSearchRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<OnlineAuditSearchPagePostData, any>({
      path: `/api/onlineAuditSearch/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const onlineAuditSearchPagePost = (...args: Parameters<InstanceType<typeof OnlineAuditSearch>['onlineAuditSearchPagePost']>) => getAPI(OnlineAuditSearch).onlineAuditSearchPagePost(...args);
