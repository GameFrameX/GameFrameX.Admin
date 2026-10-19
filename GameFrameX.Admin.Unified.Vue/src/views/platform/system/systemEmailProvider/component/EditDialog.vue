<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    width="min(900px, calc(100vw - 32px))"
    scrollable
    :defaultForm="defaultForm"
    :addApi="systemEmailProviderAddPost"
    :updateApi="systemEmailProviderUpdatePost"
    :rules="rules"
    :beforeOpen="beforeOpen"
    :beforeSubmit="beforeSubmit"
    @reloadTable="emit('reloadTable')"
  >
    <template #default="{ ruleForm }">
      <FormLabel name="purpose">用途</FormLabel>
      <FormField name="purpose" v-slot="{ props: fProps }">
        <Select filter v-bind="fProps" v-model="ruleForm.purpose" :options="purposeOptions" optionLabel="label" optionValue="value" class="w-full"/>
      </FormField>

      <FormLabel name="provider">Provider</FormLabel>
      <FormField name="provider" v-slot="{ props: fProps, invalid, error }">
        <Select filter v-bind="fProps" v-model="ruleForm.provider" :options="providerOptions" optionLabel="label" optionValue="value" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <template v-if="ruleForm.provider === 'Smtp'">
        <TextField v-model="ruleForm.host" name="host" label="SMTP Host" placeholder="smtp.example.com"/>

        <FormLabel name="port">SMTP Port</FormLabel>
        <FormField name="port" v-slot="{ props: fProps }">
          <InputNumber v-bind="fProps" v-model="ruleForm.port" class="w-full" :min="1" :useGrouping="false"/>
        </FormField>

        <FormLabel name="enableSsl">启用 SSL</FormLabel>
        <div><ToggleSwitch v-model="ruleForm.enableSsl"/></div>

        <FormLabel name="useDefaultCredentials">默认凭据</FormLabel>
        <div><ToggleSwitch v-model="ruleForm.useDefaultCredentials"/></div>

        <TextField v-model="ruleForm.userName" name="userName" label="SMTP 用户名"/>

        <FormLabel name="password">SMTP 密码</FormLabel>
        <FormField name="password" v-slot="{ props: fProps }">
          <Password v-bind="fProps" v-model="ruleForm.password" :feedback="false" toggleMask class="w-full"/>
          <small class="secret-help">编辑时保持 ****** 表示不修改</small>
        </FormField>
      </template>

      <template v-else>
        <TextField v-model="ruleForm.region" name="region" label="腾讯云区域" placeholder="ap-hongkong"/>

        <FormLabel name="secretId">SecretId</FormLabel>
        <FormField name="secretId" v-slot="{ props: fProps }">
          <Password v-bind="fProps" v-model="ruleForm.secretId" :feedback="false" toggleMask class="w-full"/>
        </FormField>

        <FormLabel name="secretKey">SecretKey</FormLabel>
        <FormField name="secretKey" v-slot="{ props: fProps }">
          <Password v-bind="fProps" v-model="ruleForm.secretKey" :feedback="false" toggleMask class="w-full"/>
          <small class="secret-help">编辑时保持 ****** 表示不修改</small>
        </FormField>

        <FormLabel name="templateId">模板 ID</FormLabel>
        <FormField name="templateId" v-slot="{ props: fProps }">
          <InputNumber v-bind="fProps" v-model="ruleForm.templateId" class="w-full" :min="1" :useGrouping="false"/>
        </FormField>

        <TextareaField v-model="ruleForm.templateDataJson" name="templateDataJson" label="模板变量 JSON" class="font-mono" rows="5" :autoResize="true"/>
      </template>

      <TextField v-model="ruleForm.defaultFromEmail" name="defaultFromEmail" label="默认发件人" placeholder="noreply@example.com"/>

      <TextField v-model="ruleForm.defaultFromName" name="defaultFromName" label="发件名"/>

      <TextField v-model="ruleForm.defaultToEmail" name="defaultToEmail" label="默认收件人"/>

      <FormLabel name="priority">优先级</FormLabel>
      <FormField name="priority" v-slot="{ props: fProps }">
        <InputNumber v-bind="fProps" v-model="ruleForm.priority" class="w-full" :useGrouping="false"/>
      </FormField>

      <FormLabel name="isEnabled">启用</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.isEnabled"/></div>

      <TextareaField v-model="ruleForm.remark" name="remark" label="备注" rows="3" :autoResize="true" maxlength="512"/>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref} from 'vue'
