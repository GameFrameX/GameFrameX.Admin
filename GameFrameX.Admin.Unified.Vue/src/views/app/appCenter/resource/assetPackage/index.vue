<template>
  <div class="tenantGameAppAssetPackage-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="tenantGameAppAssetPackage"
            :defaultSearchKey="false"
            @query="onQuery"
            
            @add="openAddTenantGameAppAssetPackage"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows rowKey="id"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="tenantGameAppAssetPackage"
                       :actions="actions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage">
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editTenantGameAppAssetPackageTitle" :appId="queryParams.appId" @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="tenantGameAppAssetPackage">
import {computed, ref} from 'vue'
import {useRoute} from 'vue-router'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type {CrudColumnConfig} from '../../../../../components/crud-data-table/types.ts'
import {useCrudTable} from '../../../../../composables/useCrudTable.ts'
import {usePrimeTablePage, useRouteAppIdQuerySync} from '../../../../../composables'
import {useCurrentApp} from '../../../../../composables/useCurrentApp.ts'
import editDialog from '/@/views/app/appCenter/resource/assetPackage/component/EditDialog.vue'
import { tenantGameAppAssetPackageDeletePost, tenantGameAppAssetPackagePagePost } from '../../../../../api/tenantGameAppAssetPackage'
import {createdNameColumn, descriptionColumn, nameColumn, nameSearchField} from '../../../../../utils/crudConfig.ts'

const route = useRoute()
const editDialogRef = ref()
const editTenantGameAppAssetPackageTitle = ref('')
const {resolveInitialAppId} = useCurrentApp()

const searchFields = computed(() => [
  nameSearchField({label: t('message.client.packageAssetName'), placeholder: t('message.client.packageAssetName')}),
])

const {
  loading, tableData, queryParams, tableParams,
  onQuery, onDelete,
} = useCrudTable({
  api: {pageApi: tenantGameAppAssetPackagePagePost, deleteApi: tenantGameAppAssetPackageDeletePost},
  immediate: true,
})

queryParams.value = {
  name: undefined,
}

const onPage = usePrimeTablePage(tableParams, onQuery)

// 打开新增页面
const openAddTenantGameAppAssetPackage = () => {
  editTenantGameAppAssetPackageTitle.value = t('message.client.addAssetPackage')
  editDialogRef.value.isEditor = false
  editDialogRef.value.openDialog({appId: queryParams.value.appId})
}

const openEditTenantGameAppAssetPackage = (row: any) => {
  editTenantGameAppAssetPackageTitle.value = t('message.client.editAssetPackage')
  editDialogRef.value.isEditor = true
  editDialogRef.value.openDialog(row)
}

const columns = computed<CrudColumnConfig[]>(() => [
  nameColumn({width: '300px'}),
  descriptionColumn({header: t('message.client.detailedDescription'), width: '300px'}),
  createdNameColumn({width: '200px'}),
])

const actions = {
  edit: openEditTenantGameAppAssetPackage,
  delete: onDelete,
}

useRouteAppIdQuerySync({
  route,
  queryParams,
  resolveAppId: resolveInitialAppId,
  onQuery,
})
</script>
