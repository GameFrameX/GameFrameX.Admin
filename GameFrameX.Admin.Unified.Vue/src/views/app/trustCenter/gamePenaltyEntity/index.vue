<template>
  <div class="gamePenaltyEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="gamePenaltyEntity"
            @query="onQuery"
            @add="openAdd"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <!-- 表格动作列前缀用已落地的端点权限族 tenantGamePenaltyEntity:*（GFX-818 联调发现：
             legacy gamePenaltyEntity: 前缀无端点映射，showActionColumn 恒隐藏整列，#action-extra
             内的「同步 Online 运行时」按钮结构性不可达；SearchForm 前缀属 allowlist 残差另论） -->
        <CrudDataTable :value="tableData" style="width: 100%" v-block-loading="loading" showGridlines stripedRows
                       rowKey="id" :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="tenantGamePenaltyEntity"
                       :customActions="customActions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage" showIdColumn idColumnWidth="180px">
          <template #body-areaId="{ data }">{{ getAreaName(data.areaId) }}</template>
          <template #body-targetType="{ data }">
            <Tag :value="getEnumLabel(PENALTY_TARGET_TYPE_OPTIONS, data.targetType)" :severity="getEnumSeverity(PENALTY_TARGET_TYPE_OPTIONS, data.targetType)" />
          </template>
          <template #body-actionType="{ data }">
            <Tag :value="getEnumLabel(PENALTY_ACTION_TYPE_OPTIONS, data.actionType)" :severity="getEnumSeverity(PENALTY_ACTION_TYPE_OPTIONS, data.actionType)" />
          </template>
          <template #body-status="{ data }">
            <Tag :value="getEnumLabel(PENALTY_STATUS_OPTIONS, data.status)" :severity="getEnumSeverity(PENALTY_STATUS_OPTIONS, data.status)" />
          </template>
          <template #body-expireTime="{ data }">{{ formatUnix(data.expireTime) }}</template>
          <template #body-revokeTime="{ data }">{{ formatUnix(data.revokeTime) }}</template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>
        <addDialog ref="addDialogRef" :title="addTitle" :appId="queryParams.appId" @reloadTable="onQuery" />
        <revokeDialog ref="revokeDialogRef" @reloadTable="onQuery" />
        <syncOnlineResultDialog ref="syncResultDialogRef" />
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="gamepenalty">
import {ref, computed, watch, onMounted} from 'vue'
import { useRoute } from 'vue-router'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type { CrudAction, CrudColumnConfig } from '../../../../components/crud-data-table/types.ts'
import SearchForm from '/@/components/search-form/index.vue'
import Tag from 'primevue/tag'
import { useCrudTable } from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import {formatUnix} from '../../../../utils/formatTime.ts'
import addDialog from '/@/views/app/trustCenter/gamePenaltyEntity/component/AddDialog.vue'
import revokeDialog from '/@/views/app/trustCenter/gamePenaltyEntity/component/RevokeDialog.vue'
import syncOnlineResultDialog from '/@/views/app/trustCenter/gamePenaltyEntity/component/SyncOnlineResultDialog.vue'
import { tenantGamePenaltyEntityPagePost, tenantGamePenaltyEntitySyncOnlineRuntimePost } from '../../../../api/tenantGamePenaltyEntity'
import { tenantGameAreaEntitySelectStringPost } from '../../../../api/tenantGameAreaEntity'
import { showConfirm, showSuccess, showError } from '../../../../utils/message.ts'
import {MAX_SELECT_SIZE} from '../../../../constants/pagination.ts'
import {
  PENALTY_TARGET_TYPE_OPTIONS,
  PENALTY_ACTION_TYPE_OPTIONS,
  PENALTY_STATUS_OPTIONS,
  findEnumOption,
  type EnumOption,
} from './constants'

const route = useRoute()
const addDialogRef = ref()
const revokeDialogRef = ref()
const syncResultDialogRef = ref()
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

const targetTypeOptions = computed(() => PENALTY_TARGET_TYPE_OPTIONS.map((o) => ({ label: t(o.labelKey), value: o.value })))
const actionTypeOptions = computed(() => PENALTY_ACTION_TYPE_OPTIONS.map((o) => ({ label: t(o.labelKey), value: o.value })))
const statusOptions = computed(() => PENALTY_STATUS_OPTIONS.map((o) => ({ label: t(o.labelKey), value: o.value })))

const searchFields = computed(() => [
  { key: 'targetType', label: t('message.gameBusiness.trustSafety.penalty.targetType'), type: 'select', options: targetTypeOptions.value, placeholder: t('message.gameBusiness.trustSafety.penalty.pleaseSelectTargetType'), advanced: true },
  { key: 'targetId', label: t('message.gameBusiness.trustSafety.penalty.targetId'), type: 'input', placeholder: t('message.gameBusiness.trustSafety.penalty.pleaseEnterTargetId'), advanced: true },
  { key: 'actionType', label: t('message.gameBusiness.trustSafety.penalty.actionType'), type: 'select', options: actionTypeOptions.value, placeholder: t('message.gameBusiness.trustSafety.penalty.pleaseSelectActionType'), advanced: true },
  { key: 'status', label: t('message.gameBusiness.trustSafety.penalty.status'), type: 'select', options: statusOptions.value, placeholder: t('message.gameBusiness.trustSafety.penalty.pleaseSelectStatus'), advanced: true },
])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
} = useCrudTable({
  api: { pageApi: tenantGamePenaltyEntityPagePost },
  immediate: true,
})

