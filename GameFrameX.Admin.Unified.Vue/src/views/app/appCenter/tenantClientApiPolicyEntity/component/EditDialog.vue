<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    width="min(760px, calc(100vw - 32px))"
    :addApi="tenantClientApiPolicyEntityAddPost"
    :updateApi="tenantClientApiPolicyEntityUpdatePost"
    :rules="rules"
    :beforeOpen="beforeOpen"
    :beforeSubmit="beforeSubmit"
    @reloadTable="emit('reloadTable')"
  >
    <template #default="{ ruleForm, isEditor }">
      <BaseEditFieldGroup :title="t('message.client.clientApiPolicySectionScope')">
        <FormLabel name="appId">{{ t('message.client.clientApiPolicyAppId') }}</FormLabel>
        <FormField name="appId" v-slot="{ props: fProps, invalid, error }">
          <InputNumber v-bind="fProps" v-model="ruleForm.appId" :min="0" :disabled="isEditor" :useGrouping="false" class="w-full"/>
          <small class="field-help">{{ t('message.client.clientApiPolicyAppIdHint') }}</small>
          <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
        </FormField>
      </BaseEditFieldGroup>

      <BaseEditFieldGroup :title="t('message.client.clientApiPolicySectionToken')">
        <FormLabel name="signingKey">{{ t('message.client.clientApiPolicySigningKey') }}</FormLabel>
        <FormField name="signingKey" v-slot="{ props: fProps, invalid, error }">
          <Password v-bind="fProps" v-model="ruleForm.signingKey" :feedback="false" toggleMask class="w-full"/>
          <small class="field-help">{{ t('message.client.clientApiPolicySigningKeyHint') }}</small>
          <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
        </FormField>

        <FormLabel name="lifetimeMinutes">{{ t('message.client.clientApiPolicyLifetimeMinutes') }}</FormLabel>
        <FormField name="lifetimeMinutes" v-slot="{ props: fProps, invalid, error }">
          <InputNumber v-bind="fProps" v-model="ruleForm.lifetimeMinutes" :min="1" :max="10080" class="w-full"/>
          <small class="field-help">{{ t('message.client.clientApiPolicyLifetimeRange') }}</small>
          <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
        </FormField>
      </BaseEditFieldGroup>

      <BaseEditFieldGroup :title="t('message.client.clientApiPolicySectionTicket')">
        <FormLabel name="ticketExpireTime">{{ t('message.client.clientApiPolicyTicketExpireTime') }}</FormLabel>
        <FormField name="ticketExpireTime" v-slot="{ props: fProps, invalid, error }">
          <DatePicker
              v-bind="fProps"
              v-model="ruleForm.ticketExpireTime"
              showTime
              class="w-full"
              :placeholder="t('message.client.clientApiPolicyTicketExpireHint')"
              :minDate="minExpireDate"
              :maxDate="maxExpireDate"
          />
          <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
        </FormField>

        <FormLabel name="enableKeyAutoCreate">{{ t('message.client.clientApiPolicyEnableKeyAutoCreate') }}</FormLabel>
        <div>
          <ToggleSwitch v-model="ruleForm.enableKeyAutoCreate"/>
          <small class="field-help block">{{ t('message.client.clientApiPolicyEnableKeyAutoCreateHint') }}</small>
        </div>
      </BaseEditFieldGroup>

      <BaseEditFieldGroup :title="t('message.client.clientApiPolicySectionArea')">
        <FormLabel name="enableReturnHttpManager">{{ t('message.client.clientApiPolicyReturnHttpManager') }}</FormLabel>
        <div><ToggleSwitch v-model="ruleForm.enableReturnHttpManager"/></div>
      </BaseEditFieldGroup>

      <BaseEditFieldGroup :title="t('message.client.clientApiPolicySectionOrder')">
        <FormLabel name="enableSandBoxRecharge">{{ t('message.client.clientApiPolicyEnableSandBoxRecharge') }}</FormLabel>
        <div><ToggleSwitch v-model="ruleForm.enableSandBoxRecharge"/></div>
      </BaseEditFieldGroup>

      <FormLabel name="remark">{{ t('message.common.label.remark') }}</FormLabel>
      <FormField name="remark" v-slot="{ props: fProps, invalid, error }">
        <Textarea v-bind="fProps" v-model="ruleForm.remark" rows="3" :autoResize="true" maxlength="512" :placeholder="t('message.common.label.remark')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref} from 'vue'
