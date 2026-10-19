<template>
  <div class="onlineTransactionDetail-container">
    <Card>
      <template #content>
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
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import SearchForm from '/@/components/search-form/index.vue'
import { formatUnix } from '../../../../utils/formatTime.ts'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import { onlineAssetTransactionQueryDetailGet } from '../../../../api/onlineAssetTransactionQuery'
import type { OnlineAssetTransactionDetailResponse } from '../../../../api/onlineAssetTransactionQuery/types'

const { currentAppId, resolveInitialAppId } = useCurrentApp()
const route = useRoute()

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
  {
    key: 'serverId',
    label: t('message.onlineCenter.transactionDetail.serverId'),
    type: 'input',
    placeholder: t('message.onlineCenter.transactionDetail.pleaseEnterServerId'),
  },
])

const onQuery = async () => {
  const transactionId = (queryParams.value.transactionId ?? '').trim()
  const serverId = (queryParams.value.serverId ?? '').trim()
  if (!transactionId || !serverId) {
    return
  }
  try {
    // 作用域必填：后端按 appId/serverId 定位区服 HttpManageUrl（C356 联调实测）
    const res: any = await onlineAssetTransactionQueryDetailGet({ appId: Number(currentAppId.value), serverId: Number(serverId), transactionId })
    detail.value = res.data?.data ?? null
  } catch {
    detail.value = null
  } finally {
    loaded.value = true
  }
}

watch(() => route.query.appId, () => {
  currentAppId.value = resolveInitialAppId(route.query)
})

onMounted(() => {
  if (!currentAppId.value) {
    currentAppId.value = resolveInitialAppId(route.query)
  }
  if (typeof route.query.serverId === 'string' && route.query.serverId) {
    queryParams.value.serverId = route.query.serverId
  }
})
</script>