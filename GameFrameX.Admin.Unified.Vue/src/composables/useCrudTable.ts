import {ref, onMounted} from 'vue'
import {showConfirm, showSuccess} from '../utils/message'
import {DEFAULT_PAGE_SIZE} from '../constants/pagination'
import type {UseCrudTableConfig, PaginationState} from './types/crud'
import {normalizeIsEnabled} from '../utils/isEnabled'
import {normalizeRowVersion} from '../utils/rowVersion'

const DEFAULT_PAGINATION: PaginationState = {page: 1, pageSize: DEFAULT_PAGE_SIZE, total: 0}

// Date 实例 → Unix 秒；数组 → 逐元素递归（保 null）。其他原值透传。
// 用在 onQuery 出参前，把 queryParams 中的 Range / 独立日期统一成后端 long 秒。
function toUnixSeconds(value: unknown): unknown {
    if (value instanceof Date) {
        return Math.floor(value.getTime() / 1000)
    }
    if (Array.isArray(value)) {
        return value.map((item) => toUnixSeconds(item))
    }
    return value
}

function normalizeQueryParams(params: Record<string, unknown>): Record<string, unknown> {
    if (!params || typeof params !== 'object') {
        return params
    }
    const next: Record<string, unknown> = {}
    for (const key of Object.keys(params)) {
        next[key] = toUnixSeconds(params[key])
    }
    return next
}

// 删除确认文案默认拼接的行标识候选字段，按优先级取首个非空值
const DEFAULT_DELETE_LABEL_FIELDS = ['name', 'account', 'title', 'fileName', 'nickName', 'code', 'jobId', 'triggerId', 'id']

// 从行数据解析删除确认文案要展示的标识：候选字段依次取首个非空字符串；数字/布尔 id 兜底转字符串
function resolveDeleteLabel(row: unknown, fields: string[]): string {
    if (!row || typeof row !== 'object') {
        return ''
    }
    const record = row as Record<string, unknown>
    for (const field of fields) {
        const value = record[field]
        if (typeof value === 'string' && value.trim() !== '') {
            return value.trim()
        }
        if (typeof value === 'number' && Number.isFinite(value)) {
            return String(value)
        }
    }
    return ''
}

export function useCrudTable<T = any>(config: UseCrudTableConfig<T>) {
    const {api, immediate = false} = config

    const loading = ref(false)
    const tableData = ref<T[]>([])
    const queryParams = ref<Record<string, unknown>>({})
    const tableParams = ref<PaginationState>({
        ...DEFAULT_PAGINATION,
        ...config.pagination,
    })

    const onQuery = async () => {
        loading.value = true
        try {
            // queryParams 中的 Date（含 Range 数组）统一转 long 秒后再合并分页参数
            const normalizedQuery = normalizeQueryParams(queryParams.value)
            const res = await api.pageApi(Object.assign(normalizedQuery, tableParams.value) as T)
            tableData.value = (res.data?.data?.items ?? []).map((item: T) => normalizeRowVersion(normalizeIsEnabled(item)))
            tableParams.value.total = res.data?.data?.total ?? 0
        } finally {
            loading.value = false
        }
    }

    const onSizeChange = (val: number) => {
        tableParams.value.pageSize = val
        onQuery()
    }

    const onCurrentChange = (val: number) => {
        tableParams.value.page = val
        onQuery()
    }

    const onDelete = async (row: T) => {
        if (!api.deleteApi) return
        // 自定义文案支持函数形式（打开弹窗前求值）；默认文案拼接行标识，让用户明确看到删的是哪条
        const customMessage = typeof config.deleteConfirmMessage === 'function' ? config.deleteConfirmMessage(row) : config.deleteConfirmMessage
        const label = resolveDeleteLabel(row, config.deleteConfirmLabelFields ?? DEFAULT_DELETE_LABEL_FIELDS)
        const message = customMessage ?? (label ? t('message.common.message.deleteConfirmWithName', {name: label}) : t('message.common.message.deleteConfirm'))
        // onAccept 内联删除动作：慢网络下确认按钮 loading 防连点，只发一次 deleteApi
        const confirmed = await showConfirm(
            message,
            t('message.common.message.tip'),
            {confirmText: t('message.common.button.confirm'), cancelText: t('message.common.button.cancel'), type: 'danger', onAccept: () => api.deleteApi!(row)},
        )
        if (!confirmed) return
        showSuccess(config.deleteSuccessMessage ?? t('message.common.message.deleteSuccess'))
        onQuery()
    }

    const onReset = () => {
        queryParams.value = {}
        tableParams.value.page = 1
        onQuery()
    }

    if (immediate) {
        onMounted(() => onQuery())
    }

    return {
        loading,
        tableData,
        queryParams,
        tableParams,
        onQuery,
        onSizeChange,
        onCurrentChange,
        onDelete,
        onReset,
    }
}
