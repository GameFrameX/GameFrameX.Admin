/* eslint-disable */
/* tslint:disable */

/** @format int32 */
export enum MenuTypeEnum {
  /** 目录 */
  Dir = 1,
  /** 菜单 */
  Menu = 2,
  /** 按钮 */
  Button = 3,
}

/** @format int32 */
export enum MenuAppScopeEnum {
  /** 租户 */
  Tenant = 0,
  /** 平台 */
  Platform = 1,
  /** 应用 */
  App = 2,
}

/**
 * 新增/编辑菜单输入参数。
 * 显式声明前端表单可填字段，不再继承 GameFrameX.Entity.Global.GlobalMenuEntity，避免种子安全元数据（SuperAdminOnly、SyncRoles、AllowTenantWhenSuperAdminOnly、Key）与子节点导航属性被前端批量赋值。
 */
export interface AddMenuRequest {
  /** 主键Id（新增为 0，编辑由前端回填） */
  id?: string;
  /** 乐观锁版本号 */
  rowVersion?: string | null;
  /** 父Id */
  pid?: string;
  type?: MenuTypeEnum;
  /** 业务键（按钮类型用于唯一性校验；管理端新增可为空） */
  key?: string | null;
  /**
   * 菜单名称
   * @minLength 1
   */
  title: string;
  /** 菜单名称国际化Key */
  titleCode?: string | null;
  /** 路由名称 */
  name?: string | null;
  /** 路由地址 */
  path?: string | null;
  /** 组件路径 */
  component?: string | null;
  /** 重定向 */
  redirect?: string | null;
  /** 权限标识 */
  permission?: string | null;
  /** 图标 */
  icon?: string | null;
  /** 外链链接 */
  outLink?: string | null;
  /**
   * 排序
   * @format int32
   */
  orderNo?: number;
  /** 是否隐藏 */
  isHide?: boolean;
  /** 是否缓存 */
  isKeepAlive?: boolean;
  /** 是否固定 */
  isAffix?: boolean;
  /** 是否内嵌 */
  isIframe?: boolean;
  /** 是否启用 */
  isEnabled?: boolean | null;
  /** 备注 */
  remark?: string | null;
}

export interface DeleteMenuRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
}

/**
 * 请求期端点权限条目（U6/C42 缓存值）：由 `ApiEndpointCatalogSync` 在启动时构建并预热，
 * JwtHandler 请求期按 RoutePath 查得，做端点→绑定按钮→已授权 MenuId 反向鉴权。
 */
export interface EndpointPermissionEntry {
  /** 端点代理 Id（稳定身份）。 */
  endpointId?: string;
  /** HTTP Method（对账键的一部分）。 */
  httpMethod?: string | null;
  /** 规范化路由（对账键，非授权令牌）。 */
  routePath?: string | null;
  /** 是否公开（[AllowAnonymous]）—— 请求期短路放行。 */
  isPublic?: boolean;
  /** 是否仅需登录态（白名单）—— 请求期短路放行。 */
  isAuthenticatedOnly?: boolean;
  /**
   * 该端点绑定的按钮 MenuId 集（global_menu_endpoint）。OR 语义。
   * @uniqueItems true
   */
  boundMenuIds?: number[] | null;
}

