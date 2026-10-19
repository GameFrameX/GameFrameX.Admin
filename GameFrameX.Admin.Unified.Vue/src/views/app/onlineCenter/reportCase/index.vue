<template>
  <div class="onlineReportCase-container">
    <Card>
      <template #content>
        <SearchForm v-model="queryParams" :fields="searchFields" :labelWidth="'110px'"
                    authPrefix="reportCase" @query="onQuery" />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #header>
        <span>{{ t('message.onlineCenter.reportCase.listTitle') }}</span>
      </template>
      <template #content>
        <OnlineCursorList ref="listRef" :request="caseRequest" :columns="columns" :pageSize="20">
          <template #body-status="{ data }">
            <Tag :value="statusLabel(data.status)" :severity="statusSeverity(data.status)" />
          </template>
          <template #body-createdAt="{ data }">{{ formatUnix(data.createdAt) }}</template>
          <template #body-updatedAt="{ data }">{{ formatUnix(data.updatedAt) }}</template>
          <template #body-actions="{ data }">
            <Button v-auth="'reportCase:handle'" class="onlineReportCase-handle-btn"
                    :label="t('message.onlineCenter.reportCase.handle')"
                    icon="fa-solid fa-gavel" severity="warn" size="small"
                    :disabled="data.status === 3 || data.status === 4"
                    @click="openHandle(data)" />
          </template>
        </OnlineCursorList>
      </template>
    </Card>
    <HandleDrawer ref="handleDrawerRef" :appId="currentAppId" :serverId="queryParams.serverId"
                  @done="onQuery" />
  </div>
</template>

<script lang="ts" setup name="onlineReportCase">
/**
 * 举报案件处置页（C342 / GFX-758，vault:C7 S6.10）。
 * 案件队列证据链字段全渲染（举报人/被举报人/场景/MatchId/ChatMessageId/原因/证据/状态/处理结果）；
 * 处置经 HandleDrawer：决策枚举 + 理由必填 + Mute/Ban 时长条件必填 + 高危确认层。
 * 处置产生的处罚写 TenantGamePenaltyEntity 事实源后触发 Online 联动命令（服务端语义）。
 */
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import Card from 'primevue/card'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import SearchForm from '/@/components/search-form/index.vue'
import OnlineCursorList from '/@/components/OnlineCursorList/index.vue'
import HandleDrawer from '/@/views/app/onlineCenter/reportCase/component/HandleDrawer.vue'
import { formatUnix } from '../../../../utils/formatTime.ts'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import { onlineReportCasePagePost } from '../../../../api/onlineReportCase'
import type { OnlineCursorListParams } from '/@/components/OnlineCursorList/types'

const route = useRoute()
const { currentAppId, resolveInitialAppId } = useCurrentApp()

const queryParams = ref<Record<string, any>>({})
const listRef = ref()
const handleDrawerRef = ref()

// OnlineReportCaseStatusKind：Pending=1 待处理 / Handling=2 处理中 / Resolved=3 已处理 / Rejected=4 已驳回
const statusOptions = computed(() => [
  { label: t('message.onlineCenter.reportCase.status.pending'), value: 1 },
  { label: t('message.onlineCenter.reportCase.status.handling'), value: 2 },
  { label: t('message.onlineCenter.reportCase.status.resolved'), value: 3 },
  { label: t('message.onlineCenter.reportCase.status.rejected'), value: 4 },
])

const statusLabel = (value: number | undefined | null) =>
  statusOptions.value.find((o) => o.value === value)?.label ?? '-'

const statusSeverity = (value: number | undefined | null): any => {
  switch (value) {
    case 1: return 'warn'
    case 2: return 'info'
    case 3: return 'success'
    case 4: return 'secondary'
    default: return 'secondary'
  }
}

