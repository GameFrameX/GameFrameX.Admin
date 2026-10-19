<template>
  <div class="gameAppOpenAccess-container">
    <Card>
      <template #content>
        <SearchForm v-model="queryParams" :fields="searchFields" :defaultSearchKey="false"
                    authPrefix="gameAppOpenAccess" :showAdd="true" @query="onQuery" @add="openAdd">
        </SearchForm>
      </template>
    </Card>

    <Card style="margin-top: 8px">
      <template #header>
        <div class="flex justify-content-between align-items-center w-full">
          <span>{{ t('message.gameBusiness.openAccess.listTitle') }}</span>
        </div>
      </template>
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows style="width: 100%"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize" :rowsPerPageOptions="[10, 20, 50, 100]"
                       authPrefix="gameAppOpenAccess"
                       :actions="actions"
                       :customActions="customActions"
                       actionColumnWidth="280px"
                       :columns="columns"
                       @page="onPage" showIdColumn idColumnWidth="180px">
          <template #body-scopes="{ data }">
            <div class="flex flex-wrap gap-1">
              <Tag v-for="scope in parseScopes(data.scopesJson)" :key="scope" severity="info" :value="scope" />
            </div>
          </template>
          <template #body-isEnabled="{ data }">
            <Tag v-if="data.revokedTime" severity="danger" :value="t('message.gameBusiness.openAccess.revoked')" />
            <Tag v-else-if="data.isEnabled !== false" severity="success" :value="t('message.gameBusiness.openAccess.enabled')" />
            <Tag v-else severity="warn" :value="t('message.gameBusiness.openAccess.disabled')" />
          </template>
          <template #body-expiresTime="{ data }">
            <span v-if="data.expiresTime">{{ formatUnix(data.expiresTime) }}</span>
            <span v-else class="text-color-secondary">{{ t('message.gameBusiness.openAccess.noExpiry') }}</span>
          </template>
          <template #body-revokedTime="{ data }">{{ formatUnix(data.revokedTime) }}</template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>

        <editDialog ref="editDialogRef" :title="editTitle" :appId="queryParams.appId"
                    @reloadTable="onQuery" @secret="onSecret" />
        <secretDialog ref="secretDialogRef" />
        <actionDialog ref="actionDialogRef" :appId="queryParams.appId"
                      @done="onQuery" @secret="onSecret" />
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="gameAppOpenAccess">
import { computed, ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import Tag from 'primevue/tag'
import Card from 'primevue/card'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type { CrudAction, CrudColumnConfig } from '../../../../components/crud-data-table/types.ts'
import { createAction } from '../../../../components/crud-data-table'
import SearchForm from '/@/components/search-form/index.vue'
import { useCrudTable } from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage, useRouteAppIdQuerySync } from '../../../../composables'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import { showConfirm, showSuccess } from '../../../../utils/message.ts'
import { formatUnix } from '../../../../utils/formatTime.ts'
import { t } from '../../../../i18n/index.ts'
import { tenantGameAppOpenAccessPagePost, tenantGameAppOpenAccessDeletePost } from '../../../../api/tenantGameAppOpenAccess'
import editDialog from './component/EditDialog.vue'
import secretDialog from './component/SecretDialog.vue'
import actionDialog from './component/ActionDialog.vue'

const route = useRoute()
const editDialogRef = ref()
const secretDialogRef = ref()
const actionDialogRef = ref()
const editTitle = ref('')

const { ensureCurrentAppFromQuery } = useCurrentApp()

// isEnabled 过滤选项
const statusOptions = computed(() => [
  { label: t('message.gameBusiness.openAccess.enabled'), value: 'true' },
  { label: t('message.gameBusiness.openAccess.disabled'), value: 'false' },
])

const searchFields = computed(() => [
  { key: 'scenario', label: t('message.gameBusiness.openAccess.scenario'), type: 'input' as const, placeholder: t('message.gameBusiness.openAccess.pleaseEnterScenarioSearch') },
  { key: 'subjectName', label: t('message.gameBusiness.openAccess.subjectName'), type: 'input' as const, placeholder: t('message.gameBusiness.openAccess.pleaseEnterSubjectNameSearch') },
  { key: 'accessKey', label: t('message.gameBusiness.openAccess.accessKey'), type: 'input' as const, placeholder: t('message.gameBusiness.openAccess.pleaseEnterAccessKeySearch') },
  { key: 'isEnabled', label: t('message.gameBusiness.openAccess.isEnabled'), type: 'select' as const, options: statusOptions.value, optionLabel: 'label', optionValue: 'value', placeholder: t('message.gameBusiness.openAccess.pleaseSelectStatus') },
])

