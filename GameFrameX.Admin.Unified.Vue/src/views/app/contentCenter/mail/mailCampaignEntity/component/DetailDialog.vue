<template>
  <Dialog v-model:visible="visible" modal :header="t('message.gameBusiness.mailCampaign.detailTitle')" :draggable="false" class="w-full md:w-50rem">
    <div v-block-loading="loading">
      <div v-if="row" class="grid grid-cols-2" style="gap: 0.5rem 1.5rem;">
        <DetailItem :label="t('message.gameBusiness.appContext.currentApp')">{{ appName(row.appId) }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.mailCampaign.areaId')">{{ row.areaId }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.mailCampaign.name')">{{ row.name }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.mailCampaign.mailType')">
          <Tag :value="mailTypeDesc(row.mailType)"/>
        </DetailItem>
        <DetailItem :label="t('message.gameBusiness.mailCampaign.publishStatus')">
          <Tag :value="statusDesc(row.publishStatus)" :severity="statusSeverity(row.publishStatus)"/>
        </DetailItem>
        <DetailItem :label="t('message.gameBusiness.mailCampaign.template')">{{ row.templateId && row.templateId > 0 ? row.templateId : '-' }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.mailCampaign.expirePolicy')">{{ expirePolicyDesc(row.expireAttachmentPolicy) }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.mailCampaign.expireTime')">{{ formatUnix(row.expireTime) }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.mailCampaign.minLevel')">{{ row.minLevel ?? 0 }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.mailCampaign.maxLevel')">{{ row.maxLevel ?? 0 }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.mailCampaign.serverCampaignId')">{{ row.serverCampaignId && row.serverCampaignId > 0 ? row.serverCampaignId : '-' }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.mailCampaign.publishVersion')">{{ row.publishVersion && row.publishVersion > 0 ? row.publishVersion : '-' }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.mailCampaign.publishOperator')">{{ row.publishOperator || '-' }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.mailCampaign.publishedTime')">{{ formatUnix(row.publishedTime) }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.mailCampaign.revokeOperator')">{{ row.revokeOperator || '-' }}</DetailItem>
        <DetailItem :label="t('message.gameBusiness.mailCampaign.revokedTime')">{{ formatUnix(row.revokedTime) }}</DetailItem>

        <div style="grid-column: 1 / -1">
          <div class="text-color-secondary text-sm mb-1">{{ t('message.gameBusiness.mailCampaign.serverIds') }}</div>
          <code class="text-sm break-all">{{ prettyIds(row.serverIdsJson) }}</code>
        </div>
        <div style="grid-column: 1 / -1">
          <div class="text-color-secondary text-sm mb-1">{{ t('message.gameBusiness.mailCampaign.channelIds') }}</div>
          <code class="text-sm break-all">{{ prettyIds(row.channelIdsJson) }}</code>
        </div>

        <div style="grid-column: 1 / -1">
          <div class="text-color-secondary text-sm mb-1">{{ t('message.gameBusiness.mailCampaign.titles') }}</div>
          <LocalizedListPreview :value="row.titlesJson"/>
        </div>
        <div style="grid-column: 1 / -1">
          <div class="text-color-secondary text-sm mb-1">{{ t('message.gameBusiness.mailCampaign.senderNames') }}</div>
          <LocalizedListPreview :value="row.senderNamesJson"/>
        </div>
        <div style="grid-column: 1 / -1">
          <div class="text-color-secondary text-sm mb-1">{{ t('message.gameBusiness.mailCampaign.contents') }}</div>
          <LocalizedListPreview :value="row.contentsJson" :block="true"/>
        </div>

        <div style="grid-column: 1 / -1">
          <div class="text-color-secondary text-sm mb-1">{{ t('message.gameBusiness.mailCampaign.attachments') }}</div>
          <AttachmentPreview :value="row.attachmentsJson"/>
        </div>

        <div v-if="row.lastServerCode !== undefined && row.lastServerCode !== null" style="grid-column: 1 / -1">
          <div class="text-color-secondary text-sm mb-1">{{ t('message.gameBusiness.mailCampaign.lastServerCode') }}</div>
          <code class="text-sm break-all">{{ row.lastServerCode }}</code>
        </div>
        <div v-if="row.lastServerMessage" style="grid-column: 1 / -1">
          <div class="text-color-secondary text-sm mb-1">{{ t('message.gameBusiness.mailCampaign.failureReason') }}</div>
          <div class="text-red-500 break-all">{{ row.lastServerMessage }}</div>
        </div>
        <div v-if="row.remark" style="grid-column: 1 / -1">
          <div class="text-color-secondary text-sm mb-1">{{ t('message.gameBusiness.mailCampaign.remark') }}</div>
          <div class="break-all">{{ row.remark }}</div>
        </div>
        <div style="grid-column: 1 / -1">
          <div class="text-color-secondary text-sm mb-1">{{ t('message.gameBusiness.mailCampaign.rowVersion') }}</div>
          <code class="text-sm break-all">{{ row.rowVersion || '-' }}</code>
        </div>
        <div style="grid-column: 1 / -1">
          <div class="text-color-secondary text-sm mb-1">{{ t('common.label.createTime') }}</div>
          <div>{{ formatUnix(row.createdTime) }}</div>
        </div>
      </div>
    </div>
    <template #footer>
      <Button :label="t('message.common.button.close')" text @click="visible = false"/>
    </template>
  </Dialog>
