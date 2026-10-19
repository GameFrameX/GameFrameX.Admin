<template>
  <div class="gameAppEntity-container">
    <Card>
      <template #content>
        <SearchForm v-model="queryParams" :fields="searchFields" :defaultSearchKey="false"
                    authPrefix="gameAppEntity" :showAdd="true" @query="onQuery" @add="openAddApp"/>
      </template>
    </Card>

    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows style="width: 100%"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize" :rowsPerPageOptions="[10, 20, 50, 100]"
                       authPrefix="gameAppEntity"
                       :actions="actions"
                       :customActions="customActions"
                       actionColumnWidth="280px"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage" showIdColumn :idColumnHeader="t('gameBusiness.app.appId')" idColumnWidth="180px">
          <template #body-lastRotatedTime="{ data }">
            <Tag v-if="needRotation(data.lastRotatedTime)" severity="warn" :value="t('gameBusiness.app.needRotation')"/>
            <Tag v-else severity="success" :value="t('gameBusiness.app.rotated')"/>
          </template>
          <template #body-isEnabled="{ data }">
            <Tag v-if="data.isEnabled !== false" severity="success" :value="t('gameBusiness.app.enabled')"/>
            <Tag v-else severity="danger" :value="t('gameBusiness.app.disabled')"/>
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>

        <editDialog ref="editDialogRef" :title="editTitle" @reloadTable="onQuery" @created="onAppCreated"/>
        <secretDialog ref="secretDialogRef" @closed="onSecretClosed"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="gameGameAppEntity">
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type {CrudAction, CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'
import SearchForm from '/@/components/search-form/index.vue'

import {ref, computed} from 'vue'
import {useRouter} from 'vue-router'
import {showConfirm, showSuccess} from '../../../../utils/message.ts'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'
import {useCurrentApp} from '../../../../composables/useCurrentApp.ts'
import {formatUnix} from '../../../../utils/formatTime.ts'
import { tenantGameAppEntityPagePost, tenantGameAppEntityDeletePost as deleteAppApi, tenantGameAppEntityResetSecretPost } from '../../../../api/tenantGameAppEntity'
import {APP_WORKSPACE_PATH, normalizeAppId} from '../../../../utils/appWorkspace.ts'
import {faArrowPointer, faFolderOpen, faKey} from '../../../../utils/icons.ts'
import editDialog from './component/EditDialog.vue'
import secretDialog from './component/SecretDialog.vue'

const editDialogRef = ref()
const editTitle = ref('')
const secretDialogRef = ref()
const router = useRouter()
const pendingCreatedAppId = ref<string | number | undefined>(undefined)
const {setCurrentApp, reloadApps} = useCurrentApp()

const statusOptions = [
  {label: t('gameBusiness.app.enabled'), value: true},
  {label: t('gameBusiness.app.disabled'), value: false},
]

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'name', header: t('gameBusiness.app.appName')},
  {field: 'appSecret', header: t('gameBusiness.app.appSecret'), width: '160px', align: 'center', sensitive: true},
  {field: 'lastRotatedTime', header: t('gameBusiness.app.lastRotatedAt'), width: '140px', align: 'center', bodyKey: 'lastRotatedTime'},
  {field: 'isEnabled', header: t('gameBusiness.app.status'), width: '100px', align: 'center', bodyKey: 'isEnabled'},
  {field: 'description', header: t('gameBusiness.app.description'), responsivePriority: 2},
  {field: 'createdTime', header: t('common.label.createTime'), align: 'center', bodyKey: 'createdTime', responsivePriority: 2},
])

const ROTATION_THRESHOLD_DAYS = 90
const needRotation = (lastRotatedTime: any): boolean => {
  if (lastRotatedTime == null || typeof lastRotatedTime !== 'number' || lastRotatedTime <= 0) {
    return true
  }
  const nowSeconds = Math.floor(Date.now() / 1000)
  return (nowSeconds - lastRotatedTime) > ROTATION_THRESHOLD_DAYS * 86400
}

