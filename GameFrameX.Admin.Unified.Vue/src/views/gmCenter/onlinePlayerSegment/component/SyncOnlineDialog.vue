<template>
  <Dialog v-model:visible="visible" modal :header="t('message.onlineCenter.playerSegment.syncDialogTitle')"
          :draggable="false" :dismissableMask="false" class="onlinePlayerSegment-sync-dialog w-full md:w-36rem">
    <div class="flex flex-column gap-3">
      <div class="font-medium text-color-secondary">
        {{ t('message.onlineCenter.playerSegment.syncHint') }}
      </div>
      <div class="form-grid">
        <label>{{ t('message.onlineCenter.playerSegment.name') }}</label>
        <InputText :modelValue="segmentName" disabled />
        <label>{{ t('message.onlineCenter.playerSegment.syncServerId') }}</label>
        <InputText :modelValue="effectiveServerId" disabled />
      </div>
      <div>
        <label class="font-medium text-color-secondary">
          {{ t('message.onlineCenter.playerSegment.reasonLabel') }}
        </label>
        <Textarea v-model="reason" :rows="3" :autoResize="true" class="w-full mt-1"
                  :placeholder="t('message.onlineCenter.playerSegment.reasonPlaceholder')" />
        <small v-if="reasonError" class="text-red-500">{{ reasonError }}</small>
      </div>
    </div>
    <template #footer>
      <Button :label="t('message.common.button.cancel')" text @click="close" />
      <Button :label="t('message.onlineCenter.playerSegment.syncOnline')" icon="fa-solid fa-cloud-arrow-up"
              severity="warn" :loading="loading" @click="confirm" />
    </template>
  </Dialog>
</template>

<script lang="ts" setup name="onlinePlayerSegmentSyncOnlineDialog">
/**
 * 分群同步 Online 弹层（高危入口）。
 * 定义快照经统一下发入口（syncPlayerSegment 通道）送达 Online，Online 分配分群标识回写本行；
 * 理由必填（≤500 字）进审计；成功 toast 回显 Online 确认版本。
 */
import { ref } from 'vue'
import { usePageServer } from '../../components/usePageServer'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import { showSuccess } from '../../../../utils/message.ts'
import { gameOnlinePlayerSegmentSyncOnlinePost } from '../../../../api/gameOnlinePlayerSegment'

const props = defineProps<{ serverId: string }>()

const { effectiveServerId, canQueryServer, beginServerRequest } = usePageServer(() => props.serverId)

const visible = ref(false)
const loading = ref(false)
const reason = ref('')
const reasonError = ref('')
const segmentId = ref<number | string>()
const segmentName = ref('')
const emit = defineEmits(['done'])

const close = () => {
  visible.value = false
  reset()
}

const reset = () => {
  reason.value = ''
  reasonError.value = ''
}

const validate = () => {
  if (!canQueryServer.value) return false
  reasonError.value = ''
  const trimmed = reason.value.trim()
  if (!trimmed) {
    reasonError.value = t('message.onlineCenter.playerSegment.reasonRequired')
    return false
  }
  if (trimmed.length > 500) {
    reasonError.value = t('message.onlineCenter.playerSegment.reasonTooLong')
    return false
  }
  if (!canQueryServer.value) {
    return false
  }
  return true
}

const confirm = async () => {
  const operation = beginServerRequest()
  if (!operation) return
  if (!validate()) {
    return
  }
  loading.value = true
  try {
    const res: any = await gameOnlinePlayerSegmentSyncOnlinePost({
      id: segmentId.value as number | string,
      serverId: operation.serverId,
      reason: reason.value.trim(),
    })
    if (!operation.isCurrent()) return
    const confirmVersion = res.data?.data?.confirmVersion
    if (!operation.isCurrent()) return
    showSuccess(confirmVersion
      ? `${t('message.onlineCenter.playerSegment.syncSuccess')}（${t('message.onlineCenter.playerSegment.confirmVersion')}: ${confirmVersion}）`
      : t('message.onlineCenter.playerSegment.syncSuccess'))
    emit('done')
    close()
  } catch {
    // 全局拦截器已弹错误提示（同步失败态由后端落库，列表 LastServerMessage 可见）
  } finally {
    if (operation.isCurrent()) loading.value = false
  }
}

defineExpose({
  openDialog: (row: any) => {
    reset()
    segmentId.value = row?.id
    segmentName.value = row?.name ?? ''
    visible.value = true
  },
})
</script>
