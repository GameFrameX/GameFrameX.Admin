<template>
  <div class="sys-userCenter-container">
    <div class="grid" style="display: grid; grid-template-columns: 1fr 2fr; gap: 8px">
      <div>
        <Card>
          <template #content>
            <div class="account-center-avatarHolder">
              <Avatar
                  v-if="hasCustomAvatar"
                  :image="userInfos.avatar"
                  size="xlarge"
                  shape="circle"
                  style="width: 100px; height: 100px; cursor: pointer"
                  @click="openCropperDialog"
                  v-block-loading="state.avatarLoading"
                  @mouseenter="mouseEnterAvatar"
                  @mouseleave="mouseLeaveAvatar"
              />
              <!-- PrimeVue Avatar 默认插槽会抑制内部 image 分支：有头像走纯 :image，无头像才渲染首字插槽 -->
              <Avatar
                  v-else
                  size="xlarge"
                  shape="circle"
                  style="width: 100px; height: 100px; cursor: pointer"
                  @click="openCropperDialog"
                  v-block-loading="state.avatarLoading"
                  @mouseenter="mouseEnterAvatar"
                  @mouseleave="mouseLeaveAvatar"
              >
                {{ userInfos.realName?.slice(0, 1) }}
              </Avatar>
              <div class="username">{{ userInfos.realName }}</div>
            </div>
            <div class="account-center-org">
              <p>
                <i class="fa-solid fa-school"></i>
                <span>{{ userInfos.orgName ?? t('message.system.user.accountType_superAdmin') }}</span>
              </p>
              <p>
                <i class="fa-solid fa-mug-hot"></i>
                <span>{{ userInfos.posName ?? t('message.system.user.accountType_superAdmin') }}</span>
              </p>
              <p>
                <i class="fa-solid fa-location-dot"></i>
                <span>{{ userInfos.address ?? t('message.system.user.addressPlaceholder') }}</span>
              </p>
            </div>
            <div class="image-signature">
              <img v-if="userInfos.signature" :src="userInfos.signature" alt="Signature" loading="lazy"
                   style="width: 100%; height: 100%; object-fit: contain" />
            </div>
            <Button @click="openSignDialog" v-auth="'systemFile:uploadSignature'">
              <i :class="faPenToSquare" class="mr-1"></i>
              {{ t('system.userCenter.signature') }}
            </Button>
            <FileUpload
                mode="basic"
                accept=".png"
                :chooseLabel="t('common.message.selectImage')"
                :auto="false"
                severity="secondary"
                customUpload
                @select="onSignFileSelect"
                v-auth="'systemFile:uploadSignature'"
                style="display: inline-block; margin-left: 12px"
            />
          </template>
        </Card>
      </div>

      <div v-block-loading="state.loading">
        <Card>
          <template #content>
            <Tabs v-model:value="state.selectedTabName">
              <TabList>
                <Tab value="0">{{ t('system.userCenter.basicInfo') }}</Tab>
                <Tab value="1">{{ t('system.user.orgSection') }}</Tab>
                <Tab value="2">{{ t('system.userCenter.changePwd') }}</Tab>
              </TabList>
              <TabPanels>
                <TabPanel value="0">
                  <Form ref="ruleFormBaseRef" :initialValues="state.ruleFormBase" :resolver="baseResolver" @submit="onBaseFormSubmit">
                    <div style="display: grid; grid-template-columns: auto 1fr; gap: 12px 16px; align-items: start;">
                      <FormLabel name="realName">{{ t('system.user.realName') }}</FormLabel>
                      <FormField name="realName" v-slot="{ props: fProps, invalid, error }">
                        <InputText v-bind="fProps" v-model="state.ruleFormBase.realName" :placeholder="t('system.user.realName')" class="w-full"/>
                        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
                      </FormField>
                      <FormLabel>{{ t('common.label.nickname') }}</FormLabel>
                      <div><InputText v-model="state.ruleFormBase.nickName" :placeholder="t('common.label.nickname')" class="w-full"/></div>
                      <FormLabel name="phone">{{ t('common.label.phone') }}</FormLabel>
                      <FormField name="phone" v-slot="{ props: fProps, invalid, error }">
                        <InputText v-bind="fProps" v-model="state.ruleFormBase.phone" :placeholder="t('common.label.phone')" class="w-full"/>
                        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
                      </FormField>
                      <FormLabel>{{ t('common.label.email') }}</FormLabel>
                      <div><InputText v-model="state.ruleFormBase.email" :placeholder="t('common.label.email')" class="w-full"/></div>
                      <FormLabel name="birthdayTime">{{ t('system.user.birthday') }}</FormLabel>
                      <FormField name="birthdayTime" v-slot="{ props: fProps, invalid, error }">
                        <DatePicker v-bind="fProps" v-model="state.ruleFormBase.birthdayTime" dateFormat="yy-mm-dd" :placeholder="t('system.user.birthday')" class="w100"/>
                        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
                      </FormField>
                      <FormLabel>{{ t('system.user.sex') }}</FormLabel>
                      <div class="flex gap-4">
                        <div class="flex align-items-center">
                          <RadioButton v-model="state.ruleFormBase.sex" :value="1" name="sex" inputId="base_sex_1"/>
                          <label for="base_sex_1" class="ml-2">{{ t('common.label.male') }}</label>
                        </div>
                        <div class="flex align-items-center">
                          <RadioButton v-model="state.ruleFormBase.sex" :value="2" name="sex" inputId="base_sex_2"/>
                          <label for="base_sex_2" class="ml-2">{{ t('common.label.female') }}</label>
                        </div>
                      </div>
                      <FormLabel>{{ t('system.user.address') }}</FormLabel>
                      <div><Textarea v-model="state.ruleFormBase.address" :placeholder="t('system.user.address')" class="w-full" autoResize/></div>
                      <FormLabel>{{ t('common.label.remark') }}</FormLabel>
                      <div><Textarea v-model="state.ruleFormBase.remark" :placeholder="t('common.label.remark')" class="w-full" autoResize/></div>
                      <FormLabel></FormLabel>
                      <div>
                        <Button v-auth="'systemUser:baseInfo'" @click="submitUserBase">
                          <i :class="faCircleCheck" class="mr-1"></i>
                          {{ t('common.button.save') }}
                        </Button>
                      </div>
                    </div>
                  </Form>
                </TabPanel>
                <TabPanel value="1">
                  <OrgTree ref="orgTreeRef"/>
                </TabPanel>
                <TabPanel value="2">
                  <Form ref="ruleFormPasswordRef" :initialValues="state.ruleFormPassword" :resolver="passwordResolver" @submit="onPasswordFormSubmit">
                    <div style="display: grid; grid-template-columns: auto 1fr; gap: 12px 16px; align-items: start;">
                      <FormLabel name="passwordOld">{{ t('system.userCenter.currentPwd') }}</FormLabel>
                      <FormField name="passwordOld" v-slot="{ props: fProps, invalid, error }">
                        <Password v-bind="fProps" v-model="state.ruleFormPassword.passwordOld" :feedback="false" toggleMask class="w-full"/>
                        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
                      </FormField>
                      <FormLabel name="passwordNew">{{ t('system.userCenter.newPwd') }}</FormLabel>
                      <FormField name="passwordNew" v-slot="{ props: fProps, invalid, error }">
                        <Password v-bind="fProps" v-model="state.ruleFormPassword.passwordNew" :feedback="false" toggleMask class="w-full"/>
                        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
                      </FormField>
                      <FormLabel name="passwordNew2">{{ t('system.userCenter.confirmPwd') }}</FormLabel>
                      <FormField name="passwordNew2" v-slot="{ props: fProps, invalid, error }">
                        <Password v-bind="fProps" v-model="state.passwordNew2" :feedback="false" toggleMask class="w-full"/>
                        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
                      </FormField>
                      <FormLabel></FormLabel>
                      <div class="flex gap-2">
                        <Button severity="secondary" @click="resetPassword">
                          <i :class="faArrowsRotate" class="mr-1"></i>
                          {{ t('common.button.reset') }}
                        </Button>
                        <Button v-auth="'systemUser:changePwd'" @click="submitPassword">
                          <i :class="faCircleCheck" class="mr-1"></i>
                          {{ t('common.button.confirm') }}
                        </Button>
                      </div>
                    </div>
                  </Form>
                </TabPanel>
              </TabPanels>
            </Tabs>
          </template>
        </Card>
      </div>
    </div>

    <Dialog v-model:visible="state.signDialogVisible" modal :style="{ width: 'min(600px, calc(100vw - 32px))' }" :draggable="true">
      <template #header>
        <div>
          <i class="fa-solid fa-pen" style="font-size: 16px; margin-right: 3px; vertical-align: middle"></i>
          <span> {{ t('system.userCenter.signature') }} </span>
        </div>
      </template>
      <NarrowScreenHint/>
      <div style="border: 1px dashed var(--p-content-border-color); width: 100%; height: 250px">
        <!-- 签名板画布恒定白底（豁免 token 纪律）：模拟白纸签名，暗色模式下深底黑笔不可见 -->
        <VueSignaturePad ref="signaturePadRef" :options="state.signOptions" style="background-color: #fff"/>
      </div>
      <div style="margin-top: 10px">
        <div style="display: inline">{{ t('system.userCenter.penWidth') }}
          <InputNumber v-model="state.signOptions.minWidth" :min="0.5" :max="2.5" :step="0.1" size="small" style="width: 100px"/>
        </div>
        <div style="display: inline; margin-left: 30px">{{ t('system.userCenter.penColor') }}
          <ColorPicker v-model="state.signOptions.penColor"/>
        </div>
      </div>
      <template #footer>
        <div class="dialog-footer">
          <Button severity="secondary" @click="unDoSign">{{ t('common.button.cancel') }}</Button>
          <Button severity="secondary" @click="clearSign">{{ t('system.log.clearLog') }}</Button>
          <Button @click="saveUploadSign">{{ t('common.button.save') }}</Button>
        </div>
      </template>
    </Dialog>

    <CropperDialog ref="cropperDialogRef" :title="state.cropperTitle" @uploadCropperImage="uploadCropperImage"/>
  </div>
