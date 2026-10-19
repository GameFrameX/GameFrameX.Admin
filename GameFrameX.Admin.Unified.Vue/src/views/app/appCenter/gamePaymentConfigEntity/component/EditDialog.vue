<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    width="min(860px, calc(100vw - 32px))"
    scrollable
    :defaultForm="defaultForm"
    :addApi="tenantGamePaymentConfigEntityAddPost"
    :updateApi="tenantGamePaymentConfigEntityUpdatePost"
    :rules="rules"
    :beforeOpen="beforeOpen"
    :beforeSubmit="beforeSubmit"
    @reloadTable="emit('reloadTable')"
  >
    <template #default="{ ruleForm }">
      <FormLabel>{{ t('gameBusiness.configScope.scope') }}</FormLabel>
      <div class="payment-config-scope">
        <Tag :severity="scopeSeverity(ruleForm)" :value="scopeText(ruleForm)"/>
      </div>

      <FormLabel name="payChannel">支付渠道</FormLabel>
      <FormField name="payChannel" v-slot="{ props: fProps, invalid, error }">
        <Select
          filter
          v-bind="fProps"
          v-model="ruleForm.payChannel"
          :options="channelOptions"
          optionLabel="label"
          optionValue="value"
          placeholder="请选择支付渠道"
          class="w-full"
          @change="onChannelChange(ruleForm)"
        />
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <FormLabel name="isSandbox">环境</FormLabel>
      <div class="payment-config-env">
        <ToggleSwitch v-model="ruleForm.isSandbox"/>
        <Tag v-if="ruleForm.isSandbox" severity="warn" value="Sandbox"/>
        <Tag v-else severity="success" value="Prod"/>
      </div>

      <template v-if="schema">
        <template v-for="field in schema.fields" :key="field.key">
          <FormLabel :name="field.key">
            {{ field.label }}<span v-if="field.required" class="payment-field-required">*</span>
          </FormLabel>
          <FormField :name="field.key" v-slot="{ props: fProps }">
            <div class="payment-field">
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
                class="w-full payment-field-mono font-mono"
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
              <InputText v-else v-bind="fProps" v-model="ruleForm[field.key]" class="w-full"/>
              <small v-if="field.help" class="payment-field-help">{{ field.help }}</small>
            </div>
          </FormField>
        </template>
      </template>

      <FormLabel v-if="!schema && ruleForm.payChannel">渠道字段</FormLabel>
      <div v-if="!schema && ruleForm.payChannel" class="payment-config-tip">正在加载字段定义...</div>

      <TextareaField v-model="ruleForm.remark" name="remark" label="备注" placeholder="备注" :autoResize="true" :rows="3" maxlength="512"/>
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
import {TextareaField} from '../../../../../components/crud-form-fields'
import {showError} from '../../../../../utils/message.ts'
import { tenantGamePaymentConfigEntityAddPost, tenantGamePaymentConfigEntityChannelsGet, tenantGamePaymentConfigEntitySchemaGet, tenantGamePaymentConfigEntityUpdatePost } from '../../../../../api/tenantGamePaymentConfigEntity'
import {normalizeAppId} from '../../../../../utils/appWorkspace.ts'

interface PaymentFieldOption {
  value: string
  label: string
}

interface PaymentField {
  key: string
  label: string
  type: string
  required: boolean
  help?: string
  default?: any
  options?: PaymentFieldOption[]
}

interface PaymentSchema {
  channel: string
  label: string
  fields: PaymentField[]
}

const props = defineProps({
  title: {type: String, default: ''},
  appOptions: {type: Array, default: () => []},
  packageMap: {type: Object, default: () => ({})},
})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()

const scopeText = (form: any) => {
  const app = normalizeAppId(form?.gameAppId) ?? normalizeAppId(form?.appId)
  const pkg = form?.packageId || 0
  if (app === undefined) {
    return t('gameBusiness.appWorkspace.appUnavailable')
  }
  const appItem = (props.appOptions as any[]).find((a: any) => String(normalizeAppId(a.id)) === String(app))
  const appText = appItem?.name || `App ${app}`
  if (pkg === 0) {
    return `${t('gameBusiness.configScope.appOverride')}：${appText}`
  }
  const pkgMap = props.packageMap as Record<number, string>
  const pkgText = pkgMap[pkg] || `Package ${pkg}`
  return `${t('gameBusiness.configScope.packageOverride')}：${pkgText}`
}

const scopeSeverity = (form: any) => {
  const app = normalizeAppId(form?.gameAppId) ?? normalizeAppId(form?.appId)
  const pkg = form?.packageId || 0
  if (app === undefined) {
    return 'danger'
  }
  if (pkg === 0) {
    return 'success'
  }
  return 'warn'
}

