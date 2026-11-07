<template>
  <div class="onlineConfigRollout-container">
    <Card>
      <template #content>
        <div class="flex align-items-center gap-2 mb-3">
          <ServerSelector mode="local" v-model="serverIdOverride" />
          <button v-if="serverIdOverride !== undefined" type="button" class="p-button p-button-text" @click="serverIdOverride = undefined">
            {{ t('message.common.button.reset') }}
          </button>
        </div>
        <p v-if="!canQueryServer" role="status">{{ t(serverStateMessage) }}</p>
        <SearchForm v-model="queryParams" :fields="searchFields" :showAdd="false"
                    authPrefix="onlineConfigRollout" :defaultSearchKey="false"
                    @query="onQuery" />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #header>
        <div class="flex justify-content-between align-items-center w-full">
          <span>{{ t('message.onlineCenter.configRollout.listTitle') }}</span>
          <div class="flex gap-2">
            <Button class="onlineConfigRollout-publish-btn" v-auth="'onlineConfigRollout:publish'"
                    :label="t('message.onlineCenter.configRollout.publish')" icon="fa-solid fa-paper-plane"
                    severity="warn" size="small" :disabled="!canQueryServer" @click="publishDialogRef?.openDialog()" />
          </div>
        </div>
      </template>
      <template #content>
        <CrudDataTable :value="tableData" style="width: 100%" v-block-loading="loading" showGridlines stripedRows
                       rowKey="id" :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="onlineConfigRollout"
                       :actions="actions"
                       :customActions="customActions"
                       :columns="columns"
                       @page="onPage" showIdColumn idColumnWidth="180px">
          <template #body-configKind="{ data }">
            <Tag :value="configKindDesc(data.configKind)" />
          </template>
          <template #body-status="{ data }">
            <Tag :value="statusDesc(data.status)" :severity="statusSeverity(data.status)" />
          </template>
          <template #body-targetVersion="{ data }">
            {{ data.targetVersion > 0 ? data.targetVersion : '-' }}
          </template>
          <template #body-onlineConfirmVersion="{ data }">
            <span v-if="data.onlineConfirmVersion">{{ data.onlineConfirmVersion }}</span>
            <span v-else>-</span>
          </template>
          <template #body-percent="{ data }">{{ data.percent }}%</template>
          <template #body-publishedTime="{ data }">{{ formatUnix(data.publishedTime) }}</template>
        </CrudDataTable>
        <publishDialog v-if="canQueryServer" :key="'publishDialog-' + serverSelectionVersion" :serverId="effectiveServerId" ref="publishDialogRef" @done="onQuery" />
        <rollbackDialog v-if="canQueryServer" :key="'rollbackDialog-' + serverSelectionVersion" :serverId="effectiveServerId" ref="rollbackDialogRef" @done="onQuery" />
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="onlineConfigRollout">
/**
 * 配置灰度发布页（前端半边）。
 * 版本行只读列表（只由 publish/rollback 追加，历史行不可改删）；publish 顶部高危入口；
 * rollback 行操作（customActions 相对码 onlineConfigRollout:rollback）。
 * Admin 只编排（SegmentId + Percent 1-100），不解释分群命中与百分比桶（Online 侧语义）。
 */
import { ref, computed, watch, onMounted } from 'vue'
import ServerSelector from '/@/components/server-selector/ServerSelector.vue'
import { usePageServer } from '../components/usePageServer'
import Card from 'primevue/card'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import SearchForm from '/@/components/search-form/index.vue'
import type { CrudAction, CrudColumnConfig } from '../../../components/crud-data-table/types.ts'
import { formatUnix } from '../../../utils/formatTime.ts'
import { DEFAULT_PAGE_SIZE } from '../../../constants/pagination'
import { normalizeRowVersion } from '../../../utils/rowVersion'
import { usePrimeTablePage } from '../../../composables'
import publishDialog from '/@/views/gmCenter/onlineConfigRollout/component/PublishDialog.vue'
import rollbackDialog from '/@/views/gmCenter/onlineConfigRollout/component/RollbackDialog.vue'
import { gameOnlineConfigRolloutPagePost } from '../../../api/gameOnlineConfigRollout'
import { OnlineConfigRolloutStatus } from '../../../api/gameOnlineConfigRollout/types'
import { ONLINE_CONFIG_ROLLOUT_KIND_OPTIONS } from './constants'

