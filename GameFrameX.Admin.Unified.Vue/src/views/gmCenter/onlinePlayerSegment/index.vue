<template>
  <div class="onlinePlayerSegment-container">
    <Card>
      <template #content>
        <div class="flex align-items-center gap-2 mb-3">
          <ServerSelector mode="local" v-model="serverIdOverride" />
          <button v-if="serverIdOverride !== undefined" type="button" class="p-button p-button-text" @click="serverIdOverride = undefined">
            {{ t('message.common.button.reset') }}
          </button>
        </div>
        <p v-if="!canQueryServer" role="status">{{ t(serverStateMessage) }}</p>
        <SearchForm v-model="queryParams" :fields="searchFields" :showAdd="canQueryServer"
                    authPrefix="onlinePlayerSegment" :defaultSearchKey="false"
                    @query="onQuery" @add="openAdd" />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" style="width: 100%" v-block-loading="loading" showGridlines stripedRows
                       rowKey="id" :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="onlinePlayerSegment"
                       :actions="actions"
                       :customActions="customActions"
                       :columns="columns"
                       @page="onPage" showIdColumn idColumnWidth="180px">
          <template #body-description="{ data }">
            {{ data.description || '-' }}
          </template>
          <template #body-conditionsJson="{ data }">
            {{ summarizeConditions(data.conditionsJson) }}
          </template>
          <template #body-syncStatus="{ data }">
            <Tag :value="syncStatusDesc(data.syncStatus)" :severity="syncStatusSeverity(data.syncStatus)" />
          </template>
          <template #body-lastSyncTime="{ data }">
            {{ data.lastSyncTime > 0 ? formatUnix(data.lastSyncTime) : '-' }}
          </template>
          <template #body-lastOnlineSegmentId="{ data }">
            {{ data.lastOnlineSegmentId && Number(data.lastOnlineSegmentId) > 0 ? data.lastOnlineSegmentId : '-' }}
          </template>
          <template #body-lastServerMessage="{ data }">
            <span v-if="data.lastServerMessage" class="text-red-500 break-all">{{ data.lastServerMessage }}</span>
            <span v-else>-</span>
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>
        <editDialog v-if="canQueryServer" :key="'editDialog-' + serverSelectionVersion" ref="editDialogRef" :title="editTitle" @reloadTable="onQuery" />
        <syncOnlineDialog v-if="canQueryServer" :key="'syncOnlineDialog-' + serverSelectionVersion" :serverId="effectiveServerId" ref="syncDialogRef" @done="onQuery" />
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="onlinePlayerSegment">
/**
 * 玩家分群页（C343 / GFX-759 前端半边）。
 * Admin 只编排分群定义（名称 + 条件白名单），不解释、不复算命中（语义解释在 Online 侧）；
 * 定义生效须经 syncOnline 弹层显式同步（高危：理由必填进审计）。
 * authPrefix 用菜单 Key 前缀 onlinePlayerSegment（与后端 seed 按钮 Key 逐字一致）。
 */
import { ref, computed, watch, onMounted } from 'vue'
import ServerSelector from '/@/components/server-selector/ServerSelector.vue'
import { usePageServer } from '../components/usePageServer'
import Card from 'primevue/card'
import Tag from 'primevue/tag'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import SearchForm from '/@/components/search-form/index.vue'
import type { CrudAction, CrudColumnConfig } from '../../../components/crud-data-table/types.ts'
import { formatUnix } from '../../../utils/formatTime.ts'
import { showConfirm, showSuccess } from '../../../utils/message'
import { DEFAULT_PAGE_SIZE } from '../../../constants/pagination'
import { normalizeRowVersion } from '../../../utils/rowVersion'
import { usePrimeTablePage } from '../../../composables'
import editDialog from '/@/views/gmCenter/onlinePlayerSegment/component/EditDialog.vue'
import syncOnlineDialog from '/@/views/gmCenter/onlinePlayerSegment/component/SyncOnlineDialog.vue'
import { gameOnlinePlayerSegmentPagePost, gameOnlinePlayerSegmentDeletePost } from '../../../api/gameOnlinePlayerSegment'
import { OnlinePlayerSegmentSyncStatus } from '../../../api/gameOnlinePlayerSegment/types'
import { ONLINE_SEGMENT_FIELD_OPTIONS, ONLINE_SEGMENT_OP_OPTIONS } from './constants'

const editDialogRef = ref()
const syncDialogRef = ref()
const editTitle = ref('')

const syncStatusDesc = (v: number) => {
  if (v === OnlinePlayerSegmentSyncStatus.Synced) return t('message.onlineCenter.playerSegment.syncStatusSynced')
  if (v === OnlinePlayerSegmentSyncStatus.Failed) return t('message.onlineCenter.playerSegment.syncStatusFailed')
  return t('message.onlineCenter.playerSegment.syncStatusNotSynced')
}

const syncStatusSeverity = (v: number): 'success' | 'warn' | 'danger' | 'info' => {
  if (v === OnlinePlayerSegmentSyncStatus.Synced) return 'success'
  if (v === OnlinePlayerSegmentSyncStatus.Failed) return 'danger'
  return 'info'
}

const fieldLabel = (field: string) => {
  const item = ONLINE_SEGMENT_FIELD_OPTIONS.find((i) => i.value === field)
  return item ? t(item.labelKey) : field
}

const opLabel = (op: string) => {
  const item = ONLINE_SEGMENT_OP_OPTIONS.find((i) => i.value === op)
  return item ? t(item.labelKey) : op
}

