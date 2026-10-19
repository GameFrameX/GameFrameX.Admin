<template>
  <BaseEditDialog ref="dialogRef" :title="props.title" :addApi="tenantGameAppAssetPackageVersionAddPost" :updateApi="tenantGameAppAssetPackageVersionUpdatePost" :rules="rules" :beforeOpen="beforeOpen" scrollable @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm, isEditor }">
      <FormLabel name="channel">{{ t('message.client.channelName') }}</FormLabel>
      <FormField name="channel" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.channel" :options="queryParams.channel" optionLabel="label" optionValue="value" showClear filter :placeholder="t('message.client.pleaseSelectChannelPlaceholder')" :disabled="isEditor" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="platform">{{ t('message.client.platform') }}</FormLabel>
      <FormField name="platform" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.platform" :options="queryParams.platform" optionLabel="label" optionValue="value" showClear filter :placeholder="t('message.client.pleaseSelectPlatformPlaceholder')" :disabled="isEditor" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="appVersion">{{ t('message.client.gameVersion') }}</FormLabel>
      <FormField name="appVersion" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.appVersion" :options="queryParams.appVersion" optionLabel="label" optionValue="value" showClear filter :placeholder="t('message.client.pleaseSelectGameVersionPlaceholder')" :disabled="isEditor" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="package">{{ t('message.client.packageName') }}</FormLabel>
      <FormField name="package" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.package" :options="ensureValueOption(queryParams.package, ruleForm.package)" optionLabel="label" optionValue="value" showClear filter :placeholder="t('message.client.pleaseSelectPackagePlaceholder')" :disabled="isEditor" @change="onPackageChange" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="language">{{ t('message.client.language') }}</FormLabel>
      <FormField name="language" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.language" :options="queryParams.language" optionLabel="label" optionValue="value" showClear filter :placeholder="t('message.client.pleaseSelectLanguagePlaceholder')" :disabled="isEditor" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="assetPackageName">{{ t('message.client.assetPackage') }}</FormLabel>
      <FormField name="assetPackageName" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.assetPackageName" :options="queryParams.assetPackageName" optionLabel="label" optionValue="value" showClear filter :placeholder="t('message.client.pleaseSelectAssetPackagePlaceholder')" :disabled="isEditor" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel>{{ t('message.client.masterPackage') }}</FormLabel>
      <div><Select v-model="selectedPackageMaster" :options="queryParams.package" optionLabel="label" optionValue="value" showClear filter :placeholder="t('message.client.masterPackagePlaceholder')" class="w-full"/></div>
      <TextField v-model="ruleForm.rootPath" name="rootPath" :label="t('message.client.resourceRootPath')" :placeholder="t('message.client.pleaseInputResourceRootPath')" maxlength="64" :disabled="!!selectedPackageMaster"/>
      <FormLabel name="resourceVersion">{{ t('message.client.assetVersion') }}</FormLabel>
      <FormField name="resourceVersion" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.resourceVersion" :options="ensureValueOption(queryParams.resourceVersion, ruleForm.resourceVersion)" optionLabel="label" optionValue="value" showClear filter editable :placeholder="t('message.client.pleaseSelectResourceVersionPlaceholder')" :disabled="!!selectedPackageMaster || !ruleForm.package" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="deviceGroupId">{{ t('message.client.deviceGroup') }}</FormLabel>
      <FormField name="deviceGroupId" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.deviceGroupId" :options="deviceGroupOptions" optionLabel="name" optionValue="id" showClear filter :placeholder="t('message.client.pleaseSelectDeviceGroup')" :disabled="!!selectedPackageMaster" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <TextareaField v-model="ruleForm.description" name="description" :label="t('message.client.detailedDescription')" :placeholder="t('message.client.pleaseInputDetailedDescription')" maxlength="512" :autoResize="true" rows="3" :disabled="!!selectedPackageMaster"/>
    </template>
  </BaseEditDialog>
