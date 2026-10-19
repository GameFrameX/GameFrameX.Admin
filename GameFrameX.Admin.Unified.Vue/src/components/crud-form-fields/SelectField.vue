<template>
  <FormLabel :name="name" :required="required">{{ label }}</FormLabel>
  <FormField :name="name" v-slot="{ props: fieldProps, invalid, error }">
    <Select
      v-bind="{...fieldProps, ...attrs}"
      v-model="value"
      :options="options"
      :optionLabel="optionLabel"
      :optionValue="optionValue"
      :placeholder="placeholder"
      class="w-full"
    />
    <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
  </FormField>
</template>

<script lang="ts" setup>
import {useAttrs} from 'vue'
import FormField from '@primevue/forms/formfield'
import Select from 'primevue/select'
import Message from 'primevue/message'
import {FormLabel} from '../form-label'

defineOptions({inheritAttrs: false})

defineProps<{
  name: string
  label: string
  options?: any[]
  optionLabel?: string
  optionValue?: string
  placeholder?: string
  required?: boolean
}>()

const value = defineModel<any>()
const attrs = useAttrs()
</script>
