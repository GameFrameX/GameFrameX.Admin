<template>
  <div class="table-container">
    <div class="table-header mb15">
      <div>
        <slot name="command"></slot>
      </div>
      <div v-block-loading="state.importLoading" class="table-footer-tool">
        <button v-if="!config.hideRefresh" type="button" class="table-footer-tool-btn"
                :title="t('message.common.button.refresh')" :aria-label="t('message.common.button.refresh')"
                @click="onRefreshTable">
          <SvgIcon name="fa-solid fa-arrows-rotate" :size="22"/>
        </button>
        <button v-if="!config.hideExport" type="button" class="table-footer-tool-btn"
                v-tooltip.bottom="t('message.common.button.export')"
                :title="t('message.common.button.export')" :aria-label="t('message.common.button.export')"
                :aria-haspopup="'menu'" @click="onExportMenuToggle">
          <SvgIcon name="fa-solid fa-cloud-arrow-down" :size="22"/>
        </button>
        <Popover ref="exportMenuRef">
          <div class="p-menu" style="min-width: 160px;">
            <div class="p-menuitem" style="padding: 8px 12px; cursor: pointer;" role="button" :tabindex="0"
                 @click="onImportTable"
                 @keydown.enter.prevent="onImportTable"
                 @keydown.space.prevent="onImportTable">
              {{ t('message.common.table.exportCurrentPage') }}
            </div>
            <div class="p-menuitem" style="padding: 8px 12px; cursor: pointer;" role="button" :tabindex="0"
                 @click="onImportTableAll"
                 @keydown.enter.prevent="onImportTableAll"
                 @keydown.space.prevent="onImportTableAll">
              {{ t('message.common.table.exportAllData') }}
            </div>
          </div>
        </Popover>
        <button v-if="!config.hidePrint" type="button" class="table-footer-tool-btn"
                :title="t('message.common.button.print')" :aria-label="t('message.common.button.print')"
                @click="onPrintTable">
          <SvgIcon name="fa-solid fa-print" :size="19"/>
        </button>
        <button v-if="!config.hideSet" type="button" class="table-footer-tool-btn"
                :title="t('message.common.table.columnDisplay')" :aria-label="t('message.common.table.columnDisplay')"
                :aria-haspopup="'dialog'" @click="onSettingsMenuToggle">
          <SvgIcon name="fa-solid fa-gear" :size="22"/>
        </button>
        <Popover ref="settingsMenuRef" :style="{ width: 'min(300px, calc(100vw - 32px))' }">
          <template #container>
            <div class="tool-box">
              <i v-tooltip.top="t('message.common.table.dragToSort')" class="fa-regular fa-circle-question ml11" style="font-size: 16px; color: var(--p-text-muted-color); cursor: help;"></i>
              <Checkbox v-model="state.checkListAll" :binary="true" class="ml10 mr1" :indeterminate="state.checkListIndeterminate"
                        @update:modelValue="onCheckAllChange"/>
              <label class="ml-1" @click="onCheckAllChange(!state.checkListAll)">{{ t('message.common.table.columnDisplay') }}</label>
              <Checkbox v-model="getConfig.isSerialNo" :binary="true" class="ml12 mr1"/>
              <label class="ml-1">{{ t('message.common.table.serialNo') }}</label>
              <Checkbox v-if="getConfig.showSelection" v-model="getConfig.isSelection" :binary="true" class="ml12 mr1"/>
              <label v-if="getConfig.showSelection" class="ml-1">{{ t('message.common.table.selection') }}</label>
            </div>
            <ScrollPanel style="height: 250px">
              <div ref="toolSetRef" class="tool-sortable">
                <div class="tool-sortable-item" v-for="v in columns" :key="v.prop"
                     v-show="!v.hideCheck && !v.fixed" :data-key="v.prop">
                  <i class="fa-solid fa-maximize handle cursor-pointer"></i>
                  <Checkbox v-model="v.isCheck" :binary="true" class="ml12 mr8" @update:modelValue="onCheckChange"/>
                  <label class="ml-1">{{ v.label }}</label>
                </div>
              </div>
            </ScrollPanel>
          </template>
        </Popover>
      </div>
    </div>
    <DataTable ref="tableRef" :value="state.data" :showGridlines="setBorder" :stripedRows="setStripe" v-bind="$attrs"
               :rowKey="'id'" :loading="state.loading" :defaultSort="defaultSort"
               v-model:selection="state.selectlist" dataKey="id"
               :scrollable="hasFixedColumns"
               @sort="onSortChange">
      <Column v-if="config.isSelection && config.showSelection" selectionMode="multiple" :headerStyle="{ width: 'min(30px, calc(100vw - 32px))' }"/>
      <Column v-if="config.isSerialNo" field="id" header="Id" :headerStyle="{ width: 'min(60px, calc(100vw - 32px))', textAlign: 'center' }" :bodyStyle="{ textAlign: 'center' }"/>
      <Column v-for="(item, index) in setHeader" :key="index" :field="item.prop" :header="item.label" :sortable="item.sortable" :style="item.width ? { width: item.width + 'px' } : {}"
              :frozen="!!item.fixed" :alignFrozen="item.fixed">
        <template #body="scope" v-if="(!item.children) && $slots[item.prop]">
          <slot :name="item.prop" v-bind="scope"></slot>
        </template>
        <template #body="scope" v-else-if="!item.children">
          <template v-if="item.type === 'image'">
            <img :alt="item.label" :style="{ width: `${item.width}px`, height: `${item.height}px`, objectFit: 'cover' }"
                 :src="scope.data[item.prop]"/>
          </template>
          <template v-else>
            {{ scope.data[item.prop] }}
          </template>
        </template>
        <Column v-for="(childrenItem, childrenIndex) in item.children" :key="childrenIndex"
                :field="childrenItem.prop" :header="childrenItem.label" :sortable="childrenItem.sortable" :style="childrenItem.width ? { width: childrenItem.width + 'px' } : {}"
                :frozen="!!childrenItem.fixed" :alignFrozen="childrenItem.fixed">
          <template #body="scope" v-if="$slots[childrenItem.prop]">
            <slot :name="childrenItem.prop" v-bind="scope"></slot>
          </template>
          <template #body="scope" v-else>
            <template v-if="childrenItem.type === 'image'">
              <img :alt="childrenItem.label" :style="{ width: `${childrenItem.width}px`, height: `${childrenItem.height}px`, objectFit: 'cover' }"
                   :src="scope.data[childrenItem.prop]"/>
            </template>
            <template v-else>
              {{ scope.data[childrenItem.prop] }}
            </template>
          </template>
        </Column>
      </Column>
      <template #empty>
        <div style="text-align: center; padding: 40px 0;">
          <i class="fa-solid fa-inbox" style="font-size: 40px; color: var(--p-surface-400); display: block; margin-bottom: 10px;"></i>
          <span style="color: var(--p-surface-500); font-size: 14px;">{{ t('message.common.table.noData') }}</span>
        </div>
      </template>
    </DataTable>
    <div v-if="shouldShowPagination" class="table-footer mt15">
      <Paginator :rows="state.page.pageSize" :totalRecords="state.total" :rowsPerPageOptions="clampedPageSizes"
                 :first="(state.page.page - 1) * state.page.pageSize"
                 @page="onPageChange"/>
    </div>
  </div>
