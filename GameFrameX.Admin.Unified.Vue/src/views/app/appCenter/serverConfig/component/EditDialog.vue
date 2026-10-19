<template>
  <BaseEditDialog ref="dialogRef" class="server-config-edit-dialog" :title="props.title" :addApi="tenantGameAppServerConfigAddPost" :updateApi="tenantGameAppServerConfigUpdatePost" :rules="rules" :beforeSubmit="beforeSubmit" scrollable @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm, isEditor }">
      <div class="server-config-edit-dialog-content">
      <FormLabel name="appId">{{ t('message.gameBusiness.app.appId') }}</FormLabel>
      <FormField name="appId" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" filter v-model="ruleForm.appId" :placeholder="t('message.common.form.pleaseSelect')" :disabled="isEditor" :options="querySelections.app" optionLabel="label" optionValue="value" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <TextField v-model="ruleForm.key" name="key" :label="t('message.client.serverConfigKey')" :placeholder="t('message.client.serverConfigKeyPlaceholder')" maxlength="256"/>
      <FormLabel>{{ t('message.client.serverConfigMd5') }}</FormLabel>
      <div>
        <span v-if="isEditor" :title="currentMd5" class="md5-readonly">{{ currentMd5 }}</span>
        <span v-else class="md5-readonly md5-pending">-</span>
        <div class="md5-help">{{ t('message.client.serverConfigMd5Help') }}</div>
      </div>
      <FormLabel name="config">{{ t('message.client.serverConfigContent') }}</FormLabel>
      <FormField name="config" v-slot="{ props: fProps, invalid, error }">
        <Textarea v-bind="fProps" v-model="ruleForm.config" :placeholder="t('message.client.serverConfigContentPlaceholder')" :autoResize="true" rows="14" class="w-full code-textarea" @update:model-value="validateJson"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
        <Message v-else-if="jsonError" severity="error" size="small" variant="simple">{{ jsonError }}</Message>
      </FormField>
      <FormLabel>{{ t('message.client.isEnabled') }}</FormLabel>
      <div>
        <ToggleSwitch v-model="ruleForm.enabled"/>
        <div class="md5-help">{{ t('message.client.serverConfigEnabledHelp') }}</div>
      </div>
      <TextareaField v-model="ruleForm.description" name="description" :label="t('message.client.descriptionInfo')" :placeholder="t('message.common.form.pleaseInput')" maxlength="512" :autoResize="true" rows="2"/>
      </div>
    </template>
  </BaseEditDialog>
</template>
<script lang="ts" setup>
import {ref} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import Textarea from 'primevue/textarea'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField, TextareaField} from '../../../../../components/crud-form-fields'
import {showError} from '../../../../../utils/message.ts'
import { tenantGameAppServerConfigAddPost, tenantGameAppServerConfigUpdatePost } from '../../../../../api/tenantGameAppServerConfig'
import { tenantGameAppEntityListGet } from '../../../../../api/tenantGameAppEntity'
import {normalizeAppId} from '../../../../../utils/appWorkspace.ts'

var props = defineProps({
  title: {
    type: String,
    default: '',
  },
})
const emit = defineEmits(['reloadTable'])

const dialogRef = ref()
const currentMd5 = ref('')
const jsonError = ref('')

const querySelections = ref<any>({
  app: [],
})

const rules = {
  key: [{required: true, message: () => t('message.client.serverConfigKeyRequired'), trigger: 'blur'}],
  config: [{required: true, message: () => t('message.client.serverConfigContentRequired'), trigger: 'blur'}],
}

// 输入实时 JSON 预校验（提交前 beforeSubmit 兜底拦截，同 deviceWhitelistJson 先例）
const validateJson = (value: string) => {
  if (value === undefined || value === null || String(value).trim() === '') {
    jsonError.value = ''
    return
  }
  try {
    JSON.parse(String(value))
    jsonError.value = ''
  } catch (e: any) {
    jsonError.value = t('message.client.serverConfigJsonInvalid') + (e?.message ? `：${e.message}` : '')
  }
}

const onAppQuery = async () => {
  const res = await tenantGameAppEntityListGet({})
  const apps = res.data?.data || []
  const options: any[] = []
  for (const a of apps) {
    const id = normalizeAppId(a.id)
    if (id === undefined || a.isEnabled === false) {
      continue
    }
    options.push({label: a.appName || a.name || String(id), value: id})
  }
  querySelections.value.app = options
}

const openDialog = (row: any) => {
  currentMd5.value = row?.md5 ? String(row.md5) : ''
  jsonError.value = ''
  dialogRef.value.openDialog(row)
}

const beforeSubmit = (ruleForm: any) => {
  const appId = normalizeAppId(ruleForm.appId)
  if (appId === undefined) {
    showError(t('gameBusiness.appWorkspace.appIdRequired'))
    return false
  }
  ruleForm.appId = appId
  ruleForm.enabled = ruleForm.enabled !== false
  ruleForm.description = ruleForm.description ?? ''
  try {
    JSON.parse(String(ruleForm.config ?? ''))
  } catch (e: any) {
    jsonError.value = t('message.client.serverConfigJsonInvalid') + (e?.message ? `：${e.message}` : '')
    showError(jsonError.value)
    return false
  }
  return ruleForm
}

defineExpose({
  openDialog,
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
onAppQuery()
</script>

<style scoped>
.server-config-edit-dialog-content {
  display: contents;
}

:global(.server-config-edit-dialog .base-edit-dialog-grid) {
  grid-template-columns: 120px minmax(0, 1fr);
}

:global(.server-config-edit-dialog .form-label) {
  align-self: center;
  min-width: 0;
  line-height: 1.35;
  white-space: normal !important;
  word-break: break-word;
}

.md5-readonly {
  font-family: var(--font-mono, monospace);
  word-break: break-all;
}

.md5-pending {
  color: var(--p-text-muted-color, inherit);
}

.md5-help {
  margin-top: 4px;
  color: var(--p-text-muted-color, inherit);
  font-size: 12px;
  line-height: 1.4;
}

.code-textarea {
  font-family: var(--font-mono, monospace);
  font-size: 12px;
}
</style>
