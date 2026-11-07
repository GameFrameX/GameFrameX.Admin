<template>
  <div class="onlineMatchAbnormalTicket-container">
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
                    :queryAuth="'onlineMatchAbnormalTicket:abnormalPage'" @query="onQuery" />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #header>
        <span>{{ t('message.onlineCenter.matchAbnormalTicket.listTitle') }}</span>
      </template>
      <template #content>
        <OnlineCursorList v-if="canQueryServer" :key="`${serverSelectionVersion}:${listQueryVersion}`" ref="listRef" :request="abnormalRequest" :columns="columns" :pageSize="20">
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
 * 异常 Ticket 查询页。
 * 三类检索（Admin 自有枚举 AbnormalKind：LongWaiting/RepeatedCancel/Orphan）+ 游标翻页；
 * 状态展示走 displayKey 映射（onlineMatch.ticketStatus.*），Admin 不复制 Ticket 状态机语义。
 */
import { ref, computed, watch, onMounted, nextTick } from 'vue'
import ServerSelector from '/@/components/server-selector/ServerSelector.vue'
import { usePageServer } from '../components/usePageServer'
import Card from 'primevue/card'
import SearchForm from '/@/components/search-form/index.vue'
import OnlineCursorList from '/@/components/OnlineCursorList/index.vue'
import { formatUnix } from '../../../utils/formatTime.ts'
import { onlineMatchTicketQueryAbnormalPagePost } from '../../../api/onlineMatchTicketQuery'
import { AbnormalKind } from '../../../api/onlineMatchTicketQuery/types'
import { te as hasKey, t as translate } from '../../../i18n/index'
import type { OnlineCursorListParams } from '/@/components/OnlineCursorList/types'

const { serverIdOverride, effectiveServerId, canQueryServer, serverStateMessage, serverSelectionVersion, beginServerRequest } = usePageServer()

const queryParams = ref<Record<string, any>>({ abnormalKind: AbnormalKind.LongWaiting })
const listRef = ref()
const listQueryVersion = ref(0)

// AbnormalKind 三值（change.md A0：Admin 侧自有检索枚举，vault 无此概念；线缆为数字枚举）
const kindOptions = computed(() => [
  { label: t('message.onlineCenter.matchAbnormalTicket.kinds.longWaiting'), value: AbnormalKind.LongWaiting },
  { label: t('message.onlineCenter.matchAbnormalTicket.kinds.repeatedCancel'), value: AbnormalKind.RepeatedCancel },
  { label: t('message.onlineCenter.matchAbnormalTicket.kinds.orphan'), value: AbnormalKind.Orphan },
])

const searchFields = computed(() => [
  { key: 'abnormalKind', label: t('message.onlineCenter.matchAbnormalTicket.search.abnormalKind'), type: 'select', options: kindOptions.value, placeholder: t('message.onlineCenter.matchAbnormalTicket.search.pleaseSelectKind') },
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
  const { ...rest } = queryParams.value
  const kind = rest.abnormalKind ?? AbnormalKind.LongWaiting
  return { ...rest, abnormalKind: kind, serverId: effectiveServerId.value }
}

const abnormalRequest = async (p: OnlineCursorListParams) => {
  const request = beginServerRequest()
  if (!request) return { items: [], hasMore: false }
  const res: any = await onlineMatchTicketQueryAbnormalPagePost({ ...buildAbnormalParams(), ...p })
  if (!request.isCurrent()) return { items: [], hasMore: false }
  const page = res.data?.data
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
  await onQuery()
}, { flush: 'post' })
onMounted(onQuery)
</script>
