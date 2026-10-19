<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="tenantGameAppRemoteConfigModeAddPost"
    :updateApi="tenantGameAppRemoteConfigModeUpdatePost"
    :rules="rules"
    :defaultForm="defaultForm"
    @reloadTable="emit('reloadTable')"
  >
    <template #default="{ ruleForm, isEditor }">
      <TextField v-model="ruleForm.modeCode" name="modeCode" :label="t('message.client.modeCode')" :placeholder="t('message.client.pleaseInputModeCode')" :disabled="isEditor"/>
      <TextField v-model="ruleForm.displayName" name="displayName" :label="t('message.client.displayName')" :placeholder="t('message.client.pleaseInputModeDisplayName')"/>
      <TextareaField v-model="ruleForm.description" name="description" :label="t('message.common.label.description')" :placeholder="t('message.common.form.pleaseInput')" :autoResize="true" rows="3"/>
      <FormLabel name="sortOrder">{{ t('message.client.sortOrder') }}</FormLabel>
      <FormField name="sortOrder" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.sortOrder" :min="0" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel>{{ t('message.client.isEnabled') }}</FormLabel>
      <div>
        <ToggleSwitch v-model="ruleForm.isEnabled"/>
      </div>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField, TextareaField} from '../../../../../../components/crud-form-fields'
import {tenantGameAppRemoteConfigModeAddPost, tenantGameAppRemoteConfigModeUpdatePost} from '../../../../../../api/tenantGameAppRemoteConfigMode'

const props = defineProps({
  title: {
    type: String,
    default: '',
  },
})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()

const defaultForm = () => ({
  modeCode: '',
  displayName: '',
  description: '',
  sortOrder: 0,
  isEnabled: true,
})

const rules = {
  modeCode: [{required: true, message: () => t('message.client.pleaseInputModeCode'), trigger: 'blur'}],
  displayName: [{required: true, message: () => t('message.client.pleaseInputModeDisplayName'), trigger: 'blur'}],
}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>
