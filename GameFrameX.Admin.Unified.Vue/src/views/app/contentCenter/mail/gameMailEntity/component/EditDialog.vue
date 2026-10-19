<template>
  <BaseEditDialog ref="dialogRef"
                  :title="props.title"

                  scrollable
                  scrollMaxHeight="65vh"
                  :addApi="tenantGameMailEntityAddPost"
                  :updateApi="tenantGameMailEntityUpdatePost"
                  :rules="rules"
                  :defaultForm="() => ({attachmentsJson: []} as any)"
                  :beforeOpen="onBeforeOpen"
                  :beforeSubmit="onBeforeSubmit"
                  @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm, isEditor }">
      <FormLabel>{{ t('message.gameBusiness.mail.mailTemplate') }}</FormLabel>
      <div>
        <Select v-model="selectedTemplateId" :options="templateList" optionLabel="name" optionValue="id"
                :placeholder="t('message.gameBusiness.mail.selectTemplateAutoFill')" showClear filter class="w-full"
                :disabled="templateList.length === 0"
                @change="onTemplateChange"/>
      </div>
      <TextField v-model="ruleForm.title" name="title" :label="t('message.gameBusiness.mail.title')" :placeholder="t('message.gameBusiness.mail.pleaseEnterTitle')" maxlength="255"/>
      <FormLabel name="areaId">{{ t('message.gameBusiness.mail.areaId') }}</FormLabel>
      <FormField name="areaId" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.areaId" :options="areaIdOptions" optionLabel="value" optionValue="id" :placeholder="t('message.gameBusiness.mail.pleaseSelectAreaId')" showClear filter class="w-full" :disabled="areaIdOptions.length === 0" @change="onAreaChange"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="expirationTime">{{ t('message.gameBusiness.mail.expirationDate') }}</FormLabel>
      <FormField name="expirationTime" v-slot="{ props: fProps, invalid, error }">
        <DatePicker v-bind="fProps" v-model="ruleForm.expirationTime" showTime class="w-full" :placeholder="t('message.gameBusiness.mail.expirationDate')"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel>{{ t('message.gameBusiness.isAllPost') }}</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.isAllPost"/></div>
      <TextField v-model="ruleForm.subTitle" name="subTitle" :label="t('message.gameBusiness.mail.subTitle')" :placeholder="t('message.gameBusiness.mail.pleaseEnterSubTitle')" maxlength="255"/>
      <FormLabel name="playerIdListJson">{{ t('message.gameBusiness.mail.playerIdList') }}</FormLabel>
      <FormField name="playerIdListJson" v-slot="{ props: fProps, invalid, error }">
        <MultiSelect v-bind="fProps" v-model="selectedPlayerIds" :options="playerOptions" optionLabel="label" optionValue="id" :placeholder="t('message.gameBusiness.mail.searchPlayerId')" showClear filter class="w-full" :loading="false" :disabled="playerOptions.length === 0"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="mailType">{{ t('message.gameBusiness.mail.mailType') }}</FormLabel>
      <FormField name="mailType" v-slot="{ props: fProps, invalid, error }">
        <Select filter v-bind="fProps" v-model="ruleForm.mailType" :options="getEnumMailTypeData" optionLabel="describe" optionValue="value" :placeholder="t('message.gameBusiness.mail.pleaseSelectMailTypeDefault')" showClear class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel>{{ t('message.gameBusiness.mail.isTranslate') }}</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.isTranslate"/></div>
      <TextareaField v-model="ruleForm.body" name="body" :label="t('message.gameBusiness.detailedContent')" :placeholder="t('message.gameBusiness.mail.pleaseEnterBody')" maxlength="8192" :autoResize="true" rows="3"/>
      <!-- 模板变量 -->
      <template v-if="templateVariables.length > 0">
        <FormLabel>{{ t('message.gameBusiness.mail.templateVariables') }}</FormLabel>
        <div>
          <div style="display: grid; grid-template-columns: repeat(3, auto 1fr); gap: 10px 16px; align-items: start;">
            <template v-for="varName in templateVariables" :key="varName">
              <FormLabel>{{ varName }}</FormLabel>
              <InputText v-model="variableValues[varName]" :placeholder="t('message.gameBusiness.mail.pleaseEnterVariable', {varName})" class="w-full"/>
            </template>
          </div>
          <Button size="small" @click="applyVariables" style="margin-top: 4px;">
            {{ t('message.gameBusiness.mail.replaceVariables') }}
          </Button>
        </div>
      </template>
      <!-- 附件列表 -->
      <FormLabel>{{ t('message.gameBusiness.rewardTemplate.copySnapshot') }}</FormLabel>
      <div>
        <Select v-model="selectedRewardTemplateId" :options="rewardTemplateList" optionLabel="value" optionValue="id"
                :placeholder="t('message.gameBusiness.rewardTemplate.selectTemplateCopyRewards')" showClear filter class="w-full"
                :disabled="isEditor || rewardTemplateList.length === 0"
                @change="onRewardTemplateChange"/>
      </div>
      <FormLabel name="attachmentsJson">{{ t('message.gameBusiness.mail.attachmentList') }}</FormLabel>
      <FormField name="attachmentsJson" v-slot="{ invalid, error }">
        <RewardItemsEditor v-model="ruleForm.attachmentsJson" :appId="ruleForm.appId" :disabled="isEditor"
                           :rewardPlaceholder="t('message.gameBusiness.mail.pleaseSelectAttachment')"
                           :addLabel="t('message.gameBusiness.mail.addAttachment')"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
    </template>
  </BaseEditDialog>
