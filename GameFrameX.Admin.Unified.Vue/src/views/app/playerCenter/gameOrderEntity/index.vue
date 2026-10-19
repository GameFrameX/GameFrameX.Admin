<template>
  <div class="gameOrderEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :labelWidth="'110px'"
            authPrefix="gameOrderEntity"
            :defaultSearchKey="false"
            @query="onQuery"

        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #header>
        <div class="flex justify-content-between align-items-center w-full">
          <span>{{ t('message.gameBusiness.order.orderSummary') }}</span>
          <Button :label="t('message.common.button.refresh')" icon="fa-solid fa-arrows-rotate" text size="small" @click="fetchSummary" />
        </div>
      </template>
      <template #content>
        <div class="grid grid-cols-4" style="gap: 1rem;">
          <div class="surface-100 p-3 border-round">
            <div class="text-sm text-color-secondary mb-1">{{ t('message.gameBusiness.order.totalAmount') }}</div>
            <div class="text-2xl font-bold text-primary">{{ summaryData.totalAmount }}</div>
          </div>
          <div class="surface-100 p-3 border-round">
            <div class="text-sm text-color-secondary mb-1">{{ t('message.gameBusiness.order.paidAmount') }}</div>
            <div class="text-2xl font-bold text-green-500">{{ summaryData.paidAmount }}</div>
          </div>
          <div class="surface-100 p-3 border-round">
            <div class="text-sm text-color-secondary mb-1">{{ t('message.gameBusiness.order.totalCount') }}</div>
            <div class="text-2xl font-bold">{{ summaryData.totalCount }}</div>
          </div>
          <div class="surface-100 p-3 border-round">
            <div class="text-sm text-color-secondary mb-1">{{ t('message.gameBusiness.order.paidCount') }}</div>
            <div class="text-2xl font-bold text-green-500">{{ summaryData.paidCount }}</div>
          </div>
        </div>
        <div class="grid mt-3 grid-cols-2" style="gap: 1rem;">
          <div class="surface-card p-3 border-round">
            <h5 class="mb-2">{{ t('message.gameBusiness.order.channelBreakdown') }}</h5>
            <DataTable :value="summaryData.payChannelBreakdown" size="small" stripedRows>
              <Column field="payChannel" :header="t('message.gameBusiness.order.payChannel')" />
              <Column field="amount" :header="t('message.gameBusiness.order.totalAmount')" />
              <Column field="count" :header="t('message.gameBusiness.order.count')" />
            </DataTable>
          </div>
          <div class="surface-card p-3 border-round">
            <h5 class="mb-2">{{ t('message.gameBusiness.order.statusBreakdown') }}</h5>
            <DataTable :value="summaryData.statusBreakdown" size="small" stripedRows>
              <Column field="status" :header="t('message.gameBusiness.order.orderStatus')">
                <template #body="{ data }">
                  <Tag :value="getEnumDesc(data.status, getEnumStatusData_Index)" />
                </template>
              </Column>
              <Column field="amount" :header="t('message.gameBusiness.order.totalAmount')" />
              <Column field="count" :header="t('message.gameBusiness.order.count')" />
            </DataTable>
          </div>
        </div>
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows rowKey="id"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="gameOrderEntity"
                       :customActions="customActions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage" showIdColumn :idColumnHeader="t('message.gameBusiness.order.orderId')" idColumnWidth="160px">
          <template #body-status="{ data }">
            <Tag :value="getEnumDesc(data.status, getEnumStatusData_Index)" :severity="data.status === 3 ? undefined : 'danger'" />
          </template>
          <template #body-amount="{ data }">
            <span class="font-bold">{{ data.amount }}</span>
          </template>
          <template #body-areaId="{ data }">
            {{ getAreaName(data.areaId) }}
          </template>
          <template #body-content="{ data }">
            <div v-if="parseContentRewards(data.content).length" class="flex flex-wrap gap-1">
              <Tag v-for="(item, index) in parseContentRewards(data.content)" :key="index" severity="info">
                {{ item.id }} x{{ item.count }}
              </Tag>
            </div>
            <span v-else class="text-color-secondary">-</span>
          </template>
          <template #body-isSandbox="{ data }">
            <Tag :value="data.isSandbox ? t('message.gameBusiness.yes') : t('message.gameBusiness.no')" :severity="data.isSandbox ? 'warn' : undefined" />
          </template>
          <template #body-roleRegisterTime="{ data }">{{ formatUnix(data.roleRegisterTime) }}</template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editGameOrderEntityTitle" :appId="queryParams.appId" @reloadTable="onQuery" />
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="gameOrderEntity">
import {ref, computed, watch, onMounted} from 'vue'
import { useRoute } from 'vue-router'
import Button from 'primevue/button'
import { showConfirm, showSuccess } from '../../../../utils/message.ts'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type { CrudAction } from '../../../../components/crud-data-table/types.ts'
import type { CrudColumnConfig } from '../../../../components/crud-data-table/types.ts'
import { formatUnix } from '../../../../utils/formatTime.ts'
import { createAction } from '../../../../components/crud-data-table'
import {parseRewardItems} from '../../../../components/reward-items-editor/rewardItems.ts'
import editDialog from '/@/views/app/playerCenter/gameOrderEntity/component/EditDialog.vue'
import { tenantGameOrderEntityPagePost, tenantGameOrderEntitySendPost, tenantGameOrderEntitySummaryPost } from '../../../../api/tenantGameOrderEntity'
import { systemEnumEnumDataListGet } from '../../../../api/systemEnum';
import commonFunction from '../../../../utils/commonFunction.ts'
import { tenantGameAreaEntitySelectStringPost } from '../../../../api/tenantGameAreaEntity'
import {MAX_SELECT_SIZE} from '../../../../constants/pagination.ts'
import { globalPlatformSelectPost } from '../../../../api/globalPlatform'
import { tenantChannelDefinitionAvailableGet } from '../../../../api/tenantChannelDefinition'
import { useCrudTable } from '../../../../composables/useCrudTable.ts'
import { usePrimeTablePage } from '../../../../composables'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import { mapAvailableChannelDefinitions } from '../../../../utils/channelDefinition.ts'

