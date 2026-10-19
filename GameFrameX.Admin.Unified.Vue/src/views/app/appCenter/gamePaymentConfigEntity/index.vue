<template>
  <div class="gamePaymentConfigEntity-container">
    <Card>
      <template #content>
        <div class="payment-scope-toolbar">
          <Select
            filter
            v-model="selectedGameAppId"
            :options="appScopeOptions"
            optionLabel="name"
            optionValue="id"
            :placeholder="t('gameBusiness.configScope.scope')"
            class="payment-scope-app"
          />
          <Select
            v-if="selectedGameAppId"
            filter
            v-model="selectedPackageId"
            :options="packageOptions"
            optionLabel="label"
            optionValue="value"
            :placeholder="t('message.client.packageName')"
            class="payment-scope-pkg"
          />
          <Tag v-if="!selectedGameAppId" severity="warn" :value="t('gameBusiness.appWorkspace.appIdRequired')"/>
        </div>
        <Message
          v-if="selectedPackageId && !loading && tableData.length === 0"
          severity="info"
          size="small"
          class="payment-inherit-message"
        >
          当前分包没有覆盖配置，将继承应用默认配置；点击新增可创建分包覆盖配置。
        </Message>
      </template>
    </Card>

    <Card>
      <template #content>
        <SearchForm
          v-model="queryParams"
          :fields="searchFields"
          :defaultSearchKey="true"
          authPrefix="gamePaymentConfigEntity"
          :showAdd="true"
          @query="onQuery"
          @add="openAddPaymentConfig"
        />
      </template>
    </Card>

    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable
          :value="tableData"
          v-block-loading="loading"
          showGridlines
          stripedRows
          style="width: 100%"
          :totalRecords="tableParams.total"
          :rows="tableParams.pageSize"
          :rowsPerPageOptions="[10, 20, 50, 100]"
          authPrefix="gamePaymentConfigEntity"
          :actions="actions"
          :customActions="customActions"
          actionColumnWidth="260px"
          :columns="columns"
          :exportQuery="queryParams"
          @page="onPage"
          showIdColumn
          idColumnWidth="120px"
        >
          <template #body-appId="{ data }">
            <Tag :severity="paymentScopeSeverity(data)" :value="paymentScopeLabel(data)"/>
          </template>
          <template #body-payChannel="{ data }">
            <Tag :value="formatChannel(data.payChannel)"/>
          </template>
          <template #body-isSandbox="{ data }">
            <Tag v-if="data.isSandbox" severity="warn" value="Sandbox"/>
            <Tag v-else severity="success" value="Prod"/>
          </template>
          <template #body-isEnabled="{ data }">
            <Tag v-if="data.isEnabled !== false" severity="success" value="启用"/>
            <Tag v-else severity="danger" value="禁用"/>
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>

        <editDialog ref="editDialogRef" :title="editTitle" :appOptions="appOptions" :packageMap="packageMap" @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="gameGamePaymentConfigEntity">
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type {CrudAction, CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'
import SearchForm from '/@/components/search-form/index.vue'
import commonFunction from '../../../../utils/commonFunction.ts'

import {computed, onMounted, ref, watch} from 'vue'
import {useRoute} from 'vue-router'
import {showConfirm, showSuccess, showWarning} from '../../../../utils/message.ts'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'
import {formatUnix} from '../../../../utils/formatTime.ts'
import { tenantGamePaymentConfigEntityPagePost, tenantGamePaymentConfigEntityDeletePost as deletePaymentConfigApi, tenantGamePaymentConfigEntityChannelsGet, tenantGamePaymentConfigEntityDetailGet } from '../../../../api/tenantGamePaymentConfigEntity'
import { tenantGameAppPackageListGet } from '../../../../api/tenantGameAppPackage'
import {useCurrentApp} from '../../../../composables/useCurrentApp.ts'
import {buildNotifyUrl} from './notifyUrl'
import editDialog from './component/EditDialog.vue'
import {normalizeAppId, normalizeRouteAppId, type AppWorkspaceId} from '../../../../utils/appWorkspace.ts'

const editDialogRef = ref()
const editTitle = ref('')
const {copyText} = commonFunction()
const route = useRoute()
const {appOptions, appName, loadApps, currentAppId, setCurrentApp} = useCurrentApp()
const selectedGameAppId = ref<AppWorkspaceId | undefined>(undefined)
const selectedPackageId = ref<number>(0)
const packageOptions = ref<{label: string; value: number}[]>([])
const packageMap = ref<Record<number, string>>({})

const appScopeOptions = computed(() => [
  ...appOptions.value
    .filter((a: any) => a.isEnabled !== false && normalizeAppId(a.id) !== undefined)
    .map((a: any) => {
      const id = normalizeAppId(a.id) as AppWorkspaceId
      return {id, name: a.appName || a.name || String(id)}
    }),
])

const channelOptions = ref<{label: string; value: string}[]>([])

const sandboxOptions = [
  {label: 'Prod', value: false},
  {label: 'Sandbox', value: true},
]

const statusOptions = [
  {label: '启用', value: true},
  {label: '禁用', value: false},
]

const searchFields = computed(() => [
  {key: 'payChannel', label: '支付渠道', type: 'select' as const, options: channelOptions.value, placeholder: '支付渠道'},
  {key: 'isSandbox', label: '环境', type: 'select' as const, options: sandboxOptions, placeholder: '环境'},
  {key: 'isActive', label: '状态', type: 'select' as const, options: statusOptions, placeholder: '状态'},
])

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'appId', header: t('gameBusiness.configScope.scope'), width: '200px', align: 'center', bodyKey: 'appId'},
  {field: 'payChannel', header: '支付渠道', width: '120px', align: 'center', bodyKey: 'payChannel'},
  {field: 'isSandbox', header: '环境', width: '110px', align: 'center', bodyKey: 'isSandbox'},
  {field: 'isEnabled', header: '状态', width: '100px', align: 'center', bodyKey: 'isEnabled'},
  {field: 'configJson', header: '配置', width: '120px', align: 'center'},
  {field: 'remark', header: '备注'},
  {field: 'createdTime', header: '创建时间', width: '180px', align: 'center', bodyKey: 'createdTime'},
])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
} = useCrudTable({
  api: {
    pageApi: (params) => tenantGamePaymentConfigEntityPagePost(params),
    deleteApi: (params) => deletePaymentConfigApi({id: params.id, rowVersion: params.rowVersion}),
  },
  immediate: false,
})