const channelOptions = ref<{ value: string; label: string }[]>([])
const schema = ref<PaymentSchema | null>(null)

const defaultForm = () => ({
  id: 0,
  appId: undefined,
  gameAppId: undefined,
  packageId: undefined,
  payChannel: undefined,
  isSandbox: false,
  configJson: '',
  remark: '',
})

const rules = {
  payChannel: [{required: true, message: '请选择支付渠道', trigger: 'change'}],
}

const defaultValue = (field: PaymentField) => {
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

const applyDefaults = (form: any, schemaValue: PaymentSchema | null) => {
  if (!schemaValue) {
    return
  }
  schemaValue.fields.forEach((field) => {
    form[field.key] = defaultValue(field)
  })
}

const loadSchema = async (channel: string): Promise<PaymentSchema | null> => {
  const normalized = String(channel || '').trim().toLowerCase()
  if (!normalized) {
    schema.value = null
    return null
  }
  try {
    const res: any = await tenantGamePaymentConfigEntitySchemaGet({payChannel: normalized})
    const data = res.data.data
    schema.value = data ?? null
    return data ?? null
  } catch {
    schema.value = null
    return null
  }
}

const clearSchemaFields = (form: any, schemaValue: PaymentSchema | null) => {
  if (!schemaValue) {
    return
  }
  schemaValue.fields.forEach((field) => {
    if (Object.prototype.hasOwnProperty.call(form, field.key)) {
      delete form[field.key]
    }
  })
}

const onChannelChange = async (form: any) => {
  clearSchemaFields(form, schema.value)
  const next = await loadSchema(form.payChannel)
  applyDefaults(form, next)
}

const beforeOpen = async (form: any, isEditor: boolean) => {
  const next = await loadSchema(form.payChannel)
  if (isEditor && form.configJson) {
    try {
      const parsed = JSON.parse(form.configJson)
      if (parsed && typeof parsed === 'object' && !Array.isArray(parsed)) {
        next?.fields.forEach((field) => {
          if (parsed[field.key] !== undefined) {
            form[field.key] = parsed[field.key]
          } else {
            form[field.key] = defaultValue(field)
          }
        })
      }
    } catch (error: any) {
      showError(`支付配置 JSON 解析失败：${error.message}`)
    }
  } else {
    applyDefaults(form, next)
  }
  return form
}

const beforeSubmit = (form: any) => {
  const appId = normalizeAppId(form.gameAppId) ?? normalizeAppId(form.appId)
  if (appId === undefined) {
    showError(t('gameBusiness.appWorkspace.appIdRequired'))
    return false
  }
  if (!form.payChannel) {
    showError('请选择支付渠道')
    return false
  }
  if (!schema.value || schema.value.fields.length === 0) {
    showError('请选择支付渠道')
    return false
  }

  for (const field of schema.value.fields) {
    if (!field.required) {
      continue
    }
    const value = form[field.key]
    if (value === undefined || value === null || value === '') {
      showError(`${field.label}不能为空`)
      return false
    }
  }

  const configObj: Record<string, any> = {}
  schema.value.fields.forEach((field) => {
    const value = form[field.key]
    configObj[field.key] = value === undefined ? defaultValue(field) : value
  })

  const normalized: Record<string, any> = {
    id: form.id,
    appId,
    gameAppId: appId,
    packageId: form.packageId || undefined,
    payChannel: String(form.payChannel || '').trim().toLowerCase(),
    isSandbox: !!form.isSandbox,
    configJson: JSON.stringify(configObj),
    remark: form.remark ?? '',
  }

  return normalized
}

onMounted(async () => {
  try {
    const res: any = await tenantGamePaymentConfigEntityChannelsGet()
    const list = res.data?.data ?? []
    channelOptions.value = (list || []).map((item: any) => ({value: item.value, label: item.label}))
  } catch {
    channelOptions.value = []
  }
})

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
})
</script>

<style scoped>
.payment-config-env {
  display: flex;
  align-items: center;
  gap: 10px;
  min-height: 40px;
}

.payment-config-scope {
  display: flex;
  align-items: center;
  min-height: 40px;
}

.payment-field {
  display: flex;
  min-width: 0;
  flex-direction: column;
  gap: 4px;
}

.payment-field-help {
  color: var(--p-text-muted-color);
  font-size: 12px;
}

.payment-field-required {
  color: var(--p-red-500);
  margin-left: 2px;
}

.payment-field-mono {
  line-height: 1.5;
  resize: vertical;
}

.payment-config-tip {
  color: var(--p-text-muted-color);
  font-size: 12px;
}
</style>
