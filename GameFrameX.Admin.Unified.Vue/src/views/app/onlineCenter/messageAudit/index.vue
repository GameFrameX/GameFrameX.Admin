<template>
  <div class="onlineMessageAudit-container">
    <Card>
      <template #content>
        <SearchForm v-model="queryParams" :fields="searchFields" :labelWidth="'110px'"
                    authPrefix="messageAudit" @query="onQuery" />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #header>
        <span>{{ t('message.onlineCenter.messageAudit.listTitle') }}</span>
      </template>
      <template #content>
        <OnlineCursorList ref="listRef" :request="auditRequest" :columns="columns" :pageSize="20">
          <template #body-participantPlayerIds="{ data }">
            {{ data.participantPlayerIds?.join(', ') || '-' }}
          </template>
          <template #body-occurredAt="{ data }">{{ formatUnix(data.occurredAt) }}</template>
        </OnlineCursorList>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="onlineMessageAudit">
/**
 * 消息审计查询页（C342 / GFX-758，vault:C7）。
 * 脱敏内容展示——Content 列 sensitive 遮蔽，与 Online 事件脱敏规则一致（Admin 侧兜底脱敏双保险）；
 * 只读，无写路径。查询实时透传 Online，Admin 不落消息镜像。
 */
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import Card from 'primevue/card'
import SearchForm from '/@/components/search-form/index.vue'
import OnlineCursorList from '/@/components/OnlineCursorList/index.vue'
import { formatUnix } from '../../../../utils/formatTime.ts'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import { onlineMessageAuditQueryPagePost } from '../../../../api/onlineMessageAuditQuery'
import type { OnlineCursorListParams } from '/@/components/OnlineCursorList/types'

const route = useRoute()
const { currentAppId, resolveInitialAppId } = useCurrentApp()

const queryParams = ref<Record<string, any>>({})
const listRef = ref()

const searchFields = computed(() => [
  { key: 'serverId', label: t('message.onlineCenter.messageAudit.search.serverId'), type: 'input', placeholder: t('message.onlineCenter.messageAudit.search.pleaseEnterServerId') },
  { key: 'participantPlayerId', label: t('message.onlineCenter.messageAudit.search.participantPlayerId'), type: 'input', placeholder: t('message.onlineCenter.messageAudit.search.pleaseEnterParticipantPlayerId') },
  { key: 'keyword', label: t('message.onlineCenter.messageAudit.search.keyword'), type: 'input', placeholder: t('message.onlineCenter.messageAudit.search.pleaseEnterKeyword'), advanced: true },
  { key: 'timeRange', label: t('message.onlineCenter.messageAudit.search.timeRange'), type: 'dateRange', placeholder: t('message.onlineCenter.messageAudit.search.startTime'), advanced: true },
])

const columns = computed(() => [
  { field: 'messageId', header: t('message.onlineCenter.messageAudit.columns.messageId'), width: '180px', sensitive: true },
  { field: 'participantPlayerIds', header: t('message.onlineCenter.messageAudit.columns.participantPlayerIds'), width: '220px' },
  { field: 'content', header: t('message.onlineCenter.messageAudit.columns.content'), width: '320px', sensitive: true },
  { field: 'occurredAt', header: t('message.onlineCenter.messageAudit.columns.occurredAt'), width: '170px', bodyKey: 'occurredAt' },
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
  const res: any = await onlineMessageAuditQueryPagePost({ ...buildAuditParams(), ...p })
  return res.data?.data ?? { items: [], hasMore: false }
}

// Online 作用域三元组（AppId+ServerId）后端硬校验（vault C2 S1.7）：ServerId 未定时分页请求必被
// 32200005「Online 作用域缺失」拒绝，不空发，列表保持未加载态（issue #4）。
const onQuery = async () => {
  if (!queryParams.value.serverId) {
    return
  }
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
