import type { ButtonProps } from 'primevue/button'

/** 单个操作配置：纯函数或带 visible 条件的对象 */
export type ActionConfig = ((row: any) => void) | {
  onClick: (row: any) => void
  visible?: (row: any) => boolean
}

/** 内置操作：key 即权限后缀，如 edit → `${authPrefix}:update` */
export interface CrudActions {
  detail?: ActionConfig
  edit?: ActionConfig
  delete?: ActionConfig
}

/** 自定义操作按钮（用于 customActions 数组） */
export interface CrudAction {
  /** 权限码后缀，拼接为 `${authPrefix}:${auth}` */
  auth: string
  /** 按钮文本 */
  label: string
  /** 图标类名（FontAwesome），如 'fa-solid fa-pen-to-square' */
  icon?: string
  /** PrimeVue Button severity */
  severity?: 'secondary' | 'success' | 'info' | 'warn' | 'danger'
  /** 行级条件，返回 false 时该行该按钮隐藏 */
  visible?: (row: any) => boolean
  /** 点击回调 */
  onClick: (row: any) => void
  /** 透传给 PrimeVue Button 的额外 props */
  buttonProps?: Partial<ButtonProps>
}

/**
 * 声明式列配置（CrudDataTable 的 columns prop）。
 *
 * 字段集合严格锁死（见 change C146 §3）：execute 期不得新增未声明字段（如
 * editable / filterable / aggregatable），需新增须先回 §3 取授权或立衍生 change。
 */
export interface CrudColumnConfig {
  /** 列标识，同时是导出字段名，作为列偏好的唯一键 */
  field: string
  /** 列标题（已国际化文案） */
  header: string
  /** 列宽，默认 auto */
  width?: string
  /** 对齐方式，默认 left */
  align?: 'left' | 'center' | 'right'
  /**
   * 数字列标记（金额/ID/数量等）：true 时单元格渲染等宽数字（tabular-nums）
   * 且未显式指定 align 时默认右对齐（C254 数字列规范）
   */
  numeric?: boolean
  /** 是否冻结列，默认 false */
  frozen?: boolean
  /** 是否可排序，默认 false */
  sortable?: boolean
  /**
   * 敏感标记（字段级数据权限，硬约束）：true 时该列
   * ① 不出现在列设置面板（用户无法手动取消隐藏）；
   * ② 不参与导出（导出列白名单剔除）。
   * 用于手机号、余额、密钥等敏感字段。默认 false。
   */
  sensitive?: boolean
  /**
   * 自定义渲染的具名插槽名：组件渲染 `<template #body-${bodyKey}>`，
   * 由使用方在外层 CrudDataTable 上提供对应插槽（如 #body-status）。
   * 不传则按 field 纯文本渲染。
   */
  bodyKey?: string
  /**
   * 移动端列优先级（change C252）：1 = 核心列，所有视口显示（默认）；
   * >= 2 = 次要列，窄视口（< 768px，与 theme/breakpoints.scss bp-down(md) 一致）
   * 下不渲染，避免手机端横向拖动。仅影响渲染层——列设置面板、偏好持久化、
   * 导出列收集均不受此字段影响（桌面/导出语义不变）。
   */
  responsivePriority?: number
}
