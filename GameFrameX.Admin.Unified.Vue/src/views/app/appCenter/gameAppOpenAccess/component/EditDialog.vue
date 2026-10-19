<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="wrappedAddApi"
    :updateApi="wrappedUpdateApi"
    :rules="rules"
    :defaultForm="defaultForm"
    :beforeOpen="beforeOpen"
    :beforeSubmit="beforeSubmit"
    :scrollable="true"
    @reloadTable="emit('reloadTable')"
  >
    <template #default="{ ruleForm, isEditor }">
      <FormLabel>{{ t('message.gameBusiness.openAccess.appId') }}</FormLabel>
      <div>
        <Select filter v-model="ruleForm.appId" :options="appOptions" optionLabel="name" optionValue="id"
                :disabled="isEditor" :placeholder="t('message.gameBusiness.appContext.pleaseSelectGameApp')" class="w-full" />
      </div>

      <FormLabel name="bindUserId">{{ t('message.gameBusiness.openAccess.bindUserId') }}</FormLabel>
      <FormField name="bindUserId" v-slot="{ props: fProps, invalid, error }">
        <Select filter v-bind="fProps" v-model="ruleForm.bindUserId" :options="userOptions" optionLabel="label" optionValue="id"
                :placeholder="t('message.gameBusiness.openAccess.pleaseSelectBindUser')" class="w-full" />
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <TextField v-model="ruleForm.scenario" name="scenario" :label="t('message.gameBusiness.openAccess.scenario')" :placeholder="t('message.gameBusiness.openAccess.pleaseEnterScenario')" maxlength="64"/>

      <TextField v-model="ruleForm.subjectName" name="subjectName" :label="t('message.gameBusiness.openAccess.subjectName')" :placeholder="t('message.gameBusiness.openAccess.pleaseEnterSubjectName')" maxlength="128"/>

      <TextareaField v-model="ruleForm.purpose" name="purpose" :label="t('message.gameBusiness.openAccess.purpose')" :placeholder="t('message.gameBusiness.openAccess.pleaseEnterPurpose')" :autoResize="true" :rows="2" maxlength="256"/>

      <FormLabel name="scopesArray">{{ t('message.gameBusiness.openAccess.scopes') }}</FormLabel>
      <FormField name="scopesArray" v-slot="{ props: fProps, invalid, error }">
        <MultiSelect v-bind="fProps" v-model="ruleForm.scopesArray" :options="scopeOptions" optionLabel="label" optionValue="value"
                     :placeholder="t('message.gameBusiness.openAccess.pleaseSelectScopes')" display="chip" class="w-full" />
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <FormLabel>{{ t('message.gameBusiness.openAccess.expiresTime') }}</FormLabel>
      <div>
        <DatePicker v-model="ruleForm.expiresTime" :placeholder="t('message.gameBusiness.openAccess.pleaseSelectExpiresTime')"
                    dateFormat="yy-mm-dd" :showTime="true" showIcon showClear class="w-full" />
      </div>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import { ref, computed } from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import Select from 'primevue/select'
import MultiSelect from 'primevue/multiselect'
import DatePicker from 'primevue/datepicker'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField, TextareaField} from '../../../../../components/crud-form-fields'
import { tenantGameAppOpenAccessAddPost, tenantGameAppOpenAccessUpdatePost } from '../../../../../api/tenantGameAppOpenAccess'
import { tenantUserPagePost } from '../../../../../api/tenantUser'
import { useCurrentApp } from '../../../../../composables/useCurrentApp.ts'
import { MAX_PAGE_SIZE } from '../../../../../constants/pagination.ts'
import { normalizeAppId } from '../../../../../utils/appWorkspace.ts'
import { t } from '../../../../../i18n/index.ts'

const props = defineProps({
  title: { type: String, default: '' },
  appId: { type: [Number, String], default: undefined },
})
const emit = defineEmits(['reloadTable', 'secret'])

const dialogRef = ref()
const userOptions = ref<any[]>([])
const { appOptions, loadApps } = useCurrentApp()

const scopeOptions = computed(() => [
  { label: t('message.gameBusiness.openAccess.scopePackageReport'), value: 'package-report' },
])

const rules = {
  bindUserId: [{ required: true, message: t('message.gameBusiness.openAccess.bindUserRequired'), trigger: 'change' }],
  scenario: [{ required: true, message: t('message.gameBusiness.openAccess.scenarioRequired'), trigger: 'blur' }],
  purpose: [{ required: true, message: t('message.gameBusiness.openAccess.purposeRequired'), trigger: 'blur' }],
  subjectName: [{ required: true, message: t('message.gameBusiness.openAccess.subjectNameRequired'), trigger: 'blur' }],
  scopesArray: [{ required: true, message: t('message.gameBusiness.openAccess.scopesRequired'), trigger: 'change' }],
}

const defaultForm = () => ({
  appId: normalizeAppId(props.appId),
  bindUserId: undefined,
  scenario: '',
  purpose: '',
  subjectName: '',
  scopesArray: [] as string[],
  scopesJson: '',
  expiresTime: undefined as any,
})

// ---- beforeOpen：解析 scopesJson → scopesArray，expiresTime Unix 秒 → Date ----
const beforeOpen = (form: any, isEditor: boolean) => {
  const appId = normalizeAppId(props.appId)
  if (appId) form.appId = appId

  if (isEditor) {
    // 解析 scopesJson → scopesArray
    if (form.scopesJson) {
      try {
        const parsed = JSON.parse(form.scopesJson)
        form.scopesArray = Array.isArray(parsed) ? parsed : []
      } catch {
        form.scopesArray = []
      }
    } else {
      form.scopesArray = []
    }
    // expiresTime Unix 秒 → Date（DatePicker 需要日期对象）
    if (form.expiresTime && typeof form.expiresTime === 'number') {
      form.expiresTime = new Date(form.expiresTime * 1000)
    } else {
      form.expiresTime = undefined
    }
  } else {
    form.scopesArray = form.scopesArray ?? []
  }
}

// ---- beforeSubmit：scopesArray → scopesJson，expiresTime Date → Unix 秒 ----
const beforeSubmit = (form: any) => {
  // scopesArray → JSON 字符串
  form.scopesJson = JSON.stringify(form.scopesArray || [])
  delete form.scopesArray

  // expiresTime Date → Unix 秒
  if (form.expiresTime instanceof Date) {
    form.expiresTime = Math.floor(form.expiresTime.getTime() / 1000)
  } else if (form.expiresTime === '' || form.expiresTime === undefined) {
    form.expiresTime = null
  }

  return form
}

// ---- wrappedAddApi：捕获 SecretResult ----
const wrappedAddApi = async (payload: any) => {
  const res = await tenantGameAppOpenAccessAddPost(payload)
  const secret = res?.data?.data
  if (secret) {
    emit('secret', secret)
  }
}

// ---- wrappedUpdateApi ----
const wrappedUpdateApi = async (payload: any) => {
  await tenantGameAppOpenAccessUpdatePost(payload)
}

// ---- 加载租户用户列表 ----
const loadUsers = async () => {
  try {
    const res: any = await tenantUserPagePost({ page: 1, pageSize: MAX_PAGE_SIZE })
    const items = res.data?.data?.items ?? []
    userOptions.value = items.map((item: any) => ({
      id: item.id,
      label: item.realName ? `${item.account}(${item.realName})` : item.account,
    }))
  } catch {
    userOptions.value = []
  }
}

defineExpose({
  openDialog: async (row: any) => {
    await Promise.all([loadUsers(), loadApps()])
    dialogRef.value?.openDialog(row)
  },
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>
