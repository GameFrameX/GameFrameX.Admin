<template>
  <div class="crud-data-table">
    <!-- 工具栏（change C146）：列设置齿轮 + 导出按钮。仅在有需要时渲染。 -->
    <div v-if="showToolbar" class="crud-data-table-toolbar">
      <div class="crud-data-table-toolbar-left">
        <slot name="toolbar-left" />
      </div>
      <div class="crud-data-table-toolbar-actions">
        <!-- 列设置：仅声明式 columns 模式下可用（slot 模式无列元数据，无法配置） -->
        <Button
          v-if="hasColumns && showColumnConfig"
          text
          size="small"
          severity="secondary"
          icon="fa-solid fa-sliders"
          :aria-label="t('common.table.columnDisplay')"
          :title="t('common.table.columnDisplay')"
          @click="openColumnConfig"
        />
        <ColumnConfigPanel
          v-if="hasColumns"
          ref="columnConfigPanelRef"
          :configurableColumns="configurableColumns"
          @toggle-visible="onToggleVisible"
          @move-up="onMoveUp"
          @move-down="onMoveDown"
          @reset="onReset"
        />
        <!-- 导出：受 showExport + `${authPrefix}:export` 权限码双重控制（功能开关层） -->
        <Button
          v-if="showExport && authPrefix"
          v-auth="`${authPrefix}:export`"
          text
          size="small"
          severity="secondary"
          icon="fa-solid fa-download"
          :loading="exporting"
          :aria-label="t('common.button.export')"
          :title="t('common.button.export')"
          @click="onExport"
        />
      </div>
    </div>
    <DataTable ref="dataTableRef" :value="value" resizableColumns columnResizeMode="expand" scrollable v-bind="$attrs">
      <Column
        v-if="showIdColumn"
        :field="idColumnField"
        :header="idColumnHeader"
        :style="{ width: idColumnWidth }"
        :headerStyle="{ textAlign: 'center' }"
        :bodyStyle="{ textAlign: 'center' }"
      />
      <!-- 额外列插槽（如 selectionMode 列）：始终渲染，默认为空 -->
      <slot name="extra-columns" />
      <!-- 声明式 columns 模式：v-for 渲染，受列设置面板 + 持久化控制 -->
      <template v-if="hasColumns">
        <Column
          v-for="col in renderColumns"
          :key="col.field"
          :field="col.field"
          :header="col.header"
          :style="col.width ? { width: col.width } : undefined"
          :headerStyle="{ textAlign: col.align ?? (col.numeric ? 'right' : undefined) }"
          :bodyStyle="{ textAlign: col.align ?? (col.numeric ? 'right' : undefined) }"
          :frozen="col.frozen"
          alignFrozen="left"
          :sortable="col.sortable"
          :bodyClass="col.numeric ? 'tabular-nums' : undefined"
        >
          <!-- bodyKey 具名插槽透传：使用方在外层提供 #body-<bodyKey> 自定义渲染（Tag/格式化/枚举等） -->
          <template v-if="col.bodyKey && $slots[`body-${col.bodyKey}`]" #body="slotProps">
            <slot :name="`body-${col.bodyKey}`" v-bind="slotProps" />
          </template>
        </Column>
      </template>
      <!-- 默认 slot 模式：未传 columns 时向后兼容，使用方继续手摆 <Column> -->
      <slot v-else />
      <Column
        v-if="showActionColumn"
        :header="resolvedActionHeader"
        :style="{ width: actionColumnWidth }"
        frozen
        alignFrozen="right"
      >
        <template #body="{ data }">
          <Button
            v-if="resolvedActions.detail"
            link size="small"
            v-auth="`${authPrefix}:detail`"
            @click="invokeAction(resolvedActions.detail, data)"
          >
            <i class="fa-solid fa-eye mr-1"></i><span class="crud-action-label">{{ t('common.button.detail') }}</span>
          </Button>
          <Button
            v-if="resolvedActions.edit"
            link size="small"
            v-auth="`${authPrefix}:update`"
            @click="invokeAction(resolvedActions.edit, data)"
          >
            <i class="fa-solid fa-pen-to-square mr-1"></i><span class="crud-action-label">{{ t('common.button.edit') }}</span>
          </Button>
          <Button
            v-if="resolvedActions.delete && (!resolvedActions.delete.visible || resolvedActions.delete.visible(data))"
            text size="small" severity="danger"
            v-auth="`${authPrefix}:delete`"
            @click="invokeAction(resolvedActions.delete, data)"
          >
            <i class="fa-solid fa-trash mr-1"></i><span class="crud-action-label">{{ t('common.button.delete') }}</span>
          </Button>
          <template v-for="action in customActions" :key="action.auth">
            <Button
              v-if="!action.visible || action.visible(data)"
              link size="small"
              :severity="action.severity"
              v-auth="`${authPrefix}:${action.auth}`"
              v-bind="action.buttonProps"
              @click="action.onClick(data)"
            >
              <i v-if="action.icon" :class="action.icon" class="mr-1"></i>
              <span class="crud-action-label">{{ action.label }}</span>
            </Button>
          </template>
          <slot name="action-extra" :data="data" :authPrefix="authPrefix" />
        </template>
      </Column>
      <template #empty>
        <slot name="empty">
          <div class="crud-data-table-empty">
            <i class="fa-solid fa-inbox crud-data-table-empty-icon"></i>
            <span class="crud-data-table-empty-text">{{ t('message.common.table.noData') }}</span>
          </div>
        </slot>
      </template>
    </DataTable>
    <div v-if="shouldShowPagination" class="crud-data-table-paginator">
      <Paginator :rows="rows" :totalRecords="totalRecords" :rowsPerPageOptions="clampedRowsPerPageOptions" :first="first"
                 @page="(e: any) => emit('page', e)" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import DataTable from 'primevue/datatable'
