/* eslint-disable */
/* tslint:disable */

import type { BaseSelectPageResponse, BaseSelectPageStringResponse } from "../_shared/data-contracts";

/** @format int32 */
export enum GameServerStatus {
  /** 未知 */
  Unknown = 0,
  /** 在线 */
  Online = 1,
  /** 离线 */
  Offline = 2,
  /** 维护中 */
  Maintenance = 3,
  /** 满员 */
  Full = 4,
}

/** @format int32 */
export enum GameLanguage {
  /** 未指定 */
  Unspecified = 0,
  /** 中文 */
  Chinese = 1,
  /** 简体中文 */
  ChineseSimplified = 2,
  /** 繁体中文 */
  ChineseTraditional = 3,
  /** 英语 */
  English = 4,
  /** 南非荷兰语 */
  Afrikaans = 5,
  /** 阿尔巴尼亚语 */
  Albanian = 6,
  /** 阿拉伯语 */
  Arabic = 7,
  /** 巴斯克语 */
  Basque = 8,
  /** 白俄罗斯语 */
  Belarusian = 9,
  /** 保加利亚语 */
  Bulgarian = 10,
  /** 加泰罗尼亚语 */
  Catalan = 11,
  /** 克罗地亚语 */
  Croatian = 12,
  /** 捷克语 */
  Czech = 13,
  /** 丹麦语 */
  Danish = 14,
  /** 荷兰语 */
  Dutch = 15,
  /** 爱沙尼亚语 */
  Estonian = 16,
  /** 法罗语 */
  Faroese = 17,
  /** 芬兰语 */
  Finnish = 18,
  /** 法语 */
  French = 19,
  /** 格鲁吉亚语 */
  Georgian = 20,
  /** 德语 */
  German = 21,
  /** 希腊语 */
  Greek = 22,
  /** 希伯来语 */
  Hebrew = 23,
  /** 匈牙利语 */
  Hungarian = 24,
  /** 冰岛语 */
  Icelandic = 25,
  /** 印尼语 */
  Indonesian = 26,
  /** 意大利语 */
  Italian = 27,
  /** 日语 */
  Japanese = 28,
  /** 韩语 */
  Korean = 29,
  /** 拉脱维亚语 */
  Latvian = 30,
  /** 立陶宛语 */
  Lithuanian = 31,
  /** 马其顿语 */
  Macedonian = 32,
  /** 马拉雅拉姆语 */
  Malayalam = 33,
  /** 挪威语 */
  Norwegian = 34,
  /** 波斯语 */
  Persian = 35,
  /** 波兰语 */
  Polish = 36,
  /** 巴西葡萄牙语 */
  PortugueseBrazil = 37,
  /** 葡萄牙语 */
  PortuguesePortugal = 38,
  /** 罗马尼亚语 */
  Romanian = 39,
  /** 俄语 */
  Russian = 40,
  /** 塞尔维亚克罗地亚语 */
  SerboCroatian = 41,
  /** 塞尔维亚西里尔语 */
  SerbianCyrillic = 42,
  /** 塞尔维亚拉丁语 */
  SerbianLatin = 43,
  /** 斯洛伐克语 */
  Slovak = 44,
  /** 斯洛文尼亚语 */
  Slovenian = 45,
  /** 西班牙语 */
  Spanish = 46,
  /** 瑞典语 */
  Swedish = 47,
  /** 泰语 */
  Thai = 48,
  /** 土耳其语 */
  Turkish = 49,
  /** 乌克兰语 */
  Ukrainian = 50,
  /** 越南语 */
  Vietnamese = 51,
}

