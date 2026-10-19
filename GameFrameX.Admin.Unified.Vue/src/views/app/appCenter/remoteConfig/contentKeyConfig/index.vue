<template>
  <div class="content-key-config-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="tenantGameAppRemoteConfigKey"
            @query="onQuery"

            @add="openAdd"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" style="width: 100%" v-block-loading="loading" showGridlines stripedRows
                       rowKey="id" :showPagination="false"
                       authPrefix="tenantGameAppRemoteConfigKey"
                       :actions="actions"
                       :columns="columns"
                       :exportQuery="queryParams">
          <template #body-valueType="{ data }">
            <Tag :value="getValueTypeLabel(data)" :severity="getValueTypeSeverity(data)"/>
          </template>
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

<script lang="ts" setup name="tenantGameAppRemoteConfigKey">
import {ref, computed} from 'vue'
import SearchForm from '/@/components/search-form/index.vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type {CrudColumnConfig} from '../../../../../components/crud-data-table/types.ts'
import {useCrudTable} from '../../../../../composables/useCrudTable.ts'
import editDialog from '/@/views/app/appCenter/remoteConfig/contentKeyConfig/component/EditDialog.vue'
import {tenantGameAppRemoteConfigKeyPagePost, tenantGameAppRemoteConfigKeyDeletePost} from '../../../../../api/tenantGameAppRemoteConfigKey'
import {isEnabled} from '../../../../../utils/isEnabled.ts'

const editDialogRef = ref()
const editTitle = ref('')

const searchFields = computed(() => [])

const getValueType = (row: any) => row.valueType || 'string'
const getValueTypeLabel = (row: any) => getValueType(row).toUpperCase()
const getValueTypeSeverity = (row: any) => {
  const valueType = getValueType(row)
  if (valueType === 'bool') return 'success'
  if (valueType === 'long') return 'warn'
  return 'info'
}

const {
  loading, tableData, queryParams,
  onQuery, onDelete,
} = useCrudTable({
  api: {pageApi: tenantGameAppRemoteConfigKeyPagePost, deleteApi: tenantGameAppRemoteConfigKeyDeletePost},
  immediate: true,
})

const openAdd = () => {
  editTitle.value = t('message.client.addKey')
  editDialogRef.value.openDialog({})
}

const openEdit = (row: any) => {
  editTitle.value = t('message.client.editKey')
  editDialogRef.value.openDialog(row)
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'keyName', header: t('message.client.keyName'), width: '120px'},
  {field: 'displayName', header: t('message.client.displayName'), width: '120px'},
  {field: 'valueType', header: t('message.common.label.type'), width: '100px', bodyKey: 'valueType'},
  {field: 'description', header: t('message.common.label.description'), width: '150px'},
  {field: 'sortOrder', header: t('message.client.sortOrder'), width: '80px'},
  {field: 'isEnabled', header: t('message.client.isEnabled'), width: '80px', bodyKey: 'isEnabled'},
])

const actions = {
  edit: openEdit,
  delete: onDelete,
}
</script>
