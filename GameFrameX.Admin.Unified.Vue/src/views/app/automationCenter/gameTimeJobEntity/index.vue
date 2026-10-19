<template>
  <div class="gameTimeJobEntity-container">
    <SearchForm
      v-model="queryParams"
      :fields="searchFields"
      :showAdd="true"
      authPrefix="gameTimeJobEntity"
      :defaultSearchKey="false"
      @query="onQuery"
      @reset="onReset"
      @add="openAdd"
    />
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]" @page="onPage"
                       authPrefix="gameTimeJobEntity"
                       :actions="actions"
                       :columns="columns"
                       :exportQuery="queryParams">
          <template #body-isOnce="{ data }">
            <ToggleSwitch v-model="data.isOnce" disabled/>
          </template>
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editTitle" :appId="queryParams.appId" @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="gameTimeJobEntity">
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import SearchForm from '/@/components/search-form/index.vue'
import {ref, onMounted, computed} from 'vue'
import {useRoute} from 'vue-router'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage, useRouteAppIdQuerySync} from '../../../../composables'
import {showConfirm} from '../../../../utils/message.ts'
import editDialog from '/@/views/app/automationCenter/gameTimeJobEntity/component/EditDialog.vue'
import { tenantGameTimeJobEntityPagePost, tenantGameTimeJobEntityDeletePost } from '../../../../api/tenantGameTimeJobEntity'
import {useCurrentApp} from '../../../../composables/useCurrentApp.ts'
import type {CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'

const route = useRoute()
const {resolveInitialAppId} = useCurrentApp()
const editDialogRef = ref()
const editTitle = ref('')

const searchFields = [
  {key: 'name', label: t('message.gameBusiness.name'), type: 'input'},
]

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
} = useCrudTable({
  api: {pageApi: tenantGameTimeJobEntityPagePost, deleteApi: tenantGameTimeJobEntityDeletePost},
  immediate: false,
})

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'name', header: t('message.gameBusiness.name'), width: '120px', align: 'center'},
  {field: 'description', header: t('message.gameBusiness.redeemcode.description'), width: '150px', align: 'center'},
  {field: 'cron', header: t('message.gameBusiness.timeJob.cron'), width: '150px', align: 'center'},
  {field: 'isOnce', header: t('message.gameBusiness.isOnce'), width: '100px', align: 'center', bodyKey: 'isOnce'},
])

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAdd = () => {
  editTitle.value = t('message.common.button.add')
  editDialogRef.value.isEditor = false
  editDialogRef.value?.openDialog({})
}

const onEdit = (row: any) => {
  editTitle.value = t('message.common.button.edit')
  editDialogRef.value?.openDialog(row)
}

const onDelete = async (row: any) => {
  const confirmed = await showConfirm(
    t('message.common.message.deleteConfirm'),
    t('message.common.message.tip'),
    {type: 'danger'},
  )
  if (!confirmed) return
  // rowVersion 必传：BaseDeleteRequest 走乐观锁删除，缺版本会让后端以 null 比较版本列（Postgres bigint = text）500
  await tenantGameTimeJobEntityDeletePost({id: row.id, rowVersion: row.rowVersion})
  onQuery()
}

const actions = {
  edit: onEdit,
  delete: { onClick: onDelete },
}

const onReset = () => {
  queryParams.value = {appId: resolveInitialAppId(route.query)}
  tableParams.value.page = 1
  onQuery()
}

useRouteAppIdQuerySync({
  route,
  queryParams,
  resolveAppId: resolveInitialAppId,
  onQuery,
})

onMounted(async () => {
  await onQuery()
})
</script>
