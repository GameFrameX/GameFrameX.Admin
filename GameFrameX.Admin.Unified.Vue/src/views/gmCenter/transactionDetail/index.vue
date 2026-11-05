<template>
  <div class="onlineTransactionDetail-container">
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
                    :queryAuth="'onlineTransactionDetail:detail'" @query="onQuery" />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #header>
        <span>{{ t('message.onlineCenter.transactionDetail.detailTitle') }}</span>
      </template>
      <template #content>
        <div v-if="detail" class="flex flex-column gap-3">
          <div class="surface-100 p-3 border-round text-sm detail-summary">
            <div><span class="text-color-secondary">{{ t('message.onlineCenter.transactionDetail.transactionId') }}：</span>{{ detail.transactionId }}</div>
            <div><span class="text-color-secondary">{{ t('message.onlineCenter.transactionDetail.status') }}：</span>{{ detail.status || '-' }}</div>
            <div><span class="text-color-secondary">{{ t('message.onlineCenter.transactionDetail.source') }}：</span>{{ detail.source || '-' }}</div>
            <div><span class="text-color-secondary">{{ t('message.onlineCenter.transactionDetail.reason') }}：</span>{{ detail.reason || '-' }}</div>
            <div><span class="text-color-secondary">{{ t('message.onlineCenter.transactionDetail.businessOrderNumber') }}：</span>{{ detail.businessOrderNumber || '-' }}</div>
            <div><span class="text-color-secondary">{{ t('message.onlineCenter.transactionDetail.operator') }}：</span>{{ detail.operatorId || '-' }}</div>
            <div><span class="text-color-secondary">{{ t('message.onlineCenter.transactionDetail.playerId') }}：</span>{{ detail.playerId || '-' }}</div>
            <div><span class="text-color-secondary">{{ t('message.onlineCenter.transactionDetail.idempotencyKey') }}：</span><code class="text-sm break-all">{{ detail.idempotencyKey }}</code></div>
            <div><span class="text-color-secondary">{{ t('message.onlineCenter.transactionDetail.occurredAt') }}：</span>{{ formatUnix(detail.occurredAt) }}</div>
          </div>
          <DataTable :value="detail.grantItems" showGridlines stripedRows scrollable>
            <Column field="itemOrCurrencyType" :header="t('message.onlineCenter.transactionDetail.currencyOrItemType')" />
            <Column field="quantity" :header="t('message.onlineCenter.transactionDetail.quantity')" />
            <Column field="beforeValue" :header="t('message.onlineCenter.transactionDetail.valueBefore')" />
            <Column field="afterValue" :header="t('message.onlineCenter.transactionDetail.valueAfter')" />
          </DataTable>
        </div>
        <div v-else class="text-center text-color-secondary p-4">
          {{ loaded ? t('message.onlineCenter.transactionDetail.notFound') : t('message.onlineCenter.transactionDetail.notLoaded') }}
        </div>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="onlineTransactionDetail">
/**
 * 交易详情页（C349 / GFX-756）。
 * 单对象详情 + GrantItems 明细表，无游标面（vault detail 端点返回单对象）。
 */
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import ServerSelector from '/@/components/server-selector/ServerSelector.vue'
import { usePageServer } from '../components/usePageServer'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import SearchForm from '/@/components/search-form/index.vue'
import { formatUnix } from '../../../utils/formatTime.ts'
import { onlineAssetTransactionQueryDetailGet } from '../../../api/onlineAssetTransactionQuery'
import type { OnlineAssetTransactionDetailResponse } from '../../../api/onlineAssetTransactionQuery/types'

const route = useRoute()

const { serverIdOverride, canQueryServer, serverStateMessage, serverSelectionVersion, beginServerRequest } = usePageServer()

const queryParams = ref<Record<string, any>>({})
const detail = ref<OnlineAssetTransactionDetailResponse | null>(null)
const loaded = ref(false)

const searchFields = computed(() => [
  {
    key: 'transactionId',
    label: t('message.onlineCenter.transactionDetail.transactionId'),
    type: 'input',
    placeholder: t('message.onlineCenter.transactionDetail.pleaseEnterTransactionId'),
  },
])

const onQuery = async () => {
  const transactionId = (queryParams.value.transactionId ?? '').trim()
  const request = beginServerRequest()
  if (!transactionId || !request) return
  try {
    // 后端 GET DTO 使用 long；生成客户端要求 number，调用处转换区服 Id。
    const res = await onlineAssetTransactionQueryDetailGet({ serverId: Number(request.serverId), transactionId })
    if (request.isCurrent()) detail.value = res.data?.data ?? null
  } catch {
    if (request.isCurrent()) detail.value = null
  } finally {
    if (request.isCurrent()) loaded.value = true
  }
}

watch(serverSelectionVersion, () => {
  detail.value = null
  loaded.value = false
  return onQuery()
})
onMounted(() => {
  if (typeof route.query.transactionId === 'string') queryParams.value.transactionId = route.query.transactionId
  return onQuery()
})
</script>
