<template>
  <div class="layout-navbars-breadcrumb-user pr15" style="flex: 1">
    <button type="button" class="layout-navbars-breadcrumb-user-icon user-tool-secondary"
            :title="t('message.user.title0')" :aria-label="t('message.user.title0')"
            :aria-haspopup="'menu'" @click="sizeMenuRef.toggle($event)">
      <i class="fa-solid fa-font"></i>
    </button>
    <Menu ref="sizeMenuRef" :model="sizeMenuItems" :popup="true"/>
    <button type="button" class="layout-navbars-breadcrumb-user-icon user-tool-secondary"
            :title="t('message.user.title1')" :aria-label="t('message.user.title1')"
            :aria-haspopup="'menu'" @click="langMenuRef.toggle($event)">
      <i class="fa-solid fa-language"></i>
    </button>
    <Menu ref="langMenuRef" :model="langMenuItems" :popup="true"/>
    <button type="button" class="layout-navbars-breadcrumb-user-icon"
            :title="t('message.user.title2')" :aria-label="t('message.user.title2')"
            @click="onSearchClick">
      <i class="fa-solid fa-magnifying-glass" style="font-size: 16px"></i>
    </button>
    <button type="button" class="layout-navbars-breadcrumb-user-icon"
            :title="themeConfig.isIsDark ? t('message.layout.fourIsDark') + ' (On)' : t('message.layout.fourIsDark') + ' (Off)'"
            :aria-label="t('message.layout.fourIsDark')" @click="onDarkModeToggle">
      <i :class="themeConfig.isIsDark ? 'fa-solid fa-sun' : 'fa-solid fa-moon'" style="font-size: 16px"></i>
    </button>
    <button type="button" class="layout-navbars-breadcrumb-user-icon"
            :title="t('message.user.title4')" :aria-label="t('message.user.title4')"
            :aria-haspopup="'dialog'" @click="noticePopoverRef.toggle($event)">
      <OverlayBadge severity="danger" v-if="hasUnreadNotice">
        <i class="fa-solid fa-bell" style="font-size: 16px"></i>
      </OverlayBadge>
      <template v-else>
        <i class="fa-solid fa-bell" style="font-size: 16px"></i>
      </template>
    </button>
    <Popover ref="noticePopoverRef">
      <template #container="{ closeCallback }">
        <div class="notice-popover-content" style="width: 300px">
          <UserNews :noticeList="state.noticeList"/>
        </div>
      </template>
    </Popover>
    <button type="button" class="layout-navbars-breadcrumb-user-icon user-tool-secondary"
            :title="state.isScreenfull ? t('message.user.title6') : t('message.user.title5')"
            :aria-label="state.isScreenfull ? t('message.user.title6') : t('message.user.title5')"
            @click="onScreenfullClick">
      <i :class="!state.isScreenfull ? 'fa-solid fa-expand' : 'fa-solid fa-compress'"></i>
    </button>
    <button v-if="isSuperAdmin && targetTenantId"
            type="button"
            class="layout-navbars-breadcrumb-user-target"
            :title="t('message.layout.user.targetTenantActive')"
            :aria-label="`${t('message.layout.user.targetTenantActive')}: ${targetTenantId}`"
            @click="openTargetTenantDialog">
      <i class="fa-solid fa-building-user"></i>
      <span>{{ t('message.layout.user.targetTenantShort') }}: {{ targetTenantId }}</span>
    </button>
    <button type="button" class="layout-navbars-breadcrumb-user-icon"
            :aria-label="userMenuLabel"
            :aria-haspopup="'menu'"
            @click="userMenuRef.toggle($event)">
				<span class="layout-navbars-breadcrumb-user-link">
					<img :src="userInfos.avatar" :alt="userInfos.realName === '' ? userInfos.account : userInfos.realName" class="layout-navbars-breadcrumb-user-link-photo mr5"/>
					<span class="user-realname">{{ userInfos.realName == '' ? userInfos.account : userInfos.realName }}</span>
					<i class="fa-solid fa-chevron-down" style="font-size: 12px"></i>
				</span>
    </button>
    <Menu ref="userMenuRef" :model="userMenuItems" :popup="true"/>
    <Search ref="searchRef"/>
    <Dialog v-model:visible="targetTenantDialogVisible"
            modal
            :header="t('message.layout.user.targetTenantDialogTitle')"
            :style="{ width: 'min(360px, calc(100vw - 32px))' }">
      <div class="target-tenant-dialog">
        <label for="target-tenant-id">{{ t('message.layout.user.targetTenantInputLabel') }}</label>
        <InputText id="target-tenant-id"
                   v-model="targetTenantInput"
                   class="w-full"
                   :placeholder="t('message.layout.user.targetTenantInputPlaceholder')"
                   @keyup.enter="saveTargetTenant"/>
        <small>{{ t('message.layout.user.targetTenantHint') }}</small>
      </div>
      <template #footer>
        <Button v-if="targetTenantId"
                text
                severity="secondary"
                :label="t('message.layout.user.targetTenantExit')"
                @click="clearTargetTenant"/>
        <Button text
                severity="secondary"
                :label="t('message.common.button.cancel')"
                @click="targetTenantDialogVisible = false"/>
        <Button :label="t('message.common.button.confirm')" @click="saveTargetTenant"/>
      </template>
    </Dialog>
  </div>
