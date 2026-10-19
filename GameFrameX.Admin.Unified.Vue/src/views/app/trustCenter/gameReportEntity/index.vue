<template>
  <div class="gameReportEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="gameReportEntity"
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
                       authPrefix="gameReportEntity"
                       :customActions="customActions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage" showIdColumn idColumnWidth="180px">
          <template #body-targetType="{ data }">
            <Tag :value="getEnumLabel(REPORT_TARGET_OPTIONS, data.targetType)" :severity="getEnumSeverity(REPORT_TARGET_OPTIONS, data.targetType)" />
          </template>
          <template #body-reportType="{ data }">
            <Tag :value="getEnumLabel(REPORT_TYPE_OPTIONS, data.reportType)" :severity="getEnumSeverity(REPORT_TYPE_OPTIONS, data.reportType)" />
          </template>
          <template #body-status="{ data }">
            <Tag :value="getEnumLabel(REPORT_STATUS_OPTIONS, data.status)" :severity="getEnumSeverity(REPORT_STATUS_OPTIONS, data.status)" />
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>
        <addDialog ref="addDialogRef" :title="addTitle" :appId="queryParams.appId" @reloadTable="onQuery" />
        <linkPenaltyDialog ref="linkPenaltyDialogRef" @reloadTable="onQuery" />
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="gamereport">
import {ref, computed} from 'vue'
import { useRoute } from 'vue-router'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type { CrudAction, CrudColumnConfig } from '../../../../components/crud-data-table/types.ts'
import SearchForm from '/@/components/search-form/index.vue'
import Tag from 'primevue/tag'
import { useCrudTable } from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage, useRouteAppIdQuerySync} from '../../../../composables'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import {formatUnix} from '../../../../utils/formatTime.ts'
import addDialog from '/@/views/app/trustCenter/gameReportEntity/component/AddDialog.vue'
import linkPenaltyDialog from '/@/views/app/trustCenter/gameReportEntity/component/LinkPenaltyDialog.vue'
import { tenantGameReportEntityPagePost } from '../../../../api/tenantGameReportEntity'
import {
  REPORT_TYPE_OPTIONS,
  REPORT_STATUS_OPTIONS,
  PENALTY_TARGET_TYPE_OPTIONS,
  findEnumOption,
  type EnumOption,
} from './constants'

const REPORT_TARGET_OPTIONS = PENALTY_TARGET_TYPE_OPTIONS

const route = useRoute()
const addDialogRef = ref()
const linkPenaltyDialogRef = ref()
const addTitle = ref('')

const {resolveInitialAppId} = useCurrentApp()

const getEnumLabel = (options: EnumOption[], value: number | undefined | null) => {
  const opt = findEnumOption(options, value)
  return opt ? t(opt.labelKey) : ''
}
const getEnumSeverity = (options: EnumOption[], value: number | undefined | null): any => {
  const opt = findEnumOption(options, value)
  return opt?.severity ?? 'secondary'
}

const targetTypeOptions = computed(() =>
  PENALTY_TARGET_TYPE_OPTIONS.map((o) => ({ label: t(o.labelKey), value: o.value })),
)
const reportTypeOptions = computed(() =>
  REPORT_TYPE_OPTIONS.map((o) => ({ label: t(o.labelKey), value: o.value })),
)
const reportStatusOptions = computed(() =>
  REPORT_STATUS_OPTIONS.map((o) => ({ label: t(o.labelKey), value: o.value })),
)

const searchFields = computed(() => [
  { key: 'targetType', label: t('message.gameBusiness.trustSafety.report.targetType'), type: 'select', options: targetTypeOptions.value, placeholder: t('message.gameBusiness.trustSafety.report.pleaseSelectTargetType'), advanced: true },
  { key: 'targetId', label: t('message.gameBusiness.trustSafety.report.targetId'), type: 'input', placeholder: t('message.gameBusiness.trustSafety.report.pleaseEnterTargetId'), advanced: true },
  { key: 'reportType', label: t('message.gameBusiness.trustSafety.report.reportType'), type: 'select', options: reportTypeOptions.value, placeholder: t('message.gameBusiness.trustSafety.report.pleaseSelectReportType'), advanced: true },
  { key: 'status', label: t('message.gameBusiness.trustSafety.report.status'), type: 'select', options: reportStatusOptions.value, placeholder: t('message.gameBusiness.trustSafety.report.pleaseSelectStatus'), advanced: true },
])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
} = useCrudTable({
  api: { pageApi: tenantGameReportEntityPagePost },
  immediate: true,
})

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAdd = () => {
  addTitle.value = t('message.gameBusiness.trustSafety.report.addReport')
  addDialogRef.value.openDialog({})
}

const onLinkPenalty = (row: any) => {
  linkPenaltyDialogRef.value.openDialog(row)
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'reporterId', header: t('message.gameBusiness.trustSafety.report.reporterId'), width: '140px'},
  {field: 'targetType', header: t('message.gameBusiness.trustSafety.report.targetType'), width: '120px', bodyKey: 'targetType'},
  {field: 'targetId', header: t('message.gameBusiness.trustSafety.report.targetId'), width: '200px'},
  {field: 'reportType', header: t('message.gameBusiness.trustSafety.report.reportType'), width: '140px', bodyKey: 'reportType'},
  {field: 'scene', header: t('message.gameBusiness.trustSafety.report.scene'), width: '200px'},
  {field: 'status', header: t('message.gameBusiness.trustSafety.report.status'), width: '120px', bodyKey: 'status'},
  {field: 'resultPenaltyId', header: t('message.gameBusiness.trustSafety.report.resultPenaltyId'), width: '140px'},
  {field: 'handlerId', header: t('message.gameBusiness.trustSafety.report.handlerId'), width: '140px'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '180px', bodyKey: 'createdTime'},
])

const customActions: CrudAction[] = [
  {
    auth: 'linkPenalty',
    label: t('message.gameBusiness.trustSafety.report.linkPenalty'),
    icon: 'fa-solid fa-link',
    onClick: onLinkPenalty,
    visible: (row: any) => row.status === 3,
  },
]

useRouteAppIdQuerySync({
  route,
  queryParams,
  resolveAppId: resolveInitialAppId,
  onQuery,
})
</script>
