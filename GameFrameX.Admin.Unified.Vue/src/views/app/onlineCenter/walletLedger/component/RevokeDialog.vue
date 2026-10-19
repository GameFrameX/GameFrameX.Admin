<template>
  <Dialog v-model:visible="visible" modal :header="t('message.onlineCenter.walletLedger.revokeDialogTitle')"
          :draggable="false" :dismissableMask="false" class="onlineWalletLedger-revoke-dialog w-full md:w-40rem">
    <div class="flex flex-column gap-3">
      <div class="font-medium text-color-secondary">
        {{ t('message.onlineCenter.walletLedger.revokeHint') }}
      </div>
      <div class="form-grid">
        <label>{{ t('message.onlineCenter.walletLedger.search.serverId') }}</label>
        <InputText v-model="serverId" :placeholder="t('message.onlineCenter.walletLedger.search.pleaseEnterServerId')" />
        <label>{{ t('message.onlineCenter.walletLedger.search.playerId') }}</label>
        <InputText v-model="playerId" :placeholder="t('message.onlineCenter.walletLedger.search.pleaseEnterPlayerId')" />
        <label>{{ t('message.onlineCenter.walletLedger.transactionIdLabel') }}</label>
        <InputText v-model="transactionId"
                   :placeholder="t('message.onlineCenter.walletLedger.pleaseEnterTransactionId')" />
        <label>{{ t('message.onlineCenter.walletLedger.businessOrderNumber') }}</label>
        <InputText v-model="businessOrderNumber"
                   :placeholder="t('message.onlineCenter.walletLedger.businessOrderNumberPlaceholder')" />
      </div>
      <div>
        <label class="font-medium text-color-secondary">
          {{ t('message.onlineCenter.walletLedger.reasonLabel') }}
        </label>
        <Textarea v-model="reason" :rows="3" :autoResize="true" class="w-full mt-1"
                  :placeholder="t('message.onlineCenter.walletLedger.reasonPlaceholder')" />
        <small v-if="reasonError" class="text-red-500">{{ reasonError }}</small>
      </div>
    </div>
    <template #footer>
      <Button :label="t('message.common.button.cancel')" text @click="close" />
      <Button :label="t('message.onlineCenter.walletLedger.revoke')" icon="fa-solid fa-triangle-exclamation"
              severity="danger" :loading="loading" @click="confirm" />
    </template>
  </Dialog>
</template>

<script lang="ts" setup name="onlineWalletLedgerRevokeDialog">
/**
 * 受限命令：撤销发放弹层（C349 / GFX-756）。
 * 按 transactionId 定位原交易（C349 P1-1 解释：UI 不再收 GrantItems）；
 * 理由必填（≤500 字）进审计。
 */
import { ref } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import { showSuccess } from '../../../../../utils/message.ts'
import { onlineAssetGrantCommandRevokePost } from '../../../../../api/onlineAssetGrantCommand'

const props = withDefaults(defineProps<{
  appId?: string | number
}>(), { appId: undefined })

const visible = ref(false)
const loading = ref(false)
const reason = ref('')
const reasonError = ref('')
const serverId = ref('')
const playerId = ref('')
const transactionId = ref('')
const businessOrderNumber = ref('')
const emit = defineEmits(['done'])

const close = () => {
  visible.value = false
  reset()
}

const reset = () => {
  reason.value = ''
  reasonError.value = ''
  serverId.value = ''
  playerId.value = ''
  transactionId.value = ''
  businessOrderNumber.value = ''
}

const validate = () => {
  reasonError.value = ''
  const trimmed = reason.value.trim()
  if (!trimmed) {
    reasonError.value = t('message.onlineCenter.walletLedger.reasonRequired')
    return false
  }
  if (trimmed.length > 500) {
    reasonError.value = t('message.onlineCenter.walletLedger.reasonTooLong')
    return false
  }
  if (!serverId.value.trim() || !playerId.value.trim() || !transactionId.value.trim()) {
    return false
  }
  return true
}

const confirm = async () => {
  if (!validate()) {
    return
  }
  loading.value = true
  try {
    await onlineAssetGrantCommandRevokePost({
      appId: props.appId as any,
      serverId: serverId.value.trim(),
      playerId: playerId.value.trim(),
      transactionId: transactionId.value.trim(),
      reason: reason.value.trim(),
      businessOrderNumber: businessOrderNumber.value.trim() || undefined,
    })
    showSuccess(t('message.onlineCenter.walletLedger.revokeSuccess'))
    emit('done')
    close()
  } catch {
    // 全局拦截器已弹错误提示
  } finally {
    loading.value = false
  }
}

defineExpose({
  openDialog: () => {
    reset()
    visible.value = true
  },
})
</script>