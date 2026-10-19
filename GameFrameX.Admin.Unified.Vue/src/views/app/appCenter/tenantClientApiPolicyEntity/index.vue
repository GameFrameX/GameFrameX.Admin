<template>
  <div class="tenantClientApiPolicyEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :defaultSearchKey="true"
            :showAdd="true"
            authPrefix="tenantClientApiPolicyEntity"
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
                       authPrefix="tenantClientApiPolicyEntity"
                       :actions="actions"
                       :customActions="customActions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       showIdColumn
                       idColumnWidth="120px"
                       @page="onPage">
          <template #body-appId="{ data }">
            <Tag v-if="String(data.appId) === '0'" severity="info" :value="t('message.client.clientApiPolicyTenantDefault')"/>
            <span v-else>{{ data.appId }}</span>
          </template>
          <template #body-ticketExpireTime="{ data }">
            <span v-if="data.ticketExpireTime">{{ formatUnix(data.ticketExpireTime) }}</span>
            <span v-else class="text-color-secondary">{{ t('message.client.clientApiPolicyTicketNeverExpires') }}</span>
          </template>
          <template #body-enableKeyAutoCreate="{ data }">
            <Tag :severity="data.enableKeyAutoCreate ? 'success' : 'info'" :value="data.enableKeyAutoCreate ? t('message.common.label.enable') : t('message.common.label.disable')"/>
          </template>
          <template #body-enableReturnHttpManager="{ data }">
            <Tag :severity="data.enableReturnHttpManager ? 'success' : 'info'" :value="data.enableReturnHttpManager ? t('message.common.label.enable') : t('message.common.label.disable')"/>
          </template>
          <template #body-enableSandBoxRecharge="{ data }">
            <Tag :severity="data.enableSandBoxRecharge ? 'warn' : 'success'" :value="data.enableSandBoxRecharge ? t('message.client.clientApiPolicySandbox') : t('message.client.clientApiPolicyProduction')"/>
          </template>
          <template #body-isEnabled="{ data }">
            <Tag :severity="data.isEnabled !== false ? 'success' : 'warn'" :value="data.isEnabled !== false ? t('message.common.label.enable') : t('message.common.label.disable')"/>
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editTitle" @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="tenantClientApiPolicyEntity">
import {computed, onMounted, ref} from 'vue'
import {useRoute} from 'vue-router'
import Tag from 'primevue/tag'
import SearchForm from '/@/components/search-form/index.vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type {CrudAction, CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'
import {useCurrentApp} from '../../../../composables/useCurrentApp.ts'
import {visibleApps} from '../../../../utils/appWorkspace.ts'
import {formatUnix} from '../../../../utils/formatTime.ts'
import {showConfirm, showSuccess} from '../../../../utils/message.ts'
import editDialog from './component/EditDialog.vue'
import { tenantClientApiPolicyEntityPagePost, tenantClientApiPolicyEntityDeletePost, tenantClientApiPolicyEntityDetailGet, tenantClientApiPolicyEntityRotatePost } from '../../../../api/tenantClientApiPolicyEntity'

const route = useRoute()
const editDialogRef = ref()
const editTitle = ref('')

const {appOptions, loadApps} = useCurrentApp()

// 从工作台跳转携带 ?appId=X 时作为初始过滤 + 新增预填；0 表示租户默认行
const initialAppId = (() => {
  const raw = Array.isArray(route.query.appId) ? route.query.appId[0] : route.query.appId
  const n = Number(raw)
  return raw !== undefined && raw !== '' && Number.isInteger(n) && n >= 0 ? String(n) : undefined
})()

// 应用过滤选项：租户默认行（AppId=0）+ 可见应用列表
const appFilterOptions = computed(() => [
  {label: t('message.client.clientApiPolicyTenantDefault'), value: '0'},
  ...visibleApps(appOptions.value).map((app: any) => ({label: app.name, value: String(app.id)})),
])

const searchFields = computed(() => [
  {key: 'appId', label: t('message.client.clientApiPolicyAppId'), type: 'select' as const, options: appFilterOptions.value, optionLabel: 'label', optionValue: 'value'},
])

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'appId', header: t('message.client.clientApiPolicyAppId'), width: '130px', align: 'center', bodyKey: 'appId'},
  {field: 'lifetimeMinutes', header: t('message.client.clientApiPolicyLifetimeMinutes'), width: '120px', align: 'center'},
  {field: 'ticketExpireTime', header: t('message.client.clientApiPolicyTicketExpireTime'), width: '170px', align: 'center', bodyKey: 'ticketExpireTime'},
  {field: 'enableKeyAutoCreate', header: t('message.client.clientApiPolicyEnableKeyAutoCreate'), width: '110px', align: 'center', bodyKey: 'enableKeyAutoCreate'},
  {field: 'enableReturnHttpManager', header: t('message.client.clientApiPolicyReturnHttpManager'), width: '140px', align: 'center', bodyKey: 'enableReturnHttpManager'},
  {field: 'enableSandBoxRecharge', header: t('message.client.clientApiPolicyEnableSandBoxRecharge'), width: '130px', align: 'center', bodyKey: 'enableSandBoxRecharge'},
  {field: 'isEnabled', header: t('message.gameBusiness.isEnabled'), width: '100px', align: 'center', bodyKey: 'isEnabled'},
  {field: 'remark', header: t('message.common.label.remark')},
  {field: 'createdTime', header: t('message.common.label.createTime'), width: '170px', align: 'center', bodyKey: 'createdTime'},
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
    pageApi: tenantClientApiPolicyEntityPagePost,
    deleteApi: tenantClientApiPolicyEntityDeletePost,
  },
  immediate: false,
})

queryParams.value = {appId: initialAppId}

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAdd = () => {
  editTitle.value = t('message.client.clientApiPolicyAddTitle')
  // 从工作台按应用跳入时预填该应用级行；否则默认租户默认行（AppId=0）
  editDialogRef.value.openDialog(initialAppId && initialAppId !== '0' ? {appId: Number(initialAppId)} : {})
}

const openEdit = async (row: any) => {
  editTitle.value = t('message.client.clientApiPolicyEditTitle')
  try {
    const res: any = await tenantClientApiPolicyEntityDetailGet({id: row.id})
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

// 轮换签名密钥（C373）：仅租户默认行（AppId=0）的密钥被运行时消费，故只在该行露出
const onRotate = async (row: any) => {
  const confirmed = await showConfirm(
      t('message.client.clientApiPolicyRotateConfirm'),
      t('message.common.message.tip'),
  )
  if (!confirmed) {
    return
  }
  await tenantClientApiPolicyEntityRotatePost({id: row.id})
  showSuccess(t('message.client.clientApiPolicyRotateSuccess'))
  onQuery()
}

const actions = {
  edit: openEdit,
  delete: onDelete,
}

const customActions: CrudAction[] = [
  {
    auth: 'rotate',
    label: t('message.client.clientApiPolicyRotate'),
    icon: 'fa-solid fa-rotate',
    severity: 'warn',
    onClick: onRotate,
    visible: (row: any) => String(row.appId) === '0',
  },
]

onMounted(() => {
  loadApps()
  onQuery()
})
</script>
