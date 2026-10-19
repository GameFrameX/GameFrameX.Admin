<template>
  <BaseEditDialog
    ref="dialogRef"
    class="global-info-content-edit-dialog"
    :title="props.title"
    :addApi="tenantGameAppRemoteConfigAddPost"
    :updateApi="tenantGameAppRemoteConfigUpdatePost"
    :beforeSubmit="beforeSubmit"
    width="min(920px, calc(100vw - 32px))"
    scrollable
    @reloadTable="emit('reloadTable')"
  >
    <template #default="{ ruleForm }">
      <div v-block-loading="loading" class="global-info-content-edit-dialog__content">
        <FormLabel>{{ t('message.client.configContent') }}</FormLabel>
        <div class="global-info-content-edit-dialog__editor">
          <ModeContentEditor
            v-if="activeTab"
            v-model:currentModeCode="activeTab"
            v-model:activeModeCode="ruleForm.activeModeCode"
            :modeDefs="modeDefs"
            :contentKeyDefs="contentKeyDefs"
            :values="currentModeValues"
            :longValueErrors="currentModeLongValueErrors"
            @updateValue="setCurrentModeContentValue"
          />
        </div>
      </div>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {computed, ref} from 'vue'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import ModeContentEditor from './ModeContentEditor.vue'
import { tenantGameAppRemoteConfigAddPost, tenantGameAppRemoteConfigUpdatePost } from '../../../../../api/tenantGameAppRemoteConfig'
import {tenantGameAppRemoteConfigKeyPagePost} from '../../../../../api/tenantGameAppRemoteConfigKey'
import {tenantGameAppRemoteConfigModePagePost} from '../../../../../api/tenantGameAppRemoteConfigMode'
import {MAX_PAGE_SIZE} from '../../../../../constants/pagination.ts'
import {showError} from '../../../../../utils/message.ts'
import {normalizeAppId} from '../../../../../utils/appWorkspace.ts'

const props = defineProps({
  title: {
    type: String,
    default: '',
  },
})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()
const loading = ref(false)

interface ContentKeyDef {
  id: number;
  keyName: string;
  displayName: string;
  valueType: 'string' | 'bool' | 'long';
  description?: string;
  sortOrder: number;
}

interface ModeDef {
  id: number;
  modeCode: string;
  displayName: string;
  description?: string;
  sortOrder: number;
}

const modeDefs = ref<ModeDef[]>([])
const contentKeyDefs = ref<ContentKeyDef[]>([])
const modeContentValues = ref<Record<string, Record<string, any>>>({})
const modeLongValueErrors = ref<Record<string, Record<string, string>>>({})
const activeTab = ref('')
const MAX_SAFE_INTEGER_TEXT = '9007199254740991'

const currentModeValues = computed(() => ensureModeContentValues(activeTab.value))
const currentModeLongValueErrors = computed(() => {
  if (!modeLongValueErrors.value[activeTab.value]) modeLongValueErrors.value[activeTab.value] = {}
  return modeLongValueErrors.value[activeTab.value]
})

const loadContentKeyDefs = async () => {
  const res = await tenantGameAppRemoteConfigKeyPagePost({
    page: 1,
    pageSize: MAX_PAGE_SIZE,
    isEnabled: true,
    field: 'sortOrder',
    order: 'ascending',
  })
  const seen = new Set<string>()
  contentKeyDefs.value = (res.data.data?.items ?? [])
    .map((item: any) => ({
      id: item.id,
      keyName: item.keyName,
      displayName: item.displayName,
      valueType: item.valueType ?? 'string',
      description: item.description,
      sortOrder: item.sortOrder ?? 0,
    }))
    .filter((item: ContentKeyDef) => {
      const keyName = item.keyName?.trim()
      if (!keyName || seen.has(keyName)) return false
      item.keyName = keyName
      item.valueType = item.valueType || 'string'
      seen.add(keyName)
      return true
    })
}

const loadModeDefs = async () => {
  const res = await tenantGameAppRemoteConfigModePagePost({
    page: 1,
    pageSize: MAX_PAGE_SIZE,
    isEnabled: true,
    field: 'sortOrder',
    order: 'ascending',
  })
  modeDefs.value = (res.data.data?.items ?? []).map((item: any) => ({
    id: item.id,
    modeCode: item.modeCode,
    displayName: item.displayName,
    description: item.description,
    sortOrder: item.sortOrder ?? 0,
  })).filter((item: ModeDef) => !!item.modeCode)
}

const getDefaultValue = (def: ContentKeyDef) => {
  if (def.valueType === 'bool') return false
  if (def.valueType === 'long') return '0'
  return ''
}

const initDefaultValues = () => {
  const values: Record<string, any> = {}
  for (const def of contentKeyDefs.value) values[def.keyName] = getDefaultValue(def)
  return values
}

const parseValueForEditor = (def: ContentKeyDef, value: any) => {
  if (def.valueType === 'bool') return Boolean(value)
  if (def.valueType === 'long') return value === null || value === undefined ? '' : String(value)
  return value ?? ''
}

const parseContentToValues = (contentStr: string) => {
  const values = initDefaultValues()
  if (contentStr) {
    try {
      const obj = JSON.parse(contentStr)
      for (const def of contentKeyDefs.value) {
        if (obj[def.keyName] !== undefined) values[def.keyName] = parseValueForEditor(def, obj[def.keyName])
      }
    } catch { /* invalid JSON, use defaults */ }
  }
  return values
}

