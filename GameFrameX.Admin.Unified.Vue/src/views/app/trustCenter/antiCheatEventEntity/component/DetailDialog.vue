<template>
  <Dialog v-model:visible="visible" modal :header="t('message.gameBusiness.antiCheat.detail.title')" :style="{ width: 'min(720px, calc(100vw - 32px))' }" :draggable="false">
    <div v-block-loading="loading" class="antiCheat-detail">
      <div class="grid">
        <div class="col-6 field"><span class="label">{{ t('message.gameBusiness.appContext.currentApp') }}:</span> {{ appName(detail.appId) }}</div>
        <div class="col-6 field"><span class="label">{{ t('message.gameBusiness.antiCheat.provider.label') }}:</span> {{ providerLabel(detail.provider) }}</div>
        <div class="col-6 field" v-if="detail.providerName"><span class="label">{{ t('message.gameBusiness.antiCheat.providerName') }}:</span> {{ detail.providerName }}</div>
        <div class="col-6 field" v-if="detail.providerEventId"><span class="label">{{ t('message.gameBusiness.antiCheat.providerEventId') }}:</span> {{ detail.providerEventId }}</div>
        <div class="col-6 field"><span class="label">{{ t('message.gameBusiness.antiCheat.targetType.label') }}:</span> {{ targetLabel(detail.targetType) }}</div>
        <div class="col-6 field"><span class="label">{{ t('message.gameBusiness.antiCheat.targetId') }}:</span> {{ detail.targetId }}</div>
        <div class="col-6 field"><span class="label">{{ t('message.gameBusiness.antiCheat.riskLevel.label') }}:</span>
          <Tag :value="riskLabel(detail.riskLevel)" :severity="riskLevelSeverity(detail.riskLevel)"/>
        </div>
        <div class="col-6 field"><span class="label">{{ t('message.gameBusiness.antiCheat.suggestedAction.label') }}:</span>
          <Tag :value="suggestedLabel(detail.suggestedAction)" :severity="suggestedActionSeverity(detail.suggestedAction)"/>
        </div>
        <div class="col-12 field" v-if="detail.suggestedActionNote"><span class="label">{{ t('message.gameBusiness.antiCheat.suggestedActionNote') }}:</span> {{ detail.suggestedActionNote }}</div>
        <div class="col-12 field"><span class="label">{{ t('message.gameBusiness.antiCheat.evidenceSummary') }}:</span>
          <pre class="evidence"><code>{{ detail.evidenceSummary }}</code></pre>
        </div>
        <div class="col-6 field"><span class="label">{{ t('message.gameBusiness.antiCheat.reviewState.label') }}:</span>
          <Tag :value="reviewLabel(detail.manualReviewState)" :severity="reviewStateSeverity(detail.manualReviewState)"/>
        </div>
        <div class="col-6 field"><span class="label">{{ t('message.gameBusiness.antiCheat.occurredAt') }}:</span> {{ formatUnix(detail.occurredTime) }}</div>
        <div class="col-6 field" v-if="detail.reviewerId"><span class="label">{{ t('message.gameBusiness.antiCheat.reviewerId') }}:</span> {{ detail.reviewerId }}</div>
        <div class="col-6 field" v-if="detail.reviewedTime"><span class="label">{{ t('message.gameBusiness.antiCheat.reviewedAt') }}:</span> {{ formatUnix(detail.reviewedTime) }}</div>
        <div class="col-12 field" v-if="detail.reviewNote"><span class="label">{{ t('message.gameBusiness.antiCheat.reviewNote') }}:</span> {{ detail.reviewNote }}</div>
        <div class="col-12 field"><span class="label">{{ t('message.gameBusiness.antiCheat.idempotencyKey') }}:</span> <code>{{ detail.idempotencyKey }}</code></div>
        <div class="col-6 field"><span class="label">{{ t('common.label.createTime') }}:</span> {{ detail.createdTime }}</div>
      </div>
    </div>
  </Dialog>
</template>

<script lang="ts" setup="" name="antiCheatDetailDialog">
import {ref} from 'vue'
import {useCurrentApp} from '../../../../../composables/useCurrentApp.ts'
import {formatUnix} from '../../../../../utils/formatTime.ts'
import { tenantGameAntiCheatEventEntityDetailGet } from '../../../../../api/tenantGameAntiCheatEventEntity'
import {
    ANTI_CHEAT_PROVIDER_OPTIONS,
    ANTI_CHEAT_SUGGESTED_ACTION_OPTIONS,
    ANTI_CHEAT_TARGET_TYPE_OPTIONS,
    MANUAL_REVIEW_STATE_OPTIONS,
    RISK_LEVEL_OPTIONS,
    riskLevelSeverity,
    reviewStateSeverity,
    suggestedActionSeverity,
} from '../constants'

const {appName} = useCurrentApp()
const visible = ref(false)
const loading = ref(false)
const detail = ref<any>({})

const labelOf = (options: Array<{ value: number; label: string }>, value: number): string => {
    const found = options.find((o) => o.value === value)
    return found ? t(found.label) : ''
}
const providerLabel = (v: number) => labelOf(ANTI_CHEAT_PROVIDER_OPTIONS, v)
const riskLabel = (v: number) => labelOf(RISK_LEVEL_OPTIONS, v)
const suggestedLabel = (v: number) => labelOf(ANTI_CHEAT_SUGGESTED_ACTION_OPTIONS, v)
const reviewLabel = (v: number) => labelOf(MANUAL_REVIEW_STATE_OPTIONS, v)
const targetLabel = (v: number) => labelOf(ANTI_CHEAT_TARGET_TYPE_OPTIONS, v)

const open = async (id: number) => {
    visible.value = true
    loading.value = true
    detail.value = {}
    try {
        const res = await tenantGameAntiCheatEventEntityDetailGet({id: id})
        detail.value = res.data?.data ?? {}
    } finally {
        loading.value = false
    }
}

defineExpose({open})
</script>

<style scoped>
.antiCheat-detail .field { padding: 4px 8px; word-break: break-all; }
.antiCheat-detail .label { color: var(--p-text-muted-color); margin-right: 4px; }
.antiCheat-detail .evidence {
  background: var(--p-content-background); padding: 8px; border-radius: 4px;
  max-height: 200px; overflow: auto; white-space: pre-wrap; word-break: break-all;
}
</style>
