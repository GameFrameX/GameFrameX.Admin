<template>
  <Drawer v-model:visible="visible" :header="t('message.onlineCenter.reportCase.handleDrawerTitle')"
          position="right" :pt="{ root: { style: 'width: 40vw' } }">
    <div v-if="caseRow" class="flex flex-column gap-3">
      <div class="font-medium text-color-secondary">
        {{ t('message.onlineCenter.reportCase.handleHint') }}
      </div>
      <div class="form-grid">
        <label>{{ t('message.onlineCenter.reportCase.columns.caseId') }}</label>
        <span data-testid="handle-case-id">{{ caseRow.caseId }}</span>
        <label>{{ t('message.onlineCenter.reportCase.columns.reportedPlayerId') }}</label>
        <span data-testid="handle-reported-player">{{ caseRow.reportedPlayerId }}</span>
        <label>{{ t('message.onlineCenter.reportCase.columns.reason') }}</label>
        <span>{{ caseRow.reason }}</span>
      </div>

      <div>
        <label class="font-medium text-color-secondary">
          {{ t('message.onlineCenter.reportCase.decisionLabel') }}
        </label>
        <Select v-model="decision" :options="decisionOptions" optionLabel="label" optionValue="value"
                :placeholder="t('message.onlineCenter.reportCase.pleaseSelectDecision')"
                class="w-full mt-1 onlineReportCase-decision" />
        <small v-if="decisionError" class="text-red-500">{{ decisionError }}</small>
      </div>

      <div v-if="needsDuration">
        <label class="font-medium text-color-secondary">
          {{ t('message.onlineCenter.reportCase.durationLabel') }}
        </label>
        <InputNumber v-model="durationMinutes" mode="decimal" :useGrouping="false" :min="1"
                     :placeholder="t('message.onlineCenter.reportCase.durationPlaceholder')"
                     class="w-full mt-1 onlineReportCase-duration" inputId="onlineReportCaseDurationInput" />
        <small v-if="durationError" class="text-red-500">{{ durationError }}</small>
      </div>

      <div>
        <label class="font-medium text-color-secondary">
          {{ t('message.onlineCenter.reportCase.reasonLabel') }}
        </label>
        <Textarea v-model="reason" :rows="3" :autoResize="true" class="w-full mt-1 onlineReportCase-reason"
                  :placeholder="t('message.onlineCenter.reportCase.reasonPlaceholder')" />
        <small v-if="reasonError" class="text-red-500">{{ reasonError }}</small>
      </div>
    </div>
    <template #footer>
      <Button :label="t('message.common.button.cancel')" text @click="close" />
      <Button :label="t('message.onlineCenter.reportCase.handle')" icon="fa-solid fa-gavel"
              severity="warn" :loading="loading" @click="confirm" />
    </template>
  </Drawer>
</template>

<script lang="ts" setup name="onlineReportCaseHandleDrawer">
/**
 * 举报案件处置抽屉（C342 / GFX-758，vault:C7 S6.10）。
 * 决策枚举 Dismiss/Warn/Mute/Ban/ForwardPenalty；理由必填（进审计）；
 * Mute/Ban 时 DurationMinutes 条件必填。提交前高危确认层（showConfirm）。
 * 服务端语义：Mute/Ban 先写 TenantGamePenaltyEntity 事实源再触发 Online 联动命令。
 */
import { ref, computed } from 'vue'
import Drawer from 'primevue/drawer'
import Button from 'primevue/button'
import Select from 'primevue/select'
import InputNumber from 'primevue/inputnumber'
import Textarea from 'primevue/textarea'
import { showConfirm, showSuccess } from '../../../../../utils/message.ts'
import { onlineReportCaseHandlePost } from '../../../../../api/onlineReportCase'

const props = withDefaults(defineProps<{
  appId?: string | number
  serverId?: string | number
}>(), { appId: undefined, serverId: undefined })

const emit = defineEmits(['done'])

