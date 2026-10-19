<template>
  <div class="system-email-provider-container">
    <Card>
      <template #content>
        <SearchForm
          v-model="queryParams"
          :fields="searchFields"
          :defaultSearchKey="true"
          :showAdd="true"
          authPrefix="systemEmailProviderEntity"
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
          authPrefix="systemEmailProviderEntity"
          :actions="actions"
          :customActions="customActions"
          :columns="columns"
          :exportQuery="queryParams"
          actionColumnWidth="220px"
          showIdColumn
          idColumnWidth="120px"
          @page="onPage"
        >
          <template #body-purpose="{ data }">
            <Tag :value="formatSystemPurpose(data.purpose)"/>
          </template>
          <template #body-secretId>
            <Tag value="******" severity="info"/>
          </template>
          <template #body-isEnabled="{ data }">
            <Tag v-if="data.isEnabled !== false" severity="success" value="启用"/>
            <Tag v-else severity="danger" value="禁用"/>
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>

        <editDialog ref="editDialogRef" :title="editTitle" @reloadTable="onQuery"/>
      </template>
    </Card>

    <Dialog v-model:visible="testDialogVisible" modal header="测试发送" :style="{ width: 'min(560px, calc(100vw - 32px))' }">
      <div class="test-send-grid">
        <label>收件邮箱</label>
        <InputText v-model="testForm.receiverEmail" class="w-full" placeholder="ops@example.com"/>
        <label>主题</label>
        <InputText v-model="testForm.subject" class="w-full"/>
        <label>正文</label>
        <Textarea v-model="testForm.body" class="w-full" rows="5" :autoResize="true"/>
      </div>
      <template #footer>
        <Button severity="secondary" @click="testDialogVisible = false">取消</Button>
        <Button :loading="testSending" @click="submitTestSend">发送</Button>
      </template>
    </Dialog>
  </div>
</template>

<script lang="ts" setup name="systemEmailProviderEntity">
import {computed, ref} from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import SearchForm from '/@/components/search-form/index.vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type {CrudAction, CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'
import {showError, showSuccess} from '../../../../utils/message.ts'
import {formatUnix} from '../../../../utils/formatTime.ts'
import editDialog from './component/EditDialog.vue'
import { systemEmailProviderDeletePost, systemEmailProviderDetailGet, systemEmailProviderPagePost, systemEmailProviderSendTestPost } from '../../../../api/systemEmailProvider'

const systemPurposeOptions = [
  {label: '默认', value: 0},
  {label: '租户生命周期', value: 1},
  {label: '平台安全', value: 2},
  {label: '平台运维', value: 3},
]

const providerOptions = [
  {label: 'TencentCloud SES', value: 'TencentCloudSes'},
  {label: 'SMTP', value: 'Smtp'},
]

const statusOptions = [
  {label: '启用', value: true},
  {label: '禁用', value: false},
]

const editDialogRef = ref()
const editTitle = ref('')
const testDialogVisible = ref(false)
const testSending = ref(false)
const currentTestRow = ref<any>(null)
const testForm = ref({
  receiverEmail: '',
  subject: '平台系统邮件测试',
  body: '这是一封来自平台系统邮件 Provider 的测试邮件。',
})

const searchFields = computed(() => [
  {key: 'purpose', label: '用途', type: 'select' as const, options: systemPurposeOptions, placeholder: '用途'},
  {key: 'provider', label: 'Provider', type: 'select' as const, options: providerOptions, placeholder: 'Provider'},
  {key: 'isActive', label: '状态', type: 'select' as const, options: statusOptions, placeholder: '状态'},
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
    pageApi: systemEmailProviderPagePost,
    deleteApi: systemEmailProviderDeletePost,
  },
  immediate: true,
})

const formatSystemPurpose = (value: any) => systemPurposeOptions.find((item) => item.value === Number(value || 0))?.label ?? String(value)
const unwrap = (res: any) => res?.data?.data

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAdd = () => {
  editTitle.value = '新增平台系统邮件配置'
  editDialogRef.value?.openDialog({})
}

const openEdit = async (row: any) => {
  editTitle.value = '编辑平台系统邮件配置'
  try {
    const detail = unwrap(await systemEmailProviderDetailGet({id: row.id}))
    editDialogRef.value?.openDialog({...row, ...detail})
  } catch {
    editDialogRef.value?.openDialog(row)
  }
}

const openTestSend = (row: any) => {
  currentTestRow.value = row
  testForm.value = {
    receiverEmail: row.defaultToEmail || '',
    subject: '平台系统邮件测试',
    body: '这是一封来自平台系统邮件 Provider 的测试邮件。',
  }
  testDialogVisible.value = true
}

const submitTestSend = async () => {
  if (!currentTestRow.value?.id) {
    showError('请选择要测试的邮件配置')
    return
  }
  if (!testForm.value.receiverEmail || !testForm.value.subject || !testForm.value.body) {
    showError('请填写收件邮箱、主题和正文')
    return
  }

  testSending.value = true
  try {
    await systemEmailProviderSendTestPost({
      id: currentTestRow.value.id,
      receiverEmail: testForm.value.receiverEmail,
      subject: testForm.value.subject,
      body: testForm.value.body,
    })
    showSuccess('测试邮件已发送')
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
  {field: 'purpose', header: '用途', width: '150px', align: 'center', bodyKey: 'purpose'},
  {field: 'provider', header: 'Provider', width: '150px'},
  {field: 'host', header: 'SMTP Host', width: '180px'},
  {field: 'port', header: 'Port', width: '90px', align: 'center'},
  {field: 'region', header: '区域', width: '130px'},
  {field: 'defaultFromEmail', header: '默认发件人', width: '220px'},
  {field: 'defaultFromName', header: '发件名', width: '150px'},
  {field: 'defaultToEmail', header: '默认收件人', width: '220px'},
  {field: 'templateId', header: '模板 ID', width: '120px', align: 'center'},
  {field: 'secretId', header: 'SecretId', width: '120px', align: 'center', sensitive: true, bodyKey: 'secretId'},
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
