<template>
  <div class="onlineMatchFailureReason-container">
    <Card>
      <template #content>
        <div class="flex align-items-center gap-2 mb-3">
          <ServerSelector mode="local" v-model="serverIdOverride" />
          <button v-if="serverIdOverride !== undefined" type="button" class="p-button p-button-text" @click="serverIdOverride = undefined">
            {{ t('message.common.button.reset') }}
          </button>
        </div>
        <p v-if="!canQueryServer" role="status">{{ t(serverStateMessage) }}</p>
        <SearchForm v-model="queryParams" :fields="searchFields" :labelWidth="'110px'"
                    :queryAuth="'onlineMatchFailureReason:failurePage'" @query="onQuery" />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #header>
        <span>{{ t('message.onlineCenter.matchFailureReason.listTitle') }}</span>
      </template>
      <template #content>
        <div v-if="reasonSummary.length" class="flex flex-wrap gap-2 mb-3">
          <Tag v-for="item in reasonSummary" :key="item.reasonCode"
               class="onlineMatchFailureReason-summary-tag"
               :value="`${displayReason(item.reasonCode, item.reasonDisplayKey)} × ${item.count}`"
               severity="warn" rounded />
        </div>
        <OnlineCursorList v-if="canQueryServer" :key="`${serverSelectionVersion}:${listQueryVersion}`" ref="listRef" :request="failureRequest" :columns="columns" :pageSize="20">
          <template #body-reasonCode="{ data }">{{ displayReason(data.reasonCode, data.reasonDisplayKey) }}</template>
          <template #body-occurredAt="{ data }">{{ formatUnix(data.occurredAt) }}</template>
        </OnlineCursorList>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="onlineMatchFailureReason">
/**
 * 匹配失败原因查询页。
 * 原因码聚合（ReasonSummary[]）+ 下钻明细（按原因码过滤游标列表）；
 * 展示走 displayKey 映射（onlineMatch.failureReason.*），禁止按 ReasonText 文案判断。
 */
import { ref, computed, watch, onMounted, nextTick } from 'vue'
import ServerSelector from '/@/components/server-selector/ServerSelector.vue'
import { usePageServer } from '../components/usePageServer'
import Card from 'primevue/card'
import Tag from 'primevue/tag'
import SearchForm from '/@/components/search-form/index.vue'
import OnlineCursorList from '/@/components/OnlineCursorList/index.vue'
import { formatUnix } from '../../../utils/formatTime.ts'
import { onlineMatchTicketQueryFailurePagePost } from '../../../api/onlineMatchTicketQuery'
import type { OnlineMatchFailureReasonSummaryResponse } from '../../../api/onlineMatchTicketQuery/types'
import { te as hasKey, t as translate } from '../../../i18n/index'
import type { OnlineCursorListParams } from '/@/components/OnlineCursorList/types'

const { serverIdOverride, effectiveServerId, canQueryServer, serverStateMessage, serverSelectionVersion, beginServerRequest } = usePageServer()

const queryParams = ref<Record<string, any>>({})
const listRef = ref()
const listQueryVersion = ref(0)
const reasonSummary = ref<OnlineMatchFailureReasonSummaryResponse[]>([])

// 失败原因 Online 原值穷举（change.md A0：Timeout/CancelRace/RateLimited/DuplicateTicket）
const reasonOptions = computed(() => [
  { label: displayReason('Timeout'), value: 'Timeout' },
  { label: displayReason('CancelRace'), value: 'CancelRace' },
  { label: displayReason('RateLimited'), value: 'RateLimited' },
  { label: displayReason('DuplicateTicket'), value: 'DuplicateTicket' },
])

const searchFields = computed(() => [
  { key: 'reasonCode', label: t('message.onlineCenter.matchFailureReason.search.reasonCode'), type: 'select', options: reasonOptions.value, placeholder: t('message.onlineCenter.matchFailureReason.search.pleaseSelectReasonCode'), advanced: true },
  { key: 'timeRange', label: t('message.onlineCenter.matchFailureReason.search.timeRange'), type: 'dateRange', placeholder: t('message.onlineCenter.matchFailureReason.search.startTime'), advanced: true },
])

const columns = computed(() => [
  { field: 'ticketId', header: t('message.onlineCenter.matchFailureReason.columns.ticketId'), width: '200px', sensitive: true },
  { field: 'partyId', header: t('message.onlineCenter.matchFailureReason.columns.partyId'), width: '180px', sensitive: true },
  { field: 'reasonCode', header: t('message.onlineCenter.matchFailureReason.columns.reasonCode'), width: '140px' },
  { field: 'reasonText', header: t('message.onlineCenter.matchFailureReason.columns.reasonText'), width: '240px' },
  { field: 'occurredAt', header: t('message.onlineCenter.matchFailureReason.columns.occurredAt'), width: '170px' },
])

/** 原因展示：displayKey（映射产物）命中词条则本地化，否则回退原值；unknown 兜底键由后端下发。 */
const displayReason = (reasonCode: string, displayKey?: string | null): string => {
  const key = displayKey ?? `onlineMatch.failureReason.${String(reasonCode || 'unknown').toLowerCase()}`
  return hasKey(key) ? translate(key) : (reasonCode || '-')
}

function toUnixSeconds(value: unknown): unknown {
  if (value instanceof Date) {
    return Math.floor(value.getTime() / 1000)
  }
  if (Array.isArray(value)) {
    return value.map((item) => toUnixSeconds(item))
  }
  return value
}

function buildFailureParams() {
  const { timeRange, ...rest } = queryParams.value
  const base: Record<string, any> = { ...rest, serverId: effectiveServerId.value }
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

const failureRequest = async (p: OnlineCursorListParams) => {
  const request = beginServerRequest()
  if (!request) return { items: [], hasMore: false }
  const res: any = await onlineMatchTicketQueryFailurePagePost({ ...buildFailureParams(), ...p })
  if (!request.isCurrent()) return { items: [], hasMore: false }
  const page = res.data?.data
  reasonSummary.value = page?.reasonSummary ?? []
  return page ?? { items: [], hasMore: false }
}

// 区服选择经过列表验证后才允许请求。
const onQuery = async () => {
  if (!canQueryServer.value) {
    return
  }
  listQueryVersion.value += 1
  await nextTick()
  await listRef.value?.reload()
}

watch(serverSelectionVersion, async () => {
  reasonSummary.value = []
  await onQuery()
}, { flush: 'post' })
onMounted(onQuery)
</script>
