<template>
  <div class="sys-database-container">
    <Card>
      <template #content>
        <form @submit.prevent class="flex flex-wrap gap-2 align-items-end" v-block-loading="state.loading">
          <div class="field">
            <label>{{ t('system.database.dbName') }}</label>
            <Select v-model="state.configId" :options="dbDataOptions" optionLabel="label" optionValue="value"
                    :placeholder="t('system.database.dbName')" filter showClear class="w100" @change="onQueryTable"/>
          </div>
          <div class="field">
            <label>{{ t('system.database.tableName') }}</label>
            <Select v-model="state.tableName" :options="tableDataOptions" optionLabel="label" optionValue="value"
                    :placeholder="t('system.database.tableName')" filter showClear class="w100" @change="onQueryColumn"/>
          </div>
          <Button @click="openEditTable" v-auth="'systemDatabase:updatetable'">
            <i class="fa-solid fa-pen-to-square mr-1"></i>
            {{ t('system.database.editTable') }}
          </Button>
          <Button severity="danger" @click="delTable" v-auth="'systemDatabase:deletetable'">
            <i class="fa-solid fa-trash mr-1"></i>
            {{ t('system.database.deleteTableBtn') }}
          </Button>
          <Button severity="secondary" @click="openAddTable">
            <i class="fa-solid fa-plus mr-1"></i>
            {{ t('system.database.addTable') }}
          </Button>
          <Button severity="secondary" @click="openAddColumn">
            <i class="fa-solid fa-plus mr-1"></i>
            {{ t('system.database.addColumn') }}
          </Button>
        </form>
      </template>
    </Card>

    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="state.columnData" v-block-loading="state.loading1" showGridlines stripedRows style="width: 100%" :showPagination="false">
          <Column field="dbColumnName" :header="t('system.database.fieldName')" align="center"/>
          <Column field="dataType" :header="t('system.database.dataType')" align="center"/>
          <Column field="isPrimarykey" :header="t('system.database.isPrimarykey')" style="width: 70px" align="center">
            <template #body="{ data }">
              <Tag v-if="data.isPrimarykey === true" severity="success" :value="t('common.label.yes')"/>
              <Tag v-else severity="info" :value="t('common.label.no')"/>
            </template>
          </Column>
          <Column field="isIdentity" :header="t('system.database.isIdentity')" style="width: 70px" align="center">
            <template #body="{ data }">
              <Tag v-if="data.isIdentity === true" severity="success" :value="t('common.label.yes')"/>
              <Tag v-else severity="info" :value="t('common.label.no')"/>
            </template>
          </Column>
          <Column field="isNullable" :header="t('system.database.isNullable')" style="width: 70px" align="center">
            <template #body="{ data }">
              <Tag v-if="data.isNullable === true" :value="t('common.label.yes')"/>
              <Tag v-else severity="info" :value="t('common.label.no')"/>
            </template>
          </Column>
          <Column field="length" :header="t('system.database.length')" style="width: 70px" align="center"/>
          <Column field="decimalDigits" :header="t('system.database.precision')" style="width: 70px" align="center"/>
          <Column field="defaultValue" :header="t('system.database.defaultValue')" align="center"/>
          <Column field="columnDescription" :header="t('common.label.description')"/>
          <Column :header="t('common.table.action')" style="width: 140px" align="center">
            <template #body="{ data }">
              <Button link @click="openEditColumn(data)">
                <i class="fa-solid fa-pen-to-square mr-1"></i>
                {{ t('common.button.edit') }}
              </Button>
              <Button link severity="danger" @click="delColumn(data)">
                <i class="fa-solid fa-trash mr-1"></i>
                {{ t('common.button.delete') }}
              </Button>
            </template>
          </Column>
        </CrudDataTable>
      </template>
    </Card>

    <EditTable ref="editTableRef" @queryTable="onQueryTable"/>
    <EditColumn ref="editColumnRef" @queryColumn="onQueryColumn"/>
    <AddTable ref="addTableRef" @addTableSubmitted="addTableSubmitted"/>
    <AddColumn ref="addColumnRef" @queryColumn="onQueryColumn"/>
  </div>
</template>

<script lang="ts" setup name="systemDatabase">
import { faPenToSquare, faPlus, faTrash } from '../../../../utils/icons.ts';

import {computed, onMounted, reactive, ref} from 'vue';
import {showConfirm, showSuccess, showError} from '../../../../utils/message.ts';
import EditTable from '/@/views/platform/system/database/component/EditTable.vue';
import EditColumn from '/@/views/platform/system/database/component/EditColumn.vue';
import AddTable from '/@/views/platform/system/database/component/AddTable.vue';
import AddColumn from '/@/views/platform/system/database/component/AddColumn.vue';

