<template>
  <!--
    DESIGN CONTRACT · impeccable 0b4f57a4 · 纸面剧作卡
    THESIS: 登录是一张纸质「操作员卡」——填字段、盖准入章；桌面是 GameFrameX 的制图纸工作台。拒绝「插画左+玻璃卡右」的类别默认与其黑底霓虹反面。
    OWN-WORLD: 制图纸蓝格网 + 白卡纸 + 品牌蓝墨 schematic 图纸（终端/机架/多端）+ mono 数据边际；颜色全部由 --p-* token 派生。
    STORY: 访客认出「这是给做游戏的人做的工具」，填卡，滑块验证盖「已验证」章，进门回到 token 化后台。
    FIRST VIEWPORT: 左制图桌（logo+副题，三张叠放墨线图纸为主视觉）；右 420px 竖卡：mono 卡号行 → 标题 → 字段 → 验证 → 油墨蓝提交；聚焦字段收四角注册标记。
    FORM: 桌游原型纸感剧作卡（自选清单第 6 位，骰子指派）· seed 0b4f57a4
    REGISTER FACE (2026-08-30 B佬需求): 同一张卡的背面——邮箱验证码自助开通工作室（注册即建租户）。入口仅当服务端 emailRegisterEnabled === true 时露出；沿用同一字段词汇与动效纪律（盖章仍是唯一动效时刻），无弹窗、无新装饰。
    FINISH: unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance.
  -->
  <div class="login-container" data-design-contract="impeccable 0b4f57a4">
    <main class="login-desk">
      <section class="login-desk-side">
        <header class="login-brand">
          <img class="login-brand-logo" :src="logoMini" alt=""/>
          <div class="login-brand-text">
            <span class="login-brand-name">{{ getThemeConfig.globalTitle }}</span>
            <span class="login-brand-tagline">{{ t('message.login.tagline') }}</span>
          </div>
        </header>
        <div class="login-sheets" aria-hidden="true">
          <div class="login-sheet login-sheet-a"><IllustTerminal/></div>
          <div class="login-sheet login-sheet-b"><IllustServer/></div>
          <div class="login-sheet login-sheet-c"><IllustDevices/></div>
        </div>
        <div class="login-desk-strip" aria-hidden="true">
          <span>{{ getThemeConfig.globalViceTitle.toUpperCase() }}</span>
          <span>GITHUB:GAMEFRAMEX</span>
          <span>{{ deskDate }}</span>
        </div>
      </section>
      <section class="login-card-zone">
        <div class="login-card">
          <div class="login-card-meta">
            <span class="login-card-org">{{ getThemeConfig.globalViceTitle }}</span>
            <div class="login-card-meta-side">
              <Select
                  v-model="currentLang"
                  :options="langOptions"
                  optionLabel="label"
                  optionValue="value"
                  class="login-lang"
                  :aria-label="t('message.login.langLabel')"
                  appendTo="body"
                  @update:model-value="onLanguageChange"
              >
                <template #value="{value}">{{ langCodeOf(value) }}</template>
              </Select>
              <span class="login-card-no">{{ cardNoPrefix }} {{ cardNo }}</span>
            </div>
          </div>
          <h1 class="login-card-title">{{ cardTitleText }}</h1>
          <Account v-if="mode === 'signin' && loginConfig" :config="loginConfig"/>
          <LoginRegister v-else-if="mode === 'register'" @switch="switchMode('signin')"/>
          <LoginSetupPassword v-else-if="mode === 'setup'" @switch="switchMode('signin')"/>
          <p v-if="mode === 'signin' && loginConfig && emailRegisterEnabled" class="login-card-alt">
            {{ t('message.login.register.entryHint') }}
            <button type="button" class="login-alt-link" @click="switchMode('register')">{{ t('message.login.register.entryLink') }}</button>
          </p>
          <p v-if="mode === 'signin' && loginConfig && superAdminNeedsSetup" class="login-card-alt">
            {{ t('message.login.setup.entryHint') }}
            <button type="button" class="login-alt-link" @click="switchMode('setup')">{{ t('message.login.setup.entryLink') }}</button>
          </p>
        </div>
      </section>
    </main>
  </div>
