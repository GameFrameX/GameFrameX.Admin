<template>
  <BaseEditDialog ref="dialogRef" class="feature-switch-edit-dialog" :title="props.title" :addApi="tenantGameAppFeatureSwitchAddPost" :updateApi="tenantGameAppFeatureSwitchUpdatePost" :rules="rules" :defaultForm="defaultForm" :beforeOpen="beforeOpen" :beforeSubmit="beforeSubmit" scrollable @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm }">
      <div v-block-loading="loading" class="feature-switch-edit-dialog-content">
      <TextField v-model="ruleForm.switchKey" name="switchKey" :label="t('message.featureSwitch.switchKey')" :placeholder="t('message.featureSwitch.switchKeyPlaceholder')" maxlength="128"/>
      <TextField v-model="ruleForm.displayName" name="displayName" :label="t('message.featureSwitch.displayName')" :placeholder="t('message.common.form.pleaseInput')" maxlength="128"/>
      <TextareaField v-model="ruleForm.description" name="description" :label="t('message.client.descriptionInfo')" :placeholder="t('message.common.form.pleaseInput')" maxlength="512" :autoResize="true" rows="2"/>
      <FormLabel name="audience">{{ t('message.featureSwitch.audience') }}</FormLabel>
      <FormField name="audience" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.audience" :placeholder="t('message.common.form.pleaseSelect')" :options="audienceOptions" optionLabel="label" optionValue="value" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <TextareaField v-model="ruleForm.valueJson" name="valueJson" :label="t('message.featureSwitch.valueJson')" :placeholder="t('message.featureSwitch.valueJsonPlaceholder')" :autoResize="true" rows="3"/>
      <Message v-if="valueJsonError" severity="error" size="small" variant="simple" class="value-json-error">{{ valueJsonError }}</Message>
      <FormLabel name="startTime">{{ t('message.gameBusiness.startTime') }}</FormLabel>
      <FormField name="startTime" v-slot="{ props: fProps }">
        <DatePicker v-bind="fProps" v-model="ruleForm.startTime" showTime showSeconds hourFormat="24" class="w-full" :placeholder="t('message.featureSwitch.startTimePlaceholder')"/>
      </FormField>
      <FormLabel name="endTime">{{ t('message.gameBusiness.endTime') }}</FormLabel>
      <FormField name="endTime" v-slot="{ props: fProps }">
        <DatePicker v-bind="fProps" v-model="ruleForm.endTime" showTime showSeconds hourFormat="24" class="w-full" :placeholder="t('message.featureSwitch.endTimePlaceholder')"/>
        <Message v-if="isTimeWindowReversed(ruleForm)" severity="error" size="small" variant="simple">{{ t('message.featureSwitch.timeWindowReversed') }}</Message>
      </FormField>
      <FormLabel name="platform">{{ t('message.featureSwitch.scopePlatform') }}</FormLabel>
      <FormField name="platform" v-slot="{ props: fProps }">
        <Select v-bind="fProps" v-model="ruleForm.platform" filter :placeholder="t('message.common.form.pleaseSelect')" :options="scopeOptions.platform" optionLabel="label" optionValue="value" class="w-full"/>
      </FormField>
      <FormLabel name="package">{{ t('message.featureSwitch.scopePackage') }}</FormLabel>
      <FormField name="package" v-slot="{ props: fProps }">
        <Select v-bind="fProps" v-model="ruleForm.package" filter :placeholder="t('message.common.form.pleaseSelect')" :options="scopeOptions.package" optionLabel="label" optionValue="value" class="w-full"/>
      </FormField>
      <FormLabel name="language">{{ t('message.featureSwitch.scopeLanguage') }}</FormLabel>
      <FormField name="language" v-slot="{ props: fProps }">
        <Select v-bind="fProps" v-model="ruleForm.language" filter :placeholder="t('message.common.form.pleaseSelect')" :options="scopeOptions.language" optionLabel="label" optionValue="value" class="w-full"/>
      </FormField>
      <FormLabel name="channel">{{ t('message.featureSwitch.scopeChannel') }}</FormLabel>
      <FormField name="channel" v-slot="{ props: fProps }">
        <Select v-bind="fProps" v-model="ruleForm.channel" filter :placeholder="t('message.common.form.pleaseSelect')" :options="scopeOptions.channel" optionLabel="label" optionValue="value" class="w-full"/>
      </FormField>
      <FormLabel name="rolloutPercent">{{ t('message.client.rolloutPercent') }}</FormLabel>
      <FormField name="rolloutPercent" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.rolloutPercent" :min="0" :max="100" suffix="%" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <TextareaField v-model="ruleForm.deviceWhitelistJson" name="deviceWhitelistJson" :label="t('message.client.deviceWhitelistJson')" :placeholder="t('message.client.deviceWhitelistJsonPlaceholder')" :autoResize="true" rows="3"/>
      <FormLabel>{{ t('message.client.isEnabled') }}</FormLabel>
      <div>
        <ToggleSwitch v-model="ruleForm.isEnabled"/>
      </div>
      </div>
    </template>
  </BaseEditDialog>
