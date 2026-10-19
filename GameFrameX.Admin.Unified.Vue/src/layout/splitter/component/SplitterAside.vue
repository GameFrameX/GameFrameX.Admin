<template>
  <aside
    class="aside-root flex flex-column w-full h-full overflow-hidden"
    :class="{ 'is-collapse': isCollapse }"
  >
    <Logo v-if="showLogo" />
    <VerticalMenu :menuList="menuList" class="aside-menu" />
    <!-- 装订扣：目录折叠/展开的主控制件（顶栏按钮保留为快捷入口） -->
    <button
      type="button"
      class="spine-toggle"
      :class="{ 'spine-toggle-collapsed': isCollapse }"
      :aria-label="isCollapse ? t('layout.header.expandMenu') : t('layout.header.collapseMenu')"
      :aria-expanded="isCollapse ? 'false' : 'true'"
      v-tooltip.right="isCollapse ? t('layout.header.expandMenu') : t('layout.header.collapseMenu')"
      @click="onToggleCollapse"
    >
      <SvgIcon :name="isCollapse ? 'fa-solid fa-angles-right' : 'fa-solid fa-angles-left'" :size="14" class="spine-icon"/>
      <span v-if="!isCollapse" class="spine-label">{{ t('layout.header.collapseMenu') }}</span>
    </button>
  </aside>
</template>

<script setup lang="ts" name="SplitterAside">
import { computed } from 'vue'
import { defineAsyncComponent } from 'vue'
import { storeToRefs } from 'pinia'
import { useRoutesListStore } from '../../../stores/routesList'
import { useThemeConfigStore } from '../../../stores/themeConfig'
import { filterRoutesByMenuContext, resolveMenuContext } from '../../../utils/appWorkspace'
import { useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import SvgIcon from '/@/components/svg-icon/index.vue'
import Tooltip from 'primevue/tooltip'

// script-setup 下指令须以 vXxx 命名才会自动注册
const vTooltip = Tooltip

// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const Logo = defineAsyncComponent(() => import('/@/layout/logo/index.vue'))
// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const VerticalMenu = defineAsyncComponent(() => import('/@/layout/navigationMenu/VerticalMenu.vue'))

const storesRoutesList = useRoutesListStore()
const storesThemeConfig = useThemeConfigStore()
const route = useRoute()
const { routesList } = storeToRefs(storesRoutesList)
const { themeConfig } = storeToRefs(storesThemeConfig)
const { t } = useI18n()

const isCollapse = computed(() => themeConfig.value.isCollapse)
const showLogo = computed(() => true)
const menuList = computed(() => filterRoutesByMenuContext(routesList.value, resolveMenuContext(route)))

// 装订扣切换目录折叠（替代原 Logo 点击折叠的隐性入口）
const onToggleCollapse = () => {
  themeConfig.value.isCollapse = !themeConfig.value.isCollapse
}
</script>

<style lang="scss" scoped>
.aside-root {
  background: var(--sidebar-paper);
  box-sizing: border-box;
}

.aside-menu {
  flex: 1;
  min-height: 0;
  overflow-y: auto;
  // 滚动条安静化：纸面上不留粗色条
  scrollbar-width: thin;
  scrollbar-color: var(--hairline) transparent;
}

:deep(.p-panelmenu) {
  width: 100%;
}

// 装订扣：侧栏底部全宽行，发丝线封底，hover 淡墨洗
.spine-toggle {
  flex-shrink: 0;
  display: flex;
  align-items: center;
  gap: 8px;
  width: 100%;
  min-height: 40px;
  padding: 0 16px;
  border: 0;
  border-top: 1px solid var(--hairline);
  background: transparent;
  color: var(--p-text-muted-color);
  font-size: 12px;
  cursor: pointer;
  box-sizing: border-box;

  &:hover {
    background: color-mix(in srgb, var(--p-primary-100) 60%, transparent);
    color: var(--p-primary-color);
  }

  &:focus-visible {
    outline: 2px solid var(--p-primary-color);
    outline-offset: -2px;
  }

  .spine-icon {
    flex-shrink: 0;
  }

  .spine-label {
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  &.spine-toggle-collapsed {
    justify-content: center;
    padding: 0;
  }
}

.is-collapse {
  :deep(.menu-label) {
    display: none;
  }

  :deep(.p-panelmenu-panel) {
    .p-panelmenu-header {
      padding: 0;
      .p-panelmenu-header-content {
        justify-content: center;
        padding: 8px 0;
      }
    }
  }
}
</style>
