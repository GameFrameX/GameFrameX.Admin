<template>
  <Dialog v-model:visible="state.dialogVisible" :draggable="true" :modal="true" :closable="true" :style="{width: state.width + 8 + 'mm'}">
    <div id="preview_content" ref="previewContentRef"></div>
    <template #footer>
      <Button :loading="state.waitShowPrinter" @click="print">
        <i class="fa-solid fa-print mr-1"></i>
        {{ t('system.print.directPrint') }}
      </Button>
      <Button @click="toPdf">
        <i class="fa-solid fa-print mr-1"></i>
        {{ t('system.print.exportPdf') }}
      </Button>
      <Button severity="secondary" @click="hideDialog">{{ t('system.print.close') }}</Button>
    </template>
  </Dialog>
</template>

<script lang="ts" setup>
import { faPrint } from '../../../../../utils/icons';
import {nextTick, reactive, ref} from 'vue';

const state = reactive({
  dialogVisible: false,
  waitShowPrinter: false,
  width: 0,
  printData: {},
  hiprintTemplate: {} as any,
});

const previewContentRef = ref();

const showDialog = (hiprintTemplate: any, printData: {}, width = 210) => {
  state.dialogVisible = true;
  state.width = width;
  state.hiprintTemplate = hiprintTemplate;
  state.printData = printData;
  nextTick(() => {
    while (previewContentRef.value?.firstChild) {
      previewContentRef.value.removeChild(previewContentRef.value.firstChild);
    }
    const newHtml = hiprintTemplate.getHtml(printData);
    previewContentRef.value.appendChild(newHtml[0]);
  });
};

const print = () => {
  state.waitShowPrinter = true;
  state.hiprintTemplate.print(
      state.printData,
      {},
      {
        callback: () => {
          state.waitShowPrinter = false;
        },
      }
  );
};

const toPdf = () => {
  state.hiprintTemplate.toPdf({}, t('system.print.pdfFileName'));
};

const hideDialog = () => {
  state.dialogVisible = false;
};

defineExpose({showDialog});
</script>

<style lang="less" scoped>
:deep(.p-dialog-content) {
  padding: 0px;
}

:deep(.p-dialog) {
  margin-bottom: 24px;
}
</style>
