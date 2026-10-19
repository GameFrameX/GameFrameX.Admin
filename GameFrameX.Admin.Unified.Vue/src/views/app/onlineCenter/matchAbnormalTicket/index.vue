<template>
  <div class="onlineMatchAbnormalTicket-container">
    <Card>
      <template #content>
        <SearchForm v-model="queryParams" :fields="searchFields" :labelWidth="'110px'"
                    :queryAuth="'onlineMatchAbnormalTicket:abnormalPage'" @query="onQuery" />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #header>
        <span>{{ t('message.onlineCenter.matchAbnormalTicket.listTitle') }}</span>
      </template>
      <template #content>
        <OnlineCursorList ref="listRef" :request="abnormalRequest" :columns="columns" :pageSize="20">
          <template #body-status="{ data }">{{ displayStatus(data.status, data.statusDisplayKey) }}</template>
          <template #body-createdAt="{ data }">{{ formatUnix(data.createdAt) }}</template>
          <template #body-expiresAt="{ data }">{{ data.expiresAt ? formatUnix(data.expiresAt) : '-' }}</template>
        </OnlineCursorList>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="onlineMatchAbnormalTicket">
/**
 * 异常 Ticket 查询页（C341 / GFX-757）。
 * 三类检索（Admin 自有枚举 AbnormalKind：LongWaiting/RepeatedCancel/Orphan）+ 游标翻页；
 * 状态展示走 displayKey 映射（onlineMatch.ticketStatus.*），Admin 不复制 Ticket 状态机语义。
 */
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import Card from 'primevue/card'
import SearchForm from '/@/components/search-form/index.vue'
import OnlineCursorList from '/@/components/OnlineCursorList/index.vue'
import { formatUnix } from '../../../../utils/formatTime.ts'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import { onlineMatchTicketQueryAbnormalPagePost } from '../../../../api/onlineMatchTicketQuery'
import { AbnormalKind } from '../../../../api/onlineMatchTicketQuery/types'
import { te as hasKey, t as translate } from '../../../../i18n/index'
import type { OnlineCursorListParams } from '/@/components/OnlineCursorList/types'

const route = useRoute()
const { currentAppId, resolveInitialAppId } = useCurrentApp()

const queryParams = ref<Record<string, any>>({ abnormalKind: AbnormalKind.LongWaiting })
const listRef = ref()

// AbnormalKind 三值（change.md §3 A0：Admin 侧自有检索枚举，vault 无此概念；线缆为数字枚举）
const kindOptions = computed(() => [
  { label: t('message.onlineCenter.matchAbnormalTicket.kinds.longWaiting'), value: AbnormalKind.LongWaiting },
  { label: t('message.onlineCenter.matchAbnormalTicket.kinds.repeatedCancel'), value: AbnormalKind.RepeatedCancel },
  { label: t('message.onlineCenter.matchAbnormalTicket.kinds.orphan'), value: AbnormalKind.Orphan },
])

const searchFields = computed(() => [
  { key: 'abnormalKind', label: t('message.onlineCenter.matchAbnormalTicket.search.abnormalKind'), type: 'select', options: kindOptions.value, placeholder: t('message.onlineCenter.matchAbnormalTicket.search.pleaseSelectKind') },
  { key: 'serverId', label: t('message.onlineCenter.matchAbnormalTicket.search.serverId'), type: 'input', placeholder: t('message.onlineCenter.matchAbnormalTicket.search.pleaseEnterServerId') },
])

const columns = computed(() => [
  { field: 'ticketId', header: t('message.onlineCenter.matchAbnormalTicket.columns.ticketId'), width: '200px', sensitive: true },
  { field: 'partyId', header: t('message.onlineCenter.matchAbnormalTicket.columns.partyId'), width: '180px', sensitive: true },
  { field: 'mode', header: t('message.onlineCenter.matchAbnormalTicket.columns.mode'), width: '120px' },
  { field: 'region', header: t('message.onlineCenter.matchAbnormalTicket.columns.region'), width: '120px' },
  { field: 'status', header: t('message.onlineCenter.matchAbnormalTicket.columns.status'), width: '120px' },
  { field: 'createdAt', header: t('message.onlineCenter.matchAbnormalTicket.columns.createdAt'), width: '170px' },
  { field: 'expiresAt', header: t('message.onlineCenter.matchAbnormalTicket.columns.expiresAt'), width: '170px' },
  { field: 'waitingSeconds', header: t('message.onlineCenter.matchAbnormalTicket.columns.waitingSeconds'), width: '110px', align: 'right' as const },
  { field: 'cancelCount', header: t('message.onlineCenter.matchAbnormalTicket.columns.cancelCount'), width: '110px', align: 'right' as const },
])

/** 状态展示：displayKey（映射产物）命中词条则本地化，否则回退原值；unknown 兜底键由后端下发。 */
const displayStatus = (status: string, displayKey?: string | null): string => {
  const key = displayKey ?? `onlineMatch.ticketStatus.${String(status || 'unknown').toLowerCase()}`
  return hasKey(key) ? translate(key) : (status || '-')
}

function buildAbnormalParams() {
  const { appId, ...rest } = queryParams.value
  const kind = rest.abnormalKind ?? AbnormalKind.LongWaiting
  return { ...rest, abnormalKind: kind, appId: appId ?? currentAppId.value }
}

const abnormalRequest = async (p: OnlineCursorListParams) => {
  const res: any = await onlineMatchTicketQueryAbnormalPagePost({ ...buildAbnormalParams(), ...p })
  const page = res.data?.data
  return page ?? { items: [], hasMore: false }
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