</template>

<script setup lang="ts" name="layoutBreadcrumbUser">
import {defineAsyncComponent, ref, computed, reactive, onMounted} from 'vue';
import {useRouter} from 'vue-router';
import {showConfirm, showSuccess, showWarning, showMessage} from '../../../utils/message';
import screenfull from 'screenfull';
import {storeToRefs} from 'pinia';
import {useUserInfoStore} from '../../../stores/userInfo';
import {useThemeConfigStore} from '../../../stores/themeConfig';
import other from '../../../utils/other';

import {clearAccessTokens} from '../../../utils/axiosUtils';
import {systemAuthLogoutPost} from '../../../api/systemAuth';
import {tenantNoticeUnReadListGet} from '../../../api/tenantNotice';

import {startConnection} from '../../../utils/onlineUserSignalR';

import Menu from 'primevue/menu';
import Popover from 'primevue/popover';
import OverlayBadge from 'primevue/overlaybadge';
import Dialog from 'primevue/dialog';
import InputText from 'primevue/inputtext';
import Button from 'primevue/button';
import {
  clearTargetTenantId,
  getTargetTenantId,
  normalizeTargetTenantId,
  setTargetTenantId,
} from '../../../utils/targetTenant';

// 引入组件
// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const UserNews = defineAsyncComponent(() => import('/@/layout/navigationBars/topBar/UserNews.vue'));
// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const Search = defineAsyncComponent(() => import('/@/layout/navigationBars/topBar/GlobalSearch.vue'));

// 定义变量内容

const router = useRouter();
const stores = useUserInfoStore();
const storesThemeConfig = useThemeConfigStore();
const {userInfos, isSuperAdmin} = storeToRefs(stores);
const {themeConfig} = storeToRefs(storesThemeConfig);
const searchRef = ref();
const sizeMenuRef = ref();
const langMenuRef = ref();
const userMenuRef = ref();
const noticePopoverRef = ref();
const targetTenantDialogVisible = ref(false);
const targetTenantId = ref(getTargetTenantId());
const targetTenantInput = ref(targetTenantId.value);
const state = reactive({
  isScreenfull: false,
  disabledI18n: 'zh-cn',
  disabledSize: 'large',
  noticeList: [] as any,
});

const sizeMenuItems = computed(() => [
  {
    label: t('message.user.dropdownLarge'),
    command: () => onComponentSizeChange('large'),
    disabled: state.disabledSize === 'large',
  },
  {
    label: t('message.user.dropdownDefault'),
    command: () => onComponentSizeChange('default'),
    disabled: state.disabledSize === 'default',
  },
  {
    label: t('message.user.dropdownSmall'),
    command: () => onComponentSizeChange('small'),
    disabled: state.disabledSize === 'small',
  },
]);

const langMenuItems = computed(() => [
  {label: t('message.layout.user.simplifiedChinese'), command: () => onLanguageChange('zh-cn'), disabled: state.disabledI18n === 'zh-cn'},
  {label: 'English', command: () => onLanguageChange('en'), disabled: state.disabledI18n === 'en'},
  {label: t('message.layout.user.traditionalChinese'), command: () => onLanguageChange('zh-tw'), disabled: state.disabledI18n === 'zh-tw'},
  {label: t('message.layout.user.japanese'), command: () => onLanguageChange('ja'), disabled: state.disabledI18n === 'ja'},
  {label: t('message.layout.user.korean'), command: () => onLanguageChange('ko'), disabled: state.disabledI18n === 'ko'},
]);

// 用户菜单触发器的读屏名称：可见用户名即名称来源，叠加菜单首项「个人中心」语义（C248 键盘/读屏可达）
const userMenuLabel = computed(() => {
  const displayName = userInfos.value.realName == '' ? userInfos.value.account : userInfos.value.realName;
  return `${displayName} (${t('message.user.dropdown2')})`;
});

const userMenuItems = computed(() => {
  const items = [
    {label: t('message.user.dropdown2'), command: () => router.push('/tenant/system/userCenter')},
  ];

  if (isSuperAdmin.value) {
    items.push({
      label: targetTenantId.value
        ? `${t('message.layout.user.targetTenantActive')}: ${targetTenantId.value}`
        : t('message.layout.user.targetTenantSet'),
      command: openTargetTenantDialog,
    });

    if (targetTenantId.value) {
      items.push({
        label: t('message.layout.user.targetTenantExit'),
        command: clearTargetTenant,
      });
    }
  }

  items.push({label: t('message.user.dropdown5'), command: () => onHandleCommandClick('logOut')});
  return items;
});

