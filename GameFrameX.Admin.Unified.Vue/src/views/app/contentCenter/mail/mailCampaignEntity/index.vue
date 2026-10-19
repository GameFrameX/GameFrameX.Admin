<template>
  <div class="tenantGameMailCampaignEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="mailCampaign"
            :defaultSearchKey="false"
            @query="onQuery"
            @add="openAdd"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" style="width: 100%" v-block-loading="loading" showGridlines stripedRows
                       rowKey="id" :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="mailCampaign"
                       :actions="actions"
                       :customActions="customActions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       actionColumnWidth="380px"
                       @page="onPage" showIdColumn idColumnWidth="180px">
          <template #body-mailType="{ data }">
            <Tag :value="mailTypeDesc(data.mailType)"/>
          </template>
          <template #body-publishStatus="{ data }">
            <Tag :value="statusDesc(data.publishStatus)" :severity="statusSeverity(data.publishStatus)"/>
          </template>
          <template #body-templateId="{ data }">
            {{ data.templateId && data.templateId > 0 ? data.templateId : '-' }}
          </template>
          <template #body-serverCampaignId="{ data }">
            <span v-if="data.serverCampaignId && data.serverCampaignId > 0">{{ data.serverCampaignId }}</span>
            <span v-else>-</span>
          </template>
          <template #body-publishVersion="{ data }">
            {{ data.publishVersion && data.publishVersion > 0 ? data.publishVersion : '-' }}
          </template>
          <template #body-publishOperator="{ data }">
            {{ data.publishOperator || '-' }}
          </template>
          <template #body-publishedTime="{ data }">{{ formatUnix(data.publishedTime) }}</template>
          <template #body-revokeOperator="{ data }">
            {{ data.revokeOperator || '-' }}
          </template>
          <template #body-revokedTime="{ data }">{{ formatUnix(data.revokedTime) }}</template>
          <template #body-lastServerMessage="{ data }">
            <span v-if="data.lastServerMessage" class="text-red-500 break-all">{{ data.lastServerMessage }}</span>
            <span v-else>-</span>
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editTitle" :appId="queryParams.appId" @reloadTable="onQuery"/>
        <detailDialog ref="detailDialogRef"/>
        <publishConfirmDialog ref="publishDialogRef" @done="onQuery"/>
        <revokeDialog ref="revokeDialogRef" @done="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="tenantGameMailCampaignEntity">
// authPrefix 用菜单 Key 前缀（mailCampaign，无 tenant 前缀）：权限下发的是 global_menu.key
// （GetOwnButtonPermListAsync 取 u.Key），Permission 字段是后端 API 鉴权用的另一套
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import SearchForm from '/@/components/search-form/index.vue'
import type { CrudAction, CrudColumnConfig } from '../../../../../components/crud-data-table/types.ts'
import {formatUnix} from '../../../../../utils/formatTime.ts'
import {ref, computed} from 'vue'
import { useRoute } from 'vue-router'
import { showSuccess } from '../../../../../utils/message.ts'
import { useCrudTable } from '../../../../../composables/useCrudTable.ts'
import {usePrimeTablePage, useRouteAppIdQuerySync } from '../../../../../composables'
import { useCurrentApp } from '../../../../../composables/useCurrentApp.ts'
import { t } from '../../../../../i18n/index.ts'
import editDialog from '/@/views/app/contentCenter/mail/mailCampaignEntity/component/EditDialog.vue'
import detailDialog from '/@/views/app/contentCenter/mail/mailCampaignEntity/component/DetailDialog.vue'
import publishConfirmDialog from '/@/views/app/contentCenter/mail/mailCampaignEntity/component/PublishConfirmDialog.vue'
import revokeDialog from '/@/views/app/contentCenter/mail/mailCampaignEntity/component/RevokeDialog.vue'
import { tenantGameMailCampaignEntityPagePost, tenantGameMailCampaignEntityDeletePost, tenantGameMailCampaignEntityPreviewPost } from '../../../../../api/tenantGameMailCampaignEntity'
import {
  MAIL_CAMPAIGN_MAIL_TYPE_OPTIONS,
  MAIL_CAMPAIGN_PUBLISH_STATUS_OPTIONS,
  MAIL_CAMPAIGN_PUBLISH_STATUS_SEVERITY,
  MailCampaignPublishStatus,
} from './constants'

const route = useRoute()
const {resolveInitialAppId} = useCurrentApp()

const editDialogRef = ref()
const detailDialogRef = ref()
const publishDialogRef = ref()
const revokeDialogRef = ref()
const editTitle = ref('')

const mailTypeDesc = (v: number) => {
  const item = MAIL_CAMPAIGN_MAIL_TYPE_OPTIONS.find((i) => i.value === v)
  return item ? t(item.labelKey) : v
}
const statusDesc = (v: number) => {
  const item = MAIL_CAMPAIGN_PUBLISH_STATUS_OPTIONS.find((i) => i.value === v)
  return item ? t(item.labelKey) : v
}
const statusSeverity = (v: number) => MAIL_CAMPAIGN_PUBLISH_STATUS_SEVERITY[v] ?? 'info'

const canEdit = (status: number) => status === MailCampaignPublishStatus.Draft || status === MailCampaignPublishStatus.Failed

