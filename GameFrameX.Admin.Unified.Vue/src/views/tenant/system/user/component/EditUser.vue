<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="(params: any) => tenantUserAddPost(params)"
    :updateApi="(params: any) => tenantUserUpdatePost(params)"
    :rules="rules"
    :beforeOpen="beforeOpen"
    :beforeSubmit="beforeSubmit"
    idCheckMode="system"
    scrollable
    @reloadTable="emit('query')"
  >
    <template #default="{ ruleForm }">
      <div v-block-loading="loading" style="grid-column: 1 / -1">
        <Tabs v-model:value="selectedTabName">
          <TabList>
            <Tab value="0">{{ t('system.userCenter.basicInfo') }}</Tab>
            <Tab value="1">{{ t('system.user.archiveInfo') }}</Tab>
          </TabList>
          <TabPanels>
            <TabPanel value="0">
              <BaseEditFieldGroup variant="plain">
                <FormLabel name="account">{{ t('system.user.accountName') }}</FormLabel>
                <FormField name="account" v-slot="{ props: fProps, invalid, error }">
                  <InputText v-bind="fProps" v-model="ruleForm.account" :placeholder="t('system.user.accountName')" class="w-full"/>
                  <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
                </FormField>
                <FormLabel name="realName">{{ t('system.user.realName') }}</FormLabel>
                <FormField name="realName" v-slot="{ props: fProps, invalid, error }">
                  <InputText v-bind="fProps" v-model="ruleForm.realName" :placeholder="t('system.user.realName')" class="w-full"/>
                  <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
                </FormField>
                <FormLabel name="phone">{{ t('common.label.phone') }}</FormLabel>
                <FormField name="phone" v-slot="{ props: fProps, invalid, error }">
                  <InputText v-bind="fProps" v-model="ruleForm.phone" :placeholder="t('common.label.phone')" class="w-full"/>
                  <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
                </FormField>
                <FormLabel name="birthdayTime">{{ t('system.user.birthday') }}</FormLabel>
                <FormField name="birthdayTime" v-slot="{ props: fProps, invalid, error }">
                  <DatePicker v-bind="fProps" v-model="ruleForm.birthdayTime" dateFormat="yy-mm-dd" :placeholder="t('system.user.birthday')" class="w100"/>
                  <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
                </FormField>
                <FormLabel>{{ t('common.label.nickname') }}</FormLabel>
                <div><InputText v-model="ruleForm.nickName" :placeholder="t('common.label.nickname')" class="w-full"/></div>
                <FormLabel>{{ t('system.user.sex') }}</FormLabel>
                <div class="flex gap-4">
                  <div class="flex align-items-center">
                    <RadioButton v-model="ruleForm.sex" :value="1" name="sex" inputId="sex_1"/>
                    <label for="sex_1" class="ml-2">{{ t('common.label.male') }}</label>
                  </div>
                  <div class="flex align-items-center">
                    <RadioButton v-model="ruleForm.sex" :value="2" name="sex" inputId="sex_2"/>
                    <label for="sex_2" class="ml-2">{{ t('common.label.female') }}</label>
                  </div>
                  <div class="flex align-items-center">
                    <RadioButton v-model="ruleForm.sex" :value="3" name="sex" inputId="sex_3"/>
                    <label for="sex_3" class="ml-2">{{ t('common.label.other') }}</label>
                  </div>
                </div>
                <FormLabel name="roleIdList">{{ t('system.user.roleSet') }}</FormLabel>
                <FormField name="roleIdList" v-slot="{ props: fProps, invalid, error }">
                  <MultiSelect v-bind="fProps" v-model="ruleForm.roleIdList" :options="roleData" optionLabel="name" optionValue="id" :placeholder="t('system.user.roleSet')" showClear filter class="w100"/>
                  <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
                </FormField>
                <FormLabel name="accountType">{{ t('system.user.accountType') }}</FormLabel>
                <FormField name="accountType" v-slot="{ props: fProps, invalid, error }">
                  <Select filter v-bind="fProps" v-model="ruleForm.accountType" :placeholder="t('system.user.accountType')" :options="accountTypeOptions" optionLabel="label" optionValue="value" class="w100"/>
                  <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
                </FormField>
                <FormLabel>{{ t('system.user.age') }}</FormLabel>
                <div><InputNumber v-model="ruleForm.age" :placeholder="t('system.user.age')" class="w100"/></div>
                <FormLabel>{{ t('common.label.sort') }}</FormLabel>
                <div><InputNumber v-model="ruleForm.orderNo" :placeholder="t('common.label.sort')" class="w100"/></div>
              </BaseEditFieldGroup>

              <BaseEditFieldGroup :title="t('system.user.orgSection')">
                <FormLabel name="orgId">{{ t('system.user.belongOrg') }}</FormLabel>
                <FormField name="orgId" v-slot="{ props: fProps, invalid, error }">
                  <CascadeSelect v-bind="fProps" v-model="ruleForm.orgId" :options="cascadeOrgOptions" optionLabel="label" optionValue="value" optionGroupLabel="label" optionGroupChildren="items" :placeholder="t('system.user.belongOrg')" class="w100" showClear/>
                  <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
                </FormField>
                <FormLabel name="posId">{{ t('system.user.position') }}</FormLabel>
                <FormField name="posId" v-slot="{ props: fProps, invalid, error }">
                  <Select filter v-bind="fProps" v-model="ruleForm.posId" :placeholder="t('system.user.position')" :options="posData" optionLabel="name" optionValue="id" showClear class="w100"/>
                  <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
                </FormField>
                <FormLabel>{{ t('system.user.jobNum') }}</FormLabel>
                <div><InputText v-model="ruleForm.jobNum" :placeholder="t('system.user.jobNum')" class="w-full"/></div>
                <FormLabel>{{ t('system.user.joinDate') }}</FormLabel>
                <div><DatePicker v-model="ruleForm.joinTime" dateFormat="yy-mm-dd" :placeholder="t('system.user.joinDate')" class="w100"/></div>
              </BaseEditFieldGroup>

              <BaseEditFieldGroup :title="t('system.user.extOrgSection')">
                <div style="grid-column: 1 / -1">
                  <div style="margin-bottom: 12px;">
                    <Button outlined @click="addExtOrgRow(ruleForm)">
                      <i :class="faPlus" class="mr-1"></i>
                      {{ t('system.user.addExtOrg') }}
                    </Button>
                    <span style="font-size: 12px; color: gray; padding-left: 5px">{{ t('system.user.extOrgDataPermission') }}</span>
                  </div>
                  <template v-if="ruleForm.extOrgIdList != undefined && ruleForm.extOrgIdList.length > 0">
                    <div v-for="(v, k) in ruleForm.extOrgIdList" :key="k" style="display: grid; grid-template-columns: auto 1fr auto 1fr; gap: 12px 16px; align-items: start; margin-bottom: 12px;">
                      <FormLabel>
                        <Button rounded severity="danger" size="small" @click="deleteExtOrgRow(ruleForm, k)">
                          <i :class="faTrash" class="mr-1"></i>
                        </Button>
                        {{ t('system.user.belongOrg') }}
                      </FormLabel>
                      <CascadeSelect v-model="ruleForm.extOrgIdList[k].orgId" :options="cascadeOrgOptions" optionLabel="label" optionValue="value" optionGroupLabel="label" optionGroupChildren="items" :placeholder="t('system.user.orgPlaceholder')" class="w100" showClear/>
                      <FormLabel>{{ t('system.user.position') }}</FormLabel>
                      <Select filter v-model="ruleForm.extOrgIdList[k].posId" :placeholder="t('system.user.posName')" :options="posData" optionLabel="name" optionValue="id" showClear class="w100"/>
                    </div>
                  </template>
                  <div v-else class="flex flex-column align-items-center" style="padding: 20px 0">
                    <i class="fa-solid fa-inbox" style="font-size: 40px; color: var(--p-text-muted-color); margin-bottom: 8px"></i>
                    <span style="color: var(--p-text-muted-color); font-size: 14px">{{ t('common.table.noData') }}</span>
                  </div>
                </div>
              </BaseEditFieldGroup>
            </TabPanel>
            <TabPanel value="1">
              <BaseEditFieldGroup variant="plain">
                <FormLabel>{{ t('system.user.cardType') }}</FormLabel>
                <div><Select filter v-model="ruleForm.cardType" :placeholder="t('system.user.cardType')" :options="cardTypeOptions" optionLabel="label" optionValue="value" class="w100"/></div>
                <FormLabel>{{ t('system.user.idCardNum') }}</FormLabel>
                <div><InputText v-model="ruleForm.idCardNum" :placeholder="t('system.user.idCardNum')" class="w-full"/></div>
                <FormLabel>{{ t('system.user.nation') }}</FormLabel>
                <div><InputText v-model="ruleForm.nation" :placeholder="t('system.user.nation')" class="w-full"/></div>
                <FormLabel>{{ t('common.label.email') }}</FormLabel>
                <div><InputText v-model="ruleForm.email" :placeholder="t('common.label.email')" class="w-full"/></div>
                <FormLabel>{{ t('system.user.address') }}</FormLabel>
                <div><Textarea v-model="ruleForm.address" :placeholder="t('system.user.address')" class="w-full" :rows="4" autoResize/></div>
                <FormLabel>{{ t('system.user.college') }}</FormLabel>
                <div><InputText v-model="ruleForm.college" :placeholder="t('system.user.college')" class="w-full"/></div>
                <FormLabel>{{ t('system.user.cultureLevel') }}</FormLabel>
                <div><Select filter v-model="ruleForm.cultureLevel" :placeholder="t('system.user.cultureLevel')" :options="cultureLevelOptions" optionLabel="label" optionValue="value" class="w100"/></div>
                <FormLabel>{{ t('system.user.politicalOutlook') }}</FormLabel>
                <div><InputText v-model="ruleForm.politicalOutlook" :placeholder="t('system.user.politicalOutlook')" class="w-full"/></div>
                <FormLabel>{{ t('system.user.officePhone') }}</FormLabel>
                <div><InputText v-model="ruleForm.officePhone" :placeholder="t('system.user.officePhone')" class="w-full"/></div>
                <FormLabel>{{ t('system.user.emergencyContact') }}</FormLabel>
                <div><InputText v-model="ruleForm.emergencyContact" :placeholder="t('system.user.emergencyContact')" class="w-full"/></div>
                <FormLabel>{{ t('system.user.emergencyPhone') }}</FormLabel>
                <div><InputText v-model="ruleForm.emergencyPhone" :placeholder="t('system.user.emergencyPhone')" class="w-full"/></div>
                <FormLabel>{{ t('system.user.emergencyAddress') }}</FormLabel>
                <div><Textarea v-model="ruleForm.emergencyAddress" :placeholder="t('system.user.emergencyContact')" class="w-full" :rows="4" autoResize/></div>
                <FormLabel>{{ t('common.label.remark') }}</FormLabel>
                <div><Textarea v-model="ruleForm.remark" :placeholder="t('common.label.remark')" class="w-full" :rows="4" autoResize/></div>
              </BaseEditFieldGroup>
            </TabPanel>
          </TabPanels>
        </Tabs>
      </div>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {faPlus, faTrash} from '../../../../../utils/icons.ts';

