<template>
  <div class="sys-menu-container">
    <Card>
      <template #content>
        <div class="flex flex-wrap gap-3 align-items-end">
          <div class="flex flex-column gap-1">
            <label>{{ t('system.menu.menuName') }}</label>
            <InputText v-model="state.queryParams.title" :placeholder="t('system.menu.menuName')" />
          </div>
          <div class="flex flex-column gap-1">
            <label>{{ t('common.label.type') }}</label>
            <Select filter v-model="state.queryParams.type" :placeholder="t('common.label.type')" showClear :options="typeOptions" optionLabel="label" optionValue="value" />
          </div>
          <div class="flex gap-2">
            <Button v-auth="'systemMenu:list'" @click="onQuery">
              <i :class="faMagnifyingGlass" class="mr-1"></i>
              {{ t('common.button.search') }}
            </Button>
            <Button severity="secondary" @click="resetQuery">
              <i :class="faArrowsRotate" class="mr-1"></i>
              {{ t('common.button.reset') }}
            </Button>
          </div>
          <div>
            <Button v-auth="'systemMenu:add'" @click="openAddMenu">
              <i :class="faPlus" class="mr-1"></i>
              {{ t('common.button.add') }}
            </Button>
          </div>
        </div>
      </template>
    </Card>

    <Card style="margin-top: 8px">
      <template #content>
        <TreeTable
            ref="menuTreeTableRef"
            :value="menuTreeNodes"
            v-model:expandedKeys="state.expandedKeys"
            v-block-loading="state.loading"
            class="menu-tree-table"
            dataKey="key"
            showGridlines
            scrollable
            scrollHeight="calc(100vh - 330px)"
            resizableColumns
            columnResizeMode="fit"
            tableStyle="width: max(100%, 1550px); min-width: 100%; table-layout: fixed"
        >
          <Column :header="t('system.menu.menuName')" expander style="width: 280px">
            <template #body="{ node }">
              <SvgIcon :name="node.data.icon"/>
              <span class="ml10">{{ t(node.data.titleCode || node.data.title) }}</span>
            </template>
          </Column>
          <Column :header="t('common.label.type')" style="width: 70px">
            <template #body="{ node }">
              <Tag v-if="node.data.type === 1" severity="warn">{{ t('system.menu.directory') }}</Tag>
              <Tag v-else-if="node.data.type === 2">{{ t('system.menu.menu') }}</Tag>
              <Tag v-else severity="secondary">{{ t('system.menu.button') }}</Tag>
            </template>
          </Column>
          <Column field="path" :header="t('system.menu.routePath')" style="width: 220px" />
          <Column field="component" :header="t('system.menu.componentPath')" style="width: 260px" />
          <Column field="permission" :header="t('system.menu.permission')" style="width: 220px" />
          <Column field="orderNo" :header="t('common.label.sort')" style="width: 70px" />
          <Column :header="t('common.label.status')" style="width: 80px">
            <template #body="{ node }">
              <Tag v-if="isEnabled(node.data)" severity="success">{{ t('common.label.enable') }}</Tag>
              <Tag v-else severity="danger">{{ t('common.label.disable') }}</Tag>
            </template>
          </Column>
          <TimeColumn field="createdTime" :header="t('common.label.createTime')" width="190px" />
          <Column
              v-if="showActionColumn"
              :header="t('common.table.action')"
              style="width: 160px"
              frozen
              alignFrozen="right"
          >
            <template #body="{ node }">
              <Button
                  link
                  size="small"
                  v-auth="'systemMenu:update'"
                  @click="openEditMenu(node.data)"
              >
                <i class="fa-solid fa-pen-to-square mr-1"></i>{{ t('common.button.edit') }}
              </Button>
              <Button
                  text
                  size="small"
                  severity="danger"
                  v-auth="'systemMenu:delete'"
                  @click="delMenu(node.data)"
              >
                <i class="fa-solid fa-trash mr-1"></i>{{ t('common.button.delete') }}
              </Button>
            </template>
          </Column>
          <template #empty>
            <div class="crud-data-table-empty">
              <i class="fa-solid fa-inbox crud-data-table-empty-icon"></i>
              <span class="crud-data-table-empty-text">{{ t('message.common.table.noData') }}</span>
            </div>
          </template>
        </TreeTable>
        <div v-show="horizontalScrollMax > 0" class="menu-horizontal-scroll-control">
          <input
              v-model.number="horizontalScrollLeft"
              class="menu-horizontal-scroll-range"
              type="range"
              min="0"
              :max="horizontalScrollMax"
              step="1"
              aria-label="menu horizontal scroll"
              :style="horizontalScrollRangeStyle"
              @input="syncTableScrollFromRange"
          />
        </div>
      </template>
    </Card>

    <EditMenu ref="editMenuRef" :title="state.editMenuTitle" :menuData="state.menuData" @query="onQuery"/>
  </div>