/** 游戏区服增加输入参数 */
export interface AddTenantGameAreaEntityRequest {
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
  /**
   * 名称
   * @minLength 1
   */
  name: string;
  /**
   * 详细描述
   * @minLength 1
   */
  description: string;
  /** 本地化Key */
  localizationKey?: string | null;
  /** 是否是默认 */
  isDefault: boolean;
  /** 是否推荐 */
  isRecommend: boolean;
  /** 大区分类Id */
  classifyId: string;
  /**
   * 游戏应用Id。区服主归属，新增必填且必须属于当前租户；
   * 更新时由服务层从既有实体回填（不可变），防止区服跨应用迁移。
   */
  appId: string;
  /**
   * Http 管理连接地址
   * @minLength 1
   */
  httpManageUrl: string;
  /**
   * TcpSocket 连接地址
   * @minLength 1
   */
  tcpSocketUrl: string;
  /**
   * WebSocket连接地址
   * @minLength 1
   */
  webSocketUrl: string;
  /**
   * 最大玩家数量
   * @format int32
   */
  capacity: number;
  /**
   * 启用的日期
   * @format int64
   */
  creationTime: number;
  /**
   * 服务器最后一次维护或更新的日期
   * @format int64
   */
  lastMaintenanceTime: number;
  /**
   * 游戏版本号
   * @minLength 1
   */
  serverVersion: string;
  /** 服务器的 Docker 容器 ID */
  serverContainerId?: string | null;
  /**
   * 服务器的游戏模式类型
   * @minLength 1
   */
  serverType: string;
  language: GameLanguage;
  serverStatus: GameServerStatus;
  /** 客户端版本 */
  appVersion?: string | null;
  /** 指定可以加入此服务器的玩家类型 */
  allowedPlayerProfile?: string | null;
  /** 特定服务器举行的事件或活动信息 */
  serverEvents?: string | null;
  /** 访问权限设置，控制哪些玩家可以访问服务器 */
  accessControls?: string | null;
  /** 运行参数。此前 BaseRequest 缺此声明，Adapt 无来源导致实体 NOT NULL 列恒 23502。 */
  serverParams?: string | null;
  /**
   * 客户端转发 Server HTTP 路由前缀（来源 change C150 §3.2 / C175）。
   *
   * Client API 代理玩家普通请求（建角、改名、查角色、创建订单等）的 Server HTTP 路由前缀，默认 `game/api`。
   */
  clientDispatchApiPrefix?: string | null;
  /**
   * 履约投递 Server HTTP 路由前缀（来源 change C150 §3.2 / C175）。
   *
   * 后台履约、发货、退款、邮件投递、广告奖励、Campaign 发布、后台运营操作等资产或运营投递的前缀，默认 `game/api`。
   */
  fulfillmentApiPrefix?: string | null;
  /** GM 命令 Server HTTP 路由前缀（来源 change C150 §3.2 / C175）。默认空，空表示 Admin 不构造 GM URL。 */
  gmApiPrefix?: string | null;
  /** 管理控制 Server HTTP 路由前缀（来源 change C150 §3.2 / C175）。默认空，空表示 Admin 不构造控制 URL。 */
  controlApiPrefix?: string | null;
  /** 是否允许客户端调试 GM 通道（来源 change C150 §3.2 / C175）。仅 Admin 配置标记，最终放行由 Game Server 启动参数与环境门禁决定，默认关闭。 */
  enableClientGmApi?: boolean;
  /** 是否启用 */
  isEnabled: boolean;
}

/** 游戏区服分页查询输入参数 */
export interface PageTenantGameAreaEntityRequest {
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
  /** 名称 */
  name?: string | null;
  /** 游戏应用Id（可选过滤） */
  appId?: string | null;
  /** 是否是默认 */
  isDefault?: boolean | null;
  /** 是否推荐 */
  isRecommend?: boolean | null;
  /** 是否启用 */
  isEnabled?: boolean | null;
  language?: GameLanguage;
}

