<template>
  <BaseEditDialog ref="dialogRef" :title="props.title"
    :addApi="(params: any) => tenantGameAnnouncementEntityAddPost(params)"
    :updateApi="(params: any) => tenantGameAnnouncementEntityUpdatePost(params)"
    :rules="rules" :beforeOpen="beforeOpen" :beforeSubmit="beforeSubmit" :defaultForm="defaultForm" scrollable @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm, isEditor }">
      <FormLabel name="language">{{ t('message.gameBusiness.language') }}</FormLabel>
      <FormField name="language" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.language" :options="languageOptions" optionLabel="label" optionValue="value" filter :placeholder="t('message.gameBusiness.pleaseSelectLanguage')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="areaId">{{ t('message.gameBusiness.mail.areaId') }}</FormLabel>
      <FormField name="areaId" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.areaId" :options="areaIdOptions" optionLabel="value" optionValue="id" showClear filter :placeholder="t('message.gameBusiness.announcement.allAreas')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="startTime">{{ t('message.gameBusiness.startTime') }}</FormLabel>
      <FormField name="startTime" v-slot="{ props: fProps, invalid, error }">
        <DatePicker v-bind="fProps" v-model="ruleForm.startTime" showTime class="w-full" :placeholder="t('message.gameBusiness.startTime')"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="endTime">{{ t('message.gameBusiness.endTime') }}</FormLabel>
      <FormField name="endTime" v-slot="{ props: fProps, invalid, error }">
        <DatePicker v-bind="fProps" v-model="ruleForm.endTime" showTime class="w-full" :placeholder="t('message.gameBusiness.endTime')"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="priority">{{ t('message.gameBusiness.priority') }}</FormLabel>
      <FormField name="priority" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.priority" :placeholder="t('message.gameBusiness.pleaseEnterPriority')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel>{{ t('message.gameBusiness.isActive') }}</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.isActive"/></div>
      <TextField v-model="ruleForm.title" name="title" :label="t('message.gameBusiness.mail.title')" :placeholder="t('message.gameBusiness.mail.pleaseEnterTitle')" maxlength="256"/>
      <TextareaField v-model="ruleForm.content" name="content" :label="t('message.gameBusiness.announcement.content')" :placeholder="t('message.gameBusiness.announcement.pleaseEnterContent')" :autoResize="true" rows="3" maxlength="2048"/>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref, onMounted} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField, TextareaField} from '../../../../../../components/crud-form-fields'
import { tenantGameAnnouncementEntityAddPost, tenantGameAnnouncementEntityUpdatePost } from '../../../../../../api/tenantGameAnnouncementEntity'
import { tenantGameAreaEntitySelectStringPost } from '../../../../../../api/tenantGameAreaEntity'
import { globalLanguageSelectPost } from '../../../../../../api/globalLanguage'
import {normalizeAppId} from '../../../../../../utils/appWorkspace.ts'
import {MAX_SELECT_SIZE} from '../../../../../../constants/pagination.ts'

const props = defineProps({
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

const areaIdOptions = ref<any[]>([])
const languageOptions = ref<any[]>([])
const defaultLanguageOption = {value: 'default', label: t('message.gameBusiness.announcement.defaultLanguage')}
const defaultForm = () => ({language: 'default', areaId: null, isActive: true, priority: 0})

const rules = {
  language: [{required: true, message: t('message.gameBusiness.pleaseSelectLanguage'), trigger: 'change'}],
  title: [{required: true, message: t('message.gameBusiness.mail.pleaseEnterTitle'), trigger: 'blur'}],
  content: [{required: true, message: t('message.gameBusiness.announcement.pleaseEnterContent'), trigger: 'blur'}],
  priority: [{required: true, message: t('message.gameBusiness.pleaseEnterPriority'), trigger: 'blur'}],
  startTime: [{required: true, message: t('message.gameBusiness.pleaseSelectStartTime'), trigger: 'blur'}],
  endTime: [{required: true, message: t('message.gameBusiness.pleaseSelectEndTime'), trigger: 'blur'}],
}

const loadAreaOptions = async (appId?: number) => {
  areaIdOptions.value = (await tenantGameAreaEntitySelectStringPost({appId: appId, pageSize: MAX_SELECT_SIZE})).data.data ?? []
}

const beforeOpen = (form: any, isEditor: boolean) => {
  const appId = normalizeAppId(props.appId)
  if (appId) form.appId = appId
  if (form.appId) {
    loadAreaOptions(form.appId)
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

onMounted(async () => {
  const res = await globalLanguageSelectPost({pageSize: MAX_SELECT_SIZE})
  const languages = (res.data.data ?? []).map((item: any) => ({
    value: item.value,
    label: item.label ? `${item.label} (${item.value})` : item.value,
  }))
  languageOptions.value = [defaultLanguageOption, ...languages.filter((item: any) => item.value !== 'default')]
})

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>