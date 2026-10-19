<template>
  <div class="sys-org-container flex gap-2 w-full">
    <div class="org-tree-sidebar">
      <!-- 移动端树折叠开关（C252）：bp-down(md) 下树默认收起，表格恢复全宽 -->
      <Button
        class="org-tree-toggle"
        severity="secondary"
        size="small"
        outlined
        :aria-expanded="isMobileTreeOpen"
        :aria-label="t('system.org.filterByOrgTree')"
        @click="isMobileTreeOpen = !isMobileTreeOpen"
      >
        <i class="fa-solid fa-sitemap mr-1"></i>
        {{ t('system.org.filterByOrgTree') }}
      </Button>
      <OrgTree v-show="isMobileTreeOpen" ref="orgTreeRef" @nodeSelect="nodeClick"/>
    </div>

    <div class="flex-1 min-w-0">
      <Card>
        <template #content>
          <div class="flex flex-wrap gap-3 align-items-end">
            <div class="flex flex-column gap-1">
              <label>{{ t('system.org.orgName') }}</label>
              <InputText v-model="state.queryParams.name" :placeholder="t('system.org.orgName')" />
            </div>
            <div class="flex flex-column gap-1">
              <label>{{ t('system.org.orgCode') }}</label>
              <InputText v-model="state.queryParams.code" :placeholder="t('system.org.orgCode')" />
            </div>
            <div class="flex flex-column gap-1">
              <label>{{ t('system.org.orgType') }}</label>
              <Select filter v-model="state.queryParams.type" showClear :options="state.orgTypeList" optionLabel="value" optionValue="code" class="w100" />
            </div>
            <div class="flex gap-2">
              <Button @click="onQuery">
                <i :class="faMagnifyingGlass" class="mr-1"></i>
                {{ t('common.button.search') }}
              </Button>
              <Button severity="secondary" @click="resetQuery">
                <i :class="faArrowsRotate" class="mr-1"></i>
                {{ t('common.button.reset') }}
              </Button>
            </div>
            <div>
              <Button v-auth="'systemOrg:add'" @click="openAddOrg">
                <i :class="faPlus" class="mr-1"></i>
                {{ t('common.button.add') }}
              </Button>
            </div>
          </div>
        </template>
      </Card>

      <Card style="margin-top: 8px">
        <template #content>
          <!-- 树形列表（GFX-922）：后端返回嵌套 children，必须显式用 TreeTable 渲染
               （CrudDataTable 内部 DataTable 不识别 children，只渲染顶层行） -->
          <TreeTable
              :value="organizationTreeNodes"
              v-model:expandedKeys="state.expandedKeys"
              style="width: 100%"
              v-block-loading="state.loading"
              dataKey="key"
              scrollable
              resizableColumns
              columnResizeMode="expand"
          >
            <Column field="name" :header="t('system.org.orgName')" expander />
            <Column field="code" :header="t('system.org.orgCode')" />
            <Column field="level" :header="t('system.org.level')" style="width: 70px" />
            <Column field="type" :header="t('system.org.orgType')" :bodyClass="'text-center'" :headerClass="'text-center'">
              <template #body="{ node }">
                {{ dictFormatter(node.data) }}
              </template>
            </Column>
            <Column field="orderNo" :header="t('common.label.sort')" style="width: 70px" />
            <Column :header="t('common.label.status')" style="width: 70px">
              <template #body="{ node }">
                <Tag v-if="isEnabled(node.data)" severity="success">{{ t('common.label.enable') }}</Tag>
                <Tag v-else severity="danger">{{ t('common.label.disable') }}</Tag>
              </template>
            </Column>
            <!-- TimeColumn 依赖 DataTable 的 {data} 插槽参数，与 TreeTable 的 {node} 不兼容，改普通 Column + formatUnix -->
            <Column field="createdTime" :header="t('common.label.createTime')" style="width: 190px">
              <template #body="{ node }">
                {{ formatUnix(node.data.createdTime) }}
              </template>
            </Column>
            <Column field="remark" :header="t('common.label.remark')" />
            <Column
                v-if="showActionColumn"
                :header="t('common.table.action')"
                style="width: 140px"
                frozen
                alignFrozen="right"
            >
              <template #body="{ node }">
                <Button link size="small" v-auth="'systemOrg:update'" @click="openEditOrg(node.data)">
                  <i class="fa-solid fa-pen-to-square mr-1"></i><span class="crud-action-label">{{ t('common.button.edit') }}</span>
                </Button>
                <Button text size="small" severity="danger" v-auth="'systemOrg:delete'" @click="delOrg(node.data)">
                  <i class="fa-solid fa-trash mr-1"></i><span class="crud-action-label">{{ t('common.button.delete') }}</span>
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
        </template>
      </Card>
    </div>

    <EditOrg ref="editOrgRef" :title="state.editOrgTitle" :orgData="state.orgTreeData" @query="onQuery"/>
  </div>