queryParams.value = {
  searchKey: undefined,
  appId: undefined,
  gameAppId: undefined,
  packageId: undefined,
  payChannel: undefined,
  isSandbox: undefined,
  isActive: undefined,
}
tableParams.value.pageSize = 10

const formatChannel = (channel: string) => {
  return channelOptions.value.find((item) => item.value === channel)?.label ?? channel
}

const openAddPaymentConfig = () => {
  const appId = normalizeAppId(selectedGameAppId.value)
  if (appId === undefined) {
    showWarning(t('gameBusiness.appWorkspace.appIdRequired'))
    return
  }
  editTitle.value = '新增支付配置'
  editDialogRef.value?.openDialog({
    appId,
    gameAppId: appId,
    packageId: selectedPackageId.value || undefined,
  })
}

const openEditPaymentConfig = async (row: any) => {
  editTitle.value = '编辑支付配置'
  // 列表 configJson 被脱敏为 ******，编辑需拉 Detail 获取真实配置才能回填字段
  try {
    const res: any = await tenantGamePaymentConfigEntityDetailGet({id: row.id})
    const detail = res.data.data
    if (detail && detail.configJson) {
      editDialogRef.value?.openDialog({...row, ...detail})
      return
    }
  } catch {
    // 忽略，降级用列表行
  }
  editDialogRef.value?.openDialog(row)
}

const onDeletePaymentConfig = async (row: any) => {
  const name = `${formatChannel(row.payChannel)} / ${paymentScopeLabel(row)} / ${row.isSandbox ? 'Sandbox' : 'Prod'}`
  const confirmed = await showConfirm(`确定要删除支付配置：【${name}】?`, t('common.message.tip'), {type: 'danger'})
  if (!confirmed) return
  await deletePaymentConfigApi({id: row.id, rowVersion: row.rowVersion})
  onQuery()
  showSuccess(t('message.common.message.deleteSuccess'))
}

