<template>
  <BaseEditDialog ref="dialogRef" :title="props.title"
    :addApi="gameReportAddPost"
    :updateApi="gameReportAddPost"
    :rules="rules"
    scrollable @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm }">
      <FormLabel name="reporterId">{{ t('message.gameBusiness.trustSafety.report.reporterId') }}</FormLabel>
      <FormField name="reporterId" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.reporterId" :min="1" class="w-full" :placeholder="t('message.gameBusiness.trustSafety.report.pleaseEnterReporterId')" :useGrouping="false"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="targetType">{{ t('message.gameBusiness.trustSafety.report.targetType') }}</FormLabel>
      <FormField name="targetType" v-slot="{ props: fProps, invalid, error }">
        <Select filter v-bind="fProps" v-model="ruleForm.targetType" :options="targetTypeOptions" optionLabel="label" optionValue="value" :placeholder="t('message.gameBusiness.trustSafety.report.pleaseSelectTargetType')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="targetId">{{ t('message.gameBusiness.trustSafety.report.targetId') }}</FormLabel>
      <FormField name="targetId" v-slot="{ props: fProps, invalid, error }">
        <InputText v-bind="fProps" v-model="ruleForm.targetId" :placeholder="t('message.gameBusiness.trustSafety.report.pleaseEnterTargetId')" maxlength="128" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="reportType">{{ t('message.gameBusiness.trustSafety.report.reportType') }}</FormLabel>
      <FormField name="reportType" v-slot="{ props: fProps, invalid, error }">
        <Select filter v-bind="fProps" v-model="ruleForm.reportType" :options="reportTypeOptions" optionLabel="label" optionValue="value" :placeholder="t('message.gameBusiness.trustSafety.report.pleaseSelectReportType')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="scene">{{ t('message.gameBusiness.trustSafety.report.scene') }}</FormLabel>
      <FormField name="scene" v-slot="{ props: fProps, invalid, error }">
        <InputText v-bind="fProps" v-model="ruleForm.scene" :placeholder="t('message.gameBusiness.trustSafety.report.pleaseEnterScene')" maxlength="256" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="evidenceJson">{{ t('message.gameBusiness.trustSafety.report.evidenceJson') }}</FormLabel>
      <FormField name="evidenceJson" v-slot="{ props: fProps, invalid, error }">
        <Textarea v-bind="fProps" v-model="ruleForm.evidenceJson" :placeholder="t('message.gameBusiness.trustSafety.report.pleaseEnterEvidenceJson')" rows="4" maxlength="4096" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import { ref, computed } from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Textarea from 'primevue/textarea'
import Select from 'primevue/select'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import { gameReportAddPost } from '../../../../api/gameReport'
import {
  PENALTY_TARGET_TYPE_OPTIONS,
  REPORT_TYPE_OPTIONS,
} from '../constants'

const props = defineProps({
  title: { type: String, default: '' },
})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()

const targetTypeOptions = computed(() =>
  PENALTY_TARGET_TYPE_OPTIONS.map((o) => ({ label: t(o.labelKey), value: o.value })),
)
const reportTypeOptions = computed(() =>
  REPORT_TYPE_OPTIONS.map((o) => ({ label: t(o.labelKey), value: o.value })),
)

const rules = {
  reporterId: [{ required: true, message: t('message.gameBusiness.trustSafety.report.pleaseEnterReporterId'), trigger: 'blur' }],
  targetType: [{ required: true, message: t('message.gameBusiness.trustSafety.report.pleaseSelectTargetType'), trigger: 'change' }],
  targetId: [{ required: true, message: t('message.gameBusiness.trustSafety.report.pleaseEnterTargetId'), trigger: 'blur' }],
  reportType: [{ required: true, message: t('message.gameBusiness.trustSafety.report.pleaseSelectReportType'), trigger: 'change' }],
  evidenceJson: [{ required: true, message: t('message.gameBusiness.trustSafety.report.pleaseEnterEvidenceJson'), trigger: 'blur' }],
}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>
