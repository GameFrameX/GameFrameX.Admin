<template>
  <div>
    <router-view/>
    <CloseFull/>
    <Upgrade v-if="needUpdate"/>
  </div>
  <Toast position="top-right" />
  <AppConfirmDialog />
</template>

<script setup lang="ts" name="app">
import {defineAsyncComponent, ref, onMounted, onUnmounted, nextTick, watch} from 'vue';
import {useRoute} from 'vue-router';
import {useI18n} from 'vue-i18n';
import {storeToRefs} from 'pinia';
import {useThemeConfigStore} from './stores/themeConfig';
import other from './utils/other';
import Watermark from './utils/watermark';
import mittBus from './utils/mitt';
// 登录页语言切换/首访浏览器检测的 store 固化入口（见 onMounted）
import {syncLocaleToStore} from './i18n/index';
import Toast from 'primevue/toast';
import AppConfirmDialog from '/@/components/app-confirm-dialog/index.vue';
import {usePrimeVue} from 'primevue/config';
import {useToast} from 'primevue/usetoast';
import {useConfirm} from 'primevue/useconfirm';
import {initMessageService} from './utils/message';
import {getPrimeVueLocale} from './utils/primevue-locale';

// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const CloseFull = defineAsyncComponent(() => import('/@/layout/navigationBars/topBar/CloseFull.vue'));
// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const Upgrade = defineAsyncComponent(() => import('/@/layout/upgrade/index.vue'));

const {locale} = useI18n();
const primeVue = usePrimeVue();
const toast = useToast();
const confirm = useConfirm();
const route = useRoute();
const storesThemeConfig = useThemeConfigStore();
const {themeConfig} = storeToRefs(storesThemeConfig);
const needUpdate = ref(false);

onMounted(() => {
  initMessageService(toast, confirm);
  nextTick(() => {
    if (themeConfig.value.isIsDark) {
      document.documentElement.classList.add('dark');
    }
    setTimeout(() => {
      if (themeConfig.value.isIsDark) document.documentElement.classList.add('dark');
      Watermark.set(themeConfig.value.watermarkText || '');
      // 先把当前 locale（首访浏览器检测/登录页切换的结果）固化进 store，再从 store 读回——保证下方赋值不把检测值打回默认
      syncLocaleToStore();
      locale.value = themeConfig.value.globalI18n;
    }, 100);
  });
});
onUnmounted(() => {
});
watch(
    () => route.path,
    () => {
      other.useTitle();
    },
    {
      deep: true,
    }
);
watch(
    () => locale.value,
    (lang) => {
      primeVue.config.locale = getPrimeVueLocale(lang);
    },
);
</script>