</template>

<script setup lang="ts" name="netxTable">
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Paginator from 'primevue/paginator';
import Popover from 'primevue/popover';
import Checkbox from 'primevue/checkbox';
import ScrollPanel from 'primevue/scrollpanel';
import Tooltip from 'primevue/tooltip';
import {reactive, computed, nextTick, ref, onMounted} from 'vue';
import {showError, showSuccess, showWarning} from '../../utils/message';
import Sortable from 'sortablejs';
import {storeToRefs} from 'pinia';
import {useThemeConfigStore} from '../../stores/themeConfig';
import {exportExcel} from '../../utils/exportExcel';
import {DEFAULT_PAGE_SIZE, DEFAULT_PAGE_SIZE_OPTIONS, MAX_PAGE_SIZE, clampPageSize} from '../../constants/pagination';
import printJs from 'print-js';

const vTooltip = Tooltip;


const props = defineProps({
  getData: {
    type: Function,
    required: true,
  },
  columns: {
    type: Array<any>,
    default: () => [],
  },
  config: {
    type: Object,
    default: () => {
    },
  },
  param: {
    type: Object,
    default: () => {
    },
  },
  defaultSort: {
    type: Object,
    default: () => {
    },
  },
  exportChangeData: {
    type: Function,
  },
  printName: {
    type: String,
    default: () => '',
  },
});

const emit = defineEmits(['pageChange', 'selectionChange', 'sortHeader']);

const toolSetRef = ref();
const tableRef = ref();
const exportMenuRef = ref();
const settingsMenuRef = ref();
const storesThemeConfig = useThemeConfigStore();
const {themeConfig} = storeToRefs(storesThemeConfig);
const state = reactive({
  data: [] as Array<EmptyObjectType>,
  loading: false,
  importLoading: false,
  total: 0,
  page: {
    page: 1,
    pageSize: 10,
    orderBy: '',
    isDescending: false,
  },
  showPagination: true,
  selectlist: [] as EmptyObjectType[],
  checkListAll: true,
  checkListIndeterminate: false,
});

