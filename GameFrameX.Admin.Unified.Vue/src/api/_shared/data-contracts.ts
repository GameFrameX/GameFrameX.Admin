/* eslint-disable */
/* tslint:disable */

// 多 tag 共享类型 + barrel（re-export 各 tag 独占类型，保持统一入口；业务可按需从 <tag>/types 精确 import）

/** @format int32 */
export enum PenaltyTargetTypeEnum {
  /** 账号 */
  Account = 1,
  /** 角色 */
  Role = 2,
  /** 设备 */
  Device = 3,
  /** IP */
  Ip = 4,
}

/** @format int32 */
export enum OnlinePenaltyCommandKind {
  /** 踢下线 */
  Kick = 1,
  /** 封禁 */
  Ban = 2,
  /** 禁言 */
  Mute = 3,
}

/** @format int32 */
export enum LogLevel {
  /** Trace */
  Trace = 0,
  /** Debug */
  Debug = 1,
  /** Information */
  Information = 2,
  /** Warning */
  Warning = 3,
  /** Error */
  Error = 4,
  /** Critical */
  Critical = 5,
  /** None */
  None = 6,
}

/** @format int32 */
export enum LiveOpsSourceKind {
  /** 远程配置 */
  RemoteConfig = 1,
  /** 设备分组 */
  DeviceGroup = 2,
  /** 定时任务 */
  ScheduledTask = 3,
  /** 玩家公告 */
  Announcement = 4,
  /** 邮件 Campaign */
  MailCampaign = 5,
}

/** @format int32 */
export enum GameMailType {
  /** 个人邮件 */
  PersonalMail = 1,
  /** 系统邮件 */
  SystemMail = 2,
  /** 公告邮件 */
  AnnouncementMail = 3,
  /** 礼包邮件 */
  GiftMail = 4,
  /** 任务邮件 */
  QuestMail = 5,
  /** GM邮件 */
  GMMail = 6,
  /** 交易邮件 */
  TradeMail = 7,
}

/** @format int32 */
export enum GameMailStatus {
  /** 草稿 */
  Draft = 0,
  /** 已发送 */
  Sent = 1,
  /** 已阅读 */
  Read = 2,
  /** 已领取附件 */
  AttachmentClaimed = 3,
  /** 已删除 */
  Deleted = 4,
  /** 已过期 */
  Expired = 5,
  /** 无效 */
  Invalid = 6,
}

/** @format int32 */
export enum ChannelTypeEnum {
  /** 官方 */
  Official = 1,
  /** 商店 */
  Store = 2,
  /** 广告 */
  Ad = 3,
  /** 自定义 */
  Custom = 4,
}

/** @format int32 */
export enum ChannelScopeEnum {
  /** 系统 */
  System = 1,
  /** 租户 */
  Tenant = 2,
}

/** @format int32 */
export enum AccountTypeEnum {
  /** 会员 */
  Member = 666,
  /** 普通账号 */
  NormalUser = 777,
  /** 系统管理员 */
  SystemAdmin = 888,
  /** 超级管理员 */
  SuperAdmin = 999,
}

/** 渠道增加输入参数 */
export interface AddChannelDefinitionRequest {
  description?: string | null;
  createdName?: string | null;
  updatedName?: string | null;
  /** @minLength 1 */
  name: string;
  isDefault?: boolean;
  /**
   * 渠道编码
   * @minLength 1
   * @maxLength 256
   */
  code: string;
  scope?: ChannelScopeEnum;
  /**
   * 适用平台编码
   * @maxLength 256
   */
  platformCode?: string | null;
  channelType?: ChannelTypeEnum;
  /** 系统渠道是否锁定 */
  isSystemLocked?: boolean;
  /** 是否启用 */
  isEnabled?: boolean | null;
}

export interface BaseDeleteRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
  /**
   * @min 0
   * @max 9223372036854776000
   */
  rowVersion: string;
}

export interface BaseIdRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
}

export interface BaseSelectPageRequest {
  searchKey?: string | null;
  /**
   * @format int32
   * @min 1
   * @max 1000
   */
  pageSize?: number;
}

export interface BaseSelectPageResponse {
  id?: string;
  value?: string | null;
  label?: string | null;
}

export interface BaseSelectPageStringResponse {
  id?: string | null;
  value?: string | null;
  label?: string | null;
}

export interface BaseUpdateRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
  /**
   * @min 0
   * @max 9223372036854776000
   */
  rowVersion: string;
}

