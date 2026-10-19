<template>
  <BaseEditDialog ref="dialogRef" :title="props.title"
    :addApi="tenantGameAreaEntityAddPost"
    :updateApi="tenantGameAreaEntityUpdatePost"
    :rules="rules"
    :beforeOpen="beforeOpen" :beforeSubmit="beforeSubmit"
    scrollable @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm }">
      <TextField v-model="ruleForm.name" name="name" :label="t('message.gameBusiness.name')" :placeholder="t('message.gameBusiness.pleaseEnterName')" maxlength="64"/>
      <FormLabel>{{ t('message.gameBusiness.isEnabled') }}</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.isEnabled"/></div>
      <FormLabel>{{ t('message.gameBusiness.isDefault') }}</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.isDefault"/></div>
      <FormLabel>{{ t('message.gameBusiness.isRecommend') }}</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.isRecommend"/></div>
      <TextField v-model="ruleForm.localizationKey" name="localizationKey" :label="t('message.gameBusiness.localizationKey')" :placeholder="t('message.gameBusiness.pleaseEnterLocalizationKey')" maxlength="512"/>
      <FormLabel name="serverStatus">{{ t('message.gameBusiness.area.serverStatus') }}</FormLabel>
      <FormField name="serverStatus" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.serverStatus" :options="serverStatusOptions" optionLabel="label" optionValue="value" :placeholder="t('message.gameBusiness.area.pleaseSelectServerStatus')" class="w-full" showClear filter/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <TextField v-model="ruleForm.serverContainerId" name="serverContainerId" :label="t('message.gameBusiness.area.containerId')" :placeholder="t('message.gameBusiness.area.pleaseEnterContainerId')"/>
      <div class="section-title">{{ t('message.gameBusiness.area.endpointConfigGroup') }}</div>
      <TextField v-model="ruleForm.httpManageUrl" name="httpManageUrl" :label="t('message.gameBusiness.area.httpManageUrl')" :placeholder="t('message.gameBusiness.area.pleaseEnterHttpManageUrl')" maxlength="255"/>
      <TextField v-model="ruleForm.clientDispatchApiPrefix" name="clientDispatchApiPrefix" :label="t('message.gameBusiness.area.clientDispatchApiPrefix')" :placeholder="t('message.gameBusiness.area.pleaseEnterClientDispatchApiPrefix')" maxlength="64"/>
      <TextField v-model="ruleForm.fulfillmentApiPrefix" name="fulfillmentApiPrefix" :label="t('message.gameBusiness.area.fulfillmentApiPrefix')" :placeholder="t('message.gameBusiness.area.pleaseEnterFulfillmentApiPrefix')" maxlength="64"/>
      <TextField v-model="ruleForm.gmApiPrefix" name="gmApiPrefix" :label="t('message.gameBusiness.area.gmApiPrefix')" :placeholder="t('message.gameBusiness.area.pleaseEnterGmApiPrefix')" maxlength="64"/>
      <TextField v-model="ruleForm.controlApiPrefix" name="controlApiPrefix" :label="t('message.gameBusiness.area.controlApiPrefix')" :placeholder="t('message.gameBusiness.area.pleaseEnterControlApiPrefix')" maxlength="64"/>
      <FormLabel>{{ t('message.gameBusiness.area.enableClientGmApi') }}</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.enableClientGmApi"/></div>
      <TextField v-model="ruleForm.tcpSocketUrl" name="tcpSocketUrl" :label="t('message.gameBusiness.area.tcpSocketUrl')" :placeholder="t('message.gameBusiness.area.pleaseEnterTcpSocketUrl')" maxlength="255"/>
      <FormLabel name="capacity">{{ t('message.gameBusiness.area.maxPlayers') }}</FormLabel>
      <FormField name="capacity" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.capacity" :placeholder="t('message.gameBusiness.area.pleaseEnterMaxPlayers')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="creationTime">{{ t('message.gameBusiness.area.enabledDate') }}</FormLabel>
      <FormField name="creationTime" v-slot="{ props: fProps, invalid, error }">
        <DatePicker v-bind="fProps" v-model="ruleForm.creationTime" showTime class="w-full" :placeholder="t('message.gameBusiness.area.enabledDate')"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="serverVersion">{{ t('message.gameBusiness.area.serverVersionLabel') }}</FormLabel>
      <FormField name="serverVersion" v-slot="{ props: fProps, invalid, error }">
        <Select filter v-bind="fProps" v-model="ruleForm.serverVersion" :options="dockerTagOptions" optionLabel="label" optionValue="value" :placeholder="t('message.gameBusiness.area.serverVersionLabel')" class="w-full" showClear/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="language">{{ t('message.gameBusiness.area.language') }}</FormLabel>
      <FormField name="language" v-slot="{ props: fProps, invalid, error }">
        <Select filter v-bind="fProps" v-model="ruleForm.language" :options="languageOptions" optionLabel="label" optionValue="value" :placeholder="t('message.gameBusiness.area.pleaseSelectLanguage')" class="w-full" showClear/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="channelId">{{ t('message.gameBusiness.area.channel') }}</FormLabel>
      <FormField name="channelId" v-slot="{ props: fProps, invalid, error }">
        <Select filter v-bind="fProps" v-model="ruleForm.channelId" :options="channelOptions" optionLabel="label" optionValue="value" :placeholder="t('message.gameBusiness.area.pleaseSelectChannel')" class="w-full" showClear/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="packageId">{{ t('message.gameBusiness.area.package') }}</FormLabel>
      <FormField name="packageId" v-slot="{ props: fProps, invalid, error }">
        <Select filter v-bind="fProps" v-model="ruleForm.packageId" :options="packageOptions" optionLabel="label" optionValue="value" :placeholder="t('message.gameBusiness.area.pleaseSelectPackage')" class="w-full" showClear/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="classifyId">{{ t('message.gameBusiness.area.classifyName') }}</FormLabel>
      <FormField name="classifyId" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.classifyId" :options="classifyIdOptions" optionLabel="label" optionValue="value" :placeholder="t('message.gameBusiness.area.pleaseSelectClassifyName')" class="w-full" showClear filter/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <TextField v-model="ruleForm.webSocketUrl" name="webSocketUrl" :label="t('message.gameBusiness.area.webSocketUrl')" :placeholder="t('message.gameBusiness.area.pleaseEnterWebSocketUrl')" maxlength="255"/>
      <FormLabel name="lastMaintenanceTime">{{ t('message.gameBusiness.area.lastMaintenanceDate') }}</FormLabel>
      <FormField name="lastMaintenanceTime" v-slot="{ props: fProps, invalid, error }">
        <DatePicker v-bind="fProps" v-model="ruleForm.lastMaintenanceTime" showTime class="w-full" :placeholder="t('message.gameBusiness.area.lastMaintenanceDate')"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <TextField v-model="ruleForm.serverType" name="serverType" :label="t('message.gameBusiness.area.serverTypeLabel')" :placeholder="t('message.gameBusiness.area.pleaseEnterServerType')" maxlength="255"/>
      <FormLabel name="platformId">{{ t('message.gameBusiness.area.platform') }}</FormLabel>
      <FormField name="platformId" v-slot="{ props: fProps, invalid, error }">
        <Select filter v-bind="fProps" v-model="ruleForm.platformId" :options="platformOptions" optionLabel="label" optionValue="value" :placeholder="t('message.gameBusiness.area.pleaseSelectPlatform')" class="w-full" showClear/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="appVersion">{{ t('message.gameBusiness.area.tenantGameAppVersionDefinition') }}</FormLabel>
      <FormField name="appVersion" v-slot="{ props: fProps, invalid, error }">
        <Select filter v-bind="fProps" v-model="ruleForm.appVersion" :options="tenantGameAppVersionDefinitionOptions" optionLabel="label" optionValue="value" :placeholder="t('message.gameBusiness.area.tenantGameAppVersionDefinition')" class="w-full" showClear/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <TextareaField v-model="ruleForm.description" name="description" :label="t('message.gameBusiness.description')" :placeholder="t('message.gameBusiness.pleaseEnterDescription')" maxlength="512" :autoResize="true" rows="3"/>
      <TextareaField v-model="ruleForm.allowedPlayerProfile" name="allowedPlayerProfile" :label="t('message.gameBusiness.area.allowedPlayerProfile')" :placeholder="t('message.gameBusiness.area.pleaseEnterAllowedPlayerProfile')" maxlength="4096" :autoResize="true" rows="3"/>
      <TextareaField v-model="ruleForm.serverEvents" name="serverEvents" :label="t('message.gameBusiness.area.serverEvents')" :placeholder="t('message.gameBusiness.area.pleaseEnterServerEvents')" maxlength="4096" :autoResize="true" rows="3"/>
      <TextareaField v-model="ruleForm.accessControls" name="accessControls" :label="t('message.gameBusiness.area.accessControls')" :placeholder="t('message.gameBusiness.area.pleaseEnterAccessControls')" maxlength="4096" :autoResize="true" rows="3"/>
      <TextareaField v-model="ruleForm.serverParams" name="serverParams" :label="t('message.gameBusiness.area.serverParams')" :placeholder="t('message.gameBusiness.area.pleaseEnterServerParams')" maxlength="4096" :autoResize="true" rows="5"/>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref, computed, onMounted} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField, TextareaField} from '../../../../../components/crud-form-fields'