import Paginator from 'primevue/paginator'
import Column from 'primevue/column'
import Button from 'primevue/button'
import { hasAnyAuth } from '../../utils/authFunction'
import { DEFAULT_PAGE_SIZE, DEFAULT_PAGE_SIZE_OPTIONS, MAX_PAGE_SIZE } from '../../constants/pagination'
import { downloadByData, getFileName } from '../../utils/download'
import { service } from '../../utils/request'
import { showSuccess, showWarning } from '../../utils/message'
import { useTableColumnPersistence } from '../../composables/useTableColumnPersistence'
import { useMediaQueryMatch } from '../../composables/useMediaQueryMatch'
import type { ActionConfig, CrudActions, CrudAction, CrudColumnConfig } from './types'
import ColumnConfigPanel from './ColumnConfigPanel.vue'

interface NormalizedAction {
  onClick: (row: any) => void
  visible?: (row: any) => boolean
}

function normalizeAction(config?: ActionConfig): NormalizedAction | undefined {
  if (!config) return undefined
  return typeof config === 'function' ? { onClick: config } : config
}

const props = withDefaults(defineProps<{
  value?: any[]
  totalRecords?: number
  rows?: number
  rowsPerPageOptions?: number[]
  first?: number
  showPagination?: boolean
  authPrefix?: string
  actions?: CrudActions
  customActions?: CrudAction[]
  showIdColumn?: boolean
  idColumnField?: string
  idColumnHeader?: string
  idColumnWidth?: string
  actionColumnWidth?: string
  actionColumnHeader?: string
  /** 声明式列配置：传入则启用列设置/导出能力，走 v-for 渲染；不传则回退默认 slot（向后兼容） */
  columns?: CrudColumnConfig[]
  /** 列偏好持久化 key 后缀，默认取 authPrefix（按实体隔离） */
  storageKey?: string
  /** 是否显示列设置齿轮按钮，默认 true（仅 columns 模式生效） */
  showColumnConfig?: boolean
  /** 是否显示导出按钮，默认 false（显式 opt-in——后端 Export 端点是逐服务显式提供的（C147），
   *  未提供端点的页面显示按钮只会得到 404 死按钮；叠加 `${authPrefix}:export` 权限码） */
  showExport?: boolean
  /** 导出时携带的查询条件，应与 pageApi 入参一致（命中数据范围） */
  exportQuery?: any
}>(), {
  value: () => [],
  totalRecords: 0,
  rows: DEFAULT_PAGE_SIZE,
  rowsPerPageOptions: () => DEFAULT_PAGE_SIZE_OPTIONS,
  first: 0,
  showPagination: true,
  showIdColumn: false,
  idColumnField: 'id',
  idColumnHeader: 'ID',
  idColumnWidth: '120px',
  actionColumnWidth: '140px',
  actionColumnHeader: '',
  showColumnConfig: true,
  showExport: false,
})

