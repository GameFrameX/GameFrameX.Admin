<template>
  <div class="gameOrderFulfillmentEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :labelWidth="'110px'"
            authPrefix="gameOrderFulfillmentEntity"
            :defaultSearchKey="false"
            @query="onQuery"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #header>
        <div class="flex justify-content-between align-items-center w-full">
          <span>{{ t('message.gameBusiness.orderFulfillment.listTitle') }}</span>
          <Button v-auth="'gameOrderFulfillmentEntity:export'" :label="t('message.gameBusiness.orderFulfillment.export')" icon="fa-solid fa-file-export" severity="info" size="small" :loading="exporting" @click="onExport" />
        </div>
      </template>
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows rowKey="id"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="gameOrderFulfillmentEntity"
                       :actions="{ detail: (row) => onDetail(row) }"
                       :customActions="customActions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage" showIdColumn :idColumnHeader="t('message.gameBusiness.orderFulfillment.entityId')" idColumnWidth="140px">
          <template #body-type="{ data }">
            <Tag :value="typeDesc(data.type)" :severity="typeSeverity(data.type)" />
          </template>
          <template #body-status="{ data }">
            <Tag :value="statusDesc(data.status)" :severity="statusSeverity(data.status)" />
          </template>
          <template #body-providerTransactionId="{ data }">{{ data.providerTransactionId || '-' }}</template>
          <template #body-amount="{ data }"><span class="font-bold">{{ data.amount }}</span></template>
          <template #body-areaId="{ data }">{{ getAreaName(data.areaId) }}</template>
          <template #body-accountId="{ data }">{{ data.accountId || '-' }}</template>
          <template #body-itemsJson="{ data }">
            <div v-if="parseItems(data.itemsJson).length" class="flex flex-wrap gap-1">
              <Tag v-for="(item, index) in parseItems(data.itemsJson)" :key="index" severity="info">{{ item.id }} x{{ item.count }}</Tag>
            </div>
            <span v-else class="text-color-secondary">-</span>
          </template>
          <template #body-idempotencyKey="{ data }"><code class="text-sm">{{ data.idempotencyKey }}</code></template>
          <template #body-operatorId="{ data }">{{ operatorDesc(data.operatorId) }}</template>
          <template #body-failureReason="{ data }">
            <span v-if="data.failureReason" class="text-red-500">{{ data.failureReason }}</span>
            <span v-else class="text-color-secondary">-</span>
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>
      </template>
    </Card>
    <detailDialog ref="detailDialogRef" />
    <actionDialog ref="actionDialogRef" @done="onQuery" />
  </div>
</template>

<script lang="ts" setup name="gameOrderFulfillmentEntity">
import {ref, computed, watch, onMounted} from 'vue'
import { useRoute } from 'vue-router'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import Card from 'primevue/card'
import SearchForm from '/@/components/search-form/index.vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type { CrudAction } from '../../../../components/crud-data-table/types.ts'
import type { CrudColumnConfig } from '../../../../components/crud-data-table/types.ts'
import { formatUnix } from '../../../../utils/formatTime.ts'
import { createAction } from '../../../../components/crud-data-table'
import {parseRewardItems} from '../../../../components/reward-items-editor/rewardItems.ts'
import detailDialog from '/@/views/app/playerCenter/gameOrderFulfillmentEntity/component/DetailDialog.vue'
import actionDialog from '/@/views/app/playerCenter/gameOrderFulfillmentEntity/component/ActionDialog.vue'
import { tenantGameOrderFulfillmentEntityPagePost, tenantGameOrderFulfillmentEntityExportPost } from '../../../../api/tenantGameOrderFulfillmentEntity'
import { tenantGameAreaEntitySelectStringPost } from '../../../../api/tenantGameAreaEntity'
import {MAX_SELECT_SIZE} from '../../../../constants/pagination.ts'
import { useCrudTable } from '../../../../composables/useCrudTable.ts'
import { usePrimeTablePage } from '../../../../composables'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import { showConfirm, showSuccess, showWarning } from '../../../../utils/message.ts'
import { exportExcel } from '../../../../utils/exportExcel.ts'
import { t } from '../../../../i18n/index.ts'
import {
  FULFILLMENT_TYPE_OPTIONS,
  FULFILLMENT_STATUS_OPTIONS,
  FULFILLMENT_TYPE_VALUES,
  FULFILLMENT_STATUS_VALUES,
} from './constants'

const route = useRoute()
const detailDialogRef = ref()
const actionDialogRef = ref()
const exporting = ref(false)

const {resolveInitialAppId} = useCurrentApp()

const typeOptions = computed(() =>
  FULFILLMENT_TYPE_OPTIONS.map((i) => ({ value: i.value, label: t(`message.gameBusiness.orderFulfillment.${i.i18nKey}`) }))
)
const statusOptions = computed(() =>
  FULFILLMENT_STATUS_OPTIONS.map((i) => ({ value: i.value, label: t(`message.gameBusiness.orderFulfillment.${i.i18nKey}`) }))
)

