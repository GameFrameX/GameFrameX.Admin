<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    width="min(860px, calc(100vw - 32px))"
    scrollable
    :defaultForm="defaultForm"
    :addApi="tenantSmsConfigEntityAddPost"
    :updateApi="tenantSmsConfigEntityUpdatePost"
    :rules="rules"
    :beforeOpen="beforeOpen"
    :beforeSubmit="beforeSubmit"
    @reloadTable="emit('reloadTable')"
  >
    <template #default="{ ruleForm }">
      <FormLabel name="appId">{{ t('gameBusiness.configScope.scope') }}</FormLabel>
      <FormField name="appId" v-slot="{ props: fProps }">
        <ConfigAppScopePicker v-bind="fProps" v-model="ruleForm.appId" :appOptions="props.appOptions" class="w-full" />
      </FormField>

      <FormLabel name="provider">Provider</FormLabel>
      <FormField name="provider" v-slot="{ props: fProps }">
        <Select filter v-bind="fProps" v-model="ruleForm.provider" :options="providerOptions" optionLabel="label" optionValue="value" class="w-full" :disabled="true" />
      </FormField>

      <FormLabel name="accessKeyId">AccessKeyId</FormLabel>
      <FormField name="accessKeyId" v-slot="{ props: fProps, invalid, error }">
        <Password v-bind="fProps" v-model="ruleForm.accessKeyId" :feedback="false" toggleMask class="w-full" />
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <FormLabel name="accessKeySecret">AccessKeySecret</FormLabel>
      <FormField name="accessKeySecret" v-slot="{ props: fProps, invalid, error }">
        <Password v-bind="fProps" v-model="ruleForm.accessKeySecret" :feedback="false" toggleMask class="w-full" />
        <small class="secret-help">编辑时保持 ****** 表示不修改</small>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <TextField v-model="ruleForm.signName" name="signName" label="短信签名" />

      <TextField v-model="ruleForm.templateCode" name="templateCode" label="模板 Code" placeholder="SMS_xxxxxx" />

      <FormLabel name="priority">优先级</FormLabel>
      <FormField name="priority" v-slot="{ props: fProps }">
        <InputNumber v-bind="fProps" v-model="ruleForm.priority" class="w-full" :useGrouping="false" />
      </FormField>

      <FormLabel name="isEnabled">启用</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.isEnabled" /></div>

      <TextareaField v-model="ruleForm.remark" name="remark" label="备注" rows="3" :autoResize="true" maxlength="512" />
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import { ref } from 'vue'
import FormField from '@primevue/forms/formfield'
import InputNumber from 'primevue/inputnumber'
import Password from 'primevue/password'
import Select from 'primevue/select'
import ToggleSwitch from 'primevue/toggleswitch'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField, TextareaField} from '../../../../components/crud-form-fields'
import ConfigAppScopePicker from '/@/components/config-app-scope-picker/index.vue'
import { tenantSmsConfigEntityAddPost, tenantSmsConfigEntityUpdatePost } from '../../../../api/tenantSmsConfigEntity'
import { isEnabled } from '../../../../utils/isEnabled'

const props = defineProps({ title: { type: String, default: '' }, appOptions: { type: Array, default: () => [] } })
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()
const SECRET_MASK = '******'

const providerOptions = [
  { label: 'Aliyun', value: 'Aliyun' },
]

const defaultForm = () => ({
  id: 0,
  appId: 0,
  provider: 'Aliyun',
  accessKeyId: '',
  accessKeySecret: '',
  signName: '',
  templateCode: '',
  priority: 100,
  isEnabled: true,
  remark: '',
})

const rules = {
  signName: [{ required: true, message: '请输入短信签名', trigger: 'blur' }],
  accessKeyId: [{ required: true, message: '请输入 AccessKeyId', trigger: 'blur' }],
  accessKeySecret: [{ required: true, message: '请输入 AccessKeySecret', trigger: 'blur' }],
}

const beforeOpen = (form: any, isEditor: boolean) => {
  const normalized = { ...defaultForm(), ...form }
  if (isEditor) {
    if (normalized.accessKeyId) normalized.accessKeyId = SECRET_MASK
    if (normalized.accessKeySecret) normalized.accessKeySecret = SECRET_MASK
  }
  return normalized
}

const beforeSubmit = (form: any, isEditor: boolean) => {
  const normalized = {
    id: form.id,
    appId: form.appId ?? 0,
    provider: form.provider || 'Aliyun',
    accessKeyId: form.accessKeyId ?? '',
    accessKeySecret: form.accessKeySecret ?? '',
    signName: form.signName ?? '',
    templateCode: form.templateCode ?? '',
    priority: Number(form.priority ?? 100),
    isEnabled: isEnabled(form),
    remark: form.remark ?? '',
  }
  if (isEditor && !normalized.accessKeyId) normalized.accessKeyId = SECRET_MASK
  if (isEditor && !normalized.accessKeySecret) normalized.accessKeySecret = SECRET_MASK
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
