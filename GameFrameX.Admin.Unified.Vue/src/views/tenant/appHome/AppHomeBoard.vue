<template>
  <div class="app-home-container layout-pd">
    <NarrowScreenHint/>

    <!-- 版头：应用驾驶舱的文件头 -->
    <header class="sheet-head">
      <div class="head-left">
        <h1>{{ greeting }}</h1>
        <div class="sub">{{ t('message.appHome.workbenchSub') }} · {{ userDisplayName }}</div>
      </div>
      <div class="head-right">
        <!-- C333（GFX-736）：顶栏切换器随 C326 移除后，这里是 appWorkspace → 应用列表的正门出口 -->
        <Button
            v-if="showBackToAppList"
            class="back-to-app-list"
            text
            severity="secondary"
            :label="t('gameBusiness.appWorkspace.backToAppList')"
            icon="fa-solid fa-arrow-left"
            @click="goBackToAppList"
        />
        <div class="head-meta mono">
          <div>SHEET NO. APP-{{ sheetDay }} · REV A</div>
          <div>DATE {{ sheetDate }}</div>
        </div>
      </div>
    </header>

    <!-- 主指标带（应用维度） -->
    <section class="grid kpis">
      <HomeKpiCard
          variant="main"
          :label="t('message.appHome.kpiOnline')"
          :value="kpi(state.onlineCount)"
          :foot="t('message.appHome.kpiOnlineFoot')">
        <template #illust>
          <HomeTermIllust/>
        </template>
      </HomeKpiCard>
      <HomeKpiCard
          :label="t('message.appHome.kpiOrders')"
          :value="kpi(state.todayOrders)"
          :foot="t('message.appHome.kpiOrdersFoot', {shipments: state.pendingShipments, refunds: state.refunding})"/>
      <HomeKpiCard
          :label="t('message.appHome.kpiUsers')"
          :value="kpi(state.playerTotal)"
          :foot="t('message.appHome.kpiUsersFoot', {count: state.playerTodayNew})"/>
      <HomeKpiCard
          :label="t('message.appHome.kpiAreas')"
          :value="kpi(state.areaTotal)"
          :foot="t('message.appHome.kpiAreasFoot', {running: state.areaRunning, maintaining: state.areaMaintenance})"/>
    </section>

    <!-- 趋势 + 公告 -->
    <section class="grid mid">
      <div class="card chart-card">
        <div class="card-head">
          <span class="title">{{ t('message.appHome.trendTitle') }}</span>
          <span class="mono-tag mono">00:00 — 24:00</span>
        </div>
        <HomeTrendChart :data="state.trend" :labels="state.trendLabels" class="trend-holder"/>
      </div>
      <div class="card">
        <HomeNoticeCard :notices="state.notices"/>
      </div>
    </section>

    <!-- 快捷入口（业务路由） -->
    <section class="quick-wrap">
      <HomeQuickNav :entries="quickEntries" :aria-label="t('message.appHome.quickNav')"/>
    </section>
  </div>
</template>

<script setup lang="ts" name="AppHomeBoard">
// APP-驾驶舱：租户选定应用后的运营首页（三层驾驶舱之一，2026-08-31 拍板）。
// 组件复用 HOME-001 系列（views/home/components），数据为应用维度（见 useAppHomeData）。
import {computed} from 'vue';
import {useRouter} from 'vue-router';
import Button from 'primevue/button';
import NarrowScreenHint from '/@/components/narrow-screen-hint/index.vue';
import HomeKpiCard from '/@/views/tenant/home/components/HomeKpiCard.vue';
import HomeTermIllust from '/@/views/tenant/home/components/HomeTermIllust.vue';
import HomeTrendChart from '/@/views/tenant/home/components/HomeTrendChart.vue';
import HomeNoticeCard from '/@/views/tenant/home/components/HomeNoticeCard.vue';
import HomeQuickNav from '/@/views/tenant/home/components/HomeQuickNav.vue';
import type {QuickNavEntry} from '/@/views/tenant/home/components/HomeQuickNav.vue';
import {useAppHomeData} from './composables/useAppHomeData';
import {useUserInfoStore} from '/@/stores/userInfo';
import {APP_SELECTION_PATH} from '/@/utils/appWorkspace';
import {
  faUsers,
  faCartShopping,
  faServer,
  faTicket,
  faBullhorn,
  faEnvelopeOpenText,
} from '/@/utils/icons';

const {state} = useAppHomeData();

// 聚合接口未就绪或失败时显示 —
const kpi = (v: number) => (state.overviewLoaded && !state.overviewFailed ? v.toLocaleString() : '—');
const userStore = useUserInfoStore();

// 显示名与顶栏 UserDropdown 同规则：realName 为空回退 account
const userDisplayName = computed(() => {
  const info = userStore.userInfos;
  return info.realName === '' ? info.account : info.realName;
});

