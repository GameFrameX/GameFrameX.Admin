<template>
  <div class="tenantAdvertisementCallbackConfigEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :defaultSearchKey="true"
            :showAdd="true"
            authPrefix="tenantAdvertisementCallbackConfigEntity"
            @query="onQuery"
            @add="openAdd"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows rowKey="id"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100]"
                       authPrefix="tenantAdvertisementCallbackConfigEntity"
                       :actions="actions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       showIdColumn
                       idColumnWidth="120px"
                       @page="onPage">
          <template #body-appId="{ data }">
            <Tag :severity="scopeOf(data.appId) === 'tenant' ? 'info' : 'success'" :value="scopeLabel(data.appId)"/>
          </template>
          <template #body-provider="{ data }">
            <Tag :value="formatProvider(data.provider)"/>
          </template>
          <template #body-signingSecret="{ data }">
            <Tag :value="hasSecret(data.signingSecret) ? '已配置' : '未配置'" :severity="hasSecret(data.signingSecret) ? 'info' : 'secondary'"/>
          </template>
          <template #body-verificationKeys="{ data }">
            <Tag :value="hasSecret(data.verificationKeys) ? '已配置' : '未配置'" :severity="hasSecret(data.verificationKeys) ? 'info' : 'secondary'"/>
          </template>
          <template #body-isEnabled="{ data }">
            <Tag v-if="data.isEnabled !== false" value="启用" severity="success"/>
            <Tag v-else value="禁用" severity="danger"/>
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editTitle" :appOptions="appOptions" @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="tenantAdvertisementCallbackConfigEntity">
import {ref, computed, onMounted} from 'vue'
import SearchForm from '/@/components/search-form/index.vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type {CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'
import {formatUnix} from '../../../../utils/formatTime.ts'
import {useConfigAppScope} from '../../../../composables/useConfigAppScope.ts'
import editDialog from './component/EditDialog.vue'
import { tenantAdvertisementCallbackConfigEntityPagePost, tenantAdvertisementCallbackConfigEntityDeletePost, tenantAdvertisementCallbackConfigEntityDetailGet, tenantAdvertisementCallbackConfigEntityProvidersGet } from '../../../../api/tenantAdvertisementCallbackConfigEntity'

const editDialogRef = ref()
const editTitle = ref('')

const {appOptions, loadApps, scopeOf, scopeLabel, scopeSearchOptions} = useConfigAppScope()

const providerOptions = ref<{label: string; value: string}[]>([])

const statusOptions = [
  {label: '启用', value: true},
  {label: '禁用', value: false},
]

const searchFields = computed(() => [
  {key: 'appId', label: t('gameBusiness.configScope.scope'), type: 'select' as const, options: scopeSearchOptions.value, placeholder: t('gameBusiness.configScope.all')},
  {key: 'provider', label: 'Provider', type: 'select' as const, options: providerOptions.value, placeholder: 'Provider'},
  {key: 'isActive', label: '状态', type: 'select' as const, options: statusOptions, placeholder: '状态'},
])

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'appId', header: t('gameBusiness.configScope.scope'), width: '120px', align: 'center', bodyKey: 'appId'},
  {field: 'provider', header: 'Provider', width: '150px', align: 'center', bodyKey: 'provider'},
  {field: 'callbackUrl', header: '回调 URL', width: '260px'},
  {field: 'signingSecret', header: '签名密钥', width: '120px', align: 'center', bodyKey: 'signingSecret', sensitive: true},
  {field: 'verificationKeys', header: '验证材料', width: '120px', align: 'center', bodyKey: 'verificationKeys', sensitive: true},
  {field: 'isEnabled', header: '状态', width: '100px', align: 'center', bodyKey: 'isEnabled'},
  {field: 'remark', header: '备注'},
  {field: 'createdTime', header: '创建时间', width: '180px', align: 'center', bodyKey: 'createdTime'},
])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
  onDelete,
} = useCrudTable({
  api: {
    pageApi: tenantAdvertisementCallbackConfigEntityPagePost,
    deleteApi: tenantAdvertisementCallbackConfigEntityDeletePost,
  },
  immediate: true,
})

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAdd = () => {
  editTitle.value = '新增广告回调配置'
  editDialogRef.value.openDialog({})
}

const openEdit = async (row: any) => {
  editTitle.value = '编辑广告回调配置'
  try {
    const res: any = await tenantAdvertisementCallbackConfigEntityDetailGet({id: row.id})
    const detail = res.data.data
    if (detail) {
      editDialogRef.value.openDialog({...row, ...detail})
      return
    }
  } catch {
    // ignore, fallback to row
  }
  editDialogRef.value.openDialog(row)
}

const actions = {
  edit: openEdit,
  delete: onDelete,
}

const formatProvider = (value: string) => providerOptions.value.find((item) => item.value === value)?.label ?? (value || '-')
const hasSecret = (value: any) => !!String(value ?? '').trim()

onMounted(async () => {
  loadApps()
  try {
    const res: any = await tenantAdvertisementCallbackConfigEntityProvidersGet()
    const list = res.data?.data ?? []
    providerOptions.value = (list || []).map((item: any) => ({label: item.label, value: item.value}))
  } catch {
    providerOptions.value = []
  }
})
</script>
