<template>
  <div class="sys-user-container flex gap-2 w-full h-full">
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
              <label>{{ t('common.label.account') }}</label>
              <InputText v-model="state.queryParams.account" :placeholder="t('common.label.account')" />
            </div>
            <div class="flex flex-column gap-1">
              <label>{{ t('common.label.phone') }}</label>
              <InputText v-model="state.queryParams.phone" :placeholder="t('common.label.phone')" />
            </div>
            <div class="flex gap-2">
              <Button v-auth="'systemUser:page'" @click="onQuery">
                <i :class="faMagnifyingGlass" class="mr-1"></i>
                {{ t('common.button.search') }}
              </Button>
              <Button severity="secondary" @click="resetQuery">
                <i :class="faArrowsRotate" class="mr-1"></i>
                {{ t('common.button.reset') }}
              </Button>
            </div>
            <div>
              <Button v-auth="'systemUser:add'" @click="openAddUser">
                <i :class="faPlus" class="mr-1"></i>
                {{ t('common.button.add') }}
              </Button>
            </div>
          </div>
        </template>
      </Card>

      <Card style="margin-top: 8px">
        <template #content>
          <CrudDataTable :value="state.userData" style="width: 100%" v-block-loading="state.loading" showGridlines
                         :totalRecords="state.tableParams.total"
                         :rows="state.tableParams.pageSize"
                         :rowsPerPageOptions="[10, 20, 50, 100]"
                         :first="(state.tableParams.page - 1) * state.tableParams.pageSize"
                         @page="onPageChange" showIdColumn :idColumnHeader="t('common.label.id')" idColumnWidth="180px">
            <Column field="account" :header="t('common.label.account')" style="width: 120px" />
            <Column field="realName" :header="t('system.user.realName')" style="width: 120px" />
            <Column :header="t('common.label.avatar')" style="width: 80px">
              <template #body="{ data }">
                <Avatar :image="data.avatar" size="normal" shape="circle">
                  {{ data.nickName?.slice(0, 1) ?? data.realName?.slice(0, 1) }}
                </Avatar>
              </template>
            </Column>
            <Column field="phone" :header="t('common.label.phone')" style="width: 120px" />
            <TimeColumn field="birthdayTime" :header="t('system.user.birthday')" width="100px" format="YYYY-mm-dd" />
            <Column :header="t('system.user.sex')" style="width: 70px">
              <template #body="{ data }">
                <Tag v-if="data.sex === 1" severity="success">{{ t('common.label.male') }}</Tag>
                <Tag v-else-if="data.sex === 2" severity="danger">{{ t('common.label.female') }}</Tag>
                <Tag v-else severity="secondary">{{ t('common.label.other') }}</Tag>
              </template>
            </Column>
            <Column :header="t('system.user.accountType')" style="width: 110px">
              <template #body="{ data }">
                <Tag v-if="data.accountType === 888">{{ t('system.user.accountType_superAdmin') }}</Tag>
                <Tag v-else-if="data.accountType === 777">{{ t('system.user.accountType_normal') }}</Tag>
                <Tag v-else-if="data.accountType === 666">{{ t('system.user.accountType_member') }}</Tag>
                <Tag v-else>{{ t('common.label.other') }}</Tag>
              </template>
            </Column>
            <Column :header="t('common.label.status')" style="width: 70px">
              <template #body="{ data }">
                <ToggleSwitch v-model="data.isEnabled"
                              @change="changeStatus(data)" v-auth="'systemUser:setStatus'" />
              </template>
            </Column>
            <Column field="orderNo" :header="t('common.label.sort')" style="width: 70px" />
            <TimeColumn field="createdTime" :header="t('common.label.createTime')" width="160px" />
            <Column field="remark" :header="t('common.label.remark')" />
            <Column :header="t('common.table.action')" style="width: 110px">
              <template #body="{ data }">
                <Button size="small" link v-auth="'systemUser:update'" @click="openEditUser(data)">
                  <i :class="faPenToSquare" class="mr-1"></i>
                  {{ t('common.button.edit') }}
                </Button>
                <Button size="small" link @click="toggleMenu($event, data)">
                  <i :class="faEllipsisVertical" class="mr-1"></i>
                </Button>
              </template>
            </Column>
          </CrudDataTable>
        </template>
      </Card>
    </div>

    <Menu ref="menuRef" :model="contextMenuItems" :popup="true" />

    <EditUser ref="editUserRef" :title="state.editUserTitle" :orgData="state.orgTreeData" @query="onQuery"/>
  </div>
</template>

<script lang="ts" setup name="systemUser">
import { faArrowsRotate, faEllipsisVertical, faMagnifyingGlass, faPenToSquare, faPlus } from '../../../../utils/icons.ts';

import {computed, onMounted, reactive, ref, watch} from 'vue';
import {showConfirm, showSuccess} from '../../../../utils/message.ts';
import {auth} from '../../../../utils/authFunction.ts';
import OrgTree from '/@/views/tenant/system/org/component/OrgTree.vue';
import EditUser from '/@/views/tenant/system/user/component/EditUser.vue';

import {tenantUserDeletePost, tenantUserPagePost, tenantUserResetPwdPost, tenantUserSetStatusPost} from '../../../../api/tenantUser';
import {tenantOrganizationListGet} from '../../../../api/tenantOrganization';
import {TenantOrganizationEntity, TenantUserEntity} from '../../../../api/_shared/data-contracts.ts';
import {normalizeIsEnabled} from '../../../../utils/isEnabled.ts';
import {useMediaQueryMatch} from '../../../../composables/useMediaQueryMatch.ts';

