<template>
  <div
    class="search-form"
    :style="{
      '--search-form-columns': String(columnsPerRow),
      '--search-form-label-width': labelWidthPx,
    }"
  >
    <div class="search-form-grid">
      <div
        v-for="field in visibleFields"
        :key="field.key"
        class="search-form-field"
        :style="getFieldGridStyle(field)"
      >
        <label class="search-form-label" :for="fieldInputId(field)">{{ field.label }}</label>
        <div class="search-form-control">
          <IconField v-if="field.key === '__searchKey__'">
            <InputText :id="fieldInputId(field)" v-model="modelValue.searchKey" class="w-full"
                      @keyup.enter="emit('query')"
                      :placeholder="t('message.gameBusiness.fuzzySearchKeyword')" />
            <InputIcon v-if="modelValue.searchKey" @click="modelValue.searchKey = ''" class="cursor-pointer">
              <i :class="faXmark.className" role="button" :tabindex="0"
                 :aria-label="t('message.common.button.clear')"
                 @keydown.enter.prevent="modelValue.searchKey = ''"
                 @keydown.space.prevent="modelValue.searchKey = ''"></i>
            </InputIcon>
          </IconField>
          <slot v-else-if="$slots[`field-${field.key}`]" :name="`field-${field.key}`" :model="modelValue" :field="field" />
          <IconField v-else-if="field.type === 'input'">
            <InputText :id="fieldInputId(field)" v-model="modelValue[field.key]"
                       :placeholder="field.placeholder" v-bind="field.props" class="w-full"
                       @keyup.enter="emit('query')" />
            <InputIcon v-if="modelValue[field.key]" @click="modelValue[field.key] = ''" class="cursor-pointer">
              <i :class="faXmark.className" role="button" :tabindex="0"
                 :aria-label="t('message.common.button.clear')"
                 @keydown.enter.prevent="modelValue[field.key] = ''"
                 @keydown.space.prevent="modelValue[field.key] = ''"></i>
            </InputIcon>
          </IconField>
          <AutoComplete v-else-if="field.type === 'select'"
                        :modelValue="getSelectDisplayValue(field)"
                        @update:modelValue="(value: any) => updateSelectValue(field, value)"
                        :suggestions="filteredOptions[field.key] ?? []"
                        @complete="(e: { query: string }) => onAutoComplete(e, field)"
                        :optionLabel="field.optionLabel ?? 'label'"
                        :showClear="field.showClear ?? true"
                        :inputId="fieldInputId(field)"
                        :forceSelection="field.forceSelection ?? true"
                        :placeholder="field.placeholder" dropdown v-bind="field.props" class="w-full" />
          <IconField v-else-if="field.type === 'number'">
            <InputNumber v-model="modelValue[field.key]"
                         :inputId="fieldInputId(field)"
                         :placeholder="field.placeholder" v-bind="field.props" class="w-full" />
            <InputIcon v-if="modelValue[field.key] != null" @click="modelValue[field.key] = null" class="cursor-pointer">
              <i :class="faXmark.className" role="button" :tabindex="0"
                 :aria-label="t('message.common.button.clear')"
                 @keydown.enter.prevent="modelValue[field.key] = null"
                 @keydown.space.prevent="modelValue[field.key] = null"></i>
            </InputIcon>
          </IconField>
          <DatePicker v-else-if="field.type === 'date'" v-model="modelValue[field.key]"
                      :inputId="fieldInputId(field)"
                      v-bind="field.props" class="w-full" />
          <DatePicker v-else-if="field.type === 'dateRange'" v-model="modelValue[field.key]"
                      :inputId="fieldInputId(field)"
                      selectionMode="range" showTime v-bind="field.props" class="w-full" />
          <ToggleSwitch v-else-if="field.type === 'switch'" v-model="modelValue[field.key]" :inputId="fieldInputId(field)" v-bind="field.props" />
        </div>
      </div>
      <div class="search-form-actions">
          <Button @click="emit('query')" v-auth="queryPermission" class="search-form-button">
            <i :class="faMagnifyingGlass.className" class="mr-1"></i>{{ queryLabel }}
          </Button>
          <Button v-if="showReset" severity="secondary" @click="onReset" class="search-form-button">{{ resetLabel }}</Button>
          <Button v-if="showAdd" @click="emit('add')" v-auth="addPermission" class="search-form-button">
            <i :class="faPlus.className" class="mr-1"></i>{{ addLabel }}
          </Button>
          <slot name="buttons" />
          <Button v-if="hasAdvanced" severity="secondary" @click="showAdvanced = !showAdvanced" class="search-form-button">
            <i :class="showAdvanced ? faMagnifyingGlassMinus.className : faMagnifyingGlassPlus.className" class="mr-1"></i>
            {{ showAdvanced ? hideLabel : advancedLabel }}
          </Button>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed } from 'vue'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import AutoComplete from 'primevue/autocomplete'
