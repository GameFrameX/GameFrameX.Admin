<template>
  <div class="tenantGameAppDevice-container">
    <Card>
      <template #content>
        <SearchForm v-model="queryParams" :fields="searchFields" :defaultSearchKey="false"
                    authPrefix="tenantDevice" :showAdd="true" @query="guardedQuery" @reset="onReset" @add="openAddDialog"/>
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows rowKey="id"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="tenantDevice"
                       :actions="actions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage" showIdColumn :idColumnHeader="t('message.common.label.id')" idColumnWidth="200px">
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editTenantGameAppDeviceTitle" @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="tenantGameAppDevice">
import {computed, onMounted, ref} from 'vue'
import SearchForm from '/@/components/search-form/index.vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type {CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'

import editDialog from '/@/views/clientDelivery/device/device/component/EditDialog.vue'
import { gameAppDevicePagePost, gameAppDeviceDeletePost } from '../../../../api/gameAppDevice'
import { deviceGroupSelectPost } from '../../../../api/deviceGroup'
import {descriptionColumn, nameColumn} from '../../../../utils/crudConfig.ts'

const editDialogRef = ref()
const editTenantGameAppDeviceTitle = ref('')
const groupOptions = ref<any[]>([])
const loadGroups = async () => {

  const res = await deviceGroupSelectPost({})
  groupOptions.value = res.data.data ?? []
}

const searchFields = computed(() => [
  // select 接口响应经 axios 拦截器转 camelCase（id/value/label）
  {key: 'groupId', label: t('message.client.deviceGroup'), type: 'select' as const, options: groupOptions.value, optionLabel: 'label', optionValue: 'id', placeholder: t('message.client.pleaseSelectDeviceGroup')},
])

const {
  loading, tableData, queryParams, tableParams,
  onQuery, onDelete,
} = useCrudTable({
  api: {pageApi: gameAppDevicePagePost, deleteApi: gameAppDeviceDeletePost},
  immediate: false,
})

const guardedQuery = async () => {

  await onQuery()
}
const onReset = () => {

  queryParams.value.groupId = undefined
  guardedQuery()
}

const onPage = usePrimeTablePage(tableParams, guardedQuery)

// 打开编辑页面
const openEditTenantGameAppDevice = (row: any) => {
  editTenantGameAppDeviceTitle.value = t('message.common.button.edit')
  editDialogRef.value.isEditor = true
  editDialogRef.value.openDialog(row)
}

// 打开新增页面
const openAddDialog = () => {
  editTenantGameAppDeviceTitle.value = t('message.common.button.add')
  editDialogRef.value.isEditor = false
  editDialogRef.value.openDialog({})
}

const columns = computed<CrudColumnConfig[]>(() => [
  nameColumn({width: '140px'}),
  descriptionColumn({header: t('message.client.detailedDescription'), width: '140px'}),
  {field: 'deviceUniqueIdentifier', header: t('message.client.machineCode')},
])

const actions = {
  edit: openEditTenantGameAppDevice,
  delete: onDelete,
}

onMounted(async () => {
  await loadGroups()
  await guardedQuery()
})
</script>
