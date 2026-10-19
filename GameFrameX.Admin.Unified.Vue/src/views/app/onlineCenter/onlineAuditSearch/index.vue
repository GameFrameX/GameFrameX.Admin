<template>
  <div class="onlineAuditSearch-container">
    <Card>
      <template #content>
        <SearchForm v-model="queryParams" :fields="searchFields" :labelWidth="'110px'"
                    authPrefix="onlineAuditSearch" @query="onQuery" />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #header>
        <span>{{ t('message.onlineCenter.auditSearch.listTitle') }}</span>
      </template>
      <template #content>
        <OnlineCursorList ref="listRef" :request="auditRequest" :columns="columns" :pageSize="20">
          <template #body-source="{ data }">
            <Tag :value="sourceLabel(data.source)" :severity="sourceSeverity(data.source)" />
          </template>
          <template #body-occurredAt="{ data }">{{ formatUnix(data.occurredAt) }}</template>
        </OnlineCursorList>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="onlineAuditSearch">
/**
 * 统一审计检索页（C344 / GFX-761）。
 * 三来源（Admin 操作日志 / Online 审计事件 / 各域命令记录）经后端
 * OnlineAuditSourceMap 五维统一映射后透传：source 列为数字枚举（1=管理操作 2=Online 事件 3=域命令）；本页只读，无写路径。
 */
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import Card from 'primevue/card'
import Tag from 'primevue/tag'
import SearchForm from '/@/components/search-form/index.vue'
import OnlineCursorList from '/@/components/OnlineCursorList/index.vue'
import { formatUnix } from '../../../../utils/formatTime.ts'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import { onlineAuditSearchPagePost } from '../../../../api/onlineAuditSearch'
import type { OnlineCursorListParams } from '/@/components/OnlineCursorList/types'

const route = useRoute()
const { currentAppId, resolveInitialAppId } = useCurrentApp()

const queryParams = ref<Record<string, any>>({})
const listRef = ref()

// 五维过滤：Operator / 对象（目标玩家）/ 时间 / 作用域（App+区服）/ 命令类型
const searchFields = computed(() => [
  { key: 'operator', label: t('message.onlineCenter.auditSearch.search.operatorId'), type: 'input', placeholder: t('message.onlineCenter.auditSearch.search.pleaseEnterOperatorId') },
  { key: 'targetPlayerId', label: t('message.onlineCenter.auditSearch.search.targetPlayerId'), type: 'input', placeholder: t('message.onlineCenter.auditSearch.search.pleaseEnterTargetPlayerId') },
  { key: 'commandKind', label: t('message.onlineCenter.auditSearch.search.commandKind'), type: 'input', placeholder: t('message.onlineCenter.auditSearch.search.pleaseEnterCommandKind') },
  { key: 'serverId', label: t('message.onlineCenter.auditSearch.search.serverId'), type: 'input', placeholder: t('message.onlineCenter.auditSearch.search.pleaseEnterServerId'), advanced: true },
  { key: 'timeRange', label: t('message.onlineCenter.auditSearch.search.timeRange'), type: 'dateRange', placeholder: t('message.onlineCenter.auditSearch.search.startTime'), advanced: true },
])

// OnlineAuditSourceKind 数字枚举：1=AdminOperation 管理操作 / 2=OnlineEvent Online 事件 / 3=DomainCommand 域命令
const sourceOptions = computed(() => [
  { label: t('message.onlineCenter.auditSearch.source.adminOperation'), value: 1 },
  { label: t('message.onlineCenter.auditSearch.source.onlineEvent'), value: 2 },
  { label: t('message.onlineCenter.auditSearch.source.domainCommand'), value: 3 },
])

const sourceLabel = (value: number | undefined | null) =>
  sourceOptions.value.find((o) => o.value === value)?.label ?? '-'

const sourceSeverity = (value: number | undefined | null): any => {
  switch (value) {
    case 1: return 'secondary'
    case 2: return 'info'
    case 3: return 'warn'
    default: return 'secondary'
  }
}

const columns = computed(() => [
  { field: 'source', header: t('message.onlineCenter.auditSearch.columns.source'), width: '140px' },
  { field: 'operatorIdentifier', header: t('message.onlineCenter.auditSearch.columns.operatorIdentifier'), width: '160px', sensitive: true },
  { field: 'commandType', header: t('message.onlineCenter.auditSearch.columns.commandType'), width: '160px' },
  { field: 'appId', header: t('message.onlineCenter.auditSearch.columns.appId'), width: '110px' },
  { field: 'serverId', header: t('message.onlineCenter.auditSearch.columns.serverId'), width: '120px' },
  { field: 'targetPlayerIdentifier', header: t('message.onlineCenter.auditSearch.columns.targetPlayerIdentifier'), width: '150px', sensitive: true },
  { field: 'summary', header: t('message.onlineCenter.auditSearch.columns.summary'), width: '200px' },
  { field: 'correlationId', header: t('message.onlineCenter.auditSearch.columns.correlationId'), width: '190px' },
  { field: 'occurredAt', header: t('message.onlineCenter.auditSearch.columns.occurredAt'), width: '170px', bodyKey: 'occurredAt' },
])

function toUnixSeconds(value: unknown): unknown {
  if (value instanceof Date) {
    return Math.floor(value.getTime() / 1000)
  }
  if (Array.isArray(value)) {
    return value.map((item) => toUnixSeconds(item))
  }
  return value
}

function buildAuditParams() {
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

const auditRequest = async (p: OnlineCursorListParams) => {
  const res: any = await onlineAuditSearchPagePost({ ...buildAuditParams(), ...p })
  return res.data?.data ?? { items: [], hasMore: false }
}

const onQuery = async () => {
  await listRef.value?.reload()
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
