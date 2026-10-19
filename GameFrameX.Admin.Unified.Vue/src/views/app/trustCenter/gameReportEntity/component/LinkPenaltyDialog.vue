<template>
  <Dialog v-model:visible="visible" modal :header="t('message.gameBusiness.trustSafety.report.linkPenalty')" :style="{ width: 'min(480px, calc(100vw - 32px))' }">
    <div class="link-penalty-body">
      <div class="report-meta">
        <div><span class="meta-label">{{ t('message.gameBusiness.trustSafety.report.targetId') }}:</span> {{ row?.targetId }}</div>
        <div><span class="meta-label">{{ t('message.gameBusiness.trustSafety.report.reportType') }}:</span> {{ reportTypeLabel }}</div>
      </div>
      <FormLabel>{{ t('message.gameBusiness.trustSafety.report.pleaseEnterPenaltyId') }}</FormLabel>
      <InputNumber v-model="penaltyId" :min="1" :useGrouping="false" class="w-full" :placeholder="t('message.gameBusiness.trustSafety.report.pleaseEnterPenaltyId')" :invalid="invalid" />
      <small v-if="invalid" class="p-error">{{ t('message.gameBusiness.trustSafety.report.pleaseEnterPenaltyId') }}</small>
    </div>
    <template #footer>
      <Button severity="secondary" @click="cancel">{{ t('message.common.button.cancel') }}</Button>
      <Button @click="confirm" :loading="submitting">{{ t('message.common.button.confirm') }}</Button>
    </template>
  </Dialog>
</template>

<script lang="ts" setup>
import { ref, computed } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import InputNumber from 'primevue/inputnumber'
import FormLabel from '/@/components/form-label/index.vue'
import { tenantGameReportEntityLinkPenaltyPost } from '../../../../../api/tenantGameReportEntity'
import { showSuccess } from '../../../../../utils/message.ts'
import { REPORT_TYPE_OPTIONS, findEnumOption } from '../constants'

const emit = defineEmits(['reloadTable'])

const visible = ref(false)
const row = ref<any>(null)
const penaltyId = ref<number | null>(null)
const invalid = ref(false)
const submitting = ref(false)

const reportTypeLabel = computed(() => {
  const opt = findEnumOption(REPORT_TYPE_OPTIONS, row.value?.reportType)
  return opt ? t(opt.labelKey) : ''
})

const openDialog = (r: any) => {
  row.value = r
  penaltyId.value = null
  invalid.value = false
  submitting.value = false
  visible.value = true
}

const cancel = () => {
  visible.value = false
}

const confirm = async () => {
  if (!penaltyId.value || penaltyId.value <= 0) {
    invalid.value = true
    return
  }
  invalid.value = false
  submitting.value = true
  try {
    await tenantGameReportEntityLinkPenaltyPost({ id: row.value?.id, penaltyId: penaltyId.value })
    showSuccess(t('message.gameBusiness.trustSafety.report.linkPenaltySuccess'))
    visible.value = false
    emit('reloadTable')
  } finally {
    submitting.value = false
  }
}

defineExpose({ openDialog })
</script>

<style scoped>
.link-penalty-body {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding-top: 8px;
}
.report-meta {
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