queryParams.value.appId = resolveInitialAppId(route.query)

const onPage = usePrimeTablePage(tableParams, onQuery)

// 区服名映射（按当前应用加载）
const areaList = ref<any>([])
const getAreaName = (areaId: number | null | undefined) => {
  if (areaId === undefined || areaId === null) {
    return t('message.gameBusiness.trustSafety.penalty.allServer')
  }
  for (let i = 0; i < areaList.value.length; i++) {
    if (areaList.value[i].id == areaId) {
      return areaList.value[i].label
    }
  }
  return String(areaId)
}
const loadAreaList = async (appId?: number) => {
  const res = await tenantGameAreaEntitySelectStringPost({ appId: appId, pageSize: MAX_SELECT_SIZE })
  areaList.value = res.data.data ?? []
}

const openAdd = () => {
  addTitle.value = t('message.gameBusiness.trustSafety.penalty.addPenalty')
  addDialogRef.value.openDialog({})
}

const onRevoke = (row: any) => {
  revokeDialogRef.value.openDialog(row)
}

// 可联动的动作白名单：Ban=1 / RestrictLogin=2 / Mute=3（与后端 MapActionTypeToLinkCommand 一致；
// RestrictTrade=4 / RestrictMatch=5 纯落账动作无 Online 运行时对应物，服务端抛 PenaltyActionNotLinkable）
const LINKABLE_ACTION_TYPES = [1, 2, 3]

// 「同步 Online 运行时」仅对生效中（status=1）且可联动的处罚开放（C342 / GFX-758）
const canSyncOnline = (row: any) =>
  row.status === 1 && LINKABLE_ACTION_TYPES.includes(row.actionType)

const onSyncOnline = async (row: any) => {
  const confirmed = await showConfirm(
    t('message.gameBusiness.trustSafety.penalty.syncConfirmMessage', { targetId: row.targetId ?? '' }),
    t('message.gameBusiness.trustSafety.penalty.syncConfirmTitle'),
    { type: 'danger' },
  )
  if (!confirmed) {
    return
  }
  try {
    const res: any = await tenantGamePenaltyEntitySyncOnlineRuntimePost({ id: row.id })
    const link: any = res.data?.data
    if (!link) {
      showError(t('message.gameBusiness.trustSafety.penalty.syncFailed'))
      return
    }
    showSuccess(link.allSucceeded
      ? t('message.gameBusiness.trustSafety.penalty.syncSuccess')
      : t('message.gameBusiness.trustSafety.penalty.syncPartialFailed'))
    syncResultDialogRef.value.openDialog(link, { targetId: row.targetId, operatorId: row.operatorId })
  } catch {
    // 全局拦截器已弹错误提示
  }
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'areaId', header: t('message.gameBusiness.trustSafety.penalty.areaId'), width: '140px', bodyKey: 'areaId'},
  {field: 'targetType', header: t('message.gameBusiness.trustSafety.penalty.targetType'), width: '120px', bodyKey: 'targetType'},
  {field: 'targetId', header: t('message.gameBusiness.trustSafety.penalty.targetId'), width: '200px'},
  {field: 'actionType', header: t('message.gameBusiness.trustSafety.penalty.actionType'), width: '140px', bodyKey: 'actionType'},
  {field: 'status', header: t('message.gameBusiness.trustSafety.penalty.status'), width: '120px', bodyKey: 'status'},
  {field: 'reason', header: t('message.gameBusiness.trustSafety.penalty.reason'), width: '240px'},
  {field: 'expireTime', header: t('message.gameBusiness.trustSafety.penalty.expiresAt'), width: '180px', bodyKey: 'expireTime'},
  {field: 'operatorId', header: t('message.gameBusiness.trustSafety.penalty.operatorId'), width: '140px'},
  {field: 'revokeReason', header: t('message.gameBusiness.trustSafety.penalty.revokeReason'), width: '200px'},
  {field: 'revokeTime', header: t('message.gameBusiness.trustSafety.penalty.revokeTime'), width: '180px', bodyKey: 'revokeTime'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '180px', bodyKey: 'createdTime'},
])

const customActions: CrudAction[] = [
  {
    auth: 'revoke',
    label: t('message.gameBusiness.trustSafety.penalty.revoke'),
    icon: 'fa-solid fa-rotate-left',
    onClick: onRevoke,
    visible: (row: any) => row.status === 1,
  },
  // 「同步 Online 运行时」走 customActions 而非 #action-extra 插槽：showActionColumn 仅按
  // customActions 的权限码门控整列，插槽按钮会被门控连带隐藏（GFX-818 联调发现——legacy
  // gamePenaltyEntity: 前缀无显式按钮行，revoke 仅为 permission:auto 隐式行，列恒不渲染）
  {
    auth: 'syncOnlineRuntime',
    label: t('message.gameBusiness.trustSafety.penalty.syncOnline'),
    icon: 'fa-solid fa-rotate',
    severity: 'info',
    onClick: onSyncOnline,
    visible: canSyncOnline,
  },
]

watch(() => queryParams.value.appId, (newVal) => {
  queryParams.value.areaId = undefined
  loadAreaList(newVal)
})

watch(() => route.query.appId, async () => {
  queryParams.value.appId = resolveInitialAppId(route.query)
  queryParams.value.areaId = undefined
  await onQuery()
})

onMounted(async () => {
  await loadAreaList(queryParams.value.appId)
})
</script>