/** 渠道输出参数 */
export interface ChannelDefinitionResponse {
  /** Id */
  id?: string;
  /** 稳定业务编码 */
  code?: string | null;
  /** 名称 */
  name?: string | null;
  scope?: ChannelScopeEnum;
  /** 适用平台编码 */
  platformCode?: string | null;
  channelType?: ChannelTypeEnum;
  /** 系统渠道是否锁定 */
  isSystemLocked?: boolean;
  /** 详细描述 */
  description?: string | null;
  /** 是否为默认 */
  isDefault?: boolean | null;
  /** 是否启用 */
  isEnabled?: boolean | null;
  /**
   * 创建时间
   * @format int64
   */
  createdTime?: number;
  /** 创建人 */
  createdName?: string | null;
  /**
   * 更新时间
   * @format int64
   */
  updateTime?: number | null;
  /** 更新人 */
  updatedName?: string | null;
  /** 行版本号（乐观锁） */
  rowVersion?: string | null;
}

export interface ChannelDefinitionResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: ChannelDefinitionResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

export interface GlobalDictionaryDataEntity {
  /** 主键Id */
  id: string;
  /** 创建人Id */
  createdId?: string | null;
  /**
   * 创建时间
   * @format int64
   */
  createdTime?: number;
  /**
   * 创建人姓名
   * @maxLength 512
   */
  createdName?: string | null;
  /**
   * 更新次数
   * @format int32
   */
  updateCount?: number | null;
  /**
   * 更新时间
   * @format int64
   */
  updateTime?: number | null;
  /** 更新人Id */
  updatedId?: string | null;
  /**
   * 更新人姓名
   * @maxLength 512
   */
  updatedName?: string | null;
  /** 软删除标记,true:删除,false:未删除,null:未设置(未删除) */
  isDeleted?: boolean | null;
  /**
   * 删除时间
   * @format int64
   */
  deleteTime?: number | null;
  /** 删除人Id */
  deletedId?: string | null;
  /**
   * 删除人姓名
   * @maxLength 512
   */
  deletedName?: string | null;
  /** 行版本号（用于乐观锁） */
  rowVersion?: string | null;
  /** 是否启用,true:启用，false:禁用，null:未设置(启用) */
  isEnabled?: boolean | null;
  dictionaryTypeId?: string;
  dictionaryType?: GlobalDictionaryTypeEntity;
  value?: string | null;
  code?: string | null;
  tagType?: string | null;
  styleSetting?: string | null;
  classSetting?: string | null;
  /** @format int32 */
  orderNo?: number;
  remark?: string | null;
  extData?: string | null;
}

/** 系统字典值输出参数 */
export interface GlobalDictionaryDataResponse {
  /** Id */
  id?: string;
  /** 字典类型Id */
  dictionaryTypeId?: string;
  /** 值 */
  value?: string | null;
  /** 编码 */
  code?: string | null;
  /** 显示样式-标签颜色 */
  tagType?: string | null;
  /** 显示样式-Style(控制显示样式) */
  styleSetting?: string | null;
  /** 显示样式-Class(控制显示样式) */
  classSetting?: string | null;
  /**
   * 排序
   * @format int32
   */
  orderNo?: number;
  /** 备注 */
  remark?: string | null;
  /** 拓展数据(保存业务功能的配置项) */
  extData?: string | null;
  /** 是否启用 */
  isEnabled?: boolean | null;
  /**
   * 创建时间
   * @format int64
   */
  createdTime?: number;
  /** 创建人 */
  createdName?: string | null;
  /**
   * 更新时间
   * @format int64
   */
  updateTime?: number | null;
  /** 更新人 */
  updatedName?: string | null;
  /** 行版本号（乐观锁） */
  rowVersion?: string | null;
}

export interface GlobalDictionaryTypeEntity {
  /** 主键Id */
  id: string;
  /** 创建人Id */
  createdId?: string | null;
  /**
   * 创建时间
   * @format int64
   */
  createdTime?: number;
  /**
   * 创建人姓名
   * @maxLength 512
   */
  createdName?: string | null;
  /**
   * 更新次数
   * @format int32
   */
  updateCount?: number | null;
  /**
   * 更新时间
   * @format int64
   */
  updateTime?: number | null;
  /** 更新人Id */
  updatedId?: string | null;
  /**
   * 更新人姓名
   * @maxLength 512
   */
  updatedName?: string | null;
  /** 软删除标记,true:删除,false:未删除,null:未设置(未删除) */
  isDeleted?: boolean | null;
  /**
   * 删除时间
   * @format int64
   */
  deleteTime?: number | null;
  /** 删除人Id */
  deletedId?: string | null;
  /**
   * 删除人姓名
   * @maxLength 512
   */
  deletedName?: string | null;
  /** 行版本号（用于乐观锁） */
  rowVersion?: string | null;
  /** 是否启用,true:启用，false:禁用，null:未设置(启用) */
  isEnabled?: boolean | null;
  name?: string | null;
  code?: string | null;
  /** @format int32 */
  orderNo?: number;
  remark?: string | null;
  children?: GlobalDictionaryDataEntity[] | null;
}

