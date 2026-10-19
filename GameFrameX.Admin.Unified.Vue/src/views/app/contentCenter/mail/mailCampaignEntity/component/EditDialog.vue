<template>
  <BaseEditDialog ref="dialogRef" :title="props.title" :width="'min(960px, calc(100vw - 32px))'"
    :addApi="(params: any) => tenantGameMailCampaignEntityAddPost(params)"
    :updateApi="(params: any) => tenantGameMailCampaignEntityUpdatePost(params)"
    :rules="rules" :defaultForm="defaultForm"
    :beforeOpen="beforeOpen" :beforeSubmit="beforeSubmit"
    scrollable @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm, isEditor }">
      <FormLabel name="areaId">{{ t('message.gameBusiness.mail.areaId') }}</FormLabel>
      <FormField name="areaId" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.areaId" :options="areaIdOptions" optionLabel="value" optionValue="id" showClear filter :placeholder="t('message.gameBusiness.announcement.allAreas')" class="w-full" :disabled="areaIdOptions.length === 0"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="name">{{ t('message.gameBusiness.mailCampaign.name') }}</FormLabel>
      <FormField name="name" v-slot="{ props: fProps, invalid, error }">
        <InputText v-bind="fProps" v-model="ruleForm.name" :placeholder="t('message.gameBusiness.mailCampaign.namePlaceholder')" maxlength="64" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="mailType">{{ t('message.gameBusiness.mailCampaign.mailType') }}</FormLabel>
      <FormField name="mailType" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.mailType" :options="mailTypeOptions" optionLabel="label" optionValue="value" :placeholder="t('message.gameBusiness.mailCampaign.mailType')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="templateId">{{ t('message.gameBusiness.mailCampaign.template') }}</FormLabel>
      <FormField name="templateId" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.templateId" :min="0" class="w-full" :placeholder="t('message.gameBusiness.mailCampaign.templatePlaceholder')" showButtons/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="expireAttachmentPolicy">{{ t('message.gameBusiness.mailCampaign.expirePolicy') }}</FormLabel>
      <FormField name="expireAttachmentPolicy" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.expireAttachmentPolicy" :options="expirePolicyOptions" optionLabel="label" optionValue="value" :placeholder="t('message.gameBusiness.mailCampaign.expirePolicy')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="expireTime">{{ t('message.gameBusiness.mailCampaign.expireTime') }}</FormLabel>
      <FormField name="expireTime" v-slot="{ props: fProps, invalid, error }">
        <DatePicker v-bind="fProps" v-model="ruleForm.expireTime" showTime showSeconds class="w-full" :placeholder="t('message.gameBusiness.mailCampaign.expireTimePlaceholder')" :minDate="minExpireDate"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <!-- 多语言标题列表 -->
      <FormLabel>{{ t('message.gameBusiness.mailCampaign.titles') }}</FormLabel>
      <div class="localized-list">
        <div v-for="(item, index) in ruleForm.titles" :key="`title-${index}`" class="localized-item">
          <Select v-model="item.language" :options="languageOptions" optionLabel="label" optionValue="value" :placeholder="t('message.gameBusiness.mailCampaign.language')" filter style="width: 160px;"/>
          <InputText v-model="item.text" :placeholder="t('message.gameBusiness.mailCampaign.titlePlaceholder')" maxlength="256" style="flex: 1;"/>
          <Button severity="danger" @click="removeLocalized(ruleForm.titles, index)" style="margin-left: 8px;">
            <i :class="faMinus.className"></i>
          </Button>
        </div>
        <Button @click="addLocalized(ruleForm.titles)" size="small">
          <i :class="faPlus.className" class="mr-1"></i>{{ t('message.gameBusiness.mailCampaign.addTitle') }}
        </Button>
      </div>

      <!-- 多语言发件人名称 -->
      <FormLabel>{{ t('message.gameBusiness.mailCampaign.senderNames') }}</FormLabel>
      <div class="localized-list">
        <div v-for="(item, index) in ruleForm.senderNames" :key="`sender-${index}`" class="localized-item">
          <Select v-model="item.language" :options="languageOptions" optionLabel="label" optionValue="value" :placeholder="t('message.gameBusiness.mailCampaign.language')" filter style="width: 160px;"/>
          <InputText v-model="item.text" :placeholder="t('message.gameBusiness.mailCampaign.senderNamePlaceholder')" maxlength="64" style="flex: 1;"/>
          <Button severity="danger" @click="removeLocalized(ruleForm.senderNames, index)" style="margin-left: 8px;">
            <i :class="faMinus.className"></i>
          </Button>
        </div>
        <Button @click="addLocalized(ruleForm.senderNames)" size="small">
          <i :class="faPlus.className" class="mr-1"></i>{{ t('message.gameBusiness.mailCampaign.addSenderName') }}
        </Button>
      </div>

      <!-- 多语言正文（Textarea） -->
      <FormLabel>{{ t('message.gameBusiness.mailCampaign.contents') }}</FormLabel>
      <div class="localized-list">
        <div v-for="(item, index) in ruleForm.contents" :key="`content-${index}`" class="localized-item localized-item-block">
          <div class="localized-item-row">
            <Select v-model="item.language" :options="languageOptions" optionLabel="label" optionValue="value" :placeholder="t('message.gameBusiness.mailCampaign.language')" filter style="width: 160px;"/>
            <Button severity="danger" @click="removeLocalized(ruleForm.contents, index)" style="margin-left: 8px;">
              <i :class="faMinus.className"></i>
            </Button>
          </div>
          <Textarea v-model="item.text" :placeholder="t('message.gameBusiness.mailCampaign.contentPlaceholder')" :autoResize="true" rows="3" maxlength="2048" style="width: 100%; margin-top: 4px;"/>
        </div>
        <Button @click="addLocalized(ruleForm.contents)" size="small">
          <i :class="faPlus.className" class="mr-1"></i>{{ t('message.gameBusiness.mailCampaign.addContent') }}
        </Button>
      </div>

      <!-- 附件列表 -->
      <FormLabel>{{ t('message.gameBusiness.mailCampaign.attachments') }}</FormLabel>
      <div class="localized-list">
        <div v-for="(item, index) in ruleForm.attachments" :key="`att-${index}`" class="localized-item">
          <InputNumber v-model="item.slotId" :min="0" :placeholder="t('message.gameBusiness.mailCampaign.attachmentSlotId')" style="width: 120px;"/>
          <Select v-model="item.rewardType" :options="rewardTypeOptions" optionLabel="label" optionValue="value" :placeholder="t('message.gameBusiness.mailCampaign.attachmentRewardType')" style="width: 160px;"/>
          <InputNumber v-model="item.itemId" :min="0" :placeholder="t('message.gameBusiness.mailCampaign.attachmentItemId')" style="width: 140px;"/>
          <InputNumber v-model="item.amount" :min="1" :placeholder="t('message.gameBusiness.mailCampaign.attachmentAmount')" style="width: 120px;"/>
          <InputNumber v-model="item.iconId" :min="0" :placeholder="t('message.gameBusiness.mailCampaign.attachmentIconId')" style="width: 120px;" :useGrouping="false"/>
          <Button severity="danger" @click="removeAttachment(index)" style="margin-left: 8px;">
            <i :class="faMinus.className"></i>
          </Button>
        </div>
        <Button @click="addAttachment(ruleForm)" size="small">
          <i :class="faPlus.className" class="mr-1"></i>{{ t('message.gameBusiness.mailCampaign.addAttachment') }}
        </Button>
      </div>

      <!-- 命中范围过滤 -->
      <FormLabel>{{ t('message.gameBusiness.mailCampaign.serverIds') }}</FormLabel>
      <div>
        <InputText v-model="ruleForm.serverIdsText" :placeholder="t('message.gameBusiness.mailCampaign.serverIdsPlaceholder')" class="w-full"/>
        <small class="text-gray-400">{{ t('message.gameBusiness.mailCampaign.idsHint') }}</small>
      </div>
      <FormLabel>{{ t('message.gameBusiness.mailCampaign.channelIds') }}</FormLabel>
      <div>
        <InputText v-model="ruleForm.channelIdsText" :placeholder="t('message.gameBusiness.mailCampaign.channelIdsPlaceholder')" class="w-full"/>
        <small class="text-gray-400">{{ t('message.gameBusiness.mailCampaign.idsHint') }}</small>
      </div>
      <FormLabel name="minLevel">{{ t('message.gameBusiness.mailCampaign.minLevel') }}</FormLabel>
      <FormField name="minLevel" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.minLevel" :min="0" class="w-full" :placeholder="t('message.gameBusiness.mailCampaign.minLevel')"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="maxLevel">{{ t('message.gameBusiness.mailCampaign.maxLevel') }}</FormLabel>
      <FormField name="maxLevel" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.maxLevel" :min="0" class="w-full" :placeholder="t('message.gameBusiness.mailCampaign.maxLevel')"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="remark">{{ t('message.gameBusiness.mailCampaign.remark') }}</FormLabel>
      <FormField name="remark" v-slot="{ props: fProps, invalid, error }">
        <Textarea v-bind="fProps" v-model="ruleForm.remark" :placeholder="t('message.gameBusiness.mailCampaign.remarkPlaceholder')" :autoResize="true" rows="2" maxlength="512" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref, computed} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import { t } from '../../../../../../i18n/index.ts'