const route = useRoute()
// eslint-disable-next-line @typescript-eslint/naming-convention -- TODO(命名治理): 存量 PascalCase 命名（枚举映射表/C# 风格函数式命名），待配套治理 issue 改名
const getEnumStatusData_Index = ref<any>([])

const { getEnumDesc } = commonFunction()
const editDialogRef = ref()
const getEnumChannelData = ref<any>([])
const getEnumPlatformData = ref<any>([])
const editGameOrderEntityTitle = ref('')

const {resolveInitialAppId} = useCurrentApp()

const enumStatusOptions = computed(() =>
  (getEnumStatusData_Index.value || []).map((item: any) => ({ label: `[${item.value}] ${item.describe}`, value: item.value }))
)
const enumChannelOptions = computed(() =>
  (getEnumChannelData.value || []).map((item: any) => ({ label: `[${item.value}]-[${item.label}]`, value: item.value }))
)
const enumPlatformOptions = computed(() =>
  (getEnumPlatformData.value || []).map((item: any) => ({ label: `[${item.value}]-[${item.label}]`, value: item.value }))
)
const sandboxOptions = computed(() => [
  { label: t('message.gameBusiness.yes'), value: true },
  { label: t('message.gameBusiness.no'), value: false },
])

const searchFields = computed(() => [
  { key: 'status', label: t('message.gameBusiness.order.orderStatus'), type: 'select', options: enumStatusOptions.value, optionLabel: 'label', optionValue: 'value', placeholder: t('message.gameBusiness.order.pleaseSelectOrderStatus') },
  { key: 'roleId', label: t('message.gameBusiness.order.user'), type: 'input', placeholder: t('message.gameBusiness.redeemcode.pleaseEnterPlayerId') },
  { key: 'amount', label: t('message.gameBusiness.order.moneyAmount'), type: 'number', placeholder: t('message.gameBusiness.order.pleaseEnterMoneyAmount') },
  { key: 'areaId', label: t('message.gameBusiness.role.areaName'), type: 'select', options: areaOptions.value, optionLabel: 'label', optionValue: 'id', placeholder: t('message.gameBusiness.order.pleaseSelectArea'), advanced: true },
  { key: 'orderId', label: t('message.gameBusiness.order.thirdPartyOrderId'), type: 'input', placeholder: t('message.gameBusiness.order.pleaseEnterThirdPartyOrderId'), advanced: true },
  { key: 'sku', label: t('message.gameBusiness.order.commodity'), type: 'input', placeholder: t('message.gameBusiness.order.pleaseEnterCommodity'), advanced: true },
  { key: 'payChannel', label: t('message.gameBusiness.order.payChannel'), type: 'input', placeholder: t('message.gameBusiness.order.pleaseEnterPayChannel'), advanced: true },
  { key: 'channel', label: t('message.gameBusiness.order.channelDefinition'), type: 'select', options: enumChannelOptions.value, optionLabel: 'label', optionValue: 'value', placeholder: t('message.gameBusiness.order.channelDefinition'), advanced: true },
  { key: 'platform', label: t('message.gameBusiness.order.globalPlatform'), type: 'select', options: enumPlatformOptions.value, optionLabel: 'label', optionValue: 'value', placeholder: t('message.gameBusiness.order.globalPlatform'), advanced: true },
  { key: 'isSandbox', label: t('message.gameBusiness.isSandbox'), type: 'select', options: sandboxOptions.value, optionLabel: 'label', optionValue: 'value', placeholder: t('message.gameBusiness.order.pleaseSelectArea'), advanced: true },
  { key: 'accountId', label: t('message.gameBusiness.order.account'), type: 'input', placeholder: t('message.gameBusiness.order.account'), advanced: true },
  { key: 'createTimeRange', label: t('message.gameBusiness.order.timeRange'), type: 'dateRange', placeholder: t('message.gameBusiness.startTime'), advanced: true },
])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery: doQuery,
  onReset
} = useCrudTable({
  api: {
    pageApi: (params) => tenantGameOrderEntityPagePost(params),
  },
  immediate: false,
})

