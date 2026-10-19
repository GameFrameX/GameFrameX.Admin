<template>
  <BaseEditDialog ref="dialogRef" :title="props.title"
    :addApi="tenantGameRedeemCodeEntityAddPost"
    :updateApi="tenantGameRedeemCodeEntityUpdatePost"
    :rules="rules"
    :beforeOpen="beforeOpen" :beforeSubmit="beforeSubmit"
    scrollable @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm, isEditor }">
      <FormLabel name="areaId">{{ t('message.gameBusiness.redeemcode.areaIdNote') }}</FormLabel>
      <FormField name="areaId" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.areaId" :options="areaIdOptions" optionLabel="label" optionValue="id" :placeholder="t('message.gameBusiness.mail.pleaseSelectAreaId')" showClear filter class="w-full" :disabled="isEditor"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <TextField v-model="ruleForm.code" name="code" :label="t('message.gameBusiness.redeemcode.code')" :placeholder="t('message.gameBusiness.redeemcode.pleaseEnterCode')" maxlength="255" :disabled="isEditor"/>
      <FormLabel>{{ t('message.gameBusiness.isActive') }}</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.isActive" :disabled="isEditor"/></div>
      <FormLabel name="startTime">{{ t('message.gameBusiness.redeemcode.validStartTime') }}</FormLabel>
      <FormField name="startTime" v-slot="{ props: fProps, invalid, error }">
        <DatePicker v-bind="fProps" v-model="ruleForm.startTime" showTime class="w-full" :placeholder="t('message.gameBusiness.redeemcode.validStartTime')" :disabled="isEditor"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="channelId">{{ t('message.gameBusiness.redeemcode.channelId') }}</FormLabel>
      <FormField name="channelId" v-slot="{ props: fProps, invalid, error }">
        <Select filter v-bind="fProps" v-model="ruleForm.channelId" :options="redeemCodeChannelOptions" optionLabel="label" optionValue="id" :placeholder="t('message.gameBusiness.redeemcode.pleaseSelectChannelId')" showClear class="w-full" :disabled="isEditor"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <TextField v-model="ruleForm.description" name="description" :label="t('message.gameBusiness.redeemcode.description')" :placeholder="t('message.gameBusiness.redeemcode.pleaseEnterDescription')" maxlength="255" :disabled="isEditor"/>
      <FormLabel name="usageLimit">{{ t('message.gameBusiness.redeemcode.usageLimit') }}</FormLabel>
      <FormField name="usageLimit" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.usageLimit" :min="1" :max="99999999" class="w-full" :placeholder="t('message.gameBusiness.redeemcode.pleaseEnterUsageLimit')" :disabled="isEditor"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="endTime">{{ t('message.gameBusiness.redeemcode.invalidTime') }}</FormLabel>
      <FormField name="endTime" v-slot="{ props: fProps, invalid, error }">
        <DatePicker v-bind="fProps" v-model="ruleForm.endTime" showTime class="w-full" :placeholder="t('message.gameBusiness.redeemcode.invalidTime')" :disabled="isEditor"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel>{{ t('message.gameBusiness.rewardTemplate.copySnapshot') }}</FormLabel>
      <div>
        <Select v-model="selectedRewardTemplateId" :options="rewardTemplateList" optionLabel="value" optionValue="id"
                :placeholder="t('message.gameBusiness.rewardTemplate.selectTemplateCopyRewards')" showClear filter class="w-full"
                :disabled="isEditor || rewardTemplateList.length === 0"
                @change="onRewardTemplateChange"/>
      </div>
      <FormLabel name="contents">{{ t('message.gameBusiness.redeemcode.giftContent') }}</FormLabel>
      <FormField name="contents" v-slot="{ invalid, error }">
        <RewardItemsEditor v-model="ruleForm.contents" :appId="ruleForm.appId" :disabled="isEditor"
                           :rewardPlaceholder="t('message.gameBusiness.mail.pleaseSelectAttachment')"
                           :addLabel="t('message.gameBusiness.mail.addAttachment')"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref, onMounted} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import RewardItemsEditor from '/@/components/reward-items-editor/index.vue'
import {TextField} from '../../../../../components/crud-form-fields'
import {parseRewardItems, toRewardItemsJson} from '../../../../../components/reward-items-editor/rewardItems.ts'
import { tenantGameRedeemCodeEntityAddPost, tenantGameRedeemCodeEntityUpdatePost } from '../../../../../api/tenantGameRedeemCodeEntity'
import { tenantGameAreaEntitySelectStringPost } from '../../../../../api/tenantGameAreaEntity'
import { tenantGameRedeemCodeChannelEntitySelectStringPost } from '../../../../../api/tenantGameRedeemCodeChannelEntity'
import {MAX_SELECT_SIZE} from '../../../../../constants/pagination.ts'
import { tenantGameRewardTemplateEntityCopySnapshotPost, tenantGameRewardTemplateEntitySelectPost } from '../../../../../api/tenantGameRewardTemplateEntity'
import {normalizeAppId, type AppWorkspaceId} from '../../../../../utils/appWorkspace.ts'