const typeDesc = (v: number) => {
  const item = FULFILLMENT_TYPE_OPTIONS.find((i) => i.value === v)
  return item ? t(`message.gameBusiness.orderFulfillment.${item.i18nKey}`) : v
}
const statusDesc = (v: number) => {
  const item = FULFILLMENT_STATUS_OPTIONS.find((i) => i.value === v)
  return item ? t(`message.gameBusiness.orderFulfillment.${item.i18nKey}`) : v
}
const typeSeverity = (v: number) => {
  const map: Record<number, any> = { 1: 'info', 2: 'warn', 3: 'success' }
  return map[v] ?? undefined
}
const statusSeverity = (v: number) => {
  const map: Record<number, any> = { 1: 'warn', 2: 'success', 3: 'danger', 4: 'secondary' }
  return map[v] ?? undefined
}
const operatorDesc = (v: number) => (v === 0 ? t('message.gameBusiness.orderFulfillment.operatorSystem') : v)

const parseItems = (itemsJson: unknown) => parseRewardItems(itemsJson)

const searchFields = computed(() => [
  { key: 'type', label: t('message.gameBusiness.orderFulfillment.type'), type: 'select', options: typeOptions.value, optionLabel: 'label', optionValue: 'value', placeholder: t('message.gameBusiness.orderFulfillment.pleaseSelectType') },
  { key: 'status', label: t('message.gameBusiness.orderFulfillment.status'), type: 'select', options: statusOptions.value, optionLabel: 'label', optionValue: 'value', placeholder: t('message.gameBusiness.orderFulfillment.pleaseSelectStatus') },
  { key: 'roleId', label: t('message.gameBusiness.orderFulfillment.roleId'), type: 'input', placeholder: t('message.gameBusiness.orderFulfillment.pleaseEnterRoleId') },
  { key: 'orderId', label: t('message.gameBusiness.orderFulfillment.orderId'), type: 'input', placeholder: t('message.gameBusiness.orderFulfillment.pleaseEnterOrderId'), advanced: true },
  { key: 'payChannel', label: t('message.gameBusiness.orderFulfillment.payChannel'), type: 'input', placeholder: t('message.gameBusiness.orderFulfillment.pleaseEnterPayChannel'), advanced: true },
  { key: 'providerTransactionId', label: t('message.gameBusiness.orderFulfillment.providerTransactionId'), type: 'input', placeholder: t('message.gameBusiness.orderFulfillment.pleaseEnterProviderTransactionId'), advanced: true },
  { key: 'sku', label: t('message.gameBusiness.orderFulfillment.sku'), type: 'input', placeholder: t('message.gameBusiness.orderFulfillment.pleaseEnterSku'), advanced: true },
  { key: 'accountId', label: t('message.gameBusiness.orderFulfillment.accountId'), type: 'input', placeholder: t('message.gameBusiness.orderFulfillment.pleaseEnterAccountId'), advanced: true },
  { key: 'areaId', label: t('message.gameBusiness.orderFulfillment.areaId'), type: 'select', options: areaOptions.value, optionLabel: 'label', optionValue: 'id', placeholder: t('message.gameBusiness.orderFulfillment.pleaseSelectArea'), advanced: true },
  { key: 'createTimeRange', label: t('message.gameBusiness.orderFulfillment.timeRange'), type: 'dateRange', placeholder: t('message.gameBusiness.startTime'), advanced: true },
])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
} = useCrudTable({
  api: {
    pageApi: (params) => tenantGameOrderFulfillmentEntityPagePost(params),
  },
  immediate: false,
})

queryParams.value.appId = resolveInitialAppId(route.query)

const onPage = usePrimeTablePage(tableParams, onQuery)

const onDetail = (row: any) => {
  detailDialogRef.value?.openDialog(row)
}

const onRefund = (row: any) => {
  actionDialogRef.value?.openDialog('refund', row)
}
const onCompensate = (row: any) => {
  actionDialogRef.value?.openDialog('compensate', row)
}
const onResend = (row: any) => {
  actionDialogRef.value?.openDialog('resend', row)
}

