<template>
  <Dialog v-model:visible="visible" modal :header="t('message.gameBusiness.advertisementReward.detailTitle')" :draggable="false" class="w-full md:w-40rem">
    <div v-block-loading="loading">
      <div v-if="row" class="grid grid-cols-2" style="gap: 0.5rem 1.5rem;">
        <DetailItem :label="t('message.gameBusiness.advertisementReward.app')">{{ appName(row.appId) }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.advertisementReward.provider')">{{ row.provider || '-' }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.advertisementReward.status')">
          <Tag :value="statusDesc(row.status)" :severity="adRewardStatusSeverity(row.status)" />
        </DetailItem>
        <DetailItem :label="t('message.gameBusiness.advertisementReward.isSandBox')">
          <Tag v-if="row.isSandBox" :value="t('message.gameBusiness.advertisementReward.sandboxTag')" severity="warn" />
          <Tag v-else :value="t('message.gameBusiness.advertisementReward.productionTag')" severity="secondary" />
        </DetailItem>
        <DetailItem :label="t('message.gameBusiness.advertisementReward.transactionId')">{{ row.transactionId || '-' }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.advertisementReward.advertisementUnit')">{{ row.advertisementUnit || '-' }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.advertisementReward.roleId')">{{ row.roleId }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.advertisementReward.userId')">{{ row.userId }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.advertisementReward.areaId')">{{ row.areaId }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.advertisementReward.rewardItem')">{{ row.rewardItem || '-' }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.advertisementReward.rewardAmount')">{{ row.rewardAmount || '-' }}</DetailItem>
        <div style="grid-column: 1 / -1">
          <div class="text-color-secondary text-sm mb-1">{{ t('message.gameBusiness.advertisementReward.idempotencyKey') }}</div>
          <code class="text-sm break-all">{{ row.idempotencyKey }}</code>
        </div>
        <div v-if="row.failureReason" style="grid-column: 1 / -1">
          <div class="text-color-secondary text-sm mb-1">{{ t('message.gameBusiness.advertisementReward.failureReason') }}</div>
          <div class="text-red-500 break-all">{{ row.failureReason }}</div>
        </div>
        <div style="grid-column: 1 / -1">
          <div class="text-color-secondary text-sm mb-1">{{ t('common.label.createTime') }}</div>
          <div>{{ row.createdTime }}</div>
        </div>
      </div>
    </div>
    <template #footer>
      <Button :label="t('message.common.button.close')" text @click="visible = false" />
    </template>
  </Dialog>
</template>

<script lang="ts" setup name="tenantGameAdRewardDetailDialog">
import { ref, h, defineComponent } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import { t } from '../../../../../i18n/index.ts'
import { useCurrentApp } from '../../../../../composables/useCurrentApp.ts'
import { tenantGameAdvertisementRewardEntityDetailGet } from '../../../../../api/tenantGameAdvertisementRewardEntity'
import {
  ADVERTISEMENT_REWARD_STATUS_OPTIONS,
  adRewardStatusSeverity,
} from '../constants'

// 内联只读字段渲染（避免依赖外部 Field 组件）
// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const DetailItem = defineComponent({
  name: 'AdRewardDetailItem',
  props: { label: { type: String, default: '' } },
  setup(props, { slots }) {
    return () =>
      h('div', { class: 'min-w-0' }, [
        h('div', { class: 'text-color-secondary text-sm mb-1' }, props.label),
        h('div', { class: 'break-all' }, slots.default?.()),
      ])
  },
})

const { appName } = useCurrentApp()

const visible = ref(false)
const loading = ref(false)
const row = ref<any>(null)

const statusDesc = (v: number) => {
  const item = ADVERTISEMENT_REWARD_STATUS_OPTIONS.find((i) => i.value === v)
  return item ? t(`message.gameBusiness.advertisementReward.${item.i18nKey}`) : v
}

const openDialog = async (target: any) => {
  visible.value = true
  row.value = target
  if (target?.id) {
    loading.value = true
    try {
      const res: any = await tenantGameAdvertisementRewardEntityDetailGet({ id: target.id })
      if (res.data?.data) {
        row.value = res.data.data
      }
    } catch {
      // 保留传入的行数据
    } finally {
      loading.value = false
    }
  }
}

defineExpose({ openDialog })
</script>
