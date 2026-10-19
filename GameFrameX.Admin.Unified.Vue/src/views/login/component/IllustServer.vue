<template>
  <!-- 制图纸插画 2/3：服务器机架立面 schematic（品牌蓝墨线，token 派生，亮暗自适应） -->
  <svg class="illust" viewBox="0 0 300 220" fill="none" xmlns="http://www.w3.org/2000/svg" role="img">
    <rect class="ink" x="66" y="14" width="168" height="184" rx="6"/>
    <line class="ink" x1="66" y1="60" x2="234" y2="60"/>
    <line class="ink" x1="66" y1="106" x2="234" y2="106"/>
    <line class="ink" x1="66" y1="152" x2="234" y2="152"/>
    <!-- 4U 单元：隔板 + 指示灯 -->
    <rect class="ink-fill-100" x="80" y="26" width="60" height="20" rx="3"/>
    <circle class="ink-solid" cx="204" cy="36" r="3.5"/>
    <circle class="ink" cx="218" cy="36" r="3.5"/>
    <rect class="ink-fill-200" x="80" y="72" width="60" height="20" rx="3"/>
    <circle class="ink-solid" cx="204" cy="82" r="3.5"/>
    <circle class="ink" cx="218" cy="82" r="3.5"/>
    <rect class="ink-fill-100" x="80" y="118" width="60" height="20" rx="3"/>
    <circle class="ink-solid" cx="204" cy="128" r="3.5"/>
    <circle class="ink" cx="218" cy="128" r="3.5"/>
    <rect class="ink-fill-200" x="80" y="164" width="60" height="20" rx="3"/>
    <circle class="ink-solid" cx="204" cy="174" r="3.5"/>
    <circle class="ink" cx="218" cy="174" r="3.5"/>
    <!-- 走线：机架底部出线 -->
    <path class="ink-thin" d="M110 198 C 110 212, 190 212, 190 198"/>
    <circle class="ink-solid" cx="110" cy="198" r="3"/>
    <circle class="ink-solid" cx="190" cy="198" r="3"/>
    <text class="label" x="234" y="214" text-anchor="end">RACK-04</text>
  </svg>
</template>

<script setup lang="ts" name="illustServer"></script>

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
