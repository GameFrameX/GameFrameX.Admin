<template>
  <div class="onlineWalletLedger-container">
    <Card>
      <template #content>
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
                    severity="warn" size="small" @click="grantDialogRef?.openDialog()" />
            <Button class="onlineWalletLedger-revoke-btn" v-auth="'onlineWalletLedger:revoke'"
                    :label="t('message.onlineCenter.walletLedger.revoke')" icon="fa-solid fa-rotate-left"
                    severity="danger" size="small" @click="revokeDialogRef?.openDialog()" />
          </div>
        </div>
      </template>
      <template #content>
        <OnlineCursorList ref="listRef" :request="ledgerRequest" :columns="columns" :pageSize="20">
          <template #body-occurredAt="{ data }">{{ formatUnix(data.occurredAt) }}</template>
        </OnlineCursorList>
      </template>
    </Card>
    <GrantDialog ref="grantDialogRef" :appId="currentAppId" @done="onQuery" />
    <RevokeDialog ref="revokeDialogRef" :appId="currentAppId" @done="onQuery" />
  </div>
</template>

<script lang="ts" setup name="onlineWalletLedger">
/**
 * 玩家资产流水页（C349 / GFX-756）。
 * 分页走 OnlineCursorList（Cursor + HasMore 透传）；Grant/Revoke 经受限命令 API。
 * 数据区不落资产镜像表——查询实时透传 Online（vault C2 S1.6 + C340 §3）。
 */
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import Card from 'primevue/card'
import Button from 'primevue/button'
import SearchForm from '/@/components/search-form/index.vue'
import OnlineCursorList from '/@/components/OnlineCursorList/index.vue'
import GrantDialog from '/@/views/app/onlineCenter/walletLedger/component/GrantDialog.vue'
import RevokeDialog from '/@/views/app/onlineCenter/walletLedger/component/RevokeDialog.vue'
import { formatUnix } from '../../../../utils/formatTime.ts'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import { onlineAssetLedgerQueryPagePost } from '../../../../api/onlineAssetLedgerQuery'
import type { OnlineCursorListParams } from '/@/components/OnlineCursorList/types'

const route = useRoute()
const { currentAppId, resolveInitialAppId } = useCurrentApp()

const queryParams = ref<Record<string, any>>({})
const listRef = ref()
const grantDialogRef = ref()
const revokeDialogRef = ref()

const searchFields = computed(() => [
  { key: 'serverId', label: t('message.onlineCenter.walletLedger.search.serverId'), type: 'input', placeholder: t('message.onlineCenter.walletLedger.search.pleaseEnterServerId') },
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

const ledgerRequest = async (p: OnlineCursorListParams) => {
  const res: any = await onlineAssetLedgerQueryPagePost({ ...buildLedgerParams(), ...p })
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