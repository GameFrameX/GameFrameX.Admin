<template>
  <div class="upgrade-dialog">
    <Dialog v-model:visible="state.isUpgrade" modal :showHeader="false" :closable="false"
            :style="{ width: 'min(300px, calc(100vw - 32px))' }" :draggable="false">
      <div class="upgrade-title">
        <div class="upgrade-title-warp">
          <span class="upgrade-title-warp-txt">{{ t('message.upgrade.title') }}</span>
          <span class="upgrade-title-warp-version">v{{ state.version }}</span>
        </div>
      </div>
      <div class="upgrade-content">
        {{ getThemeConfig.globalTitle }} {{ t('message.upgrade.msg') }}
      </div>
      <div class="upgrade-btn">
        <Button severity="secondary" text rounded @click="onCancel">{{
            t('message.upgrade.btnOne')
          }}
        </Button>
        <Button rounded @click="onUpgrade" :loading="state.isLoading">{{
            state.btnTxt
          }}
        </Button>
      </div>
    </Dialog>
  </div>
</template>

<script setup lang="ts" name="layoutUpgrade">
import {reactive, computed, onMounted} from 'vue';
import {storeToRefs} from 'pinia';
import {useThemeConfigStore} from '../../stores/themeConfig';

import Dialog from 'primevue/dialog';
import Button from 'primevue/button';

// 定义变量内容
const storesThemeConfig = useThemeConfigStore();
const {themeConfig} = storeToRefs(storesThemeConfig);
const state = reactive({
  isUpgrade: false,
  version: __NEXT_VERSION__,
  isLoading: false,
  btnTxt: '',
});

// 获取布局配置信息
const getThemeConfig = computed(() => {
  return themeConfig.value;
});
// 残忍拒绝
const onCancel = () => {
  state.isUpgrade = false;
};
// 马上更新
const onUpgrade = () => {
  state.isLoading = true;
  state.btnTxt = t('message.upgrade.btnTwoLoading');
  setTimeout(() => {
    // local.clear();
    window.location.reload();
    // local.set('version', state.version);
  }, 2000);
};
// 延迟显示，防止刷新时界面显示太快
const delayShow = () => {
  setTimeout(() => {
    state.isUpgrade = true;
  }, 2000);
};
// 页面加载时
onMounted(() => {
  delayShow();
  setTimeout(() => {
    state.btnTxt = t('message.upgrade.btnTwo');
  }, 200);
});
</script>

<style scoped lang="scss">
.upgrade-dialog {
  :deep(.p-dialog) {
    .p-dialog-content {
      padding: 0;
    }

    .upgrade-title {
      text-align: center;
      height: 130px;
      display: flex;
      align-items: center;
      justify-content: center;
      position: relative;

      &::after {
        content: '';
        position: absolute;
        background-color: var(--p-primary-100);
        width: 130%;
        height: 130px;
        border-bottom-left-radius: 100%;
        border-bottom-right-radius: 100%;
      }

      .upgrade-title-warp {
        z-index: 1;
        position: relative;

        .upgrade-title-warp-txt {
          color: var(--p-text-color);
          font-size: 22px;
          letter-spacing: 3px;
        }

        .upgrade-title-warp-version {
          color: var(--p-primary-contrast-color);
          background-color: var(--p-primary-400);
          font-size: 12px;
          position: absolute;
          display: flex;
          top: -2px;
          right: -50px;
          padding: 2px 4px;
          border-radius: 2px;
        }
      }
    }

    .upgrade-content {
      padding: 20px;
      line-height: 22px;

    }

    .upgrade-btn {
      border-top: 1px solid var(--p-content-border-color);
      display: flex;
      justify-content: space-around;
      padding: 15px 20px;

      .p-button {
        width: 100%;
      }
    }
  }
}
</style>
