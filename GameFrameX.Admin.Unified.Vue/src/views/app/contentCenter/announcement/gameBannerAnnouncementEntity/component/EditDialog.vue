<template>
  <BaseEditDialog ref="dialogRef" :title="props.title"
    :addApi="tenantGameBannerAnnouncementEntityAddPost"
    :updateApi="tenantGameBannerAnnouncementEntityUpdatePost"
    :rules="rules" scrollable :beforeOpen="beforeOpen" :beforeSubmit="beforeSubmit" @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm, isEditor }">
      <FormLabel name="areaId">{{ t('message.gameBusiness.mail.areaId') }}</FormLabel>
      <FormField name="areaId" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.areaId" :options="areaIdOptions" optionLabel="value" optionValue="id" showClear filter :placeholder="t('message.gameBusiness.mail.pleaseSelectAreaId')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="priority">{{ t('message.gameBusiness.priority') }}</FormLabel>
      <FormField name="priority" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.priority" :placeholder="t('message.gameBusiness.pleaseEnterPriority')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="startTime">{{ t('message.gameBusiness.startTime') }}</FormLabel>
      <FormField name="startTime" v-slot="{ props: fProps, invalid, error }">
        <DatePicker v-bind="fProps" v-model="ruleForm.startTime" showTime class="w-full" :placeholder="t('message.gameBusiness.startTime')"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel>{{ t('message.gameBusiness.isActive') }}</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.isActive"/></div>
      <FormLabel name="interval">{{ t('message.gameBusiness.bannerAnnouncement.interval') }}</FormLabel>
      <FormField name="interval" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.interval" :placeholder="t('message.gameBusiness.bannerAnnouncement.pleaseEnterInterval')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="repeatCount">{{ t('message.gameBusiness.bannerAnnouncement.repeatCount') }}</FormLabel>
      <FormField name="repeatCount" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.repeatCount" :min="1" :placeholder="t('message.gameBusiness.bannerAnnouncement.pleaseEnterRepeatCount')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="endTime">{{ t('message.gameBusiness.endTime') }}</FormLabel>
      <FormField name="endTime" v-slot="{ props: fProps, invalid, error }">
        <DatePicker v-bind="fProps" v-model="ruleForm.endTime" showTime class="w-full" :placeholder="t('message.gameBusiness.endTime')"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <TextareaField v-model="ruleForm.content" name="content" :label="t('message.gameBusiness.announcement.content')" :placeholder="t('message.gameBusiness.announcement.pleaseEnterContent')" :autoResize="true" rows="3" maxlength="2048"/>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextareaField} from '../../../../../../components/crud-form-fields'
import {normalizeAppId} from '../../../../../../utils/appWorkspace.ts'
import { tenantGameBannerAnnouncementEntityAddPost, tenantGameBannerAnnouncementEntityUpdatePost } from '../../../../../../api/tenantGameBannerAnnouncementEntity'
import { tenantGameAreaEntitySelectStringPost } from '../../../../../../api/tenantGameAreaEntity'
import {MAX_SELECT_SIZE} from '../../../../../../constants/pagination.ts'

const props = defineProps({title: {type: String, default: ''}, appId: {type: [Number, String], default: undefined}})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()

interface ListItem {
  id: number
  value: string
  label: string
}

const areaIdOptions = ref<ListItem[]>([])

const rules = {
  areaId: [{required: true, message: t('message.gameBusiness.pleaseSelectArea'), trigger: 'change'}],
  content: [{required: true, message: t('message.gameBusiness.announcement.pleaseEnterContent'), trigger: 'blur'}],
  priority: [{required: true, message: t('message.gameBusiness.pleaseEnterPriority'), trigger: 'blur'}],
  interval: [{required: true, message: t('message.gameBusiness.bannerAnnouncement.pleaseEnterInterval'), trigger: 'blur'}],
  repeatCount: [{required: true, message: t('message.gameBusiness.bannerAnnouncement.pleaseEnterRepeatCount'), trigger: 'blur'}],
}

const loadAreaList = async (appId?: number) => {
  areaIdOptions.value = (await tenantGameAreaEntitySelectStringPost({appId: appId, pageSize: MAX_SELECT_SIZE})).data.data ?? []
}

const beforeOpen = (form: any, isEditor: boolean) => {
  const appId = normalizeAppId(props.appId)
  if (appId) form.appId = appId
  if (form.appId) {
    loadAreaList(form.appId)
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
  const payload = {...form}
  // 提交：Date → long 秒
  if (payload.startTime) {
    payload.startTime = Math.floor(new Date(payload.startTime).getTime() / 1000)
  }
  if (payload.endTime) {
    payload.endTime = Math.floor(new Date(payload.endTime).getTime() / 1000)
  }
  return payload
}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog({
    ...row,
    repeatCount: row?.repeatCount > 0 ? row.repeatCount : 1,
  }),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>
