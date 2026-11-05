<template>
  <div class="onlineWalletLedger-container">
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
                    authPrefix="onlineWalletLedger" @query="onQuery" />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #header>
        <div class="flex justify-content-between align-items-center w-full">
          <span>{{ t('message.onlineCenter.walletLedger.listTitle') }}</span>
          <div class="flex gap-2">
            <Button class="onlineWalletLedger-grant-btn" v-auth="'onlineWalletLedger:grant'"
                    :label="t('message.onlineCenter.walletLedger.grant')" icon="fa-solid fa-gift"
                    severity="warn" size="small" :disabled="!canQueryServer" @click="grantDialogRef?.openDialog()" />
            <Button class="onlineWalletLedger-revoke-btn" v-auth="'onlineWalletLedger:revoke'"
                    :label="t('message.onlineCenter.walletLedger.revoke')" icon="fa-solid fa-rotate-left"
                    severity="danger" size="small" :disabled="!canQueryServer" @click="revokeDialogRef?.openDialog()" />
          </div>
        </div>
      </template>
      <template #content>
        <OnlineCursorList v-if="canQueryServer" :key="`${serverSelectionVersion}:${listQueryVersion}`" ref="listRef" :request="ledgerRequest" :columns="columns" :pageSize="20">
          <template #body-occurredAt="{ data }">{{ formatUnix(data.occurredAt) }}</template>
        </OnlineCursorList>
      </template>
    </Card>
    <GrantDialog v-if="canQueryServer" :key="'GrantDialog-' + serverSelectionVersion" ref="grantDialogRef" :serverId="effectiveServerId" @done="onQuery" />
    <RevokeDialog v-if="canQueryServer" :key="'RevokeDialog-' + serverSelectionVersion" ref="revokeDialogRef" :serverId="effectiveServerId" @done="onQuery" />
  </div>
</template>

<script lang="ts" setup name="onlineWalletLedger">
/**
 * 玩家资产流水页（C349 / GFX-756）。
 * 分页走 OnlineCursorList（Cursor + HasMore 透传）；Grant/Revoke 经受限命令 API。
 * 数据区不落资产镜像表——查询实时透传 Online（vault C2 S1.6 + C340 §3）。
 */
import { ref, computed, watch, onMounted, nextTick } from 'vue'
import ServerSelector from '/@/components/server-selector/ServerSelector.vue'
import { usePageServer } from '../components/usePageServer'
import Card from 'primevue/card'
import Button from 'primevue/button'
import SearchForm from '/@/components/search-form/index.vue'
import OnlineCursorList from '/@/components/OnlineCursorList/index.vue'
import GrantDialog from '/@/views/gmCenter/walletLedger/component/GrantDialog.vue'
import RevokeDialog from '/@/views/gmCenter/walletLedger/component/RevokeDialog.vue'
import { formatUnix } from '../../../utils/formatTime.ts'
import { onlineAssetLedgerQueryPagePost } from '../../../api/onlineAssetLedgerQuery'
import type { OnlineCursorListParams } from '/@/components/OnlineCursorList/types'

const { serverIdOverride, effectiveServerId, canQueryServer, serverStateMessage, serverSelectionVersion, beginServerRequest } = usePageServer()

const queryParams = ref<Record<string, any>>({})
const listRef = ref()
const listQueryVersion = ref(0)
const grantDialogRef = ref()
const revokeDialogRef = ref()

const searchFields = computed(() => [
  { key: 'playerId', label: t('message.onlineCenter.walletLedger.search.playerId'), type: 'input', placeholder: t('message.onlineCenter.walletLedger.search.pleaseEnterPlayerId') },
  { key: 'currencyOrItemType', label: t('message.onlineCenter.walletLedger.search.currencyOrItemType'), type: 'input', placeholder: t('message.onlineCenter.walletLedger.search.pleaseEnterCurrencyOrItemType'), advanced: true },
  { key: 'timeRange', label: t('message.onlineCenter.walletLedger.search.timeRange'), type: 'dateRange', placeholder: t('message.onlineCenter.walletLedger.search.startTime'), advanced: true },
])

const columns = computed(() => [
  { field: 'transactionId', header: t('message.onlineCenter.walletLedger.columns.transactionId'), width: '180px', sensitive: true },
  { field: 'source', header: t('message.onlineCenter.walletLedger.columns.source'), width: '140px' },
  { field: 'reason', header: t('message.onlineCenter.walletLedger.columns.reason'), width: '160px' },
  { field: 'businessOrderNumber', header: t('message.onlineCenter.walletLedger.columns.businessOrderNumber'), width: '180px' },
  { field: 'operatorId', header: t('message.onlineCenter.walletLedger.columns.operator'), width: '140px', sensitive: true },
  { field: 'beforeValue', header: t('message.onlineCenter.walletLedger.columns.valueBefore'), width: '120px', align: 'right' as const },
  { field: 'afterValue', header: t('message.onlineCenter.walletLedger.columns.valueAfter'), width: '120px', align: 'right' as const },
  { field: 'occurredAt', header: t('message.onlineCenter.walletLedger.columns.occurredAt'), width: '170px', bodyKey: 'occurredAt' },
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

function buildLedgerParams() {
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

const ledgerRequest = async (p: OnlineCursorListParams) => {
  const request = beginServerRequest()
  if (!request) return { items: [], hasMore: false }
  const res: any = await onlineAssetLedgerQueryPagePost({ ...buildLedgerParams(), ...p })
  if (!request.isCurrent()) return { items: [], hasMore: false }
  return res.data?.data ?? { items: [], hasMore: false }
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