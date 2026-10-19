<template>
  <li
      class="layout-navbars-tagsview-ul-li"
      :data-url="item.url"
      :class="{ 'is-active': active }"
      role="tab"
      :tabindex="0"
      :aria-selected="active ? 'true' : undefined"
      @contextmenu.prevent="$emit('contextmenu', item, $event)"
      @mousedown="$emit('mousedown', item, $event)"
      @click="$emit('click', item, index)"
      @keydown.enter.prevent="$emit('click', item, index)"
      @keydown.space.prevent="$emit('click', item, index)"
  >
    <i class="fa-solid fa-globe layout-navbars-tagsview-ul-li-iconfont" v-if="active"></i>
    <SvgIcon :name="item.meta.icon" v-if="!active" class="pr5"/>
    <span>{{ label }}</span>
    <template v-if="active">
      <button
          type="button"
          class="layout-navbars-tagsview-ul-li-action layout-navbars-tagsview-ul-li-refresh"
          :aria-label="t('message.common.button.refresh')"
          @click.stop="$emit('refresh')"
      >
        <SvgIcon name="fa-solid fa-rotate-right"/>
      </button>
      <button
          v-if="!item.meta.isAffix"
          type="button"
          class="layout-navbars-tagsview-ul-li-action layout-icon-active"
          :aria-label="t('message.common.button.close')"
          @click.stop="$emit('close', item.path)"
      >
        <SvgIcon name="fa-solid fa-xmark"/>
      </button>
    </template>
    <button
        v-if="!item.meta.isAffix"
        type="button"
        class="layout-navbars-tagsview-ul-li-action layout-icon-three"
        :aria-label="t('message.common.button.close')"
        @click.stop="$emit('close', item.path)"
    >
      <SvgIcon name="fa-solid fa-xmark"/>
    </button>
  </li>
</template>

<script setup lang="ts" name="TagsViewItem">
// 单个 tagsView 标签项：渲染与样式自包含，交互通过 emit 上抛给容器（tagsView.vue）调度
import {computed} from 'vue';
import other from '/@/utils/other';
import {locale} from '/@/i18n';

const props = defineProps<{
  item: RouteItem;
  active: boolean;
  index: number;
}>();

defineEmits<{
  (e: 'contextmenu', item: RouteItem, evt: MouseEvent): void;
  (e: 'mousedown', item: RouteItem, evt: MouseEvent): void;
  (e: 'click', item: RouteItem, index: number): void;
  (e: 'refresh'): void;
  (e: 'close', path: string): void;
}>();

// 标签标题随语言切换重算：item 引用稳定时本组件会被跳过重渲染，
// 此处显式依赖 locale ref 保证已打开标签在切换语言后立即更新
const label = computed(() => {
  void locale.value;
  return other.setTagsViewNameI18n(props.item);
});
</script>

<style scoped lang="scss">
// 档案索引标签（纸面剧作卡世界）：无边框盒，安静行；激活 = 墨色文字 + 底缘 2px 墨线
.layout-navbars-tagsview-ul-li {
  min-height: 36px;
  line-height: 1.4;
  display: flex;
  align-items: center;
  border: 0;
  border-bottom: 2px solid transparent;
  padding: 0 12px;
  margin-right: 5px;
  border-radius: 0;
  position: relative;
  z-index: 0;
  cursor: pointer;
  justify-content: space-between;
  color: var(--p-text-muted-color);
  background: transparent;
  transition: color 0.15s ease, border-color 0.15s ease;

  &:focus-visible {
    outline: 2px solid var(--p-primary-color);
    outline-offset: -2px;
  }

  &:hover {
    color: var(--p-primary-color);
  }

  // 按压反馈（GFX-661）：缩放动画受全局 prefers-reduced-motion 豁免
  &:active {
    transform: scale(0.96);
  }

  &-iconfont {
    position: relative;
    left: -5px;
    font-size: 12px;
  }

  &-icon {
    border-radius: 100%;
    position: relative;
    height: 14px;
    width: 14px;
    text-align: center;
    line-height: 14px;
    right: -5px;

    &:hover {
      color: var(--p-primary-color);
    }
  }

  &-action {
    // 触控目标 36px（GFX-661）：375px 视口下也可点准
    width: 36px;
    height: 36px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    margin-left: 4px;
    border: 0;
    border-radius: 50%;
    background: transparent;
    color: inherit;
    cursor: pointer;

    &:hover {
      color: var(--p-primary-color);
      background: color-mix(in srgb, var(--p-primary-100) 55%, transparent);
    }

    // 按压反馈（GFX-661）
    &:active {
      color: var(--p-primary-color);
      background: color-mix(in srgb, var(--p-primary-100) 80%, transparent);
      transform: scale(0.92);
    }
  }

  .layout-icon-active {
    display: block;
  }

  .layout-icon-three {
    display: none;
  }
}

.is-active {
  color: var(--p-primary-color);
  font-weight: 600;
  border-bottom-color: var(--p-primary-color);
}
</style>