queryParams.value.appId = resolveInitialAppId(route.query)

const onPage = usePrimeTablePage(tableParams, doQuery)

const summaryData = ref<any>({
  totalAmount: 0,
  paidAmount: 0,
  totalCount: 0,
  paidCount: 0,
  payChannelBreakdown: [],
  statusBreakdown: [],
})

const fetchSummary = async () => {
  try {
    const params = Object.assign({}, queryParams.value, tableParams.value)
    const res = await tenantGameOrderEntitySummaryPost(params)
    summaryData.value = res.data?.data ?? summaryData.value
  } catch {
    // Non-critical
  }
}

// content 是字符串载荷，内部 key 为 PascalCase（{RewardsItem: [{Id, Count}]}），渲染前归一为 camelCase
const parseContentRewards = (content: unknown) => {
  if (!content) return []
  try {
    const parsed = typeof content === 'string' ? JSON.parse(content) : content
    return parseRewardItems((parsed as any)?.RewardsItem)
  } catch {
    return []
  }
}

const onQuery = async () => {
  await doQuery()
  for (const valueKey in tableData.value) {
    const item = tableData.value[valueKey]
    item.content = JSON.parse(item.content)
  }
  getEnumStatusData_Index.value = (await systemEnumEnumDataListGet({ enumName: 'OrderStatusEnum' })).data.data ?? []
  fetchSummary()
}