const columns = computed<CrudColumnConfig[]>(() => [
  { field: 'scenario', header: t('message.gameBusiness.openAccess.scenario'), width: '140px' },
  { field: 'purpose', header: t('message.gameBusiness.openAccess.purpose'), width: '160px' },
  { field: 'subjectName', header: t('message.gameBusiness.openAccess.subjectName'), width: '140px' },
  { field: 'accessKey', header: t('message.gameBusiness.openAccess.accessKey'), width: '180px' },
  { field: 'bindUserName', header: t('message.gameBusiness.openAccess.bindUser'), width: '120px' },
  { field: 'scopesJson', header: t('message.gameBusiness.openAccess.scopes'), width: '180px', bodyKey: 'scopes' },
  { field: 'isEnabled', header: t('message.gameBusiness.openAccess.isEnabled'), width: '100px', align: 'center', bodyKey: 'isEnabled' },
  { field: 'expiresTime', header: t('message.gameBusiness.openAccess.expiresTime'), width: '160px', bodyKey: 'expiresTime' },
  { field: 'revokedTime', header: t('message.gameBusiness.openAccess.revokedTime'), width: '160px', bodyKey: 'revokedTime' },
  { field: 'createdTime', header: t('common.label.createTime'), width: '160px', bodyKey: 'createdTime' },
])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
} = useCrudTable({
  api: {
    pageApi: (params: any) => tenantGameAppOpenAccessPagePost(params),
  },
  immediate: false,
})

// 初始化 appId 为当前应用
queryParams.value = {
  scenario: undefined,
  subjectName: undefined,
  accessKey: undefined,
  isEnabled: undefined,
  appId: ensureCurrentAppFromQuery(route.query),
}

tableParams.value.pageSize = 10

const parseScopes = (scopesJson: string | null | undefined): string[] => {
  if (!scopesJson) return []
  try {
    const parsed = JSON.parse(scopesJson)
    return Array.isArray(parsed) ? parsed : []
  } catch {
    return []
  }
}

const onPage = usePrimeTablePage(tableParams, onQuery)

// ---- 签发 / 编辑 ----
const openAdd = () => {
  editTitle.value = t('message.gameBusiness.openAccess.issueTitle')
  editDialogRef.value.isEditor = false
  editDialogRef.value.openDialog({})
}

const openEdit = (row: any) => {
  editTitle.value = t('message.gameBusiness.openAccess.editTitle')
  editDialogRef.value.isEditor = true
  editDialogRef.value.openDialog(row)
}

// ---- 删除 ----
const onDelete = async (row: any) => {
  const confirmed = await showConfirm(
    t('message.gameBusiness.openAccess.deleteConfirm'),
    t('message.common.message.tip'),
  )
  if (!confirmed) return
  await tenantGameAppOpenAccessDeletePost({ id: row.id, appId: queryParams.value.appId, rowVersion: row.rowVersion })
  onQuery()
  showSuccess(t('message.gameBusiness.openAccess.deleteSuccess'))
}

// ---- 吊销 / 启用 / 禁用 / 轮换 ----
const onRevoke = (row: any) => {
  actionDialogRef.value?.openDialog('revoke', row)
}
const onEnable = (row: any) => {
  actionDialogRef.value?.openDialog('enable', row)
}
const onDisable = (row: any) => {
  actionDialogRef.value?.openDialog('disable', row)
}
const onRefresh = (row: any) => {
  actionDialogRef.value?.openDialog('refresh', row)
}

// 可见性条件
const canRevoke = (row: any) => !row.revokedTime
const canEnable = (row: any) => !row.revokedTime && row.isEnabled === false
const canDisable = (row: any) => !row.revokedTime && row.isEnabled !== false

const customActions: CrudAction[] = [
  createAction('revoke', onRevoke, {
    label: t('message.gameBusiness.openAccess.revoke'),
    icon: 'fa-solid fa-ban',
    severity: 'danger',
    visible: canRevoke,
  }),
  createAction('enable', onEnable, {
    label: t('message.gameBusiness.openAccess.enable'),
    icon: 'fa-solid fa-circle-check',
    severity: 'success',
    visible: canEnable,
  }),
  createAction('disable', onDisable, {
    label: t('message.gameBusiness.openAccess.disable'),
    icon: 'fa-solid fa-circle-xmark',
    severity: 'warn',
    visible: canDisable,
  }),
  createAction('refresh', onRefresh, {
    label: t('message.gameBusiness.openAccess.refresh'),
    icon: 'fa-solid fa-rotate',
    severity: 'info',
  }),
]

// ---- Secret 一次性展示 ----
const onSecret = (secretResult: any) => {
  secretDialogRef.value?.open(secretResult)
}

const actions = {
  edit: openEdit,
  delete: onDelete,
}

// ---- 路由应用切换 ----
useRouteAppIdQuerySync({
  route,
  queryParams,
  resolveAppId: ensureCurrentAppFromQuery,
  onQuery: onQuery,
})

onMounted(() => {
  onQuery()
})
</script>
