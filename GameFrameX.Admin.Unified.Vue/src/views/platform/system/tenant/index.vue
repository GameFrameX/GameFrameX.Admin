<template>
  <div class="sys-tenant-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            :defaultSearchKey="false"
            authPrefix="systemTenant"
            @query="onQuery"
            @reset="resetQuery"
            @add="openAddTenant"
        />
      </template>
    </Card>

    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows style="width: 100%"
            :totalRecords="tableParams.total" :rows="tableParams.pageSize" :rowsPerPageOptions="[10, 20, 50, 100]"
            authPrefix="systemTenant"
            :customActions="customActions"
            :columns="columns"
            :exportQuery="queryParams"
            actionColumnWidth="180px"
            @page="onPage" showIdColumn :idColumnHeader="t('common.label.id')" idColumnWidth="180px">
          <template #body-tenantType="{ data }">
            <Tag
              :severity="isMainTenant(data) ? 'info' : 'success'"
              :value="isMainTenant(data) ? t('system.tenant.mainTenant') : t('system.tenant.dbIsolation')"
            />
          </template>
          <template #body-isEnabled="{ data }">
            <Tag
              :severity="isTenantEnabled(data) ? 'success' : 'danger'"
              :value="isTenantEnabled(data) ? t('system.tenant.enabled') : t('system.tenant.disabled')"
            />
          </template>
          <template #body-dbType="{ data }">
            <Tag v-if="data.dbType === 0" value="MySql"/>
            <Tag v-else-if="data.dbType === 1" value="SqlServer"/>
            <Tag v-else-if="data.dbType === 2" value="Sqlite"/>
            <Tag v-else-if="data.dbType === 3" value="Oracle"/>
            <Tag v-else-if="data.dbType === 4" value="PostgreSQL"/>
            <Tag v-else-if="data.dbType === 5" value="Dm"/>
            <Tag v-else-if="data.dbType === 6" value="Kdbndp"/>
            <Tag v-else-if="data.dbType === 7" value="Oscar"/>
            <Tag v-else-if="data.dbType === 8" value="MySqlConnector"/>
            <Tag v-else-if="data.dbType === 9" value="Access"/>
            <Tag v-else-if="data.dbType === 10" value="OpenGauss"/>
            <Tag v-else-if="data.dbType === 11" value="QuestDB"/>
            <Tag v-else-if="data.dbType === 12" value="HG"/>
            <Tag v-else-if="data.dbType === 13" value="ClickHouse"/>
            <Tag v-else-if="data.dbType === 14" value="GBase"/>
            <Tag v-else-if="data.dbType === 900" value="Custom"/>
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
          <template #body-expireTime="{ data }">
            <div>
              {{ formatUnix(data.expireTime) }}
              <Tag v-if="isTenantExpired(data)" severity="danger" :value="t('system.tenant.expired')" style="margin-left: 4px"/>
            </div>
          </template>
          <template #body-menuSyncStatus="{ data }">
            <div v-tooltip="menuSyncTooltip(data)">
              <Tag :severity="menuSyncSeverity(data)" :value="menuSyncLabel(data)"/>
              <div class="menu-sync-time">{{ formatUnix(data.menuSyncLastSyncTime) }}</div>
            </div>
          </template>
          <template #action-extra="{ data }">
            <Button link severity="danger" @click="createTenant(data)" v-auth="'systemTenant:createDb'"
                    :disabled="isMainTenant(data) || isTenantDatabaseCreated(data)"
                    :title="isTenantDatabaseCreated(data) ? t('system.tenant.tenantDbCreated') : ''">
              <i class="fa-solid fa-coins mr-1"></i>
              {{ t('system.tenant.createDb') }}
            </Button>
            <Button v-if="isMenuSyncFailed(data)" link @click="retryMenuSync(data)" :loading="isMenuSyncRetrying(data)">
              <i class="fa-solid fa-rotate-right mr-1"></i>
              {{ t('system.tenant.menuSyncRetry') }}
            </Button>
            <Button link @click="openEditTenant(data)" v-auth="'systemTenant:update'">
              <i class="fa-solid fa-pen-to-square mr-1"></i>
              {{ t('common.button.edit') }}
            </Button>
            <Button link @click="toggleMenu($event, data)">
              <i class="fa-solid fa-ellipsis-vertical"></i>
            </Button>
          </template>
        </CrudDataTable>
      </template>
    </Card>

    <Menu ref="menuRef" :model="menuItems" :popup="true"/>
    <EditTenant ref="editTenantRef" :title="editTitle" @query="onQuery"/>
  </div>
</template>

<script lang="ts" setup name="systemTenant">
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import SearchForm from '/@/components/search-form/index.vue'

