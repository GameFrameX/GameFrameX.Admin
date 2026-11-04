<template>
  <div class="sys-grantData-container">
    <Dialog v-model:visible="state.dialogVisible" modal :style="{ width: 'min(450px, calc(100vw - 32px))' }" :draggable="true">
      <template #header>
        <div>
          <i class="fa-solid fa-pen-to-square" style="font-size: 16px; margin-right: 3px; vertical-align: middle"></i>
          <span> {{ t('system.role.grantDataScopeTitle') }} </span>
        </div>
      </template>
      <form @submit.prevent>
        <div class="grid" style="gap: 35px">
          <div class="col-12 mb20">
            <div class="field mb20">
              <label class="block mb-1">{{ t('system.role.dataScope') }}</label>
              <Select filter v-model="state.ruleForm.dataScope" :placeholder="t('system.role.dataScope')"
                      :options="state.dataScopeType" optionLabel="label" optionValue="value" class="w100" />
            </div>
          </div>
          <div class="col-12" v-show="state.ruleForm.dataScope === 5">
            <div class="field mb20">
              <label class="block mb-1">{{ t('system.role.orgList') }}</label>
              <OrgTree ref="orgTreeRef" class="w100"/>
            </div>
          </div>
        </div>
      </form>
      <template #footer>
        <div class="dialog-footer">
          <Button severity="secondary" @click="cancel">{{ t('common.button.cancel') }}</Button>
          <Button @click="submit">{{ t('common.button.confirm') }}</Button>
        </div>
      </template>
    </Dialog>
  </div>
</template>

<script lang="ts" setup name="systemGrantData">

import {reactive, ref} from 'vue';
import OrgTree from '/@/components/org-tree/OrgTree.vue';

// ponytail: C400 已删除后端端点，此处只为保持 build 可用；C403 连页删除
// 组织树授权两端点（ownOrgList/grantDataScope）已随组织域删除：本地失败桩与 404 行为一致，不发请求、不伪造数据。
const tenantRoleOwnOrgListGet = (..._args: unknown[]): Promise<never> => Promise.reject(new Error('role data-scope org endpoints retired'));
const tenantRoleGrantDataScopePost = (..._args: unknown[]): Promise<never> => Promise.reject(new Error('role data-scope org endpoints retired'));
import {RoleOrgInput} from '../../../../../api/_shared/data-contracts.ts';

import Dialog from 'primevue/dialog';
import Button from 'primevue/button';
import Select from 'primevue/select';

const emits = defineEmits(['query']);
const orgTreeRef = ref();
const state = reactive({
  dialogVisible: false,
  ruleForm: {} as RoleOrgInput,
  dataScopeType: [
    {value: 1, label: t('system.role.allData')},
    {value: 2, label: t('system.role.deptAndBelowData')},
    {value: 3, label: t('system.role.deptData')},
    {value: 4, label: t('system.role.selfData')},
    {value: 5, label: t('system.role.customData')},
  ],
});

const openDialog = async (row: any) => {
  state.ruleForm = JSON.parse(JSON.stringify(row));
  var res = await tenantRoleOwnOrgListGet(row.id);
  setTimeout(() => {
    orgTreeRef.value?.setCheckedKeys(res.data.data);
  }, 100);
  state.dialogVisible = true;
};

const cancel = () => {
  state.dialogVisible = false;
};

const closeDialog = () => {
  emits('query');
  state.dialogVisible = false;
};

const submit = async () => {
  if (state.ruleForm.dataScope === 5) state.ruleForm.orgIdList = orgTreeRef.value?.getCheckedKeys();
  await tenantRoleGrantDataScopePost(state.ruleForm);
  closeDialog();
};

defineExpose({openDialog});
</script>
