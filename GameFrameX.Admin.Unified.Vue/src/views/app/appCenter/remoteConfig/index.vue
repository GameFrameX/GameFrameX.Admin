<template>
  <div class="tenantGameAppRemoteConfig-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="tenantGameAppRemoteConfig"
            @query="runScopedQuery"
            
            @add="openAddTenantGameAppRemoteConfig"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" style="width: 100%" v-block-loading="loading" showGridlines stripedRows
                   rowKey="id" :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                   :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                   authPrefix="tenantGameAppRemoteConfig"
                   :actions="actions"
                   :customActions="customActions"
                   actionColumnWidth="220px"
                   :columns="columns"
                   :exportQuery="queryParams"
                   @page="onPage">
          <template #body-appId="{ data }">
            <Tag :value="getAppScopeLabel(data.appId)" :severity="getAppScopeSeverity(data.appId)"/>
          </template>
          <template #body-activeModeCode="{ data }">
            <Tag :value="getModeDisplayName(data.activeModeCode)"
                 :severity="getModeTagType(data.activeModeCode)"/>
          </template>
          <template #body-isEnabled="{ data }">
            <Tag :value="data.isEnabled === false ? t('gameBusiness.disable') : t('gameBusiness.enable')"
                 :severity="data.isEnabled === false ? 'danger' : 'success'"/>
          </template>
          <template #body-rolloutPercent="{ data }">{{ data.rolloutPercent ?? 100 }}%</template>
        </CrudDataTable>
        <editDialog
            ref="editDialogRef"
            :title="editTenantGameAppRemoteConfigTitle"
            @reloadTable="runScopedQuery"
        />
        <contentEditDialog
            ref="contentEditDialogRef"
            :title="contentEditTitle"
            @reloadTable="runScopedQuery"
        />
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="tenantGameAppRemoteConfig">
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import {computed, onMounted, ref} from 'vue'
import {useRoute} from 'vue-router'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'
import {useCurrentApp} from '../../../../composables/useCurrentApp.ts'
import editDialog from '/@/views/app/appCenter/remoteConfig/component/EditDialog.vue'
import contentEditDialog from '/@/views/app/appCenter/remoteConfig/component/ContentEditDialog.vue'
import { tenantGameAppRemoteConfigPagePost, tenantGameAppRemoteConfigDeletePost } from '../../../../api/tenantGameAppRemoteConfig'
import {tenantGameAppRemoteConfigModePagePost} from '../../../../api/tenantGameAppRemoteConfigMode'
import { globalLanguageSelectPost } from '../../../../api/globalLanguage'
import { tenantChannelDefinitionAvailableGet } from '../../../../api/tenantChannelDefinition'
import { globalPlatformSelectPost } from '../../../../api/globalPlatform'
import { tenantGameAppVersionDefinitionSelectPost } from '../../../../api/tenantGameAppVersionDefinition'
import { tenantGameAppPackageListGet } from '../../../../api/tenantGameAppPackage'
import {MAX_PAGE_SIZE, MAX_SELECT_SIZE} from '../../../../constants/pagination.ts'
import type {CrudAction} from '../../../../components/crud-data-table/types.ts'
import type {CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'
import {showWarning} from '../../../../utils/message.ts'
import {normalizeAppId, normalizeRouteAppId, type AppWorkspaceId} from '../../../../utils/appWorkspace.ts'
import { mapAvailableChannelDefinitions } from '../../../../utils/channelDefinition.ts'

const editDialogRef = ref()
const contentEditDialogRef = ref()
const editTenantGameAppRemoteConfigTitle = ref('')
const contentEditTitle = ref('')
const modeMap = ref<Record<string, any>>({})
const appMap = ref<Record<string, string>>({})
const route = useRoute()
const {appOptions, loadApps, currentAppId, setCurrentApp} = useCurrentApp()
const querySelections = ref<any>({
  platform: [],
  language: [],
  channel: [],
})

const {
  loading, tableData, queryParams, tableParams,
  onQuery, onDelete,
} = useCrudTable({
  api: {pageApi: tenantGameAppRemoteConfigPagePost, deleteApi: tenantGameAppRemoteConfigDeletePost},
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
  // appId 只来自 URL 解析（onMounted 里 normalizeRouteAppId），不作为查询表单字段暴露
  { key: 'channel', label: t('message.client.channelName'), type: 'select', options: querySelections.value.channel, placeholder: t('message.common.form.pleaseSelect') },
  { key: 'platform', label: t('message.client.platform'), type: 'select', options: querySelections.value.platform, placeholder: t('message.common.form.pleaseSelect') },
  { key: 'appVersion', label: t('message.client.version'), type: 'select', options: querySelections.value.version, placeholder: t('message.common.form.pleaseSelect'), advanced: true },
  { key: 'package', label: t('message.client.packageName'), type: 'select', options: querySelections.value.package, placeholder: t('message.common.form.pleaseSelect'), advanced: true },
  { key: 'language', label: t('message.client.languageName'), type: 'select', options: querySelections.value.language, placeholder: t('message.common.form.pleaseSelect'), advanced: true },
])

const getModeDisplayName = (code: string) => {
  if (!code) return '-'
  return modeMap.value[code]?.displayName || code
}

const getModeTagType = (code: string) => {
  if (code === 'review') return 'warn'
  if (code === 'formal') return 'success'
  return 'info'
}

const getAppScopeLabel = (appId: any) => {
  const id = normalizeAppId(appId)
  if (id === undefined) {
    return t('gameBusiness.appWorkspace.appUnavailable')
  }
  return appMap.value[String(id)] || String(id)
}

const getAppScopeSeverity = (appId: any) => {
  if (normalizeAppId(appId) === undefined) {
    return 'danger'
  }
  return 'success'
}

const loadModeDefs = async () => {
  const res = await tenantGameAppRemoteConfigModePagePost({
    page: 1,
    pageSize: MAX_PAGE_SIZE,
    isEnabled: true,
    field: 'sortOrder',
    order: 'ascending',
  })
  const defs = res.data.data?.items ?? []
  modeMap.value = {}
  for (const m of defs) {
    modeMap.value[m.modeCode] = m
  }
}

const onLanguageQuery = async () => {
  const res = await globalLanguageSelectPost({pageSize: MAX_SELECT_SIZE})
  querySelections.value.language = res.data.data
}

const onChannelQuery = async () => {
  const res = await tenantChannelDefinitionAvailableGet({pageSize: MAX_SELECT_SIZE})
  querySelections.value.channel = mapAvailableChannelDefinitions(res.data.data || [], 'code')
}

const onPlatformQuery = async () => {
  const res = await globalPlatformSelectPost({pageSize: MAX_SELECT_SIZE})
  querySelections.value.platform = res.data.data
}

const onVersionQuery = async (appId?: AppWorkspaceId) => {
  const id = normalizeAppId(appId)
  if (id === undefined) {
    querySelections.value.version = []
    querySelections.value.oldVersion = []
    return
  }
  const res = await tenantGameAppVersionDefinitionSelectPost({appId: id, pageSize: MAX_SELECT_SIZE})
  querySelections.value.version = res.data.data
  querySelections.value.oldVersion = res.data.data
}

// 包名候选按当前应用加载：tenantGameAppPackageListGet 已按应用过滤、默认包置顶。
// 当前应用未确认时不展示候选，避免展示其它应用包名。
const buildPackageOptions = (packages: any[]) => {
  const options: any[] = [{label: t('message.client.appDefaultPackage'), value: 'default'}]
  for (const p of packages || []) {
    const val = p.name ?? p.packageName
    if (val === null || val === undefined || val === '') {
      continue
    }
    if (options.some((o) => o.value === val)) {
      continue
    }
    const version = p.packageVersion ? ` (${p.packageVersion})` : ''
    options.push({label: `${p.packageName || p.name}${version}`, value: val})
  }
  return options
}

const onPackageQuery = async (appId?: AppWorkspaceId) => {
  const id = normalizeAppId(appId)
  if (id === undefined) {
    querySelections.value.package = []
    return
  }
  const res = await tenantGameAppPackageListGet({appId: id})
  querySelections.value.package = buildPackageOptions(res.data?.data || [])
}

const onAppQuery = async () => {
  await loadApps()
  const map: Record<string, string> = {}
  for (const a of appOptions.value) {
    const id = normalizeAppId(a.id)
    if (id === undefined || a.isEnabled === false) {
      continue
    }
    map[String(id)] = a.appName || a.name || String(id)
  }
  appMap.value = map
}

// 打开新增页面
const openAddTenantGameAppRemoteConfig = () => {
  const appId = requireCurrentQueryAppId()
  if (appId === undefined) {
    return
  }
  editTenantGameAppRemoteConfigTitle.value = t('message.client.addRemoteConfig')
  editDialogRef.value.isEditor = false
  editDialogRef.value.openDialog({appId})
}

// 打开编辑页面
const openEditTenantGameAppRemoteConfig = (row: any) => {
  if (normalizeAppId(row?.appId) === undefined) {
    showWarning(t('gameBusiness.appWorkspace.appUnavailable'))
    return
  }
  editTenantGameAppRemoteConfigTitle.value = t('message.client.editRemoteConfig')
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
  {field: 'appId', header: t('message.gameBusiness.app.appId'), width: '130px', align: 'center', bodyKey: 'appId'},
  {field: 'channel', header: t('message.client.channelName'), width: '105px'},
  {field: 'platform', header: t('message.client.platformName'), width: '90px'},
  {field: 'appVersion', header: t('message.client.newVersion'), width: '80px'},
  {field: 'package', header: t('message.client.packageName'), width: '105px'},
  {field: 'language', header: t('message.client.languageName'), width: '120px'},
  {field: 'checkResourceUrl', header: t('message.client.checkResourceUrl'), width: '240px'},
  {field: 'checkAppVersionUrl', header: t('message.client.checkAppVersionUrl'), width: '270px'},
  {field: 'description', header: t('message.common.label.description'), width: '165px'},
  {field: 'activeModeCode', header: t('message.client.currentMode'), width: '100px', align: 'center', bodyKey: 'activeModeCode'},
  {field: 'isEnabled', header: t('message.client.isEnabled'), width: '90px', align: 'center', bodyKey: 'isEnabled'},
  {field: 'priority', header: t('message.client.priority'), width: '90px', align: 'center'},
  {field: 'rolloutPercent', header: t('message.client.rolloutPercent'), width: '110px', align: 'center', bodyKey: 'rolloutPercent'},
  {field: 'createdName', header: t('message.common.label.creator'), width: '210px'},
  {field: 'updatedName', header: t('message.common.label.modifier'), width: '210px'},
])

const actions = {
  edit: openEditTenantGameAppRemoteConfig,
  delete: onScopedDelete,
}

const openContentEdit = (row: any) => {
  contentEditTitle.value = t('message.client.contentConfig')
  contentEditDialogRef.value.openDialog(row)
}

const customActions: CrudAction[] = [
  {
    auth: 'update',
    label: t('message.client.contentConfig'),
    icon: 'fa-solid fa-sliders',
    onClick: openContentEdit,
  },
]

onMounted(async () => {
  await onAppQuery()
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
  await Promise.all([
    onLanguageQuery(),
    onPlatformQuery(),
    onChannelQuery(),
    onVersionQuery(appId),
    onPackageQuery(appId),
    loadModeDefs(),
  ])
  runScopedQuery()
})
</script>
