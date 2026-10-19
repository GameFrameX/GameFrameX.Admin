<template>
  <Dialog v-model:visible="visible" modal :header="t('message.gameBusiness.openAccess.secretTitle')" :draggable="false"
          :closable="false" :dismissableMask="false" class="w-full md:w-40rem">
    <div v-if="secret" class="flex flex-column gap-3">
      <div class="bg-orange-50 border-1 border-orange-200 border-round p-3 text-orange-700 text-sm">
        <i class="fa-solid fa-triangle-exclamation mr-1"></i>
        {{ t('message.gameBusiness.openAccess.secretWarning') }}
      </div>

      <div class="flex flex-column gap-2">
        <label class="font-medium text-color-secondary">{{ t('message.gameBusiness.openAccess.accessKey') }}</label>
        <div class="flex align-items-center gap-2">
          <code class="flex-1 surface-100 p-2 border-round text-sm break-all">{{ secret.accessKey }}</code>
          <Button severity="secondary" size="small" @click="copyToClipboard(secret.accessKey)">
            {{ t('message.gameBusiness.openAccess.copyKey') }}
          </Button>
        </div>
      </div>

      <div class="flex flex-column gap-2">
        <label class="font-medium text-color-secondary">AccessSecret</label>
        <div class="flex align-items-center gap-2">
          <code class="flex-1 surface-100 p-2 border-round text-sm break-all">{{ secret.accessSecret }}</code>
          <Button severity="secondary" size="small" @click="copyToClipboard(secret.accessSecret)">
            {{ t('message.gameBusiness.openAccess.copySecret') }}
          </Button>
        </div>
      </div>
    </div>

    <template #footer>
      <Button :label="t('message.gameBusiness.openAccess.iHaveSaved')" severity="danger" @click="close" />
    </template>
  </Dialog>
</template>

<script lang="ts" setup name="gameAppOpenAccessSecretDialog">
import { ref } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import { t } from '../../../../../i18n/index.ts'
import { showSuccess, showError } from '../../../../../utils/message.ts'

const visible = ref(false)
const secret = ref<any>(null)

const copyToClipboard = async (text: string) => {
  try {
    await navigator.clipboard.writeText(text)
    showSuccess(t('message.gameBusiness.openAccess.copySuccess'))
  } catch {
    showError(t('message.gameBusiness.openAccess.copyFailed'))
  }
}

const open = (secretResult: any) => {
  secret.value = secretResult
  visible.value = true
}

const close = () => {
  visible.value = false
  secret.value = null
}

defineExpose({ open })
</script>
