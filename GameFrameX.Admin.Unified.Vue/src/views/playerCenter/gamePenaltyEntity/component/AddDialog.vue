<template>
  <BaseEditDialog ref="dialogRef" :title="props.title"
    :addApi="gamePenaltyAddPost"
    :updateApi="gamePenaltyAddPost"
    :rules="rules"
    :beforeOpen="beforeOpen" :beforeSubmit="beforeSubmit"
    scrollable @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm }">
      <FormLabel name="areaId">{{ t('message.gameBusiness.trustSafety.penalty.areaId') }}</FormLabel>
      <FormField name="areaId" v-slot="{ props: fProps, invalid, error }">
        <Select filter showClear v-bind="fProps" v-model="ruleForm.areaId" :options="areaIdOptions" optionLabel="label" optionValue="id" :placeholder="t('message.gameBusiness.trustSafety.penalty.allServer')" class="w-full" :disabled="areaIdOptions.length === 0"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="targetType">{{ t('message.gameBusiness.trustSafety.penalty.targetType') }}</FormLabel>
      <FormField name="targetType" v-slot="{ props: fProps, invalid, error }">
        <Select filter v-bind="fProps" v-model="ruleForm.targetType" :options="targetTypeOptions" optionLabel="label" optionValue="value" :placeholder="t('message.gameBusiness.trustSafety.penalty.pleaseSelectTargetType')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="targetId">{{ t('message.gameBusiness.trustSafety.penalty.targetId') }}</FormLabel>
      <FormField name="targetId" v-slot="{ props: fProps, invalid, error }">
        <InputText v-bind="fProps" v-model="ruleForm.targetId" :placeholder="t('message.gameBusiness.trustSafety.penalty.pleaseEnterTargetId')" maxlength="128" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="actionType">{{ t('message.gameBusiness.trustSafety.penalty.actionType') }}</FormLabel>
      <FormField name="actionType" v-slot="{ props: fProps, invalid, error }">
        <Select filter v-bind="fProps" v-model="ruleForm.actionType" :options="actionTypeOptions" optionLabel="label" optionValue="value" :placeholder="t('message.gameBusiness.trustSafety.penalty.pleaseSelectActionType')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="reason">{{ t('message.gameBusiness.trustSafety.penalty.reason') }}</FormLabel>
      <FormField name="reason" v-slot="{ props: fProps, invalid, error }">
        <Textarea v-bind="fProps" v-model="ruleForm.reason" :placeholder="t('message.gameBusiness.trustSafety.penalty.pleaseEnterReason')" rows="3" maxlength="1024" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="expireTime">{{ t('message.gameBusiness.trustSafety.penalty.expiresAt') }}</FormLabel>
      <FormField name="expireTime" v-slot="{ props: fProps, invalid, error }">
        <DatePicker v-bind="fProps" v-model="ruleForm.expireTime" showTime showClear class="w-full" :placeholder="t('message.gameBusiness.trustSafety.penalty.expiresAtPlaceholder')"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="sourceReportId">{{ t('message.gameBusiness.trustSafety.penalty.sourceReportId') }}</FormLabel>
      <FormField name="sourceReportId" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.sourceReportId" :min="1" :useGrouping="false" class="w-full" :placeholder="t('message.gameBusiness.trustSafety.penalty.sourceReportIdPlaceholder')"/>
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
import DatePicker from 'primevue/datepicker'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import { gamePenaltyAddPost } from '../../../../api/gamePenalty'
import { gameAreaSelectStringPost } from '../../../../api/gameArea'
import {MAX_SELECT_SIZE} from '../../../../constants/pagination.ts'
import {
  PENALTY_TARGET_TYPE_OPTIONS,
  PENALTY_ACTION_TYPE_OPTIONS,
} from '../constants'

const props = defineProps({
  title: { type: String, default: '' },
})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()

const areaIdOptions = ref<any[]>([])

const targetTypeOptions = computed(() => PENALTY_TARGET_TYPE_OPTIONS.map((o) => ({ label: t(o.labelKey), value: o.value })))
const actionTypeOptions = computed(() => PENALTY_ACTION_TYPE_OPTIONS.map((o) => ({ label: t(o.labelKey), value: o.value })))

const rules = {
  targetType: [{ required: true, message: t('message.gameBusiness.trustSafety.penalty.pleaseSelectTargetType'), trigger: 'change' }],
  targetId: [{ required: true, message: t('message.gameBusiness.trustSafety.penalty.pleaseEnterTargetId'), trigger: 'blur' }],
  actionType: [{ required: true, message: t('message.gameBusiness.trustSafety.penalty.pleaseSelectActionType'), trigger: 'change' }],
  reason: [{ required: true, message: t('message.gameBusiness.trustSafety.penalty.pleaseEnterReason'), trigger: 'blur' }],
}

const beforeOpen = async (form: any) => {
  areaIdOptions.value = []
  areaIdOptions.value = (await gameAreaSelectStringPost({ pageSize: MAX_SELECT_SIZE })).data.data ?? []
  // 回显：long(秒) → Date 供 DatePicker 显示；0 / null = 永久，保持 null
  if (form.expireTime && typeof form.expireTime === 'number') {
    form.expireTime = new Date(form.expireTime * 1000)
  } else {
    form.expireTime = null
  }
}

const beforeSubmit = (form: any) => ({
  ...form,
  // 提交：Date → long 秒；null / 空 = 永久，统一传 0
  expireTime: form.expireTime ? Math.floor(new Date(form.expireTime).getTime() / 1000) : 0,
})

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>
