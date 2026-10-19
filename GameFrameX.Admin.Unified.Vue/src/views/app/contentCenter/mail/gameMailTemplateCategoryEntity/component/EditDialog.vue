<template>
  <BaseEditDialog ref="dialogRef" :title="props.title" :addApi="tenantGameMailTemplateCategoryEntityAddPost" :updateApi="tenantGameMailTemplateCategoryEntityUpdatePost" :rules="rules" :defaultForm="defaultForm" :beforeOpen="beforeOpen" @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm, isEditor }">
      <TextField v-model="ruleForm.name" name="name" :label="t('message.gameBusiness.name')" :placeholder="t('message.gameBusiness.pleaseEnterName')" maxlength="32"/>
      <TextField v-model="ruleForm.icon" name="icon" :label="t('message.gameBusiness.goods.icon')" :placeholder="t('message.gameBusiness.goods.pleaseEnterIcon')" maxlength="256"/>
      <FormLabel name="sort">{{ t('message.gameBusiness.goods.sort') }}</FormLabel>
      <FormField name="sort" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.sort" :min="0" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel>{{ t('message.gameBusiness.isEnabled') }}</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.isEnabled"/></div>
      <TextField v-model="ruleForm.description" name="description" :label="t('message.gameBusiness.description')" :placeholder="t('message.gameBusiness.pleaseEnterDescription')" maxlength="512"/>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField} from '../../../../../../components/crud-form-fields'
import {normalizeAppId} from '../../../../../../utils/appWorkspace.ts'
import { tenantGameMailTemplateCategoryEntityAddPost, tenantGameMailTemplateCategoryEntityUpdatePost } from '../../../../../../api/tenantGameMailTemplateCategoryEntity'
import {isEnabled} from '../../../../../../utils/isEnabled.ts'

const props = defineProps({
  title: {type: String, default: ''},
  appId: {type: [Number, String], default: undefined},
})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()

const rules = {
  name: [{required: true, message: t('message.gameBusiness.pleaseEnterName'), trigger: 'blur'}],
}

const defaultForm = () => ({isEnabled: true, sort: 0})

const beforeOpen = (form: any, isEditor: boolean) => {
  const appId = normalizeAppId(props.appId)
  if (appId) form.appId = appId
  form.isEnabled = isEnabled(form)
  return form
}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>