</template>

<script lang="ts" setup name="systemMenu">
import { faArrowsRotate, faMagnifyingGlass, faPlus } from '../../../../utils/icons.ts';

import {computed, nextTick, onBeforeUnmount, onMounted, reactive, ref} from 'vue';
import {showConfirm, showSuccess} from '../../../../utils/message.ts';
import EditMenu from '/@/views/platform/system/menu/component/EditMenu.vue';

import {globalMenuListGet, globalMenuDeletePost} from '../../../../api/globalMenu';
import {GlobalMenuEntity} from '../../../../api/_shared/data-contracts.ts';
import {hasAnyAuth} from '../../../../utils/authFunction.ts';
import {isEnabled} from '../../../../utils/isEnabled.ts';

import Card from 'primevue/card';
import InputText from 'primevue/inputtext';
import Select from 'primevue/select';
import Button from 'primevue/button';
import TreeTable from 'primevue/treetable';
import Column from 'primevue/column';
import Tag from 'primevue/tag';
import type {TreeNode} from 'primevue/treenode';

const typeOptions = computed(() => [
  {label: t('system.menu.directory'), value: 1},
  {label: t('system.menu.menu'), value: 2},
  {label: t('system.menu.button'), value: 3},
]);

const editMenuRef = ref<InstanceType<typeof EditMenu>>();
const menuTreeTableRef = ref<InstanceType<typeof TreeTable>>();
const horizontalScrollWidth = ref(0);
const horizontalScrollClientWidth = ref(0);
const horizontalScrollLeft = ref(0);
let tableScrollContainer: HTMLElement | null = null;
let isSyncingScroll = false;
const state = reactive({
  loading: false,
  menuData: [] as Array<GlobalMenuEntity>,
  expandedKeys: {} as Record<string, boolean>,
  queryParams: {
    title: undefined,
    type: undefined,
  },
  editMenuTitle: '',
});

const menuTreeNodes = computed<TreeNode[]>(() => convertMenuToTreeNodes(state.menuData));

const showActionColumn = computed(() => hasAnyAuth('systemMenu:update', 'systemMenu:delete'));
const horizontalScrollMax = computed(() => Math.max(0, horizontalScrollWidth.value - horizontalScrollClientWidth.value));
const horizontalScrollThumbWidth = computed(() => {
  if (horizontalScrollWidth.value <= 0 || horizontalScrollClientWidth.value <= 0) return '48px';
  const thumbWidth = horizontalScrollClientWidth.value / horizontalScrollWidth.value * horizontalScrollClientWidth.value;
  return `${Math.max(48, Math.min(horizontalScrollClientWidth.value, thumbWidth))}px`;
});
const horizontalScrollRangeStyle = computed(() => ({
  '--menu-scroll-thumb-width': horizontalScrollThumbWidth.value,
}));

const convertMenuToTreeNodes = (data: GlobalMenuEntity[]): TreeNode[] => {
  return data.map((item) => ({
    key: String(item.id),
    data: item,
    children: item.children && item.children.length > 0 ? convertMenuToTreeNodes(item.children) : undefined,
  }));
};

const autoExpandAll = (data: GlobalMenuEntity[]) => {
  const expanded: Record<string, boolean> = {};
  const expandRecursive = (items: GlobalMenuEntity[]) => {
    for (const item of items) {
      if (item.id != null && item.children && item.children.length > 0) {
        expanded[String(item.id)] = true;
        expandRecursive(item.children);
      }
    }
  };
  expandRecursive(data);
  return expanded;
};

onMounted(async () => {
  window.addEventListener('resize', refreshHorizontalScroll);
  onQuery();
});

onBeforeUnmount(() => {
  window.removeEventListener('resize', refreshHorizontalScroll);
  unbindTableScrollContainer();
});

const onQuery = async () => {
  state.loading = true;
  var res = await globalMenuListGet({title: state.queryParams.title, type: state.queryParams.type});
  state.menuData = res.data.data ?? [];
  state.expandedKeys = autoExpandAll(state.menuData);
  state.loading = false;
  refreshHorizontalScroll();
};

const resetQuery = () => {
  state.queryParams.title = undefined;
  state.queryParams.type = undefined;
  onQuery();
};

const openAddMenu = () => {
  state.editMenuTitle = t('system.menu.addMenu');
  editMenuRef.value?.openDialog({
    type: 2,
    isHide: false,
    isKeepAlive: true,
    isAffix: false,
    isIframe: false,
    isEnabled: true,
    orderNo: 100
  });
};

const openEditMenu = (row: any) => {
  state.editMenuTitle = t('system.menu.editMenu');
  editMenuRef.value?.openDialog(row);
};