</template>

<script lang="ts" setup name="systemOrg">
import { faArrowsRotate, faMagnifyingGlass, faPlus } from '../../../../utils/icons.ts';

import {computed, onMounted, reactive, ref, watch} from 'vue';
import {showConfirm, showSuccess} from '../../../../utils/message.ts';
import OrgTree from '/@/views/tenant/system/org/component/OrgTree.vue';
import EditOrg from '/@/views/tenant/system/org/component/EditOrg.vue';

import {tenantOrganizationListGet, tenantOrganizationDeletePost} from '../../../../api/tenantOrganization';
import {globalDictionaryDataDataListGet} from '../../../../api/globalDictionaryData';
import {TenantOrganizationEntity} from '../../../../api/_shared/data-contracts.ts';
import {isEnabled} from '../../../../utils/isEnabled.ts';
import {formatUnix} from '../../../../utils/formatTime.ts';
import {hasAnyAuth} from '../../../../utils/authFunction.ts';
import {useMediaQueryMatch} from '../../../../composables/useMediaQueryMatch.ts';

import Card from 'primevue/card';
import InputText from 'primevue/inputtext';
import Select from 'primevue/select';
import Button from 'primevue/button';
import TreeTable from 'primevue/treetable';
import Column from 'primevue/column';
import Tag from 'primevue/tag';
import type {TreeNode} from 'primevue/treenode';

const editOrgRef = ref<InstanceType<typeof EditOrg>>();

// 移动端组织树折叠（C252）：窄视口（bp-down(md)）下树默认收起、点按钮展开；
// 桌面端树恒显示（脱离窄视口时保持展开）。
const isNarrowViewport = useMediaQueryMatch('(max-width: 767px)');
const isMobileTreeOpen = ref(!isNarrowViewport.value);
watch(isNarrowViewport, (narrow) => {
  if (!narrow) {
    isMobileTreeOpen.value = true;
  }
});
const orgTreeRef = ref<InstanceType<typeof OrgTree>>();
const defaultRootOrgId = 1300000000101;
const state = reactive({
  loading: false,
  orgData: [] as Array<TenantOrganizationEntity>,
  orgTreeData: [] as Array<TenantOrganizationEntity>,
  expandedKeys: {} as Record<string, boolean>,
  queryParams: {
    id: defaultRootOrgId,
    name: undefined,
    code: undefined,
    type: undefined,
  },
  editOrgTitle: '',
  orgTypeList: [] as any,
});

// ponytail: 仓内第 3 份本地 TreeNode 转换（platform/system/menu、platform/system/tenantPackage 同款），
// 出现更多消费方时提取共享 util。
const convertOrganizationToTreeNodes = (data: TenantOrganizationEntity[]): TreeNode[] => {
  return data.map((item) => ({
    key: String(item.id),
    data: item,
    children: item.children && item.children.length > 0 ? convertOrganizationToTreeNodes(item.children) : undefined,
  }));
};

const organizationTreeNodes = computed<TreeNode[]>(() => convertOrganizationToTreeNodes(state.orgData));

