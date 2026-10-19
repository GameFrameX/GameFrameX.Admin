<template>
  <div class="sys-dbColumn-container">
    <Dialog v-model:visible="state.dialogVisible" :draggable="true" :modal="true" :closable="true" :style="{width: 'min(700px, calc(100vw - 32px))'}">
      <template #header>
        <div>
          <i class="fa-solid fa-pen-to-square" style="margin-right: 3px; font-size: 16px; vertical-align: middle"></i>
          <span> {{ t('system.database.editColumn') }} </span>
        </div>
      </template>
      <form @submit.prevent ref="ruleFormRef">
        <div class="grid" style="gap: 0px">
          <div class="col-12 mb20">
            <div class="field mb20">
              <label>{{ t('system.database.columnName') }}</label>
              <InputText v-model="state.ruleForm.columnName" :placeholder="t('system.database.columnName')" class="w100"/>
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
      <template #footer>
        <Button severity="secondary" @click="cancel">{{ t('common.button.cancel') }}</Button>
        <Button @click="submit">{{ t('common.button.confirm') }}</Button>
      </template>
    </Dialog>
  </div>
</template>

<script lang="ts" setup name="systemEditColumn">

import {reactive, ref} from 'vue';

import {systemDatabaseUpdateColumnPost} from '../../../../../api/systemDatabase';
import {UpdateDbColumnInput} from '../../../../../api/_shared/data-contracts.ts';

const emits = defineEmits(['queryColumn']);
const ruleFormRef = ref();
const state = reactive({
  dialogVisible: false,
  ruleForm: {} as UpdateDbColumnInput,
});

// 打开弹窗
const openDialog = (row: any) => {
  state.ruleForm = JSON.parse(JSON.stringify(row));
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
  if (!state.ruleForm.columnName) return;
  if (!state.ruleForm.description) return;
  (async () => {
    await systemDatabaseUpdateColumnPost(state.ruleForm);
    closeDialog();
  })();
};

// 导出对象
defineExpose({openDialog});
</script>