import { tenantGameAreaEntityAddPost, tenantGameAreaEntityUpdatePost } from '../../../../../api/tenantGameAreaEntity'
import { tenantGameAreaClassifyEntitySelectPost } from '../../../../../api/tenantGameAreaClassifyEntity'
import {systemEnumEnumDataListGet} from '../../../../../api/systemEnum';
import { tenantChannelDefinitionAvailableGet } from '../../../../../api/tenantChannelDefinition'
import { globalPlatformSelectPost } from '../../../../../api/globalPlatform'
import { tenantGameAppPackagePagePost } from '../../../../../api/tenantGameAppPackage'
import { tenantGameDockerTagEntitySelectPost } from '../../../../../api/tenantGameDockerTagEntity'
import { tenantGameAppVersionDefinitionPagePost } from '../../../../../api/tenantGameAppVersionDefinition'
import {MAX_PAGE_SIZE, MAX_SELECT_SIZE} from '../../../../../constants/pagination.ts'
import {isEnabled} from '../../../../../utils/isEnabled.ts'
import {normalizeAppId} from '../../../../../utils/appWorkspace.ts'

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

const getEnumServerStatusData = ref<any>([])
const getEnumLanguageData = ref<any>([])
const getEnumChannelData = ref<any>([])
const getEnumPlatformData = ref<any>([])
const getEnumPackageData = ref<any>([])
const getEnumDockerTag = ref<any>([])
const getTenantGameAppVersionDefinitionData = ref<any>([])
const gameAreaClassifyEntityClassifyIdDropdownList = ref<any>([])
const selectedAppId = ref<number | undefined>(undefined)

