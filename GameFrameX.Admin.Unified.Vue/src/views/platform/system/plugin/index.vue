<template>
  <div class="sys-plugin-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            :defaultSearchKey="false"
            authPrefix="systemPlugin"
            @query="onQuery"
            @reset="onReset"
            @add="openAddPlugin"
        />
      </template>
    </Card>

    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows style="width: 100%"
            :totalRecords="tableParams.total" :rows="tableParams.pageSize" :rowsPerPageOptions="[10, 20, 50, 100]"
            authPrefix="systemPlugin"
            :actions="actions"
            :columns="columns"
            :exportQuery="queryParams"
            @page="onPage">
          <template #body-isEnabled="{ data }">
            <Tag v-if="isEnabled(data)" severity="success" :value="t('common.label.enable')"/>
            <Tag v-else severity="danger" :value="t('common.label.disable')"/>
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>
      </template>
    </Card>

    <EditPlugin ref="editPluginRef" :title="editTitle" @query="onQuery"/>
  </div>
</template>

<script lang="ts" setup name="systemPlugin">
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import SearchForm from '/@/components/search-form/index.vue'

import {computed, ref} from 'vue'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'
import EditPlugin from '/@/views/platform/system/plugin/component/EditPlugin.vue'
import {systemPluginPagePost, systemPluginDeletePost} from '../../../../api/systemPlugin';
import {isEnabled} from '../../../../utils/isEnabled.ts'
import {formatUnix} from '../../../../utils/formatTime.ts'
import type {CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'

const editPluginRef = ref<InstanceType<typeof EditPlugin>>()
const editTitle = ref('')
const searchFields = computed(() => [
  { key: 'name', label: t('system.plugin.pluginName'), type: 'input', placeholder: t('system.plugin.pluginName') },
])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
  onDelete,
  onReset
} = useCrudTable({
  api: {
    pageApi: (params) => systemPluginPagePost(params),
    deleteApi: (params) => systemPluginDeletePost({id: params.id}),
  },
  pagination: {page: 1, pageSize: 10, total: 0},
  deleteConfirmMessage: (row: any) => t('system.plugin.deletePluginConfirm') + `：【${row.name}】?`,
  immediate: true,
})

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAddPlugin = () => {
  editTitle.value = t('system.plugin.addPlugin')
  editPluginRef.value?.openDialog({orderNo: 100, isEnabled: true})
}

const openEditPlugin = (row: any) => {
  editTitle.value = t('system.plugin.editPlugin')
  editPluginRef.value?.openDialog(row)
}

const actions = {
  edit: openEditPlugin,
  delete: onDelete,
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'name', header: t('system.plugin.pluginName')},
  {field: 'assemblyName', header: t('system.plugin.assemblyName')},
  {field: 'orderNo', header: t('common.label.sort'), width: '70px', align: 'center'},
  {field: 'isEnabled', header: t('common.label.status'), width: '70px', align: 'center', bodyKey: 'isEnabled'},
  {field: 'createdTime', header: t('common.label.createTime'), align: 'center', bodyKey: 'createdTime'},
  {field: 'remark', header: t('common.label.remark')},
])
</script>
