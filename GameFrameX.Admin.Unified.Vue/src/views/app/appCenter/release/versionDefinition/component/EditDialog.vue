<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="tenantGameAppVersionDefinitionAddPost"
    :updateApi="tenantGameAppVersionDefinitionUpdatePost"
    :rules="rules"
    :beforeOpen="beforeOpen"
    @reloadTable="emit('reloadTable')"
  >
    <template #default="{ ruleForm }">
      <TextField v-model="ruleForm.name" name="name" :label="t('message.common.label.name')" :placeholder="t('message.common.form.pleaseInput')" maxlength="32"/>
      <TextField v-model="ruleForm.description" name="description" :label="t('message.common.label.description')" :placeholder="t('message.common.form.pleaseInput')" maxlength="512"/>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref} from 'vue'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField} from '../../../../../../components/crud-form-fields'
import { tenantGameAppVersionDefinitionAddPost, tenantGameAppVersionDefinitionUpdatePost } from '../../../../../../api/tenantGameAppVersionDefinition'
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
}

const beforeOpen = async (form: any, _isEditor: boolean) => {
  const appId = normalizeAppId(props.appId)
  if (appId) form.appId = appId
}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>
