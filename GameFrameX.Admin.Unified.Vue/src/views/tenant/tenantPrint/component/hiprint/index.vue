<template>
  <div class="grid" style="gap: 8px; margin-bottom: 5px">
    <div class="col-2">
      <Select filter v-model="state.mode" :options="state.modeList" optionLabel="name" optionValue="value" @change="changeMode" class="w100"/>
    </div>

    <div class="col-10">
      <div class="flex align-items-center gap-1 flex-wrap">
        <div class="flex gap-1">
          <Button v-for="(value, type) in state.paperTypes" :key="type"
                  :severity="curPaperType === type ? undefined : 'secondary'"
                  @click="setPaper(type, value)" size="small">
            {{ type }}
          </Button>
          <Popover>
            <Button :severity="'other' == curPaperType ? undefined : 'secondary'" size="small" style="margin-left: 10px">{{ t('system.print.customSize') }}</Button>
            <template #content>
              <div class="flex flex-column gap-3 p-4" style="width: 300px">
                <div class="flex align-items-center justify-between gap-2">
                  <InputNumber v-model="state.paperWidth" :useGrouping="false" inputStyle="width: 100px; text-align: center" :placeholder="t('system.print.widthMm')"/>
                  ~
                  <InputNumber v-model="state.paperHeight" :useGrouping="false" inputStyle="width: 100px; text-align: center" :placeholder="t('system.print.heightMm')"/>
                </div>
                <Button @click="otherPaper" class="w100">{{ t('common.button.confirm') }}</Button>
              </div>
            </template>
          </Popover>
        </div>
        <InputNumber :modelValue="state.scaleValue" :precision="2" :step="0.1" :min="state.scaleMin" :max="state.scaleMax"
                     @update:modelValue="changeScale" style="margin-right: 8px" inputStyle="width: 80px"/>

        <div class="flex gap-1">
          <Button v-tooltip.bottom="t('system.print.alignLeft')" severity="secondary" @click="setElsAlign('left')">
            <i class="fa-solid fa-arrow-left"></i>
          </Button>
          <Button v-tooltip.bottom="t('system.print.alignCenter')" severity="secondary" @click="setElsAlign('vertical')">
            <i class="fa-solid fa-expand"></i>
          </Button>
          <Button v-tooltip.bottom="t('system.print.alignRight')" severity="secondary" @click="setElsAlign('right')">
            <i class="fa-solid fa-chevron-right"></i>
          </Button>
          <Button v-tooltip.bottom="t('system.print.alignTop')" severity="secondary" @click="setElsAlign('top')">
            <i class="fa-solid fa-arrow-up"></i>
          </Button>
          <Button v-tooltip.bottom="t('system.print.alignVerticalCenter')" severity="secondary" @click="setElsAlign('horizontal')">
            <i class="fa-solid fa-sort"></i>
          </Button>
          <Button v-tooltip.bottom="t('system.print.alignBottom')" severity="secondary" @click="setElsAlign('bottom')">
            <i class="fa-solid fa-arrow-down"></i>
          </Button>
          <Button v-tooltip.bottom="t('system.print.distributeHor')" severity="secondary" @click="setElsAlign('distributeHor')">
            <i class="fa-solid fa-arrow-down-short-wide"></i>
          </Button>
          <Button v-tooltip.bottom="t('system.print.distributeVer')" severity="secondary" @click="setElsAlign('distributeVer')">
            <i class="fa-solid fa-toggle-on"></i>
          </Button>
        </div>

        <div class="flex gap-1" style="margin-left: 8px">
          <Button @click="rotatePaper">
            <i class="fa-solid fa-rotate-right mr-1"></i>
            {{ t('system.print.rotate') }}
          </Button>
          <Button @click="preView">
            <i class="fa-solid fa-eye mr-1"></i>
            {{ t('system.print.preview') }}
          </Button>
          <Button @click="print">
            <i class="fa-solid fa-print mr-1"></i>
            {{ t('system.print.directPrint') }}
          </Button>
          <Button @click="viewJson">
            <i class="fa-solid fa-circle-check mr-1"></i>
            {{ t('system.print.templateJson') }}
          </Button>
          <Button severity="danger" @click="clearPaper">
            <i class="fa-solid fa-trash mr-1"></i>
            {{ t('system.print.clearTemplate') }}
          </Button>
        </div>
      </div>
    </div>
  </div>

  <div class="grid" style="gap: 8px">
    <div class="col-2">
      <Card>
        <template #content>
          <div id="hiprintEpContainer" class="rect-printElement-types hiprintEpContainer"></div>
        </template>
      </Card>
    </div>
    <div class="col-7">
      <Card class="card-design">
        <template #content>
          <div id="hiprint-printTemplate" class="hiprint-printTemplate"></div>
        </template>
      </Card>
    </div>
    <div class="col-3 params_setting_container">
      <Card>
        <template #content>
          <div id="PrintElementOptionSetting"></div>
        </template>
      </Card>
    </div>
  </div>

  <Drawer v-model:visible="state.templateDialogVisible" :header="t('system.print.printTemplate')">
    <vue-json-pretty :data="state.templateContent" showLength showIcon showLineNumber showSelectController/>
  </Drawer>

  <PrintPreview ref="preViewRef"/>
</template>

