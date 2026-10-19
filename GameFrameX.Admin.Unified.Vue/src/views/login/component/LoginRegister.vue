<template>
  <!-- 注册面（同一张操作员卡的背面）：自助开通工作室——填卡、邮箱盖章验证、开卡即进入。
       服务端 emailRegisterEnabled 关闭时本面不可达（入口由 index.vue 严格跟随配置控制）。 -->
  <form class="login-form" novalidate @submit.prevent="onRegister">
    <div class="login-field" :class="{'is-error': !!state.errors.tenantName}">
      <label class="login-label" for="reg-tenant-name">{{ t('message.login.register.tenantNameLabel') }}</label>
      <div class="login-input-mark">
        <InputText
            id="reg-tenant-name"
            v-model="state.ruleForm.tenantName"
            type="text"
            size="large"
            class="login-input"
            autocomplete="organization"
            :invalid="!!state.errors.tenantName"
            :placeholder="t('message.login.register.tenantNamePlaceholder')"
            aria-describedby="reg-tenant-name-error"
        />
      </div>
      <p v-if="state.errors.tenantName" id="reg-tenant-name-error" class="login-error" role="alert">{{ state.errors.tenantName }}</p>
    </div>

    <div class="login-field" :class="{'is-error': !!state.errors.email}">
      <label class="login-label" for="reg-email">{{ t('message.login.register.emailLabel') }}</label>
      <div class="login-input-mark">
        <InputText
            id="reg-email"
            v-model="state.ruleForm.email"
            type="email"
            size="large"
            class="login-input"
            autocomplete="email"
            :invalid="!!state.errors.email"
            :placeholder="t('message.login.register.emailPlaceholder')"
            aria-describedby="reg-email-error"
        />
      </div>
      <p v-if="state.errors.email" id="reg-email-error" class="login-error" role="alert">{{ state.errors.email }}</p>
    </div>

    <div class="login-field" :class="{'is-error': !!state.errors.code}">
      <label class="login-label" for="reg-code">{{ t('message.login.register.codeLabel') }}</label>
      <div class="login-code-row">
        <div class="login-input-mark login-code-input">
          <InputText
              id="reg-code"
              v-model="state.ruleForm.code"
              type="text"
              inputmode="numeric"
              maxlength="6"
              size="large"
              class="login-input"
              autocomplete="one-time-code"
              :invalid="!!state.errors.code"
              :placeholder="t('message.login.register.codePlaceholder')"
              aria-describedby="reg-code-status"
          />
        </div>
        <Button
            type="button"
            size="large"
            class="login-code-send"
            :disabled="state.cooldown > 0 || state.sending"
            @click="onSendCode"
        >
          {{ sendLabel }}
        </Button>
      </div>
      <p v-if="state.errors.code" id="reg-code-status" class="login-error" role="alert">{{ state.errors.code }}</p>
      <p v-else-if="state.codeSentTip" id="reg-code-status" class="login-code-sent" role="status">{{ state.codeSentTip }}</p>
    </div>

    <div class="login-field" :class="{'is-error': !!state.errors.password}">
      <label class="login-label" for="reg-password">{{ t('message.login.register.passwordLabel') }}</label>
      <div class="login-input-mark">
        <Password
            v-model="state.ruleForm.password"
            :inputId="'reg-password'"
            :feedback="false"
            toggleMask
            size="large"
            class="login-input"
            autocomplete="new-password"
            :invalid="!!state.errors.password"
            :placeholder="t('message.login.register.passwordLabel')"
            aria-describedby="reg-password-error"
        />
      </div>
      <p v-if="state.errors.password" id="reg-password-error" class="login-error" role="alert">{{ state.errors.password }}</p>
    </div>

    <div class="login-field" :class="{'is-error': !!state.errors.confirm}">
      <label class="login-label" for="reg-confirm">{{ t('message.login.register.confirmLabel') }}</label>
      <div class="login-input-mark">
        <Password
            v-model="state.ruleForm.confirm"
            :inputId="'reg-confirm'"
            :feedback="false"
            toggleMask
            size="large"
            class="login-input"
            autocomplete="new-password"
            :invalid="!!state.errors.confirm"
            :placeholder="t('message.login.register.confirmLabel')"
            aria-describedby="reg-confirm-error"
        />
      </div>
      <p v-if="state.errors.confirm" id="reg-confirm-error" class="login-error" role="alert">{{ state.errors.confirm }}</p>
    </div>

    <Button type="submit" size="large" class="login-submit" :loading="state.loading.register">
      {{ t('message.login.register.submitText') }}
    </Button>

    <p class="login-forgot">
      {{ t('message.login.register.backHint') }}
      <button type="button" class="login-switch" @click="emit('switch')">{{ t('message.login.register.backLink') }}</button>
    </p>
  </form>
</template>

<script lang="ts" setup name="loginRegister">
import {computed, onUnmounted, reactive, ref} from 'vue';
import {useRoute, useRouter} from 'vue-router';
import {showError} from '../../../utils/message';
import {feature} from '../../../utils/axiosUtils';
import {t} from '../../../i18n/index';
import {systemAuthRegisterByEmailPost, systemAuthSendRegisterEmailCodePost} from '../../../api/systemAuth';

import InputText from 'primevue/inputtext';
import Password from 'primevue/password';
import Button from 'primevue/button';
import {completeSignIn} from '/@/views/login/component/loginSuccess';

const emit = defineEmits<{ (e: 'switch'): void }>();