import {computed, ref, reactive} from 'vue'
import {showConfirm, showSuccess} from '../../../../utils/message.ts'
import {formatUnix} from '../../../../utils/formatTime.ts'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'
import {systemTenantPagePost, systemTenantDeletePost, systemTenantResetPwdPost, systemTenantCreateDbPost, systemTenantSetStatusPost} from '../../../../api/systemTenant'
import {tenantMenuSyncSyncTenantPost} from '../../../../api/tenantMenuSync'
import EditTenant from './component/EditTenant.vue'
import type {CrudAction, CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'

const editTenantRef = ref<InstanceType<typeof EditTenant>>()
const editTitle = ref('')
const menuRef = ref()
const currentRow = ref<any>(null)
const isMainTenant = (row: any) => row?.id === 1300000000001
const isTenantEnabled = (row: any) => row?.isEnabled !== false
const isTenantExpired = (row: any) => {
  if (!row?.expireTime) return false
  return row.expireTime > 0 && row.expireTime <= Math.floor(Date.now() / 1000)
}
const isTenantDatabaseCreated = (row: any) => Number(row?.userId ?? 0) > 0 && Number(row?.orgId ?? 0) > 0
const summarizeConnection = (connection?: string) => {
  if (!connection) return '-'
  const visibleKeys = new Set(['host', 'server', 'data source', 'datasource', 'database', 'initial catalog'])
  const parts = connection
      .split(';')
      .map((part) => part.trim())
      .filter(Boolean)
      .map((part) => {
        const index = part.indexOf('=')
        if (index < 0) return null
        const key = part.slice(0, index).trim()
        const normalizedKey = key.toLowerCase()
        if (!visibleKeys.has(normalizedKey)) return null
        const value = part.slice(index + 1).trim()
        return `${key}=${value}`
      })
      .filter(Boolean)

  return parts.length > 0 ? parts.join('; ') : '******'
}
const searchFields = computed(() => [
  { key: 'name', label: t('system.tenant.tenantName'), type: 'input', placeholder: t('system.tenant.tenantName') },
  { key: 'phone', label: t('system.tenant.contactPhone'), type: 'input', placeholder: t('system.tenant.contactPhone') },
])

const menuItems = computed(() => {
  const row = currentRow.value
  const items: any[] = []
  if (!isMainTenant(row)) {
    const enabled = isTenantEnabled(row)
    items.push({
      label: () => (enabled ? t('system.tenant.disable') : t('system.tenant.enable')),
      icon: enabled ? 'fa-solid fa-pause' : 'fa-solid fa-play',
      command: () => toggleTenantStatus(row),
    })
  }
  items.push(
    {
      label: () => t('system.user.resetPwd'),
      icon: 'fa-solid fa-rotate-left',
      command: () => resetTenantPwd(row),
    },
    {
      label: () => t('system.tenant.deleteTenant'),
      icon: 'fa-solid fa-trash',
      command: () => delTenant(row),
    },
  )
  return items
})

const toggleMenu = (event: any, row: any) => {
  currentRow.value = row
  menuRef.value.toggle(event)
}

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
} = useCrudTable({
  api: {
    pageApi: (params) => systemTenantPagePost(params),
    deleteApi: (params) => systemTenantDeletePost({id: params.id}),
  },
  immediate: true,
})

// C355：TenantMenuSyncStatusEnum 三态契约（1=待同步 2=同步成功 3=同步失败；同步队列在内存、无 Running 持久态，
// 重试请求 pending 期间由前端渲染「同步中」过渡态）
const MENU_SYNC_SUCCEEDED = 2
const MENU_SYNC_FAILED = 3
const retryingTenantId = ref<string | null>(null)

const isMenuSyncFailed = (row: any) => row?.menuSyncStatus === MENU_SYNC_FAILED
const isMenuSyncRetrying = (row: any) => retryingTenantId.value != null && row?.id === retryingTenantId.value

const menuSyncLabel = (row: any) => {
  if (isMenuSyncRetrying(row)) return t('system.tenant.menuSyncRunning')
  if (row?.menuSyncStatus === MENU_SYNC_SUCCEEDED) return t('system.tenant.menuSyncSucceeded')
  if (row?.menuSyncStatus === MENU_SYNC_FAILED) return t('system.tenant.menuSyncFailed')
  return t('system.tenant.menuSyncPending')
}

const menuSyncSeverity = (row: any): 'info' | 'success' | 'danger' => {
  if (row?.menuSyncStatus === MENU_SYNC_SUCCEEDED) return 'success'
  if (row?.menuSyncStatus === MENU_SYNC_FAILED) return 'danger'
  return 'info'
}

// 仅 failed 行提供失败原因与下次自动重试时间（周期扫描兜底自愈，重试只是把恢复提前）
const menuSyncTooltip = (row: any) => {
  if (row?.menuSyncStatus !== MENU_SYNC_FAILED) return undefined
  const parts: string[] = []
  if (row.menuSyncLastError) parts.push(`${t('system.tenant.menuSyncLastError')}: ${row.menuSyncLastError}`)
  if (row.menuSyncNextRetryTime) parts.push(`${t('system.tenant.menuSyncNextRetryTime')}: ${formatUnix(row.menuSyncNextRetryTime)}`)
  return parts.length > 0 ? parts.join('\n') : undefined
}

