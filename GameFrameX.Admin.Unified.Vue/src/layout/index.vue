<template>
  <SplitterLayout />
</template>

<script setup lang="ts" name="layout">
import { onBeforeMount, onUnmounted, defineAsyncComponent } from 'vue'
import { storeToRefs } from 'pinia'
import { useThemeConfigStore } from '../stores/themeConfig'
import mittBus from '../utils/mitt'

// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const SplitterLayout = defineAsyncComponent(() => import('/@/layout/splitter/SplitterLayout.vue'))

const storesThemeConfig = useThemeConfigStore()
const { themeConfig } = storeToRefs(storesThemeConfig)

const onLayoutResize = () => {
  const clientWidth = document.body.clientWidth
  if (clientWidth < 1024) { // 移动端阈值，与 theme/breakpoints.scss --bp-lg 一致
    themeConfig.value.isCollapse = false
    mittBus.emit('layoutMobileResize', { clientWidth })
  } else {
    mittBus.emit('layoutMobileResize', { clientWidth })
  }
}

onBeforeMount(() => {
  onLayoutResize()
  window.addEventListener('resize', onLayoutResize)
})

onUnmounted(() => {
  window.removeEventListener('resize', onLayoutResize)
})
</script>