</template>

<script lang="ts" setup name="systemUserCenter">
import { faArrowsRotate, faCircleCheck, faCloudArrowUp, faPenToSquare } from '../../../../../utils/icons.ts';
import NarrowScreenHint from '/@/components/narrow-screen-hint/index.vue';

import {computed, onMounted, watch, reactive, ref} from 'vue';
import {storeToRefs} from 'pinia';
import Form from '@primevue/forms/form'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import {buildFormResolver} from '../../../../../utils/formResolver.ts'
import type {FormRules} from '../../../../../composables/types/crud.ts'
import {showConfirm} from '../../../../../utils/message.ts';
import {useUserInfoStore} from '../../../../../stores/userInfo.ts';
import {base64ToFile} from '../../../../../utils/base64Conver.ts';
import {resolveBackendFileUrl} from '../../../../../utils/fileUrl.ts';
import OrgTree from '/@/views/tenant/system/user/component/OrgTree.vue';
import CropperDialog from '/@/components/image-cropper/index.vue';
import VueGridLayout from 'vue-grid-layout';

import {clearAccessTokens} from '../../../../../utils/axiosUtils.ts';
import {tenantFileUploadAvatarPost, tenantFileUploadSignaturePost} from '../../../../../api/tenantFile';
import {tenantUserBaseInfoGet, tenantUserBaseInfoPost, tenantUserChangePwdPost} from '../../../../../api/tenantUser';
import {ChangePwdInput, TenantUserEntity} from '../../../../../api/_shared/data-contracts.ts';

