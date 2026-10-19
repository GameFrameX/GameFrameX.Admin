<template>
  <div class="narrow-screen-hint">
    <svg class="narrow-screen-hint-icon" viewBox="0 0 24 24" aria-hidden="true">
      <rect x="2" y="3" width="20" height="14" rx="2"/>
      <line x1="8" y1="21" x2="16" y2="21"/>
      <line x1="12" y1="17" x2="12" y2="21"/>
    </svg>
    <span class="narrow-screen-hint-text">
      <slot>{{ t('common.message.narrowScreenHint') }}</slot>
    </span>
  </div>
</template>

<script setup lang="ts" name="narrowScreenHint">
// F1 移动端收尾：窄屏（<640px）降级提示，桌面隐藏
// 用于复杂页（富文本 / 代码 / 图表 / 手写画板）引导用户改用电脑端
// 响应式 mixin bp-up(sm) 由 breakpoints.scss 全局注入（C143）
</script>

<style scoped lang="scss">
.narrow-screen-hint {
  display: flex;
  align-items: center;
  gap: 8px;
  margin: 0 0 12px;
  padding: 10px 14px;
  font-size: 12px;
  line-height: 1.5;
  color: var(--p-message-warn-text-color);
  background: var(--p-message-warn-background);
  border: 1px solid var(--p-message-warn-border-color);
  border-radius: 6px;
  @include bp-up(sm) {
    display: none;
  }
}

.narrow-screen-hint-icon {
  flex-shrink: 0;
  width: 18px;
  height: 18px;
  fill: none;
  stroke: currentColor;
  stroke-width: 2;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.narrow-screen-hint-text {
  flex: 1;
  min-width: 0;
}
</style>