<script lang="ts" setup name="hiprintDesign">
import { faArrowDown, faArrowDownShortWide, faArrowLeft, faArrowUp, faChevronRight, faCircleCheck, faExpand, faEye, faPrint, faRotateRight, faSort, faToggleOn, faTrash } from '../../../../../utils/icons';
import {computed, onMounted, ref, reactive} from 'vue';
import {showError, showConfirm} from '../../../../../utils/message';
import VueJsonPretty from 'vue-json-pretty';
import 'vue-json-pretty/lib/styles.css';

import {hiprint} from 'vue-plugin-hiprint';
import providers from './providers';
import PrintPreview from './PrintPreview.vue';
import printData from './print-data';

let hiprintTemplate = ref();

const preViewRef = ref();
const state = reactive({
  mode: providers[0]?.value ?? '',
  modeList: [] as any,
  curPaper: {
    type: 'A4',
    width: 220,
    height: 296.6,
  },
  paperTypes: {
    A3: {
      width: 420,
      height: 296.6,
    },
    A4: {
      width: 210,
      height: 296.6,
    },
    A5: {
      width: 210,
      height: 147.6,
    },
    B3: {
      width: 500,
      height: 352.6,
    },
    B4: {
      width: 250,
      height: 352.6,
    },
    B5: {
      width: 250,
      height: 175.6,
    },
  },
  scaleValue: 1,
  scaleMax: 5,
  scaleMin: 0.5,
  paperPopVisible: false,
  paperWidth: 220,
  paperHeight: 80,

  templateDialogVisible: false,
  templateContent: '',
});

const curPaperType = computed(() => {
  let type = 'other';
  let types: any = state.paperTypes;
  for (const key in types) {
    let item = types[key];
    let {width, height} = state.curPaper;
    if (item.width === width && item.height === height) {
      type = key;
    }
  }
  return type;
});

const changeMode = () => {
  const provider = providers.find((item) => item.value === state.mode) ?? providers[0];
  if (!provider) return;
  hiprint.init({
    providers: [provider.f],
  });

  const hiprintEpContainerEl = document.getElementById('hiprintEpContainer');
  if (hiprintEpContainerEl) {
    hiprintEpContainerEl.innerHTML = '';
  }
  hiprint.PrintElementTypeManager.build('.hiprintEpContainer', provider.value);

  const hiprintPrintTemplate = document.getElementById('hiprint-printTemplate');
  if (hiprintPrintTemplate) {
    hiprintPrintTemplate.innerHTML = '';
  }
  let template = {};
  hiprintTemplate.value = new hiprint.PrintTemplate({
    template: template,
    settingContainer: '#PrintElementOptionSetting',
    paginationContainer: '.hiprint-printPagination',
  });
  hiprintTemplate.value.design('#hiprint-printTemplate');
  state.scaleValue = hiprintTemplate.value.editingPanel.scale || 1;
};

const setPaper = (type: string, value: { width: number; height: number }) => {
  try {
    if (Object.keys(state.paperTypes).includes(type)) {
      state.curPaper = {type: type, width: value.width, height: value.height};
      hiprintTemplate.value.setPaper(value.width, value.height);
    } else {
      state.curPaper = {type: 'other', width: value.width, height: value.height};
      hiprintTemplate.value.setPaper(value.width, value.height);
    }
  } catch (error) {
    showError(t('system.print.operateFailed') + `: ${error}`);
  }
};

const changeScale = (currentValue: number) => {
  let scaleVal = currentValue;
  if (hiprintTemplate.value) {
    hiprintTemplate.value.zoom(scaleVal);
    state.scaleValue = scaleVal;
  }
};

const rotatePaper = () => {
  if (hiprintTemplate.value) {
    hiprintTemplate.value.rotatePaper();
  }
};

const setElsAlign = (e: any) => {
  hiprintTemplate.value.setElsAlign(e);
};

const clearPaper = async () => {
  if (!await showConfirm(t('system.print.clearConfirm'), t('system.print.warning'))) return;
  try {
    hiprintTemplate.value.clear();
  } catch (error) {
    showError(t('system.print.operateFailed') + `: ${error}`);
  }
};

const otherPaper = () => {
  let value = {
    width: state.paperWidth,
    height: state.paperHeight,
  };
  setPaper('other', value);
};

const preView = () => {
  let {width} = state.curPaper;
  preViewRef.value.showDialog(hiprintTemplate.value, printData, width);
};
const print = () => {
};

const viewJson = () => {
  if (hiprintTemplate.value) {
    var templateJson = JSON.stringify(hiprintTemplate.value.getJson() || {});
    state.templateContent = JSON.parse(templateJson);
    state.templateDialogVisible = true;
  }
};

onMounted(() => {
  state.modeList = providers.map((e) => {
    return {type: e.type, name: e.name, value: e.value};
  });
  changeMode();
});

defineExpose({hiprintTemplate});
</script>

<style lang="scss" scoped>
:deep(.rect-printElement-types .hiprint-printElement-type > li > ul > li > a) {
  height: auto;
  text-overflow: ellipsis;
  color: var(--p-primary-color);
  box-shadow: none !important;
  border: 1px dashed var(--p-primary-color);
}

:deep(.hiprint-printElement-image-content) {
  img {
    content: url('/@/assets/logo.png');
  }
}

.card-design {
  overflow: hidden;
  overflow-x: auto;
  overflow-y: auto;
}

:deep(.hiprint-option-item-submitBtn) {
  background: var(--p-primary-color);
}

:deep(.hiprint-option-item-deleteBtn) {
  background: var(--p-red-500);
}

:deep(.prop-tabs .prop-tab-items li.active) {
  color: var(--p-primary-color);
  border-bottom: 2px solid var(--p-primary-color);
}
</style>
