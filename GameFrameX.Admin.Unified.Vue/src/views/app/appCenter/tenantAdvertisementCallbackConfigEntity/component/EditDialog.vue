<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    width="min(900px, calc(100vw - 32px))"
    scrollable
    :defaultForm="defaultForm"
    :addApi="tenantAdvertisementCallbackConfigEntityAddPost"
    :updateApi="tenantAdvertisementCallbackConfigEntityUpdatePost"
    :rules="rules"
    :beforeOpen="beforeOpen"
    :beforeSubmit="beforeSubmit"
    @reloadTable="emit('reloadTable')"
  >
    <template #default="{ ruleForm }">
      <FormLabel name="appId">{{ t('gameBusiness.configScope.scope') }}</FormLabel>
      <FormField name="appId" v-slot="{ props: fProps }">
        <ConfigAppScopePicker v-bind="fProps" v-model="ruleForm.appId" :appOptions="props.appOptions" class="w-full"/>
      </FormField>

      <FormLabel name="provider">Provider</FormLabel>
      <FormField name="provider" v-slot="{ props: fProps, invalid, error }">
        <Select
          filter
          v-bind="fProps"
          v-model="ruleForm.provider"
          :options="providerOptions"
          optionLabel="label"
          optionValue="value"
          placeholder="请选择 Provider"
          class="w-full"
          @change="onProviderChange(ruleForm)"
        />
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <div v-if="schema" class="full-span parameter-panel">
        <div class="parameter-title">
          <span>{{ schema.label }} 参数配置</span>
          <Tag value="服务端回调验签配置" severity="info"/>
        </div>
        <div class="parameter-grid">
          <span v-for="field in schema.fields" :key="field.key" class="parameter-item">
            <code>{{ field.key }}</code>
            <span>{{ field.help }}</span>
          </span>
        </div>
      </div>

      <template v-if="schema">
        <template v-for="field in schema.fields" :key="field.key">
          <FormLabel :name="field.key">
            {{ field.label }}<span v-if="field.required" class="field-required">*</span>
          </FormLabel>
          <FormField :name="field.key" v-slot="{ props: fProps }">
            <div class="dynamic-field">
              <InputText
                v-if="field.type === 'text'"
                v-bind="fProps"
                v-model="ruleForm[field.key]"
                :placeholder="fieldPlaceholder(field)"
                class="w-full"
              />
              <Password
                v-else-if="field.type === 'secret'"
                v-bind="fProps"
                v-model="ruleForm[field.key]"
                :placeholder="fieldPlaceholder(field)"
                :feedback="false"
                toggleMask
                class="w-full"
              />
              <Textarea
                v-else-if="field.type === 'multiline'"
                v-bind="fProps"
                v-model="ruleForm[field.key]"
                :placeholder="fieldPlaceholder(field)"
                rows="4"
                :autoResize="true"
                class="w-full font-mono"
              />
              <InputNumber v-else-if="field.type === 'number'" v-bind="fProps" v-model="ruleForm[field.key]" class="w-full"/>
              <ToggleSwitch v-else-if="field.type === 'bool'" v-bind="fProps" v-model="ruleForm[field.key]"/>
              <Select
                filter
                v-else-if="field.type === 'select'"
                v-bind="fProps"
                v-model="ruleForm[field.key]"
                :options="field.options"
                optionLabel="label"
                optionValue="value"
                class="w-full"
              />
              <InputText v-else v-bind="fProps" v-model="ruleForm[field.key]" :placeholder="fieldPlaceholder(field)" class="w-full"/>
              <small v-if="field.help" class="field-help">{{ field.help }}</small>
            </div>
          </FormField>
        </template>
      </template>

      <FormLabel v-if="!schema && ruleForm.provider">Provider 字段</FormLabel>
      <div v-if="!schema && ruleForm.provider" class="config-tip">正在加载字段定义...</div>

      <FormLabel name="isEnabled">启用</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.isEnabled"/></div>

      <TextareaField v-model="ruleForm.remark" name="remark" label="备注" placeholder="备注" maxlength="512" :autoResize="true" rows="3"/>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {onMounted, ref} from 'vue'
import FormField from '@primevue/forms/formfield'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Password from 'primevue/password'
import Select from 'primevue/select'
import Textarea from 'primevue/textarea'
import ToggleSwitch from 'primevue/toggleswitch'
import Message from 'primevue/message'
import Tag from 'primevue/tag'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextareaField} from '../../../../../components/crud-form-fields'
import ConfigAppScopePicker from '/@/components/config-app-scope-picker/index.vue'
import {showError} from '../../../../../utils/message.ts'
import { tenantAdvertisementCallbackConfigEntityAddPost, tenantAdvertisementCallbackConfigEntityProvidersGet, tenantAdvertisementCallbackConfigEntitySchemaGet, tenantAdvertisementCallbackConfigEntityUpdatePost } from '../../../../../api/tenantAdvertisementCallbackConfigEntity'
import {isEnabled} from '../../../../../utils/isEnabled.ts'

interface AdCallbackFieldOption {
  value: string
  label: string
}

interface AdCallbackField {
  key: string
  label: string
  type: string
  required: boolean
  sensitive?: boolean
  help?: string
  default?: any
  options?: AdCallbackFieldOption[]
}

interface AdCallbackSchema {
  provider: string
  label: string
  fields: AdCallbackField[]
}

const props = defineProps({
  title: {type: String, default: ''},
  appOptions: {type: Array, default: () => []},
})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()
const SECRET_MASK = '******'

