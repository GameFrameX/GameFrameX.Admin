import type {InjectionKey, Ref} from 'vue'

export const requiredFieldsKey: InjectionKey<Ref<ReadonlySet<string>>> = Symbol('requiredFields')