</template>

<script lang="ts" setup name="gameMailCampaignDetailDialog">
import { ref, h, defineComponent } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import { t } from '../../../../../../i18n/index.ts'
import { useCurrentApp } from '../../../../../../composables/useCurrentApp.ts'
import {
  MAIL_CAMPAIGN_MAIL_TYPE_OPTIONS,
  MAIL_CAMPAIGN_EXPIRE_POLICY_OPTIONS,
  MAIL_CAMPAIGN_PUBLISH_STATUS_OPTIONS,
  MAIL_CAMPAIGN_PUBLISH_STATUS_SEVERITY,
} from '../constants'

// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const DetailItem = defineComponent({
  name: 'MailCampaignDetailItem',
  props: { label: { type: String, default: '' } },
  setup(props, { slots }) {
    return () =>
      h('div', { class: 'min-w-0' }, [
        h('div', { class: 'text-color-secondary text-sm mb-1' }, props.label),
        h('div', { class: 'break-all' }, slots.default?.()),
      ])
  },
})

// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const LocalizedListPreview = defineComponent({
  name: 'MailCampaignLocalizedPreview',
  props: { value: { type: [String, Array], default: '' }, block: { type: Boolean, default: false } },
  setup(props) {
    return () => {
      const arr = safeParseArray(props.value)
      if (arr.length === 0) return h('div', { class: 'text-color-secondary text-sm' }, '-')
      return h(
        'div',
        { class: 'flex flex-column gap-1' },
        arr.map((item: any, idx: number) =>
          h(
            'div',
            { key: idx, class: props.block ? 'break-all' : 'break-all flex align-items-center gap-2' },
            [
              h(Tag, { value: item.language ?? '', severity: 'info' }),
              h('span', { class: 'text-sm break-all' }, item.text ?? ''),
            ]
          )
        )
      )
    }
  },
})

// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const AttachmentPreview = defineComponent({
  name: 'MailCampaignAttachmentPreview',
  props: { value: { type: [String, Array], default: '' } },
  setup(props) {
    return () => {
      const arr = safeParseArray(props.value)
      if (arr.length === 0) return h('div', { class: 'text-color-secondary text-sm' }, '-')
      return h(
        'div',
        { class: 'flex flex-column gap-1' },
        arr.map((item: any, idx: number) =>
          h(
            'div',
            { key: idx, class: 'text-sm break-all' },
            `slotId=${item.slotId} rewardType=${item.rewardType} itemId=${item.itemId} amount=${item.amount}${item.iconId ? ` iconId=${item.iconId}` : ''}`
          )
        )
      )
    }
  },
})

const { appName } = useCurrentApp()

const visible = ref(false)
const loading = ref(false)
const row = ref<any>(null)

const safeParseArray = (raw: any): any[] => {
  if (Array.isArray(raw)) return raw
  if (typeof raw === 'string' && raw.trim()) {
    try {
      const parsed = JSON.parse(raw)
      return Array.isArray(parsed) ? parsed : []
    } catch {
      return []
    }
  }
  return []
}

const prettyIds = (raw: any): string => {
  const arr = safeParseArray(raw)
  if (arr.length === 0) return '-'
  return arr.map((n: any) => String(n)).join(', ')
}

const mailTypeDesc = (v: number) => {
  const item = MAIL_CAMPAIGN_MAIL_TYPE_OPTIONS.find((i) => i.value === v)
  return item ? t(item.labelKey) : v
}
const statusDesc = (v: number) => {
  const item = MAIL_CAMPAIGN_PUBLISH_STATUS_OPTIONS.find((i) => i.value === v)
  return item ? t(item.labelKey) : v
}
const statusSeverity = (v: number) => MAIL_CAMPAIGN_PUBLISH_STATUS_SEVERITY[v] ?? 'info'
const expirePolicyDesc = (v: number) => {
  const item = MAIL_CAMPAIGN_EXPIRE_POLICY_OPTIONS.find((i) => i.value === v)
  return item ? t(item.labelKey) : v
}

const formatUnix = (v?: number) => {
  if (!v || v <= 0) return '-'
  const d = new Date(v * 1000)
  return Number.isNaN(d.getTime()) ? '-' : d.toLocaleString()
}

const openDialog = (target: any) => {
  visible.value = true
  row.value = target
  // 后端 detail 接口一期可能未实现，直接用列表行数据
  loading.value = false
}

defineExpose({ openDialog })
</script>
