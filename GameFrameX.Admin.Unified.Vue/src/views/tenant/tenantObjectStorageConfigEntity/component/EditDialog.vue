<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    width="min(900px, calc(100vw - 32px))"
    scrollable
    idCheckMode="system"
    :defaultForm="defaultForm"
    :addApi="addStorageConfig"
    :updateApi="updateStorageConfig"
    :rules="rules"
    :beforeOpen="beforeOpen"
    :beforeSubmit="beforeSubmit"
    @reloadTable="emit('reloadTable')"
  >
    <template #default="{ ruleForm, isEditor }">
      <FormLabel name="provider">{{ t('system.objectStorage.provider') }}</FormLabel>
      <FormField name="provider" v-slot="{ props: fProps, invalid, error }">
        <Select filter
          v-bind="fProps"
          v-model="ruleForm.provider"
          :options="providerOptions"
          optionLabel="label"
          optionValue="value"
          :placeholder="t('system.objectStorage.provider')"
          :disabled="isEditor"
          class="w-full"
          @change="onProviderChange(ruleForm)"
        />
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <TextField v-model="ruleForm.pathTemplate" name="pathTemplate" :label="t('system.objectStorage.pathTemplate')" maxlength="128" placeholder="Upload/{yyyy}/{MM}/{dd}"/>

      <FormLabel name="maxFileSizeBytes">{{ t('system.objectStorage.maxFileSizeBytes') }}</FormLabel>
      <FormField name="maxFileSizeBytes" v-slot="{ props: fProps, invalid, error }">
        <InputNumber
          v-bind="fProps"
          v-model="ruleForm.maxFileSizeBytes"
          :min="1"
          :useGrouping="false"
          class="w-full"
        />
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <TextareaField v-model="ruleForm.allowedContentTypesText" name="allowedContentTypesText" :label="t('system.objectStorage.allowedContentTypes')" rows="3" :autoResize="true" :placeholder="t('system.objectStorage.allowedContentTypesPlaceholder')" class="object-storage-field-mono font-mono"/>

      <FormLabel name="enableMd5">{{ t('system.objectStorage.enableMd5') }}</FormLabel>
      <div class="object-storage-switch-row">
        <ToggleSwitch v-model="ruleForm.enableMd5"/>
        <Tag :severity="ruleForm.enableMd5 ? 'success' : 'secondary'" :value="ruleForm.enableMd5 ? t('common.label.yes') : t('common.label.no')"/>
      </div>

      <FormLabel name="isEnabled">{{ t('system.objectStorage.status') }}</FormLabel>
      <div class="object-storage-switch-row">
        <ToggleSwitch v-model="ruleForm.isEnabled"/>
        <Tag :severity="isEnabled(ruleForm) ? 'success' : 'danger'" :value="isEnabled(ruleForm) ? t('system.objectStorage.enabled') : t('system.objectStorage.disabled')"/>
      </div>

      <template v-if="schema">
        <template v-for="field in schema.fields" :key="field.key">
          <FormLabel :name="field.key">
            {{ field.label }}<span v-if="field.required" class="object-storage-field-required">*</span>
          </FormLabel>
          <FormField :name="field.key" v-slot="{ props: fProps }">
            <div class="object-storage-field">
              <InputText
                v-if="field.type === 'text'"
                v-bind="fProps"
                v-model="ruleForm[field.key]"
                class="w-full"
              />
              <Password
                v-else-if="field.type === 'secret'"
                v-bind="fProps"
                v-model="ruleForm[field.key]"
                :feedback="false"
                toggleMask
                class="w-full"
              />
              <Textarea
                v-else-if="field.type === 'multiline'"
                v-bind="fProps"
                v-model="ruleForm[field.key]"
                rows="4"
                :autoResize="true"
                class="w-full object-storage-field-mono font-mono"
              />
              <InputNumber
                v-else-if="field.type === 'number'"
                v-bind="fProps"
                v-model="ruleForm[field.key]"
                class="w-full"
              />
              <ToggleSwitch
                v-else-if="field.type === 'bool'"
                v-bind="fProps"
                v-model="ruleForm[field.key]"
              />
              <Select filter
                v-else-if="field.type === 'select'"
                v-bind="fProps"
                v-model="ruleForm[field.key]"
                :options="field.options"
                optionLabel="label"
                optionValue="value"
                class="w-full"
              />
              <InputText v-else v-bind="fProps" v-model="ruleForm[field.key]" class="w-full"/>
              <small v-if="field.help" class="object-storage-field-help">{{ field.help }}</small>
            </div>
          </FormField>
        </template>
      </template>

      <FormLabel v-if="!schema && ruleForm.provider">{{ t('system.objectStorage.providerFields') }}</FormLabel>
      <div v-if="!schema && ruleForm.provider" class="object-storage-tip">{{ t('system.objectStorage.loadingSchema') }}</div>

      <TextareaField v-model="ruleForm.remark" name="remark" :label="t('system.objectStorage.remark')" rows="3" :autoResize="true" maxlength="256" :placeholder="t('system.objectStorage.remark')"/>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {onMounted, ref} from 'vue'
