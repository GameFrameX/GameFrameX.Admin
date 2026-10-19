<template>
  <BaseEditDialog ref="dialogRef"
                  :title="dialogRef?.isEditor ? t('common.button.edit') : t('common.button.add')"
                  :addApi="(params: any) => globalPackageAddPost(params)"
                  :updateApi="(params: any) => globalPackageUpdatePost(params)"
                  :rules="rules"
                  :beforeOpen="beforeOpen"
                  :beforeSubmit="beforeSubmit"
                  idCheckMode="system"
                  scrollable
                  @reloadTable="emit('query')">
    <template #default="{ ruleForm, isEditor }">
      <Message v-if="isEnterprisePackage(ruleForm)" severity="warn" size="small" variant="simple" style="width: 100%">
        {{ t('system.tenantPackage.enterpriseReadOnlyWarning') }}
      </Message>
      <TextField v-model="ruleForm.name" name="name" :label="t('system.tenantPackage.name')" :disabled="isEditor" />

      <TextField v-model="ruleForm.code" name="code" :label="t('system.tenantPackage.code')" :disabled="isEditor" />

      <FormLabel name="maxUserCount">{{ t('system.tenantPackage.maxUserCount') }}</FormLabel>
      <FormField name="maxUserCount" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.maxUserCount" :disabled="isEditor" class="w-full" :min="1" />
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <FormLabel name="maxOrgCount">{{ t('system.tenantPackage.maxOrgCount') }}</FormLabel>
      <FormField name="maxOrgCount" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.maxOrgCount" :disabled="isEditor" class="w-full" :min="1" />
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <FormLabel name="maxOrgLevel">{{ t('system.tenantPackage.maxOrgLevel') }}</FormLabel>
      <FormField name="maxOrgLevel" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.maxOrgLevel" :disabled="isEditor" class="w-full" :min="1" />
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <FormLabel name="maxRoleCount">{{ t('system.tenantPackage.maxRoleCount') }}</FormLabel>
      <FormField name="maxRoleCount" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.maxRoleCount" :disabled="isEditor" class="w-full" :min="1" />
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <FormLabel name="maxGameAppCount">{{ t('system.tenantPackage.maxGameAppCount') }}</FormLabel>
      <FormField name="maxGameAppCount" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.maxGameAppCount" :disabled="isEditor" class="w-full" :min="1" />
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <FormLabel name="maxPackageCountPerApp">{{ t('system.tenantPackage.maxPackageCountPerApp') }}</FormLabel>
      <FormField name="maxPackageCountPerApp" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.maxPackageCountPerApp" :disabled="isEditor" class="w-full" :min="1" />
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <FormLabel name="gracePeriodDays">{{ t('system.tenantPackage.gracePeriodDays') }}</FormLabel>
      <FormField name="gracePeriodDays" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.gracePeriodDays" :disabled="isEditor" class="w-full" :min="0" :max="365" />
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <FormLabel name="isEnabled">{{ t('common.label.status') }}</FormLabel>
      <FormField name="isEnabled" v-slot="{ props: fProps }">
        <ToggleSwitch v-bind="fProps" v-model="ruleForm.isEnabled" />
      </FormField>

      <FormLabel>{{ t('system.tenantPackage.menuSelect') }}</FormLabel>
      <div>
        <Tree v-model:selectionKeys="selectedKeys" :value="menuData" selectionMode="checkbox" class="w-full border rounded" style="max-height: 300px; overflow: auto;" />
      </div>

      <TextField v-model="ruleForm.remark" name="remark" :label="t('common.label.remark')" />
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup name="editTenantPackage">
import { onMounted, ref } from 'vue';
import FormField from '@primevue/forms/formfield';
import Message from 'primevue/message';
import Tree from 'primevue/tree';
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue';
import {TextField} from '../../../../../components/crud-form-fields';
import { globalPackageAddPost, globalPackageUpdatePost } from '../../../../../api/globalPackage';
import {globalMenuListGet} from '../../../../../api/globalMenu';
import type { GlobalMenuEntity } from '../../../../../api/_shared/data-contracts.ts';
import type { TreeNode } from 'primevue/treenode';
import { isEnabled } from '../../../../../utils/isEnabled.ts';
// C354：Enterprise 保留套餐（后端 TenantDefaultDataTemplates.EnterprisePackageCode）功能菜单由系统自动维护，
// 编辑保存会被后端拒绝（TenantErrorCode.EnterprisePackageMenuReadOnly = 30600013），对话框内只读提示
const ENTERPRISE_PACKAGE_CODE = 'enterprise'
const isEnterprisePackage = (form: any) => form?.code?.toLowerCase() === ENTERPRISE_PACKAGE_CODE

const emit = defineEmits(['query']);
const dialogRef = ref();

const selectedKeys = ref<Record<string, { checked?: boolean; partialChecked?: boolean }>>({});
const menuData = ref<TreeNode[]>([]);

const convertMenuToTreeNodes = (menus: GlobalMenuEntity[]): TreeNode[] => menus.map(menu => ({
  key: String(menu.id),
  label: menu.title,
  icon: menu.icon ?? undefined,
  data: menu,
  children: menu.children?.length ? convertMenuToTreeNodes(menu.children) : undefined,
}));

const rules = {
  name: [{ required: true, message: t('system.tenantPackage.name') + t('common.form.required'), trigger: 'blur' }],
  code: [{ required: true, message: t('system.tenantPackage.code') + t('common.form.required'), trigger: 'blur' }],
};

const beforeOpen = (form: any) => {
  selectedKeys.value = {};
  if (form.menuIdList?.length) {
    const keys: Record<string, { checked?: boolean; partialChecked?: boolean }> = {};
    form.menuIdList.forEach((key: number) => {
      keys[String(key)] = { checked: true };
    });
    selectedKeys.value = keys;
  }
  return {
    ...form,
    isEnabled: isEnabled(form),
  };
};

const beforeSubmit = (form: any) => {
  form.menuIdList = Object.keys(selectedKeys.value)
    .filter(k => selectedKeys.value[k].checked && !selectedKeys.value[k].partialChecked)
    .map(Number);
  form.isEnabled = isEnabled(form);
  return form;
};

onMounted(async () => {
  const res = await globalMenuListGet();
  menuData.value = convertMenuToTreeNodes(res.data.data ?? []);
});

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
});
</script>