import { tenantGameMailCampaignEntityAddPost, tenantGameMailCampaignEntityUpdatePost } from '../../../../../../api/tenantGameMailCampaignEntity'
import { tenantGameAreaEntitySelectStringPost } from '../../../../../../api/tenantGameAreaEntity'
import { faMinus, faPlus } from '../../../../../../utils/icons.ts'
import {normalizeAppId} from '../../../../../../utils/appWorkspace.ts'
import {MAX_SELECT_SIZE} from '../../../../../../constants/pagination.ts'
import {
  MAIL_CAMPAIGN_MAIL_TYPE_OPTIONS,
  MAIL_CAMPAIGN_EXPIRE_POLICY_OPTIONS,
  MAIL_CAMPAIGN_LANGUAGE_OPTIONS,
  MAIL_CAMPAIGN_REWARD_TYPE_OPTIONS,
  buildOptionLabel,
} from '../constants'

const props = defineProps({
  title: { type: String, default: '' },
  appId: { type: [Number, String], default: undefined },
})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()
const areaIdOptions = ref<any[]>([])

const mailTypeOptions = computed(() => buildOptionLabel(MAIL_CAMPAIGN_MAIL_TYPE_OPTIONS, t))
const expirePolicyOptions = computed(() => buildOptionLabel(MAIL_CAMPAIGN_EXPIRE_POLICY_OPTIONS, t))
const rewardTypeOptions = computed(() => buildOptionLabel(MAIL_CAMPAIGN_REWARD_TYPE_OPTIONS, t))
const languageOptions = computed(() => MAIL_CAMPAIGN_LANGUAGE_OPTIONS.map((o) => ({ value: o.value, label: t(o.labelKey) })))

