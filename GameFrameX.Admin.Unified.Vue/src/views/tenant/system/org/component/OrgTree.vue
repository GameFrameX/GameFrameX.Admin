<template>
  <Card style="height: 100%">
    <template #header>
      <div class="card-header">
        <div class="tree-h-flex" style="padding: 0.75rem 1rem">
          <div class="tree-h-left">
            <InputText v-model="filterText" :placeholder="t('system.org.orgName')" class="w-full" />
          </div>
          <div class="tree-h-right">
            <Button style="margin-left: 8px" severity="secondary" @click="toggleMenu">
              <i class="fa-solid fa-ellipsis-vertical"></i>
            </Button>
            <Menu ref="menuRef" :model="menuItems" :popup="true" />
          </div>
        </div>
      </div>
    </template>
    <template #content>
      <div style="margin-bottom: 45px" v-block-loading="state.loading">
        <Tree
            ref="treeRef"
            :value="treeNodes"
            :filter="true"
            filterBy="label"
            v-model:expandedKeys="expandedKeys"
            v-model:selectionKeys="selectionKeys"
            :selectionMode="state.isShowCheckbox ? 'checkbox' : 'single'"
            @node-select="onNodeSelect"
            :metaKeySelection="false"
            class="w-full"
        />
      </div>
    </template>
  </Card>
</template>

<script lang="ts" setup>
import {onMounted, reactive, ref, computed} from 'vue';

import {tenantOrganizationListGet} from '../../../../../api/tenantOrganization';
import {TenantOrganizationEntity} from '../../../../../api/_shared/data-contracts.ts';

import Card from 'primevue/card';
import InputText from 'primevue/inputtext';
import Button from 'primevue/button';
import Tree from 'primevue/tree';
import Menu from 'primevue/menu';


const filterText = ref('');
const menuRef = ref();
const treeRef = ref();
const expandedKeys = ref<Record<string, boolean>>({});
const selectionKeys = ref<Record<string, boolean>>({});
const state = reactive({
  loading: false,
  orgData: [] as Array<TenantOrganizationEntity>,
  isShowCheckbox: false,
  ownOrgData: [] as any[],
});
const defaultRootOrgId = 1300000000101;

const menuItems = computed(() => [
  {label: t('system.org.expandAll'), command: () => expandAll()},
  {label: t('system.org.collapseAll'), command: () => collapseAll()},
  {label: t('system.org.rootNode'), command: () => emits('nodeSelect', {id: defaultRootOrgId, name: ''})},
  {label: t('common.button.refresh'), command: () => initTreeData()},
]);

const toggleMenu = (event: Event) => {
  menuRef.value.toggle(event);
};

const convertToTreeNodes = (data: TenantOrganizationEntity[]): any[] => {
  return data.map(item => ({
    key: String(item.id),
    label: item.name ?? '',
    data: item,
    children: item.children ? convertToTreeNodes(item.children) : [],
  }));
};

const treeNodes = computed(() => convertToTreeNodes(state.orgData));

onMounted(() => {
  initTreeData();
});

const initTreeData = async () => {
  state.loading = true;
  var res = await tenantOrganizationListGet({id: defaultRootOrgId});
  state.orgData = res.data.data ?? [];
  state.loading = false;
};

const expandAll = () => {
  const keys: Record<string, boolean> = {};
  const expandRecursive = (items: any[]) => {
    for (const item of items) {
      if (item.children && item.children.length > 0) {
        keys[item.key] = true;
        expandRecursive(item.children);
      }
    }
  };
  expandRecursive(treeNodes.value);
  expandedKeys.value = keys;
};

const collapseAll = () => {
  expandedKeys.value = {};
};

const setCheckedKeys = (orgData: any) => {
  state.isShowCheckbox = true;
  selectionKeys.value = {};
  state.ownOrgData = orgData;
  const keys: Record<string, boolean> = {};
  if (Array.isArray(orgData)) {
    orgData.forEach((id: any) => {
      keys[String(id)] = true;
    });
  }
  selectionKeys.value = {...keys};
};

const getCheckedKeys = () => {
  return Object.keys(selectionKeys.value).filter(k => selectionKeys.value[k]).map(Number);
};

const onNodeSelect = (node: any) => {
  emits('nodeSelect', {id: node.data?.id ?? node.key, name: node.label});
};

const emits = defineEmits(['nodeSelect']);

defineExpose({initTreeData, setCheckedKeys, getCheckedKeys});
</script>

<style lang="scss" scoped>
.tree-h-flex {
  display: flex;
}

.tree-h-left {
  flex: 1;
  width: 100%;
}

.tree-h-right {
  width: 42px;
  min-width: 42px;
}
</style>
