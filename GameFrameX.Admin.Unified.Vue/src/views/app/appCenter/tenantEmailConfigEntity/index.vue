<template>
  <div class="tenant-email-config-container">
    <Card>
      <template #content>
        <SearchForm
          v-model="queryParams"
          :fields="searchFields"
          :defaultSearchKey="true"
          :showAdd="true"
          authPrefix="tenantEmailConfigEntity"
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
          authPrefix="tenantEmailConfigEntity"
          :actions="actions"
          :customActions="customActions"
          actionColumnWidth="220px"
          showIdColumn
          idColumnWidth="120px"
          :columns="columns"
          :exportQuery="queryParams"
          @page="onPage"
        >
          <template #body-appId="{ data }">
            <Tag :severity="String(data.appId ?? '0') === '0' ? 'info' : 'success'" :value="scopeLabel(data.appId)"/>
          </template>
          <template #body-purpose="{ data }">
            <Tag :value="formatTenantPurpose(data.purpose)"/>
          </template>
          <template #body-fromEmailSecretId="{ data }">
            <Tag value="******" severity="info"/>
          </template>
          <template #body-isEnabled="{ data }">
            <Tag v-if="data.isEnabled !== false" severity="success" value="启用"/>
            <Tag v-else severity="danger" value="禁用"/>
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>

        <editDialog ref="editDialogRef" :title="editTitle" :appOptions="appOptions" @reloadTable="onQuery"/>
      </template>
    </Card>

    <Dialog v-model:visible="testDialogVisible" modal header="测试发送" :style="{ width: 'min(560px, calc(100vw - 32px))' }">
      <div class="test-send-grid">
        <label>收件邮箱</label>
        <InputText v-model="testForm.receiverEmail" class="w-full" placeholder="player@example.com"/>
        <label>模板 ID</label>
        <InputNumber v-model="testForm.templateId" class="w-full" :min="1" :useGrouping="false"/>
        <label>主题</label>
        <InputText v-model="testForm.subject" class="w-full"/>
        <label>模板变量 JSON</label>
        <Textarea v-model="testForm.templateDataJson" class="w-full font-mono" rows="5" :autoResize="true"/>
      </div>
      <template #footer>
        <Button severity="secondary" @click="testDialogVisible = false">取消</Button>
        <Button :loading="testSending" @click="submitTestSend">发送</Button>
      </template>
    </Dialog>
  </div>
</template>

<script lang="ts" setup name="tenantEmailConfigEntity">
import {computed, onMounted, ref} from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Textarea from 'primevue/textarea'
import SearchForm from '/@/components/search-form/index.vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type {CrudAction} from '../../../../components/crud-data-table/types.ts'
import type {CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'
import {formatUnix} from '../../../../utils/formatTime.ts'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'
import {useConfigAppScope} from '../../../../composables/useConfigAppScope.ts'
import {showError, showSuccess} from '../../../../utils/message.ts'
import editDialog from './component/EditDialog.vue'
import { tenantEmailConfigEntityDeletePost, tenantEmailConfigEntityDetailGet, tenantEmailConfigEntityPagePost, tenantEmailConfigEntitySendTestPost } from '../../../../api/tenantEmailConfigEntity'

const tenantPurposeOptions = [
  {label: '默认', value: 0},
  {label: '注册验证码', value: 1},
  {label: '找回密码', value: 2},
  {label: '运营触达', value: 3},
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
  templateId: undefined as number | undefined,
  subject: '租户业务邮件测试',
  templateDataJson: '{\n  "username": "test",\n  "verification_code": "123456",\n  "expiry_minutes": "5"\n}',
})

const {appOptions, loadApps, scopeLabel, scopeSearchOptions} = useConfigAppScope()
onMounted(() => {
  loadApps()
})

const searchFields = computed(() => [
  {key: 'appId', label: t('gameBusiness.configScope.scope'), type: 'select' as const, options: scopeSearchOptions.value, placeholder: t('gameBusiness.configScope.all')},
  {key: 'purpose', label: '用途', type: 'select' as const, options: tenantPurposeOptions, placeholder: '用途'},
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
    pageApi: tenantEmailConfigEntityPagePost,
    deleteApi: tenantEmailConfigEntityDeletePost,
  },
  immediate: true,
})

const formatTenantPurpose = (value: any) => tenantPurposeOptions.find((item) => item.value === Number(value || 0))?.label ?? String(value)

const unwrap = (res: any) => res?.data?.data

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAdd = () => {
  editTitle.value = '新增租户邮件配置'
  editDialogRef.value?.openDialog({})
}

const openEdit = async (row: any) => {
  editTitle.value = '编辑租户邮件配置'
  try {
    const detail = unwrap(await tenantEmailConfigEntityDetailGet({id: row.id}))
    editDialogRef.value?.openDialog({...row, ...detail})
  } catch {
    editDialogRef.value?.openDialog(row)
  }
}

const openTestSend = (row: any) => {
  currentTestRow.value = row
  testForm.value = {
    receiverEmail: '',
    templateId: undefined,
    subject: '租户业务邮件测试',
    templateDataJson: '{\n  "username": "test",\n  "verification_code": "123456",\n  "expiry_minutes": "5"\n}',
  }
  testDialogVisible.value = true
}

const submitTestSend = async () => {
  if (!currentTestRow.value?.id) {
    showError('请选择要测试的邮件配置')
    return
  }
  if (!testForm.value.receiverEmail || !testForm.value.templateId) {
    showError('请填写收件邮箱和模板 ID')
    return
  }

  let templateData: Record<string, string> = {}
  if (testForm.value.templateDataJson.trim()) {
    try {
      templateData = JSON.parse(testForm.value.templateDataJson)
    } catch (error: any) {
      showError(`模板变量 JSON 解析失败：${error.message}`)
      return
    }
  }

  testSending.value = true
  try {
    await tenantEmailConfigEntitySendTestPost({
      id: currentTestRow.value.id,
      receiverEmail: testForm.value.receiverEmail,
      templateId: testForm.value.templateId,
      subject: testForm.value.subject,
      templateData,
    })
    showSuccess('测试邮件已发送')
    testDialogVisible.value = false
  } finally {
    testSending.value = false
  }
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'appId', header: t('gameBusiness.configScope.scope'), width: '160px', align: 'center', bodyKey: 'appId'},
  {field: 'purpose', header: '用途', width: '150px', align: 'center', bodyKey: 'purpose'},
  {field: 'provider', header: 'Provider', width: '150px'},
  {field: 'region', header: '区域', width: '130px'},
  {field: 'templateSet', header: '模板组', width: '150px'},
  {field: 'fromEmailAddress', header: '发信邮箱', width: '220px'},
  {field: 'defaultFromName', header: '发件名', width: '160px'},
  {field: 'defaultSubjectPrefix', header: '主题前缀', width: '160px'},
  {field: 'fromEmailSecretId', header: 'SecretId', width: '120px', align: 'center', bodyKey: 'fromEmailSecretId', sensitive: true},
  {field: 'priority', header: '优先级', width: '100px', align: 'center'},
  {field: 'isEnabled', header: '状态', width: '100px', align: 'center', bodyKey: 'isEnabled'},
  {field: 'remark', header: '备注'},
  {field: 'createdTime', header: '创建时间', width: '180px', align: 'center', bodyKey: 'createdTime'},
])

const actions = {
  edit: openEdit,
  delete: onDelete,
}

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
