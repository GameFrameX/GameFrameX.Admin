<template>
  <div class="onlineMatchQueueOverview-container">
    <Card>
      <template #content>
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
import { useRoute } from 'vue-router'
import Card from 'primevue/card'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import SearchForm from '/@/components/search-form/index.vue'
import { formatUnix } from '../../../../utils/formatTime.ts'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import { onlineMatchQueueQueryOverviewPost } from '../../../../api/onlineMatchQueueQuery'
import type { OnlineMatchOverviewResponse } from '../../../../api/onlineMatchQueueQuery/types'
import { startOnlineMatchPolling, type OnlineMatchPollingHandle } from './polling'

const route = useRoute()
const { currentAppId, resolveInitialAppId } = useCurrentApp()

const queryParams = ref<Record<string, any>>({})
const overview = ref<OnlineMatchOverviewResponse | null>(null)
const loaded = ref(false)
const loading = ref(false)
let polling: OnlineMatchPollingHandle | null = null

const searchFields = computed(() => [
  { key: 'serverId', label: t('message.onlineCenter.matchQueueOverview.search.serverId'), type: 'input', placeholder: t('message.onlineCenter.matchQueueOverview.search.pleaseEnterServerId') },
  { key: 'mode', label: t('message.onlineCenter.matchQueueOverview.search.mode'), type: 'input', placeholder: t('message.onlineCenter.matchQueueOverview.search.pleaseEnterMode') },
  { key: 'region', label: t('message.onlineCenter.matchQueueOverview.search.region'), type: 'input', placeholder: t('message.onlineCenter.matchQueueOverview.search.pleaseEnterRegion') },
])

const queues = computed(() => overview.value?.queues ?? [])

const fetchOverview = async () => {
  loading.value = true
  try {
    const { appId, ...rest } = queryParams.value
    const res: any = await onlineMatchQueueQueryOverviewPost({ ...rest, appId: appId ?? currentAppId.value })
    overview.value = res.data?.data ?? null
  } catch (error) {
    // 错误 toast 已由拦截器透出；此处保留上一份快照后 rethrow，
    // 让 startOnlineMatchPolling 感知失败并停轮询（7xxx 镜像段退避的触发条件）
    throw error
  } finally {
    loading.value = false
    loaded.value = true
  }
}

const stopPolling = () => {
  polling?.stop()
  polling = null
}

/** 重启轮询（首查 + 定时）；查询条件或应用变化后调用。ServerId 未定时只停不发（Online 作用域缺失 32200005，issue #4）。 */
const restartPolling = () => {
  stopPolling()
  if (!queryParams.value.serverId) {
    return
  }
  polling = startOnlineMatchPolling(fetchOverview)
}

const onQuery = () => {
  restartPolling()
}

watch(() => route.query.appId, () => {
  currentAppId.value = resolveInitialAppId(route.query)
  queryParams.value.appId = currentAppId.value
  onQuery()
})

onMounted(() => {
  if (!currentAppId.value) {
    currentAppId.value = resolveInitialAppId(route.query)
  }
  queryParams.value.appId = currentAppId.value
  restartPolling()
})

onUnmounted(() => {
  stopPolling()
})
</script>
