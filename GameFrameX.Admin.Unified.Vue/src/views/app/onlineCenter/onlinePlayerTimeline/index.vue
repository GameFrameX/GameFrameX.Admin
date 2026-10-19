<template>
  <div class="onlinePlayerTimeline-container">
    <Card>
      <template #content>
        <SearchForm v-model="queryParams" :fields="searchFields" :labelWidth="'110px'"
                    queryAuth="onlinePlayerTimeline:query" @query="onQuery" />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #header>
        <div class="flex align-items-center justify-content-between flex-wrap gap-2">
          <div class="flex align-items-center gap-2">
            <span>{{ t('message.onlineCenter.playerTimeline.listTitle') }}</span>
            <Button
              v-if="timelineForm.playerId && auth('onlinePlayerTimeline:execute')"
              v-tooltip.top="t('message.onlineCenter.playerTimeline.controlledOperationHint')"
              class="onlinePlayerTimeline-controlled-entry"
              :label="t('message.onlineCenter.controlledOperation.entryButton')"
              icon="fa-solid fa-shield-halved"
              size="small"
              severity="warn"
              outlined
              @click="goControlledOperation"
            />
          </div>
          <div class="flex align-items-center gap-2">
            <span class="text-color-secondary text-sm">{{ t('message.onlineCenter.playerTimeline.eventGroupFilter') }}</span>
            <Select
              v-model="selectedGroup"
              class="onlinePlayerTimeline-group-select"
              :options="groupOptions"
              optionLabel="label"
              optionValue="value"
              :placeholder="t('message.onlineCenter.playerTimeline.allGroups')"
              :showClear="true"
              size="small"
              @change="onQuery"
            />
          </div>
        </div>
      </template>
      <template #content>
        <div v-block-loading="state.loading" class="onlinePlayerTimeline-axis">
          <div v-if="state.snapshotTime != null" class="text-color-secondary mb-2">
            <i class="fa-solid fa-clock mr-1"></i>
            <small>{{ t('message.onlineCenter.cursorList.snapshotTime') }}：{{ formatSnapshot(state.snapshotTime) }}</small>
          </div>
          <div v-if="!state.loaded" class="text-center text-color-secondary p-4 onlinePlayerTimeline-empty">
            {{ t('message.onlineCenter.playerTimeline.notLoaded') }}
          </div>
          <div v-else-if="state.items.length === 0" class="text-center text-color-secondary p-4 onlinePlayerTimeline-empty">
            {{ t('message.onlineCenter.cursorList.empty') }}
          </div>
          <ul v-else class="onlinePlayerTimeline-events list-none p-0 m-0">
            <li
              v-for="event in state.items"
              :key="event.eventId"
              class="onlinePlayerTimeline-event relative pl-4 pb-3"
              :data-event-group="event.group ?? ''"
              :data-correlation-id="event.correlationId"
            >
              <span class="onlinePlayerTimeline-dot absolute border-circle"></span>
              <div class="p-2 border-round surface-border border-1">
                <div class="flex align-items-center justify-content-between flex-wrap gap-2">
                  <div class="flex align-items-center gap-2 flex-wrap">
                    <span class="onlinePlayerTimeline-group-badge text-xs px-2 py-1 border-round">{{ groupLabel(event) }}</span>
                    <span class="font-medium" data-field="eventType">{{ event.eventType }}</span>
                  </div>
                  <small class="text-color-secondary" data-field="occurredAt">{{ formatUnix(event.occurredAt) }}</small>
                </div>
                <div class="text-sm text-color-secondary mt-1 flex flex-wrap gap-3">
                  <span data-field="eventId">EventId：{{ event.eventId }}</span>
                  <span data-field="source">Source：{{ event.source }}</span>
                  <span data-field="correlationId">CorrelationId：{{ event.correlationId }}</span>
                </div>
                <div v-if="event.payloadSummary" class="text-sm mt-1 onlinePlayerTimeline-payload" data-field="payload">
                  {{ event.payloadSummary }}
                </div>
              </div>
            </li>
          </ul>
          <div v-if="state.hasMore" class="flex justify-content-center mt-3">
            <Button
              class="onlinePlayerTimeline-load-more"
              :label="t('message.onlineCenter.cursorList.loadMore')"
              icon="fa-solid fa-chevron-down"
              size="small"
              outlined
              :loading="state.loading"
              @click="loadMore"
            />
          </div>
        </div>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="onlinePlayerTimeline">
/**
 * 玩家时间线页（C344 / GFX-761）。
 * 只消费 Online 事件信封（EventId/CorrelationId 透传），缺事件即空槽展示，
 * 不做本地推断补齐；事件分组 Session/Match/Asset/Social/Penalty/LiveOps。
 */
