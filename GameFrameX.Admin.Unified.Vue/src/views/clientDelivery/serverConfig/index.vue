<template>
  <div class="gameAppServerConfig-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="gameAppServerConfig"
            @query="runScopedQuery"

            @add="openAddGameAppServerConfig"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" style="width: 100%" v-block-loading="loading" showGridlines stripedRows
                   rowKey="id" :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                   :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                   authPrefix="gameAppServerConfig"
                   :actions="actions"
                   :columns="columns"
                   :exportQuery="queryParams"
                   @page="onPage">
          <template #body-enabled="{ data }">
            <Tag :value="data.enabled === false ? t('gameBusiness.disable') : t('gameBusiness.enable')"
                 :severity="data.enabled === false ? 'danger' : 'success'"/>
          </template>
          <template #body-md5="{ data }">
            <span :title="data.md5">{{ shortenMd5(data.md5) }}</span>
          </template>
          <template #body-updateTime="{ data }">{{ formatUnix(data.updateTime) }}</template>
        </CrudDataTable>
        <editDialog
            ref="editDialogRef"
            :title="editGameAppServerConfigTitle"
            @reloadTable="runScopedQuery"
        />
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="gameAppServerConfig">
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import {computed, onMounted, ref} from 'vue'
import {useCrudTable} from '../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../composables'

import {formatUnix} from '../../../utils/formatTime.ts'
import editDialog from '/@/views/clientDelivery/serverConfig/component/EditDialog.vue'
import { gameAppServerConfigPagePost, gameAppServerConfigDeletePost } from '../../../api/gameAppServerConfig'
import type {CrudColumnConfig} from '../../../components/crud-data-table/types.ts'

const editDialogRef = ref()
const editGameAppServerConfigTitle = ref('')


const {
  loading, tableData, queryParams, tableParams,
  onQuery, onDelete,
} = useCrudTable({
  api: {pageApi: gameAppServerConfigPagePost, deleteApi: gameAppServerConfigDeletePost},
  immediate: false,
})

const runScopedQuery = () => {



  onQuery()
}

const onPage = usePrimeTablePage(tableParams, runScopedQuery)

const searchFields = computed(() => [
  { key: 'key', label: t('message.client.serverConfigKey'), type: 'input', placeholder: t('message.common.form.pleaseInput') },
  { key: 'enabled', label: t('message.client.isEnabled'), type: 'select', options: [
    {label: t('gameBusiness.enable'), value: true},
    {label: t('gameBusiness.disable'), value: false},
  ], placeholder: t('message.common.form.pleaseSelect') },
])

const shortenMd5 = (md5: any) => {
  if (!md5) return '-'
  return String(md5).length > 10 ? String(md5).slice(0, 10) + '…' : String(md5)
}

// 打开新增页面
const openAddGameAppServerConfig = () => {


  editGameAppServerConfigTitle.value = t('message.client.addServerConfig')
  editDialogRef.value.isEditor = false
  editDialogRef.value.openDialog({})
}

// 打开编辑页面
const openEditGameAppServerConfig = (row: any) => {

  editGameAppServerConfigTitle.value = t('message.client.editServerConfig')
  editDialogRef.value.isEditor = true
  editDialogRef.value.openDialog(row)
}

const onScopedDelete = async (row: any) => {



  await onDelete(row)
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'key', header: t('message.client.serverConfigKey'), width: '200px'},
  {field: 'md5', header: t('message.client.serverConfigMd5'), width: '130px', align: 'center', bodyKey: 'md5'},
  {field: 'enabled', header: t('message.client.isEnabled'), width: '100px', align: 'center', bodyKey: 'enabled'},
  {field: 'description', header: t('message.common.label.description'), width: '200px'},
  {field: 'updatedName', header: t('message.common.label.modifier'), width: '180px'},
  {field: 'updateTime', header: t('message.common.label.updateTime'), width: '170px', bodyKey: 'updateTime'},
])

const actions = {
  edit: openEditGameAppServerConfig,
  delete: onScopedDelete,
}

onMounted(async () => {







  runScopedQuery()
})
</script>
