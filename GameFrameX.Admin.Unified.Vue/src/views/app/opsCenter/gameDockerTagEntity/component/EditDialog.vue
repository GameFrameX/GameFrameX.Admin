<template>
  <BaseEditDialog ref="dialogRef" :title="props.title" scrollable :addApi="tenantGameDockerTagEntityAddPost" :updateApi="tenantGameDockerTagEntityUpdatePost" :rules="rules" :beforeOpen="beforeOpen" @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm, isEditor }">
      <TextField v-model="ruleForm.name" name="name" :label="t('message.gameBusiness.name')" :placeholder="t('message.gameBusiness.pleaseEnterName')" maxlength="64"/>
      <TextareaField v-model="ruleForm.serverVersion" name="serverVersion" :label="t('message.gameBusiness.area.tag')" :placeholder="t('message.gameBusiness.pleaseEnterDescription')" maxlength="512" :autoResize="true" rows="3"/>
      <TextareaField v-model="ruleForm.description" name="description" :label="t('message.gameBusiness.description')" :placeholder="t('message.gameBusiness.pleaseEnterDescription')" maxlength="512" :autoResize="true" rows="3"/>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref} from 'vue'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField, TextareaField} from '../../../../../components/crud-form-fields'
import { tenantGameDockerTagEntityUpdatePost, tenantGameDockerTagEntityAddPost } from '../../../../../api/tenantGameDockerTagEntity'
import {normalizeAppId} from '../../../../../utils/appWorkspace.ts'

const props = defineProps({
  title: {type: String, default: ''},
  appId: {type: [Number, String], default: undefined},
})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()

const rules = {
  name: [{required: true, message: t('message.gameBusiness.pleaseEnterName'), trigger: 'blur'}],
  serverVersion: [{required: true, message: t('message.gameBusiness.pleaseEnterGameVersion'), trigger: 'blur'}],
}

const beforeOpen = async (form: any, isEditor: boolean) => {
  const appId = normalizeAppId(props.appId)
  if (appId) form.appId = appId
}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>
