<template>
  <div class="onlineMatchQueueOverview-container">
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
                    :queryAuth="'onlineMatchQueueOverview:overview'" @query="onQuery" />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #header>
        <div class="flex justify-content-between align-items-center w-full">
          <span>{{ t('message.onlineCenter.matchQueueOverview.listTitle') }}</span>
          <Button class="onlineMatchQueueOverview-refresh-btn" size="small" outlined
                  icon="fa-solid fa-rotate" :loading="loading"
                  :label="t('message.onlineCenter.matchQueueOverview.refresh')" @click="restartPolling" />
        </div>
      </template>
      <template #content>
        <div v-if="overview" class="flex align-items-center mb-3">
          <i class="fa-solid fa-clock text-color-secondary text-sm mr-1"></i>
          <small class="text-color-secondary mr-3">{{ t('message.onlineCenter.cursorList.snapshotTime') }}：{{ formatUnix(overview.serverTime) }}</small>
          <small v-if="overview.snapshotNote" class="text-color-secondary">{{ overview.snapshotNote }}</small>
        </div>
        <div v-if="queues.length" class="grid">
          <div v-for="queue in queues" :key="`${queue.mode}-${queue.region}`" class="col-12 md:col-6 lg:col-4">
            <div class="surface-100 p-3 border-round h-full onlineMatchQueueOverview-card">
              <div class="flex justify-content-between align-items-center mb-2">
                <span class="font-medium">{{ queue.mode }} / {{ queue.region }}</span>
                <Tag class="onlineMatchQueueOverview-depth" :value="String(queue.queueDepth)"
                     severity="info" rounded />
              </div>
              <div class="text-color-secondary text-sm mb-1">{{ t('message.onlineCenter.matchQueueOverview.queueDepth') }}</div>
              <div class="text-3xl font-bold mb-2 onlineMatchQueueOverview-depth-value">{{ queue.queueDepth }}</div>
              <div class="flex justify-content-between text-sm">
                <span><span class="text-color-secondary">{{ t('message.onlineCenter.matchQueueOverview.averageWait') }}：</span>{{ queue.averageWaitSeconds }}s</span>
                <span><span class="text-color-secondary">{{ t('message.onlineCenter.matchQueueOverview.throughput') }}：</span>{{ queue.throughputPerMinute }}/min</span>
              </div>
            </div>
          </div>
        </div>
        <div v-else-if="loaded" class="text-center text-color-secondary p-4">
          {{ t('message.onlineCenter.matchQueueOverview.empty') }}
        </div>
        <div v-else class="text-center text-color-secondary p-4">
          {{ t('message.onlineCenter.matchQueueOverview.notLoaded') }}
        </div>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="onlineMatchQueueOverview">
/**
 * 匹配队列总览页（C341 / GFX-757）。
 * 只读快照观测：Queues[] 数值卡片（Mode/Region 维度的深度/平均等待/吞吐）+ ServerTime 快照标注；
 * 默认 10s 轮询，遇 Online 7xxx 镜像码（限流/风控）即停（退避封装在 api/onlineMatch）。
 * 不缓存为权威状态——每次轮询即拉即弃（vault S4.9 只读观测方）。
 */
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import ServerSelector from '/@/components/server-selector/ServerSelector.vue'
import { usePageServer } from '../components/usePageServer'
import Card from 'primevue/card'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import SearchForm from '/@/components/search-form/index.vue'
import { formatUnix } from '../../../utils/formatTime.ts'
import { onlineMatchQueueQueryOverviewPost } from '../../../api/onlineMatchQueueQuery'
import type { OnlineMatchOverviewResponse } from '../../../api/onlineMatchQueueQuery/types'
import { startOnlineMatchPolling, type OnlineMatchPollingHandle } from './polling'

const { serverIdOverride, canQueryServer, serverStateMessage, serverSelectionVersion, beginServerRequest } = usePageServer()

const queryParams = ref<Record<string, any>>({})
const overview = ref<OnlineMatchOverviewResponse | null>(null)
const loaded = ref(false)
const loading = ref(false)
let polling: OnlineMatchPollingHandle | null = null

const searchFields = computed(() => [
  { key: 'mode', label: t('message.onlineCenter.matchQueueOverview.search.mode'), type: 'input', placeholder: t('message.onlineCenter.matchQueueOverview.search.pleaseEnterMode') },
  { key: 'region', label: t('message.onlineCenter.matchQueueOverview.search.region'), type: 'input', placeholder: t('message.onlineCenter.matchQueueOverview.search.pleaseEnterRegion') },
])

const queues = computed(() => overview.value?.queues ?? [])

const fetchOverview = async () => {
  const request = beginServerRequest()
  if (!request) return
  loading.value = true
  try {
    const res = await onlineMatchQueueQueryOverviewPost({ ...queryParams.value, serverId: request.serverId })
    if (request.isCurrent()) overview.value = res.data?.data ?? null
  } finally {
    if (request.isCurrent()) {
      loading.value = false
      loaded.value = true
    }
  }
}

const stopPolling = () => {
  polling?.stop()
  polling = null
}

const onQuery = () => {
  stopPolling()
  if (canQueryServer.value) polling = startOnlineMatchPolling(fetchOverview)
}

watch(serverSelectionVersion, () => {
  overview.value = null
  loaded.value = false
  loading.value = false
  onQuery()
})
onMounted(onQuery)
onUnmounted(stopPolling)
</script>
