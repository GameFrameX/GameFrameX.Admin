import {ref} from 'vue'
import {showError} from '../utils/message'
import type {UseEditDialogConfig} from './types/crud'
import type {FormRules} from './types/crud'
import {buildFormResolver} from '../utils/formResolver'
import {normalizeIsEnabled, applyDefaultIsEnabledForAdd} from '../utils/isEnabled'
import {readRowVersion} from '../utils/rowVersion'

function cloneByJson<T>(row: T): T {
    return JSON.parse(JSON.stringify(row))
}

function isNewGameMode(id: unknown): boolean {
    return id == undefined || id == null || id === '' || id === 0
}

function isNewSystemMode(id: unknown): boolean {
    // 雪花 long 的 JSON 形态是字符串（超出 JS 安全整数），正整数字符串同样是已有项；
    // 否则菜单/租户等 long 主键行编辑会被误判为新增，提交打到 add 接口报"路由名称重复"
    if (typeof id === 'string') return !/^[1-9]\d*$/.test(id)
    return !(id != null && typeof id === 'number' && id > 0)
}

// 从任意行对象中安全提取 id（兼容泛型 T / unknown / 缺 id 的对象）
function extractId(row: unknown): unknown {
    return row != null && typeof row === 'object' && 'id' in row ? (row as Record<string, unknown>).id : undefined
}

// 只读审计字段：编辑回显时随整行带入，但新增/编辑提交时必须剥离，
// 避免 createdTime 等 ISO 字符串污染载荷（后端 long/Int64 解析失败）或审计列被前端覆写。
// UpdateTime 与 UpdatedTime 并存：后端基类 UpdateTime 归一化为 updateTime，
// 部分老接口（JobTrigger 等）仍保留 UpdatedTime → updatedTime。
const AUDIT_FIELDS = ['createdTime', 'createdId', 'createdBy', 'updatedTime', 'updatedId', 'updatedBy', 'updateTime', 'tenantId']
const LEGACY_ROW_VERSION_FIELDS = ['version', 'Version']

function stripAuditFields<T extends object>(values: T): T {
    if (!values || typeof values !== 'object') {
        return values
    }
    const payload = (Array.isArray(values) ? [...(values as unknown[])] : {...(values as Record<string, unknown>)}) as Record<string, unknown>
    const rowVersion = readRowVersion(payload)
    if (rowVersion != null && payload.rowVersion == null) {
        payload.rowVersion = rowVersion
    }
    for (const key of AUDIT_FIELDS) {
        if (key in payload) {
            delete payload[key]
        }
    }
    for (const key of LEGACY_ROW_VERSION_FIELDS) {
        if (key in payload) {
            delete payload[key]
        }
    }
    return payload as T
}

export function useEditDialog<T = any>(config: UseEditDialogConfig<T>) {
    const {
        addApi,
        updateApi,
        defaultForm,
        idCheckMode = 'game',
        cloneStrategy = 'json',
    } = config

    const isNewItem = idCheckMode === 'system' ? isNewSystemMode : isNewGameMode

    const formRef = ref()
    const formKey = ref(0)
    const dialogVisible = ref(false)
    const isEditor = ref(false)
    const ruleForm = ref<T>(defaultForm ? defaultForm() : ({} as T))
    // 提交中标志：覆盖校验→beforeSubmit→addApi/updateApi→afterSubmit 全程，
    // 供确认按钮 loading 防连点；入口重入守卫防 Form @submit 与按钮 click 双通道并发。
    const submitting = ref(false)
    const rules = ref<FormRules | undefined>(config.rules)
    const resolver = buildFormResolver(rules)
    let _submitResolve: (() => void) | null = null

    const cloneRow = typeof cloneStrategy === 'function'
        ? cloneStrategy
        : cloneStrategy === 'structured'
            ? (row: T) => structuredClone(row)
            : cloneByJson

    const openDialog = async (row?: T) => {
        // 41 个页面的新增惯例是 openDialog({})：空对象会让 ?? 短路，页面声明的 defaultForm
        // （如公告 language:'default'、isActive:true）全部失效。空对象视为未传初始数据，回落 defaultForm；
        // 编辑模式 row 一定带 Id 等键，不受影响。
        const hasRowData = row !== undefined && row !== null && Object.keys(row).length > 0
        const cloned = cloneRow(hasRowData ? row : (defaultForm ? defaultForm() : ({} as T)))
        isEditor.value = !isNewItem(extractId(cloned))
        let source = cloned
        if (config.beforeOpen) {
            const result = await config.beforeOpen(cloned, isEditor.value)
            if (result !== undefined) source = result
        }
        // 新增模式下 IsEnabled 未显式设置（null/undefined/缺字段）时填入 false 默认值，
        // 使新增弹窗 ToggleSwitch 显示与最终提交值一致；显式 true/false 不变。
        // 编辑回显不进入此分支，保持 normalizeIsEnabled 既有「缺省视为启用」的展示语义。
        if (!isEditor.value) {
            applyDefaultIsEnabledForAdd(source)
        }
        ruleForm.value = normalizeIsEnabled(source)
        formKey.value++
        dialogVisible.value = true
    }

    const closeDialog = () => {
        dialogVisible.value = false
    }

    const cancel = () => {
        dialogVisible.value = false
    }

    const onFormSubmit = async ({valid}: { valid: boolean }) => {
        // 重入守卫：footer 按钮 click 与 Form @submit 双通道都汇到此处，
        // 提交中（含慢网络 await addApi/updateApi 期间）直接丢弃后续触发，防止重复创建。
        if (submitting.value) return
        submitting.value = true
        try {
            if (!valid) {
                showError(t('message.common.message.formValidationFailed'))
                return
            }

            let formValues = ruleForm.value
            if (config.beforeSubmit) {
                const result = await config.beforeSubmit(formValues, isEditor.value)
                if (result === false) return
                if (result !== undefined) {
                    if (isEditor.value && result && typeof result === 'object') {
                        const source = ruleForm.value as unknown as Record<string, unknown>
                        const target = result as unknown as Record<string, unknown>
                        if (target.id == null && source.id != null) target.id = source.id
                        const rowVersion = readRowVersion(source)
                        if (target.rowVersion == null && rowVersion != null) target.rowVersion = rowVersion
                    }
                    formValues = result
                }
            }

            let submitResponse: unknown
            if (isNewItem(formValues.id)) {
                submitResponse = await addApi(stripAuditFields(formValues))
            } else {
                submitResponse = await updateApi(stripAuditFields(formValues))
            }

            config.afterSubmit?.(formValues, isEditor.value, submitResponse)
            closeDialog()
        } finally {
            // 校验失败 / beforeSubmit 中止 / API 异常都走这里复位，保证按钮 loading 不卡死。
            submitting.value = false
            _submitResolve?.()
            _submitResolve = null
        }
    }

    const submit = (): Promise<void> => {
        if (!formRef.value) return Promise.resolve()
        return new Promise((resolve) => {
            _submitResolve = resolve
            formRef.value.submit()
        })
    }

    return {
        dialogVisible,
        ruleForm,
        formRef,
        formKey,
        isEditor,
        submitting,
        resolver,
        onFormSubmit,
        openDialog,
        closeDialog,
        cancel,
        submit,
    }
}