const retryMenuSync = async (row: any) => {
  retryingTenantId.value = row.id
  try {
    await tenantMenuSyncSyncTenantPost({tenantId: row.id})
    showSuccess(t('system.tenant.menuSyncRetrySubmitted'))
  } finally {
    retryingTenantId.value = null
    onQuery()
  }
}


// 初始化查询参数
queryParams.value = {name: undefined, phone: undefined}

// 重置操作
const resetQuery = () => {
  queryParams.value = {name: undefined, phone: undefined}
  onQuery()
}

// 删除
const delTenant = async (row: any) => {
  const confirmed = await showConfirm(`${t('system.tenant.deleteTenantConfirm')}：【${row.name}】?`, t('common.message.tip'), {type: 'danger'})
  if (!confirmed) return
  await systemTenantDeletePost({id: row.id})
  onQuery()
  showSuccess(t('common.message.deleteSuccess'))
}

// 打开新增页面
const openAddTenant = () => {
  editTitle.value = t('system.tenant.addTenant')
  editTenantRef.value?.openDialog({tenantType: 1, dbType: 4})
}

// 打开编辑页面
const openEditTenant = (row: any) => {
  editTitle.value = t('system.tenant.editTenant')
  editTenantRef.value?.openDialog(row)
}

// 重置密码
const resetTenantPwd = async (row: any) => {
  const confirmed = await showConfirm(`${t('system.tenant.resetPwdConfirm')}：【${row.name}】?`, t('common.message.tip'))
  if (!confirmed) return
  const res = await systemTenantResetPwdPost({userId: row.userId})
  showSuccess(`${t('system.tenant.resetPwdSuccess')}：${res.data.data?.newPassword}`)
}

// 创建租户库
const createTenant = async (row: any) => {
  const confirmed = await showConfirm(`${t('system.tenant.createDbConfirm')}：【${row.name}】?`, t('common.message.tip'))
  if (!confirmed) return
  await systemTenantCreateDbPost({id: row.id})
  showSuccess(t('system.tenant.createDbSuccess'))
  onQuery()
}

// 启用/禁用租户
const toggleTenantStatus = async (row: any) => {
  if (isMainTenant(row)) return
  const newEnabled = !isTenantEnabled(row)
  const confirmKey = newEnabled ? 'system.tenant.enableConfirm' : 'system.tenant.disableConfirm'
  const confirmed = await showConfirm(`${t(confirmKey)}：【${row.name}】?`, t('common.message.tip'))
  if (!confirmed) return
  await systemTenantSetStatusPost({id: row.id, isEnabled: newEnabled} as any)
  showSuccess(t('system.tenant.tenantStatusSuccess'))
  onQuery()
}

const onPage = usePrimeTablePage(tableParams, onQuery)

// 占位 customActions：仅用于触发 CrudDataTable 内置操作列的显隐与权限聚合判断
// （所有按钮均通过 #action-extra 插槽原样渲染，保留原有的 disabled/title/v-auth 逻辑）
const customActions: CrudAction[] = [
  {auth: 'createDb', label: '', onClick: () => {}, visible: () => false},
  {auth: 'update', label: '', onClick: () => {}, visible: () => false},
]

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'name', header: t('system.tenant.tenantName'), width: '160px', align: 'center'},
  {field: 'adminAccount', header: t('system.tenant.adminAccount'), width: '120px', align: 'center'},
  {field: 'phone', header: t('system.tenant.phone'), width: '120px', align: 'center', sensitive: true},
  {field: 'tenantType', header: t('system.tenant.tenantType'), width: '100px', align: 'center', bodyKey: 'tenantType'},
  {field: 'isEnabled', header: t('system.tenant.status'), width: '100px', align: 'center', bodyKey: 'isEnabled'},
  {field: 'dbType', header: t('system.tenant.dbType'), width: '120px', align: 'center', bodyKey: 'dbType', responsivePriority: 2},
  {field: 'createdTime', header: t('common.label.createTime'), width: '160px', align: 'center', bodyKey: 'createdTime'},
  {field: 'expireTime', header: t('system.tenant.expireTime'), width: '170px', align: 'center', bodyKey: 'expireTime', responsivePriority: 2},
  {field: 'menuSyncStatus', header: t('system.tenant.menuSyncStatus'), width: '150px', align: 'center', bodyKey: 'menuSyncStatus'},
  {field: 'remark', header: t('common.label.remark'), responsivePriority: 2},
])
</script>

<style scoped>
.menu-sync-time {
  font-size: 12px;
  color: var(--p-text-muted-color, #6b7280);
  margin-top: 2px;
}
</style>
