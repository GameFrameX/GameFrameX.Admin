<template>
  <div class="sys-notice-container">
    <Dialog v-model:visible="state.dialogVisible" :draggable="true" :modal="true" :closable="true" :style="{width: 'min(600px, calc(100vw - 32px))'}">
      <template #header>
        <div>
          <i class="fa-solid fa-pen-to-square" style="margin-right: 3px; font-size: 16px; vertical-align: middle"></i>
          <span> {{ props.title }} </span>
        </div>
      </template>
      <form @submit.prevent ref="ruleFormRef">
        <div class="grid" style="gap: 0px">
          <div class="col-12 mb20">
            <div class="field mb20">
              <label>{{ t('system.openAccess.accessName') }}</label>
              <InputText v-model="state.ruleForm.accessKey" :placeholder="t('system.openAccess.accessName')" class="w100"/>
            </div>
          </div>
          <div class="col-12 mb20">
            <div class="field mb20">
              <label>{{ t('system.openAccess.authToken') }}</label>
              <div class="flex">
                <InputText v-model="state.ruleForm.accessSecret" :placeholder="t('system.openAccess.authToken')" class="w100"/>
                <Button severity="secondary" @click="createSecret">生成密钥</Button>
              </div>
            </div>
          </div>
          <div class="col-6 mb20">
            <div class="field mb20">
              <label>{{ t('message.system.openAccess.bindTenant') }}</label>
              <Select v-model="state.ruleForm.bindTenantId" :options="state.tenantData" optionLabel="name" optionValue="id" placeholder="绑定租户" filter showClear class="w100" @change="tenantChange"/>
            </div>
          </div>
          <div class="col-6 mb20">
            <div class="field mb20">
              <label>{{ t('message.system.openAccess.bindUser') }}</label>
              <Select v-model="state.ruleForm.bindUserId" :options="userDataOptions" optionLabel="label" optionValue="id" placeholder="绑定用户" filter showClear class="w100"/>
            </div>
          </div>
        </div>
      </form>
      <template #footer>
        <Button severity="secondary" @click="cancel">{{ t('common.button.cancel') }}</Button>
        <Button @click="submit">{{ t('common.button.confirm') }}</Button>
      </template>
    </Dialog>
  </div>
</template>

<script lang="ts" setup name="systemOpenAccessEdit">

import {onMounted, reactive, ref, computed} from 'vue';

import {systemOpenAccessUpdatePost, systemOpenAccessAddPost, systemOpenAccessCreateSecretPost} from '../../../../../api/systemOpenAccess';
import {systemTenantPagePost, systemTenantUserListPost} from '../../../../../api/systemTenant';
import {TenantUserEntity, TenantOutput, UpdateOpenAccessInput} from '../../../../../api/_shared/data-contracts.ts';
import {MAX_PAGE_SIZE} from '../../../../../constants/pagination.ts';

const props = defineProps({
  title: String,
});
const emits = defineEmits(['query']);
const ruleFormRef = ref();
const state = reactive({
  dialogVisible: false,
  ruleForm: {} as UpdateOpenAccessInput,
  tenantData: [] as Array<TenantOutput>,
  userData: [] as Array<TenantUserEntity>,
});

const userDataOptions = computed(() => {
  return state.userData.map((item: any) => ({
    id: item.id,
    label: `${item.account}(${item.realName})`,
  }));
});

onMounted(async () => {
  var res = await systemTenantPagePost({page: 1, pageSize: MAX_PAGE_SIZE});
  state.tenantData = res.data.data?.items ?? [];
});

const openDialog = (row: any) => {
  state.ruleForm = JSON.parse(JSON.stringify(row));
  state.dialogVisible = true;
  ruleFormRef.value?.resetFields?.();

  tenantChange();
};

const closeDialog = () => {
  emits('query');
  state.dialogVisible = false;
};

const cancel = () => {
  state.dialogVisible = false;
};

const submit = () => {
  if (!state.ruleForm.accessKey) return;
  if (!state.ruleForm.accessSecret) return;
  (async () => {
    if (state.ruleForm.id != undefined && state.ruleForm.id > 0) {
      await systemOpenAccessUpdatePost(state.ruleForm);
    } else {
      await systemOpenAccessAddPost(state.ruleForm);
    }
    closeDialog();
  })();
};

const tenantChange = async () => {
  if (!state.ruleForm.bindTenantId) {
    state.ruleForm.bindUserId = undefined;
    state.userData = [];
    return;
  }

  var res = await systemTenantUserListPost({tenantId: state.ruleForm.bindTenantId});
  state.userData = res.data.data ?? [];
};

const createSecret = async () => {
  var res = await systemOpenAccessCreateSecretPost();
  state.ruleForm.accessSecret = res.data.data?.secret ?? '';
};

defineExpose({openDialog});
</script>
