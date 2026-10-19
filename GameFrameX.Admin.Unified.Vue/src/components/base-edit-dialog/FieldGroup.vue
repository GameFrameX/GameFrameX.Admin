<template>
  <div class="base-edit-field-group">
    <!-- divider 样式:分隔线 + 居中标题,字段流入下方 grid -->
    <Divider v-if="showDividerTitle" align="center">
      <span class="base-edit-field-group__title">{{ title }}</span>
    </Divider>

    <!-- fieldset 样式:Fieldset(legend)包裹 grid,分隔感更强 -->
    <Fieldset v-if="variant === 'fieldset' && title" :legend="title">
      <div class="base-edit-field-group__grid">
        <slot/>
      </div>
    </Fieldset>

    <!-- 其余(divider / plain,或 fieldset 无 title 回退):字段直接进 grid -->
    <div v-else class="base-edit-field-group__grid">
      <slot/>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {computed} from 'vue'
import Divider from 'primevue/divider'
import Fieldset from 'primevue/fieldset'

const props = withDefaults(defineProps<{
  title?: string
  variant?: 'divider' | 'fieldset' | 'plain'
}>(), {
  title: '',
  variant: 'divider',
})

const showDividerTitle = computed(() => props.variant === 'divider' && !!props.title)
</script>

<style scoped>
/* 跨满父级 base-edit-dialog-grid 的两列,作为独立分组块 */
.base-edit-field-group {
  grid-column: 1 / -1;
}

.base-edit-field-group__title {
  color: var(--p-text-muted-color);
  font-weight: 500;
  white-space: nowrap;
}

.base-edit-field-group :deep(.p-divider) {
  margin: 16px 0;
}

/* 继承父级下发的 --bed-label-width / --bed-gap,保证分组内外标签对齐、间距一致 */
.base-edit-field-group__grid {
  display: grid;
  grid-template-columns: var(--bed-label-width, 120px) minmax(0, 1fr);
  gap: var(--bed-gap, 12px 16px);
  align-items: start;
}

@media (max-width: 640px) {
  .base-edit-field-group__grid {
    grid-template-columns: minmax(0, 1fr);
  }
}
</style>
