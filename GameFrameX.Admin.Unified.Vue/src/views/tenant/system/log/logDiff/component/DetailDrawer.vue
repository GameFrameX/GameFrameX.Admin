<template>
  <Drawer v-model:visible="visible" :header="t('system.log.logDetail')" position="right" :pt="{ root: { style: 'width: 60vw' } }">
    <template v-if="log">
      <div class="detail-summary">
        <Tag v-if="log.diffType" :value="log.diffType" :severity="diffTypeSeverity"/>
        <span class="detail-meta">{{ t('system.log.elapsed') }}: {{ log.elapsed ?? 0 }} ms</span>
        <span v-if="log.createdTime" class="detail-meta">{{ log.createdTime }}</span>
      </div>

      <div class="detail-section">
        <div class="detail-section-title">{{ t('system.log.differenceBefore') }}</div>
        <DiffFields :raw="log.beforeData"/>
      </div>
      <div class="detail-section">
        <div class="detail-section-title">{{ t('system.log.differenceAfter') }}</div>
        <DiffFields :raw="log.afterData"/>
      </div>
      <div class="detail-section">
        <div class="detail-section-title">{{ t('system.log.differenceField') }}</div>
        <DiffFields :raw="log.snapshot"/>
      </div>

      <div class="detail-section">
        <div class="detail-section-title">Sql</div>
        <pre class="font-mono detail-pre">{{ prettySql }}</pre>
      </div>
      <div class="detail-section">
        <div class="detail-section-title">{{ t('system.log.requestParam') }}</div>
        <pre class="font-mono detail-pre">{{ prettyParameters }}</pre>
      </div>
    </template>
  </Drawer>
</template>

<script lang="ts" setup name="diffDetailDrawer">
import { computed } from 'vue';
import Drawer from 'primevue/drawer';
import { LogDiffFields } from '../../../../../../api/_shared/data-contracts.ts';
import DiffFields from './DiffFields.vue';

const props = defineProps<{
  modelValue: boolean;
  log: LogDiffFields | null;
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void;
}>();

const visible = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v),
});

const diffTypeSeverity = computed<'success' | 'info' | 'danger' | undefined>(() => {
  switch (props.log?.diffType?.toLowerCase()) {
    case 'insert':
      return 'success';
    case 'update':
      return 'info';
    case 'delete':
      return 'danger';
    default:
      return undefined;
  }
});

const prettyParameters = computed(() => {
  if (!props.log?.parameters) {
    return '-';
  }
  try {
    return JSON.stringify(JSON.parse(props.log.parameters), null, 2);
  } catch {
    return props.log.parameters;
  }
});

const prettySql = computed(() => {
  if (!props.log?.sql) {
    return '-';
  }
  return props.log.sql
    .replace(/\s*\r?\n\s*/g, ' ')
    .replace(/\s*,\s*/g, ',\n  ')
    .replace(/\s+WHERE\b/gi, '\nWHERE')
    .replace(/\s+VALUES\b/gi, '\nVALUES')
    .replace(/\s+SET\b/gi, '\nSET')
    .replace(/\s+AND\b/gi, '\n  AND')
    .replace(/\s+OR\b/gi, '\n  OR')
    .trim();
});
</script>

<style scoped>
.detail-summary {
  display: flex;
  align-items: center;
  gap: 16px;
  padding-bottom: 12px;
  margin-bottom: 16px;
  border-bottom: 1px solid var(--p-content-border-color);
}
.detail-meta {
  font-size: 12px;
  color: var(--p-text-muted-color);
}
.detail-section {
  margin-bottom: 16px;
}
.detail-section-title {
  font-weight: 600;
  margin-bottom: 6px;
}
.detail-pre {
  margin: 0;
  padding: 10px;
  background: var(--p-surface-100);
  border-radius: 4px;
  font-size: 12px;
  line-height: 1.6;
  white-space: pre-wrap;
  word-break: break-all;
  max-height: 50vh;
  overflow: auto;
}
</style>