const searchFields = computed(() => [
  { key: 'name', label: t('message.gameBusiness.mailCampaign.name'), type: 'input', placeholder: t('message.gameBusiness.mailCampaign.name') },
  { key: 'mailType', label: t('message.gameBusiness.mailCampaign.mailType'), type: 'select', options: buildOptions(MAIL_CAMPAIGN_MAIL_TYPE_OPTIONS), optionLabel: 'label', optionValue: 'value', placeholder: t('message.gameBusiness.mailCampaign.mailType'), advanced: true },
  { key: 'publishStatus', label: t('message.gameBusiness.mailCampaign.publishStatus'), type: 'select', options: buildOptions(MAIL_CAMPAIGN_PUBLISH_STATUS_OPTIONS), optionLabel: 'label', optionValue: 'value', placeholder: t('message.gameBusiness.mailCampaign.publishStatus'), advanced: true },
  { key: 'serverCampaignId', label: t('message.gameBusiness.mailCampaign.serverCampaignId'), type: 'input', placeholder: t('message.gameBusiness.mailCampaign.serverCampaignId'), advanced: true },
])

const buildOptions = (opts: { value: number; labelKey: string }[]) => opts.map((o) => ({ value: o.value, label: t(o.labelKey) }))

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
  onDelete,
} = useCrudTable({
  api: { pageApi: tenantGameMailCampaignEntityPagePost, deleteApi: tenantGameMailCampaignEntityDeletePost },
  immediate: true,
})

const onPage = usePrimeTablePage(tableParams, onQuery)

useRouteAppIdQuerySync({
  route,
  queryParams,
  resolveAppId: resolveInitialAppId,
  onQuery,
})

const openAdd = () => {
  editTitle.value = t('message.gameBusiness.mailCampaign.add')
  editDialogRef.value.openDialog({})
}

const openEdit = (row: any) => {
  editTitle.value = t('message.gameBusiness.mailCampaign.edit')
  editDialogRef.value.openDialog(row)
}

const openDetail = (row: any) => {
  detailDialogRef.value.openDialog(row)
}

const onPreview = async (row: any) => {
  try {
    const res: any = await tenantGameMailCampaignEntityPreviewPost({ id: row.id })
    const hit = res.data?.data?.estimatedHitCount
    const msg = hit === -1 || hit === undefined
      ? t('message.gameBusiness.mailCampaign.estimatedHitCountUnknown')
      : `${t('message.gameBusiness.mailCampaign.estimatedHitCountLabel')}: ${hit}`
    showSuccess(msg)
  } catch {
    // 全局拦截器已弹错误提示
  }
}

const onPublish = (row: any) => {
  publishDialogRef.value.openDialog(row)
}

const onRevoke = (row: any) => {
  revokeDialogRef.value.openDialog(row)
}

const actions = {
  edit: { onClick: openEdit, visible: (row: any) => canEdit(row.publishStatus) },
  delete: { onClick: onDelete, visible: (row: any) => canEdit(row.publishStatus) && row.isDelete !== true },
}

const customActions: CrudAction[] = [
  {
    auth: 'preview',
    label: t('message.gameBusiness.mailCampaign.preview'),
    icon: 'fa-solid fa-eye',
    onClick: onPreview,
    visible: (row: any) => canEdit(row.publishStatus),
  },
  {
    auth: 'publish',
    label: t('message.gameBusiness.mailCampaign.publish'),
    icon: 'fa-solid fa-paper-plane',
    severity: 'success',
    onClick: onPublish,
    visible: (row: any) => canEdit(row.publishStatus),
  },
  {
    auth: 'revoke',
    label: t('message.gameBusiness.mailCampaign.revoke'),
    icon: 'fa-solid fa-rotate-left',
    severity: 'warn',
    onClick: onRevoke,
    visible: (row: any) => row.publishStatus === MailCampaignPublishStatus.Published,
  },
  {
    auth: 'detail',
    label: t('message.gameBusiness.detail'),
    icon: 'fa-solid fa-circle-info',
    onClick: openDetail,
  },
]

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'name', header: t('message.gameBusiness.mailCampaign.name'), width: '160px'},
  {field: 'mailType', header: t('message.gameBusiness.mailCampaign.mailType'), width: '120px', bodyKey: 'mailType'},
  {field: 'publishStatus', header: t('message.gameBusiness.mailCampaign.publishStatus'), width: '120px', bodyKey: 'publishStatus'},
  {field: 'templateId', header: t('message.gameBusiness.mailCampaign.template'), width: '100px', bodyKey: 'templateId'},
  {field: 'serverCampaignId', header: t('message.gameBusiness.mailCampaign.serverCampaignId'), width: '140px', bodyKey: 'serverCampaignId'},
  {field: 'publishVersion', header: t('message.gameBusiness.mailCampaign.publishVersion'), width: '100px', bodyKey: 'publishVersion'},
  {field: 'publishOperator', header: t('message.gameBusiness.mailCampaign.publishOperator'), width: '120px', bodyKey: 'publishOperator'},
  {field: 'publishedTime', header: t('message.gameBusiness.mailCampaign.publishedTime'), width: '160px', bodyKey: 'publishedTime'},
  {field: 'revokeOperator', header: t('message.gameBusiness.mailCampaign.revokeOperator'), width: '120px', bodyKey: 'revokeOperator'},
  {field: 'revokedTime', header: t('message.gameBusiness.mailCampaign.revokedTime'), width: '160px', bodyKey: 'revokedTime'},
  {field: 'lastServerMessage', header: t('message.gameBusiness.mailCampaign.failureReason'), width: '200px', bodyKey: 'lastServerMessage'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '180px', bodyKey: 'createdTime'},
])
</script>