const setBorder = computed(() => {
  return props.config.isBorder ? true : false;
});
const setStripe = computed(() => {
  return props.config.isStripe ? true : false;
});
const getConfig = computed(() => {
  return props.config;
});
const setHeader = computed(() => {
  return props.columns.filter((v) => v.isCheck);
});
const hasFixedColumns = computed(() => {
  return setHeader.value.some((v) => v.fixed || v.children?.some((child: any) => child.fixed));
});
const clampedPageSizes = computed(() => {
  const pageSizes = props.config.pageSizes ?? DEFAULT_PAGE_SIZE_OPTIONS;
  return pageSizes.filter((pageSize: number) => pageSize <= MAX_PAGE_SIZE);
});
const pageCount = computed(() => {
  if (!state.page.pageSize || state.page.pageSize <= 0) return 0;
  return Math.ceil(state.total / state.page.pageSize);
});
const shouldShowPagination = computed(() => {
  return !props.config.hidePagination && state.showPagination && pageCount.value > 1;
});
const onCheckAllChange = (val: any) => {
  if (val) props.columns.forEach((v) => (v.isCheck = true));
  else props.columns.forEach((v) => (v.isCheck = false));
  state.checkListIndeterminate = false;
};
const onCheckChange = () => {
  const headers = props.columns.filter((v) => v.isCheck).length;
  state.checkListAll = headers === props.columns.length;
  state.checkListIndeterminate = headers > 0 && headers < props.columns.length;
};
const onSelectionChange = (val: EmptyObjectType[]) => {
  state.selectlist = val;
  emit('selectionChange', state.selectlist);
};
const onHandleSizeChange = (val: number) => {
  state.page.pageSize = val;
  onList();
  emit('pageChange', state.page);
};
const onHandleCurrentChange = (val: number) => {
  state.page.page = val;
  onList();
  emit('pageChange', state.page);
};
const onSortChange = (event: any) => {
  state.page.orderBy = event.sortField;
  state.page.isDescending = event.sortOrder === -1;
  onList();
};
const onPageChange = (event: any) => {
  state.page.page = event.page + 1;
  state.page.pageSize = event.rows;
  onList();
  emit('pageChange', state.page);
};
const onExportMenuToggle = (event: Event) => {
  exportMenuRef.value.toggle(event);
};
const onSettingsMenuToggle = (event: Event) => {
  settingsMenuRef.value.toggle(event);
  nextTick(() => {
    onSetTable();
  });
};
const pageReset = () => {
  state.selectlist = [];
  state.page.page = 1;
  onList();
};
const onImportTable = () => {
  if (setHeader.value.length <= 0) return showError(t('message.common.table.noExportColumns'));
  importData(state.data);
  exportMenuRef.value.hide();
};
const onImportTableAll = async () => {
  if (setHeader.value.length <= 0) return showError(t('message.common.table.noExportColumns'));
  state.importLoading = true;
  const pageSize = MAX_PAGE_SIZE;
  let page = 1;
  let total = 0;
  let data: Array<EmptyObjectType> = [];
  do {
    const param = Object.assign({}, props.param, {page, pageSize});
    const res = await props.getData(param);
    const items = res.data?.items ?? [];
    data = data.concat(items);
    total = res.data?.total ?? data.length;
    if (items.length <= 0) break;
    page++;
  } while (data.length < total);
  state.importLoading = false;
  importData(data);
  exportMenuRef.value.hide();
};
const importData = (data: Array<EmptyObjectType>) => {
  if (data.length <= 0) return showError(t('message.common.table.noExportData'));
  state.importLoading = true;
  let exportData = JSON.parse(JSON.stringify(data));
  if (props.exportChangeData) {
    exportData = props.exportChangeData(exportData);
  }
  exportExcel(
      exportData,
      `${props.config.exportFileName ? props.config.exportFileName : themeConfig.value.globalTitle}_${new Date().toLocaleString()}`,
      setHeader.value.filter((item) => {
        return item.type != 'action';
      }),
      t('message.common.table.exportData')
  );
  state.importLoading = false;
};
const onPrintTable = () => {
  let tableTh = '';
  let tableTrTd = '';
  let tableTd: any = {};
  props?.columns.forEach((v: any) => {
    if (v.prop === "action") {
      return;
    }
    tableTh += `<th class="table-th">${v.label}</th>`;
  });
  state.data.forEach((val: any, key: any) => {
    if (!tableTd[key]) tableTd[key] = [];
    props.columns.forEach((v: any) => {
      if (v.prop === "action") {
        return;
      }
      if (v.type === 'text') {
        tableTd[key].push(`<td class="table-th table-center">${val[v.prop]}</td>`);
      } else if (v.type === 'image') {
        tableTd[key].push(`<td class="table-th table-center"><img src="${val[v.prop]}" alt="" style="width:${v.width}px;height:${v.height}px;"/></td>`);
      }
    });
    tableTrTd += `<tr>${tableTd[key].join('')}</tr>`;
  });
  printJs({
    printable: `<div style=display:flex;flex-direction:column;text-align:center><h3>${props.printName}</h3></div><table border=1 cellspacing=0><tr>${tableTh}${tableTrTd}</table>`,
    type: 'raw-html',
    css: ['//cdnjs.cloudflare.com/ajax/libs/font-awesome/7.2.0/css/all.min.css', '//unpkg.com/primevue/resources/themes/aura-light-green/theme.css'],
    style: `@media print{.mb15{margin-bottom:15px;}.p-button--small i.fa-solid{font-size: 12px !important;margin-right: 5px;}}; .table-th{word-break: break-all;white-space: pre-wrap;}.table-center{text-align: center;}`,
  });
};
const onRefreshTable = () => {
  onList();
};
const onSetTable = () => {
  nextTick(() => {
    if (!toolSetRef.value) return;
    const sortable = Sortable.create(toolSetRef.value, {
      handle: '.handle',
      dataIdAttr: 'data-key',
      animation: 150,
      onEnd: () => {
        const headerList: EmptyObjectType[] = [];
        sortable.toArray().forEach((val: any) => {
          props.columns.forEach((v) => {
            if (v.prop === val) headerList.push({...v});
          });
        });
        emit('sortHeader', headerList);
      },
    });
  });
};

