<template>
  <div class="tenantGameAppFeatureSwitch-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="tenantGameAppFeatureSwitch"
            @query="runScopedQuery"

            @add="openAddFeatureSwitch"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" style="width: 100%" v-block-loading="loading" showGridlines stripedRows
                   rowKey="id" :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                   :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                   authPrefix="tenantGameAppFeatureSwitch"
                   :actions="actions"
                   actionColumnWidth="160px"
                   :columns="columns"
                   :exportQuery="queryParams"
                   @page="onPage">
          <template #body-audience="{ data }">
            <Tag :value="getAudienceLabel(data.audience)" :severity="getAudienceSeverity(data.audience)"/>
          </template>
          <template #body-scopeSummary="{ data }">{{ getScopeSummary(data) }}</template>
          <template #body-timeWindow="{ data }">{{ getTimeWindow(data) }}</template>
          <template #body-rolloutPercent="{ data }">{{ data.rolloutPercent ?? 100 }}%</template>
          <template #body-isEnabled="{ data }">
            <Tag :value="data.isEnabled === false ? t('gameBusiness.disable') : t('gameBusiness.enable')"
                 :severity="data.isEnabled === false ? 'danger' : 'success'"/>
          </template>
          <template #body-updateTime="{ data }">{{ formatUnix(data.updateTime) }}</template>
        </CrudDataTable>
        <editDialog
            ref="editDialogRef"
            :title="editFeatureSwitchTitle"
            :appId="activeAppId"
            @reloadTable="runScopedQuery"
        />
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="tenantGameAppFeatureSwitch">
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import {computed, onMounted, ref} from 'vue'
import {useRoute} from 'vue-router'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'
import {useCurrentApp} from '../../../../composables/useCurrentApp.ts'
import editDialog from '/@/views/app/appCenter/featureSwitch/component/EditDialog.vue'
import { tenantGameAppFeatureSwitchPagePost, tenantGameAppFeatureSwitchDeletePost } from '../../../../api/tenantGameAppFeatureSwitch'
import type {CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'
import {showWarning} from '../../../../utils/message.ts'
import {formatUnix} from '../../../../utils/formatTime.ts'
import {normalizeAppId, normalizeRouteAppId, type AppWorkspaceId} from '../../../../utils/appWorkspace.ts'

const editDialogRef = ref()
const editFeatureSwitchTitle = ref('')
const route = useRoute()
const {appOptions, loadApps, currentAppId, setCurrentApp} = useCurrentApp()
const appMap = ref<Record<string, string>>({})

const {
  loading, tableData, queryParams, tableParams,
  onQuery, onDelete,
} = useCrudTable({
  api: {pageApi: tenantGameAppFeatureSwitchPagePost, deleteApi: tenantGameAppFeatureSwitchDeletePost},
  immediate: false,
  deleteConfirmLabelFields: ['switchKey', 'displayName', 'id'],
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

// 弹窗 appId 绑定：新增表单无行数据，包名候选/提交归属按当前作用域应用兜底
const activeAppId = computed(() => currentQueryAppId())

const onPage = usePrimeTablePage(tableParams, runScopedQuery)

const enabledOptions = computed(() => [
  {label: t('message.gameBusiness.enable'), value: true},
  {label: t('message.gameBusiness.disable'), value: false},
])

const audienceOptions = computed(() => [
  {label: t('message.featureSwitch.audienceBoth'), value: 'both'},
  {label: t('message.featureSwitch.audienceClient'), value: 'client'},
  {label: t('message.featureSwitch.audienceServer'), value: 'server'},
])

const searchFields = computed(() => [
  // appId 只来自 URL 解析（onMounted 里 normalizeRouteAppId），不作为查询表单字段暴露
  { key: 'switchKey', label: t('message.featureSwitch.switchKey'), type: 'input', placeholder: t('message.featureSwitch.switchKeyPlaceholder') },
  { key: 'audience', label: t('message.featureSwitch.audience'), type: 'select', options: audienceOptions.value, optionLabel: 'label', optionValue: 'value', placeholder: t('message.common.form.pleaseSelect') },
  { key: 'isEnabled', label: t('message.client.isEnabled'), type: 'select', options: enabledOptions.value, optionLabel: 'label', optionValue: 'value', placeholder: t('message.common.form.pleaseSelect') },
])

const getAudienceLabel = (audience: any) => {
  const found = audienceOptions.value.find((o) => o.value === audience)
  return found ? found.label : String(audience ?? '-')
}

const getAudienceSeverity = (audience: any) => {
  if (audience === 'client') return 'info'
  if (audience === 'server') return 'warn'
  return 'success'
}

// 作用域维度值 default 为后端「不限」哨兵
const SCOPE_DEFAULT = 'default'

const getScopeValue = (value: any) => {
  if (value === null || value === undefined || value === '' || value === SCOPE_DEFAULT) {
    return t('message.featureSwitch.scopeUnlimited')
  }
  return String(value)
}

// 作用域摘要：平台·包名·语言·渠道 四维拼接，default 显示「不限」
const getScopeSummary = (row: any) => {
  return [row.platform, row.package, row.language, row.channel]
      .map((v) => getScopeValue(v))
      .join(' · ')
}

// 时间窗：两端均空显示 -；任一端有值按「起 ~ 止」拼接，空端显示 ∞（该端不限）
const getTimeWindow = (row: any) => {
  if (!row.startTime && !row.endTime) {
    return '-'
  }
  return `${row.startTime ? formatUnix(row.startTime) : '∞'} ~ ${row.endTime ? formatUnix(row.endTime) : '∞'}`
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
const openAddFeatureSwitch = () => {
  const appId = requireCurrentQueryAppId()
  if (appId === undefined) {
    return
  }
  editFeatureSwitchTitle.value = t('message.featureSwitch.addTitle')
  editDialogRef.value.isEditor = false
  // 空对象走 defaultForm 默认值（audience/rollout/isEnabled），appId 由 props 兜底注入
  editDialogRef.value.openDialog({})
}

// 打开编辑页面
const openEditFeatureSwitch = (row: any) => {
  editFeatureSwitchTitle.value = t('message.featureSwitch.editTitle')
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
  {field: 'switchKey', header: t('message.featureSwitch.switchKey'), width: '160px'},
  {field: 'displayName', header: t('message.featureSwitch.displayName'), width: '140px'},
  {field: 'audience', header: t('message.featureSwitch.audience'), width: '110px', align: 'center', bodyKey: 'audience'},
  {field: 'scopeSummary', header: t('message.featureSwitch.scope'), width: '220px', bodyKey: 'scopeSummary'},
  {field: 'timeWindow', header: t('message.featureSwitch.timeWindow'), width: '300px', bodyKey: 'timeWindow'},
  {field: 'rolloutPercent', header: t('message.client.rolloutPercent'), width: '100px', align: 'center', bodyKey: 'rolloutPercent'},
  {field: 'isEnabled', header: t('message.client.isEnabled'), width: '90px', align: 'center', bodyKey: 'isEnabled'},
  {field: 'rowVersion', header: t('message.featureSwitch.rowVersion'), width: '120px', align: 'center'},
  {field: 'updatedName', header: t('message.common.label.modifier'), width: '180px'},
  {field: 'updateTime', header: t('message.common.label.updateTime'), width: '170px', bodyKey: 'updateTime'},
])

const actions = {
  edit: openEditFeatureSwitch,
  delete: onScopedDelete,
}

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
  runScopedQuery()
})
</script>