const emit = defineEmits<{
  page: [event: any]
}>()

const dataTableRef = ref()
const columnConfigPanelRef = ref()

// ── 列配置 + 本地持久化（change C146） ──────────────────────────
// 仅 columns 模式启用；slot 模式下 columnsList 为空，composable 内部无副作用。
const columnsList = computed<CrudColumnConfig[]>(() => props.columns ?? [])
const {
  configurableColumns,
  visibleColumns,
  toggleVisible,
  moveUp,
  moveDown,
  reset: resetColumns,
} = useTableColumnPersistence(props.storageKey || props.authPrefix || '', columnsList)

const hasColumns = computed(() => columnsList.value.length > 0)

// ── 移动端列优先级过滤（change C252） ──────────────────────────
// 窄视口（< 768px，与 theme/breakpoints.scss bp-down(md) 一致）下隐藏
// responsivePriority >= 2 的次要列，核心列（默认 1）直接可读。仅作用于渲染层，
// 列设置面板 / 偏好持久化 / 导出均不受影响（导出走 visibleColumns）。
const isNarrowViewport = useMediaQueryMatch('(max-width: 767px)')
const renderColumns = computed(() => {
  if (!isNarrowViewport.value) return visibleColumns.value
  return visibleColumns.value.filter((c) => (c.responsivePriority ?? 1) < 2)
})

function openColumnConfig(event: Event) {
  columnConfigPanelRef.value?.toggle(event)
}

function onToggleVisible(field: string, visible: boolean) {
  toggleVisible(field, visible)
}

function onMoveUp(field: string) {
  moveUp(field)
}

function onMoveDown(field: string) {
  moveDown(field)
}

function onReset() {
  resetColumns()
}

// ── 导出前端层（change C146，对接后端 C147 契约） ────────────────
const exporting = ref(false)

/** 工具栏整体显隐：任一按钮需要显示即渲染 */
const showToolbar = computed(() => (hasColumns.value && props.showColumnConfig) || (props.showExport && !!props.authPrefix))

/**
 * 导出：收集可见非敏感列（field + header）+ 查询条件，请求后端通用导出端点
 * （动态路由 `${authPrefix}/export`），Blob 下载。
 *
 * 安全约束：sensitive 列在收集阶段即剔除（字段级数据权限），即便用户在列设置
 * 面板将其纳入可见集合也不会导出；导出列白名单由前端可见列决定，后端 C147
 * 再做一次白名单投影兜底。
 */
