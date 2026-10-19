<template>
  <div class="tenantSessionKeyEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :defaultSearchKey="false"
            :showAdd="false"
            authPrefix="tenantSessionKeyEntity"
            @query="onQuery"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows rowKey="id"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100]"
                       authPrefix="tenantSessionKeyEntity"
                       :customActions="customActions"
                       actionColumnWidth="280px"
                       showIdColumn
                       idColumnWidth="120px"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage">
          <template #body-kid="{ data }">
            <span class="key-kid font-mono" :title="data.kid">{{ data.kid }}</span>
          </template>
          <template #body-secret="{ data }">
            <span class="key-kid font-mono">{{ data.secret }}</span>
          </template>
          <template #body-notBeforeTime="{ data }">
            <span v-if="data.notBeforeTime">{{ formatUnix(data.notBeforeTime) }}</span>
            <Tag v-else severity="info" value="立即生效"/>
          </template>
          <template #body-expireTime="{ data }">
            <span v-if="data.expireTime">{{ formatUnix(data.expireTime) }}</span>
            <Tag v-else severity="info" value="永不过期"/>
          </template>
          <template #body-rotatedTime="{ data }">
            <span v-if="data.rotatedTime">{{ formatUnix(data.rotatedTime) }}</span>
            <span v-else>-</span>
          </template>
          <template #body-isEnabled="{ data }">
            <Tag v-if="data.isEnabled" severity="success" value="启用"/>
            <Tag v-else severity="danger" value="禁用"/>
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>
        <div class="key-tip">
          <i class="fa-solid fa-shield-halved"></i>
          会话密钥（AES-256-GCM 加密信封）通常由登录自动签发，本页手动新增用于联调或多版本客户端并行持钥；
          对称解密必需明文存储，但仅在新增/轮换时一次性回显，列表恒为脱敏占位；密钥禁删，退场请用禁用（立即拒解 60105）或到期（自然退场）；轮换后新旧密钥双活，客户端可在旧密钥到期前任意切换。
        </div>
      </template>
    </Card>
    <addDialog ref="addDialogRef" @added="onReveal"/>
    <rotateDialog ref="rotateDialogRef" @rotated="onReveal"/>
    <renewDialog ref="renewDialogRef" @renewed="onQuery"/>
    <revealDialog ref="revealDialogRef"/>
  </div>
</template>

<script lang="ts" setup name="tenantSessionKeyEntity">
import { computed, ref } from 'vue'
import SearchForm from '/@/components/search-form/index.vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type { CrudAction } from '../../../../components/crud-data-table/types.ts'
import type { CrudColumnConfig } from '../../../../components/crud-data-table/types.ts'
import { useCrudTable } from '../../../../composables/useCrudTable.ts'
import { usePrimeTablePage } from '../../../../composables'
import { formatUnix } from '../../../../utils/formatTime.ts'
import { tenantSessionKeyEntityPagePost, tenantSessionKeyEntityEnablePost, tenantSessionKeyEntityDisablePost } from '../../../../api/tenantSessionKeyEntity'
import addDialog from './component/AddDialog.vue'
import rotateDialog from './component/RotateDialog.vue'
import renewDialog from './component/RenewDialog.vue'
import revealDialog from './component/RevealDialog.vue'

const searchFields = computed(() => [
  { key: 'appId', label: '应用ID', type: 'number' as const, placeholder: '按应用 ID 筛选' },
  {
    key: 'isActive', label: '状态', type: 'select' as const, placeholder: '状态',
    options: [
      { label: '启用', value: true },
      { label: '禁用', value: false },
    ],
  },
])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
} = useCrudTable({
  api: {
    pageApi: tenantSessionKeyEntityPagePost,
  },
  immediate: true,
})

const addDialogRef = ref()
const rotateDialogRef = ref()
const renewDialogRef = ref()
const revealDialogRef = ref()

const onPage = usePrimeTablePage(tableParams, onQuery)

// Add/Rotate 响应一次性明文回显，随后刷新列表（此时密钥列已是 ******）
const onReveal = async (data: any) => {
  revealDialogRef.value?.open(data)
  onQuery()
}

const onEnable = async (row: any) => {
  await tenantSessionKeyEntityEnablePost({ id: row.id })
  onQuery()
  showSuccess('已启用')
}

const onDisable = async (row: any) => {
  const confirmed = await showConfirm('禁用后使用该 Key 的加密信封将立即拒解（60105），同应用其他 Key 不受影响。确认禁用？', '提示')
  if (!confirmed) {
    return
  }
  await tenantSessionKeyEntityDisablePost({ id: row.id })
  onQuery()
  showSuccess('已禁用')
}

const columns = computed<CrudColumnConfig[]>(() => [
  { field: 'appId', header: '应用ID', width: '110px', align: 'center' },
  { field: 'kid', header: 'Key ID', width: '260px', bodyKey: 'kid' },
  { field: 'secret', header: '密钥', width: '110px', align: 'center', bodyKey: 'secret' },
  { field: 'notBeforeTime', header: '生效时间', width: '170px', align: 'center', bodyKey: 'notBeforeTime' },
  { field: 'expireTime', header: '过期时间', width: '170px', align: 'center', bodyKey: 'expireTime' },
  { field: 'rotatedTime', header: '被轮换时间', width: '170px', align: 'center', bodyKey: 'rotatedTime' },
  { field: 'isEnabled', header: '状态', width: '90px', align: 'center', bodyKey: 'isEnabled' },
  { field: 'createdTime', header: '创建时间', width: '170px', align: 'center', bodyKey: 'createdTime' },
])

const customActions: CrudAction[] = [
  { auth: 'add', label: '新增', icon: 'fa-solid fa-plus', onClick: () => addDialogRef.value?.open() },
  { auth: 'update', label: '轮换', icon: 'fa-solid fa-rotate', onClick: (row: any) => rotateDialogRef.value?.open(row) },
  { auth: 'update', label: '续租', icon: 'fa-solid fa-calendar-plus', onClick: (row: any) => renewDialogRef.value?.open(row) },
  { auth: 'update', label: '启用', icon: 'fa-solid fa-toggle-on', severity: 'success', onClick: onEnable, visible: (row: any) => !row.isEnabled },
  { auth: 'update', label: '禁用', icon: 'fa-solid fa-toggle-off', severity: 'warn', onClick: onDisable, visible: (row: any) => !!row.isEnabled },
]
</script>

<style scoped>
.key-kid {
  max-width: 260px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-size: 12px;
  color: var(--p-text-muted-color);
}

.key-tip {
  margin-top: 8px;
  padding: 8px 12px;
  background: var(--p-surface-100);
  border-radius: 6px;
  font-size: 12px;
  color: var(--p-text-muted-color);
}

.key-tip i {
  margin-right: 6px;
  color: var(--p-primary-color);
}
</style>
