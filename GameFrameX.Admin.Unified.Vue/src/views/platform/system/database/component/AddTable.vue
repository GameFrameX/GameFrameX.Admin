<template>
  <div class="sys-dbTable-container">
    <Dialog v-model:visible="state.dialogVisible" :draggable="true" :modal="true" :closable="true" :style="{width: 'min(1400px, calc(100vw - 32px))'}">
      <template #header>
        <div>
          <i class="fa-solid fa-pen-to-square" style="margin-right: 3px; font-size: 16px; vertical-align: middle"></i>
          <span> {{ t('system.database.addTable') }} </span>
        </div>
      </template>
      <Divider align="left">
        {{ t('system.database.tableInfo') }}
      </Divider>
      <form @submit.prevent ref="ruleFormRef">
        <div class="grid" style="gap: 0px">
          <div class="col-12 mb20">
            <div class="field mb20">
              <label>{{ t('system.database.tableDescription') }}</label>
              <InputText v-model="state.ruleForm.tableName" :placeholder="t('system.database.tableDescription')" class="w100"/>
            </div>
          </div>
          <div class="col-12 mb20">
            <div class="field mb20">
              <label>{{ t('common.label.description') }}</label>
              <Textarea v-model="state.ruleForm.description" :placeholder="t('system.database.columnDescription')" class="w100" rows="3"/>
            </div>
          </div>
        </div>
      </form>
      <Divider align="left">
        {{ t('system.database.columnInfo') }}
      </Divider>
      <CrudDataTable :value="state.tableData" style="width: 100%" :scrollable="true" scrollHeight="400px" :showPagination="false">
        <Column field="dbColumnName" :header="t('system.database.fieldName')" style="width: 220px">
          <template #body="{ data }">
            <InputText v-model="data.dbColumnName" class="w100"/>
          </template>
        </Column>
        <Column field="columnDescription" :header="t('common.label.description')" style="width: 220px">
          <template #body="{ data }">
            <InputText v-model="data.columnDescription" class="w100"/>
          </template>
        </Column>
        <Column field="isPrimarykey" :header="t('system.database.isPrimarykey')" style="width: 100px">
          <template #body="{ data }">
            <Select v-model="data.isPrimarykey" :options="yesNoOptions" filter optionLabel="label" optionValue="value" class="w100"/>
          </template>
        </Column>
        <Column field="isIdentity" :header="t('system.database.isIdentity')" style="width: 100px">
          <template #body="{ data }">
            <Select v-model="data.isIdentity" :options="yesNoOptions" filter optionLabel="label" optionValue="value" class="w100"/>
          </template>
        </Column>
        <Column field="dataType" :header="t('system.database.dataType')" style="width: 150px">
          <template #body="{ data }">
            <Select v-model="data.dataType" :options="dataTypeOptions" filter optionLabel="label" optionValue="value" class="w100"/>
          </template>
        </Column>
        <Column field="isNullable" :header="t('system.database.isNullable')" style="width: 100px">
          <template #body="{ data }">
            <Select v-model="data.isNullable" :options="yesNoOptions" filter optionLabel="label" optionValue="value" class="w100"/>
          </template>
        </Column>
        <Column field="length" :header="t('system.database.length')" style="width: 100px">
          <template #body="{ data }">
            <InputNumber v-model="data.length" class="w100"/>
          </template>
        </Column>
        <Column field="decimalDigits" :header="t('system.database.decimalDigits')" style="width: 100px">
          <template #body="{ data }">
            <InputNumber v-model="data.decimalDigits" class="w100"/>
          </template>
        </Column>
        <Column :header="t('common.table.action')" style="min-width: 200px" align="center">
          <template #body="{ data, index }">
            <Button link severity="danger" @click="onColDelete(index)">
              <i class="fa-solid fa-trash mr-1"></i>
              {{ t('common.button.delete') }}
            </Button>
            <Button v-if="state.tableData.length > 1" link @click="onColUp(data, index)">
              <i class="fa-solid fa-arrow-up mr-1"></i>
              {{ t('system.database.moveUp') }}
            </Button>
            <Button v-if="state.tableData.length > 1" link @click="onColDown(data, index)">
              <i class="fa-solid fa-arrow-down mr-1"></i>
              {{ t('system.database.moveDown') }}
            </Button>
          </template>
        </Column>
      </CrudDataTable>
      <div style="text-align: left; margin-top: 10px" class="flex gap-2">
        <Button severity="secondary" @click="addPrimaryColumn">
          <i class="fa-solid fa-plus mr-1"></i>
          {{ t('system.database.addPrimaryColumn') }}
        </Button>
        <Button severity="secondary" @click="addColumn">
          <i class="fa-solid fa-plus mr-1"></i>
          {{ t('system.database.addNormalColumn') }}
        </Button>
        <Button severity="secondary" @click="addTenantColumn">
          <i class="fa-solid fa-plus mr-1"></i>
          {{ t('system.database.addTenantColumn') }}
        </Button>
        <Button severity="secondary" @click="addBaseColumn">
          <i class="fa-solid fa-plus mr-1"></i>
          {{ t('system.database.addBaseColumn') }}
        </Button>
      </div>

      <template #footer>
        <Button severity="secondary" @click="cancel">{{ t('common.button.cancel') }}</Button>
        <Button @click="submit">{{ t('common.button.confirm') }}</Button>
      </template>
    </Dialog>
  </div>
