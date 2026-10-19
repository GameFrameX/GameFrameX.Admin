<template>
  <div class="tenantGameAppDevice-container">
    <Card>
      <template #content>
        <SearchForm v-model="queryParams" :fields="searchFields" :defaultSearchKey="false"
                    authPrefix="tenantDevice" :showAdd="hasApp" @query="guardedQuery" @reset="onReset" @add="openAddDialog"/>
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
        <editDialog ref="editDialogRef" :title="editTenantGameAppDeviceTitle" :appId="queryParams.appId" @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="tenantGameAppDevice">
import {computed, onMounted, ref} from 'vue'
import {useRoute} from 'vue-router'
import SearchForm from '/@/components/search-form/index.vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type {CrudColumnConfig} from '../../../../../components/crud-data-table/types.ts'
import {useCrudTable} from '../../../../../composables/useCrudTable.ts'
import {usePrimeTablePage, useRouteAppIdQuerySync} from '../../../../../composables'
import {useCurrentApp} from '../../../../../composables/useCurrentApp.ts'
import editDialog from '/@/views/app/appCenter/device/device/component/EditDialog.vue'
import { tenantGameAppDevicePagePost, tenantGameAppDeviceDeletePost } from '../../../../../api/tenantGameAppDevice'
import { tenantDeviceGroupSelectPost } from '../../../../../api/tenantDeviceGroup'
import {descriptionColumn, nameColumn} from '../../../../../utils/crudConfig.ts'

const route = useRoute()
const editDialogRef = ref()
const editTenantGameAppDeviceTitle = ref('')
const {ensureCurrentAppFromQuery} = useCurrentApp()

// 设备分组下拉（当前应用的分组；appId 是作用域而非搜索条件，不再暴露应用下拉）
const groupOptions = ref<any[]>([])
const loadGroups = async () => {
  if (!hasApp.value) {
    groupOptions.value = []
    return
  }
  const res = await tenantDeviceGroupSelectPost({appId: queryParams.value.appId})
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
  api: {pageApi: tenantGameAppDevicePagePost, deleteApi: tenantGameAppDeviceDeletePost},
  immediate: false,
})

// 设备码为应用级资源（C211）：未选应用时不请求、清空列表，避免后端 AppId<=0 抛错。
const hasApp = computed(() => !!queryParams.value.appId && queryParams.value.appId !== "0")

const guardedQuery = async () => {
  if (!hasApp.value) {
    tableData.value = []
    tableParams.value.total = 0
    return
  }
  await onQuery()
}

// 重置搜索条件时保留当前应用上下文（appId 是作用域而非搜索条件），清分组筛选后重新查询。
const onReset = () => {
  queryParams.value.appId = ensureCurrentAppFromQuery(route.query)
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

useRouteAppIdQuerySync({
  route,
  queryParams,
  resolveAppId: ensureCurrentAppFromQuery,
  onQuery: guardedQuery,
})

onMounted(async () => {
  await loadGroups()
  await guardedQuery()
})
</script>
