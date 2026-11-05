// 登录成功后保存凭据、初始化菜单并进入唯一总览首页。
import type {Router} from 'vue-router';
import {local, session} from '../../../utils/storage';
import {ACCESS_TOKEN_KEY, clearTokens} from '../../../utils/axiosUtils';
import {initBackEndControlRoutes} from '../../../router/backEnd';
import {showSuccess, showWarning} from '../../../utils/message';
import {formatAxis} from '../../../utils/formatTime';
import {nextLoading} from '../../../utils/loading';
import {t} from '../../../i18n/index';
import {useServerStore} from '../../../stores/server';

export const completeSignIn = async (
    accessToken: string | any,
    router: Router,
    welcomeText?: string,
): Promise<void> => {
  useServerStore().resetServers();
  local.set(ACCESS_TOKEN_KEY, accessToken);
  session.set('token', accessToken);

  const isNoPower = await initBackEndControlRoutes();
  if (isNoPower) {
    showWarning(t('message.login.account.noPermission'));
    clearTokens();
    return;
  }

  await router.push('/dashboard/home');

  const greeting = t(`message.login.greeting.${formatAxis(new Date())}`);
  showSuccess(`${greeting}，${welcomeText ?? t('message.login.signInText')}`);
  nextLoading.start();
};