import FormField from '@primevue/forms/formfield'
import Password from 'primevue/password'
import InputNumber from 'primevue/inputnumber'
import DatePicker from 'primevue/datepicker'
import ToggleSwitch from 'primevue/toggleswitch'
import Textarea from 'primevue/textarea'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import BaseEditFieldGroup from '/@/components/base-edit-dialog/FieldGroup.vue'
import { tenantClientApiPolicyEntityAddPost, tenantClientApiPolicyEntityUpdatePost } from '../../../../../api/tenantClientApiPolicyEntity'
import {showError} from '../../../../../utils/message.ts'

const props = defineProps({
  title: {
    type: String,
    default: '',
  },
})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()

const SECRET_MASK = '******'
const maxTicketExpireSeconds = 30 * 24 * 60 * 60
const minExpireDate = new Date()
const maxExpireDate = new Date(Date.now() + maxTicketExpireSeconds * 1000)

const rules = {
  lifetimeMinutes: [{required: true, message: t('message.client.clientApiPolicyLifetimeRequired'), trigger: 'blur'}],
}

const beforeOpen = (form: any, isEditor: boolean) => {
  if (isEditor && form.signingKey) {
    // 输出恒为脱敏占位：编辑回显统一显示掩码，留空/掩码提交均表示保留原值
    form.signingKey = SECRET_MASK
  }
  if (!isEditor && !form.signingKey) {
    form.signingKey = ''
  }
  if (form.appId === undefined || form.appId === null || form.appId === '') {
    form.appId = 0
  }
  if (form.lifetimeMinutes === undefined || form.lifetimeMinutes === null) {
    form.lifetimeMinutes = 60
  }
  form.enableKeyAutoCreate = !!form.enableKeyAutoCreate
  form.enableReturnHttpManager = !!form.enableReturnHttpManager
  form.enableSandBoxRecharge = !!form.enableSandBoxRecharge
  // long(秒) -> Date 供 DatePicker 回显；0/空表示永不过期
  form.ticketExpireTime = form.ticketExpireTime ? new Date(form.ticketExpireTime * 1000) : null
  return form
}

const beforeSubmit = (form: any, isEditor: boolean) => {
  let lifetime = Number(form.lifetimeMinutes)
  if (!Number.isFinite(lifetime)) {
    lifetime = 60
  }
  lifetime = Math.min(10080, Math.max(1, Math.floor(lifetime)))

  let ticketExpireTime = 0
  if (form.ticketExpireTime) {
    const now = Math.floor(Date.now() / 1000)
    ticketExpireTime = Math.floor(new Date(form.ticketExpireTime).getTime() / 1000)
    if (ticketExpireTime <= now) {
      showError(t('message.client.clientApiPolicyTicketExpireInvalid'))
      return false
    }
    if (ticketExpireTime > now + maxTicketExpireSeconds) {
      showError(t('message.client.clientApiPolicyTicketExpireTooFar'))
      return false
    }
  }

  // 签名密钥：新增留空自动生成；编辑留空/掩码均保留原值（C368 空白与掩码同义）
  const signingKey = isEditor && (!form.signingKey || form.signingKey === SECRET_MASK) ? '' : (form.signingKey ?? '')

  const normalized: Record<string, any> = {
    appId: Number(form.appId) || 0,
    signingKey,
    lifetimeMinutes: lifetime,
    ticketExpireTime,
    enableKeyAutoCreate: !!form.enableKeyAutoCreate,
    enableReturnHttpManager: !!form.enableReturnHttpManager,
    enableSandBoxRecharge: !!form.enableSandBoxRecharge,
    remark: form.remark ?? '',
  }
  if (isEditor) {
    normalized.id = form.id
  }
  return normalized
}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog({
    appId: 0,
    signingKey: '',
    lifetimeMinutes: 60,
    ticketExpireTime: null,
    enableKeyAutoCreate: false,
    enableReturnHttpManager: false,
    enableSandBoxRecharge: false,
    remark: '',
    ...row,
  }),
})
</script>

<style scoped>
.field-help {
  color: var(--p-text-muted-color);
  font-size: 12px;
}
</style>