const providerOptions = ref<{value: string; label: string}[]>([])
const schema = ref<AdCallbackSchema | null>(null)

const defaultForm = () => ({
  id: 0,
  appId: 0,
  provider: 'TopOn',
  configJson: '',
  isEnabled: true,
  remark: '',
})

const rules = {
  provider: [{required: true, message: '请选择 Provider', trigger: 'change'}],
}

const defaultValue = (field: AdCallbackField) => {
  if (field.type === 'number') {
    return 0
  }
  if (field.type === 'bool') {
    return false
  }
  return ''
}

const hasDefaultValue = (field: AdCallbackField) => field.default !== undefined && field.default !== null && field.default !== ''
const fieldPlaceholder = (field: AdCallbackField) => hasDefaultValue(field) ? String(field.default) : ''
const isSensitiveField = (field: AdCallbackField) => field.sensitive === true || field.type === 'secret'
const submitValue = (field: AdCallbackField, value: any) => {
  if ((value === undefined || value === null || value === '') && hasDefaultValue(field)) {
    return field.default
  }
  return value === undefined ? defaultValue(field) : value
}

const loadSchema = async (provider: string): Promise<AdCallbackSchema | null> => {
  const normalized = String(provider || '').trim()
  if (!normalized) {
    schema.value = null
    return null
  }
  try {
    const res: any = await tenantAdvertisementCallbackConfigEntitySchemaGet({provider: normalized})
    const data = res.data.data
    schema.value = data ?? null
    return data ?? null
  } catch {
    schema.value = null
    return null
  }
}

const clearSchemaFields = (form: any, schemaValue: AdCallbackSchema | null) => {
  schemaValue?.fields.forEach((field) => {
    if (Object.prototype.hasOwnProperty.call(form, field.key)) {
      delete form[field.key]
    }
  })
}

const applyDefaults = (form: any, schemaValue: AdCallbackSchema | null) => {
  schemaValue?.fields.forEach((field) => {
    form[field.key] = defaultValue(field)
  })
}

const applyConfigJson = (form: any, schemaValue: AdCallbackSchema | null) => {
  if (!schemaValue || !form.configJson || form.configJson === SECRET_MASK) {
    applyDefaults(form, schemaValue)
    return
  }

  try {
    const parsed = JSON.parse(form.configJson)
    if (parsed && typeof parsed === 'object' && !Array.isArray(parsed)) {
      schemaValue.fields.forEach((field) => {
        form[field.key] = parsed[field.key] === undefined ? defaultValue(field) : parsed[field.key]
      })
      return
    }
  } catch (error: any) {
    showError(`广告回调配置 JSON 解析失败：${error.message}`)
  }
  applyDefaults(form, schemaValue)
}

const onProviderChange = async (form: any) => {
  clearSchemaFields(form, schema.value)
  const next = await loadSchema(form.provider)
  applyDefaults(form, next)
}

const beforeOpen = async (form: any) => {
  const normalized = {...defaultForm(), ...form}
  normalized.appId = normalized.appId ?? 0
  normalized.provider = normalized.provider || 'TopOn'
  normalized.isEnabled = normalized.isEnabled !== false
  const next = await loadSchema(normalized.provider)
  applyConfigJson(normalized, next)
  return normalized
}

const beforeSubmit = (form: any, isEditor: boolean) => {
  if (!form.provider) {
    showError('请选择 Provider')
    return false
  }
  if (!schema.value || schema.value.fields.length === 0) {
    showError('请选择 Provider')
    return false
  }

  for (const field of schema.value.fields) {
    if (!field.required) {
      continue
    }
    const value = form[field.key]
    if ((value === undefined || value === null || value === '') && !hasDefaultValue(field)) {
      showError(`${field.label}不能为空`)
      return false
    }
    if (value === SECRET_MASK && (!isEditor || !isSensitiveField(field))) {
      showError(`${field.label}不能为空`)
      return false
    }
  }

  const configObj: Record<string, any> = {}
  schema.value.fields.forEach((field) => {
    const value = form[field.key]
    configObj[field.key] = submitValue(field, value)
  })

  return {
    id: form.id,
    appId: form.appId ?? 0,
    provider: form.provider || 'TopOn',
    configJson: JSON.stringify(configObj),
    isEnabled: isEnabled(form),
    remark: form.remark ?? '',
  }
}

onMounted(async () => {
  try {
    const res: any = await tenantAdvertisementCallbackConfigEntityProvidersGet()
    const list = res.data?.data ?? []
    providerOptions.value = (list || []).map((item: any) => ({value: item.value, label: item.label}))
  } catch {
    providerOptions.value = []
  }
})

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
})
</script>

<style scoped>
.field-help {
  color: var(--p-text-muted-color);
  display: block;
  font-size: 12px;
  margin-top: 4px;
}

.field-required {
  color: var(--p-red-500);
  margin-left: 2px;
}

.full-span {
  grid-column: 1 / -1;
}

.parameter-panel {
  border: 1px solid var(--p-content-border-color);
  border-radius: 6px;
  padding: 12px;
}

.parameter-title {
  align-items: center;
  display: flex;
  gap: 8px;
  justify-content: space-between;
  margin-bottom: 10px;
}

.parameter-grid {
  display: grid;
  gap: 8px;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
}

.parameter-item {
  display: grid;
  gap: 4px;
}

.parameter-item code {
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, "Liberation Mono", "Courier New", monospace;
}

.parameter-item span,
.config-tip {
  color: var(--p-text-muted-color);
  font-size: 12px;
  line-height: 1.45;
}
</style>