import FormField from '@primevue/forms/formfield'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Textarea from 'primevue/textarea'
import InputNumber from 'primevue/inputnumber'
import ToggleSwitch from 'primevue/toggleswitch'
import Select from 'primevue/select'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField, TextareaField} from '../../../../components/crud-form-fields'
import {showError} from '../../../../utils/message'
import {tenantObjectStorageConfigEntityProvidersGet, tenantObjectStorageConfigEntitySchemaGet, tenantObjectStorageConfigEntityAddPost, tenantObjectStorageConfigEntityUpdatePost} from '../../../../api/tenantObjectStorageConfigEntity';
import type {AddTenantObjectStorageConfigInput, UpdateTenantObjectStorageConfigInput} from '../../../../api/_shared/data-contracts'
import {isEnabled} from '../../../../utils/isEnabled'
import {readRowVersion} from '../../../../utils/rowVersion'

interface ProviderOption {
  value: string
  label: string
}

interface StorageFieldOption {
  value: string
  label: string
}

interface StorageField {
  key: string
  label: string
  type: string
  required: boolean
  help?: string
  default?: any
  options?: StorageFieldOption[]
}

interface StorageSchema {
  provider: string
  label: string
  fields: StorageField[]
}

const props = defineProps({title: {type: String, default: ''}})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()
const providerOptions = ref<ProviderOption[]>([])
const schema = ref<StorageSchema | null>(null)

const readValue = (row: any, key: string) => row?.[key]
const readResult = (res: any) => res?.data?.data
const toApiId = (id: any) => String(id ?? '').trim() as unknown as number

const defaultForm = () => ({
  id: '',
  rowVersion: 0,
  provider: undefined,
  pathTemplate: 'Upload/{yyyy}/{MM}/{dd}',
  maxFileSizeBytes: 20971520,
  allowedContentTypesJson: '',
  allowedContentTypesText: '',
  enableMd5: true,
  configJson: '',
  isEnabled: true,
  remark: '',
})

const rules = {
  provider: [{required: true, message: t('system.objectStorage.selectProvider'), trigger: 'change'}],
  pathTemplate: [{required: true, message: t('system.objectStorage.pathTemplateRequired'), trigger: 'blur'}],
  maxFileSizeBytes: [{required: true, message: t('system.objectStorage.maxFileSizeRequired'), trigger: 'blur'}],
}

const normalizeProviders = (list: any[]): ProviderOption[] =>
  (list || []).map((item: any) => ({
    value: readValue(item, 'value') ?? '',
    label: readValue(item, 'label') ?? readValue(item, 'value') ?? '',
  })).filter((item) => item.value)

