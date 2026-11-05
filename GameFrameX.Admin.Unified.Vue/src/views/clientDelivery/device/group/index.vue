<template>
  <div class="DeviceGroup-container">
    <Card>
      <template #content>
        <SearchForm v-model="queryParams" :fields="searchFields" :defaultSearchKey="false"
                    authPrefix="deviceGroup" :showAdd="true" @query="guardedQuery" @reset="onReset" @add="openAddDeviceGroup"/>
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" style="width: 100%" v-block-loading="loading" showGridlines stripedRows
                   rowKey="id" :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                   :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                   authPrefix="deviceGroup"
                   :actions="actions"
                   :columns="columns"
                   :exportQuery="queryParams"
                   @page="onPage">
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editDeviceGroupTitle" @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="DeviceGroup">
import {computed, onMounted, ref} from 'vue'
import {useRoute} from 'vue-router'
import SearchForm from '/@/components/search-form/index.vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type {CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'

import {descriptionColumn, nameColumn} from '../../../../utils/crudConfig.ts'
import editDialog from '/@/views/clientDelivery/device/group/component/EditDialog.vue'
import { deviceGroupPagePost, deviceGroupDeletePost } from '../../../../api/deviceGroup'

const route = useRoute()
const editDialogRef = ref()
const editDeviceGroupTitle = ref('')

const searchFields = computed(() => [])

const {
  loading, tableData, queryParams, tableParams,
  onQuery, onDelete,
} = useCrudTable({
  api: {pageApi: deviceGroupPagePost, deleteApi: deviceGroupDeletePost},
  immediate: false,
})

const guardedQuery = async () => {

  await onQuery()
}
const onReset = () => {

  guardedQuery()
}

const onPage = usePrimeTablePage(tableParams, guardedQuery)

// 打开新增页面
const openAddDeviceGroup = () => {
  editDeviceGroupTitle.value = t('message.client.addDeviceGroup')
  editDialogRef.value.openDialog({})
}

// 打开编辑页面
const openEditDeviceGroup = (row: any) => {
  editDeviceGroupTitle.value = t('message.client.editDeviceGroup')
  editDialogRef.value.isEditor = true
  editDialogRef.value.openDialog(row)
}

const columns = computed<CrudColumnConfig[]>(() => [
  nameColumn({width: '140px'}),
  descriptionColumn({header: t('message.client.detailedDescription'), width: '140px'}),
])

const actions = {
  edit: openEditDeviceGroup,
  delete: onDelete,
}

onMounted(async () => {
  await guardedQuery()
})
</script>
