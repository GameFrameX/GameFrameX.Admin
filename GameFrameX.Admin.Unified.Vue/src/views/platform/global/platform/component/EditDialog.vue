<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="globalPlatformAddPost"
    :updateApi="globalPlatformUpdatePost"
    :rules="rules"
    @reloadTable="emit('reloadTable')"
  >
    <template #default="{ ruleForm }">
      <FormLabel name="name">{{ t('message.common.label.name') }}</FormLabel>
      <FormField name="name" v-slot="{ props: fProps, invalid, error }">
        <InputText v-bind="fProps" v-model="ruleForm.name" :placeholder="t('message.common.form.pleaseInput')" maxlength="32" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="description">{{ t('message.common.label.description') }}</FormLabel>
      <FormField name="description" v-slot="{ props: fProps, invalid, error }">
        <InputText v-bind="fProps" v-model="ruleForm.description" :placeholder="t('message.common.form.pleaseInput')" maxlength="512" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel>{{ t('message.client.isDefault') }}</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.isDefault"/></div>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import { ref } from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import { globalPlatformAddPost, globalPlatformUpdatePost } from '../../../../../api/globalPlatform'

const props = defineProps({
  title: {
    type: String,
    default: '',
  },
})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()

const rules = {
  name: [{ required: true, message: () => t('message.client.pleaseInputName'), trigger: 'blur' }],
}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>
