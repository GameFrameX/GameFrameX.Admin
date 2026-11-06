<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="(params: any) => userAddPost(params)"
    :updateApi="(params: any) => userUpdatePost(params)"
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
                <FormLabel>{{ t('system.user.jobNum') }}</FormLabel>
                <div><InputText v-model="ruleForm.jobNum" :placeholder="t('system.user.jobNum')" class="w-full"/></div>
                <FormLabel>{{ t('system.user.joinDate') }}</FormLabel>
                <div><DatePicker v-model="ruleForm.joinTime" dateFormat="yy-mm-dd" :placeholder="t('system.user.joinDate')" class="w100"/></div>
                <FormLabel>{{ t('common.label.sort') }}</FormLabel>
                <div><InputNumber v-model="ruleForm.orderNo" :placeholder="t('common.label.sort')" class="w100"/></div>
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

import {computed, onMounted, ref} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import BaseEditFieldGroup from '/@/components/base-edit-dialog/FieldGroup.vue'
import {roleListGet} from '../../../../api/role';
import {userAddPost, userUpdatePost, userOwnRoleListGet} from '../../../../api/user';
import {RoleOutput} from '../../../../api/_shared/data-contracts.ts'

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
})
const emit = defineEmits(['query'])
const dialogRef = ref()

const loading = ref(false)
const selectedTabName = ref('0')
const roleData = ref<RoleOutput[]>([])

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
  var res1 = await roleListGet()
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
    var resRole = await userOwnRoleListGet(form.id)
    form.roleIdList = resRole.data.data
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

defineExpose({
  openDialog: (row: any) => {
    selectedTabName.value = '0'
    dialogRef.value?.openDialog(row)
  }
})
</script>