// 退款：仅成功购买的账本可退（Purchase + Success）
const canRefund = (row: any) => row.type === FULFILLMENT_TYPE_VALUES.Purchase && row.status === FULFILLMENT_STATUS_VALUES.Success
// 重发：仅失败账本可重发（Failed）
const canResend = (row: any) => row.status === FULFILLMENT_STATUS_VALUES.Failed
// 人工补偿：不限制状态（任意行均可发起人工补偿）
const canCompensate = () => true

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'type', header: t('message.gameBusiness.orderFulfillment.type'), width: '110px', bodyKey: 'type'},
  {field: 'status', header: t('message.gameBusiness.orderFulfillment.status'), width: '110px', bodyKey: 'status'},
  {field: 'orderId', header: t('message.gameBusiness.orderFulfillment.orderId'), width: '130px'},
  {field: 'payChannel', header: t('message.gameBusiness.orderFulfillment.payChannel'), width: '100px'},
  {field: 'providerTransactionId', header: t('message.gameBusiness.orderFulfillment.providerTransactionId'), width: '160px', bodyKey: 'providerTransactionId'},
  {field: 'sku', header: t('message.gameBusiness.orderFulfillment.sku'), width: '90px'},
  {field: 'amount', header: t('message.gameBusiness.orderFulfillment.amountCents'), width: '100px', bodyKey: 'amount'},
  {field: 'roleId', header: t('message.gameBusiness.orderFulfillment.roleId'), width: '120px'},
  {field: 'areaId', header: t('message.gameBusiness.orderFulfillment.areaId'), width: '100px', bodyKey: 'areaId'},
  {field: 'accountId', header: t('message.gameBusiness.orderFulfillment.accountId'), width: '130px', bodyKey: 'accountId'},
  {field: 'itemsJson', header: t('message.gameBusiness.orderFulfillment.itemsJson'), width: '200px', bodyKey: 'itemsJson'},
  {field: 'idempotencyKey', header: t('message.gameBusiness.orderFulfillment.idempotencyKey'), width: '200px', bodyKey: 'idempotencyKey'},
  {field: 'operatorId', header: t('message.gameBusiness.orderFulfillment.operator'), width: '110px', bodyKey: 'operatorId'},
  {field: 'failureReason', header: t('message.gameBusiness.orderFulfillment.failureReason'), width: '200px', bodyKey: 'failureReason'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '160px', bodyKey: 'createdTime'},
])

const customActions: CrudAction[] = [
  createAction('resend', onResend, {
    label: t('message.gameBusiness.orderFulfillment.resend'),
    icon: 'fa-solid fa-rotate',
    severity: 'warn',
    visible: canResend,
  }),
  createAction('refund', onRefund, {
    label: t('message.gameBusiness.orderFulfillment.refund'),
    icon: 'fa-solid fa-rotate-left',
    severity: 'danger',
    visible: canRefund,
  }),
  createAction('compensate', onCompensate, {
    label: t('message.gameBusiness.orderFulfillment.compensate'),
    icon: 'fa-solid fa-gift',
    severity: 'success',
    visible: canCompensate,
  }),
]

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
  const res = await tenantGameAreaEntitySelectStringPost({ appId: appId, pageSize: MAX_SELECT_SIZE })
  areaList.value = res.data.data ?? []
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

const onExport = async () => {
  const confirmed = await showConfirm(t('message.gameBusiness.orderFulfillment.exportConfirm'), t('message.common.message.tip'))
  if (!confirmed) return
  exporting.value = true
  try {
    const params = Object.assign({}, queryParams.value, tableParams.value)
    const res = await tenantGameOrderFulfillmentEntityExportPost(params)
    const list = res.data?.data ?? []
    if (!list.length) {
      showWarning(t('message.gameBusiness.orderFulfillment.exportEmpty'))
      return
    }
    const headers = [
      { label: t('message.gameBusiness.orderFulfillment.orderId'), prop: 'orderId' },
      { label: t('message.gameBusiness.orderFulfillment.type'), prop: 'type' },
      { label: t('message.gameBusiness.orderFulfillment.status'), prop: 'status' },
      { label: t('message.gameBusiness.orderFulfillment.payChannel'), prop: 'payChannel' },
      { label: t('message.gameBusiness.orderFulfillment.providerTransactionId'), prop: 'providerTransactionId' },
      { label: t('message.gameBusiness.orderFulfillment.sku'), prop: 'sku' },
      { label: t('message.gameBusiness.orderFulfillment.amountCents'), prop: 'amount' },
      { label: t('message.gameBusiness.orderFulfillment.roleId'), prop: 'roleId' },
      { label: t('message.gameBusiness.orderFulfillment.areaId'), prop: 'areaId' },
      { label: t('message.gameBusiness.orderFulfillment.accountId'), prop: 'accountId' },
      { label: t('message.gameBusiness.orderFulfillment.idempotencyKey'), prop: 'idempotencyKey' },
      { label: t('message.gameBusiness.orderFulfillment.operator'), prop: 'operatorId' },
      { label: t('common.label.createTime'), prop: 'createdTime' },
    ]
    exportExcel(list, t('message.gameBusiness.orderFulfillment.exportFileName'), headers, t('message.gameBusiness.orderFulfillment.sheetName'))
    showSuccess(t('message.common.message.success'))
  } finally {
    exporting.value = false
  }
}

watch(() => queryParams.value.appId, (newVal) => {
  queryParams.value.areaId = undefined
  loadAreaList(newVal)
})

onMounted(async () => {
  await loadAreaList(queryParams.value.appId)
  onQuery()
})
</script>
