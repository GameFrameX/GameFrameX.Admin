<template>
  <main class="flex-1 flex flex-column overflow-hidden main-paper" v-show="!isTagsViewCurrenFull">
    <!-- TagsView 标签栏 -->
    <div class="flex-shrink-0">
      <TagsView />
    </div>

    <!-- 内容区域：制图纸桌面（蓝格网），页面纸卡浮于其上 -->
    <div
      ref="contentScrollRef"
      class="desk-scroll flex-1 overflow-auto"
    >
      <div class="content-inner">
        <ParentView />
      </div>
    </div>

    <!-- Footer 底部版权 -->
    <Footer />
  </main>
</template>

<script setup lang="ts" name="SplitterMain">
import { computed, defineAsyncComponent, ref } from 'vue'
import { storeToRefs } from 'pinia'
import { useTagsViewRoutesStore } from '../../../stores/tagsViewRoutes'
import { useThemeConfigStore } from '../../../stores/themeConfig'

// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const TagsView = defineAsyncComponent(() => import('/@/layout/navigationBars/tagsView/TagsView.vue'))
// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const ParentView = defineAsyncComponent(() => import('/@/layout/routerView/ParentView.vue'))
// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const Footer = defineAsyncComponent(() => import('/@/layout/footer/index.vue'))

const contentScrollRef = ref<HTMLElement>()

const storesTagsViewRoutes = useTagsViewRoutesStore()
const storesThemeConfig = useThemeConfigStore()
const { isTagsViewCurrenFull } = storeToRefs(storesTagsViewRoutes)
const { themeConfig } = storeToRefs(storesThemeConfig)

</script>

<style lang="scss" scoped>
.main-paper {
  background: var(--sidebar-paper);
}

// 制图纸桌面：desk-paper 色纸 + 24px 细格 / 120px 主格蓝格网（与 login 同源，纸面剧作卡世界）
.desk-scroll {
  padding: 16px;
  min-height: 0;
  background-color: var(--desk-paper);
  background-image:
    linear-gradient(var(--desk-grid) 1px, transparent 1px),
    linear-gradient(90deg, var(--desk-grid) 1px, transparent 1px),
    linear-gradient(var(--desk-grid-major) 1px, transparent 1px),
    linear-gradient(90deg, var(--desk-grid-major) 1px, transparent 1px);
  background-size: 24px 24px, 24px 24px, 120px 120px, 120px 120px;
}

.content-inner {
  min-height: 100%;
}
</style>
