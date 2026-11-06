<template>
  <div class="sys-user-container flex gap-2 w-full h-full">
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
                <!-- PrimeVue Avatar 的默认插槽会抑制内部 image 分支（renderSlot fallback），
                     有头像时必须走纯 :image 形态，无头像才渲染首字插槽 -->
                <Avatar v-if="avatarImageUrl(data)" :image="avatarImageUrl(data)" size="normal" shape="circle"/>
                <Avatar v-else size="normal" shape="circle">
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

    <EditUser ref="editUserRef" :title="state.editUserTitle" @query="onQuery"/>
  </div>
</template>

<script lang="ts" setup name="systemUser">
import { faArrowsRotate, faEllipsisVertical, faMagnifyingGlass, faPenToSquare, faPlus } from '../../../utils/icons.ts';

import {computed, onMounted, reactive, ref} from 'vue';
import {showConfirm, showSuccess} from '../../../utils/message.ts';
import {auth} from '../../../utils/authFunction.ts';
import {resolveBackendFileUrl} from '../../../utils/fileUrl.ts';
import EditUser from '/@/views/system/user/component/EditUser.vue';

import {userDeletePost, userPagePost, userResetPwdPost, userSetStatusPost} from '../../../api/user';
import {UserEntity} from '../../../api/_shared/data-contracts.ts';
import {normalizeIsEnabled} from '../../../utils/isEnabled.ts';

import Card from 'primevue/card';
import InputText from 'primevue/inputtext';
import Button from 'primevue/button';
import Column from 'primevue/column';
import Tag from 'primevue/tag';
import Avatar from 'primevue/avatar';
import ToggleSwitch from 'primevue/toggleswitch';
import CrudDataTable from '/@/components/crud-data-table/index.vue';
import Menu from 'primevue/menu';

const editUserRef = ref<InstanceType<typeof EditUser>>();
const menuRef = ref();
const contextMenuRow = ref<any>(null);
type UserRow = UserEntity & Record<string, any>;

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
  queryParams: {
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
  onQuery();
});

const onQuery = async () => {
  state.loading = true;
  let params = Object.assign(state.queryParams, state.tableParams);
  var res = await userPagePost(params);
  state.userData = (res.data.data?.items ?? []).map((row) => normalizeIsEnabled(row) as UserRow);
  state.tableParams.total = res.data.data?.total;
  state.loading = false;
};

const resetQuery = () => {
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
  await userDeletePost({id: row.id});
  onQuery();
  showSuccess(t('common.message.deleteSuccess'));
};

const onPageChange = (event: any) => {
  state.tableParams.pageSize = event.rows;
  state.tableParams.page = Math.floor(event.first / event.rows) + 1;
  onQuery();
};

// 头像列 URL：DB 相对对象键 → 后端 origin 绝对地址（无头像返回空串，走首字插槽分支）
const avatarImageUrl = (row: UserRow) => resolveBackendFileUrl(row.avatar);

const changeStatus = (row: any) => {
  userSetStatusPost({id: row.id, isEnabled: row.isEnabled})
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
  const res = await userResetPwdPost({id: row.id})
  showSuccess(`${t('system.user.resetPwdSuccess')}：${res.data.data?.newPassword}`);
};

</script>
