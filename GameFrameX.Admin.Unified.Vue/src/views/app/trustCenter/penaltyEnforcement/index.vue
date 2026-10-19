<!--
  聚合架构页（非独立资源）：本页属信任中心「处罚执行」视图，复用 tenantGamePenalty 的 API 与权限码
  （authPrefix=tenantGamePenaltyEntity），不复用 penalty 目录是刻意的域聚合——C302/GFX-715。
-->
<template>
  <div class="penaltyEnforcement-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="false"
            authPrefix="tenantGamePenaltyEntity"
            @query="onQuery"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" style="width: 100%" v-block-loading="loading" showGridlines stripedRows
                       rowKey="id" :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="tenantGamePenaltyEntity"
                       :customActions="customActions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage">
          <template #body-areaId="{ data }">
            {{ getAreaName(data.areaId) }}
          </template>
          <template #body-targetType="{ data }">
            {{ targetLabel(data.targetType) }}
          </template>
          <template #body-actionType="{ data }">
            <Tag :value="actionLabel(data.actionType)" :severity="actionTypeSeverity(data.actionType)"/>
          </template>
          <template #body-status="{ data }">
            <Tag :value="statusLabel(data.status)" :severity="statusSeverity(data.status)"/>
          </template>
          <template #body-runtimeStatus="{ data }">
            <Tag :value="runtimeLabel(computeRuntimeStatus(data))" :severity="runtimeStatusSeverity(computeRuntimeStatus(data))"/>
          </template>
          <template #body-expireTime="{ data }">
            {{ data.expireTime > 0 ? formatUnix(data.expireTime) : t('message.gameBusiness.penaltyEnforcement.permanent') }}
          </template>
          <template #body-sourceReportId="{ data }">{{ data.sourceReportId || '-' }}</template>
          <template #body-sourceAppealId="{ data }">{{ data.sourceAppealId || '-' }}</template>
          <template #body-revokeOperatorId="{ data }">{{ data.revokeOperatorId || '-' }}</template>
          <template #body-revokeReason="{ data }">{{ data.revokeReason || '-' }}</template>
          <template #body-revokeTime="{ data }">{{ formatUnix(data.revokeTime) }}</template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="penaltyEnforcement">
