<template>
  <BaseEditDialog ref="dialogRef" :title="props.title"
    :addApi="tenantGamePenaltyAppealEntityAddPost"
    :updateApi="tenantGamePenaltyAppealEntityAddPost"
    :rules="rules"
    :beforeSubmit="beforeSubmit"
    scrollable @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm }">
      <FormLabel name="penaltyId">{{ t('message.gameBusiness.trustSafety.appeal.penaltyId') }}</FormLabel>
      <FormField name="penaltyId" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.penaltyId" :min="1" :useGrouping="false" class="w-full" :placeholder="t('message.gameBusiness.trustSafety.appeal.pleaseEnterPenaltyId')"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="appellantId">{{ t('message.gameBusiness.trustSafety.appeal.appellantId') }}</FormLabel>
      <FormField name="appellantId" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.appellantId" :min="1" :useGrouping="false" class="w-full" :placeholder="t('message.gameBusiness.trustSafety.appeal.pleaseEnterAppellantId')"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <TextareaField v-model="ruleForm.reason" name="reason" :label="t('message.gameBusiness.trustSafety.appeal.reason')" :placeholder="t('message.gameBusiness.trustSafety.appeal.pleaseEnterReason')" rows="4" maxlength="1024"/>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import { ref } from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import InputNumber from 'primevue/inputnumber'
import Textarea from 'primevue/textarea'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextareaField} from '../../../../../components/crud-form-fields'
import { tenantGamePenaltyAppealEntityAddPost } from '../../../../../api/tenantGamePenaltyAppealEntity'

const props = defineProps({
  title: { type: String, default: '' },
})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()

const rules = {
  penaltyId: [{ required: true, message: t('message.gameBusiness.trustSafety.appeal.pleaseEnterPenaltyId'), trigger: 'blur' }],
  appellantId: [{ required: true, message: t('message.gameBusiness.trustSafety.appeal.pleaseEnterAppellantId'), trigger: 'blur' }],
  reason: [{ required: true, message: t('message.gameBusiness.trustSafety.appeal.pleaseEnterReason'), trigger: 'blur' }],
}

const beforeSubmit = (form: any) => form

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>