const loadProviders = async () => {
  const res = await tenantObjectStorageConfigEntityProvidersGet()
  providerOptions.value = normalizeProviders(readResult(res) ?? [])
}

const normalizeSchema = (data: any): StorageSchema | null => {
  if (!data) {
    return null
  }
  const fields = readValue(data, 'fields') ?? []
  return {
    provider: readValue(data, 'provider') ?? '',
    label: readValue(data, 'label') ?? '',
    fields: fields.map((field: any) => {
      const key = readValue(field, 'key') ?? ''
      const options = readValue(field, 'options') ?? []
      return {
        key,
        label: readValue(field, 'label') ?? key,
        type: readValue(field, 'type') ?? 'text',
        required: readValue(field, 'required') === true,
        help: readValue(field, 'help') ?? '',
        default: readValue(field, 'default'),
        options: (options || []).map((option: string) => ({label: option, value: option})),
      }
    }).filter((field: StorageField) => field.key),
  }
}

const normalizeRow = (row: any) => ({
  id: String(readValue(row, 'id') ?? ''),
  rowVersion: readRowVersion(row) ?? 0,
  provider: readValue(row, 'provider') ?? '',
  pathTemplate: readValue(row, 'pathTemplate') ?? 'Upload/{yyyy}/{MM}/{dd}',
  maxFileSizeBytes: readValue(row, 'maxFileSizeBytes') ?? 20971520,
  allowedContentTypesJson: readValue(row, 'allowedContentTypesJson') ?? '',
  allowedContentTypesText: contentTypesToText(readValue(row, 'allowedContentTypesJson') ?? ''),
  enableMd5: readValue(row, 'enableMd5') !== false,
  configJson: readValue(row, 'configJson') ?? '',
  isEnabled: readValue(row, 'isEnabled') !== false,
  remark: readValue(row, 'remark') ?? '',
})

const defaultValue = (field: StorageField) => {
  if (field.default !== undefined && field.default !== null) {
    return field.default
  }
  if (field.type === 'number') {
    return 0
  }
  if (field.type === 'bool') {
    return false
  }
  return ''
}

const contentTypesToText = (json: string) => {
  if (!json) {
    return ''
  }
  try {
    const parsed = JSON.parse(json)
    return Array.isArray(parsed) ? parsed.join('\n') : ''
  } catch {
    return ''
  }
}

const textToContentTypesJson = (text: string) => {
  const values = String(text || '')
    .split(/[\n,]/)
    .map((item) => item.trim())
    .filter(Boolean)
  return values.length > 0 ? JSON.stringify(values) : ''
}

const applyDefaults = (form: any, schemaValue: StorageSchema | null) => {
  schemaValue?.fields.forEach((field) => {
    form[field.key] = defaultValue(field)
  })
}

const clearSchemaFields = (form: any, schemaValue: StorageSchema | null) => {
  schemaValue?.fields.forEach((field) => {
    if (Object.prototype.hasOwnProperty.call(form, field.key)) {
      delete form[field.key]
    }
  })
}

const loadSchema = async (provider: string): Promise<StorageSchema | null> => {
  const normalized = String(provider || '').trim().toLowerCase()
  if (!normalized) {
    schema.value = null
    return null
  }
  try {
    const res = await tenantObjectStorageConfigEntitySchemaGet(normalized)
    const next = normalizeSchema(readResult(res))
    schema.value = next
    return next
  } catch {
    schema.value = null
    return null
  }
}

const onProviderChange = async (form: any) => {
  clearSchemaFields(form, schema.value)
  const next = await loadSchema(form.provider)
  applyDefaults(form, next)
}

const fillSchemaValues = (form: any, configJson: string, schemaValue: StorageSchema | null) => {
  if (!schemaValue) {
    return
  }
  let parsed: Record<string, any> = {}
  if (configJson) {
    try {
      parsed = JSON.parse(configJson)
    } catch (error: any) {
      showError(`${t('system.objectStorage.configJsonParseFailed')}: ${error.message}`)
    }
  }

  schemaValue.fields.forEach((field) => {
    form[field.key] = parsed[field.key] !== undefined ? parsed[field.key] : defaultValue(field)
  })
}

