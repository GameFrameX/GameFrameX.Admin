<template>
  <div class="mode-config-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="tenantGameAppRemoteConfigMode"
            @query="onQuery"

            @add="openAdd"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" style="width: 100%" v-block-loading="loading" showGridlines stripedRows
                       rowKey="id" :showPagination="false"
                       authPrefix="tenantGameAppRemoteConfigMode"
                       :actions="actions"
                       :columns="columns"
                       :exportQuery="queryParams">
          <template #body-isEnabled="{ data }">
            <Tag :value="isEnabled(data) ? t('message.common.label.yes') : t('message.common.label.no')"
                 :severity="isEnabled(data) ? 'success' : 'danger'"/>
          </template>
        </CrudDataTable>
        <editDialog
            ref="editDialogRef"
            :title="editTitle"
            @reloadTable="onQuery"
        />
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="tenantGameAppRemoteConfigMode">
import {ref, computed} from 'vue'
import SearchForm from '/@/components/search-form/index.vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type {CrudColumnConfig} from '../../../../../components/crud-data-table/types.ts'
import {useCrudTable} from '../../../../../composables/useCrudTable.ts'
import editDialog from '/@/views/app/appCenter/remoteConfig/modeConfig/component/EditDialog.vue'
import {tenantGameAppRemoteConfigModePagePost, tenantGameAppRemoteConfigModeDeletePost} from '../../../../../api/tenantGameAppRemoteConfigMode'
import {isEnabled} from '../../../../../utils/isEnabled.ts'

const editDialogRef = ref()
const editTitle = ref('')

const searchFields = computed(() => [])

const {
  loading, tableData, queryParams,
  onQuery, onDelete,
} = useCrudTable({
  api: {pageApi: tenantGameAppRemoteConfigModePagePost, deleteApi: tenantGameAppRemoteConfigModeDeletePost},
  immediate: true,
})

const openAdd = () => {
  editTitle.value = t('message.client.addMode')
  editDialogRef.value.openDialog({})
}

const openEdit = (row: any) => {
  editTitle.value = t('message.client.editMode')
  editDialogRef.value.openDialog(row)
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'modeCode', header: t('message.client.modeCode'), width: '120px'},
  {field: 'displayName', header: t('message.client.displayName'), width: '120px'},
  {field: 'description', header: t('message.common.label.description'), width: '150px'},
  {field: 'sortOrder', header: t('message.client.sortOrder'), width: '80px'},
  {field: 'isEnabled', header: t('message.client.isEnabled'), width: '80px', bodyKey: 'isEnabled'},
])

const actions = {
  edit: openEdit,
  delete: onDelete,
}
</script>
