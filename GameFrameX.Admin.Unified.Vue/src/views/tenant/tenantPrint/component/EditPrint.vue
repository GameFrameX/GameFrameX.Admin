<template>
  <div class="sys-print-container">
    <div class="printDialog">
      <Dialog v-model:visible="state.dialogVisible" :draggable="true" :modal="true" :closable="true" :maximizable="true" :style="{width: '100vw', height: '100vh'}">
        <template #header>
          <div>
            <i class="fa-solid fa-pen-to-square" style="margin-right: 3px; font-size: 16px; vertical-align: middle"></i>
            <span> {{ props.title }} </span>
          </div>
        </template>
        <div style="margin: -16px 0px 0px 0px">
          <HiprintDesign ref="hiprintDesignRef"/>
        </div>
        <template #footer>
          <div class="dialog-footer" style="margin-top: 10px">
            <Button severity="secondary" @click="cancel">{{ t('common.button.cancel') }}</Button>
            <Button @click="submit">{{ t('common.button.save') }}</Button>
          </div>
        </template>
      </Dialog>
    </div>

    <Dialog v-model:visible="state.showDialog2" :draggable="true" :modal="true" :closable="true" :style="{width: 'min(600px, calc(100vw - 32px))'}">
      <template #header>
        <div>
          <i class="fa-solid fa-pen-to-square" style="margin-right: 3px; font-size: 16px; vertical-align: middle"></i>
          <span>{{ props.title }}</span>
        </div>
      </template>
      <form @submit.prevent ref="ruleFormRef">
        <div class="grid" style="gap: 0px">
          <div class="col-12 mb20">
            <div class="field mb20">
              <label>{{ t('system.print.printName') }}</label>
              <InputText v-model="state.ruleForm.name" :placeholder="t('system.print.printName')" class="w100"/>
            </div>
          </div>
          <div class="col-6 mb20">
            <div class="field mb20">
              <label>{{ t('common.label.sort') }}</label>
              <InputNumber v-model="state.ruleForm.orderNo" :placeholder="t('common.label.sort')" class="w100"/>
            </div>
          </div>
          <div class="col-6 mb20">
            <div class="field mb20">
              <label>{{ t('common.label.status') }}</label>
              <div class="mt-1"><ToggleSwitch v-model="state.ruleForm.isEnabled"/></div>
            </div>
          </div>
          <div class="col-12 mb20">
            <div class="field mb20">
              <label>{{ t('common.label.remark') }}</label>
              <Textarea v-model="state.ruleForm.remark" :placeholder="t('common.form.pleaseInput')" class="w100" rows="3"/>
            </div>
          </div>
        </div>
      </form>
      <template #footer>
        <Button severity="secondary" @click="templateCancel">{{ t('common.button.cancel') }}</Button>
        <Button @click="templateSubmit">{{ t('common.button.confirm') }}</Button>
      </template>
    </Dialog>
  </div>
</template>

<script lang="ts" setup name="systemEditPrint">

import {onMounted, reactive, ref, nextTick} from 'vue';
import HiprintDesign from '/@/views/tenant/tenantPrint/component/hiprint/index.vue';

import {tenantPrintUpdatePost, tenantPrintAddPost} from '../../../../api/tenantPrint';
import {UpdatePrintInput} from '../../../../api/_shared/data-contracts';
import {isEnabled} from '../../../../utils/isEnabled';
import {readRowVersion} from '../../../../utils/rowVersion';

const hiprintDesignRef = ref<InstanceType<typeof HiprintDesign>>();

const props = defineProps({
  title: String,
});
const emits = defineEmits(['query']);
const ruleFormRef = ref();
const state = reactive({
  dialogVisible: false,
  ruleForm: {} as UpdatePrintInput,
  showDialog2: false,
});

onMounted(async () => {
});

const openDialog = (row: any) => {
  state.ruleForm = JSON.parse(JSON.stringify(row));
  state.dialogVisible = true;
  ruleFormRef.value?.resetFields?.();

  nextTick(() => {
    loadTemplate();
  });
};

const loadTemplate = () => {
  hiprintDesignRef.value?.hiprintTemplate.clear();
  if (JSON.stringify(state.ruleForm) !== '{}') {
    hiprintDesignRef.value?.hiprintTemplate.update(JSON.parse(state.ruleForm.template));
  }
};

const cancel = () => {
  state.dialogVisible = false;
};

const submit = async () => {
  state.showDialog2 = true;
  if (state.ruleForm.orderNo == undefined) state.ruleForm.orderNo = 100;
  state.ruleForm.isEnabled = isEnabled(state.ruleForm);
};

const templateCancel = () => {
  state.showDialog2 = false;
};

const templateSubmit = async () => {
  state.ruleForm.template = JSON.stringify(hiprintDesignRef.value?.hiprintTemplate.getJson());
  const payload = {
    id: state.ruleForm.id,
    name: state.ruleForm.name,
    template: state.ruleForm.template,
    orderNo: state.ruleForm.orderNo,
    isEnabled: isEnabled(state.ruleForm),
    remark: state.ruleForm.remark,
    rowVersion: readRowVersion(state.ruleForm),
  }
  if (state.ruleForm.id != undefined && state.ruleForm.id > 0) {
    await tenantPrintUpdatePost(payload);
  } else {
    await tenantPrintAddPost(payload);
  }
  cancel();
  templateCancel();
  emits('query');
};

defineExpose({openDialog});
</script>

<style lang="scss" scoped>
.printDialog {
  :deep(.p-dialog-content) {
    max-height: calc(100vh - 45px);
  }
}
</style>
