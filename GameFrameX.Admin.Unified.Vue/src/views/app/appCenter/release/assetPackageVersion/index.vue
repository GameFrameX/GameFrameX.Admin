<template>
  <div class="tenantGameAppAssetPackageVersion-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="tenantGameAppAssetPackageVersion"
            :advancedLabel="t('message.client.advanced')"
            :hideLabel="t('message.client.hide')"
            :defaultSearchKey="false"
            @query="onQuery"
            
            @add="openAddTenantGameAppAssetPackageVersion"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows rowKey="id"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="tenantGameAppAssetPackageVersion"
                       :actions="actions"
                       :customActions="customActions"
                       actionColumnWidth="200px"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage">
          <template #body-masterPackage="{ data }">
            <template v-if="packageInfoMap[data.package]">
              <span v-if="!packageInfoMap[data.package].isMaster">{{
                  packageInfoMap[data.package].masterName
                }}</span>
              <Tag v-else :value="t('message.client.masterPackage')" severity="info"/>
            </template>
          </template>
        </CrudDataTable>
        <editDialog
            ref="editDialogRef"
            :title="editTenantGameAppAssetPackageVersionTitle"
            :appId="queryParams.appId"
            @reloadTable="onQuery"
        />
        <updateHistoryDialog ref="updateHistoryDialogRef" :title="historyTenantGameAppAssetPackageVersionTitle"
                             @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="tenantGameAppAssetPackageVersion">
import {computed, ref, watch} from 'vue'
import {useRoute} from 'vue-router'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type { CrudAction, CrudColumnConfig } from '../../../../../components/crud-data-table/types.ts'
import {useCrudTable} from '../../../../../composables/useCrudTable.ts'
import {usePrimeTablePage, useRouteAppIdQuerySync} from '../../../../../composables'
import {useCurrentApp} from '../../../../../composables/useCurrentApp.ts'
import editDialog from '/@/views/app/appCenter/release/assetPackageVersion/component/EditDialog.vue'
import updateHistoryDialog from '/@/views/app/appCenter/release/assetPackageVersion/component/UpdateHistory.vue'
import { tenantGameAppAssetPackageVersionPagePost, tenantGameAppAssetPackageVersionDeletePost } from '../../../../../api/tenantGameAppAssetPackageVersion'
import { tenantChannelDefinitionAvailableGet } from '../../../../../api/tenantChannelDefinition'
import { globalPlatformSelectPost } from '../../../../../api/globalPlatform'
import { tenantGameAppVersionDefinitionSelectPost } from '../../../../../api/tenantGameAppVersionDefinition'
import { tenantGameAppPackageListGet, tenantGameAppPackagePagePost } from '../../../../../api/tenantGameAppPackage'
import { globalLanguageSelectPost } from '../../../../../api/globalLanguage'
import { tenantGameAppAssetPackageSelectPost } from '../../../../../api/tenantGameAppAssetPackage'
import {MAX_PAGE_SIZE, MAX_SELECT_SIZE} from '../../../../../constants/pagination.ts'
import { mapAvailableChannelDefinitions } from '../../../../../utils/channelDefinition.ts'

const route = useRoute()
const {resolveInitialAppId} = useCurrentApp()

const editDialogRef = ref()
const updateHistoryDialogRef = ref()
const editTenantGameAppAssetPackageVersionTitle = ref('')
const historyTenantGameAppAssetPackageVersionTitle = ref('')
const querySelectorParams = ref<any>({app: []})
const packageInfoMap = ref<Record<string, { isMaster: boolean; masterName?: string }>>({})

// 加载包名母包映射
const loadPackageInfo = async (appId?: number | string) => {
  try {
    const id = appId
    const payload: any = {page: 1, pageSize: MAX_PAGE_SIZE}
    if (id) {
      payload.appId = id
    }
    const res = await tenantGameAppPackagePagePost(payload)
    const items = res.data.data?.items ?? []
    const map: Record<string, { isMaster: boolean; masterName?: string }> = {}
    items.forEach((p: any) => {
      const info = p.masterPackageId
        ? {isMaster: false, masterName: p.masterPackageName}
        : {isMaster: true}
      const keys = [p.name, p.packageName].filter((x) => x !== null && x !== undefined && x !== '')
      keys.forEach((key) => {
        map[String(key)] = info
      })
      if (!keys.length && p.id) {
        map[String(p.id)] = info
      }
    })
    packageInfoMap.value = map
  } catch {
  }
}

const {
  loading, tableData, queryParams, tableParams,
  onQuery, onDelete,
} = useCrudTable({
  api: {pageApi: tenantGameAppAssetPackageVersionPagePost, deleteApi: tenantGameAppAssetPackageVersionDeletePost},
  immediate: true,
})

const onPage = usePrimeTablePage(tableParams, onQuery)