async function onExport() {
  if (!props.authPrefix) return
  // 导出列基于无视口过滤的可见列（visibleColumns）：移动端隐藏的次要列不随导出丢失
  const exportColumns = visibleColumns.value
    .filter((c) => !c.sensitive)
    .map((c) => ({ field: c.field, header: c.header }))
  if (exportColumns.length === 0) {
    showWarning(t('common.table.noExportColumns'))
    return
  }
  exporting.value = true
  try {
    const input = {
      entity: props.authPrefix,
      columns: exportColumns,
      query: props.exportQuery ?? {},
    }
    const res = await service.post(`/api/${props.authPrefix}/export`, input, {
      responseType: 'blob',
      headers: { Accept: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' },
    })
    const mime = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    const filename = resolveExportFilename(res.headers, props.authPrefix)
    downloadByData(res.data as BlobPart, filename, mime)
    showSuccess(t('common.table.exportData'))
  } catch {
    // 错误提示已由响应拦截器统一处理，此处不重复弹窗
  } finally {
    exporting.value = false
  }
}

/** 从 content-disposition 解析文件名，失败则回退 `<实体>-<日期>.xlsx` */
function resolveExportFilename(headers: any, authPrefix: string): string {
  try {
    if (headers && headers['content-disposition']) {
      return getFileName(headers)
    }
  } catch {
    // content-disposition 缺失或格式异常时走默认名
  }
  const ts = new Date().toISOString().slice(0, 10)
  return `${authPrefix}-${ts}.xlsx`
}

const pageCount = computed(() => {
  if (!props.rows || props.rows <= 0) return 0
  return Math.ceil(props.totalRecords / props.rows)
})

const shouldShowPagination = computed(() => props.showPagination && pageCount.value > 1)

const clampedRowsPerPageOptions = computed(() =>
  props.rowsPerPageOptions.filter((n) => n <= MAX_PAGE_SIZE),
)

const resolvedActionHeader = computed(() =>
  props.actionColumnHeader || t('common.table.action'),
)

const resolvedActions = computed(() => ({
  detail: normalizeAction(props.actions?.detail),
  edit: normalizeAction(props.actions?.edit),
  delete: normalizeAction(props.actions?.delete),
}))

function invokeAction(action: NormalizedAction | undefined, row: any) {
  action?.onClick(row)
}

const builtinAuthKeys = computed(() => {
  const keys: string[] = []
  if (props.actions?.detail) keys.push('detail')
  if (props.actions?.edit) keys.push('update')
  if (props.actions?.delete) keys.push('delete')
  return keys
})

const showActionColumn = computed(() => {
  if (!props.authPrefix) return false
  const hasBuiltin = builtinAuthKeys.value.length > 0
  const hasCustom = props.customActions && props.customActions.length > 0
  if (!hasBuiltin && !hasCustom) return false
  const auths = [
    ...builtinAuthKeys.value,
    ...(props.customActions?.map(a => a.auth) ?? []),
  ]
  return hasAnyAuth(...auths.map(a => `${props.authPrefix}:${a}`))
})

defineExpose({
  dataTableRef,
})
</script>

<style scoped lang="scss">
.crud-data-table {
  display: flex;
  flex-direction: column;

  :deep(.p-datatable) {
    flex: 1;
  }

  .crud-data-table-toolbar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 8px;
    margin-bottom: 8px;
    min-height: 32px;

    .crud-data-table-toolbar-actions {
      display: flex;
      align-items: center;
      gap: 4px;
      margin-left: auto;
    }
  }

  .crud-data-table-paginator {
    display: flex;
    justify-content: center;
    margin-top: 12px;
  }

  .crud-data-table-empty {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 40px 0;

    .crud-data-table-empty-icon {
      font-size: 40px;
      color: var(--p-surface-400);
      margin-bottom: 10px;
    }

    .crud-data-table-empty-text {
      color: var(--p-surface-500);
      font-size: 14px;
    }
  }

  // 移动端操作按钮只显示图标，隐藏文字（C143，阈值见 theme/breakpoints.scss）
  @include bp-down(sm) {
    .crud-action-label {
      display: none;
    }

    // 文字隐藏后按钮只剩图标，扩触控目标至 36px（GFX-661）
    :deep(.p-datatable-tbody .p-button) {
      min-width: 36px;
      min-height: 36px;
      justify-content: center;
    }
  }
}
</style>