const minExpireDate = computed(() => new Date())

const defaultForm = () => ({
  appId: undefined as number | undefined,
  areaId: undefined as number | undefined,
  name: '',
  mailType: 1,
  templateId: 0,
  titles: [{ language: 'zh-cn', text: '' }] as { language: string; text: string }[],
  contents: [] as { language: string; text: string }[],
  senderNames: [] as { language: string; text: string }[],
  attachments: [] as any[],
  serverIdsText: '',
  channelIdsText: '',
  minLevel: 0,
  maxLevel: 0,
  expireAttachmentPolicy: 0,
  expireTime: undefined as any,
  remark: '',
})

const rules = {
  areaId: [{ required: true, message: () => t('message.gameBusiness.mailCampaign.areaIdRequired'), trigger: 'change' }],
  name: [{ required: true, message: () => t('message.gameBusiness.mailCampaign.nameRequired'), trigger: 'blur' }],
  mailType: [{ required: true, message: () => t('message.gameBusiness.mailCampaign.mailTypeRequired'), trigger: 'change' }],
  expireAttachmentPolicy: [{ required: true, message: () => t('message.gameBusiness.mailCampaign.expirePolicyRequired'), trigger: 'change' }],
}

const loadAreaOptions = async (appId?: number) => {
  if (!appId) {
    areaIdOptions.value = []
    return
  }
  areaIdOptions.value = (await tenantGameAreaEntitySelectStringPost({ appId: appId, pageSize: MAX_SELECT_SIZE })).data.data ?? []
}

