<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    width="min(860px, calc(100vw - 32px))"
    scrollable
    :defaultForm="defaultForm"
    :addApi="tenantEmailConfigEntityAddPost"
    :updateApi="tenantEmailConfigEntityUpdatePost"
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

      <FormLabel name="purpose">用途</FormLabel>
      <FormField name="purpose" v-slot="{ props: fProps }">
        <Select filter v-bind="fProps" v-model="ruleForm.purpose" :options="purposeOptions" optionLabel="label" optionValue="value" class="w-full"/>
      </FormField>

      <FormLabel name="provider">Provider</FormLabel>
      <FormField name="provider" v-slot="{ props: fProps, invalid, error }">
        <Select filter v-bind="fProps" v-model="ruleForm.provider" :options="providerOptions" optionLabel="label" optionValue="value" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <TextField v-model="ruleForm.region" name="region" label="区域" placeholder="ap-hongkong"/>

      <TextField v-model="ruleForm.templateSet" name="templateSet" label="模板组"/>

      <TextField v-model="ruleForm.fromEmailAddress" name="fromEmailAddress" label="发信邮箱" placeholder="noreply@example.com" maxlength="128"/>

      <TextField v-model="ruleForm.defaultFromName" name="defaultFromName" label="发件名"/>

      <TextField v-model="ruleForm.defaultSubjectPrefix" name="defaultSubjectPrefix" label="主题前缀"/>

      <FormLabel name="fromEmailSecretId">SecretId</FormLabel>
      <FormField name="fromEmailSecretId" v-slot="{ props: fProps, invalid, error }">
        <Password v-bind="fProps" v-model="ruleForm.fromEmailSecretId" :feedback="false" toggleMask class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <FormLabel name="fromEmailSecretKey">SecretKey</FormLabel>
      <FormField name="fromEmailSecretKey" v-slot="{ props: fProps, invalid, error }">
        <Password v-bind="fProps" v-model="ruleForm.fromEmailSecretKey" :feedback="false" toggleMask class="w-full"/>
        <small class="secret-help">编辑时保持 ****** 表示不修改</small>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <FormLabel name="priority">优先级</FormLabel>
      <FormField name="priority" v-slot="{ props: fProps }">
        <InputNumber v-bind="fProps" v-model="ruleForm.priority" class="w-full" :useGrouping="false"/>
      </FormField>

      <FormLabel name="isEnabled">启用</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.isEnabled"/></div>

      <TextareaField v-model="ruleForm.remark" name="remark" label="备注" :autoResize="true" :rows="3" maxlength="512"/>
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
import ConfigAppScopePicker from '/@/components/config-app-scope-picker/index.vue'
import { tenantEmailConfigEntityAddPost, tenantEmailConfigEntityUpdatePost } from '../../../../../api/tenantEmailConfigEntity'
import {isEnabled} from '../../../../../utils/isEnabled.ts'

const props = defineProps({title: {type: String, default: ''}, appOptions: {type: Array, default: () => []}})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()
const SECRET_MASK = '******'

const purposeOptions = [
  {label: '默认', value: 0},
  {label: '注册验证码', value: 1},
  {label: '找回密码', value: 2},
  {label: '运营触达', value: 3},
]

const providerOptions = [
  {label: 'TencentCloud SES', value: 'TencentCloudSes'},
]

const defaultForm = () => ({
  id: 0,
  appId: 0,
  purpose: 0,
  provider: 'TencentCloudSes',
  region: '',
  templateSet: '',
  fromEmailAddress: '',
  defaultFromName: '',
  defaultSubjectPrefix: '',
  fromEmailSecretId: '',
  fromEmailSecretKey: '',
  priority: 100,
  isEnabled: true,
  remark: '',
})

const rules = {
  provider: [{required: true, message: '请选择 Provider', trigger: 'change'}],
  fromEmailAddress: [{required: true, message: '请输入发信邮箱', trigger: 'blur'}],
  fromEmailSecretId: [{required: true, message: '请输入 SecretId', trigger: 'blur'}],
  fromEmailSecretKey: [{required: true, message: '请输入 SecretKey', trigger: 'blur'}],
}

const beforeOpen = (form: any, isEditor: boolean) => {
  const normalized = {...defaultForm(), ...form}
  if (isEditor) {
    if (normalized.fromEmailSecretId) normalized.fromEmailSecretId = SECRET_MASK
    if (normalized.fromEmailSecretKey) normalized.fromEmailSecretKey = SECRET_MASK
  }
  return normalized
}

const beforeSubmit = (form: any, isEditor: boolean) => {
  const normalized = {
    id: form.id,
    appId: form.appId ?? 0,
    purpose: Number(form.purpose ?? 0),
    provider: form.provider || 'TencentCloudSes',
    region: form.region ?? '',
    templateSet: form.templateSet ?? '',
    fromEmailAddress: form.fromEmailAddress ?? '',
    defaultFromName: form.defaultFromName ?? '',
    defaultSubjectPrefix: form.defaultSubjectPrefix ?? '',
    fromEmailSecretId: form.fromEmailSecretId ?? '',
    fromEmailSecretKey: form.fromEmailSecretKey ?? '',
    priority: Number(form.priority ?? 100),
    isEnabled: isEnabled(form),
    remark: form.remark ?? '',
  }
  if (isEditor && !normalized.fromEmailSecretId) normalized.fromEmailSecretId = SECRET_MASK
  if (isEditor && !normalized.fromEmailSecretKey) normalized.fromEmailSecretKey = SECRET_MASK
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
