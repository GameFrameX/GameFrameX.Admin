<template>
  <div class="home-container layout-pd">
    <NarrowScreenHint/>

    <!-- 版头：平台驾驶舱的文件头 -->
    <header class="sheet-head">
      <div class="head-left">
        <h1>{{ greeting }}</h1>
        <div class="sub">{{ t('message.home.platformSub') }} · {{ userDisplayName }}</div>
      </div>
      <div class="head-meta mono">
        <div>SHEET NO. PLATFORM-{{ sheetDay }} · REV A</div>
        <div>DATE {{ sheetDate }}</div>
      </div>
    </header>

    <!-- 主指标带（平台跨租户视角） -->
    <section class="grid kpis">
      <HomeKpiCard
          variant="main"
          :label="t('message.home.kpiTenants')"
          :value="state.tenantsLoaded ? state.tenants.toLocaleString() : '—'"
          :foot="t('message.home.kpiTenantsFoot')">
        <template #illust>
          <HomeRackIllust/>
        </template>
      </HomeKpiCard>
      <HomeKpiCard
          :label="t('message.home.kpiApps')"
          :value="state.apps.toLocaleString()"
          :foot="t('message.home.kpiAppsFoot')"/>
      <HomeKpiCard
          :label="t('message.home.kpiExpiring')"
          :value="state.expiringTenants.toLocaleString()"
          :foot="t('message.home.kpiExpiringFoot')"/>
      <HomeKpiCard
          :label="t('message.home.kpiPackages')"
          :value="state.packagesLoaded ? state.packages.toLocaleString() : '—'"
          :foot="t('message.home.kpiPackagesFoot')"/>
    </section>

    <!-- 平台快捷入口 -->
    <section class="quick-wrap">
      <HomeQuickNav :entries="quickEntries" :aria-label="t('message.home.quickNav')"/>
    </section>
  </div>
</template>

<script setup lang="ts" name="home">
// PLATFORM-驾驶舱：超管平台首页（三层驾驶舱之一，2026-08-31 拍板）。
// 租户访问本页由路由守卫导流（未选应用→选择器 / 已选→应用驾驶舱）。
// 租户总数与套餐总数走真实接口；应用总数与到期预警为 mock（见 usePlatformHomeData TODO）。
import {computed} from 'vue';
import NarrowScreenHint from '/@/components/narrow-screen-hint/index.vue';
import HomeKpiCard from './components/HomeKpiCard.vue';
import HomeRackIllust from './components/HomeRackIllust.vue';
import HomeQuickNav from './components/HomeQuickNav.vue';
import type {QuickNavEntry} from './components/HomeQuickNav.vue';
import {usePlatformHomeData} from './composables/usePlatformHomeData';
import {useUserInfoStore} from '/@/stores/userInfo';
import {
  faTableCells,
  faGear,
  faFile,
  faBuilding,
  faCoins,
  faGlobe,
} from '/@/utils/icons';

const {state} = usePlatformHomeData();
const userStore = useUserInfoStore();

// 显示名与顶栏 UserDropdown 同规则：realName 为空回退 account
const userDisplayName = computed(() => {
  const info = userStore.userInfos;
  return info.realName === '' ? info.account : info.realName;
});

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

// 平台快捷入口：全部为平台面真实菜单路由（SuperAdmin 菜单树内）
const quickEntries: QuickNavEntry[] = [
  {path: '/platform/global/tenant', icon: faBuilding, label: t('message.home.quickTenants')},
  {path: '/platform/global/tenantPackage', icon: faCoins, label: t('message.home.quickPackages')},
  {path: '/platform/menu', icon: faTableCells, label: t('message.home.quickMenus')},
  {path: '/platform/config', icon: faGear, label: t('message.home.quickConfigs')},
  {path: '/platform/dict', icon: faFile, label: t('message.home.quickDicts')},
  {path: '/platform/global/platform', icon: faGlobe, label: t('message.home.quickPlatforms')},
];
</script>

<style scoped lang="scss">
.home-container {
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
    margin-bottom: 18px;
  }

  @include bp-down(md) {
    .kpis {
      grid-template-columns: repeat(6, 1fr);

      // 主卡独占一行，三小卡一行三列
      > :first-child {
        grid-column: 1 / -1;
      }
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
  }
}
</style>