import Card from 'primevue/card';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import Textarea from 'primevue/textarea';
import InputNumber from 'primevue/inputnumber';
import RadioButton from 'primevue/radiobutton';
import Password from 'primevue/password';
import DatePicker from 'primevue/datepicker';
import Avatar from 'primevue/avatar';
import Dialog from 'primevue/dialog';
import ColorPicker from 'primevue/colorpicker';
import FileUpload from 'primevue/fileupload';
import Tabs from 'primevue/tabs';
import TabList from 'primevue/tablist';
import Tab from 'primevue/tab';
import TabPanels from 'primevue/tabpanels';
import TabPanel from 'primevue/tabpanel';

const stores = useUserInfoStore();
const {userInfos} = storeToRefs(stores);

// store 的 avatar 无头像时回退 /favicon.ico；个人中心把回退值仍渲染为首字而非站点图标
const hasCustomAvatar = computed(() => !!userInfos.value.avatar && !userInfos.value.avatar.endsWith('/favicon.ico'));
const signaturePadRef = ref<InstanceType<typeof VueGridLayout>>();
const ruleFormBaseRef = ref();
const ruleFormPasswordRef = ref();
const cropperDialogRef = ref<InstanceType<typeof CropperDialog>>();
const state = reactive({
  loading: false,
  avatarLoading: false,
  signDialogVisible: false,
  selectedTabName: '0',
  ruleFormBase: {} as TenantUserEntity,
  ruleFormPassword: {} as ChangePwdInput,
  signOptions: {
    penColor: '#000000',
    minWidth: 1.0,
    onBegin: () => {
      signaturePadRef.value.resizeCanvas();
    },
  },
  signFileList: [] as any,
  passwordNew2: '',
  cropperTitle: '',
});

const baseRules = ref<FormRules>({
  realName: [{required: true, message: t('system.user.realName') + t('common.form.required'), trigger: 'blur'}],
  phone: [{required: true, message: t('common.label.phone') + t('common.form.required'), trigger: 'blur'}],
  birthdayTime: [{required: true, message: t('system.user.birthday') + t('common.form.required'), trigger: 'blur'}],
})
const baseResolver = buildFormResolver(baseRules)

const passwordRules = ref<FormRules>({
  passwordOld: [{required: true, message: t('system.userCenter.currentPwd') + t('common.form.required'), trigger: 'blur'}],
  passwordNew: [{required: true, message: t('system.userCenter.newPwd') + t('common.form.required'), trigger: 'blur'}],
  passwordNew2: [{required: true, message: t('system.userCenter.confirmPwd') + t('common.form.required'), trigger: 'blur'}],
})
const passwordResolver = buildFormResolver(passwordRules, (values: any) => {
  if (values.passwordNew2 !== values.passwordNew) {
    return {passwordNew2: t('system.userCenter.pwdNotMatch')}
  }
  return {}
})

