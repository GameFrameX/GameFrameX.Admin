<template>
  <Dialog v-model:visible="visible" modal :header="title" :draggable="false" :dismissableMask="false" class="w-full md:w-30rem">
    <div v-if="row" class="flex flex-column gap-2">
      <div class="surface-100 p-2 border-round text-sm">
        <div><span class="text-color-secondary">{{ t('message.gameBusiness.openAccess.scenario') }}：</span>{{ row.scenario || '-' }}</div>
        <div><span class="text-color-secondary">{{ t('message.gameBusiness.openAccess.subjectName') }}：</span>{{ row.subjectName || '-' }}</div>
        <div class="white-space-nowrap overflow-hidden text-overflow-ellipsis">
          <span class="text-color-secondary">{{ t('message.gameBusiness.openAccess.accessKey') }}：</span>{{ row.accessKey }}
        </div>
      </div>

      <div class="font-medium text-color-secondary">{{ t('message.gameBusiness.openAccess.reasonLabel') }}</div>
      <Textarea v-model="reason" :rows="3" :autoResize="true" class="w-full"
                :placeholder="t('message.gameBusiness.openAccess.reasonPlaceholder')" />
      <small v-if="reasonError" class="text-red-500">{{ reasonError }}</small>

      <div v-if="warningText" class="text-sm" :class="warningClass">{{ warningText }}</div>
    </div>

    <template #footer>
      <Button :label="t('message.common.button.cancel')" text @click="close" />
      <Button :label="confirmLabel" icon="fa-solid fa-triangle-exclamation" :severity="confirmSeverity"
              :loading="loading" @click="confirm" />
    </template>
  </Dialog>
</template>

<script lang="ts" setup name="gameAppOpenAccessActionDialog">
import { ref, computed } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import Textarea from 'primevue/textarea'
import { t } from '../../../../../i18n/index.ts'
import { showSuccess } from '../../../../../utils/message.ts'
import {
  tenantGameAppOpenAccessRevokePost,
  tenantGameAppOpenAccessEnablePost,
  tenantGameAppOpenAccessDisablePost,
  tenantGameAppOpenAccessRefreshPost,
} from '../../../../../api/tenantGameAppOpenAccess'

type ActionType = 'revoke' | 'enable' | 'disable' | 'refresh'

const props = defineProps({
  appId: { type: [Number, String], default: undefined },
})
const emit = defineEmits(['done', 'secret'])

const visible = ref(false)
const loading = ref(false)
const row = ref<any>(null)
const action = ref<ActionType>('revoke')
const reason = ref('')
const reasonError = ref('')

const title = computed(() => {
  const map: Record<ActionType, string> = {
    revoke: t('message.gameBusiness.openAccess.revokeTitle'),
    enable: t('message.gameBusiness.openAccess.enableTitle'),
    disable: t('message.gameBusiness.openAccess.disableTitle'),
    refresh: t('message.gameBusiness.openAccess.refreshTitle'),
  }
  return map[action.value]
})

const confirmLabel = computed(() => {
  const map: Record<ActionType, string> = {
    revoke: t('message.gameBusiness.openAccess.revoke'),
    enable: t('message.gameBusiness.openAccess.enable'),
    disable: t('message.gameBusiness.openAccess.disable'),
    refresh: t('message.gameBusiness.openAccess.refresh'),
  }
  return map[action.value]
})

const confirmSeverity = computed<string>(() => {
  if (action.value === 'revoke' || action.value === 'disable' || action.value === 'refresh') return 'danger'
  return 'success'
})

const warningText = computed(() => {
  if (action.value === 'revoke') return t('message.gameBusiness.openAccess.revokeWarning')
  if (action.value === 'refresh') return t('message.gameBusiness.openAccess.refreshWarning')
  return ''
})

const warningClass = computed(() => {
  if (action.value === 'revoke') return 'text-red-500'
  if (action.value === 'refresh') return 'text-orange-500'
  return 'text-color-secondary'
})

const apiMap = {
  revoke: tenantGameAppOpenAccessRevokePost,
  enable: tenantGameAppOpenAccessEnablePost,
  disable: tenantGameAppOpenAccessDisablePost,
  refresh: tenantGameAppOpenAccessRefreshPost,
}

const successMessageMap: Record<ActionType, string> = {
  revoke: 'message.gameBusiness.openAccess.revokeSuccess',
  enable: 'message.gameBusiness.openAccess.enableSuccess',
  disable: 'message.gameBusiness.openAccess.disableSuccess',
  refresh: 'message.gameBusiness.openAccess.refreshSuccess',
}

const close = () => {
  visible.value = false
  row.value = null
  reason.value = ''
  reasonError.value = ''
}

const confirm = async () => {
  const trimmed = reason.value.trim()

  // 吊销必须填写原因
  if (action.value === 'revoke' && !trimmed) {
    reasonError.value = t('message.gameBusiness.openAccess.reasonRequired')
    return
  }
  if (trimmed.length > 256) {
    reasonError.value = t('message.gameBusiness.openAccess.reasonTooLong')
    return
  }

  loading.value = true
  try {
    const payload: any = {
      id: row.value.id,
      appId: props.appId,
      rowVersion: row.value.rowVersion,
    }
    if (trimmed) {
      payload.reason = trimmed
    }

    const res: any = await apiMap[action.value](payload)

    // 轮换成功后返回 SecretResult（一次性展示）
    if (action.value === 'refresh') {
      const secret = res?.data?.data
      if (secret) {
        emit('secret', secret)
      }
    }

    showSuccess(t(successMessageMap[action.value]))
    emit('done')
    close()
  } catch {
    // 全局拦截器已弹错误提示
  } finally {
    loading.value = false
  }
}

const openDialog = (act: ActionType, targetRow: any) => {
  action.value = act
  row.value = targetRow
  reason.value = ''
  reasonError.value = ''
  visible.value = true
}

defineExpose({ openDialog })
</script>