</template>

<script setup lang="ts" name="loginIndex">
import {defineAsyncComponent, onMounted, computed, ref} from 'vue';
import {storeToRefs} from 'pinia';
import {useThemeConfigStore} from '../../stores/themeConfig';
import {nextLoading} from '../../utils/loading';
import {t, locale} from '../../i18n/index';
import other from '../../utils/other';
import {systemAuthLoginConfigGet} from '../../api/systemAuth';
import Select from 'primevue/select';
import logoMini from '/@/assets/logo-mini.svg';

import IllustTerminal from '/@/views/login/component/IllustTerminal.vue';
import IllustServer from '/@/views/login/component/IllustServer.vue';
import IllustDevices from '/@/views/login/component/IllustDevices.vue';

// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const Account = defineAsyncComponent(() => import('/@/views/login/component/LoginAccount.vue'));
// eslint-disable-next-line @typescript-eslint/naming-convention -- 同上
const LoginRegister = defineAsyncComponent(() => import('/@/views/login/component/LoginRegister.vue'));
// eslint-disable-next-line @typescript-eslint/naming-convention -- 同上
const LoginSetupPassword = defineAsyncComponent(() => import('/@/views/login/component/LoginSetupPassword.vue'));

const storesThemeConfig = useThemeConfigStore();
const {themeConfig} = storeToRefs(storesThemeConfig);

const getThemeConfig = computed(() => {
  return themeConfig.value;
});

// 语言切换（mono 边际的 LANG 字段）：收起态显示 locale 代码（表单代码字段），浮层显示各语言自称无需翻译；
// 切换写回偏好并同步标题（与顶栏用户菜单同款姿势）
const langOptions = [
  {label: '简体中文', value: 'zh-cn', code: 'ZH-CN'},
  {label: '繁體中文', value: 'zh-tw', code: 'ZH-TW'},
  {label: 'English', value: 'en', code: 'EN'},
  {label: '日本語', value: 'ja', code: 'JA'},
  {label: '한국어', value: 'ko', code: 'KO'},
];
const langCodeOf = (value: unknown): string => {
  return langOptions.find((o) => o.value === value)?.code ?? String(value ?? '').toUpperCase();
};
const currentLang = ref(locale.value);
const onLanguageChange = (lang: string) => {
  themeConfig.value.globalI18n = lang;
  locale.value = lang;
  other.useTitle();
};

const now = new Date();
const deskDate = `${now.getFullYear()}.${String(now.getMonth() + 1).padStart(2, '0')}.${String(now.getDate()).padStart(2, '0')}`;
const rollNo = () => `${String(now.getMonth() + 1).padStart(2, '0')}${String(now.getDate()).padStart(2, '0')}-${Math.random().toString(36).slice(2, 6).toUpperCase()}`;
const cardNo = ref(rollNo());

// 同一张卡的多面：正面登录、背面自助开通、专用面首装初始化超管密码（切换时换表号，制图世界的一张表一个号）
const mode = ref<'signin' | 'register' | 'setup'>('signin');
const switchMode = (next: 'signin' | 'register' | 'setup') => {
  if (mode.value === next) {
    return;
  }
  mode.value = next;
  cardNo.value = rollNo();
};

// 卡头数据边际按面切换：登录表 / 注册登记 / 初始化设置
const cardNoPrefix = computed(() => (mode.value === 'signin' ? 'FORM NO.' : mode.value === 'register' ? 'REG. NO.' : 'INIT NO.'));
const cardTitleText = computed(() => (mode.value === 'signin' ? t('message.login.cardTitle') : mode.value === 'register' ? t('message.login.register.cardTitle') : t('message.login.setup.cardTitle')));

// 登录配置上提统一拉取：严格跟随服务端（=== true 才开启），失败保持全关不兜底
const loginConfig = ref<{ secondVerEnabled: boolean; captchaEnabled: boolean } | null>(null);
const emailRegisterEnabled = ref(false);
// 首装状态：服务端明确上报 false 才需要初始化向导（旧后端/字段缺失 → undefined → 不进向导，安全默认）
const superAdminNeedsSetup = ref(false);

