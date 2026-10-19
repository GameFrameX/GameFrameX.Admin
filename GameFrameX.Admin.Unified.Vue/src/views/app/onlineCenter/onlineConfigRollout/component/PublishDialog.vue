<template>
  <Dialog v-model:visible="visible" modal :header="t('message.onlineCenter.configRollout.publishDialogTitle')"
          :draggable="false" :dismissableMask="false" class="onlineConfigRollout-publish-dialog w-full md:w-40rem">
    <div class="flex flex-column gap-3">
      <div class="font-medium text-color-secondary">
        {{ t('message.onlineCenter.configRollout.publishHint') }}
      </div>
      <div class="form-grid">
        <label>{{ t('message.onlineCenter.configRollout.configKind') }}</label>
        <Select v-model="configKind" :options="kindOptions" optionLabel="label" optionValue="value"
                :placeholder="t('message.onlineCenter.configRollout.configKindPlaceholder')" />
        <label>{{ t('message.onlineCenter.configRollout.configKey') }}</label>
        <InputNumber v-model="configKey" :useGrouping="false"
                     :placeholder="t('message.onlineCenter.configRollout.configKeyPlaceholder')" />
        <label>{{ t('message.onlineCenter.configRollout.segmentId') }}</label>
        <Select v-model="segmentId" :options="segmentOptions" optionLabel="label" optionValue="value"
                :loading="segmentLoading" :placeholder="t('message.onlineCenter.configRollout.segmentPlaceholder')" />
        <label>{{ t('message.onlineCenter.configRollout.serverId') }}</label>
        <InputNumber v-model="serverId" :useGrouping="false"
                     :placeholder="t('message.onlineCenter.configRollout.serverIdPlaceholder')" />
        <label>{{ t('message.onlineCenter.configRollout.percent') }}</label>
        <InputNumber v-model="percent" :min="1" :max="100" :useGrouping="false"
                     :placeholder="t('message.onlineCenter.configRollout.percentPlaceholder')" />
      </div>
      <small class="text-color-secondary">{{ t('message.onlineCenter.configRollout.percentHint') }}</small>
      <div>
        <label class="font-medium text-color-secondary">
          {{ t('message.onlineCenter.configRollout.reasonLabel') }}
        </label>
        <Textarea v-model="reason" :rows="3" :autoResize="true" class="w-full mt-1"
                  :placeholder="t('message.onlineCenter.configRollout.reasonPlaceholder')" />
        <small v-if="reasonError" class="text-red-500">{{ reasonError }}</small>
      </div>
    </div>
    <template #footer>
      <Button :label="t('message.common.button.cancel')" text @click="close" />
      <Button :label="t('message.onlineCenter.configRollout.publish')" icon="fa-solid fa-triangle-exclamation"
              severity="warn" :loading="loading" @click="confirm" />
    </template>
  </Dialog>
</template>

<script lang="ts" setup name="onlineConfigRolloutPublishDialog">
/**
 * 灰度发布弹层（C343 / GFX-759 高危入口）。
 * Percent 仅做 1-100 边界校验（语义边界 OnlineRolloutSemanticMap），不做桶展开/命中计算；
 * SegmentId 下拉仅列已同步 Online 的分群（未同步分群 Online 无法解释命中，后端同样拒绝）；
 * 理由必填（≤500 字）进审计；成功 toast 回显 Online 确认版本（VC-7.12 Admin 半边）。
 */
import { ref } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import InputNumber from 'primevue/inputnumber'
import Select from 'primevue/select'
import Textarea from 'primevue/textarea'
import { showSuccess } from '../../../../../utils/message.ts'
import { tenantGameOnlineConfigRolloutPublishPost } from '../../../../../api/tenantGameOnlineConfigRollout'
import { tenantGameOnlinePlayerSegmentPagePost } from '../../../../../api/tenantGameOnlinePlayerSegment'
import { OnlinePlayerSegmentSyncStatus } from '../../../../../api/tenantGameOnlinePlayerSegment/types'
import { ONLINE_CONFIG_ROLLOUT_KIND_OPTIONS } from '../constants'

const props = withDefaults(defineProps<{
  appId?: string | number
}>(), { appId: undefined })

const visible = ref(false)
const loading = ref(false)
const reason = ref('')
const reasonError = ref('')
const configKind = ref<number | null>(null)
const configKey = ref<number | null>(null)
const segmentId = ref<number | string | null>(null)
const serverId = ref<number | null>(null)
const percent = ref<number | null>(null)
const segmentOptions = ref<Array<{ value: number | string; label: string }>>([])
const segmentLoading = ref(false)
const emit = defineEmits(['done'])

const kindOptions = ONLINE_CONFIG_ROLLOUT_KIND_OPTIONS.map((o) => ({ value: o.value, label: t(o.labelKey) }))

const close = () => {
  visible.value = false
  reset()
}

const reset = () => {
  reason.value = ''
  reasonError.value = ''
  configKind.value = null
  configKey.value = null
  segmentId.value = null
  serverId.value = null
  percent.value = null
}

/** 拉取已同步分群（SyncStatus=Synced 才可被灰度引用——后端 EnsureSegmentSyncedAsync 同口径） */
const loadSyncedSegments = async () => {
  segmentLoading.value = true
  try {
    const res: any = await tenantGameOnlinePlayerSegmentPagePost({
      appId: props.appId as string | number,
      syncStatus: OnlinePlayerSegmentSyncStatus.Synced,
      page: 1,
      pageSize: 200,
    })
    segmentOptions.value = (res.data?.data?.items ?? []).map((item: any) => ({
      value: item.id,
      label: item.name,
    }))
  } catch {
    segmentOptions.value = []
  } finally {
    segmentLoading.value = false
  }
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
  if (!configKind.value || !configKey.value || configKey.value <= 0 || !segmentId.value || !serverId.value || serverId.value <= 0) {
    return false
  }
  if (!percent.value || percent.value < 1 || percent.value > 100) {
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
    const res: any = await tenantGameOnlineConfigRolloutPublishPost({
      appId: props.appId as string | number,
      serverId: serverId.value as number,
      configKind: configKind.value as number,
      configKey: String(configKey.value),
      segmentId: segmentId.value as number | string,
      percent: percent.value as number,
      reason: reason.value.trim(),
    })
    const confirmVersion = res.data?.data?.onlineConfirmVersion
    showSuccess(confirmVersion
      ? `${t('message.onlineCenter.configRollout.publishSuccess')}（${t('message.onlineCenter.configRollout.onlineConfirmVersion')}: ${confirmVersion}）`
      : t('message.onlineCenter.configRollout.publishSuccess'))
    emit('done')
    close()
  } catch {
    // 全局拦截器已弹错误提示（下发失败后端不留半生效版本行，可整重试）
  } finally {
    loading.value = false
  }
}

defineExpose({
  openDialog: async () => {
    reset()
    visible.value = true
    await loadSyncedSegments()
  },
})
</script>