/** 系统菜单输出参数 */
export interface GlobalMenuResponse {
  /** Id */
  id?: string;
  /** 父Id */
  pid?: string;
  type?: MenuTypeEnum;
  /** 业务键（种子菜单稳定身份，主键 Id 由其派生；管理端新增的菜单可为空） */
  key?: string | null;
  /** 路由名称 */
  name?: string | null;
  /** 路由地址 */
  path?: string | null;
  /** 组件路径 */
  component?: string | null;
  /** 重定向 */
  redirect?: string | null;
  /** 权限标识（按钮绑定的 API 路由名 routeName，供端点目录引导 global_menu_endpoint 绑定） */
  permission?: string | null;
  /** 菜单名称 */
  title?: string | null;
  /** 菜单名称国际化Key */
  titleCode?: string | null;
  /** 图标 */
  icon?: string | null;
  /** 是否内嵌 */
  isIframe?: boolean;
  /** 外链链接 */
  outLink?: string | null;
  /** 是否隐藏 */
  isHide?: boolean;
  /** 是否缓存 */
  isKeepAlive?: boolean;
  /** 是否固定 */
  isAffix?: boolean;
  /**
   * 排序
   * @format int32
   */
  orderNo?: number;
  /** 备注 */
  remark?: string | null;
  /** 是否仅超管可见（种子元数据，DB 驱动授权读取） */
  superAdminOnly?: boolean;
  /**
   * 租户默认角色同步标志（TenantMenuSyncRole 位枚举整数值）
   * @format int32
   */
  syncRoles?: number;
  /** 超管专属菜单下允许租户只读访问 */
  allowTenantWhenSuperAdminOnly?: boolean;
  appScope?: MenuAppScopeEnum;
  /** 菜单子项 */
  children?: GlobalMenuResponse[] | null;
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

/** 系统菜单返回结果 */
export interface MenuResponse {
  /** Id */
  id?: string;
  /** 业务键（种子菜单稳定身份，前端按 Key 关联） */
  key?: string | null;
  /** 父Id */
  pid?: string;
  type?: MenuTypeEnum;
  /** 名称 */
  name?: string | null;
  /** 路由地址 */
  path?: string | null;
  /** 组件路径 */
  component?: string | null;
  /** 权限标识 */
  permission?: string | null;
  /** 重定向 */
  redirect?: string | null;
  /**
   * 排序
   * @format int32
   */
  orderNo?: number;
  /** 是否启用 */
  isEnabled?: boolean;
  /** 备注 */
  remark?: string | null;
  /** 菜单Meta配置 */
  meta?: SystemMenuMetaResponse;
  /** 菜单子项 */
  children?: MenuResponse[] | null;
}

/** 菜单Meta配置 */
export interface SystemMenuMetaResponse {
  /** 标题 */
  title?: string | null;
  /** 标题国际化Key */
  titleCode?: string | null;
  /** 图标 */
  icon?: string | null;
  /** 是否内嵌 */
  isIframe?: boolean;
  /** 外链链接 */
  isLink?: string | null;
  /** 是否隐藏 */
  isHide?: boolean;
  /** 是否缓存 */
  isKeepAlive?: boolean;
  /** 是否固定 */
  isAffix?: boolean;
  /**
   * 菜单上下文：tenantShell / appWorkspace。
   *
   * tenantShell 是默认菜单（平台域页面同在此上下文，侧边栏保持完整菜单）；appWorkspace 只在当前应用工作台中展示。
   */
  menuContext?: string | null;
  /**
   * 菜单应用作用域：platform / tenant / app。
   *
   * platform 只允许 SuperAdmin 平台能力；tenant 只在租户 Shell 下可见；app 必须绑定具体 AppId > 0。
   */
  appScope?: string | null;
}

export interface UpdateMenuRequest {
  /** 主键Id（新增为 0，编辑由前端回填） */
  id?: string;
  /** 乐观锁版本号 */
  rowVersion?: string | null;
  /** 父Id */
  pid?: string;
  type?: MenuTypeEnum;
  /** 业务键（按钮类型用于唯一性校验；管理端新增可为空） */
  key?: string | null;
  /**
   * 菜单名称
   * @minLength 1
   */
  title: string;
  /** 菜单名称国际化Key */
  titleCode?: string | null;
  /** 路由名称 */
  name?: string | null;
  /** 路由地址 */
  path?: string | null;
  /** 组件路径 */
  component?: string | null;
  /** 重定向 */
  redirect?: string | null;
  /** 权限标识 */
  permission?: string | null;
  /** 图标 */
  icon?: string | null;
  /** 外链链接 */
  outLink?: string | null;
  /**
   * 排序
   * @format int32
   */
  orderNo?: number;
  /** 是否隐藏 */
  isHide?: boolean;
  /** 是否缓存 */
  isKeepAlive?: boolean;
  /** 是否固定 */
  isAffix?: boolean;
  /** 是否内嵌 */
  isIframe?: boolean;
  /** 是否启用 */
  isEnabled?: boolean | null;
  /** 备注 */
  remark?: string | null;
}

export type GlobalMenuAddPostData = any;

export type GlobalMenuDeletePostData = any;

export interface GlobalMenuEndpointPermissionMapAsyncGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: Record<string, EndpointPermissionEntry>;
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

export interface GlobalMenuListGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: GlobalMenuResponse[];
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

export interface GlobalMenuLoginMenuTreeGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: MenuResponse[];
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

export interface GlobalMenuOwnButtonPermListGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: string[];
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

export interface GlobalMenuOwnMenuIdListGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: number[];
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

export type GlobalMenuUpdatePostData = any;