import {computed, onMounted, ref} from 'vue'
import {storeToRefs} from 'pinia'
import {useUserInfoStore} from '../../../../../stores/userInfo.ts'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import CascadeSelect from 'primevue/cascadeselect'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import BaseEditFieldGroup from '/@/components/base-edit-dialog/FieldGroup.vue'
import {tenantPositionListGet} from '../../../../../api/tenantPosition';
import {tenantRoleListGet} from '../../../../../api/tenantRole';
import {tenantUserAddPost, tenantUserUpdatePost, tenantUserOwnRoleListGet, tenantUserOwnExtOrgListGet} from '../../../../../api/tenantUser';
import {RoleOutput, TenantOrganizationEntity, TenantPositionEntity} from '../../../../../api/_shared/data-contracts.ts'

import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import Textarea from 'primevue/textarea';
import InputNumber from 'primevue/inputnumber';
import Select from 'primevue/select';
import MultiSelect from 'primevue/multiselect';
import RadioButton from 'primevue/radiobutton';
import DatePicker from 'primevue/datepicker';
import Tabs from 'primevue/tabs';
import TabList from 'primevue/tablist';
import Tab from 'primevue/tab';
import TabPanels from 'primevue/tabpanels';
import TabPanel from 'primevue/tabpanel';

var props = defineProps({
  title: {type: String, default: ''},
  orgData: {type: Array<TenantOrganizationEntity>, default: () => []},
})
const emit = defineEmits(['query'])
const dialogRef = ref()

