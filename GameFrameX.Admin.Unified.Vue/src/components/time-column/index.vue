<template>
  <Column :field="field" :header="header" :style="columnStyle" :align="align">
    <template #body="{ data }">
      {{ formatUnix(data[field], format) }}
    </template>
  </Column>
</template>

<script setup lang="ts">
import {computed} from 'vue'
import Column from 'primevue/column'
import {formatUnix} from '../../utils/formatTime'

/**
 * Unix 秒时间戳列（C145：后端 *Time 字段统一 long 秒）。
 * 收口各页表格里反复手写的 `<Column><template #body>{{ formatUnix(data.x) }}</template></Column>` 样板。
 * 数据流仍保留原始 long，仅在此处就近格式化展示。
 */
interface Props {
  /** 行数据字段名（后端 *Time 字段，long 秒） */
  field: string
  /** 列标题 */
  header: string
  /** 列宽，如 '140px'；不传则不设定宽 */
  width?: string
  /** 最小列宽，如 '160px'（对应原 style="min-width:160px"） */
  minWidth?: string
  /** 单元格对齐 */
  align?: 'left' | 'center' | 'right'
  /** 时间格式，默认 'YYYY-mm-dd HH:MM:SS'；仅日期传 'YYYY-mm-dd' */
  format?: string
}

const props = withDefaults(defineProps<Props>(), {
  format: 'YYYY-mm-dd HH:MM:SS',
})

const columnStyle = computed(() => {
  const s: Record<string, string> = {}
  if (props.width) s.width = props.width
  if (props.minWidth) s.minWidth = props.minWidth
  return s
})
</script>
