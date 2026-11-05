<template>
  <div class="gameRedeemCodeEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="gameRedeemCodeEntity"
            @query="onQuery"

            @add="openAddGameRedeemCodeEntity"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" style="width: 100%" v-block-loading="loading" showGridlines stripedRows
                       rowKey="id" :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="gameRedeemCodeEntity"
                       :actions="{ delete: onDelete }"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage">
          <template #body-areaId="{ data }">
            {{ getAreaName(data.areaId) }}
          </template>
          <template #body-isActive="{ data }">
            <Tag :value="t('message.gameBusiness.yes')" v-if="data.isActive"/>
            <Tag :value="t('message.gameBusiness.no')" severity="danger" v-else/>
          </template>
          <template #body-channelId="{ data }">
            {{ getRedeemCodeChannelName(data.channelId) }}
          </template>
          <template #body-startTime="{ data }">{{ formatUnix(data.startTime) }}</template>
          <template #body-endTime="{ data }">{{ formatUnix(data.endTime) }}</template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
          <template #body-updateTime="{ data }">{{ formatUnix(data.updateTime) }}</template>
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editGameRedeemCodeEntityTitle" @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="gameRedeemCodeEntity">
import {ref, computed, watch, onMounted} from 'vue'
import {useRoute} from 'vue-router'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type { CrudColumnConfig } from '../../../components/crud-data-table/types.ts'
import { formatUnix } from '../../../utils/formatTime.ts'
import {useCrudTable} from '../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../composables'

import editDialog from '/@/views/commerceCenter/gameRedeemCodeEntity/component/EditDialog.vue'
import { gameRedeemCodePagePost, gameRedeemCodeDeletePost } from '../../../api/gameRedeemCode'
import { gameRedeemCodeChannelSelectStringPost } from '../../../api/gameRedeemCodeChannel'
import { gameAreaSelectStringPost } from '../../../api/gameArea'
import {MAX_SELECT_SIZE} from '../../../constants/pagination.ts'

const route = useRoute()
const editDialogRef = ref()
const editGameRedeemCodeEntityTitle = ref('')

const searchFields = computed(() => [
  { key: 'areaId', label: t('message.gameBusiness.role.areaName'), type: 'select', options: areaList.value, optionLabel: 'value', optionValue: 'id', placeholder: t('message.gameBusiness.order.pleaseSelectArea'), advanced: true },
  { key: 'channelId', label: t('message.gameBusiness.redeemcode.channelId'), type: 'select', options: redeemCodeChannelList.value, optionLabel: 'value', optionValue: 'id', placeholder: t('message.gameBusiness.redeemcode.pleaseSelectChannelId'), advanced: true },
  { key: 'usageLimit', label: t('message.gameBusiness.redeemcode.usageLimit'), type: 'number', placeholder: t('message.gameBusiness.redeemcode.pleaseEnterUsageLimit'), advanced: true },
  { key: 'usedCount', label: t('message.gameBusiness.redeemcode.usedCount'), type: 'number', placeholder: t('message.gameBusiness.redeemcode.pleaseEnterUsedCount'), advanced: true },
])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
  onDelete,
} = useCrudTable({
  api: {pageApi: gameRedeemCodePagePost, deleteApi: gameRedeemCodeDeletePost},
  immediate: true,
})

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'areaId', header: t('message.gameBusiness.role.areaName'), width: '140px', bodyKey: 'areaId'},
  {field: 'code', header: t('message.gameBusiness.redeemcode.code'), width: '340px', sensitive: true},
  {field: 'description', header: t('message.gameBusiness.redeemcode.description'), width: '200px'},
  {field: 'isActive', header: t('message.gameBusiness.isActive'), width: '100px', bodyKey: 'isActive'},
  {field: 'channelId', header: t('message.gameBusiness.redeemcode.channelId'), width: '150px', bodyKey: 'channelId'},
  {field: 'usageLimit', header: t('message.gameBusiness.redeemcode.usageLimit'), width: '140px'},
  {field: 'usedCount', header: t('message.gameBusiness.redeemcode.usedCount'), width: '140px'},
  {field: 'startTime', header: t('message.gameBusiness.redeemcode.validStartTime'), width: '200px', bodyKey: 'startTime'},
  {field: 'endTime', header: t('message.gameBusiness.redeemcode.invalidTime'), width: '200px', bodyKey: 'endTime'},
  {field: 'contents', header: t('message.gameBusiness.redeemcode.giftContent'), width: '400px'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '140px', bodyKey: 'createdTime'},
  {field: 'updateTime', header: t('message.gameBusiness.updateTime'), width: '140px', bodyKey: 'updateTime'},
  {field: 'createdName', header: t('message.common.label.creator'), width: '140px'},
  {field: 'updatedName', header: t('message.common.label.modifier'), width: '140px'},
])

const onPage = usePrimeTablePage(tableParams, onQuery)

// Open add dialog
const openAddGameRedeemCodeEntity = () => {
  editGameRedeemCodeEntityTitle.value = t('message.gameBusiness.redeemcode.addRedeemCode')
  editDialogRef.value.isEditor = false
  editDialogRef.value.openDialog({})
}

// Area list (loaded by current app scope)
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
const getAreaList = async () => {
  const res = await gameAreaSelectStringPost({pageSize: MAX_SELECT_SIZE})
  areaList.value = res.data.data ?? []
}

// Redeem code channel list (loaded by current app scope)
const redeemCodeChannelList = ref<any>([])
const getRedeemCodeChannelName = (areaId: number) => {
  // channelId 可空（未绑定渠道的兑换码），与其它空值列一致显示 "-"，避免误显"不存在的渠道"
  if (areaId == null) {
    return '-'
  }
  for (let i = 0; i < redeemCodeChannelList.value.length; i++) {
    const item = redeemCodeChannelList.value[i]
    if (item.id == areaId) {
      return item.label
    }
  }
  return t('message.gameBusiness.redeemcode.channelNotFound')
}
const getRedeemCodeChannelList = async () => {
  const res = await gameRedeemCodeChannelSelectStringPost({pageSize: MAX_SELECT_SIZE})
  redeemCodeChannelList.value = res.data.data ?? []
}

// Reset dependent filter selections when current app changes

onMounted(async () => {
  await getAreaList()
  await getRedeemCodeChannelList()
})
</script>