const delMenu = async (row: any) => {
  const confirmed = await showConfirm(`${t('system.menu.deleteMenuConfirm')}：【${row.title}】?`, t('common.message.tip'), {type: 'danger'})
  if (!confirmed) return
  await globalMenuDeletePost({id: row.id});
  onQuery();
  showSuccess(t('common.message.deleteSuccess'));
};

const getTableScrollContainer = () => {
  return (menuTreeTableRef.value?.$el as HTMLElement | undefined)?.querySelector('.p-treetable-table-container') as HTMLElement | null;
};

const unbindTableScrollContainer = () => {
  tableScrollContainer?.removeEventListener('scroll', syncHorizontalScrollFromTable);
  tableScrollContainer = null;
};

const bindTableScrollContainer = () => {
  const nextContainer = getTableScrollContainer();
  if (nextContainer === tableScrollContainer) return;
  unbindTableScrollContainer();
  tableScrollContainer = nextContainer;
  tableScrollContainer?.addEventListener('scroll', syncHorizontalScrollFromTable, {passive: true});
};

const refreshHorizontalScroll = async () => {
  await nextTick();
  bindTableScrollContainer();
  const container = getTableScrollContainer();
  horizontalScrollWidth.value = container?.scrollWidth ?? 0;
  horizontalScrollClientWidth.value = container?.clientWidth ?? 0;
  if (container) {
    horizontalScrollLeft.value = container.scrollLeft;
  }
};

const syncHorizontalScrollFromTable = () => {
  if (isSyncingScroll || !tableScrollContainer) return;
  isSyncingScroll = true;
  horizontalScrollLeft.value = tableScrollContainer.scrollLeft;
  requestAnimationFrame(() => {
    isSyncingScroll = false;
  });
};

const syncTableScrollFromRange = () => {
  if (isSyncingScroll || !tableScrollContainer) return;
  isSyncingScroll = true;
  tableScrollContainer.scrollLeft = horizontalScrollLeft.value;
  requestAnimationFrame(() => {
    isSyncingScroll = false;
  });
};

</script>

<style lang="scss" scoped>
.crud-data-table-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 40px 0;

  .crud-data-table-empty-icon {
    font-size: 40px;
    color: var(--p-surface-400);
    margin-bottom: 10px;
  }

  .crud-data-table-empty-text {
    color: var(--p-surface-500);
    font-size: 14px;
  }
}

:deep(.menu-tree-table .p-treetable-tbody > tr > td) {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

:deep(.menu-tree-table .p-treetable-table-container) {
  overflow-x: scroll !important;
  scrollbar-gutter: stable;
}

:deep(.menu-tree-table .p-treetable-table-container::-webkit-scrollbar) {
  width: 12px;
  height: 12px;
}

:deep(.menu-tree-table .p-treetable-table-container::-webkit-scrollbar-track) {
  background: var(--p-surface-100);
  border-radius: 999px;
}

:deep(.menu-tree-table .p-treetable-table-container::-webkit-scrollbar-thumb) {
  background: var(--p-surface-400);
  border-radius: 999px;
  border: 2px solid var(--p-surface-100);
}

:deep(.menu-tree-table .p-treetable-table-container::-webkit-scrollbar-thumb:hover) {
  background: var(--p-surface-500);
}

:deep(.menu-tree-table .p-treetable-frozen-column) {
  white-space: nowrap;
}

.menu-horizontal-scroll-control {
  display: flex;
  align-items: center;
  height: 22px;
  margin-top: 6px;
  padding: 0 2px;
}

.menu-horizontal-scroll-range {
  width: 100%;
  height: 14px;
  appearance: none;
  background: transparent;
  cursor: grab;
}

.menu-horizontal-scroll-range:active {
  cursor: grabbing;
}

.menu-horizontal-scroll-range::-webkit-slider-runnable-track {
  height: 10px;
  background: var(--p-surface-100);
  border: 1px solid var(--p-surface-200);
  border-radius: 999px;
}

.menu-horizontal-scroll-range::-webkit-slider-thumb {
  width: var(--menu-scroll-thumb-width, 80px);
  height: 10px;
  margin-top: -1px;
  appearance: none;
  background: var(--p-surface-400);
  border-radius: 999px;
}

.menu-horizontal-scroll-range::-webkit-slider-thumb:hover {
  background: var(--p-surface-500);
}

.menu-horizontal-scroll-range::-moz-range-track {
  height: 10px;
  background: var(--p-surface-100);
  border: 1px solid var(--p-surface-200);
  border-radius: 999px;
}

.menu-horizontal-scroll-range::-moz-range-thumb {
  width: var(--menu-scroll-thumb-width, 80px);
  height: 10px;
  background: var(--p-surface-400);
  border: 0;
  border-radius: 999px;
}
</style>