const beforeOpen = async (form: any, isEditor: boolean) => {
  if (!providerOptions.value.length) {
    await loadProviders()
  }
  const normalized = normalizeRow(form)
  if (!isEditor && !normalized.provider && providerOptions.value.length) {
    normalized.provider = providerOptions.value[0].value
  }
  const next = await loadSchema(normalized.provider)
  if (isEditor) {
    fillSchemaValues(normalized, normalized.configJson, next)
  } else {
    applyDefaults(normalized, next)
  }
  return normalized
}

const beforeSubmit = (form: any) => {
  if (!form.provider) {
    showError(t('system.objectStorage.selectProvider'))
    return false
  }
  if (!form.pathTemplate) {
    showError(t('system.objectStorage.pathTemplateRequired'))
    return false
  }
  if (!Number(form.maxFileSizeBytes) || Number(form.maxFileSizeBytes) <= 0) {
    showError(t('system.objectStorage.maxFileSizeRequired'))
    return false
  }
  if (!schema.value) {
    showError(t('system.objectStorage.selectProvider'))
    return false
  }

  for (const field of schema.value.fields) {
    if (!field.required) {
      continue
    }
    const value = form[field.key]
    if (value === undefined || value === null || value === '') {
      showError(`${field.label}${t('system.objectStorage.requiredSuffix')}`)
      return false
    }
  }

  const configObj: Record<string, any> = {}
  schema.value.fields.forEach((field) => {
    const value = form[field.key]
    configObj[field.key] = value === undefined ? defaultValue(field) : value
  })

  return {
    id: form.id,
    rowVersion: form.rowVersion ?? 0,
    provider: String(form.provider || '').trim().toLowerCase(),
    pathTemplate: form.pathTemplate,
    maxFileSizeBytes: Number(form.maxFileSizeBytes),
    allowedContentTypesJson: textToContentTypesJson(form.allowedContentTypesText),
    enableMd5: !!form.enableMd5,
    configJson: JSON.stringify(configObj),
    isEnabled: isEnabled(form),
    remark: form.remark ?? '',
  }
}

const toAddInput = (form: any): AddTenantObjectStorageConfigInput => ({
  provider: form.provider,
  pathTemplate: form.pathTemplate,
  maxFileSizeBytes: form.maxFileSizeBytes,
  allowedContentTypesJson: form.allowedContentTypesJson,
  enableMd5: form.enableMd5,
  configJson: form.configJson,
  isEnabled: form.isEnabled,
  remark: form.remark,
})

const toUpdateInput = (form: any): UpdateTenantObjectStorageConfigInput => ({
  id: toApiId(form.id),
  rowVersion: form.rowVersion,
  ...toAddInput(form),
})

const addStorageConfig = (form: any) => tenantObjectStorageConfigEntityAddPost(toAddInput(form))
const updateStorageConfig = (form: any) => tenantObjectStorageConfigEntityUpdatePost(toUpdateInput(form))

onMounted(async () => {
  await loadProviders()
})

defineExpose({
  openDialog: (row?: any) => dialogRef.value?.openDialog(row),
})
</script>

<style scoped>
.object-storage-switch-row {
  display: flex;
  align-items: center;
  gap: 10px;
  min-height: 40px;
}

.object-storage-field {
  display: flex;
  min-width: 0;
  flex-direction: column;
  gap: 4px;
}

.object-storage-field-help {
  color: var(--p-text-muted-color);
  font-size: 12px;
}

.object-storage-field-required {
  color: var(--p-red-500);
  margin-left: 2px;
}

.object-storage-field-mono {
  line-height: 1.5;
  resize: vertical;
}

.object-storage-tip {
  color: var(--p-text-muted-color);
  font-size: 12px;
}
</style>
