<template>
  <div class="home-kpi" :class="{ 'home-kpi-main': variant === 'main' }">
    <div class="kpi-label mono">{{ label }}</div>
    <div class="kpi-value">{{ value }}</div>
    <div class="kpi-foot">{{ foot }}</div>
    <div v-if="variant === 'main'" class="kpi-illust">
      <slot name="illust"></slot>
    </div>
  </div>
</template>

<script setup lang="ts" name="HomeKpiCard">
// HOME-001 指标卡：main 形态为「全服在线」大读数卡（可挂 schematic 插画），
// small 形态为次级指标小卡。安静形态：静态细边框，hover 只亮边框，无阴影抬升。
defineProps<{
  variant?: 'main' | 'small';
  label: string;
  value: string | number;
  foot?: string;
}>();
</script>

<style scoped lang="scss">
.home-kpi {
  position: relative;
  overflow: hidden;
  background: var(--p-content-background);
  color: var(--p-text-color);
  border: 1px solid var(--p-content-border-color);
  border-radius: var(--p-content-border-radius);
  padding: 16px 18px;
  transition: border-color 0.15s ease;

  &:hover {
    border-color: var(--p-primary-300);
  }

  .mono {
    font-family: ui-monospace, 'SF Mono', SFMono-Regular, Menlo, Consolas, 'Liberation Mono', monospace;
    font-size: 11px;
    letter-spacing: 0.08em;
    text-transform: uppercase;
  }

  .kpi-label {
    color: var(--p-text-muted-color);
  }

  .kpi-value {
    color: var(--p-text-color);
    font-weight: 700;
    font-size: 26px;
    line-height: 1.2;
    margin: 8px 0 2px;
    font-variant-numeric: tabular-nums;
  }

  .kpi-foot {
    color: var(--p-text-muted-color);
    font-size: 12px;
  }

  &.home-kpi-main {
    padding: 20px 22px;

    .kpi-value {
      font-weight: 800;
      font-size: 44px;
      letter-spacing: -0.01em;
      line-height: 1.15;
      margin: 6px 0 2px;
    }

    .kpi-foot {
      font-size: 12.5px;
    }

    .kpi-illust {
      position: absolute;
      right: 14px;
      bottom: 10px;
      opacity: 0.9;
      pointer-events: none;
    }
  }

  @include bp-down(md) {
    &.home-kpi-main {
      padding: 16px 18px;

      .kpi-value {
        font-size: 36px;
      }

      .kpi-illust {
        :deep(svg) {
          width: 120px;
          height: auto;
        }
      }
    }
  }

  @include bp-down(sm) {
    padding: 14px;

    .kpi-value {
      font-size: 20px;
    }

    .kpi-foot {
      font-size: 11px;
    }
  }
}
</style>