import InputNumber from 'primevue/inputnumber'
import DatePicker from 'primevue/datepicker'
import ToggleSwitch from 'primevue/toggleswitch'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import { faMagnifyingGlass, faMagnifyingGlassPlus, faMagnifyingGlassMinus, faPlus, faXmark } from '../../utils/icons'

export interface SearchFormField {
  key: string
  label: string
  type?: 'input' | 'select' | 'number' | 'date' | 'dateRange' | 'switch'
  placeholder?: string
  options?: any[]
  optionLabel?: string
  optionValue?: string
  forceSelection?: boolean
  showClear?: boolean
  advanced?: boolean
  span?: number
  props?: Record<string, any>
}

const props = withDefaults(defineProps<{
  fields: SearchFormField[]
  modelValue: Record<string, any>
  labelWidth?: string | number
  columns?: number
  showReset?: boolean
  showAdd?: boolean
  authPrefix?: string
  queryAuth?: string
  addAuth?: string
  queryLabel?: string
  resetLabel?: string
  addLabel?: string
  advancedLabel?: string
  hideLabel?: string
  defaultSearchKey?: boolean
  defaultSearchKeyLabel?: string
}>(), {
  labelWidth: '84px',
  showReset: true,
  showAdd: false,
  queryLabel: '',
  resetLabel: '',
  addLabel: '',
  advancedLabel: '',
  hideLabel: '',
  defaultSearchKey: true,
  defaultSearchKeyLabel: '',
})

const emit = defineEmits<{
  (e: 'update:modelValue', value: Record<string, any>): void
  (e: 'query'): void
  (e: 'reset'): void
  (e: 'add'): void
}>()

const queryPermission = computed(() => props.queryAuth ?? (props.authPrefix ? `${props.authPrefix}:page` : undefined))
const addPermission = computed(() => props.addAuth ?? (props.authPrefix ? `${props.authPrefix}:add` : undefined))

const showAdvanced = ref(false)
const hasAdvanced = computed(() => props.fields.some((f) => f.advanced))
const hasDefaultSearchKey = computed(() => !props.fields.some((f) => f.key === 'searchKey'))

const filteredOptions = ref<Record<string, any[]>>({})

function onAutoComplete(event: { query: string }, field: SearchFormField) {
  const options = Array.isArray(field.options) ? field.options : []
  const labelKey = field.optionLabel ?? 'label'
  const query = event.query.trim().toLowerCase()
  filteredOptions.value[field.key] = query
    ? options.filter((opt) => String(getOptionLabel(opt, labelKey)).toLowerCase().includes(query))
    : [...options]
}

function getSelectDisplayValue(field: SearchFormField) {
  const value = props.modelValue?.[field.key]
  if (value == null || typeof value === 'object') return value ?? null

  const optionValue = field.optionValue ?? 'value'
  return (field.options ?? []).find((option) => getOptionValue(option, optionValue) === value) ?? value
}