const storesUserInfo = useUserInfoStore()
const {userInfos} = storeToRefs(storesUserInfo)

const loading = ref(false)
const selectedTabName = ref('0')
const posData = ref<TenantPositionEntity[]>([])
const roleData = ref<RoleOutput[]>([])

const convertOrgToCascade = (data: TenantOrganizationEntity[]): any[] => {
  if (!data) return []
  return data.map((item: any) => {
    const children = item.children ?? item.items ?? []
    return {
      value: item.id ?? item.value ?? item.key,
      label: item.name ?? item.label ?? '',
      ...(children.length > 0 ? {items: convertOrgToCascade(children)} : {}),
    }
  })
}

const cascadeOrgOptions = computed(() => convertOrgToCascade(props.orgData))

const accountTypeOptions = computed(() => [
  {label: t('system.user.accountType_superAdmin'), value: 888},
  {label: t('system.user.accountType_normal'), value: 777},
  {label: t('system.user.accountType_member'), value: 666},
])

const cardTypeOptions = computed(() => [
  {label: t('system.user.idCard'), value: 0},
  {label: t('system.user.passport'), value: 1},
  {label: t('system.user.birthCert'), value: 2},
  {label: t('system.user.hkMacaoTaiwanPass'), value: 3},
  {label: t('system.user.foreignResidence'), value: 4},
])

