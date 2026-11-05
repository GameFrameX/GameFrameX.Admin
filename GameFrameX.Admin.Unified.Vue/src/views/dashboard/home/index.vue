<template>
  <div class="dashboard-home layout-pd">
    <NarrowScreenHint/>
    <header class="sheet-head">
      <div>
        <h1>{{ greeting }}</h1>
        <div class="subtitle">{{ t('message.menu.dashboard') }} · {{ userDisplayName }}</div>
      </div>
      <time :datetime="sheetDate">{{ sheetDate }}</time>
    </header>
    <template v-if="!userStore.isSuperAdmin">
      <BusinessOverview/>
      <ServerHealthCard/>
    </template>
    <section class="quick-wrap">
      <HomeQuickNav :entries="quickEntries" :ariaLabel="userStore.isSuperAdmin ? t('message.home.quickNav') : t('message.appHome.quickNav')"/>
    </section>
  </div>
</template>

<script setup lang="ts" name="DashboardHome">
import {computed} from 'vue';
import NarrowScreenHint from '../../../components/narrow-screen-hint/index.vue';
import BusinessOverview from './components/BusinessOverview.vue';
import ServerHealthCard from './components/ServerHealthCard.vue';
import HomeQuickNav from './components/HomeQuickNav.vue';
import type {QuickNavEntry} from './components/HomeQuickNav.vue';
import {useUserInfoStore} from '../../../stores/userInfo';
import {t} from '../../../i18n/index';
import {faUsers, faCartShopping, faServer, faTicket, faBullhorn, faEnvelopeOpenText, faTableCells, faGear, faFile} from '../../../utils/icons';

defineOptions({name: 'DashboardHome'});

const userStore = useUserInfoStore();
const userDisplayName = computed(() => userStore.userInfos.realName || userStore.userInfos.account);
const greeting = computed(() => {
  const hour = new Date().getHours();
  if (hour >= 5 && hour < 11) return t('message.home.greetingMorning');
  if (hour >= 11 && hour < 13) return t('message.home.greetingNoon');
  if (hour >= 13 && hour < 18) return t('message.home.greetingAfternoon');
  if (hour >= 18 && hour < 23) return t('message.home.greetingEvening');
  return t('message.home.greetingNight');
});
const now = new Date();
const sheetDate = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;

// 快捷入口根据真实已注册路由判可达；平台会话只使用平台相关入口。
const quickEntries = computed<QuickNavEntry[]>(() => userStore.isSuperAdmin ? [
  {path: '/system/menu', icon: faTableCells, label: t('message.home.quickMenus')},
  {path: '/system/config', icon: faGear, label: t('message.home.quickConfigs')},
  {path: '/system/dict', icon: faFile, label: t('message.home.quickDicts')},
] : [
  {path: '/playerCenter/gameUserEntity', icon: faUsers, label: t('message.appHome.quickPlayers')},
  {path: '/commerceCenter/gameOrderEntity', icon: faCartShopping, label: t('message.appHome.quickOrders')},
  {path: '/serverCenter/gameAreaEntity', icon: faServer, label: t('message.appHome.quickAreas')},
  {path: '/commerceCenter/gameRedeemCodeEntity', icon: faTicket, label: t('message.appHome.quickRedeemCodes')},
  {path: '/noticeCenter/announcement/gameAnnouncementEntity', icon: faBullhorn, label: t('message.appHome.quickAnnouncements')},
  {path: '/noticeCenter/mail/gameMailEntity', icon: faEnvelopeOpenText, label: t('message.appHome.quickMails')},
]);
</script>

<style scoped lang="scss">
.dashboard-home {
  overflow: visible;
  .sheet-head {
    display: flex;
    align-items: flex-end;
    justify-content: space-between;
    flex-wrap: wrap;
    gap: 16px;
    padding: 6px 2px 14px;
    margin-bottom: 18px;
    border-bottom: 1px solid var(--p-content-border-color);
  }
  h1 {
    margin: 0;
    font-size: 22px;
    font-weight: 800;
    color: var(--p-text-color);
  }
  .subtitle, time {
    margin-top: 4px;
    font-size: 13px;
    color: var(--p-text-muted-color);
  }
  time { font-variant-numeric: tabular-nums; }
}
</style>
