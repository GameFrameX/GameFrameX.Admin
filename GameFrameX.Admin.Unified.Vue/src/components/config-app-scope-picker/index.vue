<template>
  <div class="config-app-scope-picker">
    <SelectButton
      :modelValue="scope"
      @update:modelValue="onScopeChange"
      :options="scopeOptions"
      optionLabel="label"
      optionValue="value"
      :disabled="disabled"
    />
    <Select
      v-if="scope === 'app'"
      :modelValue="selectedAppId"
      @update:modelValue="onAppChange"
      :options="appOptions"
      optionLabel="name"
      optionValue="id"
      filter
      :placeholder="t('gameBusiness.configScope.pleaseSelectApp')"
      class="scope-app"
      :disabled="disabled"
    />
  </div>
</template>

<script lang="ts" setup>
import {ref, watch} from 'vue'
import type {AppOption} from '../../composables/useCurrentApp'

const props = withDefaults(
  defineProps<{
    modelValue: number | string | undefined | null
    appOptions?: AppOption[]
    disabled?: boolean
  }>(),
  {appOptions: () => [], disabled: false}
)
const emit = defineEmits(['update:modelValue'])

const scopeOptions = [
  {label: t('gameBusiness.configScope.tenantDefault'), value: 'tenant'},
  {label: t('gameBusiness.configScope.appOverride'), value: 'app'},
]

// C270：appId 运行时为字符串（雪花 Id），全程保持原值透传，禁止 Number() 回转（超 2^53 丢精度）。
// 0 / '0' / 空 统一表示「租户默认」。
const isTenantScopeValue = (v: number | string | undefined | null): boolean =>
  v === undefined || v === null || v === '' || v === 0 || v === '0'

const scope = ref<'tenant' | 'app'>(isTenantScopeValue(props.modelValue) ? 'tenant' : 'app')
const selectedAppId = ref<number | string>(isTenantScopeValue(props.modelValue) ? 0 : (props.modelValue as number | string))

// 外部值变化时同步内部状态（如编辑回填）
watch(
  () => props.modelValue,
  (v) => {
    const nextScope = isTenantScopeValue(v) ? 'tenant' : 'app'
    if (scope.value !== nextScope) scope.value = nextScope
    if (nextScope === 'app' && selectedAppId.value !== v) selectedAppId.value = v as number | string
  }
)

const onScopeChange = (val: 'tenant' | 'app') => {
  scope.value = val
  if (val === 'tenant') {
    emit('update:modelValue', 0)
  }
  // 切到「应用覆盖」时不立即提交，等用户在 Select 中选定具体应用后再 emit
}

const onAppChange = (val: number | string) => {
  selectedAppId.value = val ?? 0
  emit('update:modelValue', val ?? 0)
}
</script>

<style scoped>
.config-app-scope-picker {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  align-items: center;
}

.scope-app {
  min-width: 220px;
}
</style>
