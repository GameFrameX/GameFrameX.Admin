<template>
  <div class="layout-footer pb15">
    <div class="layout-footer-warp mono">
      <a href="https://github.com/GameFrameX" target="_blank" rel="noopener">Github</a>
      <span aria-hidden="true">·</span>
      <a href="https://gitee.com/GameFrameX" target="_blank" rel="noopener">Gitee</a>
      <span aria-hidden="true">·</span>
      <a href="https://cnb.cool/GameFrameX" target="_blank" rel="noopener">CNB</a>
      <span aria-hidden="true" class="footer-sep">—</span>
      <span>Copyright © 2022-{{ currentYear }} Blank All rights reserved.</span>
    </div>
  </div>
</template>

<script setup lang="ts" name="layoutFooter">
const currentYear = new Date().getFullYear();
</script>

<style scoped lang="scss">
// 版头数据行（纸面剧作卡世界的 mono 设备）：11px/0.08em 大写，安静小字
.layout-footer {
  width: 100%;
  display: flex;

  &-warp {
    margin: auto;
    color: var(--p-text-muted-color);
    font-family: ui-monospace, 'SF Mono', SFMono-Regular, Menlo, Consolas, 'Liberation Mono', monospace;
    font-size: 11px;
    letter-spacing: 0.08em;
    text-transform: uppercase;
    text-align: center;
    padding: 0 12px;

    a {
      color: inherit;
      text-decoration: none;

      &:hover {
        color: var(--p-primary-color);
      }
    }

    // flex 布局，窄屏自动换行，避免横向溢出（C143）
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    gap: 4px 8px;

    .footer-sep {
      color: var(--p-content-border-color);
    }
  }
}
</style>
