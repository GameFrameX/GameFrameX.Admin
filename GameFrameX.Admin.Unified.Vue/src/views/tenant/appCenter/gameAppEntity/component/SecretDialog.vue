<template>
  <Dialog v-model:visible="visible" modal draggable :header="t('gameBusiness.app.secretGenerated')" :style="{ width: 'min(500px, calc(100vw - 32px))' }" @hide="emit('closed')">
    <div class="flex flex-column gap-4" v-if="secretInfo">
      <div>
        <label class="block mb-2 font-bold">{{ t('gameBusiness.app.appId') }}</label>
        <div class="flex align-items-center gap-2">
          <InputText :modelValue="secretInfo.id" readonly class="w-full"/>
          <Button link @click="copyToClipboard(secretInfo.id)">
            <i class="fa-solid fa-copy"></i>
          </Button>
        </div>
      </div>
      <div>
        <label class="block mb-2 font-bold">{{ t('gameBusiness.app.appSecret') }}</label>
        <div class="flex align-items-center gap-2">
          <InputText :modelValue="secretInfo.appSecret" readonly class="w-full"/>
          <Button link @click="copyToClipboard(secretInfo.appSecret)">
            <i class="fa-solid fa-copy"></i>
          </Button>
        </div>
      </div>
      <div class="text-orange-500 text-sm">
        <i class="fa-solid fa-triangle-exclamation mr-1"></i>
        {{ t('gameBusiness.app.secretWarning') }}
      </div>
    </div>
    <template #footer>
      <Button @click="visible = false">{{ t('common.button.confirm') }}</Button>
    </template>
  </Dialog>
</template>

<script lang="ts" setup>
import {ref} from 'vue'
import {showSuccess} from '../../../../../utils/message.ts'

const emit = defineEmits(['closed'])
const visible = ref(false)
const secretInfo = ref<{ id: string; appSecret: string } | null>(null)

const openDialog = (data: any) => {
  secretInfo.value = {id: String(data.id), appSecret: data.appSecret}
  visible.value = true
}

const copyToClipboard = (text: string) => {
  navigator.clipboard.writeText(text).then(() => {
    showSuccess(t('common.message.copySuccess'))
  }).catch(() => {
    showSuccess(t('common.message.copySuccess'))
  })
}

defineExpose({openDialog})
</script>