const onCopyNotifyUrl = async (row: any) => {
  await copyText(buildNotifyUrl(row))
}

const actions = {
  edit: openEditPaymentConfig,
  delete: onDeletePaymentConfig,
}

const customActions: CrudAction[] = [
  {
    auth: 'copyNotifyUrl',
    label: '复制回调地址',
    icon: 'fa-solid fa-copy',
    onClick: onCopyNotifyUrl,
  },
]

const onPage = usePrimeTablePage(tableParams, onQuery)

const paymentScopeLabel = (row: any) => {
  const app = normalizeAppId(row.gameAppId) ?? normalizeAppId(row.appId)
  const pkg = row.packageId || 0
  if (app === undefined) {
    return t('gameBusiness.appWorkspace.appUnavailable')
  }
  const appText = appName(app) || `App ${app}`
  if (pkg === 0) {
    return `${t('gameBusiness.configScope.appOverride')}：${appText}`
  }
  const pkgText = packageMap.value[pkg] || `Package ${pkg}`
  return `${t('gameBusiness.configScope.packageOverride')}：${pkgText}`
}

const paymentScopeSeverity = (row: any) => {
  const app = normalizeAppId(row.gameAppId) ?? normalizeAppId(row.appId)
  const pkg = row.packageId || 0
  if (app === undefined) {
    return 'danger'
  }
  if (pkg === 0) {
    return 'success'
  }
  return 'warn'
}

const loadPackageOptions = async (appId: AppWorkspaceId) => {
  const id = normalizeAppId(appId)
  if (id === undefined) {
    packageOptions.value = []
    return
  }
  try {
    const res: any = await tenantGameAppPackageListGet({appId: id})
    const list = res.data?.data ?? []
    packageOptions.value = (list || []).map((p: any) => {
      const pkgId = Number(p.id)
      const pkgName = p.packageName || p.name || `Package ${pkgId}`
      packageMap.value[pkgId] = pkgName
      return {label: pkgName, value: pkgId}
    })
  } catch {
    packageOptions.value = []
  }
}

const applyScopeQuery = () => {
  const appId = normalizeAppId(selectedGameAppId.value)
  if (appId === undefined) {
    return
  }
  queryParams.value.appId = appId
  queryParams.value.gameAppId = appId
  queryParams.value.packageId = selectedPackageId.value || undefined
  onQuery()
}

watch(selectedGameAppId, async (value) => {
  selectedPackageId.value = 0
  packageOptions.value = []
  if (value) {
    await loadPackageOptions(value)
  }
  applyScopeQuery()
})

watch(selectedPackageId, () => {
  applyScopeQuery()
})

onMounted(async () => {
  await loadApps()
  const initialAppId = normalizeRouteAppId(route) ?? currentAppId.value
  const appId = normalizeAppId(initialAppId)
  if (appId === undefined) {
    showWarning(t('gameBusiness.appWorkspace.appIdRequired'))
    return
  }
  if (!appOptions.value.some((app: any) => String(normalizeAppId(app.id)) === String(appId) && app.isEnabled !== false)) {
    showWarning(t('gameBusiness.appWorkspace.appUnavailable'))
    return
  }
  setCurrentApp(appId)
  selectedGameAppId.value = appId

  try {
    const channelRes: any = await tenantGamePaymentConfigEntityChannelsGet()
    const channels = channelRes.data?.data ?? []
    channelOptions.value = (channels || []).map((item: any) => ({label: item.label, value: item.value}))
  } catch {
    channelOptions.value = []
  }

  applyScopeQuery()
})
</script>

<style scoped>
.payment-scope-toolbar {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  align-items: center;
}

.payment-scope-app {
  min-width: 220px;
}

.payment-scope-pkg {
  min-width: 220px;
}

.payment-inherit-message {
  margin-top: 8px;
}
</style>