const serverStatusOptions = computed(() =>
    getEnumServerStatusData.value.map((item: any) => ({
      label: `[${item.value}] ${item.describe}`,
      value: item.value
    }))
)
const languageOptions = computed(() =>
    getEnumLanguageData.value.map((item: any) => ({
      label: `[${item.value}] ${item.describe}`,
      value: item.value
    }))
)
const channelOptions = computed(() =>
    getEnumChannelData.value.map((item: any) => ({
      label: `[${item.name}] ${item.description}`,
      value: item.id
    }))
)
const platformOptions = computed(() =>
    getEnumPlatformData.value.map((item: any) => ({
      label: `[${item.name ?? item.value}] ${item.description ?? item.label}`,
      value: item.id
    }))
)
const packageOptions = computed(() =>
    getEnumPackageData.value
        .filter((item: any) => !selectedAppId.value || item.appId === selectedAppId.value)
        .map((item: any) => ({
          label: item.packageVersion ? `[${item.name}] ${item.packageName} / ${item.packageVersion}` : `[${item.name}] ${item.packageName}`,
          value: item.id
        }))
)
// select 字典结构 {id, value(名称), label(描述)}：镜像名做显示与取值（serverVersion 落库值）
const dockerTagOptions = computed(() =>
    getEnumDockerTag.value.map((item: any) => ({
      label: item.value,
      value: item.value
    }))
)
const tenantGameAppVersionDefinitionOptions = computed(() =>
    getTenantGameAppVersionDefinitionData.value
        .filter((item: any) => !selectedAppId.value || item.appId === selectedAppId.value)
        .map((item: any) => ({
          label: `[${item.name}] ${item.description}`,
          value: item.name
        }))
)
// select 字典结构 {id, value(名称), label(描述)}：分类下拉显示名称而非描述
const classifyIdOptions = computed(() =>
    gameAreaClassifyEntityClassifyIdDropdownList.value.map((item: any) => ({
      label: item.value,
      value: item.id
    }))
)

const rules = {
  name: [{required: true, message: t('message.gameBusiness.pleaseEnterName'), trigger: 'blur'}],
  httpManageUrl: [{required: true, message: t('message.gameBusiness.area.pleaseEnterHttpManageUrl'), trigger: 'blur'}],
  tcpSocketUrl: [{required: true, message: t('message.gameBusiness.area.pleaseEnterTcpSocketUrl'), trigger: 'blur'}],
  webSocketUrl: [{required: true, message: t('message.gameBusiness.area.pleaseEnterWebSocketUrl'), trigger: 'blur'}],
  capacity: [{required: true, message: t('message.gameBusiness.area.pleaseEnterMaxPlayers'), trigger: 'blur'}],
  serverVersion: [{required: true, message: t('message.gameBusiness.pleaseEnterGameVersion'), trigger: 'blur'}],
  serverType: [{required: true, message: t('message.gameBusiness.area.pleaseEnterServerType'), trigger: 'blur'}],
  appVersion: [{required: true, message: t('message.gameBusiness.pleaseSelectTenantGameAppVersionDefinition'), trigger: 'blur'}],
}

