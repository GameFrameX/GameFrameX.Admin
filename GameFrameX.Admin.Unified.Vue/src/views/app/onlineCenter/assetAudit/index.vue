<template>
  <div class="onlineAssetAudit-container">
    <Card>
      <template #content>
        <SearchForm v-model="queryParams" :fields="searchFields" :labelWidth="'110px'"
                    authPrefix="onlineAssetAudit" @query="onQuery" />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #header>
        <span>{{ t('message.onlineCenter.assetAudit.listTitle') }}</span>
      </template>
      <template #content>
        <OnlineCursorList ref="listRef" :request="auditRequest" :columns="columns" :pageSize="20">
          <template #body-occurredAt="{ data }">{{ formatUnix(data.occurredAt) }}</template>
        </OnlineCursorList>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="onlineAssetAudit">
/**
 * 资产审计查询页（C349 / GFX-756）。
 * 审计只读：操作者 / 命令类型 / 作用域 / 时间 + 目标玩家，无写路径。
 */
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import Card from 'primevue/card'
import SearchForm from '/@/components/search-form/index.vue'
import OnlineCursorList from '/@/components/OnlineCursorList/index.vue'
import { formatUnix } from '../../../../utils/formatTime.ts'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import { onlineAssetAuditQueryPagePost } from '../../../../api/onlineAssetAuditQuery'
import type { OnlineCursorListParams } from '/@/components/OnlineCursorList/types'

const route = useRoute()
const { currentAppId, resolveInitialAppId } = useCurrentApp()

const queryParams = ref<Record<string, any>>({})
const listRef = ref()

const searchFields = computed(() => [
  { key: 'serverId', label: t('message.onlineCenter.assetAudit.search.serverId'), type: 'input', placeholder: t('message.onlineCenter.assetAudit.search.pleaseEnterServerId') },
  { key: 'operatorId', label: t('message.onlineCenter.assetAudit.search.operatorId'), type: 'input', placeholder: t('message.onlineCenter.assetAudit.search.pleaseEnterOperatorId') },
  { key: 'playerId', label: t('message.onlineCenter.assetAudit.search.playerId'), type: 'input', placeholder: t('message.onlineCenter.assetAudit.search.pleaseEnterPlayerId') },
  { key: 'commandType', label: t('message.onlineCenter.assetAudit.search.commandType'), type: 'input', placeholder: t('message.onlineCenter.assetAudit.search.pleaseEnterCommandType'), advanced: true },
  { key: 'timeRange', label: t('message.onlineCenter.assetAudit.search.timeRange'), type: 'dateRange', placeholder: t('message.onlineCenter.assetAudit.search.startTime'), advanced: true },
])

const columns = computed(() => [
  { field: 'operatorId', header: t('message.onlineCenter.assetAudit.columns.operatorId'), width: '160px', sensitive: true },
  { field: 'commandType', header: t('message.onlineCenter.assetAudit.columns.commandType'), width: '140px' },
  { field: 'playerId', header: t('message.onlineCenter.assetAudit.columns.playerId'), width: '160px', sensitive: true },
  { field: 'transactionId', header: t('message.onlineCenter.assetAudit.columns.transactionId'), width: '180px', sensitive: true },
  { field: 'reason', header: t('message.onlineCenter.assetAudit.columns.reason'), width: '200px' },
  { field: 'occurredAt', header: t('message.onlineCenter.assetAudit.columns.occurredAt'), width: '170px', bodyKey: 'occurredAt' },
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
  const res: any = await onlineAssetAuditQueryPagePost({ ...buildAuditParams(), ...p })
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