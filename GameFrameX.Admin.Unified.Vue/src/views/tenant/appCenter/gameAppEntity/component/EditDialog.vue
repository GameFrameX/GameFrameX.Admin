<template>
  <BaseEditDialog ref="dialogRef" :title="props.title"
                  :addApi="tenantGameAppEntityAddPost" :updateApi="tenantGameAppEntityUpdatePost"
                  :rules="rules"
                  :beforeOpen="beforeOpen"
                  :afterSubmit="onAfterSubmit"
                  @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm }">
      <TextField v-model="ruleForm.name" name="name" :label="t('gameBusiness.app.appName')" :placeholder="t('gameBusiness.app.appName')" maxlength="128"/>

      <FormLabel name="description">{{ t('gameBusiness.app.description') }}</FormLabel>
      <FormField name="description" v-slot="{ props: fProps, invalid, error }">
        <InputText v-bind="fProps" v-model="ruleForm.description" :placeholder="t('gameBusiness.app.description')" maxlength="512" class="w-full" :aria-describedby="descriptionHelpId"/>
        <FormHelpText :id="descriptionHelpId">{{ t('gameBusiness.app.descriptionHelp') }}</FormHelpText>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <TextField v-model="ruleForm.icon" name="icon" :label="t('gameBusiness.app.icon')" :placeholder="t('gameBusiness.app.icon')" maxlength="512"/>

      <TextField v-model="ruleForm.remark" name="remark" :label="t('gameBusiness.app.remark')" :placeholder="t('gameBusiness.app.remark')" maxlength="512"/>

      <FormLabel>{{ t('gameBusiness.app.signatureRequired') }}</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.signatureRequired"/></div>

      <FormLabel>{{ t('gameBusiness.app.deviceProofRequired') }}</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.deviceProofRequired"/></div>

      <TextField v-model="ruleForm.riskPolicy" name="riskPolicy" :label="t('gameBusiness.app.riskPolicy')" :placeholder="t('gameBusiness.app.riskPolicy')" maxlength="512"/>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref, useId} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField} from '../../../../../components/crud-form-fields'
import { tenantGameAppEntityAddPost, tenantGameAppEntityUpdatePost } from '../../../../../api/tenantGameAppEntity'

const props = defineProps({title: {type: String, default: ''}})
const emit = defineEmits(['reloadTable', 'created'])
const dialogRef = ref()
const descriptionHelpId = useId()

const rules = {
  name: [{required: true, message: t('gameBusiness.app.appNameRequired'), trigger: 'blur'}],
}

const beforeOpen = (form: any) => {
  if (form.signatureRequired == null) {
    form.signatureRequired = false
  }
  if (form.deviceProofRequired == null) {
    form.deviceProofRequired = false
  }
  // 允许平台字段已下线：剥离后端 row 残留，确保编辑提交体不再携带 allowedPlatforms
  delete form.allowedPlatforms
  return form
}

const onAfterSubmit = (_form: any, isEditor: boolean, response?: any) => {
  // 第三参为提交响应（C228：Add 返回 GameAppOutput，AppSecret 为一次性明文，仅此一次可展示）
  if (!isEditor && response?.data?.data) {
    emit('created', response.data.data)
  }
}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>
