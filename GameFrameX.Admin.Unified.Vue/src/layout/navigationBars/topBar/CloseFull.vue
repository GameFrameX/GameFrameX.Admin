<template>
  <div class="layout-navbars-close-full" v-if="isTagsViewCurrenFull">
    <div class="layout-navbars-close-full-icon">
      <SvgIcon name="fa-solid fa-xmark" :title="t('message.tagsView.closeFullscreen')" @click="onCloseFullscreen"/>
    </div>
  </div>
</template>

<script setup lang="ts" name="layoutCloseFull">
import {storeToRefs} from 'pinia';
import {useTagsViewRoutesStore} from '../../../stores/tagsViewRoutes';

// 定义变量内容
const stores = useTagsViewRoutesStore();
const {isTagsViewCurrenFull} = storeToRefs(stores);

// 关闭当前全屏
const onCloseFullscreen = () => {
  stores.setCurrenFullscreen(false);
};
</script>

<style scoped lang="scss">
.layout-navbars-close-full {
  position: fixed;
  z-index: $z-index-fullscreen;
  right: -30px;
  top: -30px;

  .layout-navbars-close-full-icon {
    width: 60px;
    height: 60px;
    border-radius: 100%;
    cursor: pointer;
    background: var(--p-surface-200);
    transition: all 0.3s ease;
    position: relative;

    :deep(i) {
      position: absolute;
      left: 10px;
      top: 35px;
      color: var(--p-text-color);
      transition: all 0.3s ease;
    }
  }

  &:hover {
    transition: all 0.3s ease;

    :deep(i) {
      color: var(--p-primary-color);
      transition: all 0.3s ease;
    }
  }
}
</style>
