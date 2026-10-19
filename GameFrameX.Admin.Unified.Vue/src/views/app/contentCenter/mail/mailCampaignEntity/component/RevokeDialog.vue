<template>
  <Dialog v-model:visible="visible" modal :header="t('message.gameBusiness.mailCampaign.revokeTitle')" :draggable="false" :dismissableMask="false" class="w-full md:w-40rem">
    <div v-if="row" class="flex flex-column gap-3">
      <div class="surface-100 p-3 border-round text-sm">
        <div><span class="text-color-secondary">{{ t('message.gameBusiness.mailCampaign.name') }}：</span>{{ row.name }}</div>
        <div><span class="text-color-secondary">{{ t('message.gameBusiness.mailCampaign.publishStatus') }}：</span>{{ statusDesc(row.publishStatus) }}</div>
        <div><span class="text-color-secondary">{{ t('message.gameBusiness.mailCampaign.publishedTime') }}：</span>{{ formatUnix(row.publishedTime) }}</div>
        <div v-if="row.publishVersion && row.publishVersion > 0"><span class="text-color-secondary">{{ t('message.gameBusiness.mailCampaign.publishVersion') }}：</span>{{ row.publishVersion }}</div>
      </div>
      <div class="surface-ground p-3 border-round border-1 border-orange-300 bg-orange-50 text-sm">
        <i class="fa-solid fa-triangle-exclamation text-orange-700 mr-1"></i>
        <span class="text-orange-900">{{ t('message.gameBusiness.mailCampaign.revokeWarning') }}</span>
      </div>
    </div>
    <template #footer>
      <Button :label="t('message.common.button.cancel')" text @click="close"/>
      <Button :label="t('message.gameBusiness.mailCampaign.revoke')" icon="fa-solid fa-rotate-left" severity="warn" :loading="loading" @click="confirm"/>
    </template>
  </Dialog>
</template>

<script lang="ts" setup name="gameMailCampaignRevokeDialog">
import { ref } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import { t } from '../../../../../../i18n/index.ts'
import { showSuccess } from '../../../../../../utils/message.ts'
import { tenantGameMailCampaignEntityRevokePost } from '../../../../../../api/tenantGameMailCampaignEntity'
import {
  MAIL_CAMPAIGN_PUBLISH_STATUS_OPTIONS,
} from '../constants'

const emit = defineEmits(['done'])

const visible = ref(false)
const loading = ref(false)
const row = ref<any>(null)

const statusDesc = (v: number) => {
  const item = MAIL_CAMPAIGN_PUBLISH_STATUS_OPTIONS.find((i) => i.value === v)
  return item ? t(item.labelKey) : v
}

const formatUnix = (v?: number) => {
  if (!v || v <= 0) return '-'
  const d = new Date(v * 1000)
  return Number.isNaN(d.getTime()) ? '-' : d.toLocaleString()
}

const close = () => {
  visible.value = false
  row.value = null
}

const confirm = async () => {
  if (!row.value?.id) return
  loading.value = true
  try {
    await tenantGameMailCampaignEntityRevokePost({ id: row.value.id })
    showSuccess(t('message.gameBusiness.mailCampaign.revokeSuccess'))
    emit('done')
    close()
  } catch {
    // 全局拦截器已弹错误提示
  } finally {
    loading.value = false
  }
}

const openDialog = (target: any) => {
  row.value = target
  visible.value = true
}

defineExpose({ openDialog })
</script>