const onList = async () => {
  state.loading = true;
  let param = Object.assign({}, props.param, {...state.page});
  Object.keys(param).forEach((key) => (param[key] === undefined) && delete param[key]);
  const res = await props.getData(param);
  state.loading = false;
  if (res && res.data && res.data.items) {
    state.showPagination = true;
    state.data = res.data?.items ?? [];
    state.total = res.data?.total ?? 0;
  } else {
    state.showPagination = false;
    state.data = (res && res.data) ? res.data : [];
  }
};

const toggleSelection = (row: any, statu?: boolean) => {
  if (statu === false) {
    state.selectlist = state.selectlist.filter((item: any) => item.id !== row.id);
  } else if (statu === true) {
    const exists = state.selectlist.some((item: any) => item.id === row.id);
    if (!exists) state.selectlist.push(row);
  } else {
    const exists = state.selectlist.some((item: any) => item.id === row.id);
    if (exists) {
      state.selectlist = state.selectlist.filter((item: any) => item.id !== row.id);
    } else {
      state.selectlist.push(row);
    }
  }
};

const getTableData = () => {
  return state.data;
}

const setTableData = (data: Array<EmptyObjectType>, add: boolean = false) => {
  if (add) {
    var repeat = false;
    for (let newItem of data) {
      repeat = false;
      for (let item of state.data) {
        if (newItem.id === item.id) {
          repeat = true;
          break;
        }
      }
      if (!repeat) {
        state.data.push(newItem);
      }
    }
  } else {
    state.data = data;
  }
}

onMounted(() => {
  if (props.defaultSort) {
    state.page.field = props.defaultSort.prop;
    state.page.order = props.defaultSort.order;
  }
  state.page.pageSize = clampPageSize(props.config.pageSize ?? DEFAULT_PAGE_SIZE);
  onList();
});

defineExpose({
  pageReset,
  onList,
  toggleSelection,
  getTableData,
  setTableData,
});
</script>

<style scoped lang="scss">
.table-container {
  display: flex;
  flex-direction: column;
  height: 100%;

  .p-datatable {
    flex: 1;
  }

  .table-footer {
    display: flex;
    justify-content: center;
  }

  .table-header {
    display: flex;

    .table-footer-tool {
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: flex-end;

      i {
        margin-right: 10px;
        cursor: pointer;
        color: var(--p-text-muted-color);

        &:last-of-type {
          margin-right: 0;
        }
      }

      span {
        i {
          margin-right: 10px;
          color: var(--p-text-muted-color);
        }
      }

      // 工具图标由 SvgIcon/span 直挂换原生 button（C248 键盘可达），抹平 UA 默认外观并保留右距节奏
      .table-footer-tool-btn {
        border: 0;
        background: transparent;
        padding: 0;
        cursor: pointer;
        display: inline-flex;
        align-items: center;

        &:last-of-type {
          margin-right: 0;
        }

        i {
          margin-right: 10px;
          color: var(--p-text-muted-color);
        }

        &:focus-visible {
          outline: 2px solid var(--p-primary-color);
          outline-offset: 2px;
          border-radius: 2px;
        }
      }
    }
  }
}

.tool-box {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 4px;
  padding: 8px 12px;
}

.tool-sortable-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 12px;
  cursor: grab;
}

.p-menuitem {
  &:hover {
    background-color: var(--p-surface-100);
  }
}
</style>
