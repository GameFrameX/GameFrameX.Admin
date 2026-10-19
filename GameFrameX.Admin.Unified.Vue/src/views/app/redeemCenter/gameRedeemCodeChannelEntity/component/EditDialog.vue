<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="(params: any) => tenantGameRedeemCodeChannelEntityAddPost(params)"
    :updateApi="(params: any) => tenantGameRedeemCodeChannelEntityUpdatePost(params)"
    :rules="rules"
    :beforeOpen="beforeOpen"
    @reloadTable="emit('reloadTable')"
  >
    <template #default="{ ruleForm, isEditor }">
      <TextField v-model="ruleForm.name" name="name" :label="t('message.gameBusiness.name')" :placeholder="t('message.gameBusiness.pleaseEnterName')" maxlength="64"/>
      <TextField v-model="ruleForm.description" name="description" :label="t('message.gameBusiness.description')" :placeholder="t('message.gameBusiness.pleaseEnterDescription')" maxlength="512"/>
      <FormLabel name="roleUsageLimit">{{ t('message.gameBusiness.redeemcode.roleUsageLimit') }}</FormLabel>
      <FormField name="roleUsageLimit" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.roleUsageLimit" :min="0" :max="99999999" class="w-full" :placeholder="t('message.gameBusiness.redeemcode.pleaseEnterRoleUsageLimit')"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormHelpText>{{ t('message.gameBusiness.redeemcode.roleUsageLimitHint') }}</FormHelpText>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import InputNumber from 'primevue/inputnumber'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import FormHelpText from '/@/components/form-help-text/index.vue'
import {TextField} from '../../../../../components/crud-form-fields'
import { tenantGameRedeemCodeChannelEntityAddPost, tenantGameRedeemCodeChannelEntityUpdatePost } from '../../../../../api/tenantGameRedeemCodeChannelEntity'
import {normalizeAppId} from '../../../../../utils/appWorkspace.ts'

const props = defineProps({
  title: {type: String, default: ''},
  appId: {type: [Number, String], default: undefined},
})
const emit = defineEmits(['reloadTable'])

const dialogRef = ref()

const rules = {
  name: [{required: true, message: t('message.gameBusiness.pleaseEnterName'), trigger: 'blur'}],
  description: [{required: true, message: t('message.gameBusiness.pleaseEnterDescription'), trigger: 'blur'}],
}

const beforeOpen = (form: any, isEditor: boolean) => {
  const appId = normalizeAppId(props.appId)
  if (appId) form.appId = appId
  // 每角色可用次数：缺省回退 0（C159 默认值；后端未返回字段时保证新建/编辑回显一致）
  if (form.roleUsageLimit === undefined || form.roleUsageLimit === null) {
    form.roleUsageLimit = 0
  }
}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>
