<template>
  <BaseEditDialog ref="dialogRef" :title="props.title" scrollable scrollMaxHeight="65vh" :addApi="tenantGameAreaClassifyEntityAddPost" :updateApi="tenantGameAreaClassifyEntityUpdatePost" :rules="rules" :beforeOpen="beforeOpen" @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm, isEditor }">
      <TextField v-model="ruleForm.name" name="name" :label="t('message.gameBusiness.name')" :placeholder="t('message.gameBusiness.pleaseEnterName')" maxlength="64"/>
      <TextField v-model="ruleForm.description" name="description" :label="t('message.gameBusiness.description')" :placeholder="t('message.gameBusiness.pleaseEnterDescription')" maxlength="512"/>
      <TextField v-model="ruleForm.localizationKey" name="localizationKey" :label="t('message.gameBusiness.localizationKey')" :placeholder="t('message.gameBusiness.pleaseEnterLocalizationKey')" maxlength="512"/>
      <TextField v-model="ruleForm.httpManagerUrl" name="httpManagerUrl" :label="t('message.gameBusiness.area.serverManagementUrl')" :placeholder="t('message.gameBusiness.area.pleaseEnterServerManagementUrl')" maxlength="512"/>
      <TextField v-model="ruleForm.dockerRegister" name="dockerRegister" :label="t('message.gameBusiness.area.dockerRegistry')" :placeholder="t('message.gameBusiness.area.pleaseEnterDockerRegistry')" maxlength="512"/>
      <TextField v-model="ruleForm.imageName" name="imageName" :label="t('message.gameBusiness.area.imageSource')" :placeholder="t('message.gameBusiness.area.pleaseEnterImageSource')" maxlength="512"/>
      <TextField v-model="ruleForm.dockerUserName" name="dockerUserName" :label="t('message.gameBusiness.area.repositoryUsername')" :placeholder="t('message.gameBusiness.area.pleaseEnterRepositoryUsername')" maxlength="512"/>
      <TextField v-model="ruleForm.dockerPassword" name="dockerPassword" :label="t('message.gameBusiness.area.repositoryPassword')" :placeholder="t('message.gameBusiness.area.pleaseEnterRepositoryPassword')" maxlength="512"/>
      <TextField v-model="ruleForm.containerNetName" name="containerNetName" :label="t('message.gameBusiness.area.containerNetworkName')" :placeholder="t('message.gameBusiness.area.pleaseEnterContainerNetworkName')" maxlength="512"/>
      <TextField v-model="ruleForm.containerSubnet" name="containerSubnet" :label="t('message.gameBusiness.area.containerSubnet')" :placeholder="t('message.gameBusiness.area.pleaseEnterContainerSubnet')" maxlength="512"/>
      <TextField v-model="ruleForm.containerGateway" name="containerGateway" :label="t('message.gameBusiness.area.containerGateway')" :placeholder="t('message.gameBusiness.area.pleaseEnterContainerGateway')" maxlength="512"/>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref, computed} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField} from '../../../../../components/crud-form-fields'
import { tenantGameAreaClassifyEntityAddPost, tenantGameAreaClassifyEntityUpdatePost } from '../../../../../api/tenantGameAreaClassifyEntity'
import {normalizeAppId} from '../../../../../utils/appWorkspace.ts'

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

const beforeOpen = (form: any, isEditor: boolean) => {
  const appId = normalizeAppId(props.appId)
  if (appId) form.appId = appId
}

const isEditor = computed({
  get: () => dialogRef.value?.isEditor,
  set: (val) => { if (dialogRef.value) dialogRef.value.isEditor = val },
})

const openDialog = (row: any) => {
  dialogRef.value?.openDialog(row)
}

defineExpose({openDialog, isEditor})
</script>
