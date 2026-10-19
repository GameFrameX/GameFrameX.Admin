<template>
  <Dialog v-model:visible="visible" modal :header="t('message.gameBusiness.mailCampaign.publishTitle')" :draggable="false" :dismissableMask="false" class="w-full md:w-40rem">
    <div v-if="row" class="flex flex-column gap-3">
      <div class="surface-100 p-3 border-round text-sm">
        <div><span class="text-color-secondary">{{ t('message.gameBusiness.mailCampaign.name') }}：</span>{{ row.name }}</div>
        <div><span class="text-color-secondary">{{ t('message.gameBusiness.mailCampaign.mailType') }}：</span>{{ mailTypeDesc(row.mailType) }}</div>
        <div><span class="text-color-secondary">{{ t('message.gameBusiness.mailCampaign.publishStatus') }}：</span>{{ statusDesc(row.publishStatus) }}</div>
        <div v-if="row.serverCampaignId && row.serverCampaignId > 0"><span class="text-color-secondary">{{ t('message.gameBusiness.mailCampaign.serverCampaignId') }}：</span>{{ row.serverCampaignId }}</div>
      </div>
      <div class="flex align-items-center gap-2">
        <Button :label="t('message.gameBusiness.mailCampaign.refreshHitCount')" icon="fa-solid fa-bolt" severity="info" text size="small" :loading="previewLoading" @click="loadPreview"/>
        <div v-if="hitCountText" class="text-sm font-medium">{{ hitCountText }}</div>
      </div>
      <div class="surface-ground p-3 border-round border-1 border-yellow-200 bg-yellow-50 text-sm">
        <i class="fa-solid fa-triangle-exclamation text-yellow-700 mr-1"></i>
        <span class="text-yellow-900">{{ t('message.gameBusiness.mailCampaign.publishWarning') }}</span>
      </div>
    </div>
    <template #footer>
      <Button :label="t('message.common.button.cancel')" text @click="close"/>
      <Button :label="t('message.gameBusiness.mailCampaign.publish')" icon="fa-solid fa-paper-plane" severity="success" :loading="loading" @click="confirm"/>
    </template>
  </Dialog>
</template>

<script lang="ts" setup name="gameMailCampaignPublishDialog">
import { ref } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import { t } from '../../../../../../i18n/index.ts'
import { showSuccess } from '../../../../../../utils/message.ts'
import { tenantGameMailCampaignEntityPreviewPost, tenantGameMailCampaignEntityPublishPost } from '../../../../../../api/tenantGameMailCampaignEntity'
import {
  MAIL_CAMPAIGN_MAIL_TYPE_OPTIONS,
  MAIL_CAMPAIGN_PUBLISH_STATUS_OPTIONS,
} from '../constants'

const emit = defineEmits(['done'])

const visible = ref(false)
const loading = ref(false)
const previewLoading = ref(false)
const row = ref<any>(null)
const hitCountText = ref('')

const mailTypeDesc = (v: number) => {
  const item = MAIL_CAMPAIGN_MAIL_TYPE_OPTIONS.find((i) => i.value === v)
  return item ? t(item.labelKey) : v
}
const statusDesc = (v: number) => {
  const item = MAIL_CAMPAIGN_PUBLISH_STATUS_OPTIONS.find((i) => i.value === v)
  return item ? t(item.labelKey) : v
}

const loadPreview = async () => {
  if (!row.value?.id) return
  previewLoading.value = true
  try {
    const res: any = await tenantGameMailCampaignEntityPreviewPost({ id: row.value.id })
    const hit = res.data?.data?.estimatedHitCount
    hitCountText.value = hit === -1 || hit === undefined
      ? t('message.gameBusiness.mailCampaign.estimatedHitCountUnknown')
      : `${t('message.gameBusiness.mailCampaign.estimatedHitCountLabel')}: ${hit}`
  } catch {
    // 全局拦截器已弹错误提示
  } finally {
    previewLoading.value = false
  }
}

const close = () => {
  visible.value = false
  row.value = null
  hitCountText.value = ''
}

const confirm = async () => {
  if (!row.value?.id) return
  loading.value = true
  try {
    await tenantGameMailCampaignEntityPublishPost({ id: row.value.id })
    showSuccess(t('message.gameBusiness.mailCampaign.publishSuccess'))
    emit('done')
    close()
  } catch {
    // 全局拦截器已弹错误提示；后端已将状态置 Failed（含网络级失败），刷新列表让失败原因可见
    emit('done')
  } finally {
    loading.value = false
  }
}

const openDialog = (target: any) => {
  row.value = target
  hitCountText.value = ''
  visible.value = true
  loadPreview()
}

defineExpose({ openDialog })
</script>
