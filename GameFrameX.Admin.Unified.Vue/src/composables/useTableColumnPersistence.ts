import {ref, computed, watch, toValue, type Ref, type ComputedRef} from 'vue'
import {local} from '../utils/storage'
import type {CrudColumnConfig} from '../components/crud-data-table/types'

/** 单列持久化偏好（落盘结构） */
export interface ColumnPersistState {
    visible: boolean
    order: number
}

/** 运行时列状态：声明配置合并持久化偏好后的可变形态 */
export interface ColumnRuntimeState extends CrudColumnConfig {
    visible: boolean
    order: number
}

type ColumnsInput = CrudColumnConfig[] | Ref<CrudColumnConfig[]> | ComputedRef<CrudColumnConfig[]>

/**
 * 列配置本地持久化（change C146）。
 *
 * key 规范：`crud-table:columns:${authPrefix}`，按实体隔离（不按 route path，
 * 避免路由变更后配置失效）。存储结构 `{ [field]: { visible, order } }`。
 *
 * 业务/安全约束：
 * ① sensitive 列恒可见、不进列设置面板、不落盘偏好（字段级数据权限硬约束）；
 * ② 读取后与当前 columns 做交集校验，丢弃已失效的 field——页面改版后旧
 *    localStorage 不致表格空白或报错。
 *
 * @param authPrefix 实体权限前缀，用于隔离 localStorage key
 * @param columns 声明式列配置（可为数组 / ref / computed）
 */
export function useTableColumnPersistence(authPrefix: string, columns: ColumnsInput) {
    // key 规范：crud-table:columns:${authPrefix}；local.set 会再自动加项目名前缀
    const storageKey = `crud-table:columns:${authPrefix}`

    /** 读取落盘偏好；非法或缺失时返回空对象，绝不抛错 */
    function readPrefs(): Record<string, ColumnPersistState> {
        try {
            const raw = local.get(storageKey)
            if (!raw || typeof raw !== 'object' || Array.isArray(raw)) return {}
            return raw as Record<string, ColumnPersistState>
        } catch {
            return {}
        }
    }

    /** 运行时列状态（合并声明配置 + 持久化偏好） */
    const states = ref<ColumnRuntimeState[]>([])

    /**
     * 按 columns 重建运行时状态：sensitive 恒可见且不读偏好；其余按偏好恢复
     * visible/order，缺省为 visible=true / 声明序号。隐含交集校验——仅当前
     * columns 中存在的 field 才会进入 states，已失效的旧偏好自然被丢弃。
     */
    function rebuild(list: CrudColumnConfig[]) {
        const prefs = readPrefs()
        states.value = list.map((col, index) => {
            // sensitive 列：恒可见、不读偏好、order 固定为声明位置（不进面板、不落盘）
            if (col.sensitive) {
                return {...col, visible: true, order: index}
            }
            const p = prefs[col.field]
            if (p) {
                return {...col, visible: p.visible, order: p.order}
            }
            return {...col, visible: true, order: index}
        })
    }

    watch(() => toValue(columns), (list) => rebuild(list ?? []), {immediate: true, deep: true})

    /** 可配置列（非 sensitive），供列设置面板渲染 */
    const configurableColumns = computed(() => states.value.filter((s) => !s.sensitive))

    /** 表格实际渲染列：可见列按 order 升序（sensitive 恒可见，含在内） */
    const renderColumns = computed(() =>
        states.value
            .filter((s) => s.visible)
            .slice()
            .sort((a, b) => a.order - b.order),
    )

    /**
     * 仅按用户偏好过滤的可见列（change C252）：与 renderColumns 同义，
     * 作为"无视口过滤"的可见列基准，供导出列收集等不随视口变化的场景使用。
     */
    const visibleColumns = computed(() => renderColumns.value)

    /** 仅落盘非 sensitive 列的偏好（sensitive 不落盘，避免误导与泄露） */
    function persist() {
        const prefs: Record<string, ColumnPersistState> = {}
        for (const s of states.value) {
            if (s.sensitive) continue
            prefs[s.field] = {visible: s.visible, order: s.order}
        }
        local.set(storageKey, prefs)
    }

    /** 切换某列显隐；sensitive 列忽略（恒可见） */
    function toggleVisible(field: string, visible: boolean) {
        const s = states.value.find((x) => x.field === field)
        if (!s || s.sensitive) return
        s.visible = visible
        persist()
    }

    /** 在可配置列序列内交换相邻列的 order（上移 direction=-1 / 下移 +1） */
    function swapOrder(field: string, direction: -1 | 1) {
        const cfg = states.value.filter((x) => !x.sensitive).slice().sort((a, b) => a.order - b.order)
        const idx = cfg.findIndex((x) => x.field === field)
        if (idx < 0) return
        const target = idx + direction
        if (target < 0 || target >= cfg.length) return
        const a = cfg[idx]
        const b = cfg[target]
        const tmp = a.order
        a.order = b.order
        b.order = tmp
        persist()
    }

    /** 上移 */
    function moveUp(field: string) {
        swapOrder(field, -1)
    }

    /** 下移 */
    function moveDown(field: string) {
        swapOrder(field, 1)
    }

    /** 重置为声明默认（全部可见、按声明顺序），并落盘覆盖旧偏好 */
    function reset() {
        const list = toValue(columns) ?? []
        states.value = list.map((col, index) => ({...col, visible: true, order: index}))
        persist()
    }

    return {
        storageKey,
        states,
        configurableColumns,
        renderColumns,
        visibleColumns,
        toggleVisible,
        moveUp,
        moveDown,
        reset,
    }
}
