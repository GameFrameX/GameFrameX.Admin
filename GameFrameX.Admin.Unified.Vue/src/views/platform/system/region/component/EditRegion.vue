<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="(params: any) => systemRegionAddPost(params)"
    :updateApi="(params: any) => systemRegionUpdatePost(params)"
    :rules="rules"
    :beforeSubmit="beforeSubmit"
    idCheckMode="system"
    @reloadTable="emit('query')"
  >
    <template #default="{ ruleForm }">
      <FormLabel name="name">{{ t('system.region.regionName') }}</FormLabel>
      <FormField name="name" v-slot="{ props: fProps, invalid, error }">
        <InputText v-bind="fProps" v-model="ruleForm.name" :placeholder="t('system.region.regionName')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="code">{{ t('system.region.regionCode') }}</FormLabel>
      <FormField name="code" v-slot="{ props: fProps, invalid, error }">
        <InputText v-bind="fProps" v-model="ruleForm.code" :placeholder="t('system.region.regionCode')" maxlength="32" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="cityCode">{{ t('system.region.cityCode') }}</FormLabel>
      <FormField name="cityCode" v-slot="{ props: fProps, invalid, error }">
        <InputText v-bind="fProps" v-model="ruleForm.cityCode" :placeholder="t('system.region.cityCode')" maxlength="6" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel>{{ t('common.label.sort') }}</FormLabel>
      <div><InputNumber v-model="ruleForm.orderNo" :placeholder="t('common.label.sort')" class="w100"/></div>
      <FormLabel>{{ t('common.label.remark') }}</FormLabel>
      <div><Textarea v-model="ruleForm.remark" :placeholder="t('common.form.pleaseInput')" class="w-full" autoResize/></div>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>

import {ref} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {systemRegionAddPost, systemRegionUpdatePost} from '../../../../../api/systemRegion';

import InputText from 'primevue/inputtext';
import Textarea from 'primevue/textarea';
import InputNumber from 'primevue/inputnumber';

var props = defineProps({title: {type: String, default: ''}})
const emit = defineEmits(['query'])
const dialogRef = ref()

const rules = {
  name: [{required: true, message: t('system.region.regionName') + t('common.form.required'), trigger: 'blur'}],
  code: [
    {required: true, message: t('system.region.regionCode') + t('common.form.required'), trigger: 'blur'},
    {max: 32, message: t('system.region.regionCode') + '不能超过32个字符', trigger: 'blur'},
  ],
  cityCode: [{max: 6, message: t('system.region.cityCode') + '不能超过6个字符', trigger: 'blur'}],
}

const beforeSubmit = (form: any) => ({
  id: form.id,
  pid: form.pid,
  name: form.name,
  code: form.code,
  cityCode: form.cityCode,
  orderNo: form.orderNo,
  remark: form.remark,
  rowVersion: form.rowVersion,
})

defineExpose({openDialog: (row: any) => dialogRef.value?.openDialog(row)})
</script>
