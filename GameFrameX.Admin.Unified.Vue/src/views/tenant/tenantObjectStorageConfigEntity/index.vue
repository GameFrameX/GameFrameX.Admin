<template>
  <div class="tenant-object-storage-config-container">
    <Card>
      <template #content>
        <SearchForm
          v-model="queryParams"
          :fields="searchFields"
          :defaultSearchKey="true"
          authPrefix="tenantObjectStorageConfigEntity"
          :showAdd="true"
          @query="onQuery"
          @add="openAddStorageConfig"
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
          authPrefix="tenantObjectStorageConfigEntity"
          :actions="actions"
          :customActions="customActions"
          :columns="columns"
          :exportQuery="queryParams"
          actionColumnWidth="280px"
          @page="onPage"
          showIdColumn
          idColumnWidth="120px"
        >
          <template #body-provider="{ data }">
            <Tag :value="formatProvider(data.provider)"/>
          </template>
          <template #body-isEnabled="{ data }">
            <Tag v-if="data.isEnabled !== false" severity="success" :value="t('system.objectStorage.enabled')"/>
            <Tag v-else severity="danger" :value="t('system.objectStorage.disabled')"/>
          </template>
          <template #body-maxFileSizeBytes="{ data }">{{ formatBytes(data.maxFileSizeBytes) }}</template>
          <template #body-allowedContentTypesJson="{ data }">{{ formatContentTypes(data.allowedContentTypesJson) }}</template>
          <template #body-enableMd5="{ data }">
            <Tag v-if="data.enableMd5 !== false" severity="success" :value="t('common.label.yes')"/>
            <Tag v-else severity="secondary" :value="t('common.label.no')"/>
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>

        <editDialog ref="editDialogRef" :title="editTitle" @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="tenantObjectStorageConfigEntity">
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type {CrudAction, CrudColumnConfig} from '../../../components/crud-data-table/types.js'
import SearchForm from '/@/components/search-form/index.vue'
import {useCrudTable} from '../../../composables/useCrudTable.js'
import {usePrimeTablePage} from '../../../composables'
import {showConfirm, showError, showSuccess} from '../../../utils/message.js'
import {formatUnix} from '../../../utils/formatTime.js'
import {tenantObjectStorageConfigEntityPagePost, tenantObjectStorageConfigEntityDeletePost, tenantObjectStorageConfigEntityDetailGet, tenantObjectStorageConfigEntityTestConnectionGet, tenantObjectStorageConfigEntityUpdatePost, tenantObjectStorageConfigEntityProvidersGet} from '../../../api/tenantObjectStorageConfigEntity'
import type {UpdateTenantObjectStorageConfigInput} from '../../../api/_shared/data-contracts'
import {computed, onMounted, ref} from 'vue'
import editDialog from './component/EditDialog.vue'
import {readRowVersion} from '../../../utils/rowVersion.js'

interface StorageProviderOption {
  label: string
  value: string
}

const editDialogRef = ref()
const editTitle = ref('')
const providerOptions = ref<StorageProviderOption[]>([])

const statusOptions = [
  {label: t('system.objectStorage.enabled'), value: true},
  {label: t('system.objectStorage.disabled'), value: false},
]

const searchFields = computed(() => [
  {key: 'provider', label: t('system.objectStorage.provider'), type: 'select' as const, options: providerOptions.value, placeholder: t('system.objectStorage.provider')},
  {key: 'isEnabled', label: t('system.objectStorage.status'), type: 'select' as const, options: statusOptions, placeholder: t('system.objectStorage.status')},
])

const readValue = (row: any, key: string) => row?.[key]
const readResult = (res: any) => res?.data?.data
const toApiId = (id: any) => String(id ?? '').trim() as unknown as number

const normalizeRow = (row: any) => ({
  id: String(readValue(row, 'id') ?? ''),
  rowVersion: readRowVersion(row) ?? 0,
  provider: readValue(row, 'provider') ?? '',
  pathTemplate: readValue(row, 'pathTemplate') ?? '',
  maxFileSizeBytes: readValue(row, 'maxFileSizeBytes') ?? 0,
  allowedContentTypesJson: readValue(row, 'allowedContentTypesJson') ?? '',
  enableMd5: readValue(row, 'enableMd5') !== false,
  configJson: readValue(row, 'configJson') ?? '',
  isEnabled: readValue(row, 'isEnabled') !== false,
  remark: readValue(row, 'remark') ?? '',
  createdTime: readValue(row, 'createdTime') ?? '',
})

const normalizeProviders = (list: any[]): StorageProviderOption[] =>
  (list || []).map((item: any) => ({
    value: readValue(item, 'value') ?? '',
    label: readValue(item, 'label') ?? readValue(item, 'value') ?? '',
  })).filter((item) => item.value)

const pageStorageConfigs = async (params: any) => {
  const res = await tenantObjectStorageConfigEntityPagePost({
    searchKey: params.searchKey,
    page: params.page,
    pageSize: params.pageSize,
    orderBy: params.orderBy,
    isDescending: params.isDescending,
    provider: params.provider,
    isEnabled: params.isEnabled,
  })
  const result = readResult(res) ?? {}
  const items = result.items ?? []
  const total = result.total ?? 0
  return {
    data: {
      result: {
        items: items.map(normalizeRow),
        total,
      },
    },
  }
}

