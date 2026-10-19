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
  PageTenantGameOrderFulfillmentEntityRequest,
  TenantGameOrderFulfillmentEntityAddPostData,
  TenantGameOrderFulfillmentEntityCompensatePostData,
  TenantGameOrderFulfillmentEntityDeletePostData,
  TenantGameOrderFulfillmentEntityDetailGetData,
  TenantGameOrderFulfillmentEntityExportPostData,
  TenantGameOrderFulfillmentEntityPagePostData,
  TenantGameOrderFulfillmentEntityRefundPostData,
  TenantGameOrderFulfillmentEntityResendPostData,
  TenantGameOrderFulfillmentEntityUpdatePostData,
  UpdateTenantGameOrderFulfillmentEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameOrderFulfillmentEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameOrderFulfillmentEntity
   * @name TenantGameOrderFulfillmentEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameOrderFulfillmentEntity/add
   */
  tenantGameOrderFulfillmentEntityAddPost = (
    data: FulfillmentActionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameOrderFulfillmentEntityAddPostData, any>({
      path: `/api/tenantGameOrderFulfillmentEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameOrderFulfillmentEntity
   * @name TenantGameOrderFulfillmentEntityCompensatePost
   * @summary 人工补偿/补发（高风险）。委托 TenantGameOrderShipService.CompensateAsync（C113）：按 Purchase 链路重发，账本落 CompensateAsync 类型 + OperatorId 审计。 C223：Admin result 统一对象契约——失败抛 BusinessException 走 AdminResultFilter 统一错误路径，不再透传 HttpJsonResultData 双重信封。
   * @request POST:/api/tenantGameOrderFulfillmentEntity/compensate
   */
  tenantGameOrderFulfillmentEntityCompensatePost = (
    data: FulfillmentActionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameOrderFulfillmentEntityCompensatePostData, any>({
      path: `/api/tenantGameOrderFulfillmentEntity/compensate`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameOrderFulfillmentEntity
   * @name TenantGameOrderFulfillmentEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameOrderFulfillmentEntity/delete
   */
  tenantGameOrderFulfillmentEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameOrderFulfillmentEntityDeletePostData, any>({
      path: `/api/tenantGameOrderFulfillmentEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameOrderFulfillmentEntity
   * @name TenantGameOrderFulfillmentEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameOrderFulfillmentEntity/detail
   */
  tenantGameOrderFulfillmentEntityDetailGet = (
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
    this.http.request<TenantGameOrderFulfillmentEntityDetailGetData, any>({
      path: `/api/tenantGameOrderFulfillmentEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameOrderFulfillmentEntity
   * @name TenantGameOrderFulfillmentEntityExportPost
   * @summary 导出当前过滤条件下的账本列表（上限 1000 行；文件化由前端处理）。
   * @request POST:/api/tenantGameOrderFulfillmentEntity/export
   */
  tenantGameOrderFulfillmentEntityExportPost = (
    data: PageTenantGameOrderFulfillmentEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameOrderFulfillmentEntityExportPostData, any>({
      path: `/api/tenantGameOrderFulfillmentEntity/export`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameOrderFulfillmentEntity
   * @name TenantGameOrderFulfillmentEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameOrderFulfillmentEntity/page
   */
  tenantGameOrderFulfillmentEntityPagePost = (
    data: PageTenantGameOrderFulfillmentEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameOrderFulfillmentEntityPagePostData, any>({
      path: `/api/tenantGameOrderFulfillmentEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameOrderFulfillmentEntity
   * @name TenantGameOrderFulfillmentEntityRefundPost
   * @summary 退款扣除（高风险）。C310 / GFX-718 起两阶段：已支付订单先渠道退款（按 PayChannel 路由 IRefundManager—— 渠道失败 / 不支持主动退款 / 配置缺失抛统一错误码 PaymentErrorCode 段 704，本地账本不动）， 渠道成功后委托 TenantGameOrderShipService.RefundAsync（C113）：按订单状态走作废 / 扣除路径，账本落 RefundAsync 类型 + 状态 + FailureReason。 Unpaid 订单未收款，跳过渠道直接本地作废。 C223：Admin result 统一对象契约——失败抛 BusinessException 走 AdminResultFilter 统一错误路径，不再透传 HttpJsonResultData 双重信封。
   * @request POST:/api/tenantGameOrderFulfillmentEntity/refund
   */
  tenantGameOrderFulfillmentEntityRefundPost = (
    data: FulfillmentActionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameOrderFulfillmentEntityRefundPostData, any>({
      path: `/api/tenantGameOrderFulfillmentEntity/refund`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameOrderFulfillmentEntity
   * @name TenantGameOrderFulfillmentEntityResendPost
   * @summary 重新发货（高风险）。收敛到 CompensateAsync（不新增发放类型枚举，C113 已锁定 Purchase/RefundAsync/CompensateAsync 三类）。 C223：Admin result 统一对象契约，同 CompensateAsync。
   * @request POST:/api/tenantGameOrderFulfillmentEntity/resend
   */
  tenantGameOrderFulfillmentEntityResendPost = (
    data: FulfillmentActionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameOrderFulfillmentEntityResendPostData, any>({
      path: `/api/tenantGameOrderFulfillmentEntity/resend`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameOrderFulfillmentEntity
   * @name TenantGameOrderFulfillmentEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameOrderFulfillmentEntity/update
   */
  tenantGameOrderFulfillmentEntityUpdatePost = (
    data: UpdateTenantGameOrderFulfillmentEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameOrderFulfillmentEntityUpdatePostData, any>({
      path: `/api/tenantGameOrderFulfillmentEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameOrderFulfillmentEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameOrderFulfillmentEntity>['tenantGameOrderFulfillmentEntityAddPost']>) => getAPI(TenantGameOrderFulfillmentEntity).tenantGameOrderFulfillmentEntityAddPost(...args);
export const tenantGameOrderFulfillmentEntityCompensatePost = (...args: Parameters<InstanceType<typeof TenantGameOrderFulfillmentEntity>['tenantGameOrderFulfillmentEntityCompensatePost']>) => getAPI(TenantGameOrderFulfillmentEntity).tenantGameOrderFulfillmentEntityCompensatePost(...args);
export const tenantGameOrderFulfillmentEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameOrderFulfillmentEntity>['tenantGameOrderFulfillmentEntityDeletePost']>) => getAPI(TenantGameOrderFulfillmentEntity).tenantGameOrderFulfillmentEntityDeletePost(...args);
export const tenantGameOrderFulfillmentEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameOrderFulfillmentEntity>['tenantGameOrderFulfillmentEntityDetailGet']>) => getAPI(TenantGameOrderFulfillmentEntity).tenantGameOrderFulfillmentEntityDetailGet(...args);
export const tenantGameOrderFulfillmentEntityExportPost = (...args: Parameters<InstanceType<typeof TenantGameOrderFulfillmentEntity>['tenantGameOrderFulfillmentEntityExportPost']>) => getAPI(TenantGameOrderFulfillmentEntity).tenantGameOrderFulfillmentEntityExportPost(...args);
export const tenantGameOrderFulfillmentEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameOrderFulfillmentEntity>['tenantGameOrderFulfillmentEntityPagePost']>) => getAPI(TenantGameOrderFulfillmentEntity).tenantGameOrderFulfillmentEntityPagePost(...args);
export const tenantGameOrderFulfillmentEntityRefundPost = (...args: Parameters<InstanceType<typeof TenantGameOrderFulfillmentEntity>['tenantGameOrderFulfillmentEntityRefundPost']>) => getAPI(TenantGameOrderFulfillmentEntity).tenantGameOrderFulfillmentEntityRefundPost(...args);
export const tenantGameOrderFulfillmentEntityResendPost = (...args: Parameters<InstanceType<typeof TenantGameOrderFulfillmentEntity>['tenantGameOrderFulfillmentEntityResendPost']>) => getAPI(TenantGameOrderFulfillmentEntity).tenantGameOrderFulfillmentEntityResendPost(...args);
export const tenantGameOrderFulfillmentEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameOrderFulfillmentEntity>['tenantGameOrderFulfillmentEntityUpdatePost']>) => getAPI(TenantGameOrderFulfillmentEntity).tenantGameOrderFulfillmentEntityUpdatePost(...args);