import Card from 'primevue/card';
import InputText from 'primevue/inputtext';
import Button from 'primevue/button';
import Column from 'primevue/column';
import Tag from 'primevue/tag';
import Avatar from 'primevue/avatar';
import ToggleSwitch from 'primevue/toggleswitch';
import CrudDataTable from '/@/components/crud-data-table/index.vue';
import Menu from 'primevue/menu';

const orgTreeRef = ref<InstanceType<typeof OrgTree>>();
const editUserRef = ref<InstanceType<typeof EditUser>>();
const menuRef = ref();
const contextMenuRow = ref<any>(null);
const defaultRootOrgId = 1300000000101;
type UserRow = TenantUserEntity & Record<string, any>;

// 移动端组织树折叠（C252）：窄视口（bp-down(md)）下树默认收起、点按钮展开；
// 桌面端树恒显示（isMobileTreeOpen 初始为 true，且脱离窄视口时保持展开）。
const isNarrowViewport = useMediaQueryMatch('(max-width: 767px)');
const isMobileTreeOpen = ref(!isNarrowViewport.value);
watch(isNarrowViewport, (narrow) => {
  if (!narrow) {
    isMobileTreeOpen.value = true;
  }
});

const contextMenuItems = computed(() => [
  {
    label: t('system.user.resetPwd'),
    icon: 'fa-solid fa-rotate-left',
    command: () => {
      if (contextMenuRow.value) resetUserPwd(contextMenuRow.value);
    },
    disabled: !auth('systemUser:resetPwd'),
  },
  {
    label: t('common.button.delete'),
    icon: 'fa-solid fa-trash',
    command: () => {
      if (contextMenuRow.value) delUser(contextMenuRow.value);
    },
    disabled: !auth('systemUser:delete'),
  },
]);

const toggleMenu = (event: Event, row: any) => {
  contextMenuRow.value = row;
  menuRef.value.toggle(event);
};

const state = reactive({
  loading: false,
  userData: [] as Array<UserRow>,
  orgTreeData: [] as Array<TenantOrganizationEntity>,
  queryParams: {
    orgId: -1,
    account: undefined,
    realName: undefined,
    phone: undefined,
  },
  tableParams: {
    page: 1,
    pageSize: 10,
    total: 0 as any,
  },
  editUserTitle: '',
});

onMounted(async () => {
  loadOrgData();
  onQuery();
});

const loadOrgData = async () => {
  state.loading = true;
  var res = await tenantOrganizationListGet({id: defaultRootOrgId});
  state.orgTreeData = res.data.data ?? [];
  state.loading = false;
};

const onQuery = async () => {
  state.loading = true;
  let params = Object.assign(state.queryParams, state.tableParams);
  var res = await tenantUserPagePost(params);
  state.userData = (res.data.data?.items ?? []).map((row) => normalizeIsEnabled(row) as UserRow);
  state.tableParams.total = res.data.data?.total;
  state.loading = false;
};

const resetQuery = () => {
  state.queryParams.orgId = -1;
  state.queryParams.account = undefined;
  state.queryParams.realName = undefined;
  state.queryParams.phone = undefined;
  onQuery();
};

const openAddUser = () => {
  state.editUserTitle = t('system.user.addAccount');
  editUserRef.value?.openDialog({
    id: undefined,
    birthday: '2000-01-01',
    sex: 1,
    orderNo: 100,
    cardType: 0,
    cultureLevel: 5
  });
};

const openEditUser = (row: any) => {
  state.editUserTitle = t('system.user.editAccount');
  editUserRef.value?.openDialog(row);
};

const delUser = async (row: any) => {
  const confirmed = await showConfirm(`${t('system.user.deleteAccountConfirm')}：【${row.account}】?`, t('common.message.tip'), {type: 'danger'})
  if (!confirmed) return
  await tenantUserDeletePost({id: row.id});
  onQuery();
  showSuccess(t('common.message.deleteSuccess'));
};

const onPageChange = (event: any) => {
  state.tableParams.pageSize = event.rows;
  state.tableParams.page = Math.floor(event.first / event.rows) + 1;
  onQuery();
};

const changeStatus = (row: any) => {
  tenantUserSetStatusPost({id: row.id, isEnabled: row.isEnabled})
      .then(() => {
        showSuccess(t('system.user.accountStatusSuccess'));
      })
      .catch(() => {
        row.isEnabled = !row.isEnabled;
      });
};

const resetUserPwd = async (row: any) => {
  const confirmed = await showConfirm(`${t('system.user.resetPwdConfirm')}：【${row.account}】?`, t('common.message.tip'))
  if (!confirmed) return
  const res = await tenantUserResetPwdPost({id: row.id})
  showSuccess(`${t('system.user.resetPwdSuccess')}：${res.data.data?.newPassword}`);
};

const nodeClick = async (node: any) => {
  state.queryParams.orgId = node.id;
  state.queryParams.account = undefined;
  state.queryParams.realName = undefined;
  state.queryParams.phone = undefined;
  onQuery();
  // 移动端选完节点自动收起树（C252），把屏幕还给结果表
  if (isNarrowViewport.value) {
    isMobileTreeOpen.value = false;
  }
};
</script>

<style lang="scss" scoped>
.org-tree-sidebar {
  width: 280px;
  min-width: 280px;
  flex-shrink: 0;
}

// 移动端组织树侧栏降级（C252，GFX-658）：280px 定宽侧栏在 375px 手机上
// 吃掉约 75% 宽度、表格持续横拖；bp-down(md) 下容器改竖排、侧栏全宽、
// 树默认收起，由 .org-tree-toggle 按钮控制显隐（JS 侧 isMobileTreeOpen）。
.org-tree-toggle {
  display: none;
}

@include bp-down(md) {
  .sys-user-container {
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
