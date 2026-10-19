<template>
  <div class="tenantGameAppServerConfig-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="tenantGameAppServerConfig"
            @query="runScopedQuery"

            @add="openAddTenantGameAppServerConfig"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" style="width: 100%" v-block-loading="loading" showGridlines stripedRows
                   rowKey="id" :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                   :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                   authPrefix="tenantGameAppServerConfig"
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
            :title="editTenantGameAppServerConfigTitle"
            @reloadTable="runScopedQuery"
        />
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="tenantGameAppServerConfig">
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import {computed, onMounted, ref} from 'vue'
import {useRoute} from 'vue-router'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'
import {useCurrentApp} from '../../../../composables/useCurrentApp.ts'
import {formatUnix} from '../../../../utils/formatTime.ts'
import editDialog from '/@/views/app/appCenter/serverConfig/component/EditDialog.vue'
import { tenantGameAppServerConfigPagePost, tenantGameAppServerConfigDeletePost } from '../../../../api/tenantGameAppServerConfig'
import type {CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'
import {showWarning} from '../../../../utils/message.ts'
import {normalizeAppId, normalizeRouteAppId, type AppWorkspaceId} from '../../../../utils/appWorkspace.ts'

const editDialogRef = ref()
const editTenantGameAppServerConfigTitle = ref('')
const appMap = ref<Record<string, string>>({})
const route = useRoute()
const {appOptions, loadApps, currentAppId, setCurrentApp} = useCurrentApp()

const {
  loading, tableData, queryParams, tableParams,
  onQuery, onDelete,
} = useCrudTable({
  api: {pageApi: tenantGameAppServerConfigPagePost, deleteApi: tenantGameAppServerConfigDeletePost},
  immediate: false,
})

const currentQueryAppId = (): AppWorkspaceId | undefined => {
  return normalizeAppId(queryParams.value?.appId) ?? normalizeAppId(currentAppId.value)
}

const isAvailableAppId = (appId: AppWorkspaceId | undefined) => {
  return appId !== undefined && !!appMap.value[String(appId)]
}

const requireCurrentQueryAppId = (): AppWorkspaceId | undefined => {
  const appId = currentQueryAppId()
  if (appId === undefined) {
    showWarning(t('gameBusiness.appWorkspace.appIdRequired'))
    return undefined
  }
  if (!isAvailableAppId(appId)) {
    showWarning(t('gameBusiness.appWorkspace.appUnavailable'))
    return undefined
  }
  return appId
}

const runScopedQuery = () => {
  const appId = requireCurrentQueryAppId()
  if (appId === undefined) {
    return
  }
  queryParams.value.appId = appId
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
const openAddTenantGameAppServerConfig = () => {
  const appId = requireCurrentQueryAppId()
  if (appId === undefined) {
    return
  }
  editTenantGameAppServerConfigTitle.value = t('message.client.addServerConfig')
  editDialogRef.value.isEditor = false
  editDialogRef.value.openDialog({appId})
}

// 打开编辑页面
const openEditTenantGameAppServerConfig = (row: any) => {
  if (normalizeAppId(row?.appId) === undefined) {
    showWarning(t('gameBusiness.appWorkspace.appUnavailable'))
    return
  }
  editTenantGameAppServerConfigTitle.value = t('message.client.editServerConfig')
  editDialogRef.value.isEditor = true
  editDialogRef.value.openDialog(row)
}

const onScopedDelete = async (row: any) => {
  const appId = requireCurrentQueryAppId()
  if (appId === undefined) {
    return
  }
  queryParams.value.appId = appId
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
  edit: openEditTenantGameAppServerConfig,
  delete: onScopedDelete,
}

onMounted(async () => {
  await loadApps()
  const map: Record<string, string> = {}
  for (const a of appOptions.value) {
    const id = normalizeAppId(a.id)
    if (id !== undefined) {
      map[String(id)] = a.appName || a.name || String(id)
    }
  }
  appMap.value = map

  const appId = normalizeRouteAppId(route) ?? normalizeAppId(currentAppId.value)
  if (appId === undefined) {
    showWarning(t('gameBusiness.appWorkspace.appIdRequired'))
    return
  }
  if (!isAvailableAppId(appId)) {
    showWarning(t('gameBusiness.appWorkspace.appUnavailable'))
    return
  }

  setCurrentApp(appId)
  queryParams.value.appId = appId
  runScopedQuery()
})
</script>