const route = useRoute();
const router = useRouter();

const tenantNameRef = ref<any>(null);
const emailRef = ref<any>(null);
const codeRef = ref<any>(null);
const passwordRef = ref<any>(null);
const confirmRef = ref<any>(null);

const state = reactive({
  ruleForm: {
    tenantName: '',
    email: '',
    code: '',
    password: '',
    confirm: '',
  },
  errors: {
    tenantName: '',
    email: '',
    code: '',
    password: '',
    confirm: '',
  },
  loading: {
    register: false,
  },
  sending: false,
  cooldown: 0,
  codeSentTip: '',
});

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
let cooldownTimer: ReturnType<typeof setInterval> | null = null;

const sendLabel = computed(() => {
  if (state.cooldown > 0) {
    return t('message.login.register.resend', {s: state.cooldown});
  }
  if (state.sending) {
    return t('message.login.register.sending');
  }
  return t('message.login.register.sendCode');
});

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

const validateEmailField = (): boolean => {
  const email = state.ruleForm.email.trim();
  if (!email) {
    state.errors.email = t('message.login.register.emailRequired');
    focusInput(emailRef.value);
    return false;
  }
  if (!EMAIL_RE.test(email)) {
    state.errors.email = t('message.login.register.emailInvalid');
    focusInput(emailRef.value);
    return false;
  }
  state.errors.email = '';
  return true;
};

const onSendCode = async () => {
  if (!validateEmailField()) {
    return;
  }
  try {
    state.sending = true;
    const [err] = await feature(systemAuthSendRegisterEmailCodePost({email: state.ruleForm.email.trim()}));
    if (err) {
      return;
    }
    // 发送成功：60 秒冷却倒计时 + 行内 status 提示（错误由拦截器统一 toast）
    state.codeSentTip = t('message.login.register.codeSent');
    state.cooldown = 60;
    cooldownTimer = setInterval(() => {
      state.cooldown -= 1;
      if (state.cooldown <= 0) {
        state.cooldown = 0;
        if (cooldownTimer) {
          clearInterval(cooldownTimer);
          cooldownTimer = null;
        }
      }
    }, 1000);
    focusInput(codeRef.value);
  } finally {
    state.sending = false;
  }
};

const validateForm = (): boolean => {
  state.errors.tenantName = state.ruleForm.tenantName.trim() ? '' : t('message.login.register.tenantNameRequired');
  state.errors.email = '';
  state.errors.code = state.ruleForm.code.trim() ? '' : t('message.login.account.codeRequired');
  state.errors.password = state.ruleForm.password.length >= 6 ? '' : t('message.login.register.passwordMin');
  state.errors.confirm = state.ruleForm.confirm === state.ruleForm.password && state.ruleForm.confirm ? '' : t('message.login.register.confirmMismatch');

  if (state.errors.tenantName) {
    focusInput(tenantNameRef.value);
    return false;
  }
  if (!validateEmailField()) {
    return false;
  }
  if (state.errors.code) {
    focusInput(codeRef.value);
    return false;
  }
  if (state.errors.password) {
    focusInput(passwordRef.value);
    return false;
  }
  if (state.errors.confirm) {
    focusInput(confirmRef.value);
    return false;
  }
  return true;
};

const register = async () => {
  try {
    state.loading.register = true;
    const [err, res] = await feature(systemAuthRegisterByEmailPost({
      email: state.ruleForm.email.trim(),
      code: state.ruleForm.code.trim(),
      password: state.ruleForm.password,
      tenantName: state.ruleForm.tenantName.trim(),
    }));
    if (err) {
      return;
    }
    if (res.data.data?.accessToken == undefined) {
      showError(t('message.login.register.registerFailed'));
      return;
    }
    // 注册即登录：拿 accessToken 走与登录面相同的落地流程
    await completeSignIn(res.data.data?.accessToken, route, router, t('message.login.register.welcome'));
  } finally {
    state.loading.register = false;
  }
};

const onRegister = () => {
  if (!validateForm()) return;
  register();
};

onUnmounted(() => {
  if (cooldownTimer) {
    clearInterval(cooldownTimer);
    cooldownTimer = null;
  }
});
</script>

<style scoped lang="scss">
@use './loginForm' as *;

@include login-form;

.login-form {
  // 发送验证码按钮：与登录面验证码图同占 132px 格，等高拉伸对齐标记框
  .login-code-send {
    flex: 0 0 132px;
    border: 1px solid var(--p-primary-200);
    border-radius: 6px;
    background: var(--p-surface-0);
    color: var(--p-primary-color);
    font-size: 13px;
    font-weight: 600;

    &:hover:not(:disabled) {
      border-color: var(--p-primary-300);
      background: var(--p-primary-50);
    }
  }

  .login-code-sent {
    font-size: 12.5px;
    line-height: 1.4;
    color: var(--p-primary-500, var(--p-primary-color));
  }

  .login-forgot {
    display: flex;
    align-items: baseline;
    gap: 4px;
  }

  // 返回登录：文字链，mono 世界里不用下划线，用 primary 色与 hover 变化表达可点
  .login-switch {
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

// 窄屏验证码行纵排：按钮的 flex-basis(132px) 在纵向主轴上会变成高度，须重置回标准按钮
// （与登录面 .login-code-img 的窄屏处理同构）
@include bp-down(sm) {
  .login-form {
    .login-code-send {
      flex-basis: auto;
      width: 100%;
    }
  }
}
</style>
