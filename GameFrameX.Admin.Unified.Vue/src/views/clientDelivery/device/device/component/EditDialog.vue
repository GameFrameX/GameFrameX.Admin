<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="gameAppDeviceAddPost"
    :updateApi="gameAppDeviceUpdatePost"
    :rules="rules"
    :beforeOpen="beforeOpen"
    @reloadTable="emit('reloadTable')"
  >
    <template #default="{ ruleForm, isEditor }">
            <TextField v-model="ruleForm.name" name="name" :label="t('message.common.label.name')" :placeholder="t('message.common.label.name')" maxlength="128"/>
      <TextareaField v-model="ruleForm.description" name="description" :label="t('message.client.detailedDescription')" :placeholder="t('message.common.form.pleaseInput')" maxlength="512" :autoResize="true" rows="3"/>
      <TextareaField v-model="ruleForm.deviceUniqueIdentifier" name="deviceUniqueIdentifier" :label="t('message.client.machineCode')" :placeholder="t('message.client.machineCode')" maxlength="512" :autoResize="true" rows="3"/>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import { ref } from 'vue'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextareaField, TextField} from '../../../../../components/crud-form-fields'
import { gameAppDeviceAddPost, gameAppDeviceUpdatePost } from '../../../../../api/gameAppDevice'

const props = defineProps({
  title: {
    type: String,
    default: '',
  },
})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()

const rules = {
  name: [{
    required: true,
    message: () => t('message.common.form.pleaseInput'),
    trigger: 'change',
  }],
  deviceUniqueIdentifier: [{
    required: true,
    message: () => t('message.client.pleaseInputMachineCode'),
    trigger: 'change',
  }],
}

const beforeOpen = (form: any, isEditor: boolean) => {

}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>
