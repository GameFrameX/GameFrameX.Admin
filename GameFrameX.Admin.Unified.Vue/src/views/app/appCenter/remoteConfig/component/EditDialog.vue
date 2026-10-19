<template>
  <BaseEditDialog ref="dialogRef" class="global-info-edit-dialog" :title="props.title" :addApi="tenantGameAppRemoteConfigAddPost" :updateApi="tenantGameAppRemoteConfigUpdatePost" :rules="rules" :beforeSubmit="beforeSubmit" scrollable @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm, isEditor }">
      <div v-block-loading="loading" class="global-info-edit-dialog-content">
      <FormLabel name="appId">{{ t('message.gameBusiness.app.appId') }}</FormLabel>
      <FormField name="appId" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" filter v-model="ruleForm.appId" :placeholder="t('message.common.form.pleaseSelect')" :disabled="isEditor" :options="querySelections.app" optionLabel="label" optionValue="value" @change="onAppChange(ruleForm)" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="channel">{{ t('message.client.channelName') }}</FormLabel>
      <FormField name="channel" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" clearable filter v-model="ruleForm.channel" :placeholder="t('message.client.pleaseSelectChannelPlaceholder')" :disabled="isEditor" :options="ensureValueOption(querySelections.channel, ruleForm.channel)" optionLabel="label" optionValue="value" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="appVersion">{{ t('message.client.newVersion') }}</FormLabel>
      <FormField name="appVersion" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" clearable filter v-model="ruleForm.appVersion" :placeholder="t('message.common.form.pleaseSelect')" :disabled="isEditor" :options="ensureValueOption(querySelections.version, ruleForm.appVersion)" optionLabel="label" optionValue="value" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="language">{{ t('message.client.languageName') }}</FormLabel>
      <FormField name="language" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" clearable filter v-model="ruleForm.language" :placeholder="t('message.common.form.pleaseSelect')" :disabled="isEditor" :options="ensureValueOption(querySelections.language, ruleForm.language)" optionLabel="label" optionValue="value" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="platform">{{ t('message.client.platform') }}</FormLabel>
      <FormField name="platform" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" clearable filter v-model="ruleForm.platform" :placeholder="t('message.common.form.pleaseSelect')" :disabled="isEditor" :options="ensureValueOption(querySelections.platform, ruleForm.platform)" optionLabel="label" optionValue="value" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="package">{{ t('message.client.packageName') }}</FormLabel>
      <FormField name="package" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" clearable filter v-model="ruleForm.package" :placeholder="t('message.common.form.pleaseSelect')" :disabled="isEditor" :options="ensureValueOption(querySelections.package, ruleForm.package)" optionLabel="label" optionValue="value" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <TextField v-model="ruleForm.checkResourceUrl" name="checkResourceUrl" :label="t('message.client.checkResourceUrl')" :placeholder="t('message.client.pleaseInputCheckResourceUrl')" maxlength="128"/>
      <TextField v-model="ruleForm.checkAppVersionUrl" name="checkAppVersionUrl" :label="t('message.client.checkAppVersionUrlFull')" :placeholder="t('message.client.pleaseInputCheckAppVersionUrl')" maxlength="128"/>

      <TextField v-model="ruleForm.aotCodeList" name="aotCodeList" :label="t('message.client.aotCodeList')" :placeholder="t('message.client.aotCodeListPlaceholder')" maxlength="4096"/>
      <FormLabel>{{ t('message.client.isEnabled') }}</FormLabel>
      <div>
        <ToggleSwitch v-model="ruleForm.isEnabled"/>
      </div>
      <FormLabel name="priority">{{ t('message.client.priority') }}</FormLabel>
      <FormField name="priority" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.priority" :min="0" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="rolloutPercent">{{ t('message.client.rolloutPercent') }}</FormLabel>
      <FormField name="rolloutPercent" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.rolloutPercent" :min="0" :max="100" suffix="%" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <TextareaField v-model="ruleForm.deviceWhitelistJson" name="deviceWhitelistJson" :label="t('message.client.deviceWhitelistJson')" :placeholder="t('message.client.deviceWhitelistJsonPlaceholder')" :autoResize="true" rows="3"/>
      <TextareaField v-model="ruleForm.description" name="description" :label="t('message.client.descriptionInfo')" :placeholder="t('message.common.form.pleaseInput')" maxlength="512" :autoResize="true" rows="3"/>
      </div>
    </template>
  </BaseEditDialog>
</template>
<script lang="ts" setup>
import {ref} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField, TextareaField} from '../../../../../components/crud-form-fields'
import {showError} from '../../../../../utils/message.ts'
import { tenantGameAppRemoteConfigAddPost, tenantGameAppRemoteConfigUpdatePost } from '../../../../../api/tenantGameAppRemoteConfig'
import { tenantGameAppEntityListGet } from '../../../../../api/tenantGameAppEntity'
import { globalPlatformSelectPost } from '../../../../../api/globalPlatform'
import { globalLanguageSelectPost } from '../../../../../api/globalLanguage'
import { tenantGameAppVersionDefinitionSelectPost } from '../../../../../api/tenantGameAppVersionDefinition'
import { tenantGameAppPackageListGet } from '../../../../../api/tenantGameAppPackage'
import { tenantChannelDefinitionAvailableGet } from '../../../../../api/tenantChannelDefinition'
import {normalizeAppId, type AppWorkspaceId} from '../../../../../utils/appWorkspace.ts'
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
const loading = ref(false)

