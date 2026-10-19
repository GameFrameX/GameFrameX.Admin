<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="tenantChannelDefinitionAddPost"
    :updateApi="tenantChannelDefinitionUpdatePost"
    :rules="rules"
    @reloadTable="emit('reloadTable')"
  >
    <template #default="{ ruleForm }">
      <TextField v-model="ruleForm.code" name="code" :label="t('message.client.channelCode')" :placeholder="t('message.common.form.pleaseInput')" maxlength="64"/>
      <TextField v-model="ruleForm.name" name="name" :label="t('message.common.label.name')" :placeholder="t('message.common.form.pleaseInput')" maxlength="64"/>
      <TextField v-model="ruleForm.platformCode" name="platformCode" :label="t('message.client.platform')" :placeholder="t('message.client.allPlatform')" maxlength="64"/>
      <FormLabel name="channelType">{{ t('message.client.channelType') }}</FormLabel>
      <FormField name="channelType" v-slot="{ props: fProps, invalid, error }">
        <Select filter v-bind="fProps" v-model="ruleForm.channelType" :options="typeOptions" optionLabel="label" optionValue="value" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <TextField v-model="ruleForm.description" name="description" :label="t('message.common.label.description')" :placeholder="t('message.common.form.pleaseInput')" maxlength="512"/>
      <FormLabel>{{ t('message.client.isDefault') }}</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.isDefault"/></div>
      <FormLabel>{{ t('message.client.isEnabled') }}</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.isEnabled"/></div>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import { ref } from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField} from '../../../../../components/crud-form-fields'
import { tenantChannelDefinitionAddPost, tenantChannelDefinitionUpdatePost } from '../../../../../api/tenantChannelDefinition'

const props = defineProps({
  title: {
    type: String,
    default: '',
  },
})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()
const typeOptions = [
  { label: t('message.client.channelTypeOfficial'), value: 1 },
  { label: t('message.client.channelTypeStore'), value: 2 },
  { label: t('message.client.channelTypeAd'), value: 3 },
  { label: t('message.client.channelTypeCustom'), value: 4 },
]

const rules = {
  code: [{ required: true, message: () => t('message.client.pleaseInputChannelCode'), trigger: 'blur' }],
  name: [{ required: true, message: () => t('message.client.pleaseInputName'), trigger: 'blur' }],
  channelType: [{ required: true, message: () => t('message.client.pleaseSelectChannelType'), trigger: 'change' }],
}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog({
    scope: 2,
    channelType: 4,
    platformCode: 'all',
    isEnabled: true,
    ...row,
  }),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>
