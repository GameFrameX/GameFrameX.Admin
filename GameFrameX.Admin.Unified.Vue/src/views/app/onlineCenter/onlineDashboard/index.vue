<template>
  <div class="onlineDashboard-container">
    <Card>
      <template #content>
        <SearchForm v-model="queryParams" :fields="searchFields" :labelWidth="'110px'"
                    queryAuth="onlineDashboard:snapshot" @query="onQuery" />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #header>
        <div class="flex align-items-center justify-content-between">
          <span>{{ t('message.onlineCenter.dashboard.snapshotTitle') }}</span>
          <small v-if="overview?.serverTime != null" class="onlineDashboard-server-time text-color-secondary">
            <i class="fa-solid fa-clock mr-1"></i>
            {{ t('message.onlineCenter.dashboard.serverTime') }}：{{ formatServerTime(overview.serverTime) }}
          </small>
        </div>
      </template>
      <template #content>
        <div v-block-loading="loading" class="grid">
          <div class="col-12 md:col-6 lg:col-3">
            <div class="onlineDashboard-metric-card p-3 border-round surface-border border-1">
              <div class="text-color-secondary text-sm mb-1">
                <i class="fa-solid fa-user-group mr-1"></i>{{ t('message.onlineCenter.dashboard.metrics.onlinePlayerCount') }}
              </div>
              <div class="onlineDashboard-metric-value text-3xl font-bold" data-metric="onlinePlayerCount">
                {{ overview?.onlinePlayerCount ?? '-' }}
              </div>
            </div>
          </div>
          <div class="col-12 md:col-6 lg:col-3">
            <div class="onlineDashboard-metric-card p-3 border-round surface-border border-1">
              <div class="text-color-secondary text-sm mb-1">
                <i class="fa-solid fa-plug-circle-check mr-1"></i>{{ t('message.onlineCenter.dashboard.metrics.sessionCount') }}
              </div>
              <div class="onlineDashboard-metric-value text-3xl font-bold" data-metric="sessionCount">
                {{ overview?.sessionCount ?? '-' }}
              </div>
            </div>
          </div>
          <div class="col-12 md:col-6 lg:col-3">
            <div class="onlineDashboard-metric-card p-3 border-round surface-border border-1">
              <div class="text-color-secondary text-sm mb-1">
                <i class="fa-solid fa-table-cells-large mr-1"></i>{{ t('message.onlineCenter.dashboard.metrics.ongoingMatchCount') }}
              </div>
              <div class="onlineDashboard-metric-value text-3xl font-bold" data-metric="matchCount">
                {{ overview?.matchCount ?? '-' }}
              </div>
            </div>
          </div>
          <div class="col-12 md:col-6 lg:col-3">
            <div class="onlineDashboard-metric-card p-3 border-round surface-border border-1">
              <div class="text-color-secondary text-sm mb-1">
                <i class="fa-solid fa-rotate mr-1"></i>{{ t('message.onlineCenter.dashboard.metrics.reconnectRate') }}
              </div>
              <div class="onlineDashboard-metric-value text-3xl font-bold" data-metric="reconnectRate">
                {{ formatRate(overview?.reconnectRate) }}
              </div>
            </div>
          </div>
        </div>
        <div class="mt-3">
          <div class="font-medium mb-2">{{ t('message.onlineCenter.dashboard.queueSummary') }}</div>
          <DataTable :value="overview?.queueSummaries ?? []" showGridlines stripedRows>
            <Column field="mode" :header="t('message.onlineCenter.dashboard.queues.mode')"></Column>
            <Column field="region" :header="t('message.onlineCenter.dashboard.queues.region')"></Column>
            <Column field="queueDepth" :header="t('message.onlineCenter.dashboard.queues.queueDepth')">
              <template #body="{ data }">{{ data.queueDepth }}</template>
            </Column>
            <Column field="averageWaitSeconds" :header="t('message.onlineCenter.dashboard.queues.averageWaitSeconds')">
              <template #body="{ data }">{{ data.averageWaitSeconds ?? '-' }}</template>
            </Column>
            <Column field="throughputPerMinute" :header="t('message.onlineCenter.dashboard.queues.throughputPerMinute')">
              <template #body="{ data }">{{ data.throughputPerMinute ?? '-' }}</template>
            </Column>
            <template #empty>
              <div class="text-center text-color-secondary p-4">{{ t('message.onlineCenter.dashboard.queues.empty') }}</div>
            </template>
          </DataTable>
        </div>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="onlineDashboard">
/**
 * 在线总览页（C344 / GFX-761，dashboard）。
 * 数据源唯一 = Online 玩家侧查询 API 聚合快照（X6 口径）：
 * OnlineUserHub / TenantOnlineUserService / GameHomeOnlineCounter 数据一律不计入。
 */
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import SearchForm from '/@/components/search-form/index.vue'
import { formatUnix } from '../../../../utils/formatTime.ts'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import { onlineOverviewQuerySnapshotPost } from '../../../../api/onlineOverviewQuery'
import type { OnlineOverviewResponse } from '../../../../api/onlineOverviewQuery/types'

const route = useRoute()
const { currentAppId, resolveInitialAppId } = useCurrentApp()

const queryParams = ref<Record<string, any>>({})
const loading = ref(false)
const overview = ref<OnlineOverviewResponse | null>(null)

const searchFields = computed(() => [
  { key: 'serverId', label: t('message.onlineCenter.dashboard.search.serverId'), type: 'input', placeholder: t('message.onlineCenter.dashboard.search.pleaseEnterServerId') },
])

const formatServerTime = (value: number | string) => (typeof value === 'number' ? formatUnix(value) : String(value))

// 重连率：0~1 小数 → 百分比展示（保留两位）
const formatRate = (value: number | undefined | null) => {
  if (value === undefined || value === null) {
    return '-'
  }
  return `${(value * 100).toFixed(2)}%`
}

// Online 作用域三元组（AppId+ServerId）后端硬校验（vault C2 S1.7）：ServerId 未定时快照请求必被
// 32200005「Online 作用域缺失」拒绝，不空发（issue #4）。
const onQuery = async () => {
  if (!currentAppId.value || !queryParams.value.serverId) {
    return
  }
  loading.value = true
  try {
    const res: any = await onlineOverviewQuerySnapshotPost({
      appId: currentAppId.value,
      serverId: queryParams.value.serverId || undefined,
    })
    overview.value = res.data?.data ?? null
  } finally {
    loading.value = false
  }
}

watch(() => route.query.appId, async () => {
  currentAppId.value = resolveInitialAppId(route.query)
  await onQuery()
})

onMounted(async () => {
  if (!currentAppId.value) {
    currentAppId.value = resolveInitialAppId(route.query)
  }
  await onQuery()
})
</script>
