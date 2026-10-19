<template>
  <div class="tenantGameAdvertisementRewardEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :labelWidth="'110px'"
            authPrefix="tenantGameAdvertisementRewardEntity"
            @query="onQuery"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #header>
        <div class="flex justify-content-between align-items-center w-full">
          <span>{{ t('message.gameBusiness.advertisementReward.listTitle') }}</span>
          <Button v-auth="'tenantGameAdvertisementRewardEntity:export'" :label="t('message.gameBusiness.advertisementReward.export')" icon="fa-solid fa-file-export" severity="info" size="small" :loading="exporting" @click="onExport" />
        </div>
      </template>
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows rowKey="id"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="tenantGameAdvertisementRewardEntity"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage" showIdColumn :idColumnHeader="t('message.gameBusiness.advertisementReward.entityId')" idColumnWidth="140px">
          <template #body-provider="{ data }">
            <Tag :value="data.provider || '-'" />
          </template>
          <template #body-transactionId="{ data }">{{ data.transactionId || '-' }}</template>
          <template #body-advertisementUnit="{ data }">{{ data.advertisementUnit || '-' }}</template>
          <template #body-status="{ data }">
            <Tag :value="statusDesc(data.status)" :severity="adRewardStatusSeverity(data.status)" />
          </template>
          <template #body-rewardItem="{ data }">{{ data.rewardItem || '-' }}</template>
          <template #body-rewardAmount="{ data }">{{ data.rewardAmount || '-' }}</template>
          <template #body-areaId="{ data }">{{ getAreaName(data.areaId) }}</template>
          <template #body-isSandBox="{ data }">
            <Tag v-if="data.isSandBox" :value="t('message.gameBusiness.advertisementReward.sandboxTag')" severity="warn" />
            <Tag v-else :value="t('message.gameBusiness.advertisementReward.productionTag')" severity="secondary" />
          </template>
          <template #body-idempotencyKey="{ data }"><code class="text-sm break-all">{{ data.idempotencyKey }}</code></template>
          <template #body-failureReason="{ data }">
            <span v-if="data.failureReason" class="text-red-500">{{ data.failureReason }}</span>
            <span v-else class="text-color-secondary">-</span>
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>
      </template>
    </Card>
    <detailDialog ref="detailDialogRef" />
  </div>
</template>

<script lang="ts" setup name="tenantGameAdvertisementRewardEntity">
import {ref, computed, watch, onMounted} from 'vue'
import { useRoute } from 'vue-router'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import Card from 'primevue/card'
import SearchForm from '/@/components/search-form/index.vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import { formatUnix } from '../../../../utils/formatTime.ts'
import { tenantGameAdvertisementRewardEntityPagePost, tenantGameAdvertisementRewardEntityExportPost } from '../../../../api/tenantGameAdvertisementRewardEntity'
import { tenantGameAreaEntitySelectStringPost } from '../../../../api/tenantGameAreaEntity'
import {MAX_SELECT_SIZE} from '../../../../constants/pagination.ts'
import { useCrudTable } from '../../../../composables/useCrudTable.ts'
import { usePrimeTablePage } from '../../../../composables'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import { showConfirm, showSuccess, showWarning } from '../../../../utils/message.ts'
import { exportExcel } from '../../../../utils/exportExcel.ts'
import { t } from '../../../../i18n/index.ts'
import {
  ADVERTISEMENT_REWARD_STATUS_OPTIONS,
  ADVERTISEMENT_REWARD_PROVIDER_FALLBACK_OPTIONS,
  adRewardStatusSeverity,
} from './constants'

const route = useRoute()
const detailDialogRef = ref()
const exporting = ref(false)

const {resolveInitialAppId} = useCurrentApp()

const providerOptions = ref<{ label: string; value: string }[]>(ADVERTISEMENT_REWARD_PROVIDER_FALLBACK_OPTIONS)

const statusOptions = computed(() =>
  ADVERTISEMENT_REWARD_STATUS_OPTIONS.map((i) => ({ value: i.value, label: t(`message.gameBusiness.advertisementReward.${i.i18nKey}`) }))
)

const statusDesc = (v: number) => {
  const item = ADVERTISEMENT_REWARD_STATUS_OPTIONS.find((i) => i.value === v)
  return item ? t(`message.gameBusiness.advertisementReward.${item.i18nKey}`) : v
}

const searchFields = computed(() => [
  { key: 'provider', label: t('message.gameBusiness.advertisementReward.provider'), type: 'select', options: providerOptions.value, optionLabel: 'label', optionValue: 'value', placeholder: t('message.gameBusiness.advertisementReward.pleaseSelectProvider') },
  { key: 'status', label: t('message.gameBusiness.advertisementReward.status'), type: 'select', options: statusOptions.value, optionLabel: 'label', optionValue: 'value', placeholder: t('message.gameBusiness.advertisementReward.pleaseSelectStatus') },
  { key: 'roleId', label: t('message.gameBusiness.advertisementReward.roleId'), type: 'input', placeholder: t('message.gameBusiness.advertisementReward.pleaseEnterRoleId') },
  { key: 'transactionId', label: t('message.gameBusiness.advertisementReward.transactionId'), type: 'input', placeholder: t('message.gameBusiness.advertisementReward.pleaseEnterTransactionId'), advanced: true },
  { key: 'advertisementUnit', label: t('message.gameBusiness.advertisementReward.advertisementUnit'), type: 'input', placeholder: t('message.gameBusiness.advertisementReward.pleaseEnterAdvertisementUnit'), advanced: true },
  { key: 'areaId', label: t('message.gameBusiness.advertisementReward.areaId'), type: 'select', options: areaOptions.value, optionLabel: 'label', optionValue: 'id', placeholder: t('message.gameBusiness.advertisementReward.pleaseSelectArea'), advanced: true },
  { key: 'createTimeRange', label: t('message.gameBusiness.advertisementReward.timeRange'), type: 'dateRange', placeholder: t('message.gameBusiness.startTime'), advanced: true },
])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
} = useCrudTable({
  api: {
    pageApi: (params: any) => tenantGameAdvertisementRewardEntityPagePost(params),
  },
  immediate: false,
})

