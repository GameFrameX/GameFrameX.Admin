<template>
  <div class="sys-dbColumn-container">
    <Dialog v-model:visible="state.dialogVisible" :draggable="true" :modal="true" :closable="true" :style="{width: 'min(700px, calc(100vw - 32px))'}">
      <template #header>
        <div>
          <i class="fa-solid fa-pen-to-square" style="margin-right: 3px; font-size: 16px; vertical-align: middle"></i>
          <span> {{ t('system.database.addColumn') }} </span>
        </div>
      </template>
      <form @submit.prevent ref="ruleFormRef">
        <div class="grid" style="gap: 0px">
          <div class="col-12 mb20">
            <div class="field mb20">
              <label>{{ t('system.database.columnName') }}</label>
              <InputText v-model="state.ruleForm.dbColumnName" :placeholder="t('system.database.columnName')" class="w100"/>
            </div>
          </div>
          <div class="col-12 mb20">
            <div class="field mb20">
              <label>{{ t('common.label.description') }}</label>
              <Textarea v-model="state.ruleForm.columnDescription" :placeholder="t('system.database.columnDescription')" class="w100" rows="3"/>
            </div>
          </div>
          <div class="col-6 mb20">
            <div class="field mb20">
              <label>{{ t('system.database.isPrimarykey') }}</label>
              <Select v-model="state.ruleForm.isPrimarykey" :options="yesNoOptions" filter optionLabel="label" optionValue="value" class="w100"/>
            </div>
          </div>
          <div class="col-6 mb20">
            <div class="field mb20">
              <label>{{ t('system.database.isIdentity') }}</label>
              <Select v-model="state.ruleForm.isIdentity" :options="yesNoOptions" filter optionLabel="label" optionValue="value" class="w100"/>
            </div>
          </div>
          <div class="col-6 mb20">
            <div class="field mb20">
              <label>{{ t('system.database.dataType') }}</label>
              <Select v-model="state.ruleForm.dataType" :options="dataTypeOptions" filter optionLabel="label" optionValue="value" class="w100"/>
            </div>
          </div>
          <div class="col-6 mb20">
            <div class="field mb20">
              <label>{{ t('system.database.isNullable') }}</label>
              <Select v-model="state.ruleForm.isNullable" :options="yesNoOptions" filter optionLabel="label" optionValue="value" class="w100"/>
            </div>
          </div>
          <div class="col-6 mb20">
            <div class="field mb20">
              <label>{{ t('system.database.length') }}</label>
              <InputNumber v-model="state.ruleForm.length" class="w100"/>
            </div>
          </div>
          <div class="col-6 mb20">
            <div class="field mb20">
              <label>{{ t('system.database.decimalDigits') }}</label>
              <InputNumber v-model="state.ruleForm.decimalDigits" class="w100"/>
            </div>
          </div>
        </div>
      </form>
      <template #footer>
        <Button severity="secondary" @click="cancel">{{ t('common.button.cancel') }}</Button>
        <Button @click="submit">{{ t('common.button.confirm') }}</Button>
      </template>
    </Dialog>
  </div>
</template>

<script lang="ts" setup name="systemAddColumn">

import {computed, reactive, ref} from 'vue';

import {systemDatabaseAddColumnPost} from '../../../../../api/systemDatabase';
import {DbColumnInput} from '../../../../../api/_shared/data-contracts.ts';
import {DATA_TYPE_LIST, YES_NO_SELECT} from '../database';

const yesNoOptions = computed(() =>
    YES_NO_SELECT.map((item: any) => ({label: item.label, value: item.value}))
);

const dataTypeOptions = computed(() =>
    DATA_TYPE_LIST.map((item: any) => ({label: item.value, value: item.value}))
);

const emits = defineEmits(['queryColumn']);
const ruleFormRef = ref();
const state = reactive({
  dialogVisible: false,
  ruleForm: {} as DbColumnInput,
});

// 打开弹窗
const openDialog = (addRow: DbColumnInput) => {
  state.ruleForm = addRow;
  state.dialogVisible = true;
};

// 关闭弹窗
const closeDialog = () => {
  emits('queryColumn');
  state.dialogVisible = false;
};

// 取消
const cancel = () => {
  state.dialogVisible = false;
};

// 提交
const submit = () => {
  if (!state.ruleForm.dbColumnName) return;
  if (!state.ruleForm.columnDescription) return;
  (async () => {
    await systemDatabaseAddColumnPost(state.ruleForm);
    closeDialog();
  })();
};

// 导出对象
defineExpose({openDialog});
</script>
