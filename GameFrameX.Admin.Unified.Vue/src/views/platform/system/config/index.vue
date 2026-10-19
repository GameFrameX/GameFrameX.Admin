<template>
  <div class="sys-config-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="searchParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="systemConfig"
            :defaultSearchKey="false"
            @query="onSearch"
            @reset="onReset"
            @add="openAddConfig"
        >
          <template #buttons>
            <Button v-if="state.selectlist.length > 0" severity="danger" @click="bacthDelete"
                    v-auth="'systemConfig:batchDelete'">
              <i class="fa-solid fa-trash mr-1"></i>
              {{ t('system.config.batchDelete') }}
            </Button>
          </template>
        </SearchForm>
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable
            v-model:selection="state.selectlist"
            :value="tableData"
            v-block-loading="loading"
            showGridlines
            stripedRows
            rowKey="id"
            :totalRecords="tableParams.total"
            :rows="tableParams.pageSize"
            :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
            authPrefix="systemConfig"
            :actions="actions"
            :columns="columns"
            :exportQuery="queryParams"
            showIdColumn
            :idColumnHeader="t('common.label.id')"
            idColumnWidth="80px"
            @page="onPage"
        >
          <template #extra-columns>
            <Column v-if="canBatchDelete" selectionMode="multiple" :headerStyle="{ width: 'min(42px, calc(100vw - 32px))' }"/>
          </template>
          <template #body-sysFlag="{ data }">
            <Tag v-if="data.sysFlag === 1" :value="t('common.label.yes')"/>
            <Tag v-else severity="danger" :value="t('common.label.no')"/>
          </template>
        </CrudDataTable>
      </template>
    </Card>

    <EditConfig ref="editConfigRef" :title="state.editConfigTitle" :groupList="state.groupList"
                @updateData="updateData"/>
  </div>
</template>

<script lang="ts" setup name="systemConfig">
import {onMounted, reactive, ref, computed} from 'vue';
import {showConfirm, showSuccess} from '../../../../utils/message.ts';
import EditConfig from '/@/views/platform/system/config/component/EditConfig.vue';

import {
  systemConfigPagePost,
  systemConfigGroupListGet,
  systemConfigDeletePost,
  systemConfigBatchDeletePost,
} from '../../../../api/systemConfig';
import {auth} from '../../../../utils/authFunction.ts';
import SearchForm from '/@/components/search-form/index.vue';
import CrudDataTable from '/@/components/crud-data-table/index.vue';
import {useCrudTable} from '../../../../composables/useCrudTable.ts';
import type {CrudColumnConfig} from '../../../../components/crud-data-table/types.ts';

const editConfigRef = ref<InstanceType<typeof EditConfig>>();
const searchParams = ref({
  name: undefined as string | undefined,
  code: undefined as string | undefined,
  groupCode: undefined as string | undefined,
});

const state = reactive({
  editConfigTitle: '',
  selectlist: [] as EmptyObjectType[],
  groupList: [] as Array<String>,
});

const canBatchDelete = computed(() => auth('systemConfig:batchDelete'));

const searchFields = computed(() => [
  {
    key: 'name',
    label: t('system.config.configName'),
    type: 'input',
    placeholder: t('system.config.searchConfigName'),
  },
  {
    key: 'code',
    label: t('system.config.configCode'),
    type: 'input',
    placeholder: t('system.config.searchConfigCode'),
  },
  {
    key: 'groupCode',
    label: t('system.config.groupCode'),
    type: 'select',
    placeholder: t('common.form.pleaseSelect'),
    options: state.groupList.map((item) => ({label: item, value: item})),
  },
]);

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
} = useCrudTable({
  api: {
    pageApi: (params) => systemConfigPagePost(params),
  },
  immediate: true,
});

queryParams.value = {field: 'orderNo', order: 'ascending'};
tableParams.value.pageSize = 10;

const onSearch = () => {
  queryParams.value = {...queryParams.value, ...searchParams.value};
  tableParams.value.page = 1;
  state.selectlist = [];
  onQuery();
};

const onReset = () => {
  searchParams.value = {
    name: undefined,
    code: undefined,
    groupCode: undefined,
  };
  queryParams.value = {field: 'orderNo', order: 'ascending'};
  tableParams.value.page = 1;
  state.selectlist = [];
  onQuery();
};

const onPage = (e: any) => {
  state.selectlist = [];
  tableParams.value.pageSize = e.rows;
  tableParams.value.page = e.page + 1;
  onQuery();
};

const getGroupList = async () => {
  const res = await systemConfigGroupListGet();
  state.groupList = res.data.data ?? [];
};

onMounted(async () => {
  getGroupList();
});

const updateData = () => {
  onQuery();
  getGroupList();
};

const openAddConfig = () => {
  state.editConfigTitle = t('system.config.addConfig');
  editConfigRef.value?.openDialog({sysFlag: 2, orderNo: 100});
};

const openEditConfig = (row: any) => {
  state.editConfigTitle = t('system.config.editConfig');
  editConfigRef.value?.openDialog(row);
};

const delConfig = async (row: any) => {
  const confirmed = await showConfirm(`${t('system.config.deleteConfigConfirm')}：【${row.name}】?`, t('common.message.tip'), {type: 'danger'})
  if (!confirmed) return
  await systemConfigDeletePost({id: row.id});
  onQuery();
  showSuccess(t('common.message.deleteSuccess'));
};

const bacthDelete = async () => {
  if (state.selectlist.length == 0) return false;
  const confirmed = await showConfirm(t('system.config.batchDeleteConfirm'), t('common.message.tip'), {type: 'danger'})
  if (!confirmed) return
  const ids = state.selectlist.map((item) => {
    return item.id;
  });
  var res = await systemConfigBatchDeletePost(ids);
  state.selectlist = [];
  tableParams.value.page = 1;
  onQuery();
  showSuccess(t('common.message.deleteSuccess'));
};

const actions = {
  edit: openEditConfig,
  delete: delConfig,
};

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'name', header: t('system.config.configName'), width: '200px', sortable: true},
  {field: 'code', header: t('system.config.configCode'), width: '200px', sortable: true},
  {field: 'value', header: t('system.config.configValue'), width: '150px'},
  {field: 'sysFlag', header: t('system.config.builtInParam'), width: '100px', align: 'center', bodyKey: 'sysFlag'},
  {field: 'groupCode', header: t('system.config.groupCode'), width: '120px', align: 'center', sortable: true},
  {field: 'orderNo', header: t('common.label.sort'), width: '80px', align: 'center', sortable: true},
  {field: 'remark', header: t('common.label.remark'), width: '180px'},
]);
</script>