queryParams.value.appId = resolveInitialAppId(route.query)

const onPage = usePrimeTablePage(tableParams, onQuery)

const onDetail = (row: any) => {
  detailDialogRef.value?.openDialog(row)
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'provider', header: t('message.gameBusiness.advertisementReward.provider'), width: '120px', bodyKey: 'provider'},
  {field: 'transactionId', header: t('message.gameBusiness.advertisementReward.transactionId'), width: '180px', bodyKey: 'transactionId'},
  {field: 'advertisementUnit', header: t('message.gameBusiness.advertisementReward.advertisementUnit'), width: '130px', bodyKey: 'advertisementUnit'},
  {field: 'status', header: t('message.gameBusiness.advertisementReward.status'), width: '110px', bodyKey: 'status'},
  {field: 'rewardItem', header: t('message.gameBusiness.advertisementReward.rewardItem'), width: '120px', bodyKey: 'rewardItem'},
  {field: 'rewardAmount', header: t('message.gameBusiness.advertisementReward.rewardAmount'), width: '100px', bodyKey: 'rewardAmount'},
  {field: 'roleId', header: t('message.gameBusiness.advertisementReward.roleId'), width: '120px'},
  {field: 'userId', header: t('message.gameBusiness.advertisementReward.userId'), width: '110px'},
  {field: 'areaId', header: t('message.gameBusiness.advertisementReward.areaId'), width: '120px', bodyKey: 'areaId'},
  {field: 'isSandBox', header: t('message.gameBusiness.advertisementReward.isSandBox'), width: '90px', align: 'center', bodyKey: 'isSandBox'},
  {field: 'idempotencyKey', header: t('message.gameBusiness.advertisementReward.idempotencyKey'), width: '220px', bodyKey: 'idempotencyKey'},
  {field: 'failureReason', header: t('message.gameBusiness.advertisementReward.failureReason'), width: '200px', bodyKey: 'failureReason'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '160px', bodyKey: 'createdTime'},
])

const areaList = ref<any>([])
const areaOptions = computed(() =>
  areaList.value.map((item: any) => ({ id: item.id, label: `[${item.value}]-[${item.label}]` }))
)
const getAreaName = (areaId: number) => {
  for (let i = 0; i < areaList.value.length; i++) {
    const item = areaList.value[i]
    if (item.id == areaId) {
      return item.value + '-' + item.label
    }
  }
  return areaId
}

const loadAreaList = async (appId?: number) => {
  try {
    const res: any = await tenantGameAreaEntitySelectStringPost({ appId: appId, pageSize: MAX_SELECT_SIZE })
    areaList.value = res.data?.data ?? []
  } catch {
    areaList.value = []
  }
}

const onExport = async () => {
  const confirmed = await showConfirm(t('message.gameBusiness.advertisementReward.exportConfirm'), t('message.common.message.tip'))
  if (!confirmed) return
  exporting.value = true
  try {
    const params = Object.assign({}, queryParams.value, tableParams.value)
    const res: any = await tenantGameAdvertisementRewardEntityExportPost(params)
    const list = res.data?.data ?? []
    if (!list.length) {
      showWarning(t('message.gameBusiness.advertisementReward.exportEmpty'))
      return
    }
    const headers = [
      { label: t('message.gameBusiness.advertisementReward.provider'), prop: 'provider' },
      { label: t('message.gameBusiness.advertisementReward.transactionId'), prop: 'transactionId' },
      { label: t('message.gameBusiness.advertisementReward.advertisementUnit'), prop: 'advertisementUnit' },
      { label: t('message.gameBusiness.advertisementReward.status'), prop: 'status' },
      { label: t('message.gameBusiness.advertisementReward.rewardItem'), prop: 'rewardItem' },
      { label: t('message.gameBusiness.advertisementReward.rewardAmount'), prop: 'rewardAmount' },
      { label: t('message.gameBusiness.advertisementReward.roleId'), prop: 'roleId' },
      { label: t('message.gameBusiness.advertisementReward.userId'), prop: 'userId' },
      { label: t('message.gameBusiness.advertisementReward.areaId'), prop: 'areaId' },
      { label: t('message.gameBusiness.advertisementReward.idempotencyKey'), prop: 'idempotencyKey' },
      { label: t('message.gameBusiness.advertisementReward.failureReason'), prop: 'failureReason' },
      { label: t('common.label.createTime'), prop: 'createdTime' },
    ]
    exportExcel(list, t('message.gameBusiness.advertisementReward.exportFileName'), headers, t('message.gameBusiness.advertisementReward.sheetName'))
    showSuccess(t('message.common.message.success'))
  } finally {
    exporting.value = false
  }
}

watch(() => queryParams.value.appId, (newVal) => {
  queryParams.value.areaId = undefined
  loadAreaList(newVal)
})

watch(() => route.query.appId, async () => {
  queryParams.value.appId = resolveInitialAppId(route.query)
  queryParams.value.areaId = undefined
  await onQuery()
})

onMounted(async () => {
  await loadAreaList(queryParams.value.appId)
  onQuery()
})
</script>
