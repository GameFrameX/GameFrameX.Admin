<template>
  <div class="DeviceGroup-container">
    <Card>
      <template #content>
        <SearchForm v-model="queryParams" :fields="searchFields" :defaultSearchKey="false"
                    authPrefix="deviceGroup" :showAdd="hasApp" @query="guardedQuery" @reset="onReset" @add="openAddDeviceGroup"/>
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
        <editDialog ref="editDialogRef" :title="editDeviceGroupTitle" :appId="queryParams.appId" @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="DeviceGroup">
import {computed, onMounted, ref} from 'vue'
import {useRoute} from 'vue-router'
import SearchForm from '/@/components/search-form/index.vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type {CrudColumnConfig} from '../../../../../components/crud-data-table/types.ts'
import {useCrudTable} from '../../../../../composables/useCrudTable.ts'
import {usePrimeTablePage, useRouteAppIdQuerySync} from '../../../../../composables'
import {useCurrentApp} from '../../../../../composables/useCurrentApp.ts'
import {descriptionColumn, nameColumn} from '../../../../../utils/crudConfig.ts'
import editDialog from '/@/views/app/appCenter/device/group/component/EditDialog.vue'
import { tenantDeviceGroupPagePost, tenantDeviceGroupDeletePost } from '../../../../../api/tenantDeviceGroup'

const route = useRoute()
const editDialogRef = ref()
const editDeviceGroupTitle = ref('')
// appId 是作用域而非搜索条件（C211 应用级资源），不再暴露应用下拉
const {ensureCurrentAppFromQuery} = useCurrentApp()

const searchFields = computed(() => [])

const {
  loading, tableData, queryParams, tableParams,
  onQuery, onDelete,
} = useCrudTable({
  api: {pageApi: tenantDeviceGroupPagePost, deleteApi: tenantDeviceGroupDeletePost},
  immediate: false,
})

// 设备分组为应用级资源（C211）：未选应用时不请求、清空列表，避免后端 AppId<=0 抛错。
const hasApp = computed(() => !!queryParams.value.appId && queryParams.value.appId !== "0")

const guardedQuery = async () => {
  if (!hasApp.value) {
    tableData.value = []
    tableParams.value.total = 0
    return
  }
  await onQuery()
}

// 重置搜索条件时保留当前应用上下文（appId 是作用域而非搜索条件），仅清搜索关键字后重新查询。
const onReset = () => {
  queryParams.value.appId = ensureCurrentAppFromQuery(route.query)
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

useRouteAppIdQuerySync({
  route,
  queryParams,
  resolveAppId: ensureCurrentAppFromQuery,
  onQuery: guardedQuery,
})

onMounted(async () => {
  await guardedQuery()
})
</script>