onMounted(async () => {
  nextLoading.done();
  try {
    const res = await systemAuthLoginConfigGet();
    loginConfig.value = {
      secondVerEnabled: res?.data?.data?.secondVerEnabled === true,
      captchaEnabled: res?.data?.data?.captchaEnabled === true,
    };
    emailRegisterEnabled.value = res?.data?.data?.emailRegisterEnabled === true;
    superAdminNeedsSetup.value = res?.data?.data?.superAdminPasswordConfigured === false;
    // 首装零门槛：超管密码未初始化时直接落到初始化面（租户用户可点「返回登录」正常登录）
    if (superAdminNeedsSetup.value) {
      switchMode('setup');
    }
  } catch {
    loginConfig.value = {secondVerEnabled: false, captchaEnabled: false};
  }
});
</script>

<style scoped lang="scss">
// 纸面剧作卡世界：制图纸桌面 + 操作员卡。所有颜色由 PrimeVue token 派生，亮暗双主题自动映射。
.login-container {
  // desk/card 用会翻转的 content-background 派生（与外壳 SplitterLayout 同一张桌面）：
  // raw surface-50/0 暗色下不翻（#fafafa/#ffffff），旧公式暗色桌面近白、卡片纯白对比度崩
  --desk-paper: color-mix(in srgb, var(--p-content-background) 90%, var(--p-primary-400));
  --desk-grid: color-mix(in srgb, var(--p-primary-200) 38%, transparent);
  --desk-grid-major: color-mix(in srgb, var(--p-primary-200) 62%, transparent);
  --card-paper: var(--p-content-background);
  --sheet-shadow: 0 8px 22px color-mix(in srgb, var(--p-surface-900) 10%, transparent);
  --card-shadow: 0 22px 60px color-mix(in srgb, var(--p-surface-900) 13%, transparent);
  --mono-stack: ui-monospace, 'SF Mono', SFMono-Regular, Menlo, Consolas, 'Liberation Mono', monospace;

  min-height: 100dvh;
  position: relative;
  background-color: var(--desk-paper);
  background-image:
    linear-gradient(var(--desk-grid) 1px, transparent 1px),
    linear-gradient(90deg, var(--desk-grid) 1px, transparent 1px),
    linear-gradient(var(--desk-grid-major) 1px, transparent 1px),
    linear-gradient(90deg, var(--desk-grid-major) 1px, transparent 1px);
  background-size: 24px 24px, 24px 24px, 120px 120px, 120px 120px;
  overflow-y: auto;

  &::selection,
  ::selection {
    background: var(--p-primary-100);
  }

  .login-desk {
    min-height: 100dvh;
    width: min(1120px, 100%);
    margin: 0 auto;
    padding: 48px 40px;
    display: flex;
    align-items: center;
    gap: 64px;
  }

  .login-desk-side {
    flex: 1 1 0;
    min-width: 0;
    min-height: 560px;
    display: flex;
    flex-direction: column;
    gap: 28px;
  }

  .login-brand {
    display: flex;
    align-items: center;
    gap: 14px;

    .login-brand-logo {
      width: 52px;
      height: 52px;
      flex: 0 0 auto;
    }

    .login-brand-text {
      display: flex;
      flex-direction: column;
      gap: 2px;

      .login-brand-name {
        font-size: 26px;
        font-weight: 800;
        line-height: 1.15;
        letter-spacing: -0.01em;
        color: var(--p-text-color);
      }

      .login-brand-tagline {
        font-size: 13px;
        color: var(--p-text-muted-color);
      }
    }
  }

  .login-sheets {
    position: relative;
    flex: 1 1 auto;
    min-height: 320px;

    .login-sheet {
      position: absolute;
      background: var(--card-paper);
      border-radius: 4px;
      box-shadow: var(--sheet-shadow);
      padding: 10px;
    }

    .login-sheet-a {
      width: 56%;
      left: 2%;
      top: 4%;
      transform: rotate(-2.4deg);
      z-index: 1;
    }

    .login-sheet-b {
      width: 44%;
      right: 0;
      top: 14%;
      transform: rotate(1.8deg);
      z-index: 2;
    }

    .login-sheet-c {
      width: 40%;
      left: 14%;
      bottom: 0;
      transform: rotate(-1.2deg);
      z-index: 3;
    }
  }

  .login-desk-strip {
    display: flex;
    gap: 28px;
    font-family: var(--mono-stack);
    font-size: 11px;
    letter-spacing: 0.08em;
    // 门面文字走完整 muted token（72% 透明混色在浅色桌面纸上仅 ~2.9:1）；安静感交给字号与字距
    color: var(--p-text-muted-color);
    user-select: none;
  }

  .login-card-zone {
    flex: 0 0 420px;
    width: 420px;
    max-width: 100%;
    position: relative;
    z-index: 2;
  }

  .login-card {
    // 操作员卡：卡头与表单同在一张卡纸上，阴影与圆角只在容器声明一次
    background: var(--card-paper);
    border-radius: 14px;
    box-shadow: var(--card-shadow);
    padding: 34px 36px 26px;

    .login-card-meta {
      display: flex;
      justify-content: space-between;
      align-items: baseline;
      gap: 10px;
      font-family: var(--mono-stack);
      font-size: 11px;
      letter-spacing: 0.08em;
      color: var(--p-text-muted-color);

      // 组织名是弹性段：viceTitle 过长时省略号，保右侧表号/语言字段完整
      .login-card-org {
        flex: 1 1 auto;
        min-width: 0;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
      }

      .login-card-meta-side {
        flex: 0 0 auto;
        display: flex;
        align-items: baseline;
        gap: 8px;
      }

      // 语言字段：无框 mono 下拉，与 FORM NO. 同一行数据边际词汇。
      // 内部 label padding 与下拉箭头区压到最小，收起态收敛为一个 locale 代码字段的宽度
      .login-lang {
        height: 18px;
        min-height: 18px;
        padding: 0 14px 0 0;
        border: 0;
        background: transparent;
        font-family: var(--mono-stack);
        font-size: 11px;
        letter-spacing: 0.08em;
        color: var(--p-text-muted-color);
        box-shadow: none;
        // 18px 控件盒对 16.5px mono 文字行做基线对齐会上下不对称（上溢 7px/下差 5.5px），改垂直居中（偏移仅 0.75px）
        align-self: center;

        :deep(.p-select-label) {
          padding: 0;
        }

        :deep(.p-select-dropdown) {
          width: 14px;
        }

        &:hover:not(.p-disabled) {
          border-color: transparent;
          color: var(--p-text-color);
        }

        &:focus {
          box-shadow: none;
        }
      }

      .login-card-no {
        white-space: nowrap;
      }
    }

    .login-card-title {
      margin-top: 10px;
      margin-bottom: 22px;
      font-size: 26px;
      font-weight: 800;
      line-height: 1.2;
      letter-spacing: -0.01em;
      color: var(--p-text-color);
    }

    // 自助开通入口：卡片级 footer，仅服务端开启时存在
    .login-card-alt {
      display: flex;
      align-items: baseline;
      gap: 4px;
      margin-top: -6px;
      font-size: 12px;
      color: var(--p-text-muted-color);
    }

    .login-alt-link {
      padding: 0;
      border: 0;
      background: transparent;
      font-size: 12px;
      font-weight: 600;
      color: var(--p-primary-color);
      cursor: pointer;

      &:hover {
        color: var(--p-primary-600, var(--p-primary-color));
      }
    }
  }
}

@include bp-down(md) {
  .login-container {
    .login-desk {
      padding: 24px 16px;
      align-items: flex-start;
    }

    .login-desk-side {
      display: none;
    }

    .login-card-zone {
      flex: 1 1 auto;
      width: 100%;
      margin-top: calc(50dvh - 260px);
    }

    .login-card {
      padding-left: 24px;
      padding-right: 24px;
    }
  }
}

@media (prefers-reduced-motion: reduce) {
  .login-container {
    *,
    *::before,
    *::after {
      transition-duration: 0.01ms !important;
      animation-duration: 0.01ms !important;
      animation-iteration-count: 1 !important;
    }
  }
}
</style>