const loadScopedOptions = async (appId?: number) => {
  if (!appId) return
  const params = {page: 1, pageSize: MAX_PAGE_SIZE, appId}
  getEnumPackageData.value = (await tenantGameAppPackagePagePost(params)).data.data?.items ?? []
  getEnumDockerTag.value = (await tenantGameDockerTagEntitySelectPost({appId, pageSize: MAX_SELECT_SIZE})).data.data ?? []
  getTenantGameAppVersionDefinitionData.value = (await tenantGameAppVersionDefinitionPagePost(params)).data.data?.items ?? []
  const classifyList = await tenantGameAreaClassifyEntitySelectPost({searchKey: '', appId, pageSize: MAX_SELECT_SIZE})
  gameAreaClassifyEntityClassifyIdDropdownList.value = classifyList.data.data ?? []
}

const routeAppId = () => {
  // C270：normalizeAppId 已保证超 2^53 雪花 Id 保持字符串，直接透传，禁止 Number() 回转丢精度
  return normalizeAppId(props.appId)
}

const beforeOpen = async (form: any, isEditor: boolean) => {
  if (form.channelId == 0 || form.channelId == null) {
    form.channelId = getEnumChannelData.value.find((x: any) => x.isDefault)?.id ?? 0
  }
  if (form.platformId == 0 || form.platformId == null) {
    form.platformId = getEnumPlatformData.value.find((x: any) => x.isDefault || x.value === 'default')?.id ?? 0
  }
  const appId = routeAppId()
  if (appId) form.appId = appId
  selectedAppId.value = appId
  await loadScopedOptions(selectedAppId.value)
  // 接口配置默认值（C175/C176）：ClientDispatch/Fulfillment 默认 game/api；Gm/Control 空 = 不构造对应能力 URL；EnableClientGm 默认关闭。
  // 仅新增模式填前缀默认；编辑回显保持后端归一化后的存储值，null 前缀规整为空串避免显示 "null"。
  if (form.clientDispatchApiPrefix == null && !isEditor) form.clientDispatchApiPrefix = 'game/api'
  if (form.fulfillmentApiPrefix == null && !isEditor) form.fulfillmentApiPrefix = 'game/api'
  if (form.gmApiPrefix == null) form.gmApiPrefix = ''
  if (form.controlApiPrefix == null) form.controlApiPrefix = ''
  if (form.enableClientGmApi == null) form.enableClientGmApi = false
  // 回显：long(秒) → Date 供 DatePicker 显示
  if (form.creationTime && typeof form.creationTime === 'number') {
    form.creationTime = new Date(form.creationTime * 1000)
  }
  if (form.lastMaintenanceTime && typeof form.lastMaintenanceTime === 'number') {
    form.lastMaintenanceTime = new Date(form.lastMaintenanceTime * 1000)
  }
}

const beforeSubmit = (form: any) => {
  const appId = routeAppId()
  if (appId) form.appId = appId
  form.isEnabled = isEnabled(form)
  // 提交：Date → long 秒；空值保持 null（lastMaintenanceTime 允许 null）
  const payload = {...form}
  if (payload.creationTime) {
    payload.creationTime = Math.floor(new Date(payload.creationTime).getTime() / 1000)
  }
  if (payload.lastMaintenanceTime) {
    payload.lastMaintenanceTime = Math.floor(new Date(payload.lastMaintenanceTime).getTime() / 1000)
  }
  return payload
}

onMounted(async () => {
  getEnumServerStatusData.value = (await systemEnumEnumDataListGet({ EnumName: 'GameServerStatus' })).data.data ?? []
  getEnumLanguageData.value = (await systemEnumEnumDataListGet({ EnumName: 'GameLanguage' })).data.data ?? []
  getEnumChannelData.value = (await tenantChannelDefinitionAvailableGet({pageSize: MAX_SELECT_SIZE})).data.data || []
  getEnumPlatformData.value = (await globalPlatformSelectPost({searchKey: '', pageSize: MAX_SELECT_SIZE})).data.data ?? []
})

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>

<style scoped>
.section-title {
  grid-column: 1 / -1;
  font-weight: 600;
  font-size: 12px;
  color: var(--p-text-color);
  margin: 8px 0 4px;
  border-left: 3px solid var(--p-primary-color);
  padding-left: 8px;
}
</style>