/** 游戏区服输出参数 */
export interface TenantGameAreaEntityResponse {
  /** Id */
  id?: string;
  /** 名称 */
  name?: string | null;
  /** 详细描述 */
  description?: string | null;
  /** 是否是默认 */
  isDefault?: boolean;
  /** 是否推荐 */
  isRecommend?: boolean;
  /** 大区分类Id */
  classifyId?: string;
  serverStatus?: GameServerStatus;
  /** Http 管理连接地址 */
  httpManageUrl?: string | null;
  /** TcpSocket 连接地址 */
  tcpSocketUrl?: string | null;
  /** WebSocket连接地址 */
  webSocketUrl?: string | null;
  /**
   * 服务器能够承载的最大玩家数量。
   * @format int32
   */
  capacity?: number;
  /**
   * 目前服务器上的玩家数量。
   * @format int32
   */
  currentLoad?: number | null;
  /**
   * 服务器启用的日期。
   * @format int64
   */
  creationTime?: number;
  /**
   * 服务器最后一次维护或更新的日期。
   * @format int64
   */
  lastMaintenanceTime?: number | null;
  /** 服务器运行的游戏版本号。 */
  serverVersion?: string | null;
  /** 服务器的游戏模式类型，如：Normal, Newbie, Elite, PvP, PvE。 */
  serverType?: string | null;
  /** 本地化Key */
  localizationKey?: string | null;
  language?: GameLanguage;
  /** 指定可以加入此服务器的玩家类型，如：NewPlayer, VIP。 */
  allowedPlayerProfile?: string | null;
  /** 特定服务器举行的事件或活动信息。 */
  serverEvents?: string | null;
  /** 访问权限设置，控制哪些玩家可以访问服务器。 */
  accessControls?: string | null;
  /** 服务器的 Docker 容器 ID */
  serverContainerId?: string | null;
  /** 游戏应用Id。区服主归属 = TenantId + GameAppId，每条区服必属某个具体游戏应用。 */
  gameAppId?: string;
  /** 渠道id */
  channelId?: string | null;
  /** 平台id */
  platformId?: string | null;
  /** 包id。可选访问策略。 */
  packageId?: string | null;
  /** 运行参数 */
  serverParams?: string | null;
  /** 客户端版本 */
  appVersion?: string | null;
  /** 客户端转发 Server HTTP 路由前缀（来源 change C150 §3.2 / C175）。默认 game/api。 */
  clientDispatchApiPrefix?: string | null;
  /** 履约投递 Server HTTP 路由前缀（来源 change C150 §3.2 / C175）。默认 game/api。 */
  fulfillmentApiPrefix?: string | null;
  /** GM 命令 Server HTTP 路由前缀（来源 change C150 §3.2 / C175）。默认空，空表示不构造 GM URL。 */
  gmApiPrefix?: string | null;
  /** 管理控制 Server HTTP 路由前缀（来源 change C150 §3.2 / C175）。默认空，空表示不构造控制 URL。 */
  controlApiPrefix?: string | null;
  /** 是否允许客户端调试 GM 通道（Admin 配置标记，来源 change C150 §3.2 / C175）。 */
  enableClientGmApi?: boolean;
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
  /** 区服分类名称 */
  classifyIdName?: string | null;
}

export interface TenantGameAreaEntityResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantGameAreaEntityResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 游戏区服更新输入参数 */
export interface UpdateTenantGameAreaEntityRequest {
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
  /**
   * 名称
   * @minLength 1
   */
  name: string;
  /**
   * 详细描述
   * @minLength 1
   */
  description: string;
  /** 本地化Key */
  localizationKey?: string | null;
  /** 是否是默认 */
  isDefault: boolean;
  /** 是否推荐 */
  isRecommend: boolean;
  /** 大区分类Id */
  classifyId: string;
  /**
   * 游戏应用Id。区服主归属，新增必填且必须属于当前租户；
   * 更新时由服务层从既有实体回填（不可变），防止区服跨应用迁移。
   */
  appId: string;
  /**
   * Http 管理连接地址
   * @minLength 1
   */
  httpManageUrl: string;
  /**
   * TcpSocket 连接地址
   * @minLength 1
   */
  tcpSocketUrl: string;
  /**
   * WebSocket连接地址
   * @minLength 1
   */
  webSocketUrl: string;
  /**
   * 最大玩家数量
   * @format int32
   */
  capacity: number;
  /**
   * 启用的日期
   * @format int64
   */
  creationTime: number;
  /**
   * 服务器最后一次维护或更新的日期
   * @format int64
   */
  lastMaintenanceTime: number;
  /**
   * 游戏版本号
   * @minLength 1
   */
  serverVersion: string;
  /** 服务器的 Docker 容器 ID */
  serverContainerId?: string | null;
  /**
   * 服务器的游戏模式类型
   * @minLength 1
   */
  serverType: string;
  language: GameLanguage;
  serverStatus: GameServerStatus;
  /** 客户端版本 */
  appVersion?: string | null;
  /** 指定可以加入此服务器的玩家类型 */
  allowedPlayerProfile?: string | null;
  /** 特定服务器举行的事件或活动信息 */
  serverEvents?: string | null;
  /** 访问权限设置，控制哪些玩家可以访问服务器 */
  accessControls?: string | null;
  /** 运行参数。此前 BaseRequest 缺此声明，Adapt 无来源导致实体 NOT NULL 列恒 23502。 */
  serverParams?: string | null;
  /**
   * 客户端转发 Server HTTP 路由前缀（来源 change C150 §3.2 / C175）。
   *
   * Client API 代理玩家普通请求（建角、改名、查角色、创建订单等）的 Server HTTP 路由前缀，默认 `game/api`。
   */
  clientDispatchApiPrefix?: string | null;
  /**
   * 履约投递 Server HTTP 路由前缀（来源 change C150 §3.2 / C175）。
   *
   * 后台履约、发货、退款、邮件投递、广告奖励、Campaign 发布、后台运营操作等资产或运营投递的前缀，默认 `game/api`。
   */
  fulfillmentApiPrefix?: string | null;
  /** GM 命令 Server HTTP 路由前缀（来源 change C150 §3.2 / C175）。默认空，空表示 Admin 不构造 GM URL。 */
  gmApiPrefix?: string | null;
  /** 管理控制 Server HTTP 路由前缀（来源 change C150 §3.2 / C175）。默认空，空表示 Admin 不构造控制 URL。 */
  controlApiPrefix?: string | null;
  /** 是否允许客户端调试 GM 通道（来源 change C150 §3.2 / C175）。仅 Admin 配置标记，最终放行由 Game Server 启动参数与环境门禁决定，默认关闭。 */
  enableClientGmApi?: boolean;
  /** 是否启用 */
  isEnabled?: boolean;
  /** 渠道id */
  channelId?: string;
  /** 平台id */
  platformId?: string;
  /** 包id */
  packageId?: string;
}