onMounted(async () => {
  state.loading = true;
  var res = await tenantUserBaseInfoGet();
  const base: any = res.data.data ?? {account: ''};
  // 回显：long(秒) → Date 供 DatePicker 显示
  if (base.birthdayTime && typeof base.birthdayTime === 'number') {
    base.birthdayTime = new Date(base.birthdayTime * 1000);
  }
  state.ruleFormBase = base;
  state.loading = false;
});

watch(state.signOptions, () => {
  signaturePadRef.value.signaturePad.penColor = state.signOptions.penColor;
  signaturePadRef.value.signaturePad.minWidth = state.signOptions.minWidth;
});

const uploadCropperImage = async (e: any) => {
  var res = await tenantFileUploadAvatarPost({file: e.img});
  userInfos.value.avatar = resolveBackendFileUrl(res.data.data?.url || `${res.data.data?.filePath}/${res.data.data?.name}`);
};

const openSignDialog = () => {
  state.signDialogVisible = true;
};

const saveUploadSign = async () => {
  const {isEmpty, data} = signaturePadRef.value.saveSignature();
  if (isEmpty) return;

  var res = await tenantFileUploadSignaturePost({file: base64ToFile(data, userInfos.value.account + '.png')});
  userInfos.value.signature = resolveBackendFileUrl(res.data.data?.url || `${res.data.data?.filePath}/${res.data.data?.name}`);

  clearSign();
  state.signDialogVisible = false;
};

const unDoSign = () => {
  signaturePadRef.value.undoSignature();
};

const clearSign = () => {
  signaturePadRef.value.clearSignature();
};

const onSignFileSelect = async (event: any) => {
  const file = event.files?.[0]
  if (!file) return
  var res = await tenantFileUploadSignaturePost({file: file});
  userInfos.value.signature = resolveBackendFileUrl(res.data.data?.url);
};

const onChangeSignFile = (_file: any, fileList: []) => {
  state.signFileList = fileList;
};

const onBaseFormSubmit = async ({valid}: { valid: boolean }) => {
  if (!valid) return;
  if (await showConfirm(t('common.message.tip'), t('common.message.tip'))) {
    const payload: any = {...state.ruleFormBase};
    // 提交：Date → long 秒；空值保持 null
    if (payload.birthdayTime) {
      payload.birthdayTime = Math.floor(new Date(payload.birthdayTime).getTime() / 1000);
    }
    await tenantUserBaseInfoPost(payload);
  }
};

const submitUserBase = () => {
  ruleFormBaseRef.value?.submit()
};

const onPasswordFormSubmit = async ({valid}: { valid: boolean }) => {
  if (!valid) return;
  await tenantUserChangePwdPost(state.ruleFormPassword);
  if (await showConfirm(t('system.userCenter.changePwdSuccess'), t('common.message.tip'))) {
    clearAccessTokens();
  }
};

const resetPassword = () => {
  state.ruleFormPassword.passwordOld = '';
  state.ruleFormPassword.passwordNew = '';
  state.passwordNew2 = '';
};

const submitPassword = () => {
  ruleFormPasswordRef.value?.submit()
};

const openCropperDialog = () => {
  state.cropperTitle = t('common.label.avatar');
  cropperDialogRef.value?.openDialog(userInfos.value.avatar);
};

const mouseEnterAvatar = () => {
  state.avatarLoading = true;
};

const mouseLeaveAvatar = () => {
  state.avatarLoading = false;
};

defineExpose({onChangeSignFile});
</script>

<style lang="scss" scoped>
.account-center-avatarHolder {
  text-align: center;
  margin-bottom: 24px;

  .username {
    font-size: 20px;
    line-height: 28px;
    font-weight: 500;
    margin-bottom: 4px;
  }
}

.account-center-org {
  margin-bottom: 8px;
  position: relative;

  p {
    margin-top: 10px;
  }

  span {
    padding-left: 10px;
  }
}

.avatar {
  margin: 0 auto;
  width: 104px;
  height: 104px;
  margin-bottom: 20px;
  border-radius: 50%;
  overflow: hidden;

  img {
    height: 100%;
    width: 100%;
  }
}

.image-signature {
  margin-top: 20px;
  margin-bottom: 10px;
  width: 100%;
  height: 150px;
  // 已保存签名图为白底图片，容器恒定白底对齐（豁免 token 纪律）
  background-color: #fff;
  text-align: center;
  vertical-align: middle;
  border: solid 1px var(--p-content-border-color);
}
</style>
