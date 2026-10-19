<template>
  <!-- 制图纸插画 3/3：多端设备拓扑 schematic（品牌蓝墨线，token 派生，亮暗自适应） -->
  <svg class="illust" viewBox="0 0 300 220" fill="none" xmlns="http://www.w3.org/2000/svg" role="img">
    <!-- 显示器 -->
    <rect class="ink" x="30" y="24" width="156" height="104" rx="8"/>
    <rect class="ink-fill-100" x="42" y="36" width="132" height="80" rx="4"/>
    <path class="ink" d="M92 128 L92 146"/>
    <line class="ink" x1="68" y1="148" x2="148" y2="148"/>
    <!-- 手机 -->
    <rect class="ink" x="206" y="64" width="56" height="112" rx="12"/>
    <line class="ink-thin" x1="206" y1="84" x2="262" y2="84"/>
    <rect class="ink-fill-200" x="214" y="92" width="40" height="72" rx="4"/>
    <!-- 手柄 -->
    <rect class="ink" x="70" y="168" width="104" height="34" rx="17"/>
    <line class="ink" x1="92" y1="177" x2="92" y2="193"/>
    <line class="ink" x1="84" y1="185" x2="100" y2="185"/>
    <circle class="ink-solid" cx="146" cy="180" r="3.5"/>
    <circle class="ink-solid" cx="158" cy="190" r="3.5"/>
    <!-- 拓扑连线：多端接入同一节点 -->
    <path class="ink-thin" d="M186 76 L206 100"/>
    <circle class="ink-solid" cx="196" cy="88" r="4"/>
    <path class="ink-thin" d="M122 148 L122 168"/>
    <circle class="ink-solid" cx="122" cy="158" r="4"/>
    <text class="label" x="262" y="52" text-anchor="end">NODE-07</text>
  </svg>
</template>

<script setup lang="ts" name="illustDevices"></script>

<style scoped lang="scss">
.illust {
  display: block;
  width: 100%;
  height: auto;
}

.ink {
  stroke: var(--p-primary-color);
  stroke-width: 2;
}

.ink-thin {
  stroke: var(--p-primary-300);
  stroke-width: 1.5;
}

.ink-solid {
  fill: var(--p-primary-color);
}

.ink-fill-100 {
  fill: var(--p-primary-100);
}

.ink-fill-200 {
  fill: var(--p-primary-200);
}

.label {
  font-family: ui-monospace, 'SF Mono', SFMono-Regular, Menlo, Consolas, monospace;
  font-size: 9px;
  letter-spacing: 0.08em;
  fill: var(--p-text-muted-color);
}
</style>
