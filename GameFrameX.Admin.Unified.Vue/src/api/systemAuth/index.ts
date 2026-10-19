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
  LoginRequest,
  RegisterByEmailRequest,
  SendRegisterEmailCodeRequest,
  SetupSuperAdminPasswordRequest,
  SwaggerCheckUrlPostData,
  SwaggerSubmitUrlPostData,
  SwaggerSubmitUrlPostPayload,
  SystemAuthCaptchaGetData,
  SystemAuthLoginConfigGetData,
  SystemAuthLoginPostData,
  SystemAuthLogoutPostData,
  SystemAuthRefreshTokenGetData,
  SystemAuthRegisterByEmailPostData,
  SystemAuthSendRegisterEmailCodePostData,
  SystemAuthSetupSuperAdminPasswordPostData,
  SystemAuthUserInfoGetData,
  SystemAuthWatermarkConfigGetData,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class SystemAuth<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags systemAuth
   * @name SystemAuthCaptchaGet
   * @summary 获取验证码
   * @request GET:/api/systemAuth/captcha
   */
  systemAuthCaptchaGet = (params: RequestParams = {}) =>
    this.http.request<SystemAuthCaptchaGetData, any>({
      path: `/api/systemAuth/captcha`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * @description 用户名/密码：superadmin/密码首次安装时随机生成并打印于控制台
   *
   * @tags systemAuth
   * @name SystemAuthLoginPost
   * @summary 账号密码登录
   * @request POST:/api/systemAuth/login
   */
  systemAuthLoginPost = (data: LoginRequest, params: RequestParams = {}) =>
    this.http.request<SystemAuthLoginPostData, any>({
      path: `/api/systemAuth/login`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemAuth
   * @name SystemAuthLoginConfigGet
   * @summary 获取登录配置
   * @request GET:/api/systemAuth/loginConfig
   */
  systemAuthLoginConfigGet = (params: RequestParams = {}) =>
    this.http.request<SystemAuthLoginConfigGetData, any>({
      path: `/api/systemAuth/loginConfig`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemAuth
   * @name SystemAuthLogoutPost
   * @summary 退出系统
   * @request POST:/api/systemAuth/logout
   */
  systemAuthLogoutPost = (params: RequestParams = {}) =>
    this.http.request<SystemAuthLogoutPostData, any>({
      path: `/api/systemAuth/logout`,
      method: "POST",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemAuth
   * @name SystemAuthRefreshTokenGet
   * @summary 获取刷新Token
   * @request GET:/api/systemAuth/refreshToken
   */
  systemAuthRefreshTokenGet = (
    query?: {
      accessToken?: string;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<SystemAuthRefreshTokenGetData, any>({
      path: `/api/systemAuth/refreshToken`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemAuth
   * @name SystemAuthRegisterByEmailPost
   * @summary 邮箱自助注册：校验验证码后创建租户并签发登录令牌
   * @request POST:/api/systemAuth/registerByEmail
   */
  systemAuthRegisterByEmailPost = (
    data: RegisterByEmailRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemAuthRegisterByEmailPostData, any>({
      path: `/api/systemAuth/registerByEmail`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemAuth
   * @name SystemAuthSendRegisterEmailCodePost
   * @summary 发送邮箱自助注册验证码
   * @request POST:/api/systemAuth/sendRegisterEmailCode
   */
  systemAuthSendRegisterEmailCodePost = (
    data: SendRegisterEmailCodeRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemAuthSendRegisterEmailCodePostData, any>({
      path: `/api/systemAuth/sendRegisterEmailCode`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemAuth
   * @name SystemAuthSetupSuperAdminPasswordPost
   * @summary 平台超管首装密码初始化：仅在密码未配置窗口内可用，设置成功即自动登录。 fail-closed：密码一旦配置（向导设置或环境变量注入）该端点永久拒绝； 条件更新（WHERE password_configured = false）保证并发竞速只有一票生效。
   * @request POST:/api/systemAuth/setupSuperAdminPassword
   */
  systemAuthSetupSuperAdminPasswordPost = (
    data: SetupSuperAdminPasswordRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemAuthSetupSuperAdminPasswordPostData, any>({
      path: `/api/systemAuth/setupSuperAdminPassword`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemAuth
   * @name SystemAuthUserInfoGet
   * @summary 获取登录账号
   * @request GET:/api/systemAuth/userInfo
   */
  systemAuthUserInfoGet = (params: RequestParams = {}) =>
    this.http.request<SystemAuthUserInfoGetData, any>({
      path: `/api/systemAuth/userInfo`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemAuth
   * @name SystemAuthWatermarkConfigGet
   * @summary 获取水印配置
   * @request GET:/api/systemAuth/watermarkConfig
   */
  systemAuthWatermarkConfigGet = (params: RequestParams = {}) =>
    this.http.request<SystemAuthWatermarkConfigGetData, any>({
      path: `/api/systemAuth/watermarkConfig`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemAuth
   * @name SwaggerCheckUrlPost
   * @summary Swagger登录检查
   * @request POST:/swagger/checkUrl
   */
  swaggerCheckUrlPost = (params: RequestParams = {}) =>
    this.http.request<SwaggerCheckUrlPostData, any>({
      path: `/swagger/checkUrl`,
      method: "POST",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemAuth
   * @name SwaggerSubmitUrlPost
   * @summary Swagger登录提交
   * @request POST:/swagger/submitUrl
   */
  swaggerSubmitUrlPost = (
    data: SwaggerSubmitUrlPostPayload,
    params: RequestParams = {},
  ) =>
    this.http.request<SwaggerSubmitUrlPostData, any>({
      path: `/swagger/submitUrl`,
      method: "POST",
      body: data,
      type: ContentType.FormData,
      format: "json",
      ...params,
    });
}

export const systemAuthCaptchaGet = (...args: Parameters<InstanceType<typeof SystemAuth>['systemAuthCaptchaGet']>) => getAPI(SystemAuth).systemAuthCaptchaGet(...args);
export const systemAuthLoginPost = (...args: Parameters<InstanceType<typeof SystemAuth>['systemAuthLoginPost']>) => getAPI(SystemAuth).systemAuthLoginPost(...args);
export const systemAuthLoginConfigGet = (...args: Parameters<InstanceType<typeof SystemAuth>['systemAuthLoginConfigGet']>) => getAPI(SystemAuth).systemAuthLoginConfigGet(...args);
export const systemAuthLogoutPost = (...args: Parameters<InstanceType<typeof SystemAuth>['systemAuthLogoutPost']>) => getAPI(SystemAuth).systemAuthLogoutPost(...args);
export const systemAuthRefreshTokenGet = (...args: Parameters<InstanceType<typeof SystemAuth>['systemAuthRefreshTokenGet']>) => getAPI(SystemAuth).systemAuthRefreshTokenGet(...args);
export const systemAuthRegisterByEmailPost = (...args: Parameters<InstanceType<typeof SystemAuth>['systemAuthRegisterByEmailPost']>) => getAPI(SystemAuth).systemAuthRegisterByEmailPost(...args);
export const systemAuthSendRegisterEmailCodePost = (...args: Parameters<InstanceType<typeof SystemAuth>['systemAuthSendRegisterEmailCodePost']>) => getAPI(SystemAuth).systemAuthSendRegisterEmailCodePost(...args);
export const systemAuthSetupSuperAdminPasswordPost = (...args: Parameters<InstanceType<typeof SystemAuth>['systemAuthSetupSuperAdminPasswordPost']>) => getAPI(SystemAuth).systemAuthSetupSuperAdminPasswordPost(...args);
export const systemAuthUserInfoGet = (...args: Parameters<InstanceType<typeof SystemAuth>['systemAuthUserInfoGet']>) => getAPI(SystemAuth).systemAuthUserInfoGet(...args);
export const systemAuthWatermarkConfigGet = (...args: Parameters<InstanceType<typeof SystemAuth>['systemAuthWatermarkConfigGet']>) => getAPI(SystemAuth).systemAuthWatermarkConfigGet(...args);
export const swaggerCheckUrlPost = (...args: Parameters<InstanceType<typeof SystemAuth>['swaggerCheckUrlPost']>) => getAPI(SystemAuth).swaggerCheckUrlPost(...args);
export const swaggerSubmitUrlPost = (...args: Parameters<InstanceType<typeof SystemAuth>['swaggerSubmitUrlPost']>) => getAPI(SystemAuth).swaggerSubmitUrlPost(...args);