export interface LogOperationFields {
  /** 主键Id */
  id: string;
  /** 创建人Id */
  createdId?: string | null;
  /**
   * 创建时间
   * @format int64
   */
  createdTime?: number;
  /**
   * 创建人姓名
   * @maxLength 512
   */
  createdName?: string | null;
  /**
   * 更新次数
   * @format int32
   */
  updateCount?: number | null;
  /**
   * 更新时间
   * @format int64
   */
  updateTime?: number | null;
  /** 更新人Id */
  updatedId?: string | null;
  /**
   * 更新人姓名
   * @maxLength 512
   */
  updatedName?: string | null;
  /** 软删除标记,true:删除,false:未删除,null:未设置(未删除) */
  isDeleted?: boolean | null;
  /**
   * 删除时间
   * @format int64
   */
  deleteTime?: number | null;
  /** 删除人Id */
  deletedId?: string | null;
  /**
   * 删除人姓名
   * @maxLength 512
   */
  deletedName?: string | null;
  /** 行版本号（用于乐观锁） */
  rowVersion?: string | null;
  /** 是否启用,true:启用，false:禁用，null:未设置(启用) */
  isEnabled?: boolean | null;
  tenantId?: string | null;
  controllerName?: string | null;
  actionName?: string | null;
  displayTitle?: string | null;
  status?: string | null;
  remoteIp?: string | null;
  location?: string | null;
  /** @format double */
  longitude?: number | null;
  /** @format double */
  latitude?: number | null;
  browser?: string | null;
  os?: string | null;
  /** @format int64 */
  elapsed?: number | null;
  /** @format int64 */
  logDateTime?: number | null;
  logLevel?: LogLevel;
  account?: string | null;
  realName?: string | null;
  httpMethod?: string | null;
  requestUrl?: string | null;
  requestParam?: string | null;
  returnResult?: string | null;
  /** @format int32 */
  eventId?: number | null;
  /** @format int32 */
  threadId?: number | null;
  traceId?: string | null;
  exception?: string | null;
  message?: string | null;
}

