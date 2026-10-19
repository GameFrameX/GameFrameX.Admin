<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="(params: any) => tenantOrganizationAddPost(params)"
    :updateApi="(params: any) => tenantOrganizationUpdatePost(params)"
    :rules="rules"
    idCheckMode="system"
    @reloadTable="emit('query', true)"
  >
    <template #default="{ ruleForm }">
      <FormLabel>{{ t('system.org.parentOrg') }}</FormLabel>
      <div>
        <CascadeSelect v-model="ruleForm.pid" :options="cascadeOrgOptions" optionLabel="label" optionValue="value" optionGroupLabel="label" optionGroupChildren="items" :placeholder="t('system.org.selectParentOrg')" class="w100" showClear/>
      </div>
      <FormLabel name="name">{{ t('system.org.orgName') }}</FormLabel>
      <FormField name="name" v-slot="{ props: fProps, invalid, error }">
        <InputText v-bind="fProps" v-model="ruleForm.name" :placeholder="t('system.org.orgName')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="code">{{ t('system.org.orgCode') }}</FormLabel>
      <FormField name="code" v-slot="{ props: fProps, invalid, error }">
        <InputText v-bind="fProps" v-model="ruleForm.code" :placeholder="t('system.org.orgCode')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel>{{ t('system.org.level') }}</FormLabel>
      <div><InputNumber v-model="ruleForm.level" :placeholder="t('system.org.level')" class="w100"/></div>
      <FormLabel>{{ t('system.org.orgType') }}</FormLabel>
      <div><Select filter v-model="ruleForm.type" :options="orgTypeList" optionLabel="value" optionValue="code" showClear class="w100"/></div>
      <FormLabel>{{ t('common.label.sort') }}</FormLabel>
      <div><InputNumber v-model="ruleForm.orderNo" :placeholder="t('common.label.sort')" class="w100"/></div>
      <FormLabel>{{ t('common.label.status') }}</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.isEnabled"/></div>
      <FormLabel>{{ t('common.label.remark') }}</FormLabel>
      <div><Textarea v-model="ruleForm.remark" :placeholder="t('common.form.pleaseInput')" class="w-full" autoResize/></div>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>

import {computed, onMounted, ref} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import CascadeSelect from 'primevue/cascadeselect'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {tenantOrganizationAddPost, tenantOrganizationUpdatePost} from '../../../../../api/tenantOrganization';
import {globalDictionaryDataDataListGet} from '../../../../../api/globalDictionaryData';
import {TenantOrganizationEntity} from '../../../../../api/_shared/data-contracts.ts'

import InputText from 'primevue/inputtext';
import Textarea from 'primevue/textarea';
import InputNumber from 'primevue/inputnumber';
import Select from 'primevue/select';
import ToggleSwitch from 'primevue/toggleswitch';

var props = defineProps({
  title: {type: String, default: ''},
  orgData: {type: Array<TenantOrganizationEntity>, default: () => []},
})
const emit = defineEmits(['query'])
const dialogRef = ref()

const orgTypeList = ref<any>([])

const convertOrgToCascade = (data: TenantOrganizationEntity[]): any[] => {
  if (!data) return []
  return data.map((item: any) => ({
    value: item.id,
    label: item.name ?? '',
    items: item.children && item.children.length > 0 ? convertOrgToCascade(item.children) : undefined,
  }))
}

const cascadeOrgOptions = computed(() => convertOrgToCascade(props.orgData))

onMounted(async () => {
  let resDicData = await globalDictionaryDataDataListGet({code: 'org_type'})
  orgTypeList.value = resDicData.data.data
})

const rules = {
  name: [{required: true, message: t('system.org.orgName') + t('common.form.required'), trigger: 'blur'}],
  code: [{required: true, message: t('system.org.orgCode') + t('common.form.required'), trigger: 'blur'}],
}

defineExpose({openDialog: (row: any) => dialogRef.value?.openDialog(row)})
</script>
