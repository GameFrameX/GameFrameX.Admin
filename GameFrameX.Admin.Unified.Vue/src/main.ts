import {createApp} from 'vue';
import pinia from './stores/index';
import App from '/@/App.vue';
import router from './router';
import {directive} from './directive/index';
import {i18n} from './i18n/index';
import other from './utils/other';
import './utils/fontawesome';

import PrimeVue from 'primevue/config';
import Aura from '@primevue/themes/aura';
import {definePreset} from '@primevue/themes';
import ToastService from 'primevue/toastservice';
import ConfirmationService from 'primevue/confirmationservice';
import 'primeflex/primeflex.css';
import 'primeicons/primeicons.css';
import '/@/theme/primevue-overrides.scss';
import '/@/theme/common/transition.scss';
import {getPrimeVueLocale} from './utils/primevue-locale';
import './utils/primevue-inputnumber-focus-guard';
import {brandPrimaryPalette} from './theme/brand';

import VueGridLayout from 'vue-grid-layout';

import '@vue-js-cron/light/dist/light.css';
import CronLightPlugin from '@vue-js-cron/light';

import VueSignaturePad from 'vue-signature-pad'; // 电子签名
import vue3TreeOrg from 'vue3-tree-org'; // 组织架构图
import 'vue3-tree-org/lib/vue3-tree-org.css'; // 组织架构图样式

// 品牌层：基于 Aura 派生 preset，主色替换为品牌蓝（logo 采样 #03538F 的 10 阶色板，C254）
// eslint-disable-next-line @typescript-eslint/naming-convention -- 存量命名空间式单例/主题预设导出，PascalCase 为既有约定
const GameFrameXPreset = definePreset(Aura, {
  semantic: {
    primary: brandPrimaryPalette,
  },
});

const primeVueTheme = {
  theme: {
    preset: GameFrameXPreset,
    options: {
      darkModeSelector: '.dark',
      cssLayer: false,
    },
  },
  locale: getPrimeVueLocale('zh-cn'),
};

const app = createApp(App);

// Suppress known PrimeVue internal warnings
app.config.warnHandler = (msg) => {
  if (msg.includes('outsideClickListener') || msg.includes('resizeListener')) return
  if (msg.includes('onMounted is called when there is no active component instance') && msg.includes('UseStyle')) return
  console.warn(msg)
}

directive(app);
other.elSvg(app);

app
  .use(pinia)
  .use(router)
  .use(i18n)
  .use(PrimeVue, primeVueTheme)
  .use(ToastService)
  .use(ConfirmationService)
  .use(CronLightPlugin)
  .use(VueGridLayout)
  .use(VueSignaturePad)
  .use(vue3TreeOrg)
  .mount('#app');
