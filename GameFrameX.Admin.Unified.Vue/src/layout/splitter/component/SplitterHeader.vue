<template>
  <header class="layout-header flex align-items-center flex-shrink-0 min-w-0" v-show="!isTagsViewCurrenFull">
    <div class="flex align-items-center mr-auto min-w-0 flex-shrink-0">
      <Button
        text
        rounded
        class="collapse-btn flex align-items-center justify-content-center"
        :aria-label="isCollapse ? t('layout.header.expandMenu') : t('layout.header.collapseMenu')"
        @click="onToggleCollapse"
        v-tooltip.bottom="isCollapse ? t('layout.header.expandMenu') : t('layout.header.collapseMenu')"
      >
        <SvgIcon :name="isCollapse ? 'fa-solid fa-indent' : 'fa-solid fa-outdent'" :size="20" class="collapse-icon-desktop" />
        <SvgIcon name="fa-solid fa-bars" :size="20" class="collapse-icon-mobile" />
      </Button>
      <Breadcrumb class="header-breadcrumb"/>
    </div>
    <div class="flex align-items-center flex-shrink-0">
      <TopBar />
    </div>
  </header>
</template>

<script setup lang="ts" name="SplitterHeader">
import { computed, defineAsyncComponent } from 'vue'
import Button from 'primevue/button'
import Tooltip from 'primevue/tooltip'
import { storeToRefs } from 'pinia'
import { useTagsViewRoutesStore } from '../../../stores/tagsViewRoutes'
import { useThemeConfigStore } from '../../../stores/themeConfig'
import { useI18n } from 'vue-i18n'

// script-setup 下指令须以 vXxx 命名才会自动注册（v-tooltip 此前未生效，随外壳改造一并修复）
const vTooltip = Tooltip

// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const TopBar = defineAsyncComponent(() => import('/@/layout/navigationBars/topBar/index.vue'))
// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const Breadcrumb = defineAsyncComponent(() => import('/@/layout/navigationBars/topBar/BreadcrumbBar.vue'))

const storesTagsViewRoutes = useTagsViewRoutesStore()
const storesThemeConfig = useThemeConfigStore()
const { isTagsViewCurrenFull } = storeToRefs(storesTagsViewRoutes)
const { themeConfig } = storeToRefs(storesThemeConfig)
const { t } = useI18n()

// 侧边栏折叠状态
const isCollapse = computed(() => themeConfig.value.isCollapse)

// 切换折叠状态
const onToggleCollapse = () => {
  themeConfig.value.isCollapse = !themeConfig.value.isCollapse
}
</script>

<style lang="scss" scoped>
// 台面压条：纸面 + 发丝线封底（纸面剧作卡世界）
.layout-header {
  height: 50px;
  padding: 0 16px;
  background: var(--sidebar-paper);
  border-bottom: 1px solid var(--hairline);

  // TopBar 的 Toolbar 透明化，融入纸面压条
  :deep(.layout-topbar) {
    background: transparent;
    border: 0;
    gap: 0;
  }
}

.collapse-btn {
  width: 44px;
  height: 44px;
  flex-shrink: 0;
  // 窄屏下顶栏工具区可能与折叠按钮位置接近，提层保证始终可点
  position: relative;
  z-index: $z-index-local;

  i {
    font-size: 16px;
  }
}

.collapse-icon-mobile {
  display: none;
}

// 手机端：用汉堡图标 fa-bars（移动端菜单标准，比 fa-outdent 直观），深色醒目
// FA7 渲染链路已修复（C149），mobile 回归 SvgIcon 方案，与 desktop 统一
@include bp-down(sm) {
  .collapse-icon-desktop {
    display: none;
  }

  .collapse-icon-mobile {
    display: block;
    color: var(--p-text-color);
  }
}
</style>
