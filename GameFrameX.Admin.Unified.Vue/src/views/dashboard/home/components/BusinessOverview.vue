<template>
  <div class="business-overview">
    <!-- 全服聚合数据：接口不接受区服筛选，保持原有全服语义。 -->
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

    <!-- 最近 24 个整点的真实在线采样 -->
    <section class="grid mid">
      <div class="card chart-card">
        <div class="card-head">
          <span class="title">{{ t('message.appHome.trendTitle') }}</span>
          <span class="mono-tag mono">00:00 — 24:00</span>
        </div>
        <HomeTrendChart :data="state.trend" :labels="state.trendLabels" class="trend-holder"/>
      </div>
    </section>

  </div>
</template>

<script setup lang="ts">
import HomeKpiCard from './HomeKpiCard.vue';
import HomeTermIllust from './HomeTermIllust.vue';
import HomeTrendChart from './HomeTrendChart.vue';
import {useDashboardData} from '../composables/useDashboardData';
import {t} from '../../../../i18n/index';

const {state} = useDashboardData();
const kpi = (value: number) => state.overviewLoaded && !state.overviewFailed ? value.toLocaleString() : '—';
</script>

<style scoped lang="scss">
.business-overview {
  .grid {
    display: grid;
    gap: 14px;
    margin: 0 0 18px;
  }
  .kpis {
    grid-template-columns: 5fr 3fr 2fr 2fr;
  }
  .card {
    min-width: 0;
    padding: 16px 18px;
    background: var(--p-content-background);
    border: 1px solid var(--p-content-border-color);
    border-radius: var(--p-content-border-radius);
  }
  .card-head {
    display: flex;
    flex-wrap: wrap;
    justify-content: space-between;
    gap: 8px;
    font-weight: 600;
  }
  .mono-tag {
    font-size: 12px;
    color: var(--p-text-muted-color);
    font-variant-numeric: tabular-nums;
  }
  .trend-holder {
    height: clamp(220px, 26vw, 280px);
  }
  @include bp-down(md) {
    .kpis { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  }
  @include bp-down(sm) {
    .kpis { grid-template-columns: minmax(0, 1fr); }
  }
}
</style>
