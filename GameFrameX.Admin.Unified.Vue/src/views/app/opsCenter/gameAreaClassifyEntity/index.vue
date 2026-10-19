<template>
  <div class="gameAreaClassifyEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="gameAreaClassifyEntity"
            :defaultSearchKey="false"
            @query="onQuery"
            
            @add="openAddGameAreaClassifyEntity"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows style="width: 100%"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="gameAreaClassifyEntity"
                       :actions="actions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage">
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
          <template #body-updateTime="{ data }">{{ formatUnix(data.updateTime) }}</template>
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editGameAreaClassifyEntityTitle" :appId="queryParams.appId" @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="gameAreaClassifyEntity">
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import {ref, computed} from 'vue'
import {useRoute} from 'vue-router'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage, useRouteAppIdQuerySync} from '../../../../composables'
import {useCurrentApp} from '../../../../composables/useCurrentApp.ts'
import editDialog from '/@/views/app/opsCenter/gameAreaClassifyEntity/component/EditDialog.vue'
import { tenantGameAreaClassifyEntityPagePost, tenantGameAreaClassifyEntityDeletePost } from '../../../../api/tenantGameAreaClassifyEntity'
import {formatUnix} from '../../../../utils/formatTime.ts'
import type {CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'

const route = useRoute()
const {resolveInitialAppId} = useCurrentApp()

const editDialogRef = ref()
const editGameAreaClassifyEntityTitle = ref('')

const searchFields = computed(() => [
  { key: 'name', label: t('message.gameBusiness.name'), type: 'input', placeholder: t('message.gameBusiness.pleaseEnterName'), advanced: true },
])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
  onDelete,
} = useCrudTable({
  api: {
    pageApi: (params) => tenantGameAreaClassifyEntityPagePost(params),
    deleteApi: (params) => tenantGameAreaClassifyEntityDeletePost(params),
  },
  immediate: true,
})

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAddGameAreaClassifyEntity = () => {
  editGameAreaClassifyEntityTitle.value = t('message.gameBusiness.area.addAreaClassify')
  editDialogRef.value.isEditor = false
  editDialogRef.value.openDialog({})
}

const openEditGameAreaClassifyEntity = (row: any) => {
  editGameAreaClassifyEntityTitle.value = t('message.gameBusiness.area.editAreaClassify')
  editDialogRef.value.isEditor = true
  editDialogRef.value.openDialog(row)
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'name', header: t('message.gameBusiness.name'), width: '140px'},
  {field: 'description', header: t('message.gameBusiness.description'), width: '200px'},
  {field: 'localizationKey', header: t('message.gameBusiness.localizationKey'), width: '200px'},
  {field: 'httpManagerUrl', header: t('message.gameBusiness.area.serverManagementUrl'), width: '200px'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '200px', bodyKey: 'createdTime'},
  {field: 'updateTime', header: t('message.gameBusiness.updateTime'), width: '200px', bodyKey: 'updateTime'},
])

const actions = {
  edit: openEditGameAreaClassifyEntity,
  delete: { onClick: onDelete, visible: (row: any) => row.isDelete !== true },
}

useRouteAppIdQuerySync({
  route,
  queryParams,
  resolveAppId: resolveInitialAppId,
  onQuery,
})
</script>
