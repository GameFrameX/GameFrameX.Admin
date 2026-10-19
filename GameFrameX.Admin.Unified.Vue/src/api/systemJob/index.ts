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
  AddJobDetailRequest,
  AddJobTriggerRequest,
  DeleteJobDetailRequest,
  DeleteJobTriggerRequest,
  JobDetailRequest,
  JobTriggerRequest,
  PageJobRequest,
  SystemJobAddJobDetailPostData,
  SystemJobAddJobTriggerPostData,
  SystemJobCancelSleepPostData,
  SystemJobDeleteJobDetailPostData,
  SystemJobDeleteJobTriggerPostData,
  SystemJobJobClusterListGetData,
  SystemJobJobTriggerListGetData,
  SystemJobPageJobDetailPostData,
  SystemJobPauseAllJobPostData,
  SystemJobPauseJobPostData,
  SystemJobPauseTriggerPostData,
  SystemJobPersistAllPostData,
  SystemJobRunJobPostData,
  SystemJobStartAllJobPostData,
  SystemJobStartJobPostData,
  SystemJobStartTriggerPostData,
  SystemJobUpdateJobDetailPostData,
  SystemJobUpdateJobTriggerPostData,
  UpdateJobDetailRequest,
  UpdateJobTriggerRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class SystemJob<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags systemJob
   * @name SystemJobAddJobDetailPost
   * @summary 添加作业
   * @request POST:/api/systemJob/addJobDetail
   */
  systemJobAddJobDetailPost = (
    data: AddJobDetailRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemJobAddJobDetailPostData, any>({
      path: `/api/systemJob/addJobDetail`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemJob
   * @name SystemJobAddJobTriggerPost
   * @summary 添加触发器
   * @request POST:/api/systemJob/addJobTrigger
   */
  systemJobAddJobTriggerPost = (
    data: AddJobTriggerRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemJobAddJobTriggerPostData, any>({
      path: `/api/systemJob/addJobTrigger`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemJob
   * @name SystemJobCancelSleepPost
   * @summary 强制唤醒作业调度器
   * @request POST:/api/systemJob/cancelSleep
   */
  systemJobCancelSleepPost = (params: RequestParams = {}) =>
    this.http.request<SystemJobCancelSleepPostData, any>({
      path: `/api/systemJob/cancelSleep`,
      method: "POST",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemJob
   * @name SystemJobDeleteJobDetailPost
   * @summary 删除作业
   * @request POST:/api/systemJob/deleteJobDetail
   */
  systemJobDeleteJobDetailPost = (
    data: DeleteJobDetailRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemJobDeleteJobDetailPostData, any>({
      path: `/api/systemJob/deleteJobDetail`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemJob
   * @name SystemJobDeleteJobTriggerPost
   * @summary 删除触发器
   * @request POST:/api/systemJob/deleteJobTrigger
   */
  systemJobDeleteJobTriggerPost = (
    data: DeleteJobTriggerRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemJobDeleteJobTriggerPostData, any>({
      path: `/api/systemJob/deleteJobTrigger`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemJob
   * @name SystemJobJobClusterListGet
   * @summary 获取集群列表
   * @request GET:/api/systemJob/jobClusterList
   */
  systemJobJobClusterListGet = (params: RequestParams = {}) =>
    this.http.request<SystemJobJobClusterListGetData, any>({
      path: `/api/systemJob/jobClusterList`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemJob
   * @name SystemJobJobTriggerListGet
   * @summary 获取触发器列表
   * @request GET:/api/systemJob/jobTriggerList
   */
  systemJobJobTriggerListGet = (
    query?: {
      /** 作业Id */
      jobId?: string;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<SystemJobJobTriggerListGetData, any>({
      path: `/api/systemJob/jobTriggerList`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemJob
   * @name SystemJobPageJobDetailPost
   * @summary 获取作业分页列表
   * @request POST:/api/systemJob/pageJobDetail
   */
  systemJobPageJobDetailPost = (
    data: PageJobRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemJobPageJobDetailPostData, any>({
      path: `/api/systemJob/pageJobDetail`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemJob
   * @name SystemJobPauseAllJobPost
   * @summary 暂停所有作业
   * @request POST:/api/systemJob/pauseAllJob
   */
  systemJobPauseAllJobPost = (params: RequestParams = {}) =>
    this.http.request<SystemJobPauseAllJobPostData, any>({
      path: `/api/systemJob/pauseAllJob`,
      method: "POST",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemJob
   * @name SystemJobPauseJobPost
   * @summary 暂停作业
   * @request POST:/api/systemJob/pauseJob
   */
  systemJobPauseJobPost = (
    data: JobDetailRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemJobPauseJobPostData, any>({
      path: `/api/systemJob/pauseJob`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemJob
   * @name SystemJobPauseTriggerPost
   * @summary 暂停触发器
   * @request POST:/api/systemJob/pauseTrigger
   */
  systemJobPauseTriggerPost = (
    data: JobTriggerRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemJobPauseTriggerPostData, any>({
      path: `/api/systemJob/pauseTrigger`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemJob
   * @name SystemJobPersistAllPost
   * @summary 强制触发所有作业持久化
   * @request POST:/api/systemJob/persistAll
   */
  systemJobPersistAllPost = (params: RequestParams = {}) =>
    this.http.request<SystemJobPersistAllPostData, any>({
      path: `/api/systemJob/persistAll`,
      method: "POST",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemJob
   * @name SystemJobRunJobPost
   * @summary 执行作业
   * @request POST:/api/systemJob/runJob
   */
  systemJobRunJobPost = (data: JobDetailRequest, params: RequestParams = {}) =>
    this.http.request<SystemJobRunJobPostData, any>({
      path: `/api/systemJob/runJob`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemJob
   * @name SystemJobStartAllJobPost
   * @summary 启动所有作业
   * @request POST:/api/systemJob/startAllJob
   */
  systemJobStartAllJobPost = (params: RequestParams = {}) =>
    this.http.request<SystemJobStartAllJobPostData, any>({
      path: `/api/systemJob/startAllJob`,
      method: "POST",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemJob
   * @name SystemJobStartJobPost
   * @summary 启动作业
   * @request POST:/api/systemJob/startJob
   */
  systemJobStartJobPost = (
    data: JobDetailRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemJobStartJobPostData, any>({
      path: `/api/systemJob/startJob`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemJob
   * @name SystemJobStartTriggerPost
   * @summary 启动触发器
   * @request POST:/api/systemJob/startTrigger
   */
  systemJobStartTriggerPost = (
    data: JobTriggerRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemJobStartTriggerPostData, any>({
      path: `/api/systemJob/startTrigger`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemJob
   * @name SystemJobUpdateJobDetailPost
   * @summary 更新作业
   * @request POST:/api/systemJob/updateJobDetail
   */
  systemJobUpdateJobDetailPost = (
    data: UpdateJobDetailRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemJobUpdateJobDetailPostData, any>({
      path: `/api/systemJob/updateJobDetail`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemJob
   * @name SystemJobUpdateJobTriggerPost
   * @summary 更新触发器
   * @request POST:/api/systemJob/updateJobTrigger
   */
  systemJobUpdateJobTriggerPost = (
    data: UpdateJobTriggerRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemJobUpdateJobTriggerPostData, any>({
      path: `/api/systemJob/updateJobTrigger`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const systemJobAddJobDetailPost = (...args: Parameters<InstanceType<typeof SystemJob>['systemJobAddJobDetailPost']>) => getAPI(SystemJob).systemJobAddJobDetailPost(...args);
export const systemJobAddJobTriggerPost = (...args: Parameters<InstanceType<typeof SystemJob>['systemJobAddJobTriggerPost']>) => getAPI(SystemJob).systemJobAddJobTriggerPost(...args);
export const systemJobCancelSleepPost = (...args: Parameters<InstanceType<typeof SystemJob>['systemJobCancelSleepPost']>) => getAPI(SystemJob).systemJobCancelSleepPost(...args);
export const systemJobDeleteJobDetailPost = (...args: Parameters<InstanceType<typeof SystemJob>['systemJobDeleteJobDetailPost']>) => getAPI(SystemJob).systemJobDeleteJobDetailPost(...args);
export const systemJobDeleteJobTriggerPost = (...args: Parameters<InstanceType<typeof SystemJob>['systemJobDeleteJobTriggerPost']>) => getAPI(SystemJob).systemJobDeleteJobTriggerPost(...args);
export const systemJobJobClusterListGet = (...args: Parameters<InstanceType<typeof SystemJob>['systemJobJobClusterListGet']>) => getAPI(SystemJob).systemJobJobClusterListGet(...args);
export const systemJobJobTriggerListGet = (...args: Parameters<InstanceType<typeof SystemJob>['systemJobJobTriggerListGet']>) => getAPI(SystemJob).systemJobJobTriggerListGet(...args);
export const systemJobPageJobDetailPost = (...args: Parameters<InstanceType<typeof SystemJob>['systemJobPageJobDetailPost']>) => getAPI(SystemJob).systemJobPageJobDetailPost(...args);
export const systemJobPauseAllJobPost = (...args: Parameters<InstanceType<typeof SystemJob>['systemJobPauseAllJobPost']>) => getAPI(SystemJob).systemJobPauseAllJobPost(...args);
export const systemJobPauseJobPost = (...args: Parameters<InstanceType<typeof SystemJob>['systemJobPauseJobPost']>) => getAPI(SystemJob).systemJobPauseJobPost(...args);
export const systemJobPauseTriggerPost = (...args: Parameters<InstanceType<typeof SystemJob>['systemJobPauseTriggerPost']>) => getAPI(SystemJob).systemJobPauseTriggerPost(...args);
export const systemJobPersistAllPost = (...args: Parameters<InstanceType<typeof SystemJob>['systemJobPersistAllPost']>) => getAPI(SystemJob).systemJobPersistAllPost(...args);
export const systemJobRunJobPost = (...args: Parameters<InstanceType<typeof SystemJob>['systemJobRunJobPost']>) => getAPI(SystemJob).systemJobRunJobPost(...args);
export const systemJobStartAllJobPost = (...args: Parameters<InstanceType<typeof SystemJob>['systemJobStartAllJobPost']>) => getAPI(SystemJob).systemJobStartAllJobPost(...args);
export const systemJobStartJobPost = (...args: Parameters<InstanceType<typeof SystemJob>['systemJobStartJobPost']>) => getAPI(SystemJob).systemJobStartJobPost(...args);
export const systemJobStartTriggerPost = (...args: Parameters<InstanceType<typeof SystemJob>['systemJobStartTriggerPost']>) => getAPI(SystemJob).systemJobStartTriggerPost(...args);
export const systemJobUpdateJobDetailPost = (...args: Parameters<InstanceType<typeof SystemJob>['systemJobUpdateJobDetailPost']>) => getAPI(SystemJob).systemJobUpdateJobDetailPost(...args);
export const systemJobUpdateJobTriggerPost = (...args: Parameters<InstanceType<typeof SystemJob>['systemJobUpdateJobTriggerPost']>) => getAPI(SystemJob).systemJobUpdateJobTriggerPost(...args);