import {ref, computed, watch, onMounted} from 'vue'
import {useRoute, useRouter} from 'vue-router'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type {CrudAction, CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'
import {useCurrentApp} from '../../../../composables/useCurrentApp.ts'
import {formatUnix} from '../../../../utils/formatTime.ts'
import { tenantGamePenaltyEntityPagePost } from '../../../../api/tenantGamePenaltyEntity'
import { tenantGameAreaEntitySelectStringPost } from '../../../../api/tenantGameAreaEntity'
import {MAX_SELECT_SIZE} from '../../../../constants/pagination.ts'
import {showWarning} from '../../../../utils/message.ts'
import {
    PENALTY_ACTION_TYPE_OPTIONS,
    PENALTY_STATUS_OPTIONS,
    PENALTY_TARGET_TYPE_OPTIONS,
    RUNTIME_STATUS,
    actionTypeSeverity,
    computeRuntimeStatus,
    runtimeStatusSeverity,
    statusSeverity,
} from './constants'

const route = useRoute()
const router = useRouter()
const {resolveInitialAppId} = useCurrentApp()

const labelOf = (options: Array<{ value: number; label: string }>, value: number): string => {
    const found = options.find((o) => o.value === value)
    return found ? t(found.label) : ''
}
const targetLabel = (v: number) => labelOf(PENALTY_TARGET_TYPE_OPTIONS, v)
const actionLabel = (v: number) => labelOf(PENALTY_ACTION_TYPE_OPTIONS, v)
const statusLabel = (v: number) => labelOf(PENALTY_STATUS_OPTIONS, v)
const runtimeLabel = (status: string) => {
    if (status === RUNTIME_STATUS.Effective) {
        return t('message.gameBusiness.penaltyEnforcement.runtimeStatus.effective')
    }
    if (status === RUNTIME_STATUS.Expired) {
        return t('message.gameBusiness.penaltyEnforcement.runtimeStatus.expired')
    }
    return t('message.gameBusiness.penaltyEnforcement.runtimeStatus.revoked')
}

const searchFields = computed(() => [
    {key: 'targetType', label: t('message.gameBusiness.penaltyEnforcement.targetType.label'), type: 'select', options: PENALTY_TARGET_TYPE_OPTIONS, optionLabel: 'label', optionValue: 'value', placeholder: t('message.gameBusiness.penaltyEnforcement.pleaseSelectTargetType'), advanced: true},
    {key: 'targetId', label: t('message.gameBusiness.penaltyEnforcement.targetId'), type: 'input', placeholder: t('message.gameBusiness.penaltyEnforcement.pleaseEnterTargetId'), advanced: true},
    {key: 'status', label: t('message.gameBusiness.penaltyEnforcement.status.label'), type: 'select', options: PENALTY_STATUS_OPTIONS, optionLabel: 'label', optionValue: 'value', placeholder: t('message.gameBusiness.penaltyEnforcement.pleaseSelectStatus'), advanced: true},
    {key: 'actionType', label: t('message.gameBusiness.penaltyEnforcement.actionType.label'), type: 'select', options: PENALTY_ACTION_TYPE_OPTIONS, optionLabel: 'label', optionValue: 'value', placeholder: t('message.gameBusiness.penaltyEnforcement.pleaseSelectActionType'), advanced: true},
])

const {
    loading,
    tableData,
    queryParams,
    tableParams,
    onQuery,
} = useCrudTable({
    api: {pageApi: tenantGamePenaltyEntityPagePost},
    immediate: false,
})

queryParams.value.appId = resolveInitialAppId(route.query)

const onPage = usePrimeTablePage(tableParams, onQuery)

// 区服名映射（按当前应用加载，对齐 redeemCenter/gameRedeemCodeEntity）。
const areaList = ref<any>([])
const getAreaName = (areaId: number | null | undefined) => {
    if (!areaId) {
        return t('message.gameBusiness.penaltyEnforcement.allAreas')
    }
    const item = areaList.value.find((a: any) => a.id === areaId)
    return item ? item.label : String(areaId)
}
const loadAreaList = async (appId?: number) => {
    const res = await tenantGameAreaEntitySelectStringPost({appId: appId, pageSize: MAX_SELECT_SIZE})
    areaList.value = res.data?.data ?? []
}

watch(() => queryParams.value.appId, (newVal) => {
    queryParams.value.areaId = undefined
    loadAreaList(newVal)
})

// 跳转入口（issue 第 3 条）。目标路由由 C123（GFX-113）实现；未合入时 fallback 提示。
const navigateIfReady = (path: string, query: Record<string, unknown>) => {
    const resolved = router.resolve({path, query})
    if (!resolved.matched || resolved.matched.length === 0) {
        showWarning(t('message.gameBusiness.penaltyEnforcement.targetNotReady'))
        return
    }
    router.push({path, query})
}

const gotoPenalty = (row: any) => {
    navigateIfReady('/app/trustCenter/gamePenaltyEntity', {appId: queryParams.value.appId, targetType: row.targetType, targetId: row.targetId})
}

const gotoAppeal = (row: any) => {
    navigateIfReady('/app/trustCenter/gamePenaltyAppealEntity', {appId: queryParams.value.appId, penaltyId: row.id})
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'areaId', header: t('message.gameBusiness.role.areaName'), width: '130px', bodyKey: 'areaId'},
  {field: 'targetType', header: t('message.gameBusiness.penaltyEnforcement.targetType.label'), width: '110px', bodyKey: 'targetType'},
  {field: 'targetId', header: t('message.gameBusiness.penaltyEnforcement.targetId'), width: '200px'},
  {field: 'actionType', header: t('message.gameBusiness.penaltyEnforcement.actionType.label'), width: '130px', bodyKey: 'actionType'},
  {field: 'status', header: t('message.gameBusiness.penaltyEnforcement.status.label'), width: '110px', bodyKey: 'status'},
  {field: 'runtimeStatus', header: t('message.gameBusiness.penaltyEnforcement.runtimeStatus.label'), width: '130px', bodyKey: 'runtimeStatus'},
  {field: 'expireTime', header: t('message.gameBusiness.penaltyEnforcement.expiresAt'), width: '180px', bodyKey: 'expireTime'},
  {field: 'operatorId', header: t('message.gameBusiness.penaltyEnforcement.operatorId'), width: '120px'},
  {field: 'sourceReportId', header: t('message.gameBusiness.penaltyEnforcement.sourceReportId'), width: '130px', bodyKey: 'sourceReportId'},
  {field: 'sourceAppealId', header: t('message.gameBusiness.penaltyEnforcement.sourceAppealId'), width: '130px', bodyKey: 'sourceAppealId'},
  {field: 'revokeOperatorId', header: t('message.gameBusiness.penaltyEnforcement.revokeOperatorId'), width: '130px', bodyKey: 'revokeOperatorId'},
  {field: 'revokeReason', header: t('message.gameBusiness.penaltyEnforcement.revokeReason'), width: '220px', bodyKey: 'revokeReason'},
  {field: 'revokeTime', header: t('message.gameBusiness.penaltyEnforcement.revokeTime'), width: '180px', bodyKey: 'revokeTime'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '160px', bodyKey: 'createdTime'},
])

const customActions = computed<CrudAction[]>(() => [
    {auth: 'page', label: t('message.gameBusiness.penaltyEnforcement.action.gotoPenalty'), icon: 'fa-solid fa-arrow-right', onClick: gotoPenalty},
    {auth: 'page', label: t('message.gameBusiness.penaltyEnforcement.action.gotoAppeal'), icon: 'fa-solid fa-comment', onClick: gotoAppeal},
])

watch(() => route.query.appId, async () => {
    queryParams.value.appId = resolveInitialAppId(route.query)
    queryParams.value.areaId = undefined
    await onQuery()
})

onMounted(async () => {
    await loadAreaList(queryParams.value.appId)
    await onQuery()
})
</script>
