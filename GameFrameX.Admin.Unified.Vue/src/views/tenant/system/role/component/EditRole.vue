<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="(params: any) => tenantRoleAddPost(params)"
    :updateApi="(params: any) => tenantRoleUpdatePost(params)"
    :rules="rules"
    :beforeSubmit="beforeSubmit"
    idCheckMode="system"
    scrollable
    @reloadTable="emit('query')"
  >
    <template #default="{ ruleForm }">
      <div style="grid-column: 1 / -1; display: grid; grid-template-columns: auto 1fr; gap: 12px 16px; align-items: start;">
        <FormLabel name="name">{{ t('system.role.roleName') }}</FormLabel>
        <FormField name="name" v-slot="{ props: fProps, invalid, error }">
          <InputText v-bind="fProps" v-model="ruleForm.name" :placeholder="t('system.role.roleName')" class="w-full"/>
          <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
        </FormField>
        <FormLabel name="code">{{ t('system.role.roleCode') }}</FormLabel>
        <FormField name="code" v-slot="{ props: fProps, invalid, error }">
          <InputText v-bind="fProps" v-model="ruleForm.code" :placeholder="t('system.role.roleCode')" class="w-full" :disabled="ruleForm.code == 'sys_admin' && ruleForm.id != undefined"/>
          <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
        </FormField>
        <FormLabel>{{ t('common.label.sort') }}</FormLabel>
        <div><InputNumber v-model="ruleForm.orderNo" :placeholder="t('common.label.sort')" class="w100"/></div>
        <FormLabel>{{ t('common.label.status') }}</FormLabel>
        <div><ToggleSwitch v-model="ruleForm.isEnabled"/></div>
        <FormLabel>{{ t('common.label.remark') }}</FormLabel>
        <div><Textarea v-model="ruleForm.remark" :placeholder="t('common.form.pleaseInput')" class="w-full" autoResize/></div>
        <FormLabel>{{ t('system.role.menuPermission') }}</FormLabel>
        <div v-block-loading="loading">
          <Tree ref="treeRef" :value="menuTreeNodes" v-model:selectionKeys="selectedMenuKeys" selectionMode="checkbox" v-model:expandedKeys="menuExpandedKeys" class="w-full" :metaKeySelection="false"/>
        </div>
      </div>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {computed, onMounted, ref} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import Tree from 'primevue/tree';
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {globalMenuListGet} from '../../../../../api/globalMenu';
import {tenantRoleAddPost, tenantRoleUpdatePost, tenantRoleOwnMenuListGet} from '../../../../../api/tenantRole';
import {GlobalMenuEntity} from '../../../../../api/_shared/data-contracts.ts'
import {isEnabled} from '../../../../../utils/isEnabled.ts'

import InputText from 'primevue/inputtext';
import Textarea from 'primevue/textarea';
import InputNumber from 'primevue/inputnumber';
import ToggleSwitch from 'primevue/toggleswitch';

var props = defineProps({title: {type: String, default: ''}})
const emit = defineEmits(['query'])

const dialogRef = ref()
const treeRef = ref()
const loading = ref(false)
const menuData = ref<GlobalMenuEntity[]>([])
const selectedMenuKeys = ref<Record<string, boolean>>({})
const menuExpandedKeys = ref<Record<string, boolean>>({})

const convertMenuToTreeNodes = (data: GlobalMenuEntity[]): any[] => {
  return data.map(item => ({
    key: String(item.id),
    label: item.title ?? '',
    data: item,
    children: item.children ? convertMenuToTreeNodes(item.children) : [],
  }));
};

const menuTreeNodes = computed(() => convertMenuToTreeNodes(menuData.value));

const expandAllMenuNodes = () => {
  const keys: Record<string, boolean> = {};
  const expandRecursive = (items: any[]) => {
    for (const item of items) {
      if (item.children && item.children.length > 0) {
        keys[item.key] = true;
        expandRecursive(item.children);
      }
    }
  };
  expandRecursive(menuTreeNodes.value);
  menuExpandedKeys.value = keys;
};

onMounted(async () => {
  loading.value = true
  var res = await globalMenuListGet()
  menuData.value = res.data.data ?? []
  loading.value = false
  expandAllMenuNodes();
})

const rules = {
  name: [{required: true, message: t('system.role.roleName') + t('common.form.required'), trigger: 'blur'}],
  code: [{required: true, message: t('system.role.roleCode') + t('common.form.required'), trigger: 'blur'}],
}

const beforeSubmit = (form: any) => ({
  id: form.id,
  name: form.name,
  code: form.code,
  orderNo: form.orderNo,
  isEnabled: isEnabled(form),
  remark: form.remark,
  menuIdList: Object.keys(selectedMenuKeys.value)
    .filter(k => selectedMenuKeys.value[k])
    .map(Number) as Array<number>,
  rowVersion: form.rowVersion,
})

defineExpose({
  openDialog: async (row: any) => {
    selectedMenuKeys.value = {}
    await dialogRef.value?.openDialog(row)
    if (row.id != undefined) {
      var res = await tenantRoleOwnMenuListGet(row.id)
      setTimeout(() => {
        const keys: Record<string, boolean> = {};
        (res.data.data ?? []).forEach((id: any) => {
          keys[String(id)] = true;
        });
        selectedMenuKeys.value = {...keys};
      }, 100)
    }
  }
})
</script>

<style lang="scss" scoped>
</style>
