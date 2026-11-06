<template>
  <div class="gameAppAssetPackage-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="gameAppAssetPackage"
            :defaultSearchKey="false"
            @query="onQuery"

            @add="openAddGameAppAssetPackage"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows rowKey="id"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="gameAppAssetPackage"
                       :actions="actions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage">
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editGameAppAssetPackageTitle" @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="gameAppAssetPackage">
import {computed, ref} from 'vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type {CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'

import editDialog from '/@/views/clientDelivery/resource/assetPackage/component/EditDialog.vue'
import { gameAppAssetPackageDeletePost, gameAppAssetPackagePagePost } from '../../../../api/gameAppAssetPackage'
import {createdNameColumn, descriptionColumn, nameColumn, nameSearchField} from '../../../../utils/crudConfig.ts'

const editDialogRef = ref()
const editGameAppAssetPackageTitle = ref('')

const searchFields = computed(() => [
  nameSearchField({label: t('message.client.packageAssetName'), placeholder: t('message.client.packageAssetName')}),
])

const {
  loading, tableData, queryParams, tableParams,
  onQuery, onDelete,
} = useCrudTable({
  api: {pageApi: gameAppAssetPackagePagePost, deleteApi: gameAppAssetPackageDeletePost},
  immediate: true,
})

queryParams.value = {
  name: undefined,
}

const onPage = usePrimeTablePage(tableParams, onQuery)

// 打开新增页面
const openAddGameAppAssetPackage = () => {
  editGameAppAssetPackageTitle.value = t('message.client.addAssetPackage')
  editDialogRef.value.isEditor = false
  editDialogRef.value.openDialog({})
}

const openEditGameAppAssetPackage = (row: any) => {
  editGameAppAssetPackageTitle.value = t('message.client.editAssetPackage')
  editDialogRef.value.isEditor = true
  editDialogRef.value.openDialog(row)
}

const columns = computed<CrudColumnConfig[]>(() => [
  nameColumn({width: '300px'}),
  descriptionColumn({header: t('message.client.detailedDescription'), width: '300px'}),
  createdNameColumn({width: '200px'}),
])

const actions = {
  edit: openEditGameAppAssetPackage,
  delete: onDelete,
}

</script>
