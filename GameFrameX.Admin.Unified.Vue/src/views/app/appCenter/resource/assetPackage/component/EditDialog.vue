<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="tenantGameAppAssetPackageAddPost"
    :updateApi="tenantGameAppAssetPackageUpdatePost"
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
import {TextField} from '../../../../../../components/crud-form-fields'
import { tenantGameAppAssetPackageAddPost, tenantGameAppAssetPackageUpdatePost } from '../../../../../../api/tenantGameAppAssetPackage'
import {normalizeAppId} from '../../../../../../utils/appWorkspace.ts'

const props = defineProps({
  title: {
    type: String,
    default: '',
  },
  appId: {
    type: [Number, String],
    default: undefined,
  },
})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()

const rules = {
  name: [{ required: true, message: () => t('message.client.pleaseInputName'), trigger: 'blur' }],
  description: [{ required: true, message: () => t('message.client.pleaseInputDetailedDescription'), trigger: 'blur' }],
}

const resolveGameAppId = (form: any) => {
  const appId = Number(normalizeAppId(props.appId) ?? form?.appId)
  return Number.isFinite(appId) && appId > 0 ? appId : undefined
}

const beforeOpen = (form: any, _isEditor: boolean) => {
  const appId = normalizeAppId(props.appId)
  if (appId) form.appId = appId
  return form
}

const beforeSubmit = (form: any) => {
  const appId = resolveGameAppId(form)
  if (!appId) {
    return false
  }
  return {
    ...form,
    appId: appId,
  }
}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>
