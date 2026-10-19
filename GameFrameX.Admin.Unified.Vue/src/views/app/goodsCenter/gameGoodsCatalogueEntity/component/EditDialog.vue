<template>
  <BaseEditDialog ref="dialogRef" :title="props.title" :addApi="tenantGameGoodsCatalogueEntityAddPost" :updateApi="tenantGameGoodsCatalogueEntityUpdatePost" :rules="rules" :beforeOpen="beforeOpen" :beforeSubmit="beforeSubmit" @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm }">
      <TextField v-model="ruleForm.name" name="name" :label="t('message.gameBusiness.name')" :placeholder="t('message.gameBusiness.pleaseEnterName')" maxlength="64"/>
      <TextField v-model="ruleForm.description" name="description" :label="t('message.gameBusiness.description')" :placeholder="t('message.gameBusiness.pleaseEnterDescription')" maxlength="512"/>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref} from 'vue'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField} from '../../../../../components/crud-form-fields'
import { tenantGameGoodsCatalogueEntityAddPost, tenantGameGoodsCatalogueEntityUpdatePost } from '../../../../../api/tenantGameGoodsCatalogueEntity'
import {normalizeAppId} from '../../../../../utils/appWorkspace.ts'
import {showError} from '../../../../../utils/message.ts'

const props = defineProps({
  title: {type: String, default: ''},
  appId: {type: [Number, String], default: undefined},
})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()

const rules = {
  name: [{required: true, message: t('message.gameBusiness.pleaseEnterName'), trigger: 'blur'}],
  description: [{required: true, message: t('message.gameBusiness.pleaseEnterDescription'), trigger: 'blur'}],
}

const currentRouteAppId = () => normalizeAppId(props.appId)

const beforeOpen = (form: any) => {
  form.appId = form.appId ?? currentRouteAppId()
}

const beforeSubmit = (form: any) => {
  const appId = currentRouteAppId()
  if (!appId) {
    showError(t('message.gameBusiness.appContext.gameAppRequired'))
    return false
  }
  return {...form, appId}
}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>
