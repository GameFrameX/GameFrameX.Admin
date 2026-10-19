<template>
  <div class="gameRedeemCodeChannelEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="gameRedeemCodeChannelEntity"
            :defaultSearchKey="false"
            @query="onQuery"
            @add="openAddGameRedeemCodeChannelEntity"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" style="width: 100%" v-block-loading="loading" showGridlines stripedRows
                       rowKey="id" :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="gameRedeemCodeChannelEntity"
                       :actions="actions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage">
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
          <template #body-updateTime="{ data }">{{ formatUnix(data.updateTime) }}</template>
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editGameRedeemCodeChannelEntityTitle" :appId="queryParams.appId" @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="gameRedeemCodeChannelEntity">
import {ref, computed} from 'vue'
import {useRoute} from 'vue-router'
import SearchForm from '/@/components/search-form/index.vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type { CrudColumnConfig } from '../../../../components/crud-data-table/types.ts'
import { formatUnix } from '../../../../utils/formatTime.ts'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage, useRouteAppIdQuerySync} from '../../../../composables'
import {useCurrentApp} from '../../../../composables/useCurrentApp.ts'
import editDialog from '/@/views/app/redeemCenter/gameRedeemCodeChannelEntity/component/EditDialog.vue'
import { tenantGameRedeemCodeChannelEntityPagePost, tenantGameRedeemCodeChannelEntityDeletePost } from '../../../../api/tenantGameRedeemCodeChannelEntity'

const route = useRoute()
const {resolveInitialAppId} = useCurrentApp()

const searchFields = computed(() => [
  { key: 'name', label: t('message.gameBusiness.name'), type: 'input', placeholder: t('message.gameBusiness.pleaseEnterName') },
])

const editDialogRef = ref()
const editGameRedeemCodeChannelEntityTitle = ref('')

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
  onDelete,
} = useCrudTable({
  api: {
    pageApi: (params) => tenantGameRedeemCodeChannelEntityPagePost(params),
    deleteApi: (params) => tenantGameRedeemCodeChannelEntityDeletePost(params),
  },
  immediate: true,
})

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAddGameRedeemCodeChannelEntity = () => {
  editGameRedeemCodeChannelEntityTitle.value = t('message.gameBusiness.redeemcode.addChannel')
  editDialogRef.value.isEditor = false
  editDialogRef.value.openDialog({})
}

const openEditGameRedeemCodeChannelEntity = (row: any) => {
  editGameRedeemCodeChannelEntityTitle.value = t('message.gameBusiness.redeemcode.editChannel')
  editDialogRef.value.isEditor = true
  editDialogRef.value.openDialog(row)
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'name', header: t('message.gameBusiness.name'), width: '200px'},
  {field: 'description', header: t('message.gameBusiness.description'), width: '200px'},
  {field: 'roleUsageLimit', header: t('message.gameBusiness.redeemcode.roleUsageLimit'), width: '160px'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '200px', bodyKey: 'createdTime'},
  {field: 'updateTime', header: t('message.gameBusiness.updateTime'), width: '140px', bodyKey: 'updateTime'},
  {field: 'createdName', header: t('message.common.label.creator'), width: '140px'},
  {field: 'updatedName', header: t('message.common.label.modifier'), width: '140px'},
])

const actions = {
  edit: openEditGameRedeemCodeChannelEntity,
  delete: onDelete,
}

useRouteAppIdQuerySync({
  route,
  queryParams,
  resolveAppId: resolveInitialAppId,
  onQuery,
})
</script>
