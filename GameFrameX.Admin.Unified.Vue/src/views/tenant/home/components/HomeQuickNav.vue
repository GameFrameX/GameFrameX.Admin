<template>
  <nav class="home-quick" :aria-label="ariaLabel">
    <RouterLink
        v-for="entry in visibleEntries"
        :key="entry.path"
        class="quick-item"
        :to="entry.path">
      <span class="quick-icon">
        <component :is="entry.icon"/>
      </span>
      <span class="quick-label">{{ entry.label }}</span>
    </RouterLink>
    <div v-if="visibleEntries.length === 0" class="quick-empty">{{ t('message.home.quickEmpty') }}</div>
  </nav>
</template>

<script setup lang="ts" name="HomeQuickNav">
// HOME-001 快捷入口：入口集由父级传入（三层驾驶舱各喂各的路由），
// 本组件只负责渲染与可达性过滤（router.resolve 检查，未注册路由自动隐藏）。
import {computed, type Component} from 'vue';
import {useRouter} from 'vue-router';

export interface QuickNavEntry {
  path: string;
  icon: Component;
  label: string;
}

const props = defineProps<{
  entries: QuickNavEntry[];
  ariaLabel: string;
}>();

const router = useRouter();

const visibleEntries = computed(() => {
  return props.entries.filter((entry) => {
    const resolved = router.resolve(entry.path);
    return resolved.matched.length > 0 && resolved.name !== 'notFound';
  });
});
</script>

<style scoped lang="scss">
.home-quick {
  display: grid;
  grid-template-columns: repeat(6, minmax(0, 1fr));
  gap: 10px;

  .quick-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 8px;
    text-decoration: none;
    color: var(--p-text-color);

    &:focus-visible {
      outline: 2px solid var(--p-primary-color);
      outline-offset: 2px;
      border-radius: var(--p-content-border-radius);
    }
  }

  .quick-icon {
    width: 42px;
    height: 42px;
    border: 1px solid var(--p-content-border-color);
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    color: var(--p-primary-color);
    font-size: 16px;
    transition: border-color 0.15s ease;
  }

  .quick-item:hover .quick-icon {
    border-color: var(--p-primary-300);
  }

  .quick-label {
    font-size: 13px;
  }

  .quick-empty {
    grid-column: 1 / -1;
    color: var(--p-text-muted-color);
    font-size: 13px;
    padding: 14px 2px;
  }

  @include bp-down(md) {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }

  @include bp-down(sm) {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
</style>
