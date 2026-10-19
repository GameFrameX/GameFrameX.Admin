<template>
  <!-- 平台驾驶舱签名元素：RACK-04 服务器机架 schematic 线稿
       （登录门面三张图纸之一的直系后代，HOME-001 同语言）。 -->
  <svg :width="width" :height="height" viewBox="0 0 150 96" aria-hidden="true">
    <g fill="none" stroke="var(--p-primary-color)" stroke-width="2">
      <rect x="10" y="8" width="72" height="76" rx="3"/>
      <path d="M10 33 L82 33 M10 58 L82 58"/>
    </g>
    <g fill="none" stroke="var(--p-primary-300)" stroke-width="1.5">
      <path d="M18 18 L58 18 M18 24 L44 24"/>
      <path d="M18 43 L58 43 M18 49 L44 49"/>
      <path d="M18 68 L58 68 M18 74 L44 74"/>
      <circle cx="68" cy="18" r="2.5"/><circle cx="68" cy="43" r="2.5"/><circle cx="68" cy="68" r="2.5"/>
      <path d="M96 20 L96 72 M96 20 L108 30 M96 72 L108 62 M108 30 L108 62"/>
    </g>
    <rect x="96" y="40" width="12" height="12" rx="2" fill="var(--p-primary-100)" stroke="none"/>
    <text x="10" y="92" fill="var(--p-text-muted-color)" font-size="9" letter-spacing="1"
          style="font-family: ui-monospace, 'SF Mono', SFMono-Regular, Menlo, Consolas, 'Liberation Mono', monospace; text-transform: uppercase;">RACK-04</text>
  </svg>
</template>

<script setup lang="ts" name="HomeRackIllust">
withDefaults(defineProps<{ width?: number; height?: number }>(), {
  width: 150,
  height: 96,
});
</script>