</template>
<script lang="ts" setup>
import {ref, computed, reactive, onMounted} from 'vue'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField, TextareaField} from '../../../../../../components/crud-form-fields'
import RewardItemsEditor from '/@/components/reward-items-editor/index.vue'
import {parseRewardItems, toRewardItemsJson} from '../../../../../../components/reward-items-editor/rewardItems.ts'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import MultiSelect from 'primevue/multiselect'
import { tenantGameMailEntityAddPost, tenantGameMailEntityUpdatePost } from '../../../../../../api/tenantGameMailEntity'
import { tenantGameRoleEntityPagePost } from '../../../../../../api/tenantGameRoleEntity'
import { tenantGameMailTemplateEntityPagePost } from '../../../../../../api/tenantGameMailTemplateEntity'
import {systemEnumEnumDataListGet} from '../../../../../../api/systemEnum';
import { tenantGameAreaEntitySelectStringPost } from '../../../../../../api/tenantGameAreaEntity'
import {extractVariablesFromAll, renderTemplate} from '../../../../../../utils/templateUtils.ts'
import { tenantGameRewardTemplateEntityCopySnapshotPost, tenantGameRewardTemplateEntitySelectPost } from '../../../../../../api/tenantGameRewardTemplateEntity'
import {normalizeAppId, type AppWorkspaceId} from '../../../../../../utils/appWorkspace.ts'
import {MAX_PAGE_SIZE, MAX_SELECT_SIZE} from '../../../../../../constants/pagination.ts'

interface ListItem {
  id: number
  value: string
  label: string
}

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

const dialogRef = ref<InstanceType<typeof BaseEditDialog>>()

const getEnumMailTypeData = ref<any>([])
const playerOptions = ref<any[]>([])
const selectedPlayerIds = ref<string[]>([])
const areaIdOptions = ref<ListItem[]>([])

const templateList = ref<any[]>([])
const selectedTemplateId = ref<number | undefined>(undefined)
const rewardTemplateList = ref<any[]>([])
const selectedRewardTemplateId = ref<number | undefined>(undefined)
const variableValues = reactive<Record<string, string>>({})

// Access ruleForm from the BaseEditDialog ref for computed properties
const ruleForm = computed(() => dialogRef.value?.ruleForm ?? {body: '', title: '', subTitle: '', attachmentsJson: []})

const templateVariables = computed(() => {
  const form = ruleForm.value
  return extractVariablesFromAll(form.body, form.title, form.subTitle)
})

