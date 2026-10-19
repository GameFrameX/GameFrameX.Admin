<template>
  <div class="gamePenaltyAppealEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="gamePenaltyAppealEntity"
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
                       authPrefix="gamePenaltyAppealEntity"
                       :customActions="customActions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage" showIdColumn idColumnWidth="180px">
          <template #body-status="{ data }">
            <Tag :value="getEnumLabel(APPEAL_STATUS_OPTIONS, data.status)" :severity="getEnumSeverity(APPEAL_STATUS_OPTIONS, data.status)" />
          </template>
          <template #body-reviewTime="{ data }">{{ formatUnix(data.reviewTime) }}</template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>
        <addDialog ref="addDialogRef" :title="addTitle" @reloadTable="onQuery" />
        <reviewDialog ref="reviewDialogRef" @reloadTable="onQuery" />
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="gameappeal">
import {ref, computed} from 'vue'
import { useRoute } from 'vue-router'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type { CrudAction, CrudColumnConfig } from '../../../../components/crud-data-table/types.ts'
import SearchForm from '/@/components/search-form/index.vue'
import Tag from 'primevue/tag'
import { useCrudTable } from '../../../../composables/useCrudTable.ts'
import { usePrimeTablePage, useRouteAppIdQuerySync } from '../../../../composables'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import {formatUnix} from '../../../../utils/formatTime.ts'
import addDialog from '/@/views/app/trustCenter/gamePenaltyAppealEntity/component/AddDialog.vue'
import reviewDialog from '/@/views/app/trustCenter/gamePenaltyAppealEntity/component/ReviewDialog.vue'
import { tenantGamePenaltyAppealEntityPagePost } from '../../../../api/tenantGamePenaltyAppealEntity'
import {
  APPEAL_STATUS_OPTIONS,
  findEnumOption,
  type EnumOption,
} from './constants'

const route = useRoute()
const addDialogRef = ref()
const reviewDialogRef = ref()
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

const statusOptions = computed(() => APPEAL_STATUS_OPTIONS.map((o) => ({ label: t(o.labelKey), value: o.value })))

const searchFields = computed(() => [
  { key: 'penaltyId', label: t('message.gameBusiness.trustSafety.appeal.penaltyId'), type: 'number', placeholder: t('message.gameBusiness.trustSafety.appeal.pleaseEnterPenaltyId'), advanced: true },
  { key: 'appellantId', label: t('message.gameBusiness.trustSafety.appeal.appellantId'), type: 'number', placeholder: t('message.gameBusiness.trustSafety.appeal.pleaseEnterAppellantId'), advanced: true },
  { key: 'status', label: t('message.gameBusiness.trustSafety.appeal.status'), type: 'select', options: statusOptions.value, placeholder: t('message.gameBusiness.trustSafety.appeal.pleaseSelectStatus'), advanced: true },
])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
} = useCrudTable({
  api: { pageApi: tenantGamePenaltyAppealEntityPagePost },
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
  addTitle.value = t('message.gameBusiness.trustSafety.appeal.addAppeal')
  addDialogRef.value.openDialog({})
}

const onReview = (row: any) => {
  reviewDialogRef.value.openDialog(row)
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'penaltyId', header: t('message.gameBusiness.trustSafety.appeal.penaltyId'), width: '140px'},
  {field: 'appellantId', header: t('message.gameBusiness.trustSafety.appeal.appellantId'), width: '140px'},
  {field: 'reason', header: t('message.gameBusiness.trustSafety.appeal.reason'), width: '240px'},
  {field: 'status', header: t('message.gameBusiness.trustSafety.appeal.status'), width: '120px', bodyKey: 'status'},
  {field: 'reviewerId', header: t('message.gameBusiness.trustSafety.appeal.reviewerId'), width: '140px'},
  {field: 'reviewRemark', header: t('message.gameBusiness.trustSafety.appeal.reviewRemark'), width: '200px'},
  {field: 'reviewTime', header: t('message.gameBusiness.trustSafety.appeal.reviewTime'), width: '180px', bodyKey: 'reviewTime'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '180px', bodyKey: 'createdTime'},
])

const customActions: CrudAction[] = [
  {
    auth: 'review',
    label: t('message.gameBusiness.trustSafety.appeal.review'),
    icon: 'fa-solid fa-gavel',
    onClick: onReview,
    visible: (row: any) => row.status === 1,
  },
]
</script>