export interface LogOperationFieldsSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: LogOperationFields[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

export interface LogRequest {
  /**
   * 开始时间
   * @format int64
   */
  startTime?: number | null;
  /**
   * 结束时间
   * @format int64
   */
  endTime?: number | null;
}

/**
 * LiveOps 统一下发回执（来源 change C343 / vault:C8 S7.7）。
 *
 * 透传 Online 确认版本 / 状态供前端回显；`Merged` 标记邮件来源「同目标合并返回首次回执」路径。
 */
export interface OnlineLiveOpsDispatchResponse {
  source?: LiveOpsSourceKind;
  /**
   * 来源域业务主键（回显请求值）。
   * @format int64
   */
  businessKey?: number;
  /**
   * 实际路由到的下发通道标识：四类配置来源为 Online admin API 命令名（如 `publishRemoteConfig`）；
   * 邮件来源为既有链路标识 `publish_mail_campaign`（MailCampaignServerClient / Fulfillment）。
   */
  dispatchChannel?: string | null;
  /** Online / Server 确认版本（配置生效版本或 Campaign 发布版本；空 = 链路未返回版本回执）。 */
  onlineConfirmVersion?: string | null;
  /** 是否同目标合并（仅邮件来源：目标 Campaign 已发布，未重复下发，直接返回首次发布回执）。 */
  merged?: boolean;
  /** 回执消息（Online MessageKey / 链路消息原文，排查辅助）。 */
  message?: string | null;
  /**
   * 下发时间（Unix 秒，UTC）。
   * @format int64
   */
  dispatchedTime?: number;
}

/**
 * 既有运营域页面「经 Online 下发」动作的通用输入（来源 change C343 / vault:C8 S7.7）。
 *
 * 来源类型由所在 Service 固定注入（远程配置 / 公告 / 邮件 Campaign），不 exposed 给前端选择；
 * 本输入只承载目标区服、可选定向分群与下发理由，内部统一转调 OnlineLiveOpsDispatchService 单一入口。
 */
export interface OnlineLiveOpsSourceDispatchRequest {
  /**
   * 来源域业务主键（所在 Service 的实体 Id，由页面行上下文提供，服务端再校验存在性）。
   * @format int64
   * @min 1
   * @max 9223372036854776000
   */
  businessKey: number;
  /**
   * 目标区服Id（Online 命令按区服路由到对应 Runtime）。
   * @min 1
   * @max 9223372036854776000
   */
  serverId: string;
  /**
   * 可选定向分群Id（下发只对分群内玩家生效）。
   * @min 1
   * @max 9223372036854776000
   */
  segmentId?: string | null;
  /**
   * 下发理由（必填，随 MonitorLogging 全链路审计）。
   * @minLength 1
   */
  reason: string;
}

/** 处罚联动命令聚合响应（按联动目标区服集逐区服执行后的聚合结果）。 */
export interface OnlinePenaltyLinkResponse {
  /** 处罚事实源行 Id。 */
  penaltyId?: string;
  command?: OnlinePenaltyCommandKind;
  /** 各区服执行结果（全区服处罚 = App 下每个区服一条）。 */
  results?: OnlinePenaltyLinkResultResponse[] | null;
  /** 是否全部区服成功（false 时存在可重试目标；重试幂等依据 IdempotencyKey 命中 Online 首次结果）。 */
  allSucceeded?: boolean;
}

/** 单区服联动命令执行结果（VC-2.3 Admin 展示半边：旧 Token 吊销确认字段）。 */
export interface OnlinePenaltyLinkResultResponse {
  /** 目标区服 Id。 */
  serverId?: string;
  /** 该区服命令是否成功。 */
  success?: boolean;
  /**
   * Online 原始协议码（失败时透出，供按数字码排查；成功时为 null）。
   * @format int32
   */
  onlineCode?: number | null;
  /** 失败原因摘要（成功时为 null）。 */
  failureMessage?: string | null;
  /**
   * 旧 Token 吊销确认（vault:C2 S1.4：命令生效后 Online 确认已吊销该玩家旧 Token；
   * 服务端旧 Token 被拒的验证归 vault 侧用例，Admin 断言展示层确认字段）。
   */
  tokenRevokedConfirmed?: boolean;
}

/** 渠道分页查询输入参数 */
export interface PageChannelDefinitionRequest {
  searchKey?: string | null;
  /**
   * @format int32
   * @min 1
   * @max 2147483647
   */
  page?: number;
  /**
   * @format int32
   * @min 1
   * @max 100
   */
  pageSize?: number;
  orderBy?: string | null;
  isDescending?: boolean;
  /** 渠道编码 */
  code?: string | null;
  scope?: ChannelScopeEnum;
  /** 适用平台编码 */
  platformCode?: string | null;
  channelType?: ChannelTypeEnum;
  /** 是否启用 */
  isEnabled?: boolean | null;
}

export interface PageLogRequest {
  /**
   * @format int32
   * @min 1
   * @max 2147483647
   */
  page?: number;
  /**
   * @format int32
   * @min 1
   * @max 100
   */
  pageSize?: number;
  orderBy?: string | null;
  isDescending?: boolean;
  /**
   * 关键字查询（不区分大小写，按各日志服务声明的 `SearchableTextColumns` 多列 OR 模糊匹配）。
   * 与时间范围条件 `AND` 叠加；为空/null 时行为与现状完全一致（不过滤）。
   * 继承自 GameFrameX.Core.BaseInputOutput.BasePageRequest，此处显式列出便于契约可读。
   */
  searchKey?: string | null;
  /**
   * 开始时间
   * @format int64
   */
  startTime?: number | null;
  /**
   * 结束时间
   * @format int64
   */
  endTime?: number | null;
}

/** 渠道更新输入参数 */
export interface UpdateChannelDefinitionRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
  /**
   * @min 0
   * @max 9223372036854776000
   */
  rowVersion: string;
  name?: string | null;
  description?: string | null;
  isDefault?: boolean;
  /**
   * 渠道编码
   * @minLength 1
   * @maxLength 256
   */
  code: string;
  scope?: ChannelScopeEnum;
  /**
   * 适用平台编码
   * @maxLength 256
   */
  platformCode?: string | null;
  channelType?: ChannelTypeEnum;
  /** 系统渠道是否锁定 */
  isSystemLocked?: boolean;
  /** 是否启用 */
  isEnabled?: boolean | null;
}

