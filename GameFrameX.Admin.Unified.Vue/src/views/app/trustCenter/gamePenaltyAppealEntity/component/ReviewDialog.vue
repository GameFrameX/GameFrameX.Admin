<template>
  <Dialog v-model:visible="visible" modal :header="t('message.gameBusiness.trustSafety.appeal.review')" :style="{ width: 'min(480px, calc(100vw - 32px))' }">
    <div class="review-body">
      <div class="appeal-meta">
        <div><span class="meta-label">{{ t('message.gameBusiness.trustSafety.appeal.penaltyId') }}:</span> {{ row?.penaltyId }}</div>
        <div><span class="meta-label">{{ t('message.gameBusiness.trustSafety.appeal.appellantId') }}:</span> {{ row?.appellantId }}</div>
      </div>
      <FormLabel>{{ t('message.gameBusiness.trustSafety.appeal.reviewResult') }}</FormLabel>
      <Select v-model="approve" :options="reviewOptions" optionLabel="label" optionValue="value" class="w-full" />
      <FormLabel>{{ t('message.gameBusiness.trustSafety.appeal.reviewRemark') }}</FormLabel>
      <Textarea v-model="remark" rows="4" maxlength="1024" class="w-full" :placeholder="t('message.gameBusiness.trustSafety.appeal.reviewRemarkPlaceholder')" />
    </div>
    <template #footer>
      <Button severity="secondary" @click="cancel">{{ t('message.common.button.cancel') }}</Button>
      <Button @click="confirm" :loading="submitting">{{ t('message.common.button.confirm') }}</Button>
    </template>
  </Dialog>
</template>

<script lang="ts" setup>
import { ref } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import Select from 'primevue/select'
import Textarea from 'primevue/textarea'
import FormLabel from '/@/components/form-label/index.vue'
import { tenantGamePenaltyAppealEntityReviewPost } from '../../../../../api/tenantGamePenaltyAppealEntity'
import { showSuccess } from '../../../../../utils/message.ts'

const emit = defineEmits(['reloadTable'])

const visible = ref(false)
const row = ref<any>(null)
const approve = ref<boolean>(true)
const remark = ref('')
const submitting = ref(false)

const reviewOptions = [
  { label: t('message.gameBusiness.trustSafety.appeal.approve'), value: true },
  { label: t('message.gameBusiness.trustSafety.appeal.reject'), value: false },
]

const openDialog = (r: any) => {
  row.value = r
  approve.value = true
  remark.value = ''
  submitting.value = false
  visible.value = true
}

const cancel = () => {
  visible.value = false
}

const confirm = async () => {
  submitting.value = true
  try {
    await tenantGamePenaltyAppealEntityReviewPost({
      id: row.value?.id,
      approve: approve.value,
      remark: remark.value?.trim() || undefined,
    })
    showSuccess(approve.value
      ? t('message.gameBusiness.trustSafety.appeal.approveSuccess')
      : t('message.gameBusiness.trustSafety.appeal.rejectSuccess'))
    visible.value = false
    emit('reloadTable')
  } finally {
    submitting.value = false
  }
}

defineExpose({ openDialog })
</script>

<style scoped>
.review-body {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding-top: 8px;
}
.appeal-meta {
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