const ensureModeContentValues = (modeCode: string) => {
  if (!modeCode) return initDefaultValues()
  if (!modeContentValues.value[modeCode]) modeContentValues.value[modeCode] = initDefaultValues()
  return modeContentValues.value[modeCode]
}

const setModeContentValue = (modeCode: string, keyName: string, value: any) => {
  if (!modeCode) return
  ensureModeContentValues(modeCode)[keyName] = value
  syncModeContents()
}

const setCurrentModeContentValue = (keyName: string, value: any) => {
  setModeContentValue(activeTab.value, keyName, value)
}

const parseLongValue = (value: any) => {
  const text = String(value ?? '').trim()
  if (!/^-?\d+$/.test(text)) {
    return {valid: false, message: t('message.client.longIntegerRequired'), value: 0}
  }

  const longValue = BigInt(text)
  const maxSafeInteger = BigInt(MAX_SAFE_INTEGER_TEXT)
  if (longValue > maxSafeInteger || longValue < -maxSafeInteger) {
    return {valid: false, message: t('message.client.longSafeIntegerExceeded', {max: MAX_SAFE_INTEGER_TEXT}), value: 0}
  }

  return {valid: true, message: '', value: Number(text)}
}

const buildContentObject = (values: Record<string, any>, errors: Record<string, string>) => {
  const obj: Record<string, any> = {}
  let valid = true
  for (const def of contentKeyDefs.value) {
    const val = values[def.keyName]
    errors[def.keyName] = ''
    if (def.valueType === 'bool') {
      obj[def.keyName] = Boolean(val)
      continue
    }

    if (def.valueType === 'long') {
      const parsed = parseLongValue(val)
      if (!parsed.valid) {
        errors[def.keyName] = parsed.message
        valid = false
      }
      obj[def.keyName] = parsed.value
      continue
    }

    obj[def.keyName] = val !== undefined && val !== null ? String(val) : ''
  }

  return {obj, valid}
}

const parseModeContents = (modeContentsStr: string, fallbackContent: string) => {
  let modeContents: Record<string, any> = {}
  if (modeContentsStr) {
    try {
      const parsed = JSON.parse(modeContentsStr)
      if (parsed && typeof parsed === 'object' && !Array.isArray(parsed)) modeContents = parsed
    } catch { /* invalid JSON, use defaults */ }
  }

  const values: Record<string, Record<string, any>> = {}
  const errors: Record<string, Record<string, string>> = {}
  for (const modeDef of modeDefs.value) {
    const modeCode = modeDef.modeCode
    const modeValue = modeContents[modeCode]
    const modeContentStr = modeValue === undefined
      ? ''
      : typeof modeValue === 'string'
        ? modeValue
        : JSON.stringify(modeValue)
    values[modeCode] = parseContentToValues(modeContentStr || (modeCode === 'formal' ? fallbackContent : ''))
    errors[modeCode] = {}
  }

  return {values, errors}
}

const buildModeContents = () => {
  const modeContents: Record<string, any> = {}
  let valid = true
  for (const modeDef of modeDefs.value) {
    const modeCode = modeDef.modeCode
    if (!modeLongValueErrors.value[modeCode]) modeLongValueErrors.value[modeCode] = {}
    const built = buildContentObject(modeContentValues.value[modeCode] ?? {}, modeLongValueErrors.value[modeCode])
    modeContents[modeCode] = built.obj
    if (!built.valid) valid = false
  }

  return {modeContents, valid}
}

const syncModeContents = () => {
  const {modeContents} = buildModeContents()
  dialogRef.value.ruleForm.modeContents = JSON.stringify(modeContents)
}

const openDialog = async (row: any) => {
  dialogRef.value.isEditor = true
  dialogRef.value.openDialog(row)
  loading.value = true
  try {
    await Promise.all([loadContentKeyDefs(), loadModeDefs()])

    const ruleForm = dialogRef.value.ruleForm
    if (!ruleForm.activeModeCode) {
      ruleForm.activeModeCode = modeDefs.value.some(x => x.modeCode === 'formal') ? 'formal' : modeDefs.value[0]?.modeCode
    }

    const parsed = parseModeContents(ruleForm.modeContents || '', ruleForm.content || '')
    modeContentValues.value = parsed.values
    modeLongValueErrors.value = parsed.errors
    activeTab.value = ruleForm.activeModeCode || modeDefs.value[0]?.modeCode || ''
    syncModeContents()
  } finally {
    loading.value = false
  }
}

const beforeSubmit = (ruleForm: any) => {
  const appId = normalizeAppId(ruleForm.appId)
  if (appId === undefined) {
    showError(t('gameBusiness.appWorkspace.appIdRequired'))
    return false
  }

  const {modeContents, valid} = buildModeContents()
  if (!valid) {
    showError(t('message.client.configValueInvalid'))
    return false
  }

  ruleForm.appId = appId
  ruleForm.modeContents = JSON.stringify(modeContents)
  ruleForm.content = JSON.stringify(modeContents[ruleForm.activeModeCode] ?? {})
  return ruleForm
}

defineExpose({openDialog})
</script>

<style scoped>
.global-info-content-edit-dialog__content {
  display: contents;
}

:global(.global-info-content-edit-dialog .base-edit-dialog-grid) {
  grid-template-columns: 120px minmax(0, 1fr);
}

:global(.global-info-content-edit-dialog .form-label) {
  align-self: start;
  min-width: 0;
  line-height: 1.35;
  white-space: normal !important;
  word-break: break-word;
}

.global-info-content-edit-dialog__editor {
  min-width: 0;
}
</style>
