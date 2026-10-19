<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="globalLanguageAddPost"
    :updateApi="globalLanguageUpdatePost"
    :rules="rules"
    @reloadTable="emit('reloadTable')"
  >
    <template #default="{ ruleForm }">
      <TextField v-model="ruleForm.name" name="name" :label="t('message.common.label.name')" :placeholder="t('message.common.form.pleaseInput')" maxlength="32"/>
      <TextField v-model="ruleForm.code" name="code" :label="t('message.client.languageCode')" :placeholder="t('message.client.languageCode')" maxlength="32"/>
      <TextField v-model="ruleForm.description" name="description" :label="t('message.common.label.description')" :placeholder="t('message.common.form.pleaseInput')" maxlength="512"/>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import { ref } from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField} from '../../../../../components/crud-form-fields'
import { globalLanguageAddPost, globalLanguageUpdatePost } from '../../../../../api/globalLanguage'

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
