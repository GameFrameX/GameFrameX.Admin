<template>
  <BaseEditDialog ref="dialogRef" :title="props.title"
    :addApi="(params: any) => gameMailTemplateAddPost(params)"
    :updateApi="(params: any) => gameMailTemplateUpdatePost(params)"
    :rules="rules" :defaultForm="defaultForm"
    :beforeOpen="beforeOpen" :beforeSubmit="beforeSubmit"
    scrollable @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm, isEditor }">
      <TextField v-model="ruleForm.name" name="name" :label="t('message.gameBusiness.name')" :placeholder="t('message.gameBusiness.mail.pleaseEnterTemplateName')" maxlength="32"/>
      <TextField v-model="ruleForm.titleTemplate" name="titleTemplate" :label="t('message.gameBusiness.mail.titleTemplate')" :placeholder="t('message.gameBusiness.mail.pleaseEnterTitleTemplate')" maxlength="512"/>
      <FormLabel name="defaultMailType">{{ t('message.gameBusiness.mail.defaultMailType') }}</FormLabel>
      <FormField name="defaultMailType" v-slot="{ props: fProps, invalid, error }">
        <Select filter v-bind="fProps" v-model="ruleForm.defaultMailType" :options="getEnumMailTypeData" optionLabel="describe" optionValue="value" :placeholder="t('message.gameBusiness.mail.pleaseSelectMailTypeDefault')" showClear class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="sort">{{ t('message.gameBusiness.goods.sort') }}</FormLabel>
      <FormField name="sort" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.sort" :min="0" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="categoryId">{{ t('message.gameBusiness.mail.category') }}</FormLabel>
      <FormField name="categoryId" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.categoryId" :options="categoryList" optionLabel="value" optionValue="id" :placeholder="t('message.gameBusiness.mail.pleaseSelectCategory')" showClear filter class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <TextField v-model="ruleForm.subTitleTemplate" name="subTitleTemplate" :label="t('message.gameBusiness.mail.subTitleTemplate')" :placeholder="t('message.gameBusiness.mail.pleaseEnterSubTitleTemplate')" maxlength="512"/>
      <FormLabel name="defaultExpirationDays">{{ t('message.gameBusiness.mail.defaultExpirationDays') }}</FormLabel>
      <FormField name="defaultExpirationDays" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.defaultExpirationDays" :min="1" :max="365" class="w-full" :placeholder="t('message.gameBusiness.dayUnit')"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel>{{ t('message.gameBusiness.isEnabled') }}</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.isEnabled"/></div>
      <TextareaField v-model="ruleForm.bodyTemplate" name="bodyTemplate" :label="t('message.gameBusiness.mail.bodyTemplate')" :placeholder="t('message.gameBusiness.mail.pleaseEnterBodyTemplate')" maxlength="8192" :autoResize="true" rows="6"/>
      <template v-if="detectedVariables.length > 0">
        <FormLabel>{{ t('message.gameBusiness.mail.detectedVariables') }}</FormLabel>
        <div>
          <Tag v-for="(label, idx) in detectedVariableLabels" :key="idx" :value="label" severity="info" style="margin-right: 8px;"/>
        </div>
      </template>
      <FormLabel>{{ t('message.gameBusiness.mail.defaultAttachments') }}</FormLabel>
      <RewardItemsEditor v-model="ruleForm.defaultAttachmentsJson"
                         :rewardPlaceholder="t('message.gameBusiness.mail.pleaseSelectAttachment')"
                         :addLabel="t('message.gameBusiness.mail.addAttachment')"/>
      <TextField v-model="ruleForm.description" name="description" :label="t('message.gameBusiness.description')" :placeholder="t('message.gameBusiness.pleaseEnterDescription')" maxlength="512"/>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref, computed, onMounted} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField, TextareaField} from '../../../../../components/crud-form-fields'
import RewardItemsEditor from '/@/components/reward-items-editor/index.vue'
import {parseRewardItems, toRewardItemsJson} from '../../../../../components/reward-items-editor/rewardItems.ts'
import { gameMailTemplateAddPost, gameMailTemplateUpdatePost } from '../../../../../api/gameMailTemplate'
import { gameMailTemplateCategorySelectPost } from '../../../../../api/gameMailTemplateCategory'
import {systemEnumEnumDataListGet} from '../../../../../api/systemEnum';
import {extractVariablesFromAll, toVariableLabels} from '../../../../../utils/templateUtils.ts'
import {isEnabled} from '../../../../../utils/isEnabled.ts'

import {MAX_SELECT_SIZE} from '../../../../../constants/pagination.ts'

const props = defineProps({title: {type: String, default: ''}})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()

const categoryList = ref<any[]>([])
const getEnumMailTypeData = ref<any>([])

const loadCategoryList = async () => {
  const catRes = await gameMailTemplateCategorySelectPost({pageSize: MAX_SELECT_SIZE})
  categoryList.value = catRes.data.data ?? []
}

const defaultForm = () => ({isEnabled: true, sort: 0, defaultAttachmentsJson: []} as any)

const rules = {
  name: [{required: true, message: t('message.gameBusiness.pleaseEnterName'), trigger: 'blur'}],
  categoryId: [{required: true, message: t('message.gameBusiness.mail.pleaseSelectCategory'), trigger: 'change'}],
  titleTemplate: [{required: true, message: t('message.gameBusiness.pleaseEnterTitleTemplate'), trigger: 'blur'}],
  bodyTemplate: [{required: true, message: t('message.gameBusiness.pleaseEnterContentTemplate'), trigger: 'blur'}],
}

const beforeOpen = (form: any) => {
  form.isEnabled = isEnabled(form)


  form.defaultAttachmentsJson = parseRewardItems(form.defaultAttachmentsJson)
  loadCategoryList()
  return form
}

const beforeSubmit = (form: any) => {

  return {
    ...form,
    defaultAttachmentsJson: toRewardItemsJson(form.defaultAttachmentsJson),
  }
}

const detectedVariables = computed(() => {
  const ruleForm = dialogRef.value?.ruleForm
  if (!ruleForm) return []
  return extractVariablesFromAll(ruleForm.bodyTemplate, ruleForm.titleTemplate, ruleForm.subTitleTemplate)
})

const detectedVariableLabels = computed(() => toVariableLabels(detectedVariables.value))

onMounted(async () => {
  getEnumMailTypeData.value = (await systemEnumEnumDataListGet({ EnumName: 'GameMailType' })).data.data ?? []
})

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>
