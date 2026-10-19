<template>
  <div class="version-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="tenantGameAppVersionDefinition"
            @query="onQuery"

            @add="openAddTenantGameAppVersionDefinition"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" style="width: 100%" v-block-loading="loading" showGridlines stripedRows
                   rowKeys="id" :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                   :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                   authPrefix="tenantGameAppVersionDefinition"
                   :actions="actions"
                   :columns="columns"
                   :exportQuery="queryParams"
                   @page="onPage">
        </CrudDataTable>
        <editDialog
            ref="editDialogRef"
            :title="editTenantGameAppVersionDefinitionTitle"
            :appId="queryParams.appId"
            @reloadTable="onQuery"
        />
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="version">
import {computed, ref} from 'vue'
import {useRoute} from 'vue-router'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type {CrudColumnConfig} from '../../../../../components/crud-data-table/types.ts'
import {useCrudTable} from '../../../../../composables/useCrudTable.ts'
import {usePrimeTablePage, useRouteAppIdQuerySync} from '../../../../../composables'
import {useCurrentApp} from '../../../../../composables/useCurrentApp.ts'
import editDialog from '/@/views/app/appCenter/release/versionDefinition/component/EditDialog.vue'
import { tenantGameAppVersionDefinitionPagePost, tenantGameAppVersionDefinitionDeletePost } from '../../../../../api/tenantGameAppVersionDefinition'
import {createdNameColumn, descriptionColumn, nameColumn, updatedNameColumn} from '../../../../../utils/crudConfig.ts'

const route = useRoute()
const {resolveInitialAppId} = useCurrentApp()

const editDialogRef = ref()
const editTenantGameAppVersionDefinitionTitle = ref('')

const searchFields = computed(() => [
])

const columns = computed<CrudColumnConfig[]>(() => [
  nameColumn({width: '200px'}),
  descriptionColumn({width: '400px'}),
  createdNameColumn({width: '210px'}),
  updatedNameColumn({width: '210px'}),
])

const {
  loading, tableData, queryParams, tableParams,
  onQuery, onDelete,
} = useCrudTable({
  api: {pageApi: tenantGameAppVersionDefinitionPagePost, deleteApi: tenantGameAppVersionDefinitionDeletePost},
  immediate: true,
})

const onPage = usePrimeTablePage(tableParams, onQuery)

// 打开新增页面
const openAddTenantGameAppVersionDefinition = () => {
  editTenantGameAppVersionDefinitionTitle.value = t('message.client.addVersion')
  editDialogRef.value.openDialog({appId: queryParams.value.appId})
}

// 打开编辑页面
const openEditTenantGameAppVersionDefinition = (row: any) => {
  editTenantGameAppVersionDefinitionTitle.value = t('message.client.editVersion')
  editDialogRef.value.openDialog(row)
}

const actions = {
  edit: openEditTenantGameAppVersionDefinition,
  delete: onDelete,
}

useRouteAppIdQuerySync({
  route,
  queryParams,
  resolveAppId: resolveInitialAppId,
  onQuery,
})
</script>
