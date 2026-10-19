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
  DeleteFileRequest,
  FileRequest,
  PageFileRequest,
  TenantFileDeletePostData,
  TenantFileDownloadFilePostData,
  TenantFilePagePostData,
  TenantFileUpdatePostData,
  TenantFileUploadAvatarPostData,
  TenantFileUploadAvatarPostPayload,
  TenantFileUploadFileFromBase64PostData,
  TenantFileUploadFilePostData,
  TenantFileUploadFilePostPayload,
  TenantFileUploadFilesPostData,
  TenantFileUploadFilesPostPayload,
  TenantFileUploadSignaturePostData,
  TenantFileUploadSignaturePostPayload,
  UploadFileFromBase64Request,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantFile<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantFile
   * @name TenantFileDeletePost
   * @summary 删除文件
   * @request POST:/api/tenantFile/delete
   */
  tenantFileDeletePost = (
    data: DeleteFileRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantFileDeletePostData, any>({
      path: `/api/tenantFile/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantFile
   * @name TenantFileDownloadFilePost
   * @summary 根据文件Id或Url下载
   * @request POST:/api/tenantFile/downloadFile
   */
  tenantFileDownloadFilePost = (
    data: FileRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantFileDownloadFilePostData, any>({
      path: `/api/tenantFile/downloadFile`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantFile
   * @name TenantFilePagePost
   * @summary 获取文件分页列表
   * @request POST:/api/tenantFile/page
   */
  tenantFilePagePost = (data: PageFileRequest, params: RequestParams = {}) =>
    this.http.request<TenantFilePagePostData, any>({
      path: `/api/tenantFile/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantFile
   * @name TenantFileUpdatePost
   * @summary 更新文件
   * @request POST:/api/tenantFile/update
   */
  tenantFileUpdatePost = (data: FileRequest, params: RequestParams = {}) =>
    this.http.request<TenantFileUpdatePostData, any>({
      path: `/api/tenantFile/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantFile
   * @name TenantFileUploadAvatarPost
   * @summary 上传头像
   * @request POST:/api/tenantFile/uploadAvatar
   */
  tenantFileUploadAvatarPost = (
    data: TenantFileUploadAvatarPostPayload,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantFileUploadAvatarPostData, any>({
      path: `/api/tenantFile/uploadAvatar`,
      method: "POST",
      body: data,
      type: ContentType.FormData,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantFile
   * @name TenantFileUploadFilePost
   * @summary 上传文件
   * @request POST:/api/tenantFile/uploadFile
   */
  tenantFileUploadFilePost = (
    data: TenantFileUploadFilePostPayload,
    query?: {
      path?: string;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<TenantFileUploadFilePostData, any>({
      path: `/api/tenantFile/uploadFile`,
      method: "POST",
      query: query,
      body: data,
      type: ContentType.FormData,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantFile
   * @name TenantFileUploadFileFromBase64Post
   * @summary 上传文件Base64
   * @request POST:/api/tenantFile/uploadFileFromBase64
   */
  tenantFileUploadFileFromBase64Post = (
    data: UploadFileFromBase64Request,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantFileUploadFileFromBase64PostData, any>({
      path: `/api/tenantFile/uploadFileFromBase64`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantFile
   * @name TenantFileUploadFilesPost
   * @summary 上传多文件
   * @request POST:/api/tenantFile/uploadFiles
   */
  tenantFileUploadFilesPost = (
    data: TenantFileUploadFilesPostPayload,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantFileUploadFilesPostData, any>({
      path: `/api/tenantFile/uploadFiles`,
      method: "POST",
      body: data,
      type: ContentType.FormData,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantFile
   * @name TenantFileUploadSignaturePost
   * @summary 上传电子签名
   * @request POST:/api/tenantFile/uploadSignature
   */
  tenantFileUploadSignaturePost = (
    data: TenantFileUploadSignaturePostPayload,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantFileUploadSignaturePostData, any>({
      path: `/api/tenantFile/uploadSignature`,
      method: "POST",
      body: data,
      type: ContentType.FormData,
      format: "json",
      ...params,
    });
}

export const tenantFileDeletePost = (...args: Parameters<InstanceType<typeof TenantFile>['tenantFileDeletePost']>) => getAPI(TenantFile).tenantFileDeletePost(...args);
export const tenantFileDownloadFilePost = (...args: Parameters<InstanceType<typeof TenantFile>['tenantFileDownloadFilePost']>) => getAPI(TenantFile).tenantFileDownloadFilePost(...args);
export const tenantFilePagePost = (...args: Parameters<InstanceType<typeof TenantFile>['tenantFilePagePost']>) => getAPI(TenantFile).tenantFilePagePost(...args);
export const tenantFileUpdatePost = (...args: Parameters<InstanceType<typeof TenantFile>['tenantFileUpdatePost']>) => getAPI(TenantFile).tenantFileUpdatePost(...args);
export const tenantFileUploadAvatarPost = (...args: Parameters<InstanceType<typeof TenantFile>['tenantFileUploadAvatarPost']>) => getAPI(TenantFile).tenantFileUploadAvatarPost(...args);
export const tenantFileUploadFilePost = (...args: Parameters<InstanceType<typeof TenantFile>['tenantFileUploadFilePost']>) => getAPI(TenantFile).tenantFileUploadFilePost(...args);
export const tenantFileUploadFileFromBase64Post = (...args: Parameters<InstanceType<typeof TenantFile>['tenantFileUploadFileFromBase64Post']>) => getAPI(TenantFile).tenantFileUploadFileFromBase64Post(...args);
export const tenantFileUploadFilesPost = (...args: Parameters<InstanceType<typeof TenantFile>['tenantFileUploadFilesPost']>) => getAPI(TenantFile).tenantFileUploadFilesPost(...args);
export const tenantFileUploadSignaturePost = (...args: Parameters<InstanceType<typeof TenantFile>['tenantFileUploadSignaturePost']>) => getAPI(TenantFile).tenantFileUploadSignaturePost(...args);
