<template>
  <div class="tenant-sms-config-container">
    <Card>
      <template #content>
        <SearchForm
          v-model="queryParams"
          :fields="searchFields"
          :defaultSearchKey="true"
          :showAdd="true"
          authPrefix="tenantSmsConfigEntity"
          @query="onQuery"
          @add="openAdd"
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
          rowKey="id"
          :totalRecords="tableParams.total"
          :rows="tableParams.pageSize"
          :rowsPerPageOptions="[10, 20, 50, 100]"
          authPrefix="tenantSmsConfigEntity"
          :actions="actions"
          :customActions="customActions"
          :columns="columns"
          :exportQuery="queryParams"
          actionColumnWidth="220px"
          showIdColumn
          idColumnWidth="120px"
          @page="onPage"
        >
          <template #body-appId="{ data }">
            <Tag :severity="String(data.appId ?? '0') === '0' ? 'info' : 'success'" :value="scopeLabel(data.appId)" />
          </template>
          <template #body-accessKeySecret>
            <Tag value="******" severity="info" />
          </template>
          <template #body-isEnabled="{ data }">
            <Tag v-if="data.isEnabled !== false" severity="success" value="启用" />
            <Tag v-else severity="danger" value="禁用" />
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>

        <editDialog ref="editDialogRef" :title="editTitle" :appOptions="appOptions" @reloadTable="onQuery" />
      </template>
    </Card>

    <Dialog v-model:visible="testDialogVisible" modal header="测试发送" :style="{ width: 'min(560px, calc(100vw - 32px))' }">
      <div class="test-send-grid">
        <label>目标手机号</label>
        <InputText v-model="testForm.phone" class="w-full" placeholder="13800000000" />
        <label>模板 Code</label>
        <InputText v-model="testForm.templateCode" class="w-full" placeholder="留空则使用配置存储值" />
      </div>
      <template #footer>
        <Button severity="secondary" @click="testDialogVisible = false">取消</Button>
        <Button :loading="testSending" @click="submitTestSend">发送</Button>
      </template>
    </Dialog>
  </div>
</template>

<script lang="ts" setup name="tenantSmsConfigEntity">
import { computed, onMounted, ref } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import SearchForm from '/@/components/search-form/index.vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type { CrudAction, CrudColumnConfig } from '../../../components/crud-data-table/types.js'
import { useCrudTable } from '../../../composables/useCrudTable.js'
import {usePrimeTablePage } from '../../../composables'
import { useConfigAppScope } from '../../../composables/useConfigAppScope.js'
import { showError, showSuccess } from '../../../utils/message.js'
import { formatUnix } from '../../../utils/formatTime.js'
import editDialog from './component/EditDialog.vue'
import { tenantSmsConfigEntityDeletePost, tenantSmsConfigEntityDetailGet, tenantSmsConfigEntityPagePost, tenantSmsConfigEntitySendTestPost } from '../../../api/tenantSmsConfigEntity'

const statusOptions = [
  { label: '启用', value: true },
  { label: '禁用', value: false },
]

const editDialogRef = ref()
const editTitle = ref('')
const testDialogVisible = ref(false)
const testSending = ref(false)
const currentTestRow = ref<any>(null)
const testForm = ref({
  phone: '',
  templateCode: '',
})

const { appOptions, loadApps, scopeLabel, scopeSearchOptions } = useConfigAppScope()
onMounted(() => {
  loadApps()
})

const searchFields = computed(() => [
  { key: 'appId', label: t('gameBusiness.configScope.scope'), type: 'select' as const, options: scopeSearchOptions.value, placeholder: t('gameBusiness.configScope.all') },
  { key: 'isActive', label: '状态', type: 'select' as const, options: statusOptions, placeholder: '状态' },
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
    pageApi: tenantSmsConfigEntityPagePost,
    deleteApi: tenantSmsConfigEntityDeletePost,
  },
  immediate: true,
})

const unwrap = (res: any) => res?.data?.data

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAdd = () => {
  editTitle.value = '新增 SMS 配置'
  editDialogRef.value?.openDialog({})
}

const openEdit = async (row: any) => {
  editTitle.value = '编辑 SMS 配置'
  try {
    const detail = unwrap(await tenantSmsConfigEntityDetailGet({ id: row.id }))
    editDialogRef.value?.openDialog({ ...row, ...detail })
  } catch {
    editDialogRef.value?.openDialog(row)
  }
}

const openTestSend = (row: any) => {
  currentTestRow.value = row
  testForm.value = {
    phone: '',
    templateCode: row.templateCode || '',
  }
  testDialogVisible.value = true
}

const submitTestSend = async () => {
  if (!currentTestRow.value?.id) {
    showError('请选择要测试的 SMS 配置')
    return
  }
  if (!testForm.value.phone) {
    showError('请填写目标手机号')
    return
  }

  testSending.value = true
  try {
    await tenantSmsConfigEntitySendTestPost({
      id: currentTestRow.value.id,
      phone: testForm.value.phone,
      templateCode: testForm.value.templateCode || undefined,
    })
    showSuccess('测试短信已提交发送')
    testDialogVisible.value = false
  } finally {
    testSending.value = false
  }
}

const actions = {
  edit: openEdit,
  delete: onDelete,
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'appId', header: t('gameBusiness.configScope.scope'), width: '160px', align: 'center', bodyKey: 'appId'},
  {field: 'provider', header: 'Provider', width: '120px', align: 'center'},
  {field: 'accessKeyId', header: 'AccessKeyId', width: '200px'},
  {field: 'accessKeySecret', header: 'AccessKeySecret', width: '140px', align: 'center', sensitive: true, bodyKey: 'accessKeySecret'},
  {field: 'signName', header: '短信签名', width: '160px'},
  {field: 'templateCode', header: '模板 Code', width: '180px'},
  {field: 'priority', header: '优先级', width: '100px', align: 'center'},
  {field: 'isEnabled', header: '状态', width: '100px', align: 'center', bodyKey: 'isEnabled'},
  {field: 'remark', header: '备注'},
  {field: 'createdTime', header: '创建时间', width: '180px', align: 'center', bodyKey: 'createdTime'},
])

const customActions: CrudAction[] = [
  {
    auth: 'sendTest',
    label: '测试',
    icon: 'fa-solid fa-paper-plane',
    severity: 'info',
    visible: (row) => row.isEnabled !== false,
    onClick: openTestSend,
  },
]
</script>

<style scoped>
.test-send-grid {
  display: grid;
  grid-template-columns: 96px minmax(0, 1fr);
  gap: 12px 16px;
  align-items: start;
}

.test-send-grid > label {
  color: var(--p-text-muted-color);
  line-height: 2.35;
  text-align: right;
}

@media (max-width: 640px) {
  .test-send-grid {
    grid-template-columns: minmax(0, 1fr);
  }

  .test-send-grid > label {
    line-height: 1.4;
    text-align: left;
  }
}
</style>
