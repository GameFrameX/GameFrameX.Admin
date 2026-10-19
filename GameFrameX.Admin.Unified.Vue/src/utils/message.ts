/**
 * 框架无关的消息/确认对话框抽象层
 * 当前实现委托给 PrimeVue Toast / ConfirmDialog。
 * 需在 App.vue 中调用 initMessageService(toast, confirm) 完成初始化。
 */
import type {ToastMessageOptions} from 'primevue/toast'
import type {ConfirmationOptions} from 'primevue/confirmationoptions'
import {t} from '../i18n/index'

type ToastService = { add: (options: ToastMessageOptions) => void }
type ConfirmService = { require: (options: ConfirmationOptions) => void; close?: () => void }

let toastRef: ToastService | null = null
let confirmRef: ConfirmService | null = null

export function initMessageService(toast: ToastService, confirm: ConfirmService) {
    toastRef = toast
    confirmRef = confirm
}

export type MessageType = 'success' | 'error' | 'warning' | 'info'

const severityMap: Record<MessageType, string> = {
    success: 'success',
    error: 'error',
    warning: 'warn',
    info: 'info',
}

// toast 按 severity 分级停留时长（GFX-661）：错误类长文案需要更长阅读时间，成功/提示类快速反馈即可
const severityLifeMap: Record<MessageType, number> = {
    success: 3000,
    info: 3500,
    warning: 4500,
    error: 6000,
}

export function showMessage(message: string, type: MessageType = 'info') {
    toastRef?.add({severity: severityMap[type], detail: message, life: severityLifeMap[type]})
}

export function showSuccess(message: string) {
    showMessage(message, 'success')
}

export function showError(message: string, sticky: boolean = false) {
    if (sticky) {
        // sticky 错误：不自动消失（life 省略），仅能通过 toast 关闭按钮手动清除
        toastRef?.add({severity: severityMap.error, detail: message, closable: true})
        return
    }
    showMessage(message, 'error')
}

export function showWarning(message: string) {
    showMessage(message, 'warning')
}

export interface ConfirmOptions {
    confirmText?: string
    cancelText?: string
    /** 'danger' = 红色确认按钮 + 警告图标，用于删除等危险操作 */
    type?: 'default' | 'danger'
    /**
     * 确认后的异步动作钩子：提供时点击确认按钮进入 loading 并 await 完成后才关闭弹窗，
     * 防止慢网络下连点确认重复触发删除等危险操作。
     * 执行失败弹错误提示并保持弹窗打开，本函数 resolve(false)；未提供时保持旧语义（点击即 resolve(true) 关闭）。
     */
    onAccept?: () => Promise<unknown>
}

export function showConfirm(message: string, title: string = t('message.common.message.tip'), options?: ConfirmOptions): Promise<boolean> {
    return new Promise((resolve) => {
        const baseOptions = {
            group: options?.type === 'danger' ? 'danger' : undefined,
            message,
            header: title,
            acceptLabel: options?.confirmText ?? t('message.common.button.confirm'),
            rejectLabel: options?.cancelText ?? t('message.common.button.cancel'),
            acceptIcon: options?.type === 'danger' ? 'pi pi-trash' : undefined,
            acceptClass: options?.type === 'danger' ? 'p-button-danger' : undefined,
        }
        if (!options?.onAccept) {
            confirmRef?.require({
                ...baseOptions,
                accept: () => resolve(true),
                reject: () => resolve(false),
            })
            return
        }
        // onAccept 防重路径：点击确认后不关闭弹窗，重发同一 confirm（acceptProps.loading 置true），
        // await 完成才关闭；失败弹错误提示并同样关闭。慢网络下确认按钮 loading 期间连点无效。
        const onAccept = options.onAccept
        confirmRef?.require({
            ...baseOptions,
            acceptProps: {loading: false},
            accept: () => {
                confirmRef?.require({
                    ...baseOptions,
                    acceptProps: {loading: true},
                    accept: () => {},
                })
                onAccept().then(() => {
                    confirmRef?.close?.()
                    resolve(true)
                }).catch(() => {
                    confirmRef?.close?.()
                    showError(t('message.common.message.operationFailed'))
                    resolve(false)
                })
            },
            reject: () => resolve(false),
        })
    })
}
