<template>
  <form class="login-form" novalidate @submit.prevent="onSignIn">
    <div class="login-field" :class="{'is-error': !!state.errors.account}">
      <label class="login-label" for="login-account">{{ t('message.login.account.accountLabel') }}</label>
      <div class="login-input-mark">
        <InputText
            id="login-account"
            ref="accountRef"
            v-model="state.ruleForm.account"
            type="text"
            size="large"
            class="login-input"
            autocomplete="username"
            :invalid="!!state.errors.account"
            :placeholder="t('message.login.account.accountPlaceholder')"
            aria-describedby="login-account-error"
        />
      </div>
      <p v-if="state.errors.account" id="login-account-error" class="login-error" role="alert">{{ state.errors.account }}</p>
    </div>

    <div class="login-field" :class="{'is-error': !!state.errors.password}">
      <label class="login-label" for="login-password">{{ t('message.login.account.passwordLabel') }}</label>
      <div class="login-input-mark">
        <Password
            ref="passwordRef"
            v-model="state.ruleForm.password"
            :inputId="'login-password'"
            :feedback="false"
            toggleMask
            size="large"
            class="login-input"
            autocomplete="current-password"
            :invalid="!!state.errors.password"
            :placeholder="t('message.login.account.passwordPlaceholder')"
            aria-describedby="login-password-error"
        />
      </div>
      <p v-if="state.errors.password" id="login-password-error" class="login-error" role="alert">{{ state.errors.password }}</p>
    </div>

    <div v-if="state.captchaEnabled" class="login-field" :class="{'is-error': !!state.errors.code}">
      <label class="login-label" for="login-code">{{ t('message.login.account.codeLabel') }}</label>
      <div class="login-code-row">
        <div class="login-input-mark login-code-input">
          <InputText
              id="login-code"
              ref="codeRef"
              v-model="state.ruleForm.code"
              type="text"
              maxlength="4"
              size="large"
              class="login-input"
              autocomplete="off"
              :invalid="!!state.errors.code"
              :placeholder="t('message.login.account.accountPlaceholder3')"
              aria-describedby="login-code-error"
          />
        </div>
        <button class="login-code-img" type="button" :aria-label="t('message.login.account.captchaRefresh')" @click="getCaptcha()">
          <img width="130" height="40" :src="state.captchaImage" :alt="t('message.login.account.captchaImageAlt')"/>
        </button>
      </div>
      <p v-if="state.errors.code" id="login-code-error" class="login-error" role="alert">{{ state.errors.code }}</p>
    </div>

    <div v-if="state.secondVerEnabled" class="login-field" :class="{'is-error': !!state.errors.verify}">
      <span class="login-label" id="login-verify-label">{{ t('message.login.slideVerify.label') }}</span>
      <LoginSlideVerify v-model="state.verified" ref="slideRef" :invalid="!!state.errors.verify" :describedBy="'login-verify-error'"/>
      <p v-if="state.errors.verify" id="login-verify-error" class="login-error" role="alert">{{ state.errors.verify }}</p>
    </div>

    <Button type="submit" size="large" class="login-submit" :loading="state.loading.signIn">
      {{ t('message.login.account.accountBtnText') }}
    </Button>

    <p class="login-forgot">{{ t('message.login.forgotHint') }}</p>
  </form>
</template>

<script lang="ts" setup name="loginAccount">
import {onMounted, reactive, ref} from 'vue';
import {useRoute, useRouter} from 'vue-router';
import {showError} from '../../../utils/message';
import {t} from '../../../i18n/index';

import {feature} from '../../../utils/axiosUtils';
import {systemAuthCaptchaGet, systemAuthLoginPost} from '../../../api/systemAuth';

import InputText from 'primevue/inputtext';
import Password from 'primevue/password';
import Button from 'primevue/button';
import LoginSlideVerify from '/@/views/login/component/LoginSlideVerify.vue';
import {completeSignIn} from '/@/views/login/component/loginSuccess';

// 登录配置由卡壳（index.vue）统一拉取后下发：严格跟随服务端，前端不兜底默认开启
const props = defineProps<{
  config: { secondVerEnabled: boolean; captchaEnabled: boolean };
}>();

const route = useRoute();
const router = useRouter();

const accountRef = ref<any>(null);
const passwordRef = ref<any>(null);
const codeRef = ref<any>(null);
const slideRef = ref<InstanceType<typeof LoginSlideVerify> | null>(null);

const state = reactive({
  ruleForm: {
    account: '',
    password: '',
    code: '',
    codeId: 0,
  },
  errors: {
    account: '',
    password: '',
    code: '',
    verify: '',
  },
  loading: {
    signIn: false,
  },
  captchaImage: '',
  captchaEnabled: false,
  secondVerEnabled: false,
  verified: false,
});

