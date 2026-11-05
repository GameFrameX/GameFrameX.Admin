<template>
  <Toolbar class="layout-topbar">
    <template #end>
      <ServerSelector v-if="!userStore.isSuperAdmin" class="global-server-selector"/>
      <User/>
    </template>
  </Toolbar>
</template>

<script setup lang="ts" name="layoutBreadcrumbIndex">
import {defineAsyncComponent} from 'vue';
import Toolbar from 'primevue/toolbar';
import ServerSelector from '/@/components/server-selector/ServerSelector.vue';
import {useUserInfoStore} from '../../../stores/userInfo';

const userStore = useUserInfoStore();

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
    gap: 8px;
  }
}
.global-server-selector {
  max-width: 220px;
  :deep(.p-select) { width: 180px; }
  @include bp-down(sm) {
    max-width: 150px;
    :deep(.p-select) { width: 130px; }
  }
}
</style>