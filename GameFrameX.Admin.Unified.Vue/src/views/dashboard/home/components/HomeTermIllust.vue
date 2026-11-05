<template>
  <!-- 签名元素：登录门面 TERM-01 schematic 终端线稿的直系后代（HOME-001）。
       全页唯一的世界装饰，颜色全部走 --p-* token，亮暗自动换墨。 -->
  <svg :width="width" :height="height" viewBox="0 0 150 96" aria-hidden="true">
    <g fill="none" stroke="var(--p-primary-color)" stroke-width="2">
      <rect x="8" y="8" width="98" height="58" rx="4"/>
      <path d="M40 66 L40 74 M74 66 L74 74 M26 78 L88 78"/>
    </g>
    <g fill="none" stroke="var(--p-primary-300)" stroke-width="1.5">
      <path d="M14 14 L100 14 M14 20 L100 20"/>
      <path d="M14 30 L34 30 M14 38 L46 38 M14 46 L28 46"/>
      <circle cx="120" cy="26" r="6"/>
      <path d="M120 32 L120 44 M120 44 L114 52 M120 44 L126 52"/>
    </g>
    <rect x="60" y="40" width="22" height="12" rx="2" fill="var(--p-primary-100)" stroke="none"/>
    <text x="10" y="92" fill="var(--p-text-muted-color)" font-size="9" letter-spacing="1"
          style="font-family: ui-monospace, 'SF Mono', SFMono-Regular, Menlo, Consolas, 'Liberation Mono', monospace; text-transform: uppercase;">TERM-01</text>
  </svg>
</template>

<script setup lang="ts" name="HomeTermIllust">
withDefaults(defineProps<{ width?: number; height?: number }>(), {
  width: 150,
  height: 96,
});
</script>
