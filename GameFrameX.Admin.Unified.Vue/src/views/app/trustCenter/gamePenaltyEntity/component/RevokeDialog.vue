<template>
  <Dialog v-model:visible="visible" modal :header="t('message.gameBusiness.trustSafety.penalty.revoke')" :style="{ width: 'min(480px, calc(100vw - 32px))' }">
    <div class="revoke-body">
      <div class="penalty-meta">
        <div><span class="meta-label">{{ t('message.gameBusiness.trustSafety.penalty.targetId') }}:</span> {{ row?.targetId }}</div>
        <div><span class="meta-label">{{ t('message.gameBusiness.trustSafety.penalty.actionType') }}:</span> {{ actionTypeLabel }}</div>
      </div>
      <FormLabel>{{ t('message.gameBusiness.trustSafety.penalty.pleaseEnterRevokeReason') }}</FormLabel>
      <Textarea v-model="reason" rows="4" maxlength="1024" class="w-full" :placeholder="t('message.gameBusiness.trustSafety.penalty.pleaseEnterRevokeReason')" :invalid="invalid" />
      <small v-if="invalid" class="p-error">{{ t('message.gameBusiness.trustSafety.penalty.pleaseEnterRevokeReason') }}</small>
    </div>
    <template #footer>
      <Button severity="secondary" @click="cancel">{{ t('message.common.button.cancel') }}</Button>
      <Button severity="danger" @click="confirm" :loading="submitting">{{ t('message.gameBusiness.trustSafety.penalty.revoke') }}</Button>
    </template>
  </Dialog>
</template>

<script lang="ts" setup>
import { ref, computed } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import Textarea from 'primevue/textarea'
import FormLabel from '/@/components/form-label/index.vue'
import { tenantGamePenaltyEntityRevokePost } from '../../../../../api/tenantGamePenaltyEntity'
import { showSuccess } from '../../../../../utils/message.ts'
import { PENALTY_ACTION_TYPE_OPTIONS, findEnumOption } from '../constants'

const emit = defineEmits(['reloadTable'])

const visible = ref(false)
const row = ref<any>(null)
const reason = ref('')
const invalid = ref(false)
const submitting = ref(false)

const actionTypeLabel = computed(() => {
  const opt = findEnumOption(PENALTY_ACTION_TYPE_OPTIONS, row.value?.actionType)
  return opt ? t(opt.labelKey) : ''
})

const openDialog = (r: any) => {
  row.value = r
  reason.value = ''
  invalid.value = false
  submitting.value = false
  visible.value = true
}

const cancel = () => {
  visible.value = false
}

const confirm = async () => {
  if (!reason.value || !reason.value.trim()) {
    invalid.value = true
    return
  }
  invalid.value = false
  submitting.value = true
  try {
    await tenantGamePenaltyEntityRevokePost({ id: row.value?.id, reason: reason.value.trim() })
    showSuccess(t('message.gameBusiness.trustSafety.penalty.revokeSuccess'))
    visible.value = false
    emit('reloadTable')
  } finally {
    submitting.value = false
  }
}

defineExpose({ openDialog })
</script>

<style scoped>
.revoke-body {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding-top: 8px;
}
.penalty-meta {
  background: var(--p-content-background);
  border: 1px solid var(--p-content-border-color);
  border-radius: 6px;
  padding: 8px 12px;
  display: flex;
  flex-direction: column;
  gap: 4px;
  font-size: 12px;
}
.meta-label {
  color: var(--p-text-muted-color);
  margin-right: 4px;
}
</style>