// JSON 字符串 → 数组
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

// 逗号/分号/空白分隔字符串 → number[] → 文本框可编辑形态
const parseIdsToText = (raw: any): string => {
  const arr = safeParseArray(raw)
  if (arr.length === 0) return ''
  return arr.map((n: any) => String(n)).join(',')
}

const parseIdsToArray = (text: string): number[] => {
  if (!text) return []
  return text
    .split(/[,;\s]+/)
    .map((s) => s.trim())
    .filter((s) => s.length > 0)
    
    .filter((s) => /^[1-9]\d*$/.test(s))
}

// Date → Unix 秒
const toDate = (v: any): Date | null => {
  if (!v) return null
  if (v instanceof Date) return v
  if (typeof v === 'number') {
    const d = new Date(v * 1000)
    return Number.isNaN(d.getTime()) ? null : d
  }
  return null
}

const beforeOpen = async (form: any, _isEditor: boolean) => {
  const appId = normalizeAppId(props.appId)
  if (appId) form.appId = appId
  if (form.appId) {
    await loadAreaOptions(form.appId)
  }
  form.titles = safeParseArray(form.titlesJson)
  if (form.titles.length === 0) form.titles = [{ language: 'zh-cn', text: '' }]
  form.contents = safeParseArray(form.contentsJson)
  form.senderNames = safeParseArray(form.senderNamesJson)
  form.attachments = safeParseArray(form.attachmentsJson)
  form.serverIdsText = parseIdsToText(form.serverIdsJson)
  form.channelIdsText = parseIdsToText(form.channelIdsJson)
  form.mailType = form.mailType ?? 1
  form.templateId = form.templateId ?? 0
  form.minLevel = form.minLevel ?? 0
  form.maxLevel = form.maxLevel ?? 0
  form.expireAttachmentPolicy = form.expireAttachmentPolicy ?? 0
  form.expireTime = toDate(form.expireTime)
  form.remark = form.remark ?? ''
  form.name = form.name ?? ''
  return form
}

const beforeSubmit = (form: any) => {
  form.titlesJson = JSON.stringify((form.titles ?? []).filter((i: any) => i && i.language && (i.text ?? '') !== ''))
  form.contentsJson = JSON.stringify((form.contents ?? []).filter((i: any) => i && i.language && (i.text ?? '') !== ''))
  form.senderNamesJson = JSON.stringify((form.senderNames ?? []).filter((i: any) => i && i.language && (i.text ?? '') !== ''))
  form.attachmentsJson = JSON.stringify((form.attachments ?? []).filter((i: any) => i && i.slotId !== undefined && i.itemId !== undefined))
  form.serverIdsJson = JSON.stringify(parseIdsToArray(form.serverIdsText))
  form.channelIdsJson = JSON.stringify(parseIdsToArray(form.channelIdsText))
  form.expireTime = form.expireTime instanceof Date ? Math.floor(form.expireTime.getTime() / 1000) : 0
  delete form.titles
  delete form.contents
  delete form.senderNames
  delete form.attachments
  delete form.serverIdsText
  delete form.channelIdsText
  return form
}

const addLocalized = (arr: any[]) => {
  arr.push({ language: 'zh-cn', text: '' })
}
const removeLocalized = (arr: any[], index: number) => {
  arr.splice(index, 1)
}
const addAttachment = (form: any) => {
  form.attachments = form.attachments ?? []
  form.attachments.push({ slotId: form.attachments.length, rewardType: 0, itemId: 0, amount: 1, iconId: 0 })
}
const removeAttachment = (index: number) => {
  const form = dialogRef.value?.ruleForm
  if (!form) return
  form.attachments?.splice(index, 1)
}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>

<style scoped>
.localized-list {
  grid-column: 2 / 3;
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.localized-item {
  display: flex;
  align-items: center;
  gap: 4px;
  flex-wrap: wrap;
}
.localized-item-block {
  flex-direction: column;
  align-items: stretch;
}
.localized-item-row {
  display: flex;
  align-items: center;
}
</style>
