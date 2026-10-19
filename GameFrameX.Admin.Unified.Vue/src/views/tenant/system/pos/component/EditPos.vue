<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="(params: any) => tenantPositionAddPost(params)"
    :updateApi="(params: any) => tenantPositionUpdatePost(params)"
    :rules="rules"
    idCheckMode="system"
    @reloadTable="emits('query')"
  >
    <template #default="{ ruleForm }">
      <FormLabel name="name">{{ t('system.pos.posName') }}</FormLabel>
      <FormField name="name" v-slot="{ props: fProps, invalid, error }">
        <InputText v-bind="fProps" v-model="ruleForm.name" :placeholder="t('system.pos.posName')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="code">{{ t('system.pos.posCode') }}</FormLabel>
      <FormField name="code" v-slot="{ props: fProps, invalid, error }">
        <InputText v-bind="fProps" v-model="ruleForm.code" :placeholder="t('system.pos.posCode')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel>{{ t('common.label.sort') }}</FormLabel>
      <div><InputNumber v-model="ruleForm.orderNo" :placeholder="t('common.label.sort')" class="w100"/></div>
      <FormLabel>{{ t('common.label.status') }}</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.isEnabled"/></div>
      <FormLabel>{{ t('common.label.remark') }}</FormLabel>
      <div><Textarea v-model="ruleForm.remark" :placeholder="t('common.form.pleaseInput')" class="w-full" autoResize/></div>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup name="systemEditPos">

import {ref} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {tenantPositionAddPost, tenantPositionUpdatePost} from '../../../../../api/tenantPosition';

import InputText from 'primevue/inputtext';
import Textarea from 'primevue/textarea';
import InputNumber from 'primevue/inputnumber';
import ToggleSwitch from 'primevue/toggleswitch';

const props = defineProps({title: String})
const emits = defineEmits(['query'])
const dialogRef = ref()

const rules = {
  name: [{required: true, message: t('system.pos.posName') + t('common.form.required'), trigger: 'blur'}],
  code: [{required: true, message: t('system.pos.posCode') + t('common.form.required'), trigger: 'blur'}],
}

defineExpose({openDialog: (row: any) => dialogRef.value?.openDialog(row)})
</script>
