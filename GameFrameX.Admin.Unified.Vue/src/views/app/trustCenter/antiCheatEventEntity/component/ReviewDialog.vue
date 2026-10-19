<template>
  <Dialog v-model:visible="visible" modal :header="t('message.gameBusiness.antiCheat.review.title')" :style="{ width: 'min(480px, calc(100vw - 32px))' }" :draggable="false">
    <div class="antiCheat-review">
      <div class="field">
        <label>{{ t('message.gameBusiness.antiCheat.targetId') }}</label>
        <div class="readonly">{{ row.targetId }}</div>
      </div>
      <div class="field">
        <label>{{ t('message.gameBusiness.antiCheat.suggestedAction.label') }}</label>
        <div class="readonly">
          <Tag :value="suggestedLabel(row.suggestedAction)" :severity="suggestedActionSeverity(row.suggestedAction)"/>
        </div>
      </div>
      <div class="field">
        <label class="required">{{ t('message.gameBusiness.antiCheat.review.target') }}</label>
        <Select v-model="form.manualReviewState" :options="REVIEW_TARGET_OPTIONS" optionLabel="label" optionValue="value"
                :placeholder="t('message.gameBusiness.antiCheat.review.pleaseSelectTarget')" class="w-full" filter/>
      </div>
      <div class="field">
        <label>{{ t('message.gameBusiness.antiCheat.reviewNote') }}</label>
        <Textarea v-model="form.reviewNote" rows="3" class="w-full" :maxlength="500"/>
      </div>
    </div>
    <template #footer>
      <Button :label="t('message.common.button.cancel')" severity="secondary" @click="visible = false"/>
      <Button :label="t('message.common.button.confirm')" severity="warn" :loading="submitting" @click="submit"/>
    </template>
  </Dialog>
</template>

<script lang="ts" setup="" name="antiCheatReviewDialog">
import {ref} from 'vue'
import {showConfirm, showSuccess} from '../../../../../utils/message.ts'
import { tenantGameAntiCheatEventEntityReviewPost } from '../../../../../api/tenantGameAntiCheatEventEntity'
import {
    ANTI_CHEAT_SUGGESTED_ACTION_OPTIONS,
    REVIEW_TARGET_OPTIONS,
    suggestedActionSeverity,
} from '../constants'

const emit = defineEmits<{ (e: 'reviewed'): void }>()

const visible = ref(false)
const submitting = ref(false)
const row = ref<any>({})
const form = ref<{ manualReviewState: number | undefined; reviewNote: string }>({manualReviewState: undefined, reviewNote: ''})

const labelOf = (options: Array<{ value: number; label: string }>, value: number): string => {
    const found = options.find((o) => o.value === value)
    return found ? t(found.label) : ''
}
const suggestedLabel = (v: number) => labelOf(ANTI_CHEAT_SUGGESTED_ACTION_OPTIONS, v)

const open = (data: any) => {
    row.value = data
    form.value = {manualReviewState: undefined, reviewNote: ''}
    visible.value = true
}

const submit = async () => {
    if (!form.value.manualReviewState) {
        return
    }
    const confirmed = await showConfirm(
        t('message.gameBusiness.antiCheat.review.confirm'),
        t('message.common.message.tip'),
        {confirmText: t('message.common.button.confirm'), cancelText: t('message.common.button.cancel'), type: 'warning'},
    )
    if (!confirmed) {
        return
    }
    submitting.value = true
    try {
        await tenantGameAntiCheatEventEntityReviewPost({
            id: row.value.id,
            rowVersion: row.value.rowVersion,
            manualReviewState: form.value.manualReviewState,
            reviewNote: form.value.reviewNote?.trim() || null,
        })
        showSuccess(t('message.gameBusiness.antiCheat.review.success'))
        visible.value = false
        emit('reviewed')
    } finally {
        submitting.value = false
    }
}

defineExpose({open})
</script>

<style scoped>
.antiCheat-review .field { margin-bottom: 12px; }
.antiCheat-review .field label { display: block; margin-bottom: 4px; }
.antiCheat-review .field label.required::after { content: ' *'; color: var(--red-500); }
.antiCheat-review .readonly { color: var(--p-text-muted-color); }
</style>
