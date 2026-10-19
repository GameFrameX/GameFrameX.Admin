<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    width="700px"
    idCheckMode="system"
    :addApi="addDictType"
    :updateApi="updateDictType"
    :rules="rules"
    :beforeOpen="beforeOpen"
    :beforeSubmit="beforeSubmit"
    @reloadTable="emits('query')"
  >
    <template #default="{ ruleForm }">
      <FormLabel name="name">{{ t('system.dict.dictName') }}</FormLabel>
      <FormField name="name" v-slot="{ props: fProps, invalid, error }">
        <InputText v-bind="fProps" v-model="ruleForm.name" :placeholder="t('system.dict.dictName')" maxlength="64" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <FormLabel name="code">{{ t('system.dict.dictCode') }}</FormLabel>
      <FormField name="code" v-slot="{ props: fProps, invalid, error }">
        <InputText v-bind="fProps" v-model="ruleForm.code" :placeholder="t('system.dict.dictCode')" maxlength="64" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <FormLabel name="isEnabled">{{ t('common.label.status') }}</FormLabel>
      <FormField name="isEnabled" v-slot="{ props: fProps }">
        <ToggleSwitch v-bind="fProps" v-model="ruleForm.isEnabled"/>
      </FormField>

      <FormLabel name="orderNo">{{ t('common.label.sort') }}</FormLabel>
      <FormField name="orderNo" v-slot="{ props: fProps }">
        <InputNumber v-bind="fProps" v-model="ruleForm.orderNo" :placeholder="t('common.label.sort')" class="w-full"/>
      </FormField>

      <FormLabel name="remark">{{ t('common.label.remark') }}</FormLabel>
      <FormField name="remark" v-slot="{ props: fProps }">
        <Textarea
          v-bind="fProps"
          v-model="ruleForm.remark"
          :placeholder="t('common.form.pleaseInput')"
          class="w-full"
          :autoResize="true"
          rows="2"
        />
      </FormField>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup name="systemEditDictType">
import { ref } from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {globalDictionaryTypeAddPost, globalDictionaryTypeUpdatePost} from '../../../../../api/globalDictionaryType';
import { isEnabled } from '../../../../../utils/isEnabled.ts'

const props = defineProps({
  title: String,
})
const emits = defineEmits(['query'])
const dialogRef = ref()

const addDictType = (params: any) => globalDictionaryTypeAddPost(params)
const updateDictType = (params: any) => globalDictionaryTypeUpdatePost(params)
const beforeOpen = (form: any) => ({
  ...form,
  isEnabled: isEnabled(form),
  orderNo: form?.orderNo ?? 100,
})
const beforeSubmit = (form: any) => ({
  id: form.id,
  name: form.name,
  code: form.code,
  isEnabled: isEnabled(form),
  orderNo: form.orderNo,
  remark: form.remark,
  rowVersion: form.rowVersion,
})

const rules = {
  name: [{required: true, message: t('system.dict.dictName') + t('common.form.required'), trigger: 'blur'}],
  code: [{required: true, message: t('system.dict.dictCode') + t('common.form.required'), trigger: 'blur'}],
}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
})
</script>