</template>
<script lang="ts" setup>
import {computed, ref, watch} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField, TextareaField} from '../../../../../components/crud-form-fields'
import {showError} from '../../../../../utils/message.ts'
import { tenantGameAppFeatureSwitchAddPost, tenantGameAppFeatureSwitchUpdatePost } from '../../../../../api/tenantGameAppFeatureSwitch'
import { globalPlatformSelectPost } from '../../../../../api/globalPlatform'
import { globalLanguageSelectPost } from '../../../../../api/globalLanguage'
import { tenantChannelDefinitionAvailableGet } from '../../../../../api/tenantChannelDefinition'
import { tenantGameAppPackageListGet } from '../../../../../api/tenantGameAppPackage'
import {normalizeAppId, type AppWorkspaceId} from '../../../../../utils/appWorkspace.ts'
import { mapAvailableChannelDefinitions } from '../../../../../utils/channelDefinition.ts'
import {MAX_SELECT_SIZE} from '../../../../../constants/pagination.ts'

var props = defineProps({
  title: {
    type: String,
    default: '',
  },
  // 页面所属应用（新增时表单无行数据，包名候选按该 appId 加载）
  appId: {
    type: [Number, String],
    default: undefined,
  },
})
const emit = defineEmits(['reloadTable'])

const dialogRef = ref()
const loading = ref(false)
const valueJsonError = ref('')

const defaultForm = () => ({
  audience: 'both',
  rolloutPercent: 100,
  isEnabled: false,
})

const audienceOptions = computed(() => [
  {label: t('message.featureSwitch.audienceBoth'), value: 'both'},
  {label: t('message.featureSwitch.audienceClient'), value: 'client'},
  {label: t('message.featureSwitch.audienceServer'), value: 'server'},
])

// 作用域空值语义「不限」：后端把空白归一为 default 哨兵，前端选项直接提供 value='default' 的「不限」项，
// 编辑回显（行值为 default）与展示双向闭环；提交侧再归一为 null 交后端处理。
const unlimitedOption = () => ({label: t('message.featureSwitch.scopeUnlimited'), value: 'default'})

const scopeOptions = ref<Record<string, any[]>>({
  platform: [],
  package: [],
  language: [],
  channel: [],
})

const rules = {
  switchKey: [{required: true, message: () => t('message.featureSwitch.switchKeyRequired'), trigger: 'blur'}],
  displayName: [{required: true, message: () => t('message.featureSwitch.displayNameRequired'), trigger: 'blur'}],
  audience: [{required: true, message: () => t('message.common.form.pleaseSelect'), trigger: 'change'}],
  valueJson: [{required: true, message: () => t('message.featureSwitch.valueJsonRequired'), trigger: 'blur'}],
}

// Date 实例 → Unix 秒；空值透传 null
const toUnixSeconds = (v: unknown): number | null => {
  if (v instanceof Date) {
    return Math.floor(v.getTime() / 1000)
  }
  return typeof v === 'number' && v > 0 && !Number.isNaN(v) ? v : null
}

// Unix 秒 → Date 实例（DatePicker 回显）；null/0/非法返回 null 表示未设置
const toDate = (v: unknown): Date | null => {
  if (v instanceof Date) return v
  if (typeof v === 'number' && v > 0 && !Number.isNaN(v)) {
    const d = new Date(v * 1000)
    return Number.isNaN(d.getTime()) ? null : d
  }
  return null
}

const isTimeWindowReversed = (form: any) => {
  const start = toDate(form?.startTime)
  const end = toDate(form?.endTime)
  return start !== null && end !== null && start.getTime() >= end.getTime()
}

const onPlatformQuery = async () => {
  const res = await globalPlatformSelectPost({pageSize: MAX_SELECT_SIZE})
  scopeOptions.value.platform = [unlimitedOption(), ...(res.data.data || [])]
}

const onLanguageQuery = async () => {
  const res = await globalLanguageSelectPost({pageSize: MAX_SELECT_SIZE})
  scopeOptions.value.language = [unlimitedOption(), ...(res.data.data || [])]
}

const onChannelQuery = async () => {
  const res = await tenantChannelDefinitionAvailableGet({pageSize: MAX_SELECT_SIZE})
  scopeOptions.value.channel = [unlimitedOption(), ...mapAvailableChannelDefinitions(res.data.data || [], 'code')]
}

