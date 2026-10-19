<template>
  <div class="tenantGameAppPackage-container">
    <Card>
      <template #content>
        <SearchForm v-model="queryParams" :fields="searchFields" :defaultSearchKey="false"
                    authPrefix="tenantGameAppPackage" :showAdd="true" @query="onQuery" @add="openAddPackage">
        </SearchForm>
      </template>
    </Card>

    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows style="width: 100%"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize" :rowsPerPageOptions="[10, 20, 50, 100]"
                       authPrefix="tenantGameAppPackage"
                       :actions="actions"
                       :customActions="customActions"
                       actionColumnWidth="260px"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage" showIdColumn idColumnWidth="180px">
          <template #body-parentPackageId="{ data }">
            <Tag v-if="data.parentPackageId" severity="info" :value="t('gameBusiness.package.roleChild')"/>
            <Tag v-else severity="success" :value="t('gameBusiness.package.roleMaster')"/>
          </template>
          <template #body-isDefault="{ data }">
            <Tag v-if="data.isDefault" severity="success" :value="t('gameBusiness.yes')"/>
            <Tag v-else severity="info" :value="t('gameBusiness.no')"/>
          </template>
          <template #body-isEnabled="{ data }">
            <Tag v-if="data.isEnabled !== false" severity="success" :value="t('gameBusiness.app.enabled')"/>
            <Tag v-else severity="danger" :value="t('gameBusiness.app.disabled')"/>
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>

        <editDialog ref="editDialogRef" :title="editTitle" :appId="queryParams.appId" @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="gameTenantGameAppPackage">
import { computed, ref } from 'vue'
import { useRoute } from 'vue-router'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type { CrudAction, CrudColumnConfig } from '../../../../components/crud-data-table/types.ts'
import SearchForm from '/@/components/search-form/index.vue'
import { useCrudTable } from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage, useRouteAppIdQuerySync } from '../../../../composables'
import { showConfirm, showSuccess } from '../../../../utils/message.ts'
import { formatUnix } from '../../../../utils/formatTime.ts'
import { faCircleCheck, faPowerOff, faToggleOn } from '../../../../utils/icons.ts'
import { tenantGameAppPackageDeletePost, tenantGameAppPackagePagePost, tenantGameAppPackageSetDefaultPost, tenantGameAppPackageSetStatusPost } from '../../../../api/tenantGameAppPackage'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import editDialog from './component/EditDialog.vue'

const route = useRoute()
const editDialogRef = ref()
const editTitle = ref('')
const {resolveInitialAppId} = useCurrentApp()

const searchFields = computed(() => [
  { key: 'name', label: t('gameBusiness.package.name'), type: 'input' as const, placeholder: t('gameBusiness.package.name') },
  { key: 'packageName', label: t('gameBusiness.package.packageName'), type: 'input' as const, placeholder: t('gameBusiness.package.packageName') },
  { key: 'packageVersion', label: t('gameBusiness.package.packageVersion'), type: 'input' as const, placeholder: t('gameBusiness.package.packageVersion') },
])

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'name', header: t('gameBusiness.package.name')},
  {field: 'packageName', header: t('gameBusiness.package.packageName')},
  {field: 'platformName', header: t('gameBusiness.package.platform')},
  {field: 'channelName', header: t('gameBusiness.package.channel')},
  {field: 'channelCode', header: t('gameBusiness.package.channelCode'), width: '160px'},
  {field: 'packageVersion', header: t('gameBusiness.package.packageVersion')},
  {field: 'parentPackageId', header: t('gameBusiness.package.roleColumn'), width: '110px', align: 'center', bodyKey: 'parentPackageId'},
  {field: 'isDefault', header: t('gameBusiness.package.isDefault'), width: '100px', align: 'center', bodyKey: 'isDefault'},
  {field: 'isEnabled', header: t('gameBusiness.app.status'), width: '100px', align: 'center', bodyKey: 'isEnabled'},
  {field: 'createdTime', header: t('common.label.createTime'), align: 'center', bodyKey: 'createdTime'},
])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
} = useCrudTable({
  api: {
    pageApi: (params) => tenantGameAppPackagePagePost({ ...params, appId: params.appId }),
    deleteApi: (params) => tenantGameAppPackageDeletePost({ id: params.id, rowVersion: params.rowVersion }),
  },
  immediate: true,
})

queryParams.value = {
  name: undefined,
  packageName: undefined,
  packageVersion: undefined,
  appId: resolveInitialAppId(route.query),
}
tableParams.value.pageSize = 10

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAddPackage = () => {
  editTitle.value = t('gameBusiness.package.addTitle')
  editDialogRef.value.isEditor = false
  editDialogRef.value.openDialog({})
}

const openEditPackage = (row: any) => {
  editTitle.value = t('gameBusiness.package.editTitle')
  editDialogRef.value.isEditor = true
  editDialogRef.value.openDialog(row)
}

const onDeletePackage = async (row: any) => {
  const confirmed = await showConfirm(`${t('gameBusiness.package.deleteConfirm')}：【${row.name}】?`, t('common.message.tip'), {type: 'danger'})
  if (!confirmed) return
  await tenantGameAppPackageDeletePost({ id: row.id, rowVersion: row.rowVersion })
  onQuery()
  showSuccess(t('gameBusiness.app.deleteSuccess'))
}

const onSetDefault = async (row: any) => {
  await tenantGameAppPackageSetDefaultPost({ id: row.id })
  onQuery()
  showSuccess(t('gameBusiness.package.setDefaultSuccess'))
}

const onToggleStatus = async (row: any) => {
  await tenantGameAppPackageSetStatusPost({ id: row.id, isEnabled: row.isEnabled === false })
  onQuery()
}

const actions = {
  edit: openEditPackage,
  delete: onDeletePackage,
}

const customActions: CrudAction[] = [
  {
    auth: 'setDefault',
    label: t('gameBusiness.package.setDefault'),
    icon: faCircleCheck.className,
    severity: 'success',
    visible: (row) => !row.isDefault && row.isEnabled !== false,
    onClick: onSetDefault,
  },
  {
    auth: 'setStatus',
    label: t('gameBusiness.package.toggleStatus'),
    icon: faToggleOn.className || faPowerOff.className,
    severity: 'secondary',
    onClick: onToggleStatus,
  },
]

useRouteAppIdQuerySync({
  route,
  queryParams,
  resolveAppId: resolveInitialAppId,
  onQuery,
})
</script>