</template>
<script lang="ts" setup>
import {ref, computed, onMounted} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField, TextareaField} from '../../../../../../components/crud-form-fields'
import { tenantGameAppAssetPackageVersionAddPost, tenantGameAppAssetPackageVersionSelectPost, tenantGameAppAssetPackageVersionUpdatePost } from '../../../../../../api/tenantGameAppAssetPackageVersion'
import { globalLanguageSelectPost } from '../../../../../../api/globalLanguage'
import { tenantGameAppPackageListGet, tenantGameAppPackagePagePost } from '../../../../../../api/tenantGameAppPackage'
import { globalPlatformSelectPost } from '../../../../../../api/globalPlatform'
import { tenantChannelDefinitionAvailableGet } from '../../../../../../api/tenantChannelDefinition'
import { tenantGameAppVersionDefinitionSelectPost } from '../../../../../../api/tenantGameAppVersionDefinition'
import { tenantGameAppAssetPackageSelectPost } from '../../../../../../api/tenantGameAppAssetPackage'
import { tenantDeviceGroupPagePost } from '../../../../../../api/tenantDeviceGroup'
import {MAX_PAGE_SIZE, MAX_SELECT_SIZE} from '../../../../../../constants/pagination.ts'
import { mapAvailableChannelDefinitions } from '../../../../../../utils/channelDefinition.ts'

var props = defineProps({
  title: {
    type: String,
    default: '',
  },
  appId: {
    type: [Number, String],
    default: undefined,
  },
})
const emit = defineEmits(['reloadTable'])

const dialogRef = ref()
const queryParams = ref<any>({app: [], resourceVersion: []})

// 编辑态历史包名可能已不在当前应用启用列表，注入兜底选项使其只读回显，不静默替换。
const ensureValueOption = (options: any[], value: any) => {
  if (value === null || value === undefined || value === '') {
    return options
  }
  const list = options || []
  if (list.some((o: any) => o?.value === value)) {
    return list
  }
  return [{label: String(value), value}, ...list]
}
const deviceGroupData = ref<any>([])
const deviceGroupOptions = computed(() => deviceGroupData.value)
const packageInfoMap = ref<Record<string, { id?: number; isMaster: boolean; masterName?: string }>>({})
const selectedPackageMaster = ref<string>('')

const rules = {
  channel: [{required: true, message: () => t('message.client.pleaseSelectChannel'), trigger: 'change'}],
  platform: [{required: true, message: () => t('message.client.pleaseSelectPlatform'), trigger: 'change'}],
  appVersion: [{required: true, message: () => t('message.client.pleaseSelectGameVersion'), trigger: 'change'}],
  package: [{required: true, message: () => t('message.client.pleaseSelectPackage'), trigger: 'change'}],
  language: [{required: true, message: () => t('message.client.pleaseSelectLanguage'), trigger: 'change'}],
  assetPackageName: [{
    required: true,
    message: () => t('message.client.pleaseSelectAssetPackage'),
    trigger: 'change'
  }],
  resourceVersion: [{required: true, message: () => t('message.client.pleaseSelectResourceVersion'), trigger: 'change'}],
  rootPath: [{required: true, message: () => t('message.client.pleaseInputResourceRootPath'), trigger: 'blur'}],
  description: [{required: true, message: () => t('message.client.pleaseInputDetailedDescription'), trigger: 'blur'}],
  deviceGroupId: [{required: true, message: () => t('message.client.pleaseSelectDeviceGroup'), trigger: 'blur'}],
}

// 母包映射按当前应用过滤，避免跨应用母包错配；无应用时退回全量兜底。
const loadPackageInfo = async (appId?: number | string) => {
  try {
    const payload: any = {page: 1, pageSize: MAX_PAGE_SIZE}
    const id = appId
    if (id) {
      payload.appId = id
    }
    const res = await tenantGameAppPackagePagePost(payload)
    const items = res.data.data?.items ?? []
    const map: Record<string, { id?: number; isMaster: boolean; masterName?: string }> = {}
    items.forEach((p: any) => {
      const info: { id?: number; isMaster: boolean; masterName?: string } = p.masterPackageId
        ? {id: p.id, isMaster: false, masterName: p.masterPackageName}
        : {id: p.id, isMaster: true}
      const keys = [p.name, p.packageName].filter((x) => x !== null && x !== undefined && x !== '')
      if (p.masterPackageId) {
        info.masterName = p.masterPackageName
      }
      keys.forEach((key) => {
        map[String(key)] = info
      })
    })
    packageInfoMap.value = map
  } catch {
  }
}

const resolvePackageId = (packageName?: string) => {
  if (!packageName) {
    return undefined
  }
  return packageInfoMap.value[packageName]?.id
}

const loadResourceVersionOptions = async (appId?: number, packageName?: string) => {
  const id = appId
  const packageId = resolvePackageId(packageName)
  if (!id || !packageId) {
    queryParams.value.resourceVersion = []
    return
  }

  const res = await tenantGameAppAssetPackageVersionSelectPost({
    appId: id,
    appPackageId: packageId,
    pageSize: MAX_SELECT_SIZE,
  })
  queryParams.value.resourceVersion = res.data?.data || []
}

