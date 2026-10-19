<template>
  <label class="form-label">
    <slot/>
    <span v-if="isRequired" class="form-label-required">*</span>
  </label>
</template>

<script lang="ts" setup>
import {computed, inject} from 'vue'
import {requiredFieldsKey} from './key'

const props = defineProps({
  required: {
    type: Boolean,
    default: undefined,
  },
  name: String,
})

const injectedRequiredFields = inject(requiredFieldsKey, undefined)

const isRequired = computed(() => {
  if (props.required === true) return true
  if (props.required === false) return false
  if (props.name && injectedRequiredFields?.value) {
    return injectedRequiredFields.value.has(props.name)
  }
  return false
})
</script>

<style scoped>
.form-label {
  display: inline-flex;
  align-items: center;
  white-space: nowrap;
}

.form-label-required {
  color: var(--p-red-500);
  margin-left: 2px;
  font-weight: bold;
}
</style>
