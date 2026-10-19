<template>
  <div class="sys-dictData-container">
    <Dialog v-model:visible="state.dialogVisible" draggable modal :style="{ width: 'min(700px, calc(100vw - 32px))' }"
            :header="props.title">
      <form @submit.prevent>
        <div class="flex flex-column gap-4">
          <div class="grid grid-cols-2" style="gap: 16px;">
            <div class="field">
              <label>{{ t('system.dict.dictValue') }} <span class="text-red-500">*</span></label>
              <InputText v-model="state.ruleForm.value" :placeholder="t('system.dict.dictValue')" class="w-full"/>
            </div>
            <div class="field">
              <label>{{ t('common.label.code') }} <span class="text-red-500">*</span></label>
              <InputText v-model="state.ruleForm.code" :placeholder="t('common.label.code')" class="w-full"/>
            </div>
          </div>
          <div class="field">
            <label>{{ t('system.dict.tagType') }}</label>
            <div class="flex gap-4 mt-1 flex-wrap">
              <div v-for="opt in tagTypeOptions" :key="opt.value" class="flex align-items-center gap-2">
                <RadioButton v-model="state.ruleForm.tagType" :value="opt.value"/>
                <Tag :severity="opt.severity" :value="opt.label"/>
              </div>
            </div>
          </div>
          <div class="field">
            <label>{{ t('message.system.dict.styleSetting') }}</label>
            <InputText v-model="state.ruleForm.styleSetting" placeholder="样式(Style)" class="w-full"/>
          </div>
          <div class="field">
            <label>{{ t('message.system.dict.classSetting') }}</label>
            <InputText v-model="state.ruleForm.classSetting" placeholder="样式(Class)" class="w-full"/>
          </div>
          <div class="grid grid-cols-2" style="gap: 16px;">
            <div class="field">
              <label>{{ t('common.label.status') }}</label>
              <div class="mt-1"><ToggleSwitch v-model="state.ruleForm.isEnabled"/></div>
            </div>
            <div class="field">
              <label>{{ t('common.label.sort') }}</label>
              <InputNumber v-model="state.ruleForm.orderNo" :placeholder="t('common.label.sort')" class="w-full"/>
            </div>
          </div>
          <div class="field">
            <label>{{ t('common.label.remark') }}</label>
            <Textarea v-model="state.ruleForm.remark" :placeholder="t('common.form.pleaseInput')" class="w-full"
                      :autoResize="true" rows="3"/>
          </div>
          <div class="field">
            <label>{{ t('system.dict.extData') }}</label>
            <Textarea v-model="state.ruleForm.extData" :placeholder="t('system.dict.extData')" class="w-full"
                      :autoResize="true" rows="4"/>
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

<script lang="ts" setup name="systemEditDictData">

import {reactive} from 'vue';

import {globalDictionaryDataUpdatePost, globalDictionaryDataAddPost} from '../../../../../api/globalDictionaryData';
import {UpdateDictDataInput} from '../../../../../api/_shared/data-contracts.ts';
import {isEnabled} from '../../../../../utils/isEnabled.ts';

const tagTypeOptions = [
  {label: t('system.dict.themeColor'), value: 'default', severity: undefined},
  {label: 'success', value: 'success', severity: 'success'},
  {label: 'info', value: 'info', severity: 'info'},
  {label: 'warning', value: 'warning', severity: 'warn'},
  {label: 'danger', value: 'danger', severity: 'danger'},
];

const props = defineProps({
  title: String,
  dictTypeId: Number,
});
const emits = defineEmits(['query']);
const state = reactive({
  dialogVisible: false,
  ruleForm: {} as UpdateDictDataInput,
});

const openDialog = (row: any) => {
  state.ruleForm = JSON.parse(JSON.stringify(row));
  state.ruleForm.isEnabled = isEnabled(state.ruleForm);
  if (JSON.stringify(row) == '{}') {
    state.ruleForm.dictTypeId = props.dictTypeId;
  }
  state.dialogVisible = true;
};

const closeDialog = () => {
  emits('query');
  state.dialogVisible = false;
};

const cancel = () => {
  state.dialogVisible = false;
};

const submit = async () => {
  if (state.ruleForm.id != undefined && state.ruleForm.id > 0) {
    await globalDictionaryDataUpdatePost(state.ruleForm);
  } else {
    await globalDictionaryDataAddPost(state.ruleForm);
  }
  closeDialog();
};

defineExpose({openDialog});
</script>