import FormField from '@primevue/forms/formfield'
import InputNumber from 'primevue/inputnumber'
import Password from 'primevue/password'
import Select from 'primevue/select'
import ToggleSwitch from 'primevue/toggleswitch'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField, TextareaField} from '../../../../../components/crud-form-fields'
import {showError} from '../../../../../utils/message.ts'
import { systemEmailProviderAddPost, systemEmailProviderUpdatePost } from '../../../../../api/systemEmailProvider'
import {isEnabled} from '../../../../../utils/isEnabled.ts'

const props = defineProps({title: {type: String, default: ''}})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()
const SECRET_MASK = '******'

const purposeOptions = [
  {label: '默认', value: 0},
  {label: '租户生命周期', value: 1},
  {label: '平台安全', value: 2},
  {label: '平台运维', value: 3},
]

const providerOptions = [
  {label: 'TencentCloud SES', value: 'TencentCloudSes'},
  {label: 'SMTP', value: 'Smtp'},
]

const defaultForm = () => ({
  id: 0,
  purpose: 0,
  provider: 'Smtp',
  host: '',
  port: 465,
  enableSsl: true,
  useDefaultCredentials: false,
  userName: '',
  password: '',
  region: '',
  secretId: '',
  secretKey: '',
  defaultFromEmail: '',
  defaultFromName: '',
  defaultToEmail: '',
  templateId: undefined,
  templateDataJson: '',
  priority: 100,
  isEnabled: true,
  remark: '',
})

const rules = {
  provider: [{required: true, message: '请选择 Provider', trigger: 'change'}],
  defaultFromEmail: [{required: true, message: '请输入默认发件人邮箱', trigger: 'blur'}],
}

const beforeOpen = (form: any, isEditor: boolean) => {
  const normalized = {...defaultForm(), ...form}
  if (isEditor) {
    if (normalized.userName) normalized.userName = SECRET_MASK
    if (normalized.password) normalized.password = SECRET_MASK
    if (normalized.secretId) normalized.secretId = SECRET_MASK
    if (normalized.secretKey) normalized.secretKey = SECRET_MASK
  }
  return normalized
}

const beforeSubmit = (form: any, isEditor: boolean) => {
  if (form.provider === 'TencentCloudSes' && form.templateDataJson) {
    try {
      JSON.parse(form.templateDataJson)
    } catch (error: any) {
      showError(`模板变量 JSON 解析失败：${error.message}`)
      return false
    }
  }

  const normalized = {
    id: form.id,
    purpose: Number(form.purpose ?? 0),
    provider: form.provider || 'Smtp',
    host: form.host ?? '',
    port: form.port == null ? null : Number(form.port),
    enableSsl: form.enableSsl === true,
    useDefaultCredentials: form.useDefaultCredentials === true,
    userName: form.userName ?? '',
    password: form.password ?? '',
    region: form.region ?? '',
    secretId: form.secretId ?? '',
    secretKey: form.secretKey ?? '',
    defaultFromEmail: form.defaultFromEmail ?? '',
    defaultFromName: form.defaultFromName ?? '',
    defaultToEmail: form.defaultToEmail ?? '',
    templateId: form.templateId ?? null,
    templateDataJson: form.templateDataJson ?? '',
    priority: Number(form.priority ?? 100),
    isEnabled: isEnabled(form),
    remark: form.remark ?? '',
  }

  if (isEditor) {
    if (!normalized.userName) normalized.userName = SECRET_MASK
    if (!normalized.password) normalized.password = SECRET_MASK
    if (!normalized.secretId) normalized.secretId = SECRET_MASK
    if (!normalized.secretKey) normalized.secretKey = SECRET_MASK
  }
  return normalized
}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
})
</script>

<style scoped>
.secret-help {
  color: var(--p-text-muted-color);
  display: block;
  font-size: 12px;
  margin-top: 4px;
}
</style>
