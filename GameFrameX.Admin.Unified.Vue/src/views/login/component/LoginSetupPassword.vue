<template>
  <!-- 首装初始化面（同一张操作员卡的专用面）：首次安装时为平台管理员 superadmin 设置登录密码，
       设置成功即自动进入系统。服务端 superAdminPasswordConfigured=true 时本面不可达（由 index.vue 控制切换）。 -->
  <form class="login-form" novalidate @submit.prevent="onSetup">
    <p class="login-setup-intro">{{ t('message.login.setup.introHint') }}</p>

    <div class="login-field" :class="{'is-error': !!state.errors.password}">
      <label class="login-label" for="setup-password">{{ t('message.login.setup.passwordLabel') }}</label>
      <div class="login-input-mark">
        <Password
            ref="passwordRef"
            v-model="state.ruleForm.password"
            :inputId="'setup-password'"
            :feedback="false"
            toggleMask
            size="large"
            class="login-input"
            autocomplete="new-password"
            :invalid="!!state.errors.password"
            :placeholder="t('message.login.setup.passwordLabel')"
            aria-describedby="setup-password-error"
        />
      </div>
      <p v-if="state.errors.password" id="setup-password-error" class="login-error" role="alert">{{ state.errors.password }}</p>
    </div>

    <div class="login-field" :class="{'is-error': !!state.errors.confirmPassword}">
      <label class="login-label" for="setup-confirm">{{ t('message.login.setup.confirmLabel') }}</label>
      <div class="login-input-mark">
        <Password
            ref="confirmRef"
            v-model="state.ruleForm.confirmPassword"
            :inputId="'setup-confirm'"
            :feedback="false"
            toggleMask
            size="large"
            class="login-input"
            autocomplete="new-password"
            :invalid="!!state.errors.confirmPassword"
            :placeholder="t('message.login.setup.confirmLabel')"
            aria-describedby="setup-confirm-error"
        />
      </div>
      <p v-if="state.errors.confirmPassword" id="setup-confirm-error" class="login-error" role="alert">{{ state.errors.confirmPassword }}</p>
    </div>

    <Button type="submit" size="large" class="login-submit" :loading="state.loading.setup">
      {{ t('message.login.setup.submitText') }}
    </Button>

    <p class="login-forgot">
      {{ t('message.login.setup.backHint') }}
      <button type="button" class="login-switch" @click="emit('switch')">{{ t('message.login.setup.backLink') }}</button>
    </p>
  </form>
</template>

<script lang="ts" setup name="loginSetupPassword">
import {reactive, ref} from 'vue';
import {useRoute, useRouter} from 'vue-router';
import {t} from '../../../i18n/index';
import {feature} from '../../../utils/axiosUtils';
import {showError} from '../../../utils/message';
import {systemAuthSetupSuperAdminPasswordPost} from '../../../api/systemAuth';

import Password from 'primevue/password';
import Button from 'primevue/button';
import {completeSignIn} from '/@/views/login/component/loginSuccess';

const emit = defineEmits<{ (e: 'switch'): void }>();

const route = useRoute();
const router = useRouter();

const passwordRef = ref<any>(null);
const confirmRef = ref<any>(null);

const state = reactive({
  ruleForm: {
    password: '',
    confirmPassword: '',
  },
  errors: {
    password: '',
    confirmPassword: '',
  },
  loading: {
    setup: false,
  },
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

const validateForm = (): boolean => {
  // 与后端最小长度一致（6 位）；平台强密码策略开启时由服务端正则做二次校验
  state.errors.password = state.ruleForm.password.length >= 6 ? '' : t('message.login.setup.passwordMin');
  state.errors.confirmPassword = state.errors.password === '' && state.ruleForm.confirmPassword === state.ruleForm.password
      ? ''
      : t('message.login.setup.confirmMismatch');

  if (state.errors.password) {
    focusInput(passwordRef.value);
    return false;
  }
  if (state.errors.confirmPassword) {
    focusInput(confirmRef.value);
    return false;
  }
  return true;
};

const setup = async () => {
  try {
    state.loading.setup = true;
    const [err, res] = await feature(systemAuthSetupSuperAdminPasswordPost({
      password: state.ruleForm.password,
      confirmPassword: state.ruleForm.confirmPassword,
    }));
    if (err) {
      return;
    }
    if (res.data.data?.accessToken == undefined) {
      showError(t('message.login.setup.setupFailed'));
      return;
    }
    // 设置成功即自动登录（首装零门槛：不回登录页再登一次）
    await completeSignIn(res.data.data?.accessToken, route, router);
  } finally {
    state.loading.setup = false;
  }
};

const onSetup = () => {
  if (!validateForm()) return;
  setup();
};
</script>

<style lang="scss" scoped>
@use './loginForm' as *;

// 字段词汇（label/四角标记/错误行/提交按钮等）与登录/注册面共享 loginForm mixin，此处只留初始化面特有样式
@include login-form;

.login-form {
  // 首装指引行：与 mono 数据边际同族的说明文字，不与字段 label 竞争层级
  .login-setup-intro {
    margin: -6px 0 0;
    font-size: 12.5px;
    line-height: 1.6;
    color: var(--p-text-muted-color);
  }

  // 底部返回链接：与注册面同款（flex 基线对齐 + login-switch 文字链）
  .login-forgot {
    display: flex;
    align-items: baseline;
    gap: 4px;
  }

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
</style>
