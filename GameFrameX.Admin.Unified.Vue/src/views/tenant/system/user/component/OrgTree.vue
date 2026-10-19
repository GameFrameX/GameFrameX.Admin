<template>
  <div v-block-loading="state.loading">
    <div style="text-align: right">
      <div style="margin-right: 10px">
        <ToggleSwitch v-model="state.horizontal"/>
        {{ t('system.server.serverInfo') }}
      </div>
    </div>
    <div style="height: 500px">
      <vue3-tree-org
          :data="state.orgData"
          :props="{ id: 'id', pid: 'pid', label: 'name', expand: 'expand', children: 'children' }"
          :labelStyle="state.style"
          :defaultExpandLevel="100"
          :horizontal="state.horizontal"
          :collapsable="state.collapsable"
          :onlyOneNode="state.onlyOneNode"
          :cloneNodeDrag="state.cloneNodeDrag"
          :nodeDraggable="state.nodeDraggable"
          style="background-color: var(--p-content-background)"
      >
        <template v-slot="{ node }">
          <div class="tree-org-node__text node-label">
            <div class="node-title">{{ node.label }}</div>
            <div class="node-id">{{ t('common.label.code') }}{{ node.id }}</div>
          </div>
        </template>
        <template v-slot:expand="{ node }">
          <div>{{ node.children.length }}</div>
        </template>
      </vue3-tree-org>
    </div>
  </div>
</template>

<script lang="ts" setup name="orgTree">
import {onMounted, reactive} from 'vue';
import {storeToRefs} from 'pinia';
import {useUserInfoStore} from '../../../../../stores/userInfo.ts';

import {tenantOrganizationListGet} from '../../../../../api/tenantOrganization';

const stores = useUserInfoStore();
const {userInfos} = storeToRefs(stores);
const currentNodeStyle = {color: 'var(--p-text-color)', background: 'var(--p-surface-200)'};
const state = reactive({
  loading: false,
  orgData: [] as any,
  horizontal: false,
  collapsable: true,
  onlyOneNode: false,
  cloneNodeDrag: false,
  nodeDraggable: false,
  style: {
    background: 'var(--p-primary-color)',
    color: 'var(--p-primary-contrast-color)',
  },
});
const defaultRootOrgId = 1300000000101;

onMounted(async () => {
  state.loading = true;
  var res = await tenantOrganizationListGet({id: defaultRootOrgId});
  var d = res.data.data ?? [];
  state.orgData = d[0] ?? []; // 默认第一个树分支
  if (state.orgData.id == userInfos.value.orgId) state.orgData.style = currentNodeStyle;
  else initOrg(state.orgData.children, userInfos.value.orgId);
  state.loading = false;
});

// 递归遍历
const initOrg = (orgData: any, id: any) => {
  if (orgData && orgData.length > 0) {
    orgData.forEach(function (u: any) {
      if (u.id == id) {
        u.style = currentNodeStyle;
        return;
      } else {
        initOrg(u.children, id);
      }
    });
  }
};
</script>

<style lang="scss" scoped>
.tree-org-node__text {
  // text-align: left;
  font-size: 14px;

  .node-title {
    padding-bottom: 8px;
    margin-bottom: 8px;
    border-bottom: 1px solid currentColor;
  }

  .node-id {
    font-size: 12px;
  }
}
</style>
