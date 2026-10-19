// 登录/注册成功后的统一落地：存 token、初始化后端路由、按 redirect 跳转、问候 toast。
// 登录面与注册面共用；注册面传 welcomeText 区分问候语（「欢迎回来」vs「开通成功」）。
import type {RouteLocationNormalizedLoaded, Router} from 'vue-router';
import {local, session} from '../../../utils/storage';
import {ACCESS_TOKEN_KEY, clearTokens} from '../../../utils/axiosUtils';
import {initBackEndControlRoutes} from '../../../router/backEnd';
import {showSuccess, showWarning} from '../../../utils/message';
import {formatAxis} from '../../../utils/formatTime';
import {nextLoading} from '../../../utils/loading';
import {t} from '../../../i18n/index';

export const completeSignIn = async (
    accessToken: string | any,
    route: RouteLocationNormalizedLoaded,
    router: Router,
    welcomeText?: string,
): Promise<void> => {
  local.set(ACCESS_TOKEN_KEY, accessToken);
  session.set('token', accessToken);

  const isNoPower = await initBackEndControlRoutes();
  if (isNoPower) {
    showWarning(t('message.login.account.noPermission'));
    clearTokens();
    return;
  }

  if (route.query?.redirect) {
    await router.push({
      path: <string>route.query?.redirect,
      query: Object.keys(<string>route.query?.params).length > 0 ? JSON.parse(<string>route.query?.params) : '',
    });
  } else {
    await router.push('/');
  }

  const greeting = t(`message.login.greeting.${formatAxis(new Date())}`);
  showSuccess(`${greeting}，${welcomeText ?? t('message.login.signInText')}`);
  nextLoading.start();
};
