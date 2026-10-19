<template>
  <!-- 制图纸插画 1/3：代码终端 schematic（品牌蓝墨线，token 派生，亮暗自适应） -->
  <svg class="illust" viewBox="0 0 300 220" fill="none" xmlns="http://www.w3.org/2000/svg" role="img">
    <rect class="ink" x="16" y="16" width="268" height="180" rx="8"/>
    <line class="ink" x1="16" y1="48" x2="284" y2="48"/>
    <circle class="ink" cx="36" cy="32" r="5"/>
    <circle class="ink" cx="54" cy="32" r="5"/>
    <circle class="ink-solid" cx="72" cy="32" r="5"/>
    <polyline class="ink" points="34,68 44,79 34,90"/>
    <rect class="ink-fill-100" x="58" y="72" width="150" height="8" rx="4"/>
    <rect class="ink-fill-200" x="70" y="92" width="108" height="8" rx="4"/>
    <rect class="ink-fill-100" x="58" y="112" width="176" height="8" rx="4"/>
    <rect class="ink-solid" x="70" y="132" width="86" height="8" rx="4"/>
    <rect class="ink-solid" x="34" y="164" width="10" height="14"/>
    <!-- Actor 拓扑角标：节点与连线 -->
    <line class="ink-thin" x1="216" y1="150" x2="244" y2="150"/>
    <line class="ink-thin" x1="216" y1="150" x2="216" y2="172"/>
    <line class="ink-thin" x1="216" y1="172" x2="244" y2="172"/>
    <circle class="ink-solid" cx="216" cy="150" r="4"/>
    <circle class="ink" cx="244" cy="150" r="4"/>
    <circle class="ink" cx="216" cy="172" r="4"/>
    <circle class="ink" cx="244" cy="172" r="4"/>
    <text class="label" x="284" y="210" text-anchor="end">TERM-01</text>
  </svg>
</template>

<script setup lang="ts" name="illustTerminal"></script>

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
