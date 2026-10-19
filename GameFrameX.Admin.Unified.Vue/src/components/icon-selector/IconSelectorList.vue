<template>
  <div class="icon-selector-warp-row">
    <ScrollPanel style="height: 230px">
      <div class="grid" style="gap: 10px; padding: 15px" v-if="props.list.length > 0">
        <div class="col-6 sm:col-4 md:col-4 lg:col-4 xl:col-4" v-for="(v, k) in list" :key="k" @click="onColClick(v)">
          <div class="icon-selector-warp-item" :class="{ 'icon-selector-active': prefix === v }">
            <SvgIcon :name="v"/>
          </div>
        </div>
      </div>
      <div v-if="list.length <= 0" class="icon-selector-empty">
        <div style="text-align: center; padding: 40px 0;">
          <i class="fa-solid fa-inbox" style="font-size: 40px; color: var(--p-surface-400); display: block; margin-bottom: 10px;"></i>
          <span style="color: var(--p-surface-500); font-size: 14px;">{{ empty }}</span>
        </div>
      </div>
    </ScrollPanel>
  </div>
</template>

<script setup lang="ts" name="iconSelectorList">
import ScrollPanel from 'primevue/scrollpanel';
import SvgIcon from '/@/components/svg-icon/index.vue';


const props = defineProps({
  list: {
    type: Array,
    default: () => [],
  },
  empty: {
    type: String,
    default: () => t('message.common.message.noIcon'),
  },
  prefix: {
    type: String,
    default: () => '',
  },
});

const emit = defineEmits(['getIcon']);

const onColClick = (v: unknown | string) => {
  emit('getIcon', v);
};
</script>

<style scoped lang="scss">
.icon-selector-warp-row {
  overflow: hidden;

  .icon-selector-warp-item {
    display: flex;
    justify-content: center;
    align-items: center;
    border: 1px solid var(--p-surface-300);
    border-radius: 5px;
    margin-bottom: 10px;
    // 触控目标 36px（GFX-661）
    height: 36px;

    i {
      font-size: 20px;
      color: var(--p-text-muted-color);
    }

    &:hover {
      cursor: pointer;
      background-color: var(--p-primary-100);
      border: 1px solid var(--p-primary-300);

      i {
        color: var(--p-primary-color);
      }
    }

    // 按压反馈（GFX-661）：缩放动画受全局 prefers-reduced-motion 豁免
    &:active {
      transform: scale(0.94);
    }
  }

  .icon-selector-active {
    background-color: var(--p-primary-100);
    border: 1px solid var(--p-primary-300);

    i {
      color: var(--p-primary-color);
    }
  }
}
</style>