// 服务端返回的 base64 图片嗅探正确的 MIME（原实现一律声明 text/html，靠浏览器容错渲染）
const sniffCaptchaMime = (base64: string): string => {
  if (base64.startsWith('iVBOR')) return 'image/png';
  if (base64.startsWith('PHN2')) return 'image/svg+xml';
  if (base64.startsWith('/9j/')) return 'image/jpeg';
  return 'text/html';
};

const focusInput = (input: any) => {
  const el: HTMLElement | undefined = input?.$el;
  if (!el) {
    return;
  }
  if (el instanceof HTMLInputElement) {
    el.focus();
  } else {
    el.querySelector('input')?.focus();
  }
};

const validateForm = (): boolean => {
  state.errors.account = state.ruleForm.account.trim() ? '' : t('message.login.account.accountRequired');
  state.errors.password = state.ruleForm.password ? '' : t('message.login.account.passwordRequired');
  state.errors.code = !state.captchaEnabled || state.ruleForm.code.trim() ? '' : t('message.login.account.codeRequired');
  state.errors.verify = !state.secondVerEnabled || state.verified ? '' : t('message.login.account.verifyRequired');

  if (state.errors.account) {
    focusInput(accountRef.value);
    return false;
  }
  if (state.errors.password) {
    focusInput(passwordRef.value);
    return false;
  }
  if (state.errors.code) {
    focusInput(codeRef.value);
    return false;
  }
  if (state.errors.verify) {
    slideRef.value?.focus();
    return false;
  }
  return true;
};

onMounted(async () => {
  const accessToken = route.query.token;
  if (accessToken != null && accessToken !== undefined) {
    await saveTokenAndInitRoutes(accessToken);
  }

  // 严格跟随服务端配置：前端不兜底默认开启（原实现 secondVerEnabled ?? true 会在配置缺失时强制开启二次校验）
  state.secondVerEnabled = props.config.secondVerEnabled === true;
  state.captchaEnabled = props.config.captchaEnabled === true;

  getCaptcha();
});

const getCaptcha = async (resetCode = false) => {
  if (!state.captchaEnabled) return;

  // 仅在验证码确实失效时清空（失败重试后旧码已消费）；日常刷新不打断已输入内容
  if (resetCode) {
    state.ruleForm.code = '';
  }
  const res = await systemAuthCaptchaGet();
  const img = res.data.data?.img ?? '';
  state.captchaImage = `data:${sniffCaptchaMime(img)};base64,${img}`;
  state.ruleForm.codeId = res.data.data?.id;
};

const onLoginFailure = () => {
  if (state.captchaEnabled) {
    getCaptcha(true);
  }
  if (state.secondVerEnabled) {
    slideRef.value?.reset();
    state.verified = false;
  }
};

const signIn = async () => {
  try {
    state.loading.signIn = true;

    const loginInput = {
      ...state.ruleForm,
      account: state.ruleForm.account.trim(),
    };
    const [err, res] = await feature(systemAuthLoginPost(loginInput));
    if (err) {
      onLoginFailure();
      return;
    }
    if (res.data.data?.accessToken == undefined) {
      onLoginFailure();
      showError(t('message.login.account.loginFailed'));
      return;
    }
    await saveTokenAndInitRoutes(res.data.data?.accessToken);
  } finally {
    state.loading.signIn = false;
  }
};

// 落地流程（存 token → 初始化路由 → redirect → 问候）与注册面共用 loginSuccess.completeSignIn
const saveTokenAndInitRoutes = async (accessToken: string | any) => {
  await completeSignIn(accessToken, route, router);
};

const onSignIn = () => {
  if (!validateForm()) return;
  signIn();
};

defineExpose({saveTokenAndInitRoutes});
</script>

<style lang="scss" scoped>
@use './loginForm' as *;

// 字段词汇（label/四角标记/错误行/提交按钮等）与注册面共享 loginForm mixin，此处只留登录面特有样式
@include login-form;

.login-form {
  // 图形验证码图按钮：132px 格，等高拉伸对齐标记框
  .login-code-img {
    flex: 0 0 132px;
    min-height: 44px;
    padding: 0;
    border: 0;
    background: transparent;
    cursor: pointer;

    img {
      width: 100%;
      height: 44px;
      object-fit: cover;
      border: 1px solid var(--p-content-border-color);
      border-radius: 6px;
      transition: border-color 0.15s ease;
    }

    &:hover img {
      border-color: var(--p-inputtext-hover-border-color);
    }
  }
}

@include bp-down(sm) {
  .login-form {
    .login-code-img {
      flex-basis: auto;
      width: 100%;
    }
  }
}
</style>