/** 条件摘要：`活跃天数 ≥ 7；留存天数 ≤ 30`（不可解析 JSON 原样展示，暴露异常数据） */
const summarizeConditions = (conditionsJson?: string | null): string => {
  if (!conditionsJson) {
    return t('message.onlineCenter.playerSegment.unconditional')
  }
  try {
    const conditions = JSON.parse(conditionsJson) as Array<{ field: string; op: string; value: string }>
    if (!Array.isArray(conditions) || conditions.length === 0) {
      return t('message.onlineCenter.playerSegment.unconditional')
    }
    return conditions.map((c) => `${fieldLabel(c.field)} ${opLabel(c.op)} ${c.value}`).join('；')
  } catch {
    return conditionsJson
  }
}

const searchFields = computed(() => [
  { key: 'name', label: t('message.onlineCenter.playerSegment.name'), type: 'input', placeholder: t('message.onlineCenter.playerSegment.name') },
  { key: 'syncStatus', label: t('message.onlineCenter.playerSegment.syncStatus'), type: 'select', options: buildSyncStatusOptions(), optionLabel: 'label', optionValue: 'value', placeholder: t('message.onlineCenter.playerSegment.syncStatus'), advanced: true },
])

const buildSyncStatusOptions = () => ([
  { value: OnlinePlayerSegmentSyncStatus.NotSynced, label: t('message.onlineCenter.playerSegment.syncStatusNotSynced') },
  { value: OnlinePlayerSegmentSyncStatus.Synced, label: t('message.onlineCenter.playerSegment.syncStatusSynced') },
  { value: OnlinePlayerSegmentSyncStatus.Failed, label: t('message.onlineCenter.playerSegment.syncStatusFailed') },
])

const { serverIdOverride, effectiveServerId, canQueryServer, serverStateMessage, serverSelectionVersion, beginServerRequest } = usePageServer()
const loading = ref(false)
const tableData = ref<any[]>([])
const queryParams = ref<Record<string, any>>({})
const tableParams = ref({ page: 1, pageSize: DEFAULT_PAGE_SIZE, total: 0 })

const onQuery = async () => {
  const request = beginServerRequest()
  if (!request) return
  loading.value = true
  try {
    const response = await gameOnlinePlayerSegmentPagePost({ ...queryParams.value, ...tableParams.value })
    if (!request.isCurrent()) return
    tableData.value = (response.data?.data?.items ?? []).map(item => normalizeRowVersion(item))
    tableParams.value.total = response.data?.data?.total ?? 0
  } finally {
    if (request.isCurrent()) loading.value = false
  }
}

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAdd = () => {
  if (!canQueryServer.value) return
  editTitle.value = t('message.onlineCenter.playerSegment.add')
  editDialogRef.value.openDialog({})
}

const openEdit = (row: any) => {
  editTitle.value = t('message.onlineCenter.playerSegment.edit')
  editDialogRef.value.openDialog(row)
}

const onSyncOnline = (row: any) => {
  syncDialogRef.value.openDialog(row)
}

const onDelete = async (row: any) => {
  if (!canQueryServer.value) return
  const selectedVersion = serverSelectionVersion.value
  let deleted = false
  const confirmed = await showConfirm(
    t('message.common.message.deleteConfirmWithName', { name: row.name || row.id }),
    t('message.common.message.tip'),
    {
      type: 'danger',
      onAccept: async () => {
        if (!canQueryServer.value || selectedVersion !== serverSelectionVersion.value) return
        await gameOnlinePlayerSegmentDeletePost(row)
        deleted = true
      },
    },
  )
  if (!confirmed || !deleted || selectedVersion !== serverSelectionVersion.value) return
  showSuccess(t('message.common.message.deleteSuccess'))
  await onQuery()
}

const actions = {
  edit: openEdit,
  delete: onDelete,
}

const customActions: CrudAction[] = [
  {
    auth: 'syncOnline',
    label: t('message.onlineCenter.playerSegment.syncOnline'),
    icon: 'fa-solid fa-cloud-arrow-up',
    severity: 'info',
    onClick: onSyncOnline,
  },
]

const columns = computed<CrudColumnConfig[]>(() => [
  { field: 'name', header: t('message.onlineCenter.playerSegment.name'), width: '160px' },
  { field: 'description', header: t('common.label.description'), width: '160px', bodyKey: 'description' },
  { field: 'conditionsJson', header: t('message.onlineCenter.playerSegment.conditions'), width: '240px', bodyKey: 'conditionsJson' },
  { field: 'syncStatus', header: t('message.onlineCenter.playerSegment.syncStatus'), width: '110px', bodyKey: 'syncStatus' },
  { field: 'lastSyncTime', header: t('message.onlineCenter.playerSegment.lastSyncTime'), width: '170px', bodyKey: 'lastSyncTime' },
  { field: 'lastOnlineSegmentId', header: t('message.onlineCenter.playerSegment.lastOnlineSegmentId'), width: '150px', bodyKey: 'lastOnlineSegmentId' },
  { field: 'lastServerMessage', header: t('message.onlineCenter.playerSegment.lastServerMessage'), width: '200px', bodyKey: 'lastServerMessage' },
  { field: 'createdTime', header: t('common.label.createTime'), width: '170px', bodyKey: 'createdTime' },
])
watch(serverSelectionVersion, () => {
  tableParams.value.page = 1
  tableParams.value.total = 0
  tableData.value = []
  loading.value = false
  return onQuery()
})
onMounted(onQuery)
</script>
