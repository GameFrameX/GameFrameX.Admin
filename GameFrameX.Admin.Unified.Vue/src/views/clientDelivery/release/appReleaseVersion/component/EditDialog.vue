<template>
  <BaseEditDialog ref="dialogRef" :title="props.title" :addApi="gameAppReleaseVersionAddPost" :updateApi="gameAppReleaseVersionUpdatePost" :rules="rules" :defaultForm="() => ({ isForce: false })" :beforeOpen="beforeOpen" scrollable @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm, isEditor }">
      <FormLabel name="channel">{{ t('message.client.channelName') }}</FormLabel>
      <FormField name="channel" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.channel" :options="querySelections.channel" optionLabel="label" optionValue="value" showClear filter :placeholder="t('message.client.pleaseSelectChannelPlaceholder')" :disabled="isEditor" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="oldVersion">{{ t('message.client.oldVersion') }}</FormLabel>
      <FormField name="oldVersion" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.oldVersion" :options="querySelections.oldVersion" optionLabel="label" optionValue="value" showClear filter :placeholder="t('message.client.pleaseSelectOldVersionPlaceholder')" :disabled="isEditor" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="platform">{{ t('message.client.platform') }}</FormLabel>
      <FormField name="platform" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.platform" :options="querySelections.platform" optionLabel="label" optionValue="value" showClear filter :placeholder="t('message.common.form.pleaseSelect')" :disabled="isEditor" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="package">{{ t('message.client.packageName') }}</FormLabel>
      <FormField name="package" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.package" :options="ensureValueOption(querySelections.package, ruleForm.package)" optionLabel="label" optionValue="value" showClear filter :placeholder="t('message.common.form.pleaseSelect')" :disabled="isEditor" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="language">{{ t('message.client.languageName') }}</FormLabel>
      <FormField name="language" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.language" :options="querySelections.language" optionLabel="label" optionValue="value" showClear filter :placeholder="t('message.common.form.pleaseSelect')" :disabled="isEditor" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="appVersion">{{ t('message.client.newVersion') }}</FormLabel>
      <FormField name="appVersion" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.appVersion" :options="querySelections.version" optionLabel="label" optionValue="value" showClear filter :placeholder="t('message.common.form.pleaseSelect')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <TextField v-model="ruleForm.downloadUrl" name="downloadUrl" :label="t('message.client.downloadUrl')" :placeholder="t('message.client.pleaseInputDownloadUrl')" maxlength="256"/>
      <FormLabel>{{ t('message.client.isForceUpdate') }}</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.isForce"/></div>
      <TextField v-model="ruleForm.title" name="title" :label="t('message.client.announcementTitle')" :placeholder="t('message.client.pleaseInputAnnouncement')" maxlength="20"/>
      <TextField v-model="ruleForm.description" name="description" :label="t('message.common.label.description')" :placeholder="t('message.common.form.pleaseInput')" maxlength="512"/>
      <TextareaField v-model="ruleForm.announcement" name="announcement" :label="t('message.client.announcementContent')" :placeholder="t('message.client.pleaseInputAnnouncement')" maxlength="4096" :autoResize="true" rows="3"/>
    </template>
  </BaseEditDialog>
</template>
<script lang="ts" setup>
import {ref} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField, TextareaField} from '../../../../../components/crud-form-fields'
import { gameAppReleaseVersionAddPost, gameAppReleaseVersionUpdatePost } from '../../../../../api/gameAppReleaseVersion'
import { globalPlatformSelectPost } from '../../../../../api/globalPlatform'
import { globalLanguageSelectPost } from '../../../../../api/globalLanguage'
import { gameAppVersionDefinitionSelectPost } from '../../../../../api/gameAppVersionDefinition'
import { gameAppPackageListGet } from '../../../../../api/gameAppPackage'
import { channelDefinitionAvailableGet } from '../../../../../api/channelDefinition'
import { mapAvailableChannelDefinitions } from '../../../../../utils/channelDefinition.ts'
import {MAX_SELECT_SIZE} from '../../../../../constants/pagination.ts'

