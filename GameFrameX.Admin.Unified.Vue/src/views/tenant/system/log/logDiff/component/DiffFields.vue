<template>
  <div class="diff-fields">
    <span v-if="tables.length === 0" class="diff-empty">-</span>
    <div v-for="table in tables" :key="table.tableName" class="diff-table-group">
      <div v-if="tables.length > 1" class="diff-table-name">
        {{ table.tableDescription || table.tableName }}
      </div>
      <div v-for="col in table.columns" :key="col.columnName" class="diff-field-row">
        <span class="diff-field-name">{{ col.columnDescription || col.columnName }}</span>
        <span class="diff-field-sep">:</span>
        <span class="diff-field-value">{{ formatValue(col.value) }}</span>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup name="diffFields">
import { computed } from 'vue';

interface DiffColumn {
  columnName?: string;
  columnDescription?: string;
  value?: any;
  isPrimaryKey?: boolean;
}
interface DiffTable {
  tableName?: string;
  tableDescription?: string;
  columns?: DiffColumn[];
}

const props = defineProps<{ raw?: string | null }>();

const tables = computed<DiffTable[]>(() => {
  if (!props.raw) {
    return [];
  }
  try {
    const parsed = JSON.parse(props.raw);
    return Array.isArray(parsed) ? (parsed as DiffTable[]) : [];
  } catch {
    return [];
  }
});

const formatValue = (value: any): string => {
  if (value === null || value === undefined) {
    return '(空)';
  }
  if (typeof value === 'object') {
    return JSON.stringify(value);
  }
  return String(value);
};
</script>

<style scoped>
.diff-fields {
  font-size: 12px;
  line-height: 1.6;
}
.diff-empty {
  color: var(--text-color-secondary, #999);
}
.diff-table-group {
  margin-bottom: 4px;
}
.diff-table-name {
  font-weight: 600;
  color: var(--text-color-secondary, #999);
  margin-bottom: 2px;
}
.diff-field-row {
  display: flex;
  gap: 4px;
  word-break: break-all;
}
.diff-field-name {
  color: var(--text-color-secondary, #999);
  white-space: nowrap;
}
.diff-field-value {
  flex: 1;
}
</style>