const visible = ref(false)
const loading = ref(false)
const caseRow = ref<any>(null)
const decision = ref<string | null>(null)
const decisionError = ref('')
const reason = ref('')
const reasonError = ref('')
const durationMinutes = ref<number | null>(null)
const durationError = ref('')

// OnlineReportDecisionKind（数字枚举 1-5，C356 联调实测——无字符串枚举转换器，线缆必须传数字）：
// Dismiss=1 驳回 / Warn=2 警告 / Mute=3 禁言 / Ban=4 封禁 / ForwardPenalty=5 转人工处罚
const DECISION_WIRE: Record<string, number> = { Dismiss: 1, Warn: 2, Mute: 3, Ban: 4, ForwardPenalty: 5 }
const decisionOptions = computed(() => [
  { label: t('message.onlineCenter.reportCase.decision.dismiss'), value: 'Dismiss' },
  { label: t('message.onlineCenter.reportCase.decision.warn'), value: 'Warn' },
  { label: t('message.onlineCenter.reportCase.decision.mute'), value: 'Mute' },
  { label: t('message.onlineCenter.reportCase.decision.ban'), value: 'Ban' },
  { label: t('message.onlineCenter.reportCase.decision.forwardPenalty'), value: 'ForwardPenalty' },
])

// Mute/Ban 产生处罚事实，DurationMinutes 条件必填（change.md §3）
const needsDuration = computed(() => decision.value === 'Mute' || decision.value === 'Ban')

const close = () => {
  visible.value = false
  reset()
}

const reset = () => {
  caseRow.value = null
  decision.value = null
  decisionError.value = ''
  reason.value = ''
  reasonError.value = ''
  durationMinutes.value = null
  durationError.value = ''
}

const validate = () => {
  decisionError.value = ''
  reasonError.value = ''
  durationError.value = ''
  let valid = true
  if (decision.value == null) {
    decisionError.value = t('message.onlineCenter.reportCase.decisionRequired')
    valid = false
  }
  const trimmed = reason.value.trim()
  if (!trimmed) {
    reasonError.value = t('message.onlineCenter.reportCase.reasonRequired')
    valid = false
  } else if (trimmed.length > 500) {
    reasonError.value = t('message.onlineCenter.reportCase.reasonTooLong')
    valid = false
  }
  if (needsDuration.value && (durationMinutes.value == null || durationMinutes.value < 1)) {
    durationError.value = t('message.onlineCenter.reportCase.durationRequired')
    valid = false
  }
  return valid
}

const confirm = async () => {
  if (!validate()) {
    return
  }
  const confirmed = await showConfirm(
    t('message.onlineCenter.reportCase.handleConfirmMessage', { caseId: caseRow.value?.caseId ?? '' }),
    t('message.onlineCenter.reportCase.handleConfirmTitle'),
    { type: 'danger' },
  )
  if (!confirmed) {
    return
  }
  loading.value = true
  try {
    await onlineReportCaseHandlePost({
      appId: props.appId as any,
      serverId: (caseRow.value?.serverId ?? props.serverId) as any,
      caseId: caseRow.value.caseId,
      reportedPlayerId: caseRow.value.reportedPlayerId,
      // 线缆契约：后端 OnlineReportDecisionKind 数字枚举（无字符串枚举转换器）
      decision: DECISION_WIRE[decision.value as string],
      reason: reason.value.trim(),
      durationMinutes: needsDuration.value ? (durationMinutes.value as number) : undefined,
    })
    showSuccess(t('message.onlineCenter.reportCase.handleSuccess'))
    emit('done')
    close()
  } catch {
    // 全局拦截器已弹错误提示
  } finally {
    loading.value = false
  }
}

defineExpose({
  openDrawer: (row: any) => {
    reset()
    caseRow.value = row
    visible.value = true
  },
})
</script>

<style scoped>
.form-grid {
  display: grid;
  grid-template-columns: 140px 1fr;
  gap: 0.5rem 1rem;
  align-items: center;
}
</style>
