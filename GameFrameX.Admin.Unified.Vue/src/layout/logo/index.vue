<template>
  <div class="layout-logo" v-if="setShowLogo">
    <img :src="logoMini" :alt="themeConfig.globalTitle" class="layout-logo-img"/>
    <span class="layout-logo-title">{{ themeConfig.globalTitle }}</span>
  </div>
  <div class="layout-logo-size" v-else>
    <img :src="logoMini" :alt="themeConfig.globalTitle" class="layout-logo-size-img"/>
  </div>
</template>

<script setup lang="ts" name="layoutLogo">
import {computed} from 'vue';
import {storeToRefs} from 'pinia';
import {useThemeConfigStore} from '../../stores/themeConfig';
import logoMini from '/@/assets/logo-mini.svg';

// 定义变量内容
const storesThemeConfig = useThemeConfigStore();
const {themeConfig} = storeToRefs(storesThemeConfig);

// 设置 logo 的显示
const setShowLogo = computed(() => {
  const {isCollapse} = themeConfig.value;
  return !isCollapse || document.body.clientWidth < 1024; // 移动端阈值，与 theme/breakpoints.scss --bp-lg 一致
});
</script>

<style scoped lang="scss">
// 图纸目录版头：纸面直排 + 发丝线封底（无阴影抬升，纸面剧作卡世界）
.layout-logo {
  width: 100%;
  height: 50px;
  display: flex;
  align-items: center;
  padding: 0 16px;
  border-bottom: 1px solid var(--p-content-border-color);
  box-sizing: border-box;
  gap: 8px;

  .layout-logo-img {
    width: 24px;
    height: 24px;
    flex-shrink: 0;
  }

  .layout-logo-title {
    white-space: nowrap;
    display: inline-block;
    font-weight: 800;
    font-size: 15px;
    letter-spacing: -0.01em;
    color: var(--p-text-color);
    overflow: hidden;
    text-overflow: ellipsis;
  }
}

// 折叠态：仅居中印记
.layout-logo-size {
  width: 100%;
  height: 50px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-bottom: 1px solid var(--p-content-border-color);
  box-sizing: border-box;

  &-img {
    width: 24px;
    height: 24px;
  }
}
</style>
