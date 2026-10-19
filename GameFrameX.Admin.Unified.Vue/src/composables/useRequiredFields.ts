import {computed, provide} from 'vue'
import type {FormRules} from './types/crud'
import {requiredFieldsKey} from '../components/form-label'

export function useRequiredFields(rules: FormRules | undefined) {
  const requiredFields = computed(() => {
    if (!rules) return new Set<string>()
    const set = new Set<string>()
    for (const [fieldName, fieldRules] of Object.entries(rules)) {
      const ruleList = Array.isArray(fieldRules) ? fieldRules : [fieldRules]
      if (ruleList.some(r => r.required === true)) {
        set.add(fieldName)
      }
    }
    return set as ReadonlySet<string>
  })
  provide(requiredFieldsKey, requiredFields)
}
