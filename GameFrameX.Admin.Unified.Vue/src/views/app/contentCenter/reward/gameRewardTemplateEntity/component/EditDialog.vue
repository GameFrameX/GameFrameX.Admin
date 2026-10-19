<template>
  <BaseEditDialog ref="dialogRef" :title="props.title"
                  :addApi="tenantGameRewardTemplateEntityAddPost"
                  :updateApi="tenantGameRewardTemplateEntityUpdatePost"
                  :rules="rules" :defaultForm="defaultForm"
                  :beforeOpen="beforeOpen" :beforeSubmit="beforeSubmit"
                  scrollable @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm }">
      <TextField v-model="ruleForm.name" name="name" :label="t('message.gameBusiness.name')" :placeholder="t('message.gameBusiness.pleaseEnterName')" maxlength="32"/>
      <FormLabel name="groupId">{{ t('message.gameBusiness.rewardTemplate.group') }}</FormLabel>
      <FormField name="groupId" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.groupId" :options="groupList" optionLabel="value" optionValue="id" :placeholder="t('message.gameBusiness.rewardTemplate.pleaseSelectGroup')" showClear filter class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="sort">{{ t('message.gameBusiness.goods.sort') }}</FormLabel>
      <FormField name="sort" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.sort" :min="0" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel>{{ t('message.gameBusiness.isEnabled') }}</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.isEnabled"/></div>
      <FormLabel>{{ t('message.gameBusiness.rewardTemplate.rewardItems') }}</FormLabel>
      <RewardItemsEditor v-model="ruleForm.rewardItemsJson" :appId="ruleForm.appId"/>
      <TextField v-model="ruleForm.description" name="description" :label="t('message.gameBusiness.description')" :placeholder="t('message.gameBusiness.pleaseEnterDescription')" maxlength="512"/>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import RewardItemsEditor from '/@/components/reward-items-editor/index.vue'
import {TextField} from '../../../../../../components/crud-form-fields'
import {parseRewardItems, toRewardItemsJson} from '../../../../../../components/reward-items-editor/rewardItems.ts'
import { tenantGameRewardTemplateEntityAddPost, tenantGameRewardTemplateEntityUpdatePost } from '../../../../../../api/tenantGameRewardTemplateEntity'
import { tenantGameRewardTemplateGroupEntitySelectPost } from '../../../../../../api/tenantGameRewardTemplateGroupEntity'
import {isEnabled} from '../../../../../../utils/isEnabled.ts'
import {normalizeAppId, type AppWorkspaceId} from '../../../../../../utils/appWorkspace.ts'
import {MAX_SELECT_SIZE} from '../../../../../../constants/pagination.ts'

const props = defineProps({title: {type: String, default: ''}, appId: {type: [Number, String], default: undefined}})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()
const groupList = ref<any[]>([])

const rules = {
  name: [{required: true, message: t('message.gameBusiness.pleaseEnterName'), trigger: 'blur'}],
  groupId: [{required: true, message: t('message.gameBusiness.rewardTemplate.pleaseSelectGroup'), trigger: 'change'}],
  rewardItemsJson: [{required: true, message: t('message.gameBusiness.rewardTemplate.pleaseAddRewardItem'), trigger: 'change'}],
}

const defaultForm = () => ({isEnabled: true, sort: 100, rewardItemsJson: [], snapshotSchemaVersion: 1} as any)

const loadGroupList = async (appId?: AppWorkspaceId) => {
  groupList.value = appId ? ((await tenantGameRewardTemplateGroupEntitySelectPost({appId, pageSize: MAX_SELECT_SIZE})).data.data ?? []) : []
}

const beforeOpen = (form: any, isEditor: boolean) => {
  form.appId = isEditor ? normalizeAppId(form.appId) : normalizeAppId(props.appId)
  form.isEnabled = isEnabled(form)
  form.rewardItemsJson = parseRewardItems(form.rewardItemsJson)
  loadGroupList(form.appId)
  return form
}

const beforeSubmit = (form: any) => {
  return {
    ...form,
    rewardItemsJson: toRewardItemsJson(form.rewardItemsJson),
    snapshotSchemaVersion: 1,
  }
}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>
