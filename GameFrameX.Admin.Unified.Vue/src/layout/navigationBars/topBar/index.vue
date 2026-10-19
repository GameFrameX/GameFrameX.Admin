<template>
  <Toolbar class="layout-topbar">
    <template #end>
      <User/>
    </template>
  </Toolbar>
</template>

<script setup lang="ts" name="layoutBreadcrumbIndex">
import {defineAsyncComponent} from 'vue';
import Toolbar from 'primevue/toolbar';

// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const User = defineAsyncComponent(() => import('/@/layout/navigationBars/topBar/UserDropdown.vue'));
</script>

<style scoped lang="scss">
.layout-topbar {
  height: 50px;
  padding: 0;

  :deep(.p-toolbar-start),
  :deep(.p-toolbar-center),
  :deep(.p-toolbar-end) {
    display: flex;
    align-items: center;
  }
}
</style>