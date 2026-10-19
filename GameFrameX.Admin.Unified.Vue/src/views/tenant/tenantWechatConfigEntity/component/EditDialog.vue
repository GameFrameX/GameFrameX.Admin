<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    width="min(900px, calc(100vw - 32px))"
    scrollable
    :defaultForm="defaultForm"
    :addApi="tenantWechatConfigEntityAddPost"
    :updateApi="tenantWechatConfigEntityUpdatePost"
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

      <div class="section-title">微信公众号凭证</div>

      <TextField v-model="ruleForm.wechatAppId" name="wechatAppId" label="Wechat AppId" placeholder="公众号 AppId" />

      <FormLabel name="wechatAppSecret">Wechat AppSecret</FormLabel>
      <FormField name="wechatAppSecret" v-slot="{ props: fProps, invalid, error }">
        <Password v-bind="fProps" v-model="ruleForm.wechatAppSecret" :feedback="false" toggleMask class="w-full" />
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
        <small class="secret-help">编辑时保持 ****** 表示不修改</small>
      </FormField>

      <div class="section-title">微信小程序凭证</div>

      <TextField v-model="ruleForm.wxOpenAppId" name="wxOpenAppId" label="小程序 AppId" placeholder="小程序 AppId" />

      <FormLabel name="wxOpenSecret">小程序 AppSecret</FormLabel>
      <FormField name="wxOpenSecret" v-slot="{ props: fProps, invalid, error }">
        <Password v-bind="fProps" v-model="ruleForm.wxOpenSecret" :feedback="false" toggleMask class="w-full" />
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
        <small class="secret-help">编辑时保持 ****** 表示不修改</small>
      </FormField>

      <div class="section-title">选择 / 状态</div>

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
import ToggleSwitch from 'primevue/toggleswitch'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField, TextareaField} from '../../../../components/crud-form-fields'
import ConfigAppScopePicker from '/@/components/config-app-scope-picker/index.vue'
import { tenantWechatConfigEntityAddPost, tenantWechatConfigEntityUpdatePost } from '../../../../api/tenantWechatConfigEntity'
import { isEnabled } from '../../../../utils/isEnabled'

const props = defineProps({ title: { type: String, default: '' }, appOptions: { type: Array, default: () => [] } })
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()
const SECRET_MASK = '******'

const defaultForm = () => ({
  id: 0,
  appId: 0,
  wechatAppId: '',
  wechatAppSecret: '',
  wxOpenAppId: '',
  wxOpenSecret: '',
  priority: 100,
  isEnabled: true,
  remark: '',
})

const trimValue = (value: unknown) => String(value ?? '').trim()

const getCurrentForm = () => dialogRef.value?.ruleForm ?? defaultForm()

const hasCredentialPair = (appId: unknown, secret: unknown) =>
  !!trimValue(appId) && !!trimValue(secret)

const credentialValidationMessage = () => {
  const form = getCurrentForm()
  const wechatAppId = trimValue(form.wechatAppId)
  const wechatAppSecret = trimValue(form.wechatAppSecret)
  const wxOpenAppId = trimValue(form.wxOpenAppId)
  const wxOpenSecret = trimValue(form.wxOpenSecret)
  const hasWechatHalf = !!wechatAppId || !!wechatAppSecret
  const hasWxOpenHalf = !!wxOpenAppId || !!wxOpenSecret

  if (!hasWechatHalf && !hasWxOpenHalf) {
    return '请输入公众号或小程序凭证对，至少完整填写一组'
  }
  if ((!!wechatAppId) !== (!!wechatAppSecret)) {
    return '公众号 AppId 与 AppSecret 必须同时填写或同时为空'
  }
  if ((!!wxOpenAppId) !== (!!wxOpenSecret)) {
    return '小程序 AppId 与 AppSecret 必须同时填写或同时为空'
  }
  if (!hasCredentialPair(wechatAppId, wechatAppSecret) && !hasCredentialPair(wxOpenAppId, wxOpenSecret)) {
    return '请输入公众号或小程序凭证对，至少完整填写一组'
  }
  return ''
}

const credentialValidator = (_rule: any, _value: any, callback: (error?: Error | string) => void) => {
  const message = credentialValidationMessage()
  callback(message || undefined)
}

const rules = {
  wechatAppId: [{ validator: credentialValidator, trigger: 'blur' }],
  wechatAppSecret: [{ validator: credentialValidator, trigger: 'blur' }],
  wxOpenAppId: [{ validator: credentialValidator, trigger: 'blur' }],
  wxOpenSecret: [{ validator: credentialValidator, trigger: 'blur' }],
}

const beforeOpen = (form: any, isEditor: boolean) => {
  const normalized = { ...defaultForm(), ...form }
  if (isEditor) {
    if (normalized.wechatAppSecret) normalized.wechatAppSecret = SECRET_MASK
    if (normalized.wxOpenSecret) normalized.wxOpenSecret = SECRET_MASK
  }
  return normalized
}

const beforeSubmit = (form: any, isEditor: boolean) => {
  const normalized = {
    id: form.id,
    appId: form.appId ?? 0,
    wechatAppId: form.wechatAppId ?? '',
    wechatAppSecret: form.wechatAppSecret ?? '',
    wxOpenAppId: form.wxOpenAppId ?? '',
    wxOpenSecret: form.wxOpenSecret ?? '',
    priority: Number(form.priority ?? 100),
    isEnabled: isEnabled(form),
    remark: form.remark ?? '',
  }
  if (isEditor && !normalized.wechatAppSecret) normalized.wechatAppSecret = SECRET_MASK
  if (isEditor && !normalized.wxOpenSecret) normalized.wxOpenSecret = SECRET_MASK
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

.section-title {
  grid-column: 1 / -1;
  font-weight: 600;
  font-size: 12px;
  color: var(--p-text-color);
  margin: 8px 0 4px;
  border-left: 3px solid var(--p-primary-color);
  padding-left: 8px;
}
</style>