// 是否有未读消息
const hasUnreadNotice = computed(() => {
  return state.noticeList.some((r: any) => r.readStatus == undefined || r.readStatus == 0);
});
// 全屏点击时
const onScreenfullClick = () => {
  if (!screenfull.isEnabled) {
    showWarning(t('message.layout.user.fullscreenNotSupported'));
    return false;
  }
  screenfull.toggle();
  screenfull.on('change', () => {
    if (screenfull.isFullscreen) state.isScreenfull = true;
    else state.isScreenfull = false;
  });
};
// 下拉菜单点击时
const onHandleCommandClick = async (path: string) => {
  if (path === 'logOut') {
    const confirmed = await showConfirm(t('message.user.logOutMessage'), t('message.user.logOutTitle'));
    if (confirmed) {
      await systemAuthLogoutPost();
      clearAccessTokens();
    }
  } else {
    router.push(path);
  }
};

const openTargetTenantDialog = () => {
  targetTenantInput.value = targetTenantId.value;
  targetTenantDialogVisible.value = true;
};

const saveTargetTenant = () => {
  const normalized = normalizeTargetTenantId(targetTenantInput.value);
  if (!normalized) {
    showWarning(t('message.layout.user.targetTenantInvalid'));
    return;
  }

  setTargetTenantId(normalized);
  targetTenantId.value = normalized;
  targetTenantDialogVisible.value = false;
  showSuccess(t('message.layout.user.targetTenantSaved'));
  window.location.reload();
};

const clearTargetTenant = () => {
  clearTargetTenantId();
  targetTenantId.value = '';
  targetTenantInput.value = '';
  targetTenantDialogVisible.value = false;
  showSuccess(t('message.layout.user.targetTenantCleared'));
  window.location.reload();
};

// 菜单搜索点击
const onSearchClick = () => {
  searchRef.value.openSearch();
};
// 暗色模式切换
const onDarkModeToggle = () => {
  themeConfig.value.isIsDark = !themeConfig.value.isIsDark;
  const html = document.documentElement;
  if (themeConfig.value.isIsDark) html.classList.add('dark');
  else html.classList.remove('dark');
};
// 组件大小改变
const onComponentSizeChange = (size: string) => {
  themeConfig.value.globalComponentSize = size;
  initI18nOrSize('globalComponentSize', 'disabledSize');
  window.location.reload();
};
// 语言切换
const onLanguageChange = (lang: string) => {
  themeConfig.value.globalI18n = lang;
  locale.value = lang;
  other.useTitle();
  initI18nOrSize('globalI18n', 'disabledI18n');
};
// 初始化组件大小/i18n
const initI18nOrSize = (value: string, attr: string) => {
  (<any>state)[attr] = (<any>themeConfig.value)[value];
};
// 页面加载时
onMounted(async () => {
  initI18nOrSize('globalComponentSize', 'disabledSize');
  initI18nOrSize('globalI18n', 'disabledI18n');

  // 加载未读的站内信
  var res = await tenantNoticeUnReadListGet();
  state.noticeList = res.data.data ?? [];

  // 接收站内信
  const conn = await startConnection();
  conn.on('PublicNotice', receiveNotice);
});

const receiveNotice = (msg: any) => {
  state.noticeList.unshift(msg);

  showMessage(t('message.layout.user.notificationMessage'), 'info');
};
</script>

<style scoped lang="scss">
.layout-navbars-breadcrumb-user {
  display: flex;
  align-items: center;
  justify-content: flex-end;

  &-link {
    height: 100%;
    display: flex;
    align-items: center;
    white-space: nowrap;

    &-photo {
      width: 25px;
      height: 25px;
      border-radius: 100%;
    }
  }

  &-icon {
    padding: 0 10px;
    cursor: pointer;
    color: var(--p-text-color);
    height: 100%;
    display: flex;
    align-items: center;
    // 触发器由 div 换原生 button（C248 键盘可达），抹平 UA 默认外观
    border: 0;
    background: transparent;
    font: inherit;

    &:hover {
      background: var(--p-content-hover-background);

      i {
        display: inline-block;
        animation: logoAnimation 0.3s ease-in-out;
      }
    }

    // 按压反馈（GFX-661）：缩放动画受全局 prefers-reduced-motion 豁免
    &:active {
      background: var(--p-content-border-color);
      transform: scale(0.96);
    }

    &:focus-visible {
      outline: 2px solid var(--p-primary-color);
      outline-offset: -2px;
    }
  }

  &-target {
    max-width: 220px;
    height: 28px;
    padding: 0 10px;
    margin-right: 6px;
    border: 1px solid var(--p-primary-color);
    border-radius: 14px;
    color: var(--p-primary-color);
    display: flex;
    align-items: center;
    gap: 6px;
    cursor: pointer;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    // 由 div 换原生 button（C248 键盘可达），字体沿用全局文本
    background: transparent;
    font: inherit;

    // 按压反馈（GFX-661）
    &:active {
      background: var(--p-primary-100);
      transform: scale(0.96);
    }

    &:focus-visible {
      outline: 2px solid var(--p-primary-color);
      outline-offset: 2px;
    }
  }
}

.target-tenant-dialog {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

// 手机端隐藏低频工具（组件大小/语言/全屏）与用户名，为折叠按钮腾出空间（C143，阈值见 theme/breakpoints.scss）
@include bp-down(sm) {
  .user-tool-secondary {
    display: none;
  }

  .user-realname {
    display: none;
  }
}
</style>