// C333（GFX-736）：应用驾驶舱是 appWorkspace 上下文的常驻首屏，顶栏切换器已随
// C326 移除，返回应用列表（切回 tenantShell 上下文）的正门出口落在这里；
// 超管（accountType=999）不参与租户应用工作流，保持无该入口（同 C326 前顶栏 gate）。
const router = useRouter();
const showBackToAppList = computed(() => !userStore.isSuperAdmin);
const goBackToAppList = () => {
  router.push(APP_SELECTION_PATH);
};

const greeting = computed(() => {
  const hour = new Date().getHours();
  if (hour >= 5 && hour < 11) {
    return t('message.home.greetingMorning');
  }
  if (hour >= 11 && hour < 13) {
    return t('message.home.greetingNoon');
  }
  if (hour >= 13 && hour < 18) {
    return t('message.home.greetingAfternoon');
  }
  if (hour >= 18 && hour < 23) {
    return t('message.home.greetingEvening');
  }
  return t('message.home.greetingNight');
});

// 表单号按当日日期生成（mono 文件版头数据，不进 i18n）
const now = new Date();
const pad2 = (n: number): string => String(n).padStart(2, '0');
const sheetDay = `${pad2(now.getMonth() + 1)}${pad2(now.getDate())}`;
const sheetDate = `${now.getFullYear()}-${pad2(now.getMonth() + 1)}-${pad2(now.getDate())}`;

const quickEntries: QuickNavEntry[] = [
  {path: '/app/playerCenter/gameUserEntity', icon: faUsers, label: t('message.appHome.quickPlayers')},
  {path: '/app/playerCenter/gameOrderEntity', icon: faCartShopping, label: t('message.appHome.quickOrders')},
  {path: '/app/opsCenter/gameAreaEntity', icon: faServer, label: t('message.appHome.quickAreas')},
  {path: '/app/redeemCenter/gameRedeemCodeEntity', icon: faTicket, label: t('message.appHome.quickRedeemCodes')},
  {path: '/app/contentCenter/announcement', icon: faBullhorn, label: t('message.appHome.quickAnnouncements')},
  {path: '/app/contentCenter/mail', icon: faEnvelopeOpenText, label: t('message.appHome.quickMails')},
];
</script>

<style scoped lang="scss">
.app-home-container {
  overflow: visible;

  .mono {
    font-family: ui-monospace, 'SF Mono', SFMono-Regular, Menlo, Consolas, 'Liberation Mono', monospace;
    font-size: 11px;
    letter-spacing: 0.08em;
    text-transform: uppercase;
  }

  .sheet-head {
    display: flex;
    align-items: flex-end;
    justify-content: space-between;
    gap: 16px;
    padding: 6px 2px 14px;
    border-bottom: 1px solid var(--p-content-border-color);
    margin-bottom: 18px;
    flex-wrap: wrap;

    h1 {
      font-size: 22px;
      font-weight: 800;
      letter-spacing: -0.01em;
      color: var(--p-text-color);
      line-height: 1.25;
      margin: 0;
    }

    .sub {
      color: var(--p-text-muted-color);
      font-size: 13px;
      margin-top: 4px;
    }

    .head-right {
      display: flex;
      align-items: flex-end;
      gap: 14px;
      flex-wrap: wrap;
    }

    .head-meta {
      color: var(--p-text-muted-color);
      line-height: 1.9;
      text-align: right;
      white-space: nowrap;
    }
  }

  .grid {
    display: grid;
    gap: 14px;
  }

  .kpis {
    grid-template-columns: 5fr 3fr 2fr 2fr;
    margin-bottom: 14px;
  }

  .mid {
    grid-template-columns: 7fr 5fr;
    margin-bottom: 14px;
  }

  .card {
    background: var(--p-content-background);
    color: var(--p-text-color);
    border: 1px solid var(--p-content-border-color);
    border-radius: var(--p-content-border-radius);
    padding: 16px 18px;

    .card-head {
      display: flex;
      justify-content: space-between;
      align-items: baseline;
      margin-bottom: 10px;

      .title {
        font-weight: 600;
        color: var(--p-text-color);
        font-size: 14px;
      }

      .mono-tag {
        color: var(--p-text-muted-color);
      }
    }

    .trend-holder {
      height: clamp(220px, 26vw, 280px);
    }
  }

  @include bp-down(md) {
    .kpis {
      grid-template-columns: repeat(6, 1fr);

      // 主卡独占一行，三小卡一行三列
      > :first-child {
        grid-column: 1 / -1;
      }
    }

    .mid {
      grid-template-columns: 1fr;
    }
  }

  @include bp-down(sm) {
    .sheet-head {
      flex-direction: column;
      align-items: flex-start;
      gap: 8px;
      padding-bottom: 12px;

      h1 {
        font-size: 19px;
      }

      .head-meta {
        text-align: left;
        line-height: 1.7;
      }
    }

    .kpis {
      grid-template-columns: repeat(3, 1fr);
      gap: 10px;
    }

    .card {
      padding: 14px 16px;
    }
  }
}
</style>
