<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="(params: any) => systemTenantAddPost(params)"
    :updateApi="(params: any) => systemTenantUpdatePost(params)"
    :rules="rules"
    idCheckMode="system"
    scrollable
    :beforeOpen="normalizeTenantOpen"
    :beforeSubmit="normalizeTenantSubmit"
    @reloadTable="emit('query')"
  >
    <template #default="{ ruleForm, isEditor }">
      <FormLabel name="tenantType">{{ t('system.tenant.tenantType') }}</FormLabel>
      <div class="tenant-type-summary">
        <Tag
          :severity="isMainTenant(ruleForm) ? 'info' : 'success'"
          :value="isMainTenant(ruleForm) ? t('system.tenant.mainTenant') : t('system.tenant.dbIsolation')"
        />
      </div>

      <TextField v-model="ruleForm.name" name="name" :label="t('system.tenant.tenantName')" :placeholder="t('system.tenant.tenantName')"/>

      <TextField v-model="ruleForm.adminAccount" name="adminAccount" :label="t('system.tenant.adminAccount')" :placeholder="t('system.tenant.adminAccount')"/>

      <TextField v-model="ruleForm.phone" name="phone" :label="t('system.tenant.phone')" :placeholder="t('system.tenant.phone')"/>

      <TextField v-model="ruleForm.email" name="email" :label="t('common.label.email')" :placeholder="t('common.label.email')"/>

      <FormLabel name="dbType">{{ t('system.tenant.dbType') }}</FormLabel>
      <div class="tenant-type-summary">
        <Tag severity="success" :value="t('system.tenant.postgresqlAutoProvision')" />
      </div>

      <template v-if="isEditor">
        <FormLabel name="connection">{{ t('system.tenant.connectionString') }}</FormLabel>
        <InputText :modelValue="summarizeConnection(ruleForm.connection)" class="w-full" disabled/>
      </template>

      <TextareaField v-model="ruleForm.remark" name="remark" :label="t('common.label.remark')" :placeholder="t('common.form.pleaseInput')" :autoResize="true" rows="2"/>

      <FormLabel name="packageId">{{ t('system.tenant.package') }}</FormLabel>
      <FormField name="packageId" v-slot="{ props: fProps, invalid, error }">
        <Select filter
          v-bind="fProps"
          v-model="ruleForm.packageId"
          :options="packageOptions"
          optionLabel="name"
          optionValue="id"
          :placeholder="t('common.form.pleaseSelect')"
          :showClear="true"
          class="w-full"
        />
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <FormLabel name="expireTime">{{ t('system.tenant.expireTime') }}</FormLabel>
      <FormField name="expireTime" v-slot="{ props: fProps, invalid, error }">
        <DatePicker
          v-bind="fProps"
          v-model="ruleForm.expireTime"
          :showTime="true"
          :showSeconds="true"
          :placeholder="t('system.tenant.expireTimePlaceholder')"
          :showClear="true"
          class="w-full"
        />
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <FormLabel name="gracePeriodDays">{{ t('system.tenant.gracePeriodDaysOverride') }}</FormLabel>
      <FormField name="gracePeriodDays" v-slot="{ props: fProps, invalid, error }">
        <InputNumber
          v-bind="fProps"
          v-model="ruleForm.gracePeriodDays"
          :placeholder="t('system.tenant.gracePeriodDaysPlaceholder')"
          :min="0"
          :max="365"
          class="w-full"
        />
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup name="editTenant">
import {onMounted, ref} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField, TextareaField} from '../../../../../components/crud-form-fields'
import {systemTenantAddPost, systemTenantUpdatePost} from '../../../../../api/systemTenant';
import {globalPackagePagePost} from '../../../../../api/globalPackage'
import {MAX_PAGE_SIZE} from '../../../../../constants/pagination.ts'

const props = defineProps({
  title: {
    type: String,
    default: '',
  },
})
const emit = defineEmits(['query'])
const dialogRef = ref()
const packageOptions = ref<any[]>([])

const rules = {
  name: [{required: true, message: t('system.tenant.tenantName') + t('common.form.required'), trigger: 'blur'}],
  adminAccount: [{required: true, message: t('system.tenant.adminAccount') + t('common.form.required'), trigger: 'blur'}],
  phone: [{required: true, message: t('system.tenant.phone') + t('common.form.required'), trigger: 'blur'}],
}

const isMainTenant = (row: any) => row?.id === 1300000000001
const summarizeConnection = (connection?: string) => {
  if (!connection) return '-'
  const visibleKeys = new Set(['host', 'server', 'data source', 'datasource', 'database', 'initial catalog'])
  const parts = connection
      .split(';')
      .map((part) => part.trim())
      .filter(Boolean)
      .map((part) => {
        const index = part.indexOf('=')
        if (index < 0) return null
        const key = part.slice(0, index).trim()
        const normalizedKey = key.toLowerCase()
        if (!visibleKeys.has(normalizedKey)) return null
        const value = part.slice(index + 1).trim()
        return `${key}=${value}`
      })
      .filter(Boolean)

  return parts.length > 0 ? parts.join('; ') : '******'
}

onMounted(async () => {
  const res = await globalPackagePagePost({page: 1, pageSize: MAX_PAGE_SIZE})
  packageOptions.value = res.data?.data?.items ?? res.data?.items ?? []
})

const normalizeTenantOpen = (form: any) => {
  const next = {...form}
  if (next.expireTime && typeof next.expireTime === 'number') {
    next.expireTime = new Date(next.expireTime * 1000)
  }
  return next
}

const normalizeTenantSubmit = (form: any, isEditor: boolean) => {
  const payload = {...form, tenantType: 1, dbType: 4}
  if (payload.expireTime instanceof Date) {
    payload.expireTime = Math.floor(payload.expireTime.getTime() / 1000)
  } else if (payload.expireTime) {
    payload.expireTime = Math.floor(new Date(payload.expireTime).getTime() / 1000)
  }
  if (!isEditor) {
    delete payload.connection
  }
  return payload
}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
})
</script>

<style scoped>
.tenant-type-summary {
  display: flex;
  align-items: center;
  min-height: 38px;
  margin-bottom: 0.5rem;
}
</style>
