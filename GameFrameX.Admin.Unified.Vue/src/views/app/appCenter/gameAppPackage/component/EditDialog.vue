<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="tenantGameAppPackageAddPost"
    :updateApi="tenantGameAppPackageUpdatePost"
    :rules="rules"
    :defaultForm="defaultForm"
    :beforeOpen="beforeOpen"
    :beforeSubmit="beforeSubmit"
    @reloadTable="emit('reloadTable')"
  >
    <template #default="{ ruleForm, isEditor }">

      <FormLabel>{{ t('gameBusiness.package.parentPackage') }}</FormLabel>
      <div>
        <Select filter v-model="ruleForm.parentPackageId" :options="parentPackageOptions(ruleForm.appId)" optionLabel="label" optionValue="id" :placeholder="ruleForm.appId ? t('gameBusiness.package.parentPackagePlaceholder') : t('gameBusiness.package.parentPackageNoApp')" :disabled="!ruleForm.appId" showClear class="w-full"/>
        <Message severity="secondary" size="small" variant="simple" style="margin-top: 4px">{{ t('gameBusiness.package.parentPackageHint') }}</Message>
      </div>

      <TextField v-model="ruleForm.packageName" name="packageName" :label="t('gameBusiness.package.packageName')" :placeholder="t('gameBusiness.package.packageName')" maxlength="256" :disabled="isEditor"/>

      <FormLabel name="platformCode">{{ t('gameBusiness.package.platform') }}</FormLabel>
      <FormField name="platformCode" v-slot="{ props: fProps, invalid, error }">
        <Select filter v-bind="fProps" v-model="ruleForm.platformCode" :options="platformOptions" optionLabel="value" optionValue="value" :disabled="isEditor" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <FormLabel name="channelId">{{ t('gameBusiness.package.channel') }}</FormLabel>
      <FormField name="channelId" v-slot="{ props: fProps, invalid, error }">
        <Select filter v-bind="fProps" v-model="ruleForm.channelId" :options="channelOptions" optionLabel="label" optionValue="value" :disabled="isEditor" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <TextField v-model="ruleForm.name" name="name" :label="t('gameBusiness.package.name')" :placeholder="t('gameBusiness.package.name')" maxlength="128"/>

      <TextField v-model="ruleForm.packageVersion" name="packageVersion" :label="t('gameBusiness.package.packageVersion')" :placeholder="t('gameBusiness.package.packageVersion')" maxlength="64"/>

      <FormLabel>{{ t('gameBusiness.package.isDefault') }}</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.isDefault"/></div>

      <TextField v-model="ruleForm.remark" name="remark" :label="t('gameBusiness.app.remark')" :placeholder="t('gameBusiness.app.remark')" maxlength="512"/>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField} from '../../../../../components/crud-form-fields'
import { tenantGameAppPackageAddPost, tenantGameAppPackageUpdatePost, tenantGameAppPackagePagePost } from '../../../../../api/tenantGameAppPackage'
import { globalPlatformSelectPost } from '../../../../../api/globalPlatform'
import { tenantChannelDefinitionAvailableGet } from '../../../../../api/tenantChannelDefinition'
import { MAX_PAGE_SIZE, MAX_SELECT_SIZE } from '../../../../../constants/pagination.ts'
import {normalizeAppId} from '../../../../../utils/appWorkspace.ts'
import { mapAvailableChannelDefinitions } from '../../../../../utils/channelDefinition.ts'

const props = defineProps({
  title: { type: String, default: '' },
  appId: { type: [Number, String], default: undefined },
})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()
const platformOptions = ref<any[]>([])
const channelOptions = ref<any[]>([])
const allPackages = ref<any[]>([])
const editingId = ref<number | undefined>(undefined)

const rules = {
  name: [{ required: true, message: t('gameBusiness.package.nameRequired'), trigger: 'blur' }],
  packageName: [{ required: true, message: t('gameBusiness.package.packageNameRequired'), trigger: 'blur' }],
  platformCode: [{ required: true, message: t('gameBusiness.package.platformRequired'), trigger: 'change' }],
  channelId: [{ required: true, message: t('gameBusiness.package.channelRequired'), trigger: 'change' }],
}

const defaultForm = () => ({
  appId: normalizeAppId(props.appId),
  parentPackageId: undefined,
  isDefault: false,
})

const firstOptionValue = (options: any[], ...keys: string[]) => {
  const first = options[0]
  if (!first) {
    return undefined
  }
  for (const key of keys) {
    if (first[key] !== undefined && first[key] !== null && first[key] !== '') {
      return first[key]
    }
  }
  return undefined
}

const beforeOpen = (form: any, isEditor: boolean) => {
  const appId = normalizeAppId(props.appId)
  if (appId) form.appId = appId
  if (isEditor) {
    return
  }
  form.platformCode = form.platformCode ?? firstOptionValue(platformOptions.value, 'value', 'code')
  form.channelId = form.channelId ?? firstOptionValue(channelOptions.value, 'value', 'id')
}

const parentPackageOptions = (appId: any) => {
  if (!appId) {
    return []
  }
  return allPackages.value
    .filter((p) => p.appId === String(appId) && !p.parentPackageId && p.isEnabled !== false && p.id !== editingId.value)
    .map((p) => ({
      id: p.id,
      label: p.packageVersion ? `${p.packageName} / ${p.packageVersion}` : p.packageName,
    }))
}

const loadOptions = async () => {
  const [platforms, channels, packages] = await Promise.all([
    globalPlatformSelectPost({ page: 1, pageSize: MAX_SELECT_SIZE }),
    tenantChannelDefinitionAvailableGet({pageSize: MAX_SELECT_SIZE}),
    tenantGameAppPackagePagePost({ page: 1, pageSize: MAX_PAGE_SIZE, appId: normalizeAppId(props.appId) }),
  ])
  platformOptions.value = platforms.data.data || []
  channelOptions.value = mapAvailableChannelDefinitions(channels.data.data || [], 'id')
  const packageResult = packages.data.data || []
  allPackages.value = packageResult.items || packageResult.Items || (Array.isArray(packageResult) ? packageResult : [])
}

const beforeSubmit = (form: any) => {
  form.platformName = form.platformCode
  form.parentPackageId = form.parentPackageId || null
  return form
}

defineExpose({
  openDialog: async (row: any) => {
    await loadOptions()
    editingId.value = row?.id
    dialogRef.value?.openDialog(row)
  },
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>
