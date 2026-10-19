<template>
  <div class="tenant-wechat-config-container">
    <Card>
      <template #content>
        <SearchForm
          v-model="queryParams"
          :fields="searchFields"
          :defaultSearchKey="true"
          :showAdd="true"
          authPrefix="tenantWechatConfigEntity"
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
          authPrefix="tenantWechatConfigEntity"
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
          <template #body-isEnabled="{ data }">
            <Tag v-if="data.isEnabled !== false" severity="success" value="启用" />
            <Tag v-else severity="danger" value="禁用" />
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>

        <editDialog ref="editDialogRef" :title="editTitle" :appOptions="appOptions" @reloadTable="onQuery" />
      </template>
    </Card>

    <Dialog v-model:visible="testDialogVisible" modal header="测试发送" :style="{ width: 'min(620px, calc(100vw - 32px))' }">
      <div class="test-send-grid">
        <label>接收者 OpenId</label>
        <InputText v-model="testForm.receiverOpenId" class="w-full" placeholder="oUpF8uMuAJ..." />
        <label>模板 ID</label>
        <InputText v-model="testForm.templateId" class="w-full" />
        <label>跳转链接</label>
        <InputText v-model="testForm.url" class="w-full" placeholder="https://example.com" />
        <label>小程序页面路径</label>
        <InputText v-model="testForm.miniProgramPagePath" class="w-full" placeholder="pages/index/index" />
        <label>模板变量 (JSON)</label>
        <Textarea v-model="testForm.testDataJson" class="w-full font-mono" rows="5" :autoResize="true" />
      </div>
      <template #footer>
        <Button severity="secondary" @click="testDialogVisible = false">取消</Button>
        <Button :loading="testSending" @click="submitTestSend">发送</Button>
      </template>
    </Dialog>
  </div>
</template>

<script lang="ts" setup name="tenantWechatConfigEntity">
import { computed, onMounted, ref } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import SearchForm from '/@/components/search-form/index.vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type { CrudAction, CrudColumnConfig } from '../../../components/crud-data-table/types.js'
import { useCrudTable } from '../../../composables/useCrudTable.js'
import {usePrimeTablePage } from '../../../composables'
import { useConfigAppScope } from '../../../composables/useConfigAppScope.js'
import { showError, showSuccess } from '../../../utils/message.js'
import { formatUnix } from '../../../utils/formatTime.js'
import editDialog from './component/EditDialog.vue'
import { tenantWechatConfigEntityDeletePost, tenantWechatConfigEntityDetailGet, tenantWechatConfigEntityPagePost, tenantWechatConfigEntitySendTestPost } from '../../../api/tenantWechatConfigEntity'

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
  receiverOpenId: '',
  templateId: '',
  url: '',
  miniProgramPagePath: '',
  testDataJson: '{}',
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
    pageApi: tenantWechatConfigEntityPagePost,
    deleteApi: tenantWechatConfigEntityDeletePost,
  },
  immediate: true,
})

const unwrap = (res: any) => res?.data?.data

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAdd = () => {
  editTitle.value = '新增 WeChat 配置'
  editDialogRef.value?.openDialog({})
}

const openEdit = async (row: any) => {
  editTitle.value = '编辑 WeChat 配置'
  try {
    const detail = unwrap(await tenantWechatConfigEntityDetailGet({ id: row.id }))
    editDialogRef.value?.openDialog({ ...row, ...detail })
  } catch {
    editDialogRef.value?.openDialog(row)
  }
}

const openTestSend = (row: any) => {
  currentTestRow.value = row
  testForm.value = {
    receiverOpenId: '',
    templateId: '',
    url: '',
    miniProgramPagePath: '',
    testDataJson: '{}',
  }
  testDialogVisible.value = true
}

const submitTestSend = async () => {
  if (!currentTestRow.value?.id) {
    showError('请选择要测试的 WeChat 配置')
    return
  }
  if (!testForm.value.receiverOpenId || !testForm.value.templateId) {
    showError('请填写接收者 OpenId 和模板 ID')
    return
  }

  let testData: Record<string, string> = {}
  const raw = (testForm.value.testDataJson || '').trim()
  if (raw && raw !== '{}') {
    try {
      const parsed = JSON.parse(raw)
      if (parsed && typeof parsed === 'object' && !Array.isArray(parsed)) {
        const flat: Record<string, string> = {}
        for (const key of Object.keys(parsed)) {
          flat[key] = parsed[key] == null ? '' : String(parsed[key])
        }
        testData = flat
      }
    } catch (error: any) {
      showError(`模板变量 JSON 解析失败：${error.message}`)
      return
    }
  }

  testSending.value = true
  try {
    await tenantWechatConfigEntitySendTestPost({
      id: currentTestRow.value.id,
      receiverOpenId: testForm.value.receiverOpenId,
      templateId: testForm.value.templateId,
      url: testForm.value.url || undefined,
      miniProgramPagePath: testForm.value.miniProgramPagePath || undefined,
      testData,
    })
    showSuccess('测试模板消息已提交发送')
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
  {field: 'wechatAppId', header: '公众号 AppId', width: '200px'},
  {field: 'wxOpenAppId', header: '小程序 AppId', width: '200px'},
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
  grid-template-columns: 110px minmax(0, 1fr);
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
