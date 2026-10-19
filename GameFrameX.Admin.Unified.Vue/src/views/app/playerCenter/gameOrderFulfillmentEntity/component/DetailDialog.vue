<template>
  <Dialog v-model:visible="visible" modal :header="t('message.gameBusiness.orderFulfillment.detailTitle')" :draggable="false" class="w-full md:w-40rem">
    <div v-block-loading="loading">
      <div v-if="row" class="grid grid-cols-2" style="gap: 0.5rem 1.5rem;">
        <DetailItem :label="t('message.gameBusiness.orderFulfillment.app')">{{ appName(row.appId) }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.orderFulfillment.type')">
          <Tag :value="typeDesc(row.type)" :severity="typeSeverity(row.type)" />
        </DetailItem>
        <DetailItem :label="t('message.gameBusiness.orderFulfillment.status')">
          <Tag :value="statusDesc(row.status)" :severity="statusSeverity(row.status)" />
        </DetailItem>
        <DetailItem :label="t('message.gameBusiness.orderFulfillment.orderId')">{{ row.orderId }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.orderFulfillment.payChannel')">{{ row.payChannel }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.orderFulfillment.providerTransactionId')">{{ row.providerTransactionId || '-' }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.orderFulfillment.sku')">{{ row.sku }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.orderFulfillment.amountCents')">{{ row.amount }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.orderFulfillment.roleId')">{{ row.roleId }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.orderFulfillment.areaId')">{{ row.areaId }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.orderFulfillment.accountId')">{{ row.accountId || '-' }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.orderFulfillment.operator')">{{ operatorDesc(row.operatorId) }}</DetailItem>
        <div style="grid-column: 1 / -1">
          <div class="text-color-secondary text-sm mb-1">{{ t('message.gameBusiness.orderFulfillment.idempotencyKey') }}</div>
          <code class="text-sm break-all">{{ row.idempotencyKey }}</code>
        </div>
        <div v-if="row.failureReason" style="grid-column: 1 / -1">
          <div class="text-color-secondary text-sm mb-1">{{ t('message.gameBusiness.orderFulfillment.failureReason') }}</div>
          <div class="text-red-500 break-all">{{ row.failureReason }}</div>
        </div>
        <div style="grid-column: 1 / -1">
          <div class="text-color-secondary text-sm mb-1">{{ t('message.gameBusiness.orderFulfillment.itemsJson') }}</div>
          <div v-if="parsedItems.length" class="flex flex-wrap gap-1">
            <Tag v-for="(item, index) in parsedItems" :key="index" severity="info">{{ item.id }} x{{ item.count }}</Tag>
          </div>
          <span v-else class="text-color-secondary">-</span>
        </div>
        <div style="grid-column: 1 / -1">
          <div class="text-color-secondary text-sm mb-1">{{ t('common.label.createTime') }}</div>
          <div>{{ row.createdTime }}</div>
        </div>
      </div>
    </div>
    <template #footer>
      <Button :label="t('message.common.button.close')" text @click="visible = false" />
    </template>
  </Dialog>
</template>

<script lang="ts" setup name="gameOrderFulfillmentDetailDialog">
import { ref, computed, h, defineComponent } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import { t } from '../../../../../i18n/index.ts'
import { useCurrentApp } from '../../../../../composables/useCurrentApp.ts'
import { tenantGameOrderFulfillmentEntityDetailGet } from '../../../../../api/tenantGameOrderFulfillmentEntity'
import {parseRewardItems} from '../../../../../components/reward-items-editor/rewardItems.ts'
import {
  FULFILLMENT_TYPE_OPTIONS,
  FULFILLMENT_STATUS_OPTIONS,
} from '../constants'

// 内联只读字段渲染（避免依赖外部 Field 组件）
// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const DetailItem = defineComponent({
  name: 'FulfillmentDetailItem',
  props: { label: { type: String, default: '' } },
  setup(props, { slots }) {
    return () =>
      h('div', { class: 'min-w-0' }, [
        h('div', { class: 'text-color-secondary text-sm mb-1' }, props.label),
        h('div', { class: 'break-all' }, slots.default?.()),
      ])
  },
})

const { appName } = useCurrentApp()

const visible = ref(false)
const loading = ref(false)
const row = ref<any>(null)

const parsedItems = computed(() => parseRewardItems(row.value?.itemsJson))

const typeDesc = (v: number) => {
  const item = FULFILLMENT_TYPE_OPTIONS.find((i) => i.value === v)
  return item ? t(`message.gameBusiness.orderFulfillment.${item.i18nKey}`) : v
}
const statusDesc = (v: number) => {
  const item = FULFILLMENT_STATUS_OPTIONS.find((i) => i.value === v)
  return item ? t(`message.gameBusiness.orderFulfillment.${item.i18nKey}`) : v
}
const typeSeverity = (v: number) => {
  const map: Record<number, any> = { 1: 'info', 2: 'warn', 3: 'success' }
  return map[v] ?? undefined
}
const statusSeverity = (v: number) => {
  const map: Record<number, any> = { 1: 'warn', 2: 'success', 3: 'danger', 4: 'secondary' }
  return map[v] ?? undefined
}
const operatorDesc = (v: number) => (v === 0 ? t('message.gameBusiness.orderFulfillment.operatorSystem') : v)

const openDialog = async (target: any) => {
  visible.value = true
  row.value = target
  if (target?.id) {
    loading.value = true
    try {
      const res = await tenantGameOrderFulfillmentEntityDetailGet({ id: target.id })
      if (res.data?.data) {
        row.value = res.data.data
      }
    } catch {
      // 保留传入的行数据
    } finally {
      loading.value = false
    }
  }
}

defineExpose({ openDialog })
</script>
