<template>
  <Drawer
    v-model:visible="visible"
    position="left"
    :dismissable="true"
    :showCloseIcon="false"
    :pt="{ root: { style: 'width: 260px' } }"
    class="mobile-menu-drawer"
  >
    <template #header>
      <Logo />
    </template>
    <div class="mobile-menu-content">
      <VerticalMenu :menuList="menuList" />
    </div>
  </Drawer>
</template>

<script setup lang="ts" name="MobileMenuDrawer">
import { computed, watch, defineAsyncComponent } from 'vue'
import Drawer from 'primevue/drawer'
import { storeToRefs } from 'pinia'
import { useRoute } from 'vue-router'
import { useThemeConfigStore } from '../../../stores/themeConfig'
import { useRoutesListStore } from '../../../stores/routesList'
import { filterRoutesByMenuContext, resolveMenuContext } from '../../../utils/appWorkspace'

// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const Logo = defineAsyncComponent(() => import('/@/layout/logo/index.vue'))
// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const VerticalMenu = defineAsyncComponent(() => import('/@/layout/navigationMenu/VerticalMenu.vue'))

const storesThemeConfig = useThemeConfigStore()
const storesRoutesList = useRoutesListStore()
const { themeConfig } = storeToRefs(storesThemeConfig)
const { routesList } = storeToRefs(storesRoutesList)
const menuList = computed(() => filterRoutesByMenuContext(routesList.value, resolveMenuContext(route)))

const route = useRoute()

// 移动端时显示抽屉；阈值与 theme/breakpoints.scss --bp-lg 一致
const visible = computed({
  get: () => {
    const clientWidth = document.body.clientWidth
    return clientWidth < 1024 && themeConfig.value.isCollapse
  },
  set: (val) => {
    themeConfig.value.isCollapse = val
  },
})

// 移动端选择菜单项（路由变化）后自动收起抽屉
watch(() => route.path, () => {
  if (document.body.clientWidth < 1024) {
    themeConfig.value.isCollapse = false
  }
})
</script>

<style lang="scss" scoped>
// 抽屉 = 抽出的一张目录纸：纸面 + 发丝线（纸面剧作卡世界）
.mobile-menu-drawer {
  background: var(--p-content-background);

  :deep(.p-drawer-header) {
    padding: 0;
    border-bottom: 1px solid var(--p-content-border-color);
  }

  :deep(.p-drawer-content) {
    padding: 0;
  }
}

.mobile-menu-content {
  height: calc(100dvh - 60px);
  overflow-y: auto;
  // 滚动条安静化：纸面上不留粗色条
  scrollbar-width: thin;
  scrollbar-color: var(--p-content-border-color) transparent;
}
</style>
