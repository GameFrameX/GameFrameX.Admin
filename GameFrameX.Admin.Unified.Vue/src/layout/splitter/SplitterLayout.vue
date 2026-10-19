<template>
  <div class="layout-shell overflow-hidden">
    <!-- 移动端：使用抽屉式菜单 -->
    <MobileMenuDrawer v-if="isMobile" />

    <!-- 桌面端：使用 PrimeFlex 布局 -->
    <div v-if="!isMobile" class="flex w-full h-full">
      <!-- 左侧菜单（图纸目录）：右缘发丝线，与纸面世界一致 -->
      <aside class="aside-panel flex-shrink-0" :class="{ 'aside-panel-collapsed': isCollapse }">
        <SplitterAside />
      </aside>

      <!-- 右侧内容 -->
      <main class="flex-1 min-w-0 flex flex-column overflow-hidden desk-surface">
        <SplitterHeader />
        <SplitterMain />
      </main>
    </div>

    <!-- 移动端：主内容常驻，菜单通过抽屉打开 -->
    <div v-if="isMobile" class="flex w-full h-full">
      <main class="flex-1 min-w-0 flex flex-column overflow-hidden desk-surface">
        <SplitterHeader />
        <SplitterMain />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts" name="SplitterLayout">
import { computed, ref, onMounted, onUnmounted } from 'vue'
import { storeToRefs } from 'pinia'
import SplitterAside from './component/SplitterAside.vue'
import SplitterHeader from './component/SplitterHeader.vue'
import SplitterMain from './component/SplitterMain.vue'
import MobileMenuDrawer from './component/MobileMenuDrawer.vue'
import { useThemeConfigStore } from '../../stores/themeConfig.js'

const storesThemeConfig = useThemeConfigStore()
const { themeConfig } = storeToRefs(storesThemeConfig)

const defaultAsideWidth = 220
const defaultAsideWidthPx = `${defaultAsideWidth}px`

// 移动端检测
const isMobile = ref(false)
const checkMobile = () => {
  isMobile.value = document.body.clientWidth < 1024 // 移动端阈值，与 theme/breakpoints.scss --bp-lg 一致
}

// 侧边栏折叠状态
const isCollapse = computed(() => themeConfig.value.isCollapse)

// 暴露方法给外部调用
defineExpose({
  asideWidth: defaultAsideWidth,
})

// 生命周期
onMounted(() => {
  checkMobile()
  window.addEventListener('resize', checkMobile)
})

onUnmounted(() => {
  window.removeEventListener('resize', checkMobile)
})
</script>

<style lang="scss" scoped>
// 制图纸工作台 token（纸面剧作卡世界，亮暗双主题自动换墨）
.layout-shell {
  // desk-paper 用会翻转的 content-background 混墨蓝 400 阶：
  // surface-50/primary-50 在暗色下分别翻成近白/不翻转（login 同公式暗色桌面因此发白），此处不能沿用
  --desk-paper: color-mix(in srgb, var(--p-content-background) 90%, var(--p-primary-400));
  --desk-grid: color-mix(in srgb, var(--p-primary-200) 38%, transparent);
  --desk-grid-major: color-mix(in srgb, var(--p-primary-200) 62%, transparent);
  --sidebar-paper: var(--p-content-background);
  --hairline: var(--p-content-border-color);
  height: 100dvh;
  min-height: 100dvh;
}

.aside-panel {
  width: v-bind(defaultAsideWidthPx);
  height: 100%;
  background: var(--sidebar-paper);
  border-right: 1px solid var(--hairline); // 发丝线替代 4px 色块分隔（纸面世界结构线）
  overflow: hidden;
  transition: width 0.25s ease; // 折叠状态确认动效；全局 prefers-reduced-motion 已灭（GFX-659/C253）

  &.aside-panel-collapsed {
    width: 64px; // 与 navigationMenu/vertical.vue 的 .menu-collapsed 对齐，避免图标被 overflow:hidden 裁切（C256）
  }
}

// 桌面：制图纸色纸（格网铺在 SplitterMain 内容滚动区，header/tags 保持纸面）
.desk-surface {
  background-color: var(--desk-paper);
}
</style>