const onTemplateChange = (eventOrId: any) => {
  // Select @change 传的是事件对象（value 在 event.value），直接当 id 用会恒查不到模板
  const templateId = eventOrId?.value ?? eventOrId
  if (!templateId) return
  const template = templateList.value.find(t => t.id === templateId)
  if (!template) return

  ruleForm.value.title = template.titleTemplate || ''
  ruleForm.value.subTitle = template.subTitleTemplate || ''
  ruleForm.value.body = template.bodyTemplate || ''

  if (template.defaultMailType != null) {
    ruleForm.value.mailType = template.defaultMailType
  }

  if (template.defaultAttachmentsJson) {
    ruleForm.value.attachmentsJson = parseRewardItems(template.defaultAttachmentsJson)
  }

  if (template.defaultExpirationDays && template.defaultExpirationDays > 0) {
    const expDate = new Date()
    expDate.setDate(expDate.getDate() + template.defaultExpirationDays)
    ruleForm.value.expirationTime = expDate
  }

  Object.keys(variableValues).forEach(k => delete variableValues[k])
}

const resolveRewardTemplateId = (value: any) => (value && typeof value === "object" ? (value.value ?? value.id) : value)

const onRewardTemplateChange = async (event?: any) => {
  const templateId = resolveRewardTemplateId(event?.value ?? selectedRewardTemplateId.value)
  selectedRewardTemplateId.value = Number.isFinite(templateId) ? templateId : undefined
  if (!templateId || !ruleForm.value.appId) return
  const response = await tenantGameRewardTemplateEntityCopySnapshotPost({id: templateId, appId: ruleForm.value.appId})
  const snapshot = response.data.data
  ruleForm.value.attachmentsJson = parseRewardItems(snapshot?.rewardItemsJson)
}

const applyVariables = () => {
  const data: Record<string, string> = {}
  templateVariables.value.forEach(varName => {
    data[varName] = variableValues[varName] || ''
  })
  ruleForm.value.title = renderTemplate(ruleForm.value.title ?? '', data)
  ruleForm.value.subTitle = renderTemplate(ruleForm.value.subTitle ?? '', data)
  ruleForm.value.body = renderTemplate(ruleForm.value.body ?? '', data)
  Object.keys(variableValues).forEach(k => delete variableValues[k])
}

const rules = {
  title: [{required: true, message: t('message.gameBusiness.mail.pleaseEnterTitle'), trigger: 'blur'}],
  subTitle: [{required: true, message: t('message.gameBusiness.mail.pleaseEnterSubTitle'), trigger: 'blur'}],
  body: [{required: true, message: t('message.gameBusiness.mail.pleaseEnterBody'), trigger: 'blur'}],
  areaId: [{required: true, message: t('message.gameBusiness.pleaseSelectArea'), trigger: 'blur'}],
  // 发全部用户时玩家列表无意义；不校验 Form state（MultiSelect 走独立 selectedPlayerIds ref，Form state 恒空）
  playerIdListJson: [{
    validator: (_rule: any, _value: any, callback: (error?: string) => void) => {
      if (!ruleForm.value.isAllPost && selectedPlayerIds.value.length === 0) {
        callback(t('message.gameBusiness.mail.pleaseEnterPlayerIdList'))
      }
    },
    trigger: 'blur',
  }],
  attachmentsJson: [{required: true, message: t('message.gameBusiness.pleaseEnterAttachmentList'), trigger: 'blur'}],
  mailType: [{required: true, message: t('message.gameBusiness.mail.pleaseSelectMailTypeDefault'), trigger: 'change'}],
  expirationTime: [{required: true, message: t('message.gameBusiness.pleaseSelectMailExpiry'), trigger: 'change'}],
}

const loadAreaOptions = async (appId?: AppWorkspaceId) => {
  areaIdOptions.value = (await tenantGameAreaEntitySelectStringPost({appId: appId, pageSize: MAX_SELECT_SIZE})).data.data ?? []
}

// 玩家选项来自角色表（后端契约：playerIdList 与角色 Id 匹配），按区服过滤
const loadPlayerOptions = async (appId?: AppWorkspaceId, areaId?: number) => {
  if (!appId || !areaId) {
    playerOptions.value = []
    return
  }
  const res = await tenantGameRoleEntityPagePost({appId, areaId, page: 1, pageSize: 100})
  playerOptions.value = (res.data.data?.items ?? []).map((item: any) => ({id: String(item.id), label: item.name ?? String(item.id)}))
}

