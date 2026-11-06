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
  FileDeletePostData,
  FileDownloadFilePostData,
  FilePagePostData,
  FileRequest,
  FileUpdatePostData,
  FileUploadAvatarPostData,
  FileUploadAvatarPostPayload,
  FileUploadFileFromBase64PostData,
  FileUploadFilePostData,
  FileUploadFilePostPayload,
  FileUploadFilesPostData,
  FileUploadFilesPostPayload,
  FileUploadSignaturePostData,
  FileUploadSignaturePostPayload,
  PageFileRequest,
  UploadFileFromBase64Request,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class File<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags file
   * @name FileDeletePost
   * @summary 删除文件
   * @request POST:/api/file/delete
   */
  fileDeletePost = (data: DeleteFileRequest, params: RequestParams = {}) =>
    this.http.request<FileDeletePostData, any>({
      path: `/api/file/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags file
   * @name FileDownloadFilePost
   * @summary 根据文件Id或Url下载
   * @request POST:/api/file/downloadFile
   */
  fileDownloadFilePost = (data: FileRequest, params: RequestParams = {}) =>
    this.http.request<FileDownloadFilePostData, any>({
      path: `/api/file/downloadFile`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags file
   * @name FilePagePost
   * @summary 获取文件分页列表
   * @request POST:/api/file/page
   */
  filePagePost = (data: PageFileRequest, params: RequestParams = {}) =>
    this.http.request<FilePagePostData, any>({
      path: `/api/file/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags file
   * @name FileUpdatePost
   * @summary 更新文件
   * @request POST:/api/file/update
   */
  fileUpdatePost = (data: FileRequest, params: RequestParams = {}) =>
    this.http.request<FileUpdatePostData, any>({
      path: `/api/file/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags file
   * @name FileUploadAvatarPost
   * @summary 上传头像
   * @request POST:/api/file/uploadAvatar
   */
  fileUploadAvatarPost = (
    data: FileUploadAvatarPostPayload,
    params: RequestParams = {},
  ) =>
    this.http.request<FileUploadAvatarPostData, any>({
      path: `/api/file/uploadAvatar`,
      method: "POST",
      body: data,
      type: ContentType.FormData,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags file
   * @name FileUploadFilePost
   * @summary 上传文件
   * @request POST:/api/file/uploadFile
   */
  fileUploadFilePost = (
    data: FileUploadFilePostPayload,
    query?: {
      path?: string;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<FileUploadFilePostData, any>({
      path: `/api/file/uploadFile`,
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
   * @tags file
   * @name FileUploadFileFromBase64Post
   * @summary 上传文件Base64
   * @request POST:/api/file/uploadFileFromBase64
   */
  fileUploadFileFromBase64Post = (
    data: UploadFileFromBase64Request,
    params: RequestParams = {},
  ) =>
    this.http.request<FileUploadFileFromBase64PostData, any>({
      path: `/api/file/uploadFileFromBase64`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags file
   * @name FileUploadFilesPost
   * @summary 上传多文件
   * @request POST:/api/file/uploadFiles
   */
  fileUploadFilesPost = (
    data: FileUploadFilesPostPayload,
    params: RequestParams = {},
  ) =>
    this.http.request<FileUploadFilesPostData, any>({
      path: `/api/file/uploadFiles`,
      method: "POST",
      body: data,
      type: ContentType.FormData,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags file
   * @name FileUploadSignaturePost
   * @summary 上传电子签名
   * @request POST:/api/file/uploadSignature
   */
  fileUploadSignaturePost = (
    data: FileUploadSignaturePostPayload,
    params: RequestParams = {},
  ) =>
    this.http.request<FileUploadSignaturePostData, any>({
      path: `/api/file/uploadSignature`,
      method: "POST",
      body: data,
      type: ContentType.FormData,
      format: "json",
      ...params,
    });
}

export const fileDeletePost = (...args: Parameters<InstanceType<typeof File>['fileDeletePost']>) => getAPI(File).fileDeletePost(...args);
export const fileDownloadFilePost = (...args: Parameters<InstanceType<typeof File>['fileDownloadFilePost']>) => getAPI(File).fileDownloadFilePost(...args);
export const filePagePost = (...args: Parameters<InstanceType<typeof File>['filePagePost']>) => getAPI(File).filePagePost(...args);
export const fileUpdatePost = (...args: Parameters<InstanceType<typeof File>['fileUpdatePost']>) => getAPI(File).fileUpdatePost(...args);
export const fileUploadAvatarPost = (...args: Parameters<InstanceType<typeof File>['fileUploadAvatarPost']>) => getAPI(File).fileUploadAvatarPost(...args);
export const fileUploadFilePost = (...args: Parameters<InstanceType<typeof File>['fileUploadFilePost']>) => getAPI(File).fileUploadFilePost(...args);
export const fileUploadFileFromBase64Post = (...args: Parameters<InstanceType<typeof File>['fileUploadFileFromBase64Post']>) => getAPI(File).fileUploadFileFromBase64Post(...args);
export const fileUploadFilesPost = (...args: Parameters<InstanceType<typeof File>['fileUploadFilesPost']>) => getAPI(File).fileUploadFilesPost(...args);
export const fileUploadSignaturePost = (...args: Parameters<InstanceType<typeof File>['fileUploadSignaturePost']>) => getAPI(File).fileUploadSignaturePost(...args);