const showActionColumn = computed(() => hasAnyAuth('systemOrg:update', 'systemOrg:delete'));

onMounted(async () => {
  onQuery();

  let resDicData = await globalDictionaryDataDataListGet({code: 'org_type'});
  state.orgTypeList = resDicData.data.data;
});

const onQuery = async (updateTree: boolean = false) => {
  state.loading = true;
  var res = await tenantOrganizationListGet({id: state.queryParams.id, name: state.queryParams.name, code: state.queryParams.code, type: state.queryParams.type});
  state.orgData = res.data.data ?? [];
  state.expandedKeys = autoExpandAll(state.orgData);
  state.loading = false;
  if (updateTree == true) {
    orgTreeRef.value?.initTreeData();
  }

  if (state.queryParams.id == defaultRootOrgId && state.queryParams.name == undefined && state.queryParams.code == undefined && state.queryParams.type == undefined) state.orgTreeData = state.orgData;
};

const resetQuery = () => {
  state.queryParams.id = defaultRootOrgId;
  state.queryParams.name = undefined;
  state.queryParams.code = undefined;
  state.queryParams.type = undefined;
  onQuery();
};

const openAddOrg = () => {
  state.editOrgTitle = t('system.org.addOrg');
  editOrgRef.value?.openDialog({isEnabled: true, orderNo: 100});
};

const openEditOrg = (row: any) => {
  state.editOrgTitle = t('system.org.editOrg');
  editOrgRef.value?.openDialog(row);
};

const delOrg = async (row: any) => {
  if (!await showConfirm(`${t('system.org.deleteOrgConfirm')}：【${row.name}】?`, t('common.message.tip'), {type: 'danger'})) return;
  await tenantOrganizationDeletePost({id: row.id});
  showSuccess(t('common.message.deleteSuccess'));
  onQuery(true);
};

const nodeClick = async (node: any) => {
  state.queryParams.id = node.id;
  state.queryParams.name = undefined;
  state.queryParams.code = undefined;
  state.queryParams.type = undefined;
  onQuery();
  // 移动端选完节点自动收起树（C252），把屏幕还给结果表
  if (isNarrowViewport.value) {
    isMobileTreeOpen.value = false;
  }
};

const dictFormatter = (data: any) => {
  return state.orgTypeList.find((u: any) => u.code == data.type)?.value;
};

const autoExpandAll = (data: any[]) => {
  const expanded: any = {};
  const expandRecursive = (items: any[]) => {
    for (const item of items) {
      if (item.children && item.children.length > 0) {
        expanded[item.id] = true;
        expandRecursive(item.children);
      }
    }
  };
  expandRecursive(data);
  return expanded;
};
</script>

<style lang="scss" scoped>
.org-tree-sidebar {
  width: 280px;
  min-width: 280px;
  flex-shrink: 0;
}

// 空态复刻 CrudDataTable #empty（GFX-922，样式按 platform/system/menu 先例页内复制）
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

// 移动端操作按钮只显示图标，隐藏文字（对齐替换前 CrudDataTable 行为，阈值见 theme/breakpoints.scss）
@include bp-down(sm) {
  .crud-action-label {
    display: none;
  }

  // 文字隐藏后按钮只剩图标，扩触控目标至 36px（GFX-661）
  :deep(.p-treetable-tbody .p-button) {
    min-width: 36px;
    min-height: 36px;
    justify-content: center;
  }
}

// 移动端组织树侧栏降级（C252，GFX-658）：同 system/user，bp-down(md) 下
// 容器竖排、侧栏全宽、树默认收起，由 .org-tree-toggle 按钮控制显隐。
.org-tree-toggle {
  display: none;
}

@include bp-down(md) {
  .sys-org-container {
    flex-direction: column;
  }

  .org-tree-sidebar {
    width: 100%;
    min-width: 0;
    flex-shrink: 1;
  }

  .org-tree-toggle {
    display: inline-flex;
    width: 100%;
    justify-content: center;
  }
}
</style>
