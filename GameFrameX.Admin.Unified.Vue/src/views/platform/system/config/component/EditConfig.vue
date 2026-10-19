<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    width="700px"
    idCheckMode="system"
    :addApi="addConfig"
    :updateApi="updateConfig"
    :rules="rules"
    :beforeOpen="beforeOpen"
    :beforeSubmit="beforeSubmit"
    @reloadTable="emit('updateData')"
  >
    <template #default="{ ruleForm }">
      <TextField v-model="ruleForm.name" name="name" :label="t('system.config.configName')" :placeholder="t('system.config.configName')"/>

      <FormLabel name="code">{{ t('system.config.configCode') }}</FormLabel>
      <FormField name="code" v-slot="{ props: fProps, invalid, error }">
        <InputText
          v-bind="fProps"
          v-model="ruleForm.code"
          :placeholder="t('system.config.configCode')"
          class="w-full"
          :disabled="ruleForm.sysFlag == 1"
          :aria-describedby="codeHelpId"
        />
        <FormHelpText :id="codeHelpId">{{ t('system.config.configCodeHelp') }}</FormHelpText>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <TextareaField v-model="ruleForm.value" name="value" :label="t('common.label.value')" :placeholder="t('common.label.value')" :autoResize="true" rows="2"/>

      <FormLabel name="sysFlag">{{ t('system.config.builtInParam') }}</FormLabel>
      <FormField name="sysFlag" v-slot="{ props: fProps, invalid, error }">
        <div class="flex gap-4 mt-1">
          <div class="flex align-items-center gap-2">
            <RadioButton
              v-bind="fProps"
              v-model="ruleForm.sysFlag"
              :value="1"
              :disabled="ruleForm.sysFlag == 1 && ruleForm.id != undefined"
            />
            <label>{{ t('common.label.yes') }}</label>
          </div>
          <div class="flex align-items-center gap-2">
            <RadioButton
              v-bind="fProps"
              v-model="ruleForm.sysFlag"
              :value="2"
              :disabled="ruleForm.sysFlag == 1 && ruleForm.id != undefined"
            />
            <label>{{ t('common.label.no') }}</label>
          </div>
        </div>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <TextField v-model="ruleForm.groupCode" name="groupCode" :label="t('system.config.groupCode')" :placeholder="t('system.config.groupCode')" :disabled="ruleForm.sysFlag == 1"/>

      <FormLabel name="orderNo">{{ t('common.label.sort') }}</FormLabel>
      <FormField name="orderNo" v-slot="{ props: fProps }">
        <InputNumber v-bind="fProps" v-model="ruleForm.orderNo" :placeholder="t('common.label.sort')" class="w-full"/>
      </FormField>

      <TextareaField v-model="ruleForm.remark" name="remark" :label="t('common.label.remark')" :placeholder="t('common.form.pleaseInput')" :autoResize="true" rows="2"/>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import { ref, useId } from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField, TextareaField} from '../../../../../components/crud-form-fields'
import {systemConfigAddPost, systemConfigUpdatePost} from '../../../../../api/systemConfig';

const props = defineProps({title: {type: String, default: ''}})
const emit = defineEmits(['updateData'])
const dialogRef = ref()
const codeHelpId = useId()

const addConfig = (params: any) => systemConfigAddPost(params)
const updateConfig = (params: any) => systemConfigUpdatePost(params)
const beforeOpen = (form: any) => ({
  ...form,
  sysFlag: form?.sysFlag == null ? 2 : Number(form.sysFlag),
})
const beforeSubmit = (form: any) => ({
  id: form.id,
  name: form.name,
  code: form.code,
  value: form.value,
  sysFlag: form.sysFlag == null ? form.sysFlag : Number(form.sysFlag),
  groupCode: form.groupCode,
  orderNo: form.orderNo,
  remark: form.remark,
  rowVersion: form.rowVersion,
})

const rules = {
  name: [{required: true, message: t('system.config.configName') + t('common.form.required'), trigger: 'blur'}],
  code: [{required: true, message: t('system.config.configCode') + t('common.form.required'), trigger: 'blur'}],
  value: [{required: true, message: t('common.label.value') + t('common.form.required'), trigger: 'blur'}],
  sysFlag: [{required: true, message: t('system.config.builtInParam') + t('common.form.required'), trigger: 'blur'}],
}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
})
</script>