const searchFields = computed(() => [
  { key: 'channel', label: t('message.client.channelName'), type: 'select', options: querySelectorParams.value.channel, placeholder: t('message.client.pleaseSelectChannelPlaceholder'), advanced: true },
  { key: 'platform', label: t('message.client.platform'), type: 'select', options: querySelectorParams.value.platform, placeholder: t('message.client.pleaseSelectPlatformPlaceholder'), advanced: true },
  { key: 'appVersion', label: t('message.client.gameVersion'), type: 'select', options: querySelectorParams.value.appVersion, placeholder: t('message.client.pleaseSelectGameVersionPlaceholder'), advanced: true },
  { key: 'package', label: t('message.client.packageName'), type: 'select', options: querySelectorParams.value.package, placeholder: t('message.client.pleaseSelectPackagePlaceholder'), advanced: true },
  { key: 'language', label: t('message.client.language'), type: 'select', options: querySelectorParams.value.language, placeholder: t('message.client.pleaseSelectLanguagePlaceholder'), advanced: true },
  { key: 'assetPackageName', label: t('message.client.assetPackage'), type: 'select', options: querySelectorParams.value.assetPackageName, placeholder: t('message.client.pleaseSelectAssetPackagePlaceholder'), advanced: true },
])

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'channel', header: t('message.client.channelName'), width: '140px'},
  {field: 'platform', header: t('message.client.platform'), width: '200px'},
  {field: 'appVersion', header: t('message.client.gameVersion'), width: '140px'},
  {field: 'package', header: t('message.client.packageName'), width: '300px'},
  {field: 'masterPackage', header: t('message.client.masterPackage'), width: '300px', bodyKey: 'masterPackage'},
  {field: 'language', header: t('message.client.language'), width: '140px'},
  {field: 'assetPackageName', header: t('message.client.assetPackage'), width: '140px'},
  {field: 'resourceVersion', header: t('message.client.assetVersion'), width: '200px'},
  {field: 'rootPath', header: t('message.client.resourceRootPath'), width: '140px'},
  {field: 'description', header: t('message.client.detailedDescription'), width: '140px'},
  {field: 'createdName', header: t('message.common.label.creator'), width: '140px'},
  {field: 'updatedName', header: t('message.common.label.modifier'), width: '140px'},
])

// 打开新增页面
const openAddTenantGameAppAssetPackageVersion = () => {
  editTenantGameAppAssetPackageVersionTitle.value = t('message.client.addAssetPackageVersion')
  editDialogRef.value.isEditor = false
  editDialogRef.value.openDialog({appId: queryParams.value.appId})
}

// 打开编辑页面
const openEditTenantGameAppAssetPackageVersion = (row: any) => {
  editTenantGameAppAssetPackageVersionTitle.value = t('message.client.editAssetPackageVersion')
  editDialogRef.value.isEditor = true
  editDialogRef.value.openDialog(row)
}

const actions = {
  edit: openEditTenantGameAppAssetPackageVersion,
  delete: onDelete,
}

// 打开历史记录页面（需要在 customActions 之前定义）
const openTenantGameAppAssetPackageVersionHistory = (row: any) => {
  historyTenantGameAppAssetPackageVersionTitle.value = t('message.client.historyRecord')
  updateHistoryDialogRef.value.openDialog(row)
}

const customActions: CrudAction[] = [
  {
    auth: 'history',
    label: t('message.client.history'),
    icon: 'fa-solid fa-file',
    onClick: openTenantGameAppAssetPackageVersionHistory,
  },
]

const onQuerySelectorChannel = async () => {
  const res = await tenantChannelDefinitionAvailableGet({pageSize: MAX_SELECT_SIZE})
  querySelectorParams.value.channel = mapAvailableChannelDefinitions(res.data.data || [], 'code')
}
const onQuerySelectorPlatform = async () => {
  const res = await globalPlatformSelectPost({pageSize: MAX_SELECT_SIZE})
  querySelectorParams.value.platform = res.data.data
}
const onQuerySelectorAppVersion = async (appId?: number | string) => {
  const id = appId
  const res = await tenantGameAppVersionDefinitionSelectPost(id ? {appId: id, pageSize: MAX_SELECT_SIZE} : {pageSize: MAX_SELECT_SIZE})
  querySelectorParams.value.appVersion = res.data.data
}
// 包名候选按当前应用加载：tenantGameAppPackageListGet 已按应用过滤、默认包置顶；
// 无应用或租户默认(appId=0)时只保留 default 应用默认选项，避免展示其它应用包名。
const buildPackageOptions = (packages: any[]) => {
  const options: any[] = [{label: t('message.client.appDefaultPackage'), value: 'default'}]
  for (const p of packages || []) {
    const val = p.packageName ?? p.name
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

const onQuerySelectorPackage = async (appId?: number | string) => {
  const id = appId
  if (!id) {
    querySelectorParams.value.package = buildPackageOptions([])
    return
  }
  const res = await tenantGameAppPackageListGet({appId: id, pageSize: MAX_SELECT_SIZE})
  querySelectorParams.value.package = buildPackageOptions(res.data?.data || [])
}
const onQuerySelectorLanguage = async () => {
  const res = await globalLanguageSelectPost({pageSize: MAX_SELECT_SIZE})
  querySelectorParams.value.language = res.data.data
}
const onQuerySelectorAssetPackageName = async (appId?: number | string) => {
  const id = appId
  const res = await tenantGameAppAssetPackageSelectPost(id ? {appId: id, pageSize: MAX_SELECT_SIZE} : {pageSize: MAX_SELECT_SIZE})
  querySelectorParams.value.assetPackageName = res.data.data
}

// 切换应用：清空包名筛选并按新应用重载包名候选，避免残留其它应用的包。
watch(() => queryParams.value?.appId, (next) => {
  if (queryParams.value) {
    queryParams.value.package = undefined
    queryParams.value.assetPackageName = undefined
  }
  onQuerySelectorPackage(next)
  onQuerySelectorAppVersion(next)
  onQuerySelectorAssetPackageName(next)
  loadPackageInfo(next)
})

useRouteAppIdQuerySync({
  route,
  queryParams,
  resolveAppId: resolveInitialAppId,
  onQuery,
})

onQuerySelectorChannel()
onQuerySelectorPlatform()
onQuerySelectorAppVersion(queryParams.value?.appId)
onQuerySelectorPackage(queryParams.value?.appId)
onQuerySelectorLanguage()
onQuerySelectorAssetPackageName(queryParams.value?.appId)
loadPackageInfo(queryParams.value?.appId)
</script>