const onPackageChange = (val: any, clearResourceVersion = true) => {
  const ruleForm = dialogRef.value?.ruleForm
  const packageName = val?.value ?? val
  const info = packageInfoMap.value[packageName]
  if (ruleForm && clearResourceVersion) {
    ruleForm.resourceVersion = undefined
  }
  if (info && !info.isMaster && info.masterName) {
    selectedPackageMaster.value = info.masterName
  } else {
    selectedPackageMaster.value = ''
  }
  loadResourceVersionOptions(ruleForm?.appId, packageName)
}

const onQuerySelectorChannel = async () => {
  const res = await tenantChannelDefinitionAvailableGet({pageSize: MAX_SELECT_SIZE})
  queryParams.value.channel = mapAvailableChannelDefinitions(res.data.data || [], 'code')
}
const onQuerySelectorPlatform = async () => {
  const res = await globalPlatformSelectPost({pageSize: MAX_SELECT_SIZE})
  queryParams.value.platform = res.data.data
}
const onQuerySelectorAppVersion = async (appId?: number | string) => {
  const id = appId
  const res = await tenantGameAppVersionDefinitionSelectPost(id ? {appId: id, pageSize: MAX_SELECT_SIZE} : {pageSize: MAX_SELECT_SIZE})
  queryParams.value.appVersion = res.data.data
}
// 包名候选按当前应用加载，置顶 default 应用默认选项。
const buildPackageOptions = (packages: any[]) => {
  const options: any[] = [{label: t('message.client.appDefaultPackage'), value: 'default'}]
  for (const p of packages || []) {
    const val = p.packageName ?? p.name
    if (val === null || val === undefined || val === '') {
      continue
    }
    if (options.some((o) => o.value === val)) {
      continue
    }
    const version = p.packageVersion ? ` (${p.packageVersion})` : ''
    options.push({label: `${p.packageName || p.name}${version}`, value: val})
  }
  return options
}

const onQuerySelectorPackage = async (appId?: number | string) => {
  const id = appId
  if (!id) {
    queryParams.value.package = buildPackageOptions([])
    return
  }
  const res = await tenantGameAppPackageListGet({appId: id})
  queryParams.value.package = buildPackageOptions(res.data?.data || [])
}
const onQuerySelectorLanguage = async () => {
  const res = await globalLanguageSelectPost({pageSize: MAX_SELECT_SIZE})
  queryParams.value.language = res.data.data
}
const onQuerySelectorAssetPackageName = async (appId?: number | string) => {
  const id = appId
  const res = await tenantGameAppAssetPackageSelectPost(id ? {appId: id, pageSize: MAX_SELECT_SIZE} : {pageSize: MAX_SELECT_SIZE})
  queryParams.value.assetPackageName = res.data.data
}

const resolveInitialAppId = (form: any) => {
  const raw = props.appId ?? form?.appId
  // C270：appId 运行时为字符串（雪花 Id），保持字符串判断有效性，禁止 Number() 回转
  const id = raw === undefined || raw === null || raw === 0 || raw === '0' ? undefined : String(raw)
  return id || undefined
}

const beforeOpen = (form: any, isEditor: boolean) => {
  const appId = resolveInitialAppId(form)
  if (appId) form.appId = appId
  return form
}

const openDialog = (row: any) => {
  dialogRef.value.openDialog(row)
  // 编辑态按记录应用加载包名候选与母包映射，使历史包名能命中并正确解析母包。
  onQuerySelectorPackage(row?.appId)
  onQuerySelectorAppVersion(row?.appId)
  onQuerySelectorAssetPackageName(row?.appId)
  loadPackageInfo(row?.appId).then(() => {
    if (dialogRef.value?.ruleForm?.package) {
      onPackageChange(dialogRef.value.ruleForm.package, false)
    } else {
      selectedPackageMaster.value = ''
    }
  })
}

onMounted(async () => {
  // 设备分组按应用维度过滤（后端 appId 必填）；无应用上下文时跳过，避免空参 400
  const appId = resolveInitialAppId(null)
  if (!appId) {
    return
  }
  deviceGroupData.value = (await tenantDeviceGroupPagePost({
    page: 1,
    pageSize: MAX_PAGE_SIZE,
    appId
  })).data.data?.items ?? []
})

defineExpose({openDialog})
onQuerySelectorChannel()
onQuerySelectorPlatform()
onQuerySelectorAppVersion()
onQuerySelectorPackage()
onQuerySelectorLanguage()
onQuerySelectorAssetPackageName()
</script>