const searchFields = [
  {key: 'name', label: t('gameBusiness.app.appName'), type: 'input' as const, placeholder: t('gameBusiness.app.appName')},
  {key: 'isActive', label: t('gameBusiness.app.status'), type: 'select' as const, options: statusOptions, placeholder: t('gameBusiness.app.status')},
]

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
} = useCrudTable({
  api: {
    pageApi: (params) => tenantGameAppEntityPagePost(params),
    deleteApi: (params) => deleteAppApi({id: params.id, rowVersion: params.rowVersion}),
  },
  immediate: true,
})

queryParams.value = {name: undefined, isActive: undefined}
tableParams.value.pageSize = 10

const openAddApp = () => {
  editTitle.value = t('gameBusiness.app.addAppTitle')
  editDialogRef.value.isEditor = false
  editDialogRef.value.openDialog({})
}

const openEditApp = (row: any) => {
  editTitle.value = t('gameBusiness.app.editAppTitle')
  editDialogRef.value.isEditor = true
  editDialogRef.value.openDialog(row)
}

const onDeleteApp = async (row: any) => {
  const confirmed = await showConfirm(`${t('gameBusiness.app.deleteAppConfirm')}：【${row.name}】?`, t('common.message.tip'), {type: 'danger'})
  if (!confirmed) return
  // 删除走乐观锁：GameAppOutput 继承实体，列表行已携带 RowVersion，直接回传即可；
  // 不再调用 tenantGameAppEntityDetailGet，避免触发租户无权访问的 tenantGameAppEntity:detail 路由（403）。
  await deleteAppApi({id: row.id, rowVersion: row?.rowVersion})
  onQuery()
  showSuccess(t('gameBusiness.app.deleteSuccess'))
}

const onResetSecret = async (row: any) => {
  const confirmed = await showConfirm(t('gameBusiness.app.resetSecretConfirm'), t('common.message.tip'))
  if (!confirmed) return
  const res = await tenantGameAppEntityResetSecretPost({id: row.id})
  secretDialogRef.value?.openDialog(res.data?.data)
}

const openPackages = (row: any) => {
  router.push({ path: '/app/appCenter/gameAppPackage', query: { appId: row.id } })
}

const openWorkspace = (row: any) => {
  const appId = normalizeAppId(row?.id)
  if (appId === undefined || row?.isEnabled === false) {
    return
  }
  setCurrentApp(appId)
  router.push({ path: APP_WORKSPACE_PATH, query: { appId } })
}

const actions = {
  edit: openEditApp,
  delete: onDeleteApp,
}

const customActions: CrudAction[] = [
  {
    auth: 'package',
    label: t('gameBusiness.package.manage'),
    icon: faFolderOpen.className,
    severity: 'info',
    onClick: openPackages,
  },
  {
    auth: 'page',
    label: t('gameBusiness.appWorkspace.enterWorkspace'),
    icon: faArrowPointer.className,
    severity: 'secondary',
    visible: (row) => row?.isEnabled !== false && normalizeAppId(row?.id) !== undefined,
    onClick: openWorkspace,
  },
  {
    auth: 'resetSecret',
    label: t('gameBusiness.app.resetSecret'),
    icon: faKey.className,
    onClick: onResetSecret,
  },
]

const onPage = usePrimeTablePage(tableParams, onQuery)

const onAppCreated = async (entity: any) => {
  const appId = normalizeAppId(entity?.id)
  pendingCreatedAppId.value = appId
  if (appId !== undefined) {
    setCurrentApp(appId)
    await reloadApps()
  }
  secretDialogRef.value?.openDialog(entity)
}

const onSecretClosed = () => {
  if (pendingCreatedAppId.value === undefined) {
    return
  }
  const appId = pendingCreatedAppId.value
  pendingCreatedAppId.value = undefined
  router.push({path: APP_WORKSPACE_PATH, query: {appId: appId}})
}
</script>