const querySelections = ref<any>({
  platform: [],
  language: [],
  channel: [],
  app: [],
})

// 禁用 Select 在编辑模式下若 options 不含当前值（如渠道仅返回可用项、主数据为空或已删除），
// PrimeVue Select 会回退到占位符。这里在当前值未命中时注入兜底选项，使其始终回显选中值。
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
  checkAppVersionUrl: [{
    required: true,
    message: () => t('message.client.pleaseInputCheckAppVersionUrl'),
    trigger: 'blur'
  }],
  checkResourceUrl: [{
    required: true,
    message: () => t('message.client.pleaseInputCheckResourceUrl'),
    trigger: 'blur'
  }],
  aotCodeList: [{required: true, message: () => t('message.client.pleaseInputAotCodeList'), trigger: 'blur'}],
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
  const res = await tenantChannelDefinitionAvailableGet({pageSize: MAX_SELECT_SIZE})
  querySelections.value.channel = mapAvailableChannelDefinitions(res.data.data || [], 'code')
}

const onPlatformQuery = async () => {
  const res = await globalPlatformSelectPost({pageSize: MAX_SELECT_SIZE})
  querySelections.value.platform = res.data.data
}

const onVersionQuery = async (appId?: AppWorkspaceId) => {
  const id = normalizeAppId(appId)
  if (id === undefined) {
    querySelections.value.version = []
    querySelections.value.oldVersion = []
    return
  }
  const res = await tenantGameAppVersionDefinitionSelectPost({appId: id, pageSize: MAX_SELECT_SIZE})
  querySelections.value.version = res.data.data
  querySelections.value.oldVersion = res.data.data
}

const onPackageQuery = async (appId?: AppWorkspaceId) => {
  const id = normalizeAppId(appId)
  if (id === undefined) {
    querySelections.value.package = []
    return
  }
  const res = await tenantGameAppPackageListGet({appId: id})
  querySelections.value.package = buildPackageOptions(res.data?.data || [])
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

// 切换应用：清空包名并按新应用重载候选，避免保留其它应用的包。
const onAppChange = (ruleForm: any) => {
  if (ruleForm) {
    ruleForm.package = undefined
    ruleForm.version = undefined
    ruleForm.oldVersion = undefined
  }
  onPackageQuery(ruleForm?.appId)
  onVersionQuery(ruleForm?.appId)
}

const onAppQuery = async () => {
  const res = await tenantGameAppEntityListGet({})
  const apps = res.data?.data || []
  const options: any[] = []
  for (const a of apps) {
    const id = normalizeAppId(a.id)
    if (id === undefined || a.isEnabled === false) {
      continue
    }
    options.push({label: a.appName || a.name || String(id), value: id})
  }
  querySelections.value.app = options
}

const openDialog = async (row: any) => {
  dialogRef.value.openDialog(row)
  loading.value = true
  try {
    // 编辑态按记录应用加载候选，使历史包名与版本能命中选项。
    await Promise.all([
      onPackageQuery(row?.appId),
      onVersionQuery(row?.appId),
    ])
  } finally {
    loading.value = false
  }
}

const beforeSubmit = (ruleForm: any) => {
  const appId = normalizeAppId(ruleForm.appId)
  if (appId === undefined) {
    showError(t('gameBusiness.appWorkspace.appIdRequired'))
    return false
  }
  ruleForm.appId = appId
  ruleForm.content = ruleForm.content || '{}'
  ruleForm.modeContents = ruleForm.modeContents || '{}'
  ruleForm.isEnabled = ruleForm.isEnabled !== false
  ruleForm.priority = Number(ruleForm.priority ?? 0)
  ruleForm.rolloutPercent = Number(ruleForm.rolloutPercent ?? 100)
  ruleForm.description = ruleForm.description ?? ''
  if (ruleForm.deviceWhitelistJson) {
    try {
      const whitelist = JSON.parse(ruleForm.deviceWhitelistJson)
      if (!Array.isArray(whitelist)) {
        showError(t('message.client.deviceWhitelistJsonInvalid'))
        return false
      }
    } catch {
      showError(t('message.client.deviceWhitelistJsonInvalid'))
      return false
    }
  }
  return ruleForm
}

defineExpose({
  openDialog,
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
onLanguageQuery()
onPlatformQuery()
onChannelQuery()
onAppQuery()
</script>

<style scoped>
.global-info-edit-dialog-content {
  display: contents;
}

:global(.global-info-edit-dialog .base-edit-dialog-grid) {
  grid-template-columns: 120px minmax(0, 1fr);
}

:global(.global-info-edit-dialog .form-label) {
  align-self: center;
  min-width: 0;
  line-height: 1.35;
  white-space: normal !important;
  word-break: break-word;
}

</style>