const publishDialogRef = ref()
const rollbackDialogRef = ref()

const configKindDesc = (v: number) => {
  const item = ONLINE_CONFIG_ROLLOUT_KIND_OPTIONS.find((i) => i.value === v)
  return item ? t(item.labelKey) : v
}

const statusDesc = (v: number) => v === OnlineConfigRolloutStatus.Published
  ? t('message.onlineCenter.configRollout.statusPublished')
  : t('message.onlineCenter.configRollout.statusRolledBack')

const statusSeverity = (v: number): 'success' | 'secondary' => v === OnlineConfigRolloutStatus.Published ? 'success' : 'secondary'

const searchFields = computed(() => [
  { key: 'configKind', label: t('message.onlineCenter.configRollout.configKind'), type: 'select', options: buildKindOptions(), optionLabel: 'label', optionValue: 'value', placeholder: t('message.onlineCenter.configRollout.configKind') },
  { key: 'configKey', label: t('message.onlineCenter.configRollout.configKey'), type: 'input', placeholder: t('message.onlineCenter.configRollout.configKey'), advanced: true },
  { key: 'status', label: t('message.onlineCenter.configRollout.status'), type: 'select', options: buildStatusOptions(), optionLabel: 'label', optionValue: 'value', placeholder: t('message.onlineCenter.configRollout.status'), advanced: true },
])

const buildKindOptions = () => ONLINE_CONFIG_ROLLOUT_KIND_OPTIONS.map((o) => ({ value: o.value, label: t(o.labelKey) }))

const buildStatusOptions = () => ([
  { value: OnlineConfigRolloutStatus.Published, label: t('message.onlineCenter.configRollout.statusPublished') },
  { value: OnlineConfigRolloutStatus.RolledBack, label: t('message.onlineCenter.configRollout.statusRolledBack') },
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
    const response = await gameOnlineConfigRolloutPagePost({ ...queryParams.value, ...tableParams.value, serverId: request.serverId })
    if (!request.isCurrent()) return
    tableData.value = (response.data?.data?.items ?? []).map(item => normalizeRowVersion(item))
    tableParams.value.total = response.data?.data?.total ?? 0
  } finally {
    if (request.isCurrent()) loading.value = false
  }
}

const onPage = usePrimeTablePage(tableParams, onQuery)

const onRollback = (row: any) => {
  rollbackDialogRef.value.openDialog(row)
}

// 版本行无编辑/删除（不可变审计历史）；仅回滚行操作
const actions = {}

const customActions: CrudAction[] = [
  {
    auth: 'rollback',
    label: t('message.onlineCenter.configRollout.rollback'),
    icon: 'fa-solid fa-rotate-left',
    severity: 'danger',
    onClick: onRollback,
    visible: (row: any) => row.status === OnlineConfigRolloutStatus.Published,
  },
]

const columns = computed<CrudColumnConfig[]>(() => [
  { field: 'configKind', header: t('message.onlineCenter.configRollout.configKind'), width: '110px', bodyKey: 'configKind' },
  { field: 'configKey', header: t('message.onlineCenter.configRollout.configKey'), width: '120px' },
  { field: 'configVersion', header: t('message.onlineCenter.configRollout.configVersion'), width: '100px' },
  { field: 'segmentId', header: t('message.onlineCenter.configRollout.segmentId'), width: '110px' },
  { field: 'percent', header: t('message.onlineCenter.configRollout.percent'), width: '90px', bodyKey: 'percent' },
  { field: 'status', header: t('message.onlineCenter.configRollout.status'), width: '100px', bodyKey: 'status' },
  { field: 'targetVersion', header: t('message.onlineCenter.configRollout.targetVersion'), width: '110px', bodyKey: 'targetVersion' },
  { field: 'onlineConfirmVersion', header: t('message.onlineCenter.configRollout.onlineConfirmVersion'), width: '150px', bodyKey: 'onlineConfirmVersion' },
  { field: 'reason', header: t('message.onlineCenter.configRollout.reason'), width: '180px' },
  { field: 'dispatchOperator', header: t('message.onlineCenter.configRollout.dispatchOperator'), width: '120px' },
  { field: 'publishedTime', header: t('message.onlineCenter.configRollout.publishedTime'), width: '170px', bodyKey: 'publishedTime' },
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
