<template>
  <Dialog v-model:visible="visible" modal :header="t('message.onlineCenter.configRollout.rollbackDialogTitle')"
          :draggable="false" :dismissableMask="false" class="onlineConfigRollout-rollback-dialog w-full md:w-36rem">
    <div class="flex flex-column gap-3">
      <div class="font-medium text-color-secondary">
        {{ t('message.onlineCenter.configRollout.rollbackHint') }}
      </div>
      <div class="form-grid">
        <label>{{ t('message.onlineCenter.configRollout.configKind') }}</label>
        <InputText :modelValue="configKindDesc" disabled />
        <label>{{ t('message.onlineCenter.configRollout.configKey') }}</label>
        <InputText :modelValue="row?.configKey ?? ''" disabled />
        <label>{{ t('message.onlineCenter.configRollout.currentVersion') }}</label>
        <InputText :modelValue="row?.configVersion ?? ''" disabled />
        <label>{{ t('message.onlineCenter.configRollout.targetVersion') }}</label>
        <InputText :modelValue="row?.targetVersion > 0 ? row.targetVersion : '-'" disabled />
        <label>{{ t('message.onlineCenter.configRollout.rollbackTo') }}</label>
        <InputNumber v-model="targetVersion" :min="1" :useGrouping="false"
                     :placeholder="t('message.onlineCenter.configRollout.rollbackToPlaceholder')" />
        <label>{{ t('message.onlineCenter.configRollout.serverId') }}</label>
        <InputText :modelValue="row?.serverId ?? ''" disabled />
      </div>
      <div>
        <label class="font-medium text-color-secondary">
          {{ t('message.onlineCenter.configRollout.reasonLabel') }}
        </label>
        <Textarea v-model="reason" :rows="3" :autoResize="true" class="w-full mt-1"
                  :placeholder="t('message.onlineCenter.configRollout.rollbackReasonPlaceholder')" />
        <small v-if="reasonError" class="text-red-500">{{ reasonError }}</small>
      </div>
    </div>
    <template #footer>
      <Button :label="t('message.common.button.cancel')" text @click="close" />
      <Button :label="t('message.onlineCenter.configRollout.rollback')" icon="fa-solid fa-rotate-left"
              severity="danger" :loading="loading" @click="confirm" />
    </template>
  </Dialog>
</template>

<script lang="ts" setup name="onlineConfigRolloutRollbackDialog">
/**
 * 灰度回滚弹层（C343 / GFX-759 高危入口）。
 * 回滚 = 版本化新下发：新行 TargetVersion 指向要恢复的历史版本（SegmentId/Percent 恢复到目标语义），
 * 旧行置 RolledBack 不可变；理由必填进审计（VC-7.12：回滚后行为恢复 + 审计完整）。
 * ServerId 取发布行原值（同配置维度），不可在回滚时改目标作用域。
 */
import { ref, computed } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Textarea from 'primevue/textarea'
import { showSuccess } from '../../../../../utils/message.ts'
import { tenantGameOnlineConfigRolloutRollbackPost } from '../../../../../api/tenantGameOnlineConfigRollout'
import { ONLINE_CONFIG_ROLLOUT_KIND_OPTIONS } from '../constants'

const props = withDefaults(defineProps<{
  appId?: string | number
}>(), { appId: undefined })

const visible = ref(false)
const loading = ref(false)
const reason = ref('')
const reasonError = ref('')
const targetVersion = ref<number | null>(null)
const row = ref<any>(null)
const emit = defineEmits(['done'])

const configKindDesc = computed(() => {
  const item = ONLINE_CONFIG_ROLLOUT_KIND_OPTIONS.find((i) => i.value === row.value?.configKind)
  return item ? t(item.labelKey) : (row.value?.configKind ?? '')
})

const close = () => {
  visible.value = false
  reset()
}

const reset = () => {
  reason.value = ''
  reasonError.value = ''
  targetVersion.value = null
  row.value = null
}

const validate = () => {
  reasonError.value = ''
  const trimmed = reason.value.trim()
  if (!trimmed) {
    reasonError.value = t('message.onlineCenter.configRollout.reasonRequired')
    return false
  }
  if (trimmed.length > 500) {
    reasonError.value = t('message.onlineCenter.configRollout.reasonTooLong')
    return false
  }
  if (!targetVersion.value || targetVersion.value <= 0) {
    return false
  }
  return true
}

const confirm = async () => {
  if (!validate() || !row.value) {
    return
  }
  loading.value = true
  try {
    await tenantGameOnlineConfigRolloutRollbackPost({
      appId: props.appId as string | number,
      serverId: row.value.serverId,
      configKind: row.value.configKind,
      configKey: row.value.configKey,
      targetVersion: targetVersion.value as number,
      reason: reason.value.trim(),
    })
    showSuccess(t('message.onlineCenter.configRollout.rollbackSuccess'))
    emit('done')
    close()
  } catch {
    // 全局拦截器已弹错误提示
  } finally {
    loading.value = false
  }
}

defineExpose({
  openDialog: (target: any) => {
    reset()
    row.value = target
    visible.value = true
  },
})
</script>
