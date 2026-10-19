import type {Ref} from 'vue'
import type {FormRules, FormRule} from '../composables/types/crud'

interface ResolverResult {
    errors?: Record<string, string[]>
}

export function buildFormResolver(rules: Ref<FormRules | undefined>) {
    return ({values}: { values: Record<string, any> }): ResolverResult => {
        const currentRules = rules.value
        if (!currentRules) return {}

        const errors: Record<string, string[]> = {}

        for (const [fieldName, fieldRules] of Object.entries(currentRules)) {
            const ruleList: FormRule[] = Array.isArray(fieldRules) ? fieldRules : [fieldRules]
            const value = values[fieldName]
            const fieldErrors: string[] = []

            for (const rule of ruleList) {
                if (rule.required) {
                    if (value == null || value === '' || (Array.isArray(value) && value.length === 0)) {
                        const msg = typeof rule.message === 'function' ? rule.message() : (rule.message || `${fieldName} is required`)
                        fieldErrors.push(msg)
                    }
                }
                if (rule.min != null && typeof value === 'string' && value.length < rule.min) {
                    const msg = typeof rule.message === 'function' ? rule.message() : (rule.message || `Min length is ${rule.min}`)
                    fieldErrors.push(msg)
                }
                if (rule.max != null && typeof value === 'string' && value.length > rule.max) {
                    const msg = typeof rule.message === 'function' ? rule.message() : (rule.message || `Max length is ${rule.max}`)
                    fieldErrors.push(msg)
                }
                if (rule.len != null && typeof value === 'string' && value.length !== rule.len) {
                    const msg = typeof rule.message === 'function' ? rule.message() : (rule.message || `Length must be ${rule.len}`)
                    fieldErrors.push(msg)
                }
                if (rule.pattern && !rule.pattern.test(String(value ?? ''))) {
                    const msg = typeof rule.message === 'function' ? rule.message() : (rule.message || 'Invalid format')
                    fieldErrors.push(msg)
                }
                if (rule.validator) {
                    let validatorError: string | null = null
                    rule.validator(rule, value, (error?: Error | string) => {
                        if (error) validatorError = typeof error === 'string' ? error : error.message
                    })
                    if (validatorError) {
                        fieldErrors.push(validatorError)
                    }
                }
            }

            if (fieldErrors.length > 0) {
                errors[fieldName] = fieldErrors
            }
        }

        return Object.keys(errors).length > 0 ? {errors} : {}
    }
}