export interface TenantGameAreaEntityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏区服输出参数 */
  data: TenantGameAreaEntityResponse;
  /** 链路追踪标识，未启用追踪时为 null */
  trackId: string | null;
  /** 已退役字段：统一错误码体系（C265）下恒为 null，数字业务错误码承载在 code 字段 */
  errorCode: string | null;
  /** 响应类型（success/warning/error） */
  type: string | null;
  /**
   * Unix 时间戳（秒）
   * @format int64
   */
  time: number;
  extras: object | null;
}

export type TenantGameAreaEntityDeletePostData = any;

export interface TenantGameAreaEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏区服输出参数 */
  data: TenantGameAreaEntityResponse;
  /** 链路追踪标识，未启用追踪时为 null */
  trackId: string | null;
  /** 已退役字段：统一错误码体系（C265）下恒为 null，数字业务错误码承载在 code 字段 */
  errorCode: string | null;
  /** 响应类型（success/warning/error） */
  type: string | null;
  /**
   * Unix 时间戳（秒）
   * @format int64
   */
  time: number;
  extras: object | null;
}

export type TenantGameAreaEntityDisableDeletePostData = any;

export type TenantGameAreaEntityOpenPostData = any;

export interface TenantGameAreaEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGameAreaEntityResponseSqlSugarPagedList;
  /** 链路追踪标识，未启用追踪时为 null */
  trackId: string | null;
  /** 已退役字段：统一错误码体系（C265）下恒为 null，数字业务错误码承载在 code 字段 */
  errorCode: string | null;
  /** 响应类型（success/warning/error） */
  type: string | null;
  /**
   * Unix 时间戳（秒）
   * @format int64
   */
  time: number;
  extras: object | null;
}

export type TenantGameAreaEntityRestartPostData = any;

export interface TenantGameAreaEntitySelectPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: BaseSelectPageResponse[];
  /** 链路追踪标识，未启用追踪时为 null */
  trackId: string | null;
  /** 已退役字段：统一错误码体系（C265）下恒为 null，数字业务错误码承载在 code 字段 */
  errorCode: string | null;
  /** 响应类型（success/warning/error） */
  type: string | null;
  /**
   * Unix 时间戳（秒）
   * @format int64
   */
  time: number;
  extras: object | null;
}

export interface TenantGameAreaEntitySelectStringPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: BaseSelectPageStringResponse[];
  /** 链路追踪标识，未启用追踪时为 null */
  trackId: string | null;
  /** 已退役字段：统一错误码体系（C265）下恒为 null，数字业务错误码承载在 code 字段 */
  errorCode: string | null;
  /** 响应类型（success/warning/error） */
  type: string | null;
  /**
   * Unix 时间戳（秒）
   * @format int64
   */
  time: number;
  extras: object | null;
}

export type TenantGameAreaEntityStopPostData = any;

export type TenantGameAreaEntityUpdatePostData = any;