import CrudDataTable from '/@/components/crud-data-table/index.vue';
import {
  systemDatabaseListGet,
  systemDatabaseTableListGet,
  systemDatabaseColumnListGet,
  systemDatabaseDeleteTablePost,
  systemDatabaseDeleteColumnPost,
} from '../../../../api/systemDatabase';
import {
  DbColumnOutput,
  DbTableInfo,
  DbColumnInput,
  DeleteDbTableInput,
  DeleteDbColumnInput
} from '../../../../api/_shared/data-contracts.ts';

const editTableRef = ref<InstanceType<typeof EditTable>>();
const editColumnRef = ref<InstanceType<typeof EditColumn>>();
const addTableRef = ref<InstanceType<typeof AddTable>>();
const addColumnRef = ref<InstanceType<typeof AddColumn>>();
const state = reactive({
  loading: false,
  loading1: false,
  dbData: [] as any,
  configId: '',
  tableData: [] as Array<DbTableInfo>,
  tableName: '',
  columnData: [] as Array<DbColumnOutput>,
  queryParams: {
    name: undefined,
    code: undefined,
  },
  editPosTitle: '',
});

const dbDataOptions = computed(() =>
    state.dbData.map((item: string) => ({label: item, value: item}))
);

const tableDataOptions = computed(() =>
    state.tableData.map((item: DbTableInfo) => ({
      label: item.name + '[' + item.description + ']',
      value: item.name,
    }))
);

onMounted(async () => {
  state.loading = true;
  var res = await systemDatabaseListGet();
  state.dbData = res.data.data;
  state.loading = false;
});

// 增加表
const addTableSubmitted = (e: any) => {
  onQueryTable();
  state.tableName = e;
  onQueryColumn();
};

// 表查询操作
const onQueryTable = async () => {
  state.tableName = '';
  state.columnData = [];
  state.loading = true;

  var res = await systemDatabaseTableListGet(state.configId);
  state.tableData = res.data.data ?? [];
  state.loading = false;
};

// 列查询操作
const onQueryColumn = async () => {
  state.columnData = [];
  if (state.tableName == '') return;

  state.loading1 = true;
  var res = await systemDatabaseColumnListGet({tableName: state.tableName, configId: state.configId});
  state.columnData = res.data.data ?? [];
  state.loading1 = false;
};

// 打开表编辑页面
const openEditTable = () => {
  if (state.configId == '' || state.tableName == '') return;

  var res = state.tableData.filter((u: any) => u.name == state.tableName);
  var table: any = {
    configId: state.configId,
    tableName: state.tableName,
    oldTableName: state.tableName,
    description: res[0].description,
  };
  editTableRef.value?.openDialog(table);
};

// 打开表增加页面
const openAddTable = () => {
  if (state.configId == '') {
    showError(t('system.database.selectDbName'));
    return;
  }

  var table: any = {
    configId: state.configId,
    tableName: '',
    oldTableName: '',
    description: '',
  };
  addTableRef.value?.openDialog(table);
};

// 打开列编辑页面
const openEditColumn = (row: any) => {
  var column: any = {
    configId: state.configId,
    tableName: row.tableName,
    columnName: row.dbColumnName,
    oldColumnName: row.dbColumnName,
    description: row.columnDescription,
  };
  editColumnRef.value?.openDialog(column);
};

// 打开列增加页面
const openAddColumn = () => {
  if (state.configId == '' || state.tableName == '') {
    showError(t('system.database.selectDbAndTable'));
    return;
  }
  const addRow: DbColumnInput = {
    configId: state.configId,
    tableName: state.tableName,
    columnDescription: '',
    dataType: '',
    dbColumnName: '',
    decimalDigits: 0,
    isIdentity: 0,
    isNullable: 0,
    isPrimarykey: 0,
    length: 0,
    // key: 0,
    // editable: true,
    // isNew: true,
  };
  addColumnRef.value?.openDialog(addRow);
};

// 删除表
const delTable = async () => {
  const confirmed = await showConfirm(`${t('system.database.deleteTableConfirm')}：【${state.tableName}】?`, t('common.message.tip'), {type: 'danger'})
  if (!confirmed) return
  const deleteDbTableInput: DeleteDbTableInput = {
    configId: state.configId,
    tableName: state.tableName,
  };
  await systemDatabaseDeleteTablePost(deleteDbTableInput);
  onQueryTable();
  showSuccess(t('system.database.tableDeleteSuccess'));
};

// 删除列
const delColumn = async (row: any) => {
  const confirmed = await showConfirm(`${t('system.database.deleteColumnConfirm')}：【${row.dbColumnName}】?`, t('common.message.tip'), {type: 'danger'})
  if (!confirmed) return
  const eleteDbColumnInput: DeleteDbColumnInput = {
    configId: state.configId,
    tableName: state.tableName,
    dbColumnName: row.dbColumnName,
  };
  await systemDatabaseDeleteColumnPost(eleteDbColumnInput);
  onQueryTable();
  showSuccess(t('system.database.columnDeleteSuccess'));
};
</script>
