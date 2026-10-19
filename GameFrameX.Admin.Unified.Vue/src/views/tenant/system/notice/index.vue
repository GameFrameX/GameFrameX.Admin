<template>
  <div class="sys-notice-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :defaultSearchKey="false"
            :showAdd="true"
            authPrefix="systemNotice"
            @query="onQuery"
            @add="openAddNotice"
        />
      </template>
    </Card>

    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows style="width: 100%"
            :totalRecords="tableParams.total" :rows="tableParams.pageSize" :rowsPerPageOptions="[10, 20, 50, 100]"
            authPrefix="systemNotice"
            :customActions="customActions"
            :columns="columns"
            :exportQuery="queryParams"
            actionColumnWidth="200px"
            @page="onPage">
          <template #body-content="{ data }">{{ removeHtml(data.content) }}</template>
          <template #body-type="{ data }">
            <Tag v-if="data.type === 1" :value="t('system.notice.notice')"/>
            <Tag v-else severity="warn" :value="t('system.notice.announcement')"/>
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
          <template #body-status="{ data }">
            <Tag v-if="data.status === 1" severity="info" :value="t('system.notice.published')"/>
            <Tag v-else severity="warn" :value="t('system.notice.unpublished')"/>
          </template>
          <template #body-publicTime="{ data }">{{ formatUnix(data.publicTime) }}</template>
          <template #action-extra="{ data }">
            <Button link @click="publicNotice(data)" v-auth="'systemNotice:public'" :disabled="data.status === 1">
              <i class="fa-solid fa-location-dot mr-1"></i>
              {{ t('system.notice.publish') }}
            </Button>
            <Button link @click="openEditNotice(data)" v-auth="'systemNotice:update'" :disabled="data.status === 1">
              <i class="fa-solid fa-pen-to-square mr-1"></i>
              {{ t('common.button.edit') }}
            </Button>
            <Button link severity="danger" @click="delNotice(data)" v-auth="'systemNotice:delete'"
                    :disabled="data.status === 1">
              <i class="fa-solid fa-trash mr-1"></i>
              {{ t('common.button.delete') }}
            </Button>
          </template>
        </CrudDataTable>
      </template>
    </Card>

    <EditNotice ref="editNoticeRef" :title="editTitle" @query="onQuery"/>
  </div>
</template>

<script lang="ts" setup name="systemNotice">
import {faLocationDot, faPenToSquare, faTrash} from '../../../../utils/icons.ts';
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import SearchForm from '/@/components/search-form/index.vue'

import {computed, ref} from 'vue'
import {showConfirm, showSuccess} from '../../../../utils/message.ts'
import commonFunction from '../../../../utils/commonFunction.ts'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'
import {tenantNoticePagePost, tenantNoticeDeletePost, tenantNoticePublishPost} from '../../../../api/tenantNotice'
import EditNotice from './component/EditNotice.vue'
import type {CrudAction, CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'
import {formatUnix} from '../../../../utils/formatTime.ts'

const editNoticeRef = ref<InstanceType<typeof EditNotice>>()
const editTitle = ref('')
const {removeHtml} = commonFunction()

const typeOptions = computed(() => [
  {label: t('system.notice.notice'), value: 1},
  {label: t('system.notice.announcement'), value: 2},
])

const searchFields = computed(() => [
  {key: 'title', label: t('system.notice.noticeTitle'), type: 'input', placeholder: t('system.notice.noticeTitle')},
  {key: 'type', label: t('common.label.type'), type: 'select', options: typeOptions.value, optionLabel: 'label', optionValue: 'value', placeholder: t('common.label.type')},
])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
} = useCrudTable({
  api: {
    pageApi: (params) => tenantNoticePagePost(params),
    deleteApi: (params) => tenantNoticeDeletePost({id: params.id}),
  },
  immediate: true,
})

queryParams.value = {title: undefined, type: undefined}
tableParams.value.pageSize = 10

const delNotice = async (row: any) => {
  const confirmed = await showConfirm(`${t('system.notice.deleteNoticeConfirm')}：【${row.title}】?`, t('common.message.tip'), {type: 'danger'})
  if (!confirmed) return
  await tenantNoticeDeletePost({id: row.id})
  onQuery()
  showSuccess(t('common.message.deleteSuccess'))
}

const openAddNotice = () => {
  editTitle.value = t('system.notice.addNoticeTitle')
  editNoticeRef.value?.openDialog({type: 1})
}

const openEditNotice = (row: any) => {
  editTitle.value = t('system.notice.editNoticeTitle')
  editNoticeRef.value?.openDialog(row)
}

const publicNotice = async (row: any) => {
  const confirmed = await showConfirm(`${t('system.notice.publishConfirm')}：【${row.title}】${t('system.notice.publishIrrevocable')}`, t('common.message.tip'))
  if (!confirmed) return
  await tenantNoticePublishPost({id: row.id})
  onQuery()
  showSuccess(t('system.notice.publishSuccess'))
}

const onPage = usePrimeTablePage(tableParams, onQuery)

// 占位 customActions：仅用于触发 CrudDataTable 内置操作列的显隐与权限聚合判断
// （所有按钮均通过 #action-extra 插槽原样渲染，保留原有的 disabled/v-auth 逻辑）
const customActions: CrudAction[] = [
  {auth: 'public', label: '', onClick: () => {}, visible: () => false},
  {auth: 'update', label: '', onClick: () => {}, visible: () => false},
  {auth: 'delete', label: '', onClick: () => {}, visible: () => false},
]

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'title', header: t('system.notice.noticeTitle')},
  {field: 'content', header: t('system.notice.noticeContent'), bodyKey: 'content'},
  {field: 'type', header: t('common.label.type'), width: '100px', align: 'center', bodyKey: 'type'},
  {field: 'createdTime', header: t('common.label.createTime'), align: 'center', bodyKey: 'createdTime'},
  {field: 'status', header: t('common.label.status'), width: '100px', align: 'center', bodyKey: 'status'},
  {field: 'publicUserName', header: t('system.notice.publisher'), align: 'center'},
  {field: 'publicTime', header: t('system.notice.publishTime'), align: 'center', bodyKey: 'publicTime'},
])
</script>