const cultureLevelOptions = computed(() => [
  {label: t('system.user.culture_other'), value: 0},
  {label: t('system.user.primary'), value: 1},
  {label: t('system.user.juniorHigh'), value: 2},
  {label: t('system.user.seniorHigh'), value: 3},
  {label: t('system.user.technicalSchool'), value: 4},
  {label: t('system.user.vocationalEdu'), value: 5},
  {label: t('system.user.vocationalHigh'), value: 6},
  {label: t('system.user.secondaryCollege'), value: 7},
  {label: t('system.user.juniorCollege'), value: 8},
  {label: t('system.user.bachelor'), value: 9},
  {label: t('system.user.master'), value: 10},
  {label: t('system.user.doctor'), value: 11},
])

onMounted(async () => {
  loading.value = true
  var res = await tenantPositionListGet()
  posData.value = res.data.data ?? []
  var res1 = await tenantRoleListGet()
  roleData.value = res1.data.data ?? []
  loading.value = false
})

const rules = {
  account: [{required: true, message: t('system.user.accountName') + t('common.form.required'), trigger: 'blur'}],
  realName: [{required: true, message: t('system.user.realName') + t('common.form.required'), trigger: 'blur'}],
  phone: [{required: true, message: t('common.label.phone') + t('common.form.required'), trigger: 'blur'}],
  birthdayTime: [{required: true, message: t('system.user.birthday') + t('common.form.required'), trigger: 'blur'}],
  roleIdList: [{required: true, message: t('system.user.roleSet') + t('common.form.required'), trigger: 'blur'}],
  accountType: [{required: true, message: t('system.user.accountType') + t('common.form.required'), trigger: 'blur'}],
  orgId: [{required: true, message: t('system.user.belongOrg') + t('common.form.required'), trigger: 'blur'}],
  posId: [{required: true, message: t('system.user.posName') + t('common.form.required'), trigger: 'blur'}],
}

const beforeOpen = async (form: any, isEditor: boolean) => {
  // 回显：long(秒) → Date 供 DatePicker 显示
  if (form.birthdayTime && typeof form.birthdayTime === 'number') {
    form.birthdayTime = new Date(form.birthdayTime * 1000)
  }
  if (form.joinTime && typeof form.joinTime === 'number') {
    form.joinTime = new Date(form.joinTime * 1000)
  }
  if (isEditor && form.id != undefined) {
    var resRole = await tenantUserOwnRoleListGet(form.id)
    form.roleIdList = resRole.data.data
    var resExtOrg = await tenantUserOwnExtOrgListGet(form.id)
    form.extOrgIdList = resExtOrg.data.data
  } else {
    form.accountType = 777
  }
}

const beforeSubmit = (form: any) => {
  const payload = {...form}
  // 提交：Date → long 秒；空值保持 null
  if (payload.birthdayTime) {
    payload.birthdayTime = Math.floor(new Date(payload.birthdayTime).getTime() / 1000)
  }
  if (payload.joinTime) {
    payload.joinTime = Math.floor(new Date(payload.joinTime).getTime() / 1000)
  }
  return payload
}

const addExtOrgRow = (form: any) => {
  if (form.extOrgIdList == undefined) form.extOrgIdList = []
  form.extOrgIdList?.push({})
}

const deleteExtOrgRow = (form: any, k: number) => {
  form.extOrgIdList?.splice(k, 1)
}

defineExpose({
  openDialog: (row: any) => {
    selectedTabName.value = '0'
    dialogRef.value?.openDialog(row)
  }
})
</script>