const deleteStorageConfig = (row: any) => tenantObjectStorageConfigEntityDeletePost({id: toApiId(row.id), rowVersion: row.rowVersion})

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
} = useCrudTable({
  api: {
    pageApi: pageStorageConfigs,
    deleteApi: deleteStorageConfig,
  },
  deleteConfirmMessage: t('system.objectStorage.deleteConfirm'),
  immediate: true,
})

queryParams.value = {
  searchKey: undefined,
  provider: undefined,
  isEnabled: undefined,
}

const formatProvider = (provider: string) => providerOptions.value.find((item) => item.value === provider)?.label ?? provider

const formatBytes = (bytes: number) => {
  const value = Number(bytes || 0)
  if (value >= 1024 * 1024) {
    return `${(value / 1024 / 1024).toFixed(1)} MB`
  }
  if (value >= 1024) {
    return `${(value / 1024).toFixed(1)} KB`
  }
  return `${value} B`
}

const formatContentTypes = (json: string) => {
  if (!json) {
    return t('system.objectStorage.noLimit')
  }
  try {
    const parsed = JSON.parse(json)
    return Array.isArray(parsed) && parsed.length > 0 ? parsed.join(', ') : t('system.objectStorage.noLimit')
  } catch {
    return json
  }
}

const openAddStorageConfig = () => {
  editTitle.value = t('system.objectStorage.addConfig')
  editDialogRef.value?.openDialog()
}

const openEditStorageConfig = async (row: any) => {
  editTitle.value = t('system.objectStorage.editConfig')
  const res = await tenantObjectStorageConfigEntityDetailGet(toApiId(row.id))
  editDialogRef.value?.openDialog(normalizeRow(readResult(res)))
}

const onDeleteStorageConfig = async (row: any) => {
  const confirmed = await showConfirm(
    `${t('system.objectStorage.deleteConfirm')}：【${formatProvider(row.provider)}】?`,
    t('common.message.tip'),
    {type: 'danger'},
  )
  if (!confirmed) return
  const res = await tenantObjectStorageConfigEntityDetailGet(toApiId(row.id))
  await deleteStorageConfig(normalizeRow(readResult(res)))
  showSuccess(t('message.common.message.deleteSuccess'))
  onQuery()
}

const testConnection = async (row: any) => {
  const res = await tenantObjectStorageConfigEntityTestConnectionGet(toApiId(row.id))
  if (readResult(res) !== true) {
    showError(t('system.objectStorage.testConnectionFailed'))
    return
  }
  showSuccess(t('system.objectStorage.testConnectionSuccess'))
}

const toggleEnabled = async (row: any) => {
  const nextEnabled = row.isEnabled === false
  const confirmed = await showConfirm(
    nextEnabled ? t('system.objectStorage.enableConfirm') : t('system.objectStorage.disableConfirm'),
    t('common.message.tip'),
  )
  if (!confirmed) return

  const res = await tenantObjectStorageConfigEntityDetailGet(toApiId(row.id))
  const detail = normalizeRow(readResult(res))
  const input: UpdateTenantObjectStorageConfigInput = {
    id: toApiId(detail.id),
    rowVersion: detail.rowVersion,
    provider: detail.provider,
    pathTemplate: detail.pathTemplate,
    maxFileSizeBytes: detail.maxFileSizeBytes,
    allowedContentTypesJson: detail.allowedContentTypesJson,
    enableMd5: detail.enableMd5,
    configJson: detail.configJson,
    isEnabled: nextEnabled,
    remark: detail.remark,
  }
  await tenantObjectStorageConfigEntityUpdatePost(input)
  showSuccess(nextEnabled ? t('system.objectStorage.enableSuccess') : t('system.objectStorage.disableSuccess'))
  onQuery()
}

const actions = {
  edit: openEditStorageConfig,
  delete: onDeleteStorageConfig,
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'provider', header: t('system.objectStorage.provider'), width: '150px', align: 'center', bodyKey: 'provider'},
  {field: 'isEnabled', header: t('system.objectStorage.status'), width: '110px', align: 'center', bodyKey: 'isEnabled'},
  {field: 'pathTemplate', header: t('system.objectStorage.pathTemplate'), width: '180px'},
  {field: 'maxFileSizeBytes', header: t('system.objectStorage.maxFileSize'), width: '130px', align: 'center', bodyKey: 'maxFileSizeBytes'},
  {field: 'allowedContentTypesJson', header: t('system.objectStorage.allowedContentTypes'), width: '180px', bodyKey: 'allowedContentTypesJson'},
  {field: 'enableMd5', header: t('system.objectStorage.enableMd5'), width: '110px', align: 'center', bodyKey: 'enableMd5'},
  {field: 'remark', header: t('system.objectStorage.remark'), width: '160px'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '180px', align: 'center', bodyKey: 'createdTime'},
])

const customActions: CrudAction[] = [
  {
    auth: 'testConnection',
    label: t('system.objectStorage.testConnection'),
    icon: 'fa-solid fa-plug-circle-check',
    onClick: testConnection,
  },
  {
    auth: 'update',
    label: t('system.objectStorage.toggleEnabled'),
    icon: 'fa-solid fa-power-off',
    onClick: toggleEnabled,
  },
]

const onPage = usePrimeTablePage(tableParams, onQuery)

onMounted(async () => {
  const res = await tenantObjectStorageConfigEntityProvidersGet()
  providerOptions.value = normalizeProviders(readResult(res) ?? [])
})
</script>
