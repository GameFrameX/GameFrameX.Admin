<template>
  <div class="gameRedeemCodeUseRecordEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            authPrefix="gameRedeemCodeUseRecordEntity"
            :defaultSearchKey="false"
            @query="onQuery"

        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" style="width: 100%" v-block-loading="loading" showGridlines stripedRows
                       rowKey="id" :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage" showIdColumn idColumnWidth="180px">
          <template #body-areaId="{ data }">
            {{ getAreaName(data.areaId) }}
          </template>
          <template #body-channelId="{ data }">
            {{ getRedeemCodeChannelName(data) }}
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="gameRedeemCodeUseRecordEntity">
import {ref, computed, watch, onMounted} from 'vue'
import {useRoute} from 'vue-router'
import { tenantGameRedeemCodeUseRecordEntityPagePost } from '../../../../api/tenantGameRedeemCodeUseRecordEntity'
import { tenantGameRedeemCodeChannelEntitySelectStringPost } from '../../../../api/tenantGameRedeemCodeChannelEntity'
import { tenantGameAreaEntitySelectStringPost } from '../../../../api/tenantGameAreaEntity'
import {MAX_SELECT_SIZE} from '../../../../constants/pagination.ts'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type { CrudColumnConfig } from '../../../../components/crud-data-table/types.ts'
import { formatUnix } from '../../../../utils/formatTime.ts'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'
import {useCurrentApp} from '../../../../composables/useCurrentApp.ts'

const route = useRoute()
const {resolveInitialAppId} = useCurrentApp()

const searchFields = computed(() => [
  { key: 'areaId', label: t('message.gameBusiness.role.areaName'), type: 'select', options: areaList.value, optionLabel: 'value', optionValue: 'id', placeholder: t('message.gameBusiness.order.pleaseSelectArea') },
  { key: 'channelId', label: t('message.gameBusiness.redeemcode.channelId'), type: 'select', options: redeemCodeChannelList.value, optionLabel: 'value', optionValue: 'id', placeholder: t('message.gameBusiness.redeemcode.pleaseSelectChannelId'), advanced: true },
  { key: 'code', label: t('message.gameBusiness.redeemcode.code'), type: 'input', placeholder: t('message.gameBusiness.redeemcode.pleaseEnterCode'), advanced: true },
  { key: 'playerId', label: t('message.gameBusiness.redeemcode.playerId'), type: 'input', placeholder: t('message.gameBusiness.redeemcode.pleaseEnterPlayerId'), advanced: true },
])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
} = useCrudTable({
  api: {
    pageApi: (params) => tenantGameRedeemCodeUseRecordEntityPagePost(params),
  },
  immediate: true,
})

queryParams.value.appId = resolveInitialAppId(route.query)

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'areaId', header: t('message.gameBusiness.role.areaName'), width: '200px', bodyKey: 'areaId'},
  {field: 'code', header: t('message.gameBusiness.redeemcode.code'), width: '300px', sensitive: true},
  {field: 'channelId', header: t('message.gameBusiness.redeemcode.channelId'), width: '150px', bodyKey: 'channelId'},
  {field: 'playerIdString', header: t('message.gameBusiness.redeemcode.playerId'), width: '200px'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '200px', bodyKey: 'createdTime'},
])

const onPage = usePrimeTablePage(tableParams, onQuery)

const areaList = ref<any>([])
const getAreaName = (areaId: number) => {
  for (let i = 0; i < areaList.value.length; i++) {
    const item = areaList.value[i]
    if (item.id == areaId) {
      return item.label
    }
  }
  return t('message.gameBusiness.redeemcode.allServer')
}

const loadAreaList = async (appId?: number) => {
  const res = await tenantGameAreaEntitySelectStringPost({appId: appId, pageSize: MAX_SELECT_SIZE})
  areaList.value = res.data.data ?? []
}

// Redeem code channel list (reuse list page pattern)
const redeemCodeChannelList = ref<any>([])
const getRedeemCodeChannelName = (row: any) => {
  if (row?.channelName) {
    return row.channelName
  }
  const channelId = row?.channelId
  for (let i = 0; i < redeemCodeChannelList.value.length; i++) {
    const item = redeemCodeChannelList.value[i]
    if (item.id == channelId) {
      return item.label
    }
  }
  return t('message.gameBusiness.redeemcode.channelNotFound')
}
const loadChannelList = async (appId?: number) => {
  const res = await tenantGameRedeemCodeChannelEntitySelectStringPost({appId: appId, pageSize: MAX_SELECT_SIZE})
  redeemCodeChannelList.value = res.data.data ?? []
}

watch(() => queryParams.value.appId, (newVal) => {
  queryParams.value.areaId = undefined
  queryParams.value.channelId = undefined
  loadAreaList(newVal)
  loadChannelList(newVal)
})

watch(() => route.query.appId, async () => {
  queryParams.value.appId = resolveInitialAppId(route.query)
  queryParams.value.areaId = undefined
  queryParams.value.channelId = undefined
  await onQuery()
})

onMounted(async () => {
  await Promise.all([
    loadAreaList(queryParams.value.appId),
    loadChannelList(queryParams.value.appId),
  ])
})
</script>