// 包名候选按当前应用加载（包名维度仅应用作用域行有效）；置顶「不限」项。
const onPackageQuery = async (appId?: AppWorkspaceId) => {
  const id = normalizeAppId(appId)
  if (id === undefined) {
    scopeOptions.value.package = [unlimitedOption()]
    return
  }
  const res = await tenantGameAppPackageListGet({appId: id})
  const packages = res.data?.data || []
  const options: any[] = [unlimitedOption()]
  for (const p of packages) {
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
  scopeOptions.value.package = options
}

// ValueJson 实时格式校验（同 serverConfig 交互）：非法时行内提示并在提交前拦截
const validateValueJson = (value: unknown) => {
  if (value === undefined || value === null || String(value).trim() === '') {
    valueJsonError.value = ''
    return
  }
  try {
    JSON.parse(String(value))
    valueJsonError.value = ''
  } catch (e: any) {
    valueJsonError.value = t('message.featureSwitch.valueJsonInvalid') + (e?.message ? `: ${e.message}` : '')
  }
}

const beforeOpen = async (form: any, _isEditor: boolean) => {
  // Unix 秒 → Date 供 DatePicker 回显
  form.startTime = toDate(form.startTime)
  form.endTime = toDate(form.endTime)
  // appId 归属：编辑态用行自带值；新增态（openDialog({})）回落页面传入的 props.appId
  const appId = normalizeAppId(form?.appId) ?? normalizeAppId(props.appId)
  if (appId !== undefined) {
    form.appId = appId
  }
  loading.value = true
  try {
    await onPackageQuery(appId)
  } finally {
    loading.value = false
  }
  return form
}

const beforeSubmit = (ruleForm: any) => {
  if (valueJsonError.value) {
    showError(valueJsonError.value)
    return false
  }
  try {
    JSON.parse(String(ruleForm.valueJson ?? ''))
  } catch {
    showError(t('message.featureSwitch.valueJsonInvalid'))
    return false
  }
  if (isTimeWindowReversed(ruleForm)) {
    showError(t('message.featureSwitch.timeWindowReversed'))
    return false
  }
  let whitelistJson = ruleForm.deviceWhitelistJson
  if (whitelistJson && String(whitelistJson).trim() !== '') {
    try {
      const whitelist = JSON.parse(String(whitelistJson))
      if (!Array.isArray(whitelist)) {
        showError(t('message.client.deviceWhitelistJsonInvalid'))
        return false
      }
    } catch {
      showError(t('message.client.deviceWhitelistJsonInvalid'))
      return false
    }
  } else {
    whitelistJson = null
  }
  // 归一化只落在提交载荷副本上：弹窗表单仍被 DatePicker 等控件绑定，
  // 就地把 Date 改写为 Unix 数字会打爆 PrimeVue 内部 viewDate，导致弹窗关闭渲染中断。
  // 作用域：default 哨兵与空值等价，统一归 null 交后端归一。
  const normScope = (v: unknown) => (v === 'default' || !v ? null : v)
  return {
    ...ruleForm,
    switchKey: String(ruleForm.switchKey ?? '').trim(),
    displayName: String(ruleForm.displayName ?? '').trim(),
    audience: ruleForm.audience || 'both',
    startTime: toUnixSeconds(ruleForm.startTime),
    endTime: toUnixSeconds(ruleForm.endTime),
    platform: normScope(ruleForm.platform),
    package: normScope(ruleForm.package),
    language: normScope(ruleForm.language),
    channel: normScope(ruleForm.channel),
    rolloutPercent: Number(ruleForm.rolloutPercent ?? 100),
    deviceWhitelistJson: whitelistJson,
    description: ruleForm.description ?? '',
  }
}

// BaseEditDialog 打开后 ruleForm 才建立；watch 弹窗表单的 valueJson 做实时校验
watch(() => dialogRef.value?.ruleForm?.valueJson, (value) => {
  validateValueJson(value)
}, {immediate: true})

defineExpose({
  openDialog: (row?: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
onPlatformQuery()
onLanguageQuery()
onChannelQuery()
</script>

<style scoped>
.feature-switch-edit-dialog-content {
  display: contents;
}

:global(.feature-switch-edit-dialog .base-edit-dialog-grid) {
  grid-template-columns: 120px minmax(0, 1fr);
}

:global(.feature-switch-edit-dialog .form-label) {
  align-self: center;
  min-width: 0;
  line-height: 1.35;
  white-space: normal !important;
  word-break: break-word;
}

.value-json-error {
  grid-column: 1 / -1;
  margin-bottom: 8px;
}
</style>
