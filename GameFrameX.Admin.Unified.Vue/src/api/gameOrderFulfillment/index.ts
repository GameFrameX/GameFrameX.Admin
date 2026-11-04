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
  FulfillmentActionRequest,
  GameOrderFulfillmentAddPostData,
  GameOrderFulfillmentCompensatePostData,
  GameOrderFulfillmentDeletePostData,
  GameOrderFulfillmentDetailGetData,
  GameOrderFulfillmentExportPostData,
  GameOrderFulfillmentPagePostData,
  GameOrderFulfillmentRefundPostData,
  GameOrderFulfillmentResendPostData,
  GameOrderFulfillmentUpdatePostData,
  PageTenantGameOrderFulfillmentEntityRequest,
  UpdateTenantGameOrderFulfillmentEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameOrderFulfillment<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameOrderFulfillment
   * @name GameOrderFulfillmentAddPost
   * @summary 新增
   * @request POST:/api/gameOrderFulfillment/add
   */
  gameOrderFulfillmentAddPost = (
    data: FulfillmentActionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameOrderFulfillmentAddPostData, any>({
      path: `/api/gameOrderFulfillment/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameOrderFulfillment
   * @name GameOrderFulfillmentCompensatePost
   * @summary 人工补偿/补发（高风险）。委托 GameOrderShipService.CompensateAsync（C113）：按 Purchase 链路重发，账本落 CompensateAsync 类型 + OperatorId 审计。 C223：Admin result 统一对象契约——失败抛 BusinessException 走 AdminResultFilter 统一错误路径，不再透传 HttpJsonResultData 双重信封。
   * @request POST:/api/gameOrderFulfillment/compensate
   */
  gameOrderFulfillmentCompensatePost = (
    data: FulfillmentActionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameOrderFulfillmentCompensatePostData, any>({
      path: `/api/gameOrderFulfillment/compensate`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameOrderFulfillment
   * @name GameOrderFulfillmentDeletePost
   * @summary 删除
   * @request POST:/api/gameOrderFulfillment/delete
   */
  gameOrderFulfillmentDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameOrderFulfillmentDeletePostData, any>({
      path: `/api/gameOrderFulfillment/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameOrderFulfillment
   * @name GameOrderFulfillmentDetailGet
   * @summary 详情
   * @request GET:/api/gameOrderFulfillment/detail
   */
  gameOrderFulfillmentDetailGet = (
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
    this.http.request<GameOrderFulfillmentDetailGetData, any>({
      path: `/api/gameOrderFulfillment/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameOrderFulfillment
   * @name GameOrderFulfillmentExportPost
   * @summary 导出当前过滤条件下的账本列表（上限 1000 行；文件化由前端处理）。
   * @request POST:/api/gameOrderFulfillment/export
   */
  gameOrderFulfillmentExportPost = (
    data: PageTenantGameOrderFulfillmentEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameOrderFulfillmentExportPostData, any>({
      path: `/api/gameOrderFulfillment/export`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameOrderFulfillment
   * @name GameOrderFulfillmentPagePost
   * @summary 分页查询
   * @request POST:/api/gameOrderFulfillment/page
   */
  gameOrderFulfillmentPagePost = (
    data: PageTenantGameOrderFulfillmentEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameOrderFulfillmentPagePostData, any>({
      path: `/api/gameOrderFulfillment/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameOrderFulfillment
   * @name GameOrderFulfillmentRefundPost
   * @summary 退款扣除（高风险）。C310 / GFX-718 起两阶段：已支付订单先渠道退款（按 PayChannel 路由 IRefundManager—— 渠道失败 / 不支持主动退款 / 配置缺失抛统一错误码 PaymentErrorCode 段 704，本地账本不动）， 渠道成功后委托 GameOrderShipService.RefundAsync（C113）：按订单状态走作废 / 扣除路径，账本落 RefundAsync 类型 + 状态 + FailureReason。 Unpaid 订单未收款，跳过渠道直接本地作废。 C223：Admin result 统一对象契约——失败抛 BusinessException 走 AdminResultFilter 统一错误路径，不再透传 HttpJsonResultData 双重信封。
   * @request POST:/api/gameOrderFulfillment/refund
   */
  gameOrderFulfillmentRefundPost = (
    data: FulfillmentActionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameOrderFulfillmentRefundPostData, any>({
      path: `/api/gameOrderFulfillment/refund`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameOrderFulfillment
   * @name GameOrderFulfillmentResendPost
   * @summary 重新发货（高风险）。收敛到 CompensateAsync（不新增发放类型枚举，C113 已锁定 Purchase/RefundAsync/CompensateAsync 三类）。 C223：Admin result 统一对象契约，同 CompensateAsync。
   * @request POST:/api/gameOrderFulfillment/resend
   */
  gameOrderFulfillmentResendPost = (
    data: FulfillmentActionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameOrderFulfillmentResendPostData, any>({
      path: `/api/gameOrderFulfillment/resend`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameOrderFulfillment
   * @name GameOrderFulfillmentUpdatePost
   * @summary 更新
   * @request POST:/api/gameOrderFulfillment/update
   */
  gameOrderFulfillmentUpdatePost = (
    data: UpdateTenantGameOrderFulfillmentEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameOrderFulfillmentUpdatePostData, any>({
      path: `/api/gameOrderFulfillment/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameOrderFulfillmentAddPost = (...args: Parameters<InstanceType<typeof GameOrderFulfillment>['gameOrderFulfillmentAddPost']>) => getAPI(GameOrderFulfillment).gameOrderFulfillmentAddPost(...args);
export const gameOrderFulfillmentCompensatePost = (...args: Parameters<InstanceType<typeof GameOrderFulfillment>['gameOrderFulfillmentCompensatePost']>) => getAPI(GameOrderFulfillment).gameOrderFulfillmentCompensatePost(...args);
export const gameOrderFulfillmentDeletePost = (...args: Parameters<InstanceType<typeof GameOrderFulfillment>['gameOrderFulfillmentDeletePost']>) => getAPI(GameOrderFulfillment).gameOrderFulfillmentDeletePost(...args);
export const gameOrderFulfillmentDetailGet = (...args: Parameters<InstanceType<typeof GameOrderFulfillment>['gameOrderFulfillmentDetailGet']>) => getAPI(GameOrderFulfillment).gameOrderFulfillmentDetailGet(...args);
export const gameOrderFulfillmentExportPost = (...args: Parameters<InstanceType<typeof GameOrderFulfillment>['gameOrderFulfillmentExportPost']>) => getAPI(GameOrderFulfillment).gameOrderFulfillmentExportPost(...args);
export const gameOrderFulfillmentPagePost = (...args: Parameters<InstanceType<typeof GameOrderFulfillment>['gameOrderFulfillmentPagePost']>) => getAPI(GameOrderFulfillment).gameOrderFulfillmentPagePost(...args);
export const gameOrderFulfillmentRefundPost = (...args: Parameters<InstanceType<typeof GameOrderFulfillment>['gameOrderFulfillmentRefundPost']>) => getAPI(GameOrderFulfillment).gameOrderFulfillmentRefundPost(...args);
export const gameOrderFulfillmentResendPost = (...args: Parameters<InstanceType<typeof GameOrderFulfillment>['gameOrderFulfillmentResendPost']>) => getAPI(GameOrderFulfillment).gameOrderFulfillmentResendPost(...args);
export const gameOrderFulfillmentUpdatePost = (...args: Parameters<InstanceType<typeof GameOrderFulfillment>['gameOrderFulfillmentUpdatePost']>) => getAPI(GameOrderFulfillment).gameOrderFulfillmentUpdatePost(...args);