const onSendGameOrderEntity = async (row: any) => {
  const confirmed = await showConfirm(t('message.gameBusiness.order.confirmDelivery'), t('message.common.message.tip'))
  if (!confirmed) return
  await tenantGameOrderEntitySendPost(row)
  onQuery()
  showSuccess(t('message.common.message.success'))
}

const sendShowButton = (row: any) => {
  for (let valueKey in getEnumStatusData_Index.value) {
    const enumItem = getEnumStatusData_Index.value[valueKey]
    if (enumItem && row.status == enumItem.value && enumItem.name == 'NotShipped') {
      return true
    }
  }
  return false
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'status', header: t('message.gameBusiness.order.orderStatus'), width: '100px', bodyKey: 'status'},
  {field: 'amount', header: t('message.gameBusiness.order.moneyAmountCents'), width: '100px', bodyKey: 'amount'},
  {field: 'areaId', header: t('message.gameBusiness.role.areaName'), width: '140px', bodyKey: 'areaId'},
  {field: 'roleId', header: t('message.gameBusiness.redeemcode.playerId'), width: '120px'},
  {field: 'accountId', header: t('message.gameBusiness.user.userId'), width: '140px'},
  {field: 'content', header: t('message.gameBusiness.order.orderContent'), width: '200px', bodyKey: 'content'},
  {field: 'payChannel', header: t('message.gameBusiness.order.payChannel'), width: '100px'},
  {field: 'sku', header: t('message.gameBusiness.order.commoditySku'), width: '100px'},
  {field: 'platform', header: t('message.gameBusiness.order.globalPlatform'), width: '80px'},
  {field: 'channel', header: t('message.gameBusiness.order.channelDefinition'), width: '100px'},
  {field: 'isSandbox', header: t('message.gameBusiness.order.isSandboxEnv'), width: '80px', bodyKey: 'isSandbox'},
  {field: 'adventureSuffix', header: t('message.gameBusiness.order.adventureSuffix'), width: '80px'},
  {field: 'orderId', header: t('message.gameBusiness.order.thirdPartyOrderId'), width: '140px'},
  {field: 'subChannel', header: t('message.gameBusiness.order.subChannel'), width: '100px'},
  {field: 'roleRegisterTime', header: t('message.gameBusiness.order.roleRegisterTime'), width: '140px', bodyKey: 'roleRegisterTime'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '160px', bodyKey: 'createdTime'},
])

const customActions: CrudAction[] = [
  createAction('send', onSendGameOrderEntity, {
    label: t('message.gameBusiness.delivery'),
    icon: 'fa-solid fa-cart-shopping',
    visible: sendShowButton,
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

const onResetAndSummary = () => {
  onReset()
  fetchSummary()
}

// Reset dependent filter selections when current app changes (also reload dropdowns by app)
watch(() => queryParams.value.appId, (newVal) => {
  queryParams.value.areaId = undefined
  loadAreaList(newVal)
  // channel/platform are tenant/global scope; reload dropdown candidates for consistency
  tenantChannelDefinitionAvailableGet({pageSize: MAX_SELECT_SIZE}).then((res: any) => {
    getEnumChannelData.value = mapAvailableChannelDefinitions((res.data.data || []), 'code')
  })
})

watch(() => route.query.appId, async () => {
  queryParams.value.appId = resolveInitialAppId(route.query)
  queryParams.value.areaId = undefined
  await onQuery()
})

onMounted(async () => {
  await loadAreaList(queryParams.value.appId)
  getEnumChannelData.value = mapAvailableChannelDefinitions((await tenantChannelDefinitionAvailableGet({pageSize: MAX_SELECT_SIZE})).data.data || [], 'code')
  getEnumPlatformData.value = (await globalPlatformSelectPost({pageSize: MAX_SELECT_SIZE})).data.data ?? []
  onQuery()
})
</script>