const searchFields = computed(() => [
  { key: 'serverId', label: t('message.onlineCenter.reportCase.search.serverId'), type: 'input', placeholder: t('message.onlineCenter.reportCase.search.pleaseEnterServerId') },
  { key: 'status', label: t('message.onlineCenter.reportCase.search.status'), type: 'select', options: statusOptions.value, placeholder: t('message.onlineCenter.reportCase.search.pleaseSelectStatus') },
  { key: 'reporterPlayerId', label: t('message.onlineCenter.reportCase.search.reporterPlayerId'), type: 'input', placeholder: t('message.onlineCenter.reportCase.search.pleaseEnterReporterPlayerId'), advanced: true },
  { key: 'timeRange', label: t('message.onlineCenter.reportCase.search.timeRange'), type: 'dateRange', placeholder: t('message.onlineCenter.reportCase.search.startTime'), advanced: true },
])

const columns = computed(() => [
  { field: 'caseId', header: t('message.onlineCenter.reportCase.columns.caseId'), width: '160px', sensitive: true },
  { field: 'status', header: t('message.onlineCenter.reportCase.columns.status'), width: '100px' },
  { field: 'reporterPlayerId', header: t('message.onlineCenter.reportCase.columns.reporterPlayerId'), width: '140px', sensitive: true },
  { field: 'reportedPlayerId', header: t('message.onlineCenter.reportCase.columns.reportedPlayerId'), width: '140px', sensitive: true },
  { field: 'scene', header: t('message.onlineCenter.reportCase.columns.scene'), width: '120px' },
  { field: 'reason', header: t('message.onlineCenter.reportCase.columns.reason'), width: '200px' },
  { field: 'evidence', header: t('message.onlineCenter.reportCase.columns.evidence'), width: '180px', sensitive: true },
  { field: 'matchId', header: t('message.onlineCenter.reportCase.columns.matchId'), width: '140px' },
  { field: 'chatMessageId', header: t('message.onlineCenter.reportCase.columns.chatMessageId'), width: '160px' },
  { field: 'handleResult', header: t('message.onlineCenter.reportCase.columns.handleResult'), width: '180px' },
  { field: 'createdAt', header: t('message.onlineCenter.reportCase.columns.createdAt'), width: '170px', bodyKey: 'createdAt' },
  { field: 'updatedAt', header: t('message.onlineCenter.reportCase.columns.updatedAt'), width: '170px', bodyKey: 'updatedAt' },
  { field: 'actions', header: t('message.onlineCenter.reportCase.columns.actions'), width: '120px' },
])

/** Date 实例 / 数组 → Unix 秒（与 useCrudTable.toUnixSeconds 同语义，cursor 形态独立维护）。 */
function toUnixSeconds(value: unknown): unknown {
  if (value instanceof Date) {
    return Math.floor(value.getTime() / 1000)
  }
  if (Array.isArray(value)) {
    return value.map((item) => toUnixSeconds(item))
  }
  return value
}

function buildCaseParams() {
  const { timeRange, appId, ...rest } = queryParams.value
  const base: Record<string, any> = { ...rest, appId: appId ?? currentAppId.value }
  if (Array.isArray(timeRange) && timeRange.length === 2) {
    const [start, end] = toUnixSeconds(timeRange) as [number | null, number | null]
    if (start) {
      base.startTime = start
    }
    if (end) {
      base.endTime = end
    }
  }
  return base
}

const caseRequest = async (p: OnlineCursorListParams) => {
  const res: any = await onlineReportCasePagePost({ ...buildCaseParams(), ...p })
  return res.data?.data ?? { items: [], hasMore: false }
}

const onQuery = async () => {
  await listRef.value?.reload()
}

const openHandle = (row: any) => {
  handleDrawerRef.value?.openDrawer(row)
}

watch(() => route.query.appId, async () => {
  currentAppId.value = resolveInitialAppId(route.query)
  queryParams.value.appId = currentAppId.value
  await onQuery()
})

onMounted(async () => {
  if (!currentAppId.value) {
    currentAppId.value = resolveInitialAppId(route.query)
  }
  queryParams.value.appId = currentAppId.value
  await onQuery()
})
</script>