interface ListItem {
  id: number
  value: string
  label: string
}

var props = defineProps({
  title: {
    type: String,
    default: '',
  },
  appId: {
    type: [Number, String],
    default: undefined,
  },
})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()

const areaIdOptions = ref<ListItem[]>([])
const redeemCodeChannelOptions = ref<any>([])
const rewardTemplateList = ref<any[]>([])
const selectedRewardTemplateId = ref<number | undefined>(undefined)

const rules = {
  code: [{required: true, message: t('message.gameBusiness.redeemcode.pleaseEnterCode'), trigger: 'blur'}],
  description: [{required: true, message: t('message.gameBusiness.redeemcode.pleaseEnterDescription'), trigger: 'blur'}],
  usageLimit: [{required: true, message: t('message.gameBusiness.redeemcode.pleaseEnterUsageLimit'), trigger: 'blur'}],
  contents: [{required: true, message: t('message.gameBusiness.redeemcode.giftContent'), trigger: 'blur'}],
  startTime: [{required: true, message: t('message.gameBusiness.redeemcode.validStartTime'), trigger: 'change'}],
  endTime: [
    {required: true, message: t('message.gameBusiness.pleaseSelectExpiryTime'), trigger: 'change'},
    {
      validator: (rule: any, value: any, callback: any) => {
        const ruleForm = dialogRef.value?.ruleForm
        if (!ruleForm) { callback(); return }
        if (value && ruleForm.startTime) {
          const startTime = new Date(ruleForm.startTime).getTime()
          const endTime = new Date(value).getTime()
          const diffMinutes = (endTime - startTime) / (1000 * 60)
          if (endTime <= startTime) {
            callback(new Error(t('message.gameBusiness.redeemcode.endTimeMustAfterStart')))
          } else if (diffMinutes < 1) {
            callback(new Error(t('message.gameBusiness.redeemcode.endTimeMinAfterStart')))
          } else {
            callback()
          }
        } else {
          callback()
        }
      },
      trigger: ['change', 'blur'],
    },
  ],
}

const loadAreaOptions = async (appId?: AppWorkspaceId) => {
  areaIdOptions.value = (await tenantGameAreaEntitySelectStringPost({appId: appId, pageSize: MAX_SELECT_SIZE})).data.data ?? []
}

const loadChannelOptions = async (appId?: AppWorkspaceId) => {
  redeemCodeChannelOptions.value = (await tenantGameRedeemCodeChannelEntitySelectStringPost({appId: appId, pageSize: MAX_SELECT_SIZE})).data.data ?? []
}

const loadRewardTemplateOptions = async (appId?: AppWorkspaceId) => {
  rewardTemplateList.value = appId ? ((await tenantGameRewardTemplateEntitySelectPost({appId, pageSize: MAX_SELECT_SIZE})).data.data ?? []) : []
}

const loadScopedOptions = async (appId?: AppWorkspaceId) => {
  if (!appId) return
  await Promise.all([
    loadAreaOptions(appId),
    loadChannelOptions(appId),
    loadRewardTemplateOptions(appId),
  ])
}

const resolveRewardTemplateId = (value: any) => (value && typeof value === "object" ? (value.value ?? value.id) : value)

const onRewardTemplateChange = async (event?: any) => {
  const ruleForm = dialogRef.value?.ruleForm
  const templateId = resolveRewardTemplateId(event?.value ?? selectedRewardTemplateId.value)
  selectedRewardTemplateId.value = Number.isFinite(templateId) ? templateId : undefined
  if (!templateId || !ruleForm?.appId) return
  const response = await tenantGameRewardTemplateEntityCopySnapshotPost({id: templateId, appId: ruleForm.appId})
  const snapshot = response.data.data
  ruleForm.contents = parseRewardItems(snapshot?.rewardItemsJson)
}

const beforeOpen = (form: any, isEditor: boolean) => {
  selectedRewardTemplateId.value = undefined
  form.contents = parseRewardItems(form.contents)
  const appId = normalizeAppId(props.appId)
  if (appId) form.appId = appId
  if (form.appId) {
    loadScopedOptions(form.appId)
  }
  // 回显：long(秒) → Date 供 DatePicker 显示
  if (form.startTime && typeof form.startTime === 'number') {
    form.startTime = new Date(form.startTime * 1000)
  }
  if (form.endTime && typeof form.endTime === 'number') {
    form.endTime = new Date(form.endTime * 1000)
  }
}

const beforeSubmit = (form: any) => {
  const payload = {
    ...form,
    contents: toRewardItemsJson(form.contents),
  }
  // 提交：Date → long 秒
  if (payload.startTime) {
    payload.startTime = Math.floor(new Date(payload.startTime).getTime() / 1000)
  }
  if (payload.endTime) {
    payload.endTime = Math.floor(new Date(payload.endTime).getTime() / 1000)
  }
  return payload
}

onMounted(async () => {
  await loadScopedOptions(normalizeAppId(props.appId))
})

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>