</template>

<script lang="ts" setup name="systemAddTable">
import { faArrowDown, faArrowUp, faPlus, faTrash } from '../../../../../utils/icons.ts';

import {computed, reactive, ref} from 'vue';
import {showError} from '../../../../../utils/message.ts';

import CrudDataTable from '/@/components/crud-data-table/index.vue';
import {systemDatabaseAddTablePost} from '../../../../../api/systemDatabase';
import {UpdateDbTableInput} from '../../../../../api/_shared/data-contracts.ts';
import {DATA_TYPE_LIST, EditRecordRow, YES_NO_SELECT} from '../database';

const yesNoOptions = computed(() =>
    YES_NO_SELECT.map((item: any) => ({label: item.label, value: item.value}))
);

const dataTypeOptions = computed(() =>
    DATA_TYPE_LIST.map((item: any) => ({label: item.value, value: item.value}))
);

var colIndex = 0;
const emits = defineEmits(['addTableSubmitted']);
const ruleFormRef = ref();
const state = reactive({
  dialogVisible: false,
  ruleForm: {} as UpdateDbTableInput,
  tableData: [] as any,
});

// 打开弹窗
const openDialog = (row: any) => {
  state.ruleForm = row;
  state.dialogVisible = true;
};

// 关闭弹窗
const closeDialog = () => {
  emits('addTableSubmitted', state.ruleForm.tableName ?? '');
  state.tableData = [];
  state.dialogVisible = false;
};

// 取消
const cancel = () => {
  state.dialogVisible = false;
};

// 提交
const submit = () => {
  if (!state.ruleForm.tableName) return;
  if (!state.ruleForm.description) return;
  if (state.tableData.length === 0) {
    showError(t('system.database.addColumnRequired'));
    return;
  }
  (async () => {
    const params: any = {
      dbColumnInfoList: state.tableData,
      ...state.ruleForm,
    };
    await systemDatabaseAddTablePost(params);
    closeDialog();
  })();
};

// 增加主键列
function addPrimaryColumn() {
  state.tableData.push({
    columnDescription: t('system.database.primaryKeyId'),
    dataType: 'bigint',
    dbColumnName: 'Id',
    decimalDigits: 0,
    isIdentity: 0,
    isNullable: 0,
    isPrimarykey: 1,
    length: 0,
    key: colIndex,
    editable: true,
    isNew: true,
  });
  colIndex++;
}

// 增加普通列
function addColumn() {
  state.tableData.push({
    columnDescription: '',
    dataType: 'varchar',
    dbColumnName: '',
    decimalDigits: 0,
    isIdentity: 0,
    isNullable: 1,
    isPrimarykey: 0,
    length: 32,
    key: colIndex,
    editable: true,
    isNew: true,
  });
  colIndex++;
}

// 增加租户列
function addTenantColumn() {
  state.tableData.push({
    columnDescription: t('system.database.tenantId'),
    dataType: 'bigint',
    dbColumnName: 'TenantId',
    decimalDigits: 0,
    isIdentity: 0,
    isNullable: 1,
    isPrimarykey: 0,
    length: 0,
    key: colIndex,
    editable: true,
    isNew: true,
  });
  colIndex++;
}

// 增加通用基础列
function addBaseColumn() {
  const fileds = [
    {
      dataType: 'datetime',
      name: 'CreatedTime',
      desc: t('system.database.createTimeDesc'),
    },
    {
      dataType: 'datetime',
      name: 'UpdateTime',
      desc: t('system.database.updateTimeDesc'),
    },
    {
      dataType: 'bigint',
      name: 'CreatedId',
      desc: t('system.database.creatorIdDesc'),
    },
    {
      dataType: 'varchar',
      name: 'CreatedName',
      desc: t('system.database.creatorNameDesc'),
      length: 64,
    },
    {
      dataType: 'bigint',
      name: 'UpdatedId',
      desc: t('system.database.modifierIdDesc'),
    },
    {
      dataType: 'varchar',
      name: 'UpdatedName',
      desc: t('system.database.modifierNameDesc'),
      length: 64,
    },
    {
      dataType: 'bit',
      name: 'IsDeleted',
      desc: t('system.database.softDeleteDesc'),
      isNullable: 0,
    },
  ];

  fileds.forEach((m: any) => {
    state.tableData.push({
      columnDescription: m.desc,
      dataType: m.dataType,
      dbColumnName: m.name,
      decimalDigits: 0,
      isIdentity: 0,
      isNullable: m.isNullable === 0 ? 0 : 1,
      isPrimarykey: 0,
      length: m.length || 0,
      key: colIndex,
      editable: true,
      isNew: true,
    });
    colIndex++;
  });
}

function onColDelete(index: number) {
  state.tableData.splice(index, 1);
}

// 上移
function onColUp(record: EditRecordRow, index: number) {
  if (record.isNew) {
    var data1 = changeExForArray(index, index - 1, state.tableData);
    return data1;
  }
}

// 下移
function onColDown(record: EditRecordRow, index: number) {
  if (record.isNew) {
    return changeExForArray(index, index + 1, state.tableData);
  }
}

function changeExForArray(index1: number, index2: number, array: Array<EditRecordRow>) {
  let temp = array[index1];
  array[index1] = array[index2];
  array[index2] = temp;
  return array;
}

// 导出对象
defineExpose({openDialog});
</script>