const onAreaChange = async (event?: any) => {
  const areaId = event?.value ?? ruleForm.value.areaId
  selectedPlayerIds.value = []
  await loadPlayerOptions(await resolveAppId(ruleForm.value), areaId)
}

const loadTemplateOptions = async (appId?: AppWorkspaceId) => {
  // 模板下拉需取全字段（titleTemplate/bodyTemplate/defaultMailType/defaultExpirationDays/defaultAttachmentsJson）：
  // select 接口只有 id/value/label，onTemplateChange 取不到模板字段会清空表单而非填充
  const tplRes = await tenantGameMailTemplateEntityPagePost({appId, page: 1, pageSize: MAX_PAGE_SIZE})
  templateList.value = (tplRes.data.data?.items ?? []).map((item: any) => ({
    ...item,
    name: item.name || item.label || item.value || item.titleTemplate || String(item.id ?? ''),
  }))
}

const loadRewardTemplateOptions = async (appId?: AppWorkspaceId) => {
  rewardTemplateList.value = appId ? ((await tenantGameRewardTemplateEntitySelectPost({appId, pageSize: MAX_SELECT_SIZE})).data.data ?? []) : []
}

const resolveAppId = async (form: any) => {
  const raw = props.appId ?? form.appId
  return normalizeAppId(raw)
}

const loadScopedOptions = async (appId?: AppWorkspaceId) => {
  if (!appId) return
  await Promise.all([
    loadAreaOptions(appId),
    loadTemplateOptions(appId),
    loadRewardTemplateOptions(appId),
  ])
}

const onBeforeOpen = async (form: any) => {
  selectedTemplateId.value = undefined
  selectedRewardTemplateId.value = undefined
  Object.keys(variableValues).forEach(k => delete variableValues[k])

  form.attachmentsJson = parseRewardItems(form.attachmentsJson)

  // 回显：long(秒) → Date 供 DatePicker 显示
  if (form.expirationTime && typeof form.expirationTime === 'number') {
    form.expirationTime = new Date(form.expirationTime * 1000)
  }

  if (typeof form.playerIdListJson === 'string' && form.playerIdListJson) {
    try {
      const playerIds = JSON.parse(form.playerIdListJson)
      selectedPlayerIds.value = Array.isArray(playerIds) ? playerIds : []
    } catch {
      selectedPlayerIds.value = []
    }
  } else {
    selectedPlayerIds.value = []
  }

  const appId = await resolveAppId(form)
  if (appId) {
    form.appId = appId
  }
  await loadScopedOptions(appId)
  // 编辑回显：已选玩家需要 options 才能显示名称
  await loadPlayerOptions(appId, form.areaId)
}

const onBeforeSubmit = (form: any) => {
  const payload = {...form}
  const appId = normalizeAppId(props.appId)
  if (appId) {
    payload.appId = appId
  }
  if (templateVariables.value.length > 0) {
    const data: Record<string, string> = {}
    templateVariables.value.forEach(varName => {
      data[varName] = variableValues[varName] || ''
    })
    payload.title = renderTemplate(form.title ?? '', data)
    payload.subTitle = renderTemplate(form.subTitle ?? '', data)
    payload.body = renderTemplate(form.body ?? '', data)
  }

  // 提交：Date → long 秒
  if (payload.expirationTime) {
    payload.expirationTime = Math.floor(new Date(payload.expirationTime).getTime() / 1000)
  }

  // 玩家选择在独立 selectedPlayerIds ref（MultiSelect），提交前同步为 JSON 文本
  payload.playerIdListJson = JSON.stringify(selectedPlayerIds.value ?? [])
  payload.attachmentsJson = toRewardItemsJson(form.attachmentsJson)
  return payload
}

const openDialog = (row: any) => {
  dialogRef.value?.openDialog(row)
}

onMounted(async () => {
  getEnumMailTypeData.value = (await systemEnumEnumDataListGet({ EnumName: 'GameMailType' })).data.data ?? []
})

defineExpose({
  openDialog,
  get isEditor() {
    return dialogRef.value?.isEditor
  },
  set isEditor(value) {
    if (dialogRef.value) dialogRef.value.isEditor = value
  },
})
</script>
