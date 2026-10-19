<template>
  <Dialog v-model:visible="visible" modal :header="t('message.onlineCenter.walletLedger.grantDialogTitle')"
          :draggable="false" :dismissableMask="false" class="onlineWalletLedger-grant-dialog w-full md:w-40rem">
    <div class="flex flex-column gap-3">
      <div class="font-medium text-color-secondary">
        {{ t('message.onlineCenter.walletLedger.grantHint') }}
      </div>
      <div class="form-grid">
        <label>{{ t('message.onlineCenter.walletLedger.search.serverId') }}</label>
        <InputText v-model="serverId" :placeholder="t('message.onlineCenter.walletLedger.search.pleaseEnterServerId')" />
        <label>{{ t('message.onlineCenter.walletLedger.search.playerId') }}</label>
        <InputText v-model="playerId" :placeholder="t('message.onlineCenter.walletLedger.search.pleaseEnterPlayerId')" />
        <label>{{ t('message.onlineCenter.walletLedger.businessOrderNumber') }}</label>
        <InputText v-model="businessOrderNumber"
                   :placeholder="t('message.onlineCenter.walletLedger.businessOrderNumberPlaceholder')" />
      </div>

      <div class="flex justify-content-between align-items-center">
        <span class="font-medium">{{ t('message.onlineCenter.walletLedger.grantItems') }}</span>
        <Button :label="t('message.onlineCenter.walletLedger.addGrantItem')" icon="fa-solid fa-plus" size="small"
                outlined @click="addItem" />
      </div>
      <div v-for="(item, idx) in items" :key="idx" class="flex gap-2 align-items-center">
        <InputText v-model="item.itemOrCurrencyType"
                   :placeholder="t('message.onlineCenter.walletLedger.search.pleaseEnterCurrencyOrItemType')" class="flex-1" />
        <InputNumber v-model="item.quantity" :placeholder="t('message.onlineCenter.walletLedger.quantity')" class="flex-1" />
        <Button icon="fa-solid fa-xmark" severity="secondary" text :disabled="items.length <= 1"
                @click="removeItem(idx)" />
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
      <Button :label="t('message.onlineCenter.walletLedger.grant')" icon="fa-solid fa-triangle-exclamation"
              severity="warn" :loading="loading" @click="confirm" />
    </template>
  </Dialog>
</template>

<script lang="ts" setup name="onlineWalletLedgerGrantDialog">
/**
 * 受限命令：人工发放弹层（C349 / GFX-756）。
 * 经 Online Grant API 转发（绝不直写资产——vault C2 S1.6 / C340 §3 高危红线）。
 * 理由必填（≤500 字），幂等键 = businessOrderNumber（缺省时服务端生成）。
 */
import { ref } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Textarea from 'primevue/textarea'
import { showSuccess } from '../../../../../utils/message.ts'
import { onlineAssetGrantCommandGrantPost } from '../../../../../api/onlineAssetGrantCommand'

const props = withDefaults(defineProps<{
  appId?: string | number
}>(), { appId: undefined })

const visible = ref(false)
const loading = ref(false)
const reason = ref('')
const reasonError = ref('')
const serverId = ref('')
const playerId = ref('')
const businessOrderNumber = ref('')
const items = ref<Array<{ itemOrCurrencyType: string; quantity: number | string }>>([
  { itemOrCurrencyType: '', quantity: '' },
])
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
  businessOrderNumber.value = ''
  items.value = [{ itemOrCurrencyType: '', quantity: '' }]
}

const addItem = () => {
  items.value.push({ itemOrCurrencyType: '', quantity: '' })
}

const removeItem = (idx: number) => {
  if (items.value.length <= 1) {
    return
  }
  items.value.splice(idx, 1)
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
  if (!serverId.value.trim() || !playerId.value.trim()) {
    return false
  }
  for (const item of items.value) {
    if (!item.itemOrCurrencyType.trim()) {
      return false
    }
    if (item.quantity === '' || item.quantity === null || item.quantity === undefined) {
      return false
    }
  }
  return true
}

const confirm = async () => {
  if (!validate()) {
    return
  }
  loading.value = true
  try {
    await onlineAssetGrantCommandGrantPost({
      appId: props.appId as any,
      serverId: serverId.value.trim(),
      playerId: playerId.value.trim(),
      grantItems: items.value.map((i) => ({
        itemOrCurrencyType: i.itemOrCurrencyType.trim(),
        quantity: typeof i.quantity === 'string' ? Number(i.quantity) : i.quantity,
      })),
      reason: reason.value.trim(),
      businessOrderNumber: businessOrderNumber.value.trim() || undefined,
    })
    showSuccess(t('message.onlineCenter.walletLedger.grantSuccess'))
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