function updateSelectValue(field: SearchFormField, value: any) {
  if (value == null || value === '') {
    props.modelValue[field.key] = null
    return
  }

  const optionValue = field.optionValue ?? 'value'
  props.modelValue[field.key] = typeof value === 'object' ? getOptionValue(value, optionValue) : value
}

function getOptionLabel(option: any, labelKey: string) {
  return option != null && typeof option === 'object' ? option[labelKey] : option
}

function getOptionValue(option: any, valueKey: string) {
  return option != null && typeof option === 'object' ? option[valueKey] : option
}

const columnsPerRow = computed(() => props.columns || 4)

const visibleFields = computed(() => {
  const fields = props.fields.filter((f) => !f.advanced || showAdvanced.value)
  if (props.defaultSearchKey && hasDefaultSearchKey.value) {
    return [
      { key: '__searchKey__', label: props.defaultSearchKeyLabel || t('message.gameBusiness.keyword'), type: 'input' as const },
      ...fields,
    ]
  }
  return fields
})

const labelWidthPx = computed(() => {
  const w = props.labelWidth
  return typeof w === 'number' ? `${w}px` : w
})

const fieldInputId = (field: SearchFormField) => `search-form-${field.key}`

const getFieldGridStyle = (field: SearchFormField) => {
  const span = Math.max(1, Math.min(field.span ?? 1, columnsPerRow.value))
  return span > 1 ? { gridColumn: `span ${span}` } : undefined
}

const queryLabel = computed(() => props.queryLabel || t('message.common.button.search'))
const resetLabel = computed(() => props.resetLabel || t('message.common.button.reset'))
const addLabel = computed(() => props.addLabel || t('message.common.button.add'))
const advancedLabel = computed(() => props.advancedLabel || t('message.gameBusiness.advanced'))
const hideLabel = computed(() => props.hideLabel || t('message.gameBusiness.hide'))

const onReset = () => {
  if (props.modelValue) {
    // 只清空表单声明字段（含默认关键字）：modelValue 还承载页面注入的上下文键
    // （如应用维度页面的 appId，来自 URL query），一并清掉会让后端必填校验 500。
    const fieldKeys = new Set(props.fields.map((f) => f.key))
    if (props.defaultSearchKey) {
      fieldKeys.add('searchKey')
    }
    Object.keys(props.modelValue).forEach((key) => {
      if (fieldKeys.has(key)) {
        props.modelValue[key] = null
      }
    })
  }
  emit('reset')
  // 重置即回到默认列表：多数页面未绑定 @reset，这里补发 query 保证清空条件后表格刷新，
  // 已绑定 @reset 的页面至多多发一次同参幂等查询。
  emit('query')
}
</script>

<style scoped lang="scss">
.search-form {
  width: 100%;
}

.search-form-grid {
  display: grid;
  grid-template-columns: repeat(var(--search-form-columns), minmax(0, 1fr));
  gap: 10px 16px;
  align-items: start;
}

.search-form-field {
  min-width: 0;
  display: grid;
  grid-template-columns: var(--search-form-label-width) minmax(0, 1fr);
  gap: 8px;
  align-items: center;
}

.search-form-label {
  min-width: 0;
  color: var(--p-text-muted-color);
  text-align: right;
  line-height: 1.4;
}

.search-form-control {
  min-width: 0;
}

.search-form-actions {
  grid-column: 1 / -1;
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  align-items: center;
  padding-top: 2px;
}

.search-form-button {
  min-height: 40px;
}

@media (max-width: 1024px) {
  .search-form-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 640px) {
  .search-form-grid {
    grid-template-columns: minmax(0, 1fr);
  }

  .search-form-field {
    grid-template-columns: minmax(0, 1fr);
    gap: 4px;
  }

  .search-form-label {
    text-align: left;
  }

  .search-form-button {
    flex: 1 1 120px;
  }
}
</style>
