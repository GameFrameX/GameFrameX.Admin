<template>
  <div ref="chartRef" class="home-trend-chart"></div>
</template>

<script setup lang="ts" name="HomeTrendChart">
// HOME-001 在线趋势：单序列 2px 折线，无图例（标题即命名，由父级卡片给出）。
// dataviz 规范：薄标记、弱网格、文本用轴色 token（echarts canvas 无法读 CSS 变量，
// 轴/网格色按 CHART_AXIS_COLORS 亮暗二值切换，与 useHomeCharts 既有模式一致）。
import {onMounted, onUnmounted, ref, watch} from 'vue';
import * as echarts from 'echarts';
import {storeToRefs} from 'pinia';
import {useThemeConfigStore} from '/@/stores/themeConfig';
import {CHART_AXIS_COLORS, CHART_SERIES_COLORS} from '/@/theme/colors';

const props = withDefaults(defineProps<{
  data: Array<number | null>;
  /** x 轴整点标签；缺省时按 00:00-24:00 生成 */
  labels?: string[];
}>(), {
  labels: () => [],
});

const chartRef = ref<HTMLDivElement>();
const storesThemeConfig = useThemeConfigStore();
const {themeConfig} = storeToRefs(storesThemeConfig);

let chart: echarts.ECharts | null = null;
let resizeObserver: ResizeObserver | null = null;

const hourLabels = () => {
  if (props.labels.length === props.data.length && props.labels.length > 0) {
    return props.labels;
  }
  const labels: string[] = [];
  for (let i = 0; i < props.data.length; i++) {
    labels.push(String(i).padStart(2, '0') + ':00');
  }
  return labels;
};

const renderChart = () => {
  if (!chartRef.value) {
    return;
  }
  const isDark = themeConfig.value.isIsDark;
  const axisColors = isDark ? CHART_AXIS_COLORS.dark : CHART_AXIS_COLORS.light;
  chart?.dispose();
  chart = echarts.init(chartRef.value, isDark ? 'dark' : '');
  chart.setOption({
    backgroundColor: 'transparent',
    grid: {top: 20, right: 12, bottom: 28, left: 48},
    tooltip: {
      trigger: 'axis',
      axisPointer: {type: 'cross', label: {backgroundColor: isDark ? '#3f3f46' : '#f5f5f5'}},
    },
    xAxis: {
      type: 'category',
      boundaryGap: false,
      data: hourLabels(),
      axisLabel: {color: axisColors.label, fontSize: 11},
      axisTick: {show: false},
      axisLine: {lineStyle: {color: axisColors.splitLine}},
    },
    yAxis: {
      type: 'value',
      scale: true,
      axisLabel: {color: axisColors.label, fontSize: 11},
      splitLine: {lineStyle: {color: axisColors.splitLine}},
    },
    series: [
      {
        type: 'line',
        data: props.data,
        showSymbol: false,
        symbol: 'circle',
        symbolSize: 7,
        lineStyle: {width: 2, color: CHART_SERIES_COLORS[0]},
        itemStyle: {color: CHART_SERIES_COLORS[0]},
        emphasis: {focus: 'series'},
      },
    ],
  });
};

onMounted(() => {
  renderChart();
  resizeObserver = new ResizeObserver(() => {
    chart?.resize();
  });
  if (chartRef.value) {
    resizeObserver.observe(chartRef.value);
  }
});

onUnmounted(() => {
  resizeObserver?.disconnect();
  chart?.dispose();
  chart = null;
});

watch(
    () => themeConfig.value.isIsDark,
    () => {
      renderChart();
    },
);
</script>

<style scoped lang="scss">
.home-trend-chart {
  width: 100%;
  height: 100%;
  min-height: 220px;
}
</style>
