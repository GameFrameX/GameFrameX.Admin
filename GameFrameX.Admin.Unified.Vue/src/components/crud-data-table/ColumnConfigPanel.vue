<template>
  <!-- 列设置面板（change C146）：PrimeVue Popover 承载，仅展示可配置（非 sensitive）列。
       sensitive 列由调用方保证不进入 configurableColumns，因此面板天然不可见、不可被用户取消隐藏。 -->
  <Popover ref="popoverRef" class="column-config-popover">
    <div class="column-config-panel">
      <div class="panel-title">{{ t('common.table.columnDisplay') }}</div>
      <ul class="panel-list">
        <li v-for="(col, idx) in sortedColumns" :key="col.field" class="panel-row">
          <Button
            icon="fa-solid fa-arrow-up"
            text
            size="small"
            severity="secondary"
            :disabled="idx === 0"
            :aria-label="t('common.table.dragToSort')"
            @click="emit('move-up', col.field)"
          />
          <Button
            icon="fa-solid fa-arrow-down"
            text
            size="small"
            severity="secondary"
            :disabled="idx === sortedColumns.length - 1"
            :aria-label="t('common.table.dragToSort')"
            @click="emit('move-down', col.field)"
          />
          <Checkbox
            :modelValue="col.visible"
            :binary="true"
            :inputId="`col-cfg-${col.field}`"
            @update:modelValue="(v: boolean) => emit('toggle-visible', col.field, v)"
          />
          <label :for="`col-cfg-${col.field}`" class="panel-label">{{ col.header }}</label>
        </li>
      </ul>
      <div v-if="sortedColumns.length === 0" class="panel-empty">{{ t('common.table.noData') }}</div>
      <div class="panel-footer">
        <Button
          :label="t('common.button.reset')"
          text
          size="small"
          severity="secondary"
          @click="emit('reset')"
        />
      </div>
    </div>
  </Popover>
</template>

<script setup lang="ts">
import {computed, ref} from 'vue'
import Popover from 'primevue/popover'
import Button from 'primevue/button'
import Checkbox from 'primevue/checkbox'
import type {ColumnRuntimeState} from '../../composables/useTableColumnPersistence'

const props = defineProps<{
  configurableColumns: ColumnRuntimeState[]
}>()

const emit = defineEmits<{
  'toggle-visible': [field: string, visible: boolean]
  'move-up': [field: string]
  'move-down': [field: string]
  reset: []
}>()

const popoverRef = ref()

// 面板按 order 升序展示，与表格列顺序保持一致；首/尾分别禁用上移/下移
const sortedColumns = computed(() =>
  props.configurableColumns.slice().sort((a, b) => a.order - b.order),
)

/** 由工具栏齿轮按钮调用，切换面板显隐 */
function toggle(event: Event) {
  popoverRef.value?.toggle(event)
}

defineExpose({toggle})
</script>

<style scoped lang="scss">
.column-config-panel {
  min-width: 240px;
  max-width: 320px;

  .panel-title {
    font-weight: 600;
    margin-bottom: 8px;
    color: var(--p-text-color);
  }

  .panel-list {
    list-style: none;
    margin: 0;
    padding: 0;
    display: flex;
    flex-direction: column;
    gap: 2px;
  }

  .panel-row {
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 2px 0;
  }

  .panel-label {
    cursor: pointer;
    user-select: none;
    font-size: 14px;
    color: var(--p-text-color);
  }

  .panel-empty {
    color: var(--p-surface-500);
    font-size: 12px;
    padding: 8px 0;
    text-align: center;
  }

  .panel-footer {
    margin-top: 8px;
    display: flex;
    justify-content: flex-end;
    border-top: 1px solid var(--p-surface-200);
    padding-top: 8px;
  }
}
</style>
