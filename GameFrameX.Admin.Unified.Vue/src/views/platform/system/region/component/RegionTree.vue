<template>
  <Card style="height: 100%">
    <template #header>
      <div class="card-header">
        <div class="tree-h-flex" style="padding: 0.75rem 1rem">
          <div class="tree-h-left">
            <InputText v-model="filterText" :placeholder="t('system.region.regionName')" class="w-full" />
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
            :filter="!!filterText"
            filterBy="label"
            v-model:expandedKeys="expandedKeys"
            @node-expand="onNodeExpand"
            @node-select="onNodeSelect"
            :loading="state.loading"
            class="w-full"
        />
      </div>
    </template>
  </Card>
</template>

<script lang="ts" setup>
import {onMounted, reactive, ref, computed} from 'vue';

import {systemRegionListGet} from '../../../../../api/systemRegion';
import {SystemRegionEntity} from '../../../../../api/_shared/data-contracts.ts';

import Card from 'primevue/card';
import InputText from 'primevue/inputtext';
import Button from 'primevue/button';
import Tree from 'primevue/tree';
import Menu from 'primevue/menu';


const filterText = ref('');
const menuRef = ref();
const treeRef = ref();
const expandedKeys = ref<Record<string, boolean>>({});
const state = reactive({
  loading: false,
  regionData: [] as Array<SystemRegionEntity>,
});
const defaultRootRegionId = 1;

const menuItems = computed(() => [
  {label: t('system.org.expandAll'), command: () => expandAll()},
  {label: t('system.org.collapseAll'), command: () => collapseAll()},
  {label: t('system.org.rootNode'), command: () => emits('nodeSelect', {id: defaultRootRegionId, name: ''})},
  {label: t('common.button.refresh'), command: () => initTreeData()},
]);

const toggleMenu = (event: Event) => {
  menuRef.value.toggle(event);
};

const convertToTreeNodes = (data: SystemRegionEntity[]): any[] => {
  return data.map(item => ({
    key: String(item.id),
    label: item.name ?? '',
    data: item,
    leaf: false,
    children: [],
  }));
};

const treeNodes = computed(() => convertToTreeNodes(state.regionData));

onMounted(() => {
  initTreeData();
});

const initTreeData = async () => {
  state.loading = true;
  var res = await systemRegionListGet({id: defaultRootRegionId});
  state.regionData = res.data.data ?? [];
  state.loading = false;
};

const onNodeExpand = async (node: any) => {
  if (node.children && node.children.length > 0) return;

  state.loading = true;
  var res = await systemRegionListGet({id: node.key});
  var childrenData = res.data.data ?? [];
  node.children = convertToTreeNodes(childrenData);
  state.loading = false;
};

const expandAll = () => {
  const keys: Record<string, boolean> = {};
  const expandRecursive = (items: any[]) => {
    for (const item of items) {
      keys[item.key] = true;
      if (item.children && item.children.length > 0) {
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

const getCheckedKeys = () => {
  return [] as number[];
};

const onNodeSelect = (node: any) => {
  emits('nodeSelect', {id: node.data?.id ?? node.key, name: node.label});
};

const emits = defineEmits(['nodeSelect']);

defineExpose({initTreeData, getCheckedKeys});
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
