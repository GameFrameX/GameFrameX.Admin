<template>
  <div class="onlineConfigRollout-container">
    <Card>
      <template #content>
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
                    severity="warn" size="small" @click="publishDialogRef?.openDialog()" />
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
        <publishDialog ref="publishDialogRef" :appId="currentAppId" @done="onQuery" />
        <rollbackDialog ref="rollbackDialogRef" :appId="currentAppId" @done="onQuery" />
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="onlineConfigRollout">
/**
 * 配置灰度发布页（C343 / GFX-759 前端半边）。
 * 版本行只读列表（只由 publish/rollback 追加，历史行不可改删）；publish 顶部高危入口；
 * rollback 行操作（customActions 相对码 onlineConfigRollout:rollback）。
 * Admin 只编排（SegmentId + Percent 1-100），不解释分群命中与百分比桶（Online 侧语义）。
 */
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import Card from 'primevue/card'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import SearchForm from '/@/components/search-form/index.vue'
import type { CrudAction, CrudColumnConfig } from '../../../../components/crud-data-table/types.ts'
import { formatUnix } from '../../../../utils/formatTime.ts'
import { useCrudTable } from '../../../../composables/useCrudTable.ts'
import { usePrimeTablePage } from '../../../../composables'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import publishDialog from '/@/views/app/onlineCenter/onlineConfigRollout/component/PublishDialog.vue'
import rollbackDialog from '/@/views/app/onlineCenter/onlineConfigRollout/component/RollbackDialog.vue'
import { tenantGameOnlineConfigRolloutPagePost } from '../../../../api/tenantGameOnlineConfigRollout'
import { OnlineConfigRolloutStatus } from '../../../../api/tenantGameOnlineConfigRollout/types'
import { ONLINE_CONFIG_ROLLOUT_KIND_OPTIONS } from './constants'

const route = useRoute()
const { currentAppId, resolveInitialAppId } = useCurrentApp()

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

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
} = useCrudTable({
  api: { pageApi: tenantGameOnlineConfigRolloutPagePost },
  immediate: true,
})

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

watch(() => route.query.appId, async () => {
  currentAppId.value = resolveInitialAppId(route.query)
  queryParams.value.appId = currentAppId.value
  await onQuery()
})

onMounted(async () => {
  if (!currentAppId.value) {
    currentAppId.value = resolveInitialAppId(route.query)
  }
  queryParams.value.appId = currentAppId.value
  await onQuery()
})
</script>