import { reactive, ref, computed, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Card from 'primevue/card'
import Button from 'primevue/button'
import Select from 'primevue/select'
import SearchForm from '/@/components/search-form/index.vue'
import { formatUnix } from '../../../../utils/formatTime.ts'
import { auth } from '../../../../utils/authFunction.ts'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import { onlinePlayerTimelineQueryQueryPost } from '../../../../api/onlinePlayerTimelineQuery'
import { OnlinePlayerTimelineEventGroup, type OnlinePlayerTimelineEntryResponse } from '../../../../api/onlinePlayerTimelineQuery/types'
import {
  createCursorPagerState,
  beginLoad,
  applyFirstPage,
  appendPage,
  failLoad,
  buildRequestParams,
} from '/@/components/OnlineCursorList/cursorPager.ts'

// 事件分组名与线缆值派生自生成层枚举：请求侧 eventGroup 为数字枚举，
// 响应侧 group 为 Online 原值字符串（未知值 groupLabel 兜底展示）
type OnlineTimelineGroupName = keyof typeof OnlinePlayerTimelineEventGroup
const ONLINE_TIMELINE_EVENT_GROUPS = Object.keys(OnlinePlayerTimelineEventGroup)
  .filter((key) => Number.isNaN(Number(key))) as OnlineTimelineGroupName[]

const route = useRoute()
const router = useRouter()
const { currentAppId, resolveInitialAppId } = useCurrentApp()

const queryParams = ref<Record<string, any>>({})
// 后端 eventGroup 只收单值（缺省 = 全部分组），多选过滤不存在
const selectedGroup = ref<OnlineTimelineGroupName | null>(null)
const timelineForm = ref({ playerId: '' })

const state = reactive(createCursorPagerState<OnlinePlayerTimelineEntryResponse>())

const groupOptions = computed(() =>
  ONLINE_TIMELINE_EVENT_GROUPS.map((group) => ({ label: t(`message.onlineCenter.playerTimeline.groups.${group}`), value: group })))

const searchFields = computed(() => [
  { key: 'playerId', label: t('message.onlineCenter.playerTimeline.search.playerId'), type: 'input', placeholder: t('message.onlineCenter.playerTimeline.search.pleaseEnterPlayerId') },
  { key: 'serverId', label: t('message.onlineCenter.playerTimeline.search.serverId'), type: 'input', placeholder: t('message.onlineCenter.playerTimeline.search.pleaseEnterServerId') },
  { key: 'timeRange', label: t('message.onlineCenter.playerTimeline.search.timeRange'), type: 'dateRange', placeholder: t('message.onlineCenter.playerTimeline.search.startTime'), advanced: true },
])

const groupLabel = (event: OnlinePlayerTimelineEntryResponse) =>
  event.group ? t(`message.onlineCenter.playerTimeline.groups.${event.group}`) : event.group ?? '-'

const formatSnapshot = (value: string | number) => (typeof value === 'number' ? formatUnix(value) : String(value))

function toUnixSeconds(value: unknown): unknown {
  if (value instanceof Date) {
    return Math.floor(value.getTime() / 1000)
  }
  if (Array.isArray(value)) {
    return value.map((item) => toUnixSeconds(item))
  }
  return value
}

function buildTimelineParams() {
  const { timeRange, appId, ...rest } = queryParams.value
  const base: Record<string, any> = { ...rest, appId: appId ?? currentAppId.value }
  if (selectedGroup.value) {
    base.eventGroup = OnlinePlayerTimelineEventGroup[selectedGroup.value]
  }
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

const loadPage = async (params: { cursor?: string; pageSize?: number }) => {
  const res: any = await onlinePlayerTimelineQueryQueryPost({ ...buildTimelineParams(), ...params })
  return res.data?.data ?? { events: [], hasMore: false }
}

const onQuery = async () => {
  timelineForm.value.playerId = String(queryParams.value.playerId ?? '')
  const fresh = createCursorPagerState<OnlinePlayerTimelineEntryResponse>()
  Object.assign(state, fresh)
  if (!timelineForm.value.playerId) {
    return
  }
  beginLoad(state)
  try {
    const page = await loadPage(buildRequestParams(state, 20))
    applyFirstPage(state, { ...page, items: page.events ?? [] })
  } catch {
    failLoad(state)
  }
}

const loadMore = async () => {
  if (state.loading || !state.hasMore) {
    return
  }
  beginLoad(state)
  try {
    const page = await loadPage(buildRequestParams(state, 20))
    appendPage(state, { ...page, items: page.events ?? [] })
  } catch {
    failLoad(state)
  }
}

// 受控操作入口：携带当前 PlayerId / ServerId 预填（独立页承载，C344 交付清单四页面）
const goControlledOperation = () => {
  const query: Record<string, string> = { appId: String(currentAppId.value ?? '') }
  if (timelineForm.value.playerId) {
    query.playerId = timelineForm.value.playerId
  }
  if (queryParams.value.serverId) {
    query.serverId = String(queryParams.value.serverId)
  }
  router.push({ path: '/app/onlineCenter/controlledOperation', query })
}

watch(() => route.query.appId, async () => {
  currentAppId.value = resolveInitialAppId(route.query)
  queryParams.value.appId = currentAppId.value
})

onMounted(() => {
  if (!currentAppId.value) {
    currentAppId.value = resolveInitialAppId(route.query)
  }
  queryParams.value.appId = currentAppId.value
  if (typeof route.query.playerId === 'string' && route.query.playerId) {
    queryParams.value.playerId = route.query.playerId
  }
  if (typeof route.query.serverId === 'string' && route.query.serverId) {
    queryParams.value.serverId = route.query.serverId
  }
})
</script>

<style scoped>
.onlinePlayerTimeline-dot {
  left: 0;
  top: 0.6rem;
  width: 0.6rem;
  height: 0.6rem;
  background: var(--primary-color);
}

.onlinePlayerTimeline-event::before {
  content: '';
  position: absolute;
  left: 0.28rem;
  top: 1.2rem;
  bottom: -0.2rem;
  width: 1px;
  background: var(--surface-border);
}

.onlinePlayerTimeline-event:last-child::before {
  display: none;
}

.onlinePlayerTimeline-group-badge {
  background: var(--surface-100);
  color: var(--text-color-secondary);
}
</style>