var props = defineProps({
  title: {
    type: String,
    default: '',
  },
})
const emit = defineEmits(['reloadTable'])

const dialogRef = ref()
const querySelections = ref<any>({
  platform: [],
  language: [],
  channel: [],
  app: [],
})

// 编辑态历史包名可能已不在当前应用启用列表，注入兜底选项使其只读回显，不静默替换。
const ensureValueOption = (options: any[], value: any) => {
  if (value === null || value === undefined || value === '') {
    return options
  }
  const list = options || []
  if (list.some((o: any) => o?.value === value)) {
    return list
  }
  return [{label: String(value), value}, ...list]
}

const rules = {
  channel: [{required: true, message: () => t('message.client.pleaseInputChannelPlaceholder'), trigger: 'change'}],
  announcement: [{required: true, message: () => t('message.client.pleaseInputAnnouncement'), trigger: 'blur'}],
  oldVersion: [{required: true, message: () => t('message.client.pleaseSelectOldVersion'), trigger: 'change'}],
  downloadUrl: [{required: true, message: () => t('message.client.pleaseInputDownloadUrlAlt'), trigger: 'blur'}],
  platform: [{required: true, message: () => t('message.client.pleaseSelectPlatformAlt'), trigger: 'change'}],
  appVersion: [{required: true, message: () => t('message.client.pleaseSelectVersionAlt'), trigger: 'change'}],
  package: [{required: true, message: () => t('message.client.pleaseSelectPackageAlt'), trigger: 'change'}],
  language: [{required: true, message: () => t('message.client.pleaseSelectLanguageAlt'), trigger: 'change'}],
}

const onLanguageQuery = async () => {
  const res = await globalLanguageSelectPost({pageSize: MAX_SELECT_SIZE})
  querySelections.value.language = res.data.data
}

const onChannelQuery = async () => {
  const res = await channelDefinitionAvailableGet({pageSize: MAX_SELECT_SIZE})
  querySelections.value.channel = mapAvailableChannelDefinitions(res.data.data || [], 'code')
}

const onPlatformQuery = async () => {
  const res = await globalPlatformSelectPost({pageSize: MAX_SELECT_SIZE})
  querySelections.value.platform = res.data.data
}

const onVersionQuery = async () => {

  const res = await gameAppVersionDefinitionSelectPost({pageSize: MAX_SELECT_SIZE})
  querySelections.value.version = res.data.data
  querySelections.value.oldVersion = res.data.data
}

// 包名候选按当前应用加载，置顶 default 应用默认选项。
const buildPackageOptions = (packages: any[]) => {
  const options: any[] = [{label: t('message.client.appDefaultPackage'), value: 'default'}]
  for (const p of packages || []) {
    const val = p.name ?? p.packageName
    if (val === null || val === undefined || val === '') {
      continue
    }
    if (options.some((o) => o.value === val)) {
      continue
    }
    const version = p.packageVersion ? ` (${p.packageVersion})` : ''
    options.push({label: `${p.packageName || p.name}${version}`, value: val})
  }
  return options
}

const onPackageQuery = async () => {


  const res = await gameAppPackageListGet({pageSize: MAX_SELECT_SIZE})
  querySelections.value.package = buildPackageOptions(res.data?.data || [])
}

const beforeOpen = (form: any, isEditor: boolean) => {


  return form
}

// 切换应用：清空包名并按新应用重载候选，避免保留其它应用的包。

const openDialog = (row: any) => {
  dialogRef.value.openDialog(row)
  // 编辑态按记录应用加载包名，使历史包名能命中候选。
  onPackageQuery()
  onVersionQuery()
}

defineExpose({openDialog})
onLanguageQuery()
onPackageQuery()
onPlatformQuery()
onChannelQuery()
onVersionQuery()
</script>
