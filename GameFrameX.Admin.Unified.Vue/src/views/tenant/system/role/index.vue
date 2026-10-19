<template>
  <div class="sys-role-container">
    <Card>
      <template #content>
        <div class="flex flex-wrap gap-3 align-items-end">
          <div class="flex flex-column gap-1">
            <label>{{ t('system.role.roleName') }}</label>
            <InputText v-model="state.queryParams.name" :placeholder="t('system.role.roleName')" />
          </div>
          <div class="flex flex-column gap-1">
            <label>{{ t('system.role.roleCode') }}</label>
            <InputText v-model="state.queryParams.code" :placeholder="t('system.role.roleCode')" />
          </div>
          <div class="flex gap-2">
            <Button v-auth="'systemRole:page'" @click="onQuery">
              <i :class="faMagnifyingGlass" class="mr-1"></i>
              {{ t('common.button.search') }}
            </Button>
            <Button severity="secondary" @click="resetQuery">
              <i :class="faArrowsRotate" class="mr-1"></i>
              {{ t('common.button.reset') }}
            </Button>
          </div>
          <div>
            <Button v-auth="'systemRole:add'" @click="openAddRole">
              <i :class="faPlus" class="mr-1"></i>
              {{ t('common.button.add') }}
            </Button>
          </div>
        </div>
      </template>
    </Card>

    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="state.roleData" style="width: 100%" v-block-loading="state.loading" showGridlines
                       :totalRecords="state.tableParams.total"
                       :rows="state.tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100]"
                       :first="(state.tableParams.page - 1) * state.tableParams.pageSize"
                       @page="onPageChange">
          <Column field="name" :header="t('system.role.roleName')" />
          <Column field="code" :header="t('system.role.roleCode')" />
          <Column :header="t('system.role.dataScope')">
            <template #body="{ data }">
              <Tag v-if="data.dataScope === 1">{{ t('system.role.allData') }}</Tag>
              <Tag v-else-if="data.dataScope === 2">{{ t('system.role.deptAndBelowData') }}</Tag>
              <Tag v-else-if="data.dataScope === 3">{{ t('system.role.deptData') }}</Tag>
              <Tag v-else-if="data.dataScope === 4">{{ t('system.role.selfData') }}</Tag>
              <Tag v-else-if="data.dataScope === 5">{{ t('system.role.customData') }}</Tag>
            </template>
          </Column>
          <Column field="orderNo" :header="t('common.label.sort')" style="width: 70px" />
          <Column :header="t('common.label.status')" style="width: 70px">
            <template #body="{ data }">
              <Tag v-if="isEnabled(data)" severity="success">{{ t('common.label.enable') }}</Tag>
              <Tag v-else severity="danger">{{ t('common.label.disable') }}</Tag>
            </template>
          </Column>
          <TimeColumn field="createdTime" :header="t('common.label.createTime')" />
          <Column field="remark" :header="t('common.label.remark')" />
          <Column :header="t('common.table.action')" style="width: 110px">
            <template #body="{ data }">
              <Button size="small" link v-auth="'systemRole:update'" @click="openEditRole(data)">
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

    <Menu ref="menuRef" :model="contextMenuItems" :popup="true" />

    <EditRole ref="editRoleRef" :title="state.editRoleTitle" @query="onQuery"/>
    <GrantData ref="grantDataRef" @query="onQuery"/>
  </div>
</template>

<script lang="ts" setup name="systemRole">
import { faArrowsRotate, faEllipsisVertical, faMagnifyingGlass, faPenToSquare, faPlus } from '../../../../utils/icons.ts';

import {computed, onMounted, reactive, ref} from 'vue';
import {showConfirm, showSuccess} from '../../../../utils/message.ts';
import {auth} from '../../../../utils/authFunction.ts';
import EditRole from '/@/views/tenant/system/role/component/EditRole.vue';
import GrantData from '/@/views/tenant/system/role/component/GrantData.vue';

import {tenantRolePagePost, tenantRoleDeletePost} from '../../../../api/tenantRole';
import {TenantRoleEntity} from '../../../../api/_shared/data-contracts.ts';
import {isEnabled} from '../../../../utils/isEnabled.ts';

import Card from 'primevue/card';
import InputText from 'primevue/inputtext';
import Button from 'primevue/button';
import Column from 'primevue/column';
import Tag from 'primevue/tag';
import CrudDataTable from '/@/components/crud-data-table/index.vue';
import Menu from 'primevue/menu';

const editRoleRef = ref<InstanceType<typeof EditRole>>();
const grantDataRef = ref<InstanceType<typeof GrantData>>();
const menuRef = ref();
const contextMenuRow = ref<any>(null);

const contextMenuItems = computed(() => [
  {
    label: t('system.role.grantDataScope'),
    icon: 'fa-solid fa-building',
    command: () => {
      if (contextMenuRow.value) openGrantData(contextMenuRow.value);
    },
    disabled: !auth('systemRole:grantDataScope'),
  },
  {
    label: t('common.button.delete') + t('system.role.roleName'),
    icon: 'fa-solid fa-trash',
    command: () => {
      if (contextMenuRow.value) delRole(contextMenuRow.value);
    },
    disabled: !auth('systemRole:delete'),
  },
]);

const toggleMenu = (event: Event, row: any) => {
  contextMenuRow.value = row;
  menuRef.value.toggle(event);
};

const state = reactive({
  loading: false,
  roleData: [] as Array<TenantRoleEntity>,
  queryParams: {
    name: undefined,
    code: undefined,
  },
  tableParams: {
    page: 1,
    pageSize: 10,
    total: 0 as any,
  },
  editRoleTitle: '',
});

onMounted(async () => {
  onQuery();
});

const onQuery = async () => {
  state.loading = true;
  let params = Object.assign(state.queryParams, state.tableParams);
  var res = await tenantRolePagePost(params);
  state.roleData = res.data.data?.items ?? [];
  state.tableParams.total = res.data.data?.total;
  state.loading = false;
};

const resetQuery = () => {
  state.queryParams.name = undefined;
  state.queryParams.code = undefined;
  onQuery();
};

const openAddRole = () => {
  state.editRoleTitle = t('system.role.addRole');
  editRoleRef.value?.openDialog({id: undefined, isEnabled: true, orderNo: 100});
};

const openEditRole = async (row: any) => {
  state.editRoleTitle = t('system.role.editRole');
  editRoleRef.value?.openDialog(row);
};

const openGrantData = (row: any) => {
  grantDataRef.value?.openDialog(row);
};

const delRole = async (row: any) => {
  const confirmed = await showConfirm(`${t('system.role.deleteRoleConfirm')}：【${row.name}】?`, t('common.message.tip'), {type: 'danger'})
  if (!confirmed) return
  await tenantRoleDeletePost({id: row.id});
  onQuery();
  showSuccess(t('common.message.deleteSuccess'));
};

const onPageChange = (event: any) => {
  state.tableParams.pageSize = event.rows;
  state.tableParams.page = Math.floor(event.first / event.rows) + 1;
  onQuery();
};
</script>
