<template>
  <div class="onlinePlayerSegment-container">
    <Card>
      <template #content>
        <SearchForm v-model="queryParams" :fields="searchFields" :showAdd="true"
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
        <editDialog ref="editDialogRef" :title="editTitle" :appId="queryParams.appId" @reloadTable="onQuery" />
        <syncOnlineDialog ref="syncDialogRef" @done="onQuery" />
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
import { ref, computed } from 'vue'
import { useRoute } from 'vue-router'
import Card from 'primevue/card'
import Tag from 'primevue/tag'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import SearchForm from '/@/components/search-form/index.vue'
import type { CrudAction, CrudColumnConfig } from '../../../../components/crud-data-table/types.ts'
import { formatUnix } from '../../../../utils/formatTime.ts'
import { useCrudTable } from '../../../../composables/useCrudTable.ts'
import { usePrimeTablePage, useRouteAppIdQuerySync } from '../../../../composables'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import editDialog from '/@/views/app/onlineCenter/onlinePlayerSegment/component/EditDialog.vue'
import syncOnlineDialog from '/@/views/app/onlineCenter/onlinePlayerSegment/component/SyncOnlineDialog.vue'
import { tenantGameOnlinePlayerSegmentPagePost, tenantGameOnlinePlayerSegmentDeletePost } from '../../../../api/tenantGameOnlinePlayerSegment'
import { OnlinePlayerSegmentSyncStatus } from '../../../../api/tenantGameOnlinePlayerSegment/types'
import { ONLINE_SEGMENT_FIELD_OPTIONS, ONLINE_SEGMENT_OP_OPTIONS } from './constants'

const route = useRoute()
const { resolveInitialAppId } = useCurrentApp()

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

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
  onDelete,
} = useCrudTable({
  api: { pageApi: tenantGameOnlinePlayerSegmentPagePost, deleteApi: tenantGameOnlinePlayerSegmentDeletePost },
  immediate: true,
})

const onPage = usePrimeTablePage(tableParams, onQuery)

useRouteAppIdQuerySync({
  route,
  queryParams,
  resolveAppId: resolveInitialAppId,
  onQuery,
})

const openAdd = () => {
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
</script>
