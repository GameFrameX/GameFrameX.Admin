<template>
  <div class="icon-selector w100 h100">
    <span class="p-input-icon-left w100" ref="inputWidthRef">
      <i :class="state.fontIconPrefix === '' ? prepend : state.fontIconPrefix"/>
      <InputText
          v-model="state.fontIconSearch"
          :placeholder="state.fontIconPlaceholder"
          :disabled="disabled"
          class="w-full"
          @focus="onIconFocus"
          @blur="onIconBlur"
      />
    </span>
    <Popover ref="op">
      <template #container>
        <div class="icon-selector-warp">
          <div class="icon-selector-warp-title">{{ title }}</div>
          <Tabs v-model:value="state.fontIconTabActive" @update:value="onTabChange">
            <TabList>
              <Tab value="solid">solid</Tab>
              <Tab value="regular">regular</Tab>
              <Tab value="brands">brands</Tab>
            </TabList>
            <TabPanels>
              <TabPanel value="solid">
                <IconList :list="fontIconSheetsFilterList" :empty="emptyDescription" :prefix="state.fontIconPrefix"
                          @getIcon="onColClick"/>
              </TabPanel>
              <TabPanel value="regular">
                <IconList :list="fontIconSheetsFilterList" :empty="emptyDescription" :prefix="state.fontIconPrefix"
                          @getIcon="onColClick"/>
              </TabPanel>
              <TabPanel value="brands">
                <IconList :list="fontIconSheetsFilterList" :empty="emptyDescription" :prefix="state.fontIconPrefix"
                          @getIcon="onColClick"/>
              </TabPanel>
            </TabPanels>
          </Tabs>
        </div>
      </template>
    </Popover>
  </div>
</template>

<script setup lang="ts" name="iconSelector">
import InputText from 'primevue/inputtext';
import Popover from 'primevue/popover';
import Tabs from 'primevue/tabs';
import TabList from 'primevue/tablist';
import Tab from 'primevue/tab';
import TabPanels from 'primevue/tabpanels';
import TabPanel from 'primevue/tabpanel';
import {defineAsyncComponent, ref, reactive, onMounted, nextTick, computed, watch} from 'vue';
import fontAwesomeIcons from '../../utils/getStyleSheets';


const props = defineProps({
  prepend: {
    type: String,
    default: () => 'fa-solid fa-arrow-pointer',
  },
  placeholder: {
    type: String,
    default: () => t('message.common.form.pleaseInput'),
  },
  size: {
    type: String,
    default: () => 'default',
  },
  title: {
    type: String,
    default: () => t('message.common.form.pleaseSelect'),
  },
  disabled: {
    type: Boolean,
    default: () => false,
  },
  clearable: {
    type: Boolean,
    default: () => true,
  },
  emptyDescription: {
    type: String,
    default: () => t('message.common.message.noIcon'),
  },
  modelValue: String,
});

const emit = defineEmits(['update:modelValue', 'get', 'clear']);

// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const IconList = defineAsyncComponent(() => import('/@/components/icon-selector/IconSelectorList.vue'));

const inputWidthRef = ref();
const op = ref();
const state = reactive({
  fontIconPrefix: '',
  fontIconWidth: 0,
  fontIconSearch: '',
  fontIconPlaceholder: '',
  fontIconTabActive: 'solid',
  fontIconList: {
    solid: [] as string[],
    regular: [] as string[],
    brands: [] as string[],
  },
});

// PrimeVue Popover.show 需要 event/target 作锚点，无参调用会在 alignOverlay 读 currentTarget 直接抛
// TypeError（HEAD 存量 bug，GFX-714 冒烟暴露）；suppressNextFocusShow 防止选完图标 hide 后 refocus 立即重开
let suppressNextFocusShow = false;
const onIconFocus = (event?: FocusEvent) => {
  if (!props.modelValue) return false;
  state.fontIconSearch = '';
  state.fontIconPlaceholder = props.modelValue;
  if (suppressNextFocusShow) {
    suppressNextFocusShow = false;
    return;
  }
  op.value?.show(event);
};
const onIconBlur = () => {
  const list = fontIconTabNameList();
  setTimeout(() => {
    const icon = list.filter((icon: string) => icon === state.fontIconSearch);
    if (icon.length <= 0) state.fontIconSearch = '';
  }, 300);
};
const fontIconSheetsFilterList = computed(() => {
  const list = fontIconTabNameList();
  if (!state.fontIconSearch) return list;
  let search = state.fontIconSearch.trim().toLowerCase();
  return list.filter((item: string) => {
    if (item.toLowerCase().indexOf(search) !== -1) return item;
  });
});
const fontIconTabNameList = () => {
  if (state.fontIconTabActive === 'solid') return state.fontIconList.solid;
  if (state.fontIconTabActive === 'regular') return state.fontIconList.regular;
  if (state.fontIconTabActive === 'brands') return state.fontIconList.brands;
  return [];
};
const initModeValueEcho = () => {
  if (props.modelValue === '') return ((<string | undefined>state.fontIconPlaceholder) = props.placeholder);
  (<string | undefined>state.fontIconPlaceholder) = props.modelValue;
  (<string | undefined>state.fontIconPrefix) = props.modelValue;
};
const initFontIconName = () => {
  let name = 'solid';
  if (props.modelValue) {
    if (props.modelValue.indexOf('fa-regular') > -1) name = 'regular';
    else if (props.modelValue.indexOf('fa-brands') > -1) name = 'brands';
  }
  state.fontIconTabActive = name;
  return name;
};
const initFontIconData = async (name: string) => {
  if (name === 'solid') {
    if (state.fontIconList.solid.length > 0) return;
    await fontAwesomeIcons.solid().then((res: any) => {
      state.fontIconList.solid = res;
    });
  } else if (name === 'regular') {
    if (state.fontIconList.regular.length > 0) return;
    await fontAwesomeIcons.regular().then((res: any) => {
      state.fontIconList.regular = res;
    });
  } else if (name === 'brands') {
    if (state.fontIconList.brands.length > 0) return;
    await fontAwesomeIcons.brands().then((res: any) => {
      state.fontIconList.brands = res;
    });
  }
  state.fontIconPlaceholder = props.placeholder;
  initModeValueEcho();
};
const onTabChange = (tabName: string) => {
  initFontIconData(tabName);
  suppressNextFocusShow = true;
  inputWidthRef.value?.focus();
};
const onColClick = (v: string) => {
  state.fontIconPlaceholder = v;
  state.fontIconPrefix = v;
  emit('get', state.fontIconPrefix);
  emit('update:modelValue', state.fontIconPrefix);
  op.value?.hide();
  suppressNextFocusShow = true;
  inputWidthRef.value?.focus();
};
const onClearFontIcon = () => {
  state.fontIconPrefix = '';
  emit('clear', state.fontIconPrefix);
  emit('update:modelValue', state.fontIconPrefix);
};
const getInputWidth = () => {
  nextTick(() => {
    if (inputWidthRef.value) {
      state.fontIconWidth = inputWidthRef.value.offsetWidth || (inputWidthRef.value.$el && inputWidthRef.value.$el.offsetWidth) || 0;
    }
  });
};
const initResize = () => {
  window.addEventListener('resize', () => {
    getInputWidth();
  });
};
onMounted(() => {
  initFontIconData(initFontIconName());
  initResize();
  getInputWidth();
});
watch(
    () => props.modelValue,
    () => {
      initModeValueEcho();
      initFontIconName();
    }
);
</script>
