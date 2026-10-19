import type { CrudAction } from './types'
import { t } from '../../i18n/index'
import { faPenToSquare, faTrash, faEye } from '@fortawesome/free-solid-svg-icons'

function iconClass(icon: { iconName: string }): string {
  return `fa-solid fa-${icon.iconName}`
}

/** 创建自定义操作按钮 */
export function createAction(
  auth: string,
  onClick: (row: any) => void,
  options?: {
    label?: string
    icon?: string
    severity?: CrudAction['severity']
    visible?: (row: any) => boolean
  },
): CrudAction {
  return {
    auth,
    label: options?.label ?? auth,
    icon: options?.icon,
    severity: options?.severity,
    visible: options?.visible,
    onClick,
  }
}

/** delete 回调包装：支持行级 visible 条件 */
export function withVisible(fn: (row: any) => void, visible: (row: any) => boolean): (row: any) => void {
  // visible 检查移到组件层面，这里仅透传
  return fn
}
