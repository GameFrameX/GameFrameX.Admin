<template>
  <Dialog v-model:visible="visible" modal :header="title" :draggable="false" :dismissableMask="false" class="w-full md:w-30rem">
    <div v-if="row" class="flex flex-column gap-2">
      <div class="surface-100 p-2 border-round text-sm">
        <div><span class="text-color-secondary">{{ t('message.gameBusiness.orderFulfillment.app') }}：</span>{{ appName(row.appId) }}</div>
        <div><span class="text-color-secondary">{{ t('message.gameBusiness.orderFulfillment.orderId') }}：</span>{{ row.orderId }}</div>
        <div><span class="text-color-secondary">{{ t('message.gameBusiness.orderFulfillment.amountCents') }}：</span>{{ row.amount }}</div>
        <div><span class="text-color-secondary">{{ t('message.gameBusiness.orderFulfillment.type') }}：</span>{{ typeDesc(row.type) }}</div>
        <div><span class="text-color-secondary">{{ t('message.gameBusiness.orderFulfillment.status') }}：</span>{{ statusDesc(row.status) }}</div>
        <div class="white-space-nowrap overflow-hidden text-overflow-ellipsis"><span class="text-color-secondary">{{ t('message.gameBusiness.orderFulfillment.idempotencyKey') }}：</span>{{ row.idempotencyKey }}</div>
      </div>
      <div class="font-medium text-color-secondary">{{ t('message.gameBusiness.orderFulfillment.reasonLabel') }}</div>
      <Textarea v-model="reason" :rows="3" :autoResize="true" class="w-full"
                :placeholder="t('message.gameBusiness.orderFulfillment.reasonPlaceholder')" />
      <small v-if="reasonError" class="text-red-500">{{ reasonError }}</small>
      <div class="text-sm text-color-secondary">{{ t('message.gameBusiness.orderFulfillment.actionWarning') }}</div>
    </div>
    <template #footer>
      <Button :label="t('message.common.button.cancel')" text @click="close" />
      <Button :label="confirmLabel" icon="fa-solid fa-triangle-exclamation" severity="danger" :loading="loading" @click="confirm" />
    </template>
  </Dialog>
</template>

<script lang="ts" setup name="gameOrderFulfillmentActionDialog">
import { ref, computed } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import Textarea from 'primevue/textarea'
import { t } from '../../../../../i18n/index.ts'
import { showSuccess } from '../../../../../utils/message.ts'
import { tenantGameOrderFulfillmentEntityRefundPost, tenantGameOrderFulfillmentEntityCompensatePost, tenantGameOrderFulfillmentEntityResendPost } from '../../../../../api/tenantGameOrderFulfillmentEntity'
import { useCurrentApp } from '../../../../../composables/useCurrentApp.ts'
import { FULFILLMENT_TYPE_OPTIONS, FULFILLMENT_STATUS_OPTIONS } from '../constants'

const { appName } = useCurrentApp()

type ActionType = 'refund' | 'compensate' | 'resend'

const visible = ref(false)
const loading = ref(false)
const row = ref<any>(null)
const action = ref<ActionType>('refund')
const reason = ref('')
const reasonError = ref('')

const emit = defineEmits(['done'])

const title = computed(() => {
  const map: Record<ActionType, string> = {
    refund: t('message.gameBusiness.orderFulfillment.refundTitle'),
    compensate: t('message.gameBusiness.orderFulfillment.compensateTitle'),
    resend: t('message.gameBusiness.orderFulfillment.resendTitle'),
  }
  return map[action.value]
})

const confirmLabel = computed(() => {
  const map: Record<ActionType, string> = {
    refund: t('message.gameBusiness.orderFulfillment.refund'),
    compensate: t('message.gameBusiness.orderFulfillment.compensate'),
    resend: t('message.gameBusiness.orderFulfillment.resend'),
  }
  return map[action.value]
})

const typeDesc = (v: number) => {
  const item = FULFILLMENT_TYPE_OPTIONS.find((i) => i.value === v)
  return item ? t(`message.gameBusiness.orderFulfillment.${item.i18nKey}`) : v
}
const statusDesc = (v: number) => {
  const item = FULFILLMENT_STATUS_OPTIONS.find((i) => i.value === v)
  return item ? t(`message.gameBusiness.orderFulfillment.${item.i18nKey}`) : v
}

const apiMap = {
  refund: tenantGameOrderFulfillmentEntityRefundPost,
  compensate: tenantGameOrderFulfillmentEntityCompensatePost,
  resend: tenantGameOrderFulfillmentEntityResendPost,
}

const close = () => {
  visible.value = false
  row.value = null
  reason.value = ''
  reasonError.value = ''
}

const confirm = async () => {
  const trimmed = reason.value.trim()
  if (!trimmed) {
    reasonError.value = t('message.gameBusiness.orderFulfillment.reasonRequired')
    return
  }
  if (trimmed.length > 500) {
    reasonError.value = t('message.gameBusiness.orderFulfillment.reasonTooLong')
    return
  }
  loading.value = true
  try {
    await apiMap[action.value]({ id: row.value.id, reason: trimmed })
    showSuccess(t('message.common.message.success'))
    emit('done')
    close()
  } catch {
    // 全局拦截器已弹错误提示
  } finally {
    loading.value = false
  }
}

const openDialog = (act: ActionType, targetRow: any) => {
  action.value = act
  row.value = targetRow
  reason.value = ''
  reasonError.value = ''
  visible.value = true
}

defineExpose({ openDialog })
</script>
