<template>
  <div class="antiCheatEventEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="false"
            authPrefix="antiCheatEventEntity"
            @query="onQuery"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" style="width: 100%" v-block-loading="loading" showGridlines stripedRows
                       rowKey="id" :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="antiCheatEventEntity"
                       :customActions="customActions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage">
          <template #body-provider="{ data }">
            <Tag :value="providerLabel(data.provider)" severity="info"/>
          </template>
          <template #body-targetType="{ data }">
            {{ targetLabel(data.targetType) }}
          </template>
          <template #body-riskLevel="{ data }">
            <Tag :value="riskLabel(data.riskLevel)" :severity="riskLevelSeverity(data.riskLevel)"/>
          </template>
          <template #body-suggestedAction="{ data }">
            <Tag :value="suggestedLabel(data.suggestedAction)" :severity="suggestedActionSeverity(data.suggestedAction)"/>
          </template>
          <template #body-manualReviewState="{ data }">
            <Tag :value="reviewLabel(data.manualReviewState)" :severity="reviewStateSeverity(data.manualReviewState)"/>
          </template>
          <template #body-occurredTime="{ data }">{{ formatUnix(data.occurredTime) }}</template>
          <template #body-idempotencyKey="{ data }">
            <code style="word-break: break-all">{{ data.idempotencyKey }}</code>
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>
        <detailDialog ref="detailDialogRef"/>
        <reviewDialog ref="reviewDialogRef" @reviewed="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="antiCheatEventEntity">
import {ref, computed, onMounted} from 'vue'
import {useRoute} from 'vue-router'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type {CrudAction, CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage, useRouteAppIdQuerySync} from '../../../../composables'
import {useCurrentApp} from '../../../../composables/useCurrentApp.ts'
import {formatUnix} from '../../../../utils/formatTime.ts'
import detailDialog from '/@/views/app/trustCenter/antiCheatEventEntity/component/DetailDialog.vue'
import reviewDialog from '/@/views/app/trustCenter/antiCheatEventEntity/component/ReviewDialog.vue'
import { tenantGameAntiCheatEventEntityPagePost } from '../../../../api/tenantGameAntiCheatEventEntity'
import {
    ANTI_CHEAT_PROVIDER_OPTIONS,
    ANTI_CHEAT_SUGGESTED_ACTION_OPTIONS,
    ANTI_CHEAT_TARGET_TYPE_OPTIONS,
    MANUAL_REVIEW_STATE,
    MANUAL_REVIEW_STATE_OPTIONS,
    RISK_LEVEL_OPTIONS,
    riskLevelSeverity,
    reviewStateSeverity,
    suggestedActionSeverity,
} from './constants'

const route = useRoute()
const detailDialogRef = ref()
const reviewDialogRef = ref()

const {resolveInitialAppId} = useCurrentApp()

const labelOf = (options: Array<{ value: number; label: string }>, value: number): string => {
    const found = options.find((o) => o.value === value)
    return found ? t(found.label) : ''
}
const providerLabel = (v: number) => labelOf(ANTI_CHEAT_PROVIDER_OPTIONS, v)
const riskLabel = (v: number) => labelOf(RISK_LEVEL_OPTIONS, v)
const suggestedLabel = (v: number) => labelOf(ANTI_CHEAT_SUGGESTED_ACTION_OPTIONS, v)
const reviewLabel = (v: number) => labelOf(MANUAL_REVIEW_STATE_OPTIONS, v)
const targetLabel = (v: number) => labelOf(ANTI_CHEAT_TARGET_TYPE_OPTIONS, v)

const searchFields = computed(() => [
    {key: 'provider', label: t('message.gameBusiness.antiCheat.provider.label'), type: 'select', options: ANTI_CHEAT_PROVIDER_OPTIONS, optionLabel: 'label', optionValue: 'value', placeholder: t('message.gameBusiness.antiCheat.pleaseSelectProvider'), advanced: true},
    {key: 'riskLevel', label: t('message.gameBusiness.antiCheat.riskLevel.label'), type: 'select', options: RISK_LEVEL_OPTIONS, optionLabel: 'label', optionValue: 'value', placeholder: t('message.gameBusiness.antiCheat.pleaseSelectRiskLevel'), advanced: true},
    {key: 'manualReviewState', label: t('message.gameBusiness.antiCheat.reviewState.label'), type: 'select', options: MANUAL_REVIEW_STATE_OPTIONS, optionLabel: 'label', optionValue: 'value', placeholder: t('message.gameBusiness.antiCheat.pleaseSelectReviewState'), advanced: true},
    {key: 'targetType', label: t('message.gameBusiness.antiCheat.targetType.label'), type: 'select', options: ANTI_CHEAT_TARGET_TYPE_OPTIONS, optionLabel: 'label', optionValue: 'value', placeholder: t('message.gameBusiness.antiCheat.pleaseSelectTargetType'), advanced: true},
    {key: 'targetId', label: t('message.gameBusiness.antiCheat.targetId'), type: 'input', placeholder: t('message.gameBusiness.antiCheat.pleaseEnterTargetId'), advanced: true},
])

const {
    loading,
    tableData,
    queryParams,
    tableParams,
    onQuery,
} = useCrudTable({
    api: {pageApi: tenantGameAntiCheatEventEntityPagePost},
    immediate: false,
})

const onPage = usePrimeTablePage(tableParams, onQuery)

const openDetail = (row: any) => {
    detailDialogRef.value.open(row.id)
}

const openReview = (row: any) => {
    reviewDialogRef.value.open(row)
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'provider', header: t('message.gameBusiness.antiCheat.provider.label'), width: '140px', bodyKey: 'provider'},
  {field: 'targetType', header: t('message.gameBusiness.antiCheat.targetType.label'), width: '110px', bodyKey: 'targetType'},
  {field: 'targetId', header: t('message.gameBusiness.antiCheat.targetId'), width: '200px'},
  {field: 'riskLevel', header: t('message.gameBusiness.antiCheat.riskLevel.label'), width: '110px', bodyKey: 'riskLevel'},
  {field: 'suggestedAction', header: t('message.gameBusiness.antiCheat.suggestedAction.label'), width: '130px', bodyKey: 'suggestedAction'},
  {field: 'manualReviewState', header: t('message.gameBusiness.antiCheat.reviewState.label'), width: '120px', bodyKey: 'manualReviewState'},
  {field: 'occurredTime', header: t('message.gameBusiness.antiCheat.occurredAt'), width: '180px', bodyKey: 'occurredTime'},
  {field: 'reviewerId', header: t('message.gameBusiness.antiCheat.reviewerId'), width: '120px'},
  {field: 'idempotencyKey', header: t('message.gameBusiness.antiCheat.idempotencyKey'), width: '220px', bodyKey: 'idempotencyKey'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '160px', bodyKey: 'createdTime'},
])

const customActions = computed<CrudAction[]>(() => [
    {auth: 'detail', label: t('message.gameBusiness.antiCheat.action.detail'), icon: 'fa-solid fa-eye', onClick: openDetail},
    {auth: 'review', label: t('message.gameBusiness.antiCheat.action.review'), icon: 'fa-solid fa-gavel', severity: 'warn', onClick: openReview, visible: (row: any) => row.manualReviewState === MANUAL_REVIEW_STATE.Pending},
])

useRouteAppIdQuerySync({
    route,
    queryParams,
    resolveAppId: resolveInitialAppId,
    onQuery,
})

onMounted(async () => {
    await onQuery()
})
</script>
