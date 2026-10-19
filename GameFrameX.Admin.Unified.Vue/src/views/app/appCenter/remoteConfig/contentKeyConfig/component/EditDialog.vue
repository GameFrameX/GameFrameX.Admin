<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="tenantGameAppRemoteConfigKeyAddPost"
    :updateApi="tenantGameAppRemoteConfigKeyUpdatePost"
    :rules="rules"
    :defaultForm="defaultForm"
    :beforeOpen="beforeOpen"
    @reloadTable="emit('reloadTable')"
  >
    <template #default="{ ruleForm, isEditor }">
      <TextField v-model="ruleForm.keyName" name="keyName" :label="t('message.client.keyName')" :placeholder="t('message.client.pleaseInputKeyName')" :disabled="isEditor"/>
      <TextField v-model="ruleForm.displayName" name="displayName" :label="t('message.client.displayName')" :placeholder="t('message.client.pleaseInputDisplayName')"/>
      <FormLabel name="valueType">{{ t('message.client.valueType') }}</FormLabel>
      <FormField name="valueType" v-slot="{ props: fProps, invalid, error }">
        <Select filter v-bind="fProps" v-model="ruleForm.valueType" :options="valueTypeOptions"
                optionLabel="label" optionValue="value" :disabled="isEditor" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
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
import {tenantGameAppRemoteConfigKeyAddPost, tenantGameAppRemoteConfigKeyUpdatePost} from '../../../../../../api/tenantGameAppRemoteConfigKey'

const props = defineProps({
  title: {
    type: String,
    default: '',
  },
})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()

const defaultForm = () => ({
  keyName: '',
  displayName: '',
  valueType: 'string',
  description: '',
  sortOrder: 0,
  isEnabled: true,
})

const beforeOpen = (form: any, isEditor: boolean) => {
  if (isEditor) {
    return form
  }
  return {...defaultForm(), ...form}
}

const valueTypeOptions = [
  {label: 'String', value: 'string'},
  {label: 'Bool', value: 'bool'},
  {label: 'Long', value: 'long'},
]

const rules = {
  keyName: [{required: true, message: () => t('message.client.pleaseInputKeyName'), trigger: 'blur'}],
  displayName: [{required: true, message: () => t('message.client.pleaseInputDisplayName'), trigger: 'blur'}],
  valueType: [{required: true, message: () => t('message.client.pleaseSelectValueType'), trigger: 'change'}],
}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>
