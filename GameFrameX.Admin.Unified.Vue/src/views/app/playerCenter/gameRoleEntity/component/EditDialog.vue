<template>
  <BaseEditDialog ref="dialogRef" :title="props.title" :addApi="tenantGameRoleEntityAddPost" :updateApi="tenantGameRoleEntityUpdatePost" :rules="rules" :beforeOpen="beforeOpen" @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm, isEditor }">
      <TextField v-model="ruleForm.description" name="description" :label="t('message.gameBusiness.description')" :placeholder="t('message.gameBusiness.pleaseEnterDescription')" maxlength="512"/>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref} from 'vue'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField} from '../../../../../components/crud-form-fields'
import { tenantGameRoleEntityAddPost, tenantGameRoleEntityUpdatePost } from '../../../../../api/tenantGameRoleEntity'
import {normalizeAppId} from '../../../../../utils/appWorkspace.ts'

const props = defineProps({
  title: {type: String, default: ''},
  appId: {type: [Number, String], default: undefined},
})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()

const rules = {
}

const beforeOpen = (form: any, isEditor: boolean) => {
  const appId = normalizeAppId(props.appId)
  if (appId) form.appId = appId
}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>