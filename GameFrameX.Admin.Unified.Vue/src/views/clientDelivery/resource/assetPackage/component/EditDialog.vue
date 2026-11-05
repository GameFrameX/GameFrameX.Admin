<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="gameAppAssetPackageAddPost"
    :updateApi="gameAppAssetPackageUpdatePost"
    :rules="rules"
    :beforeOpen="beforeOpen"
    :beforeSubmit="beforeSubmit"
    @reloadTable="emit('reloadTable')"
  >
    <template #default="{ ruleForm }">
      <TextField v-model="ruleForm.name" name="name" :label="t('message.common.label.name')" :placeholder="t('message.common.form.pleaseInput')" maxlength="32"/>
      <TextField v-model="ruleForm.description" name="description" :label="t('message.client.detailedDescription')" :placeholder="t('message.client.pleaseInputDetailedDescription')" maxlength="512"/>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import { ref } from 'vue'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField} from '../../../../../components/crud-form-fields'
import { gameAppAssetPackageAddPost, gameAppAssetPackageUpdatePost } from '../../../../../api/gameAppAssetPackage'

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
  description: [{ required: true, message: () => t('message.client.pleaseInputDetailedDescription'), trigger: 'blur' }],
}

const beforeOpen = (form: any, _isEditor: boolean) => {


  return form
}

const beforeSubmit = (form: any) => {


  return {
    ...form,
  }
}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>