export * from "../advertisementCallbackConfig/types";
export * from "../appSignatureKey/types";
export * from "../channelDefinition/types";
export * from "../clientApiPolicy/types";
export * from "../deviceGroup/types";
export * from "../gameAdvertisementReward/types";
export * from "../gameAnnouncement/types";
export * from "../gameAntiCheatEvent/types";
export * from "../gameAppAssetPackage/types";
export * from "../gameAppAssetPackageVersion/types";
export * from "../gameAppAssetPackageVersionHistory/types";
export * from "../gameAppDevice/types";
export * from "../gameAppFeatureSwitch/types";
export * from "../gameAppOpenAccess/types";
export * from "../gameAppPackage/types";
export * from "../gameAppReleaseVersion/types";
export * from "../gameAppRemoteConfig/types";
export * from "../gameAppRemoteConfigKey/types";
export * from "../gameAppRemoteConfigMode/types";
export * from "../gameAppServerConfig/types";
export * from "../gameAppVersionDefinition/types";
export * from "../gameArea/types";
export * from "../gameAreaClassify/types";
export * from "../gameBannerAnnouncement/types";
export * from "../gameCommodity/types";
export * from "../gameDockerTag/types";
export * from "../gameGoods/types";
export * from "../gameGoodsCatalogue/types";
export * from "../gameHome/types";
export * from "../gameLoginTicketKey/types";
export * from "../gameMail/types";
export * from "../gameMailCampaign/types";
export * from "../gameMailTemplate/types";
export * from "../gameMailTemplateCategory/types";
export * from "../gameOnlineConfigRollout/types";
export * from "../gameOnlinePlayerSegment/types";
export * from "../gameOrder/types";
export * from "../gameOrderFulfillment/types";
export * from "../gamePaymentConfig/types";
export * from "../gamePenalty/types";
export * from "../gamePenaltyAppeal/types";
export * from "../gameRedeemCode/types";
export * from "../gameRedeemCodeChannel/types";
export * from "../gameRedeemCodeUseRecord/types";
export * from "../gameReport/types";
export * from "../gameRewardTemplate/types";
export * from "../gameRewardTemplateGroup/types";
export * from "../gameRole/types";
export * from "../gameTimeJob/types";
export * from "../gameUser/types";
export * from "../gameWebShop/types";
export * from "../gameWebShopOrder/types";
export * from "../globalChannelDefinition/types";
export * from "../globalDictionaryData/types";
export * from "../globalDictionaryType/types";
export * from "../globalLanguage/types";
export * from "../globalMenu/types";
export * from "../globalPlatform/types";
export * from "../logDiff/types";
export * from "../logException/types";
export * from "../logOperation/types";
export * from "../logVisit/types";
export * from "../objectStorageConfig/types";
export * from "../onlineAssetAuditQuery/types";
export * from "../onlineAssetGrantCommand/types";
export * from "../onlineAssetLedgerQuery/types";
export * from "../onlineAssetTransactionQuery/types";
export * from "../onlineAuditSearch/types";
export * from "../onlineControlledCommand/types";
export * from "../onlineLiveOpsDispatch/types";
export * from "../onlineMatchQueueQuery/types";
export * from "../onlineMatchTicketQuery/types";
export * from "../onlineMessageAuditQuery/types";
export * from "../onlineOverviewQuery/types";
export * from "../onlinePenaltyLinkCommand/types";
export * from "../onlinePlayerTimelineQuery/types";
export * from "../onlineReportCase/types";
export * from "../platformCaptchaConfig/types";
export * from "../platformJwtConfig/types";
export * from "../platformLoggingConfig/types";
export * from "../platformPasswordPolicyConfig/types";
export * from "../platformRateLimitConfig/types";
export * from "../sessionKey/types";
export * from "../systemAuth/types";
export * from "../systemCache/types";
export * from "../systemConfig/types";
export * from "../systemConst/types";
export * from "../systemDatabase/types";
export * from "../systemEmailProvider/types";
export * from "../systemEnum/types";
export * from "../systemJob/types";
export * from "../systemMessage/types";
export * from "../systemOpenAccess/types";
export * from "../tenantFile/types";
export * from "../tenantRole/types";
export * from "../tenantUser/types";
