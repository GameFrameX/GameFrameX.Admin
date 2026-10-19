<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="(params: any) => tenantGameTimeJobEntityAddPost(params)"
    :updateApi="(params: any) => tenantGameTimeJobEntityUpdatePost(params)"
    :rules="rules"
    :beforeOpen="beforeOpen"
    @reloadTable="emit('reloadTable')"
  >
    <template #default="{ ruleForm }">
      <TextField v-model="ruleForm.name" name="name" :label="t('message.gameBusiness.name')" :placeholder="t('message.gameBusiness.pleaseEnterName')"/>
      <TextField v-model="ruleForm.description" name="description" :label="t('message.gameBusiness.redeemcode.description')" :placeholder="t('message.gameBusiness.pleaseEnterDescription')"/>
      <TextField v-model="ruleForm.cron" name="cron" :label="t('message.gameBusiness.timeJob.cron')" placeholder="0 0/5 * * * ?"/>
      <FormLabel>{{ t('message.gameBusiness.isOnce') }}</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.isOnce"/></div>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref} from 'vue'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField} from '../../../../../components/crud-form-fields'
import { tenantGameTimeJobEntityAddPost, tenantGameTimeJobEntityUpdatePost } from '../../../../../api/tenantGameTimeJobEntity'
import {normalizeAppId} from '../../../../../utils/appWorkspace.ts'
import ToggleSwitch from 'primevue/toggleswitch';

var props = defineProps({
  title: {type: String, default: ''},
  appId: {type: [Number, String], default: undefined},
})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()

const rules = {
  name: [{required: true, message: t('message.gameBusiness.pleaseEnterName'), trigger: 'blur'}],
  cron: [{required: true, message: t('message.gameBusiness.timeJob.cron'), trigger: 'blur'}],
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
