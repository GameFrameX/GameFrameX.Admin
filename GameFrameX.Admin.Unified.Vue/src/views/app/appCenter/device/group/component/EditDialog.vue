<template>
  <BaseEditDialog ref="dialogRef" :title="props.title" :addApi="tenantDeviceGroupAddPost" :updateApi="tenantDeviceGroupUpdatePost" :beforeOpen="beforeOpen" :beforeSubmit="beforeSubmit" @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm, isEditor }">
      <TextField v-if="isEditor" :modelValue="appName(ruleForm.appId)" name="_appDisplay" :label="t('message.gameBusiness.appContext.gameApp')" disabled/>
      <TextField v-model="ruleForm.name" name="name" :label="t('message.client.groupName')" :placeholder="t('message.client.groupName')" maxlength="128"/>
      <TextareaField v-model="ruleForm.description" name="description" :label="t('message.client.descriptionInfo')" :placeholder="t('message.common.form.pleaseInput')" maxlength="512" :autoResize="true" rows="3"/>
      <FormLabel name="deviceIds">{{ t('message.client.deviceSelection') }}</FormLabel>
      <FormField name="deviceIds" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" clearable filter v-model="ruleForm.deviceIds" :placeholder="t('message.client.machineCode')" multiple :options="deviceOptions" optionLabel="deviceUniqueIdentifier" optionValue="id" :disabled="!hasApp" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
    </template>
  </BaseEditDialog>
</template>
<script lang="ts" setup>
import {ref, computed} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextareaField, TextField} from '../../../../../../components/crud-form-fields'
import { tenantDeviceGroupAddPost, tenantDeviceGroupUpdatePost } from '../../../../../../api/tenantDeviceGroup'
import { tenantGameAppDeviceSelectPost } from '../../../../../../api/tenantGameAppDevice'
import { useCurrentApp } from '../../../../../../composables/useCurrentApp.ts'
import {MAX_SELECT_SIZE} from '../../../../../../constants/pagination.ts'

const { appName } = useCurrentApp()

var props = defineProps({
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
const deviceList = ref<any>([])
const deviceOptions = computed(() => deviceList.value)
const hasApp = computed(() => !!props.appId && props.appId !== "0")

const loadDevices = async (appId?: number | string) => {
  if (!appId || appId === "0") {
    deviceList.value = []
    return
  }
  try {
    deviceList.value = (await tenantGameAppDeviceSelectPost({appId: appId, pageSize: MAX_SELECT_SIZE})).data.data ?? []
  } catch {
    deviceList.value = []
  }
}

const beforeOpen = (form: any, isEditor: boolean) => {
  if (!isEditor && form.appId == null) {
    form.appId = props.appId
  }
  loadDevices(form.appId ?? props.appId)
}

const beforeSubmit = (form: any) => {
  form.deviceIds = Array.isArray(form.deviceIds) ? form.deviceIds : []
  return form
}

const openDialog = async (row: any) => {
  dialogRef.value.openDialog(row)
}

defineExpose({openDialog})
</script>
