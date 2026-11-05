<template>
  <div class="tenantClientApiPolicyEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :defaultSearchKey="true"
            :showAdd="true"
            authPrefix="clientApiPolicy"
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
                       authPrefix="clientApiPolicy"
                       :actions="actions"
                       :customActions="customActions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       showIdColumn
                       idColumnWidth="120px"
                       @page="onPage">

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
import Tag from 'primevue/tag'
import SearchForm from '/@/components/search-form/index.vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type {CrudAction, CrudColumnConfig} from '../../../components/crud-data-table/types.ts'
import {useCrudTable} from '../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../composables'

import {formatUnix} from '../../../utils/formatTime.ts'
import {showConfirm, showSuccess} from '../../../utils/message.ts'
import editDialog from './component/EditDialog.vue'
import { clientApiPolicyPagePost, clientApiPolicyDeletePost, clientApiPolicyDetailGet, clientApiPolicyRotatePost } from '../../../api/clientApiPolicy'

const editDialogRef = ref()
const editTitle = ref('')

const searchFields = computed(() => [

])

const columns = computed<CrudColumnConfig[]>(() => [
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
    pageApi: clientApiPolicyPagePost,
    deleteApi: clientApiPolicyDeletePost,
  },
  immediate: false,
})

queryParams.value = {}

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAdd = () => {
  editTitle.value = t('message.client.clientApiPolicyAddTitle')
  editDialogRef.value.openDialog({})
}

const openEdit = async (row: any) => {
  editTitle.value = t('message.client.clientApiPolicyEditTitle')
  try {
    const res: any = await clientApiPolicyDetailGet({id: row.id})
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
const onRotate = async (row: any) => {
  const confirmed = await showConfirm(
      t('message.client.clientApiPolicyRotateConfirm'),
      t('message.common.message.tip'),
  )
  if (!confirmed) {
    return
  }
  await clientApiPolicyRotatePost({id: row.id})
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

  },
]

onMounted(() => {

  onQuery()
})
</script>
