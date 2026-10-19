/** 兼容 Element Plus 的表单验证规则类型，解耦对 element-plus 的类型依赖 */
export interface FormRule {
    required?: boolean
    message?: string | (() => string)
    trigger?: string | string[]
    min?: number
    max?: number
    len?: number
    pattern?: RegExp
    validator?: (rule: any, value: any, callback: (error?: Error | string) => void) => void

    [key: string]: any
}

export type FormRules = Record<string, FormRule | FormRule[]>

/** 单个 API 函数，接收参数返回 AxiosResponse */
type ApiFn<T = any> = (params?: T) => Promise<any>

/** API 配置 */
interface ApiConfig<T = any> {
    pageApi: ApiFn<T>
    addApi?: ApiFn<T>
    updateApi?: ApiFn<T>
    deleteApi?: ApiFn<T>
}

/** 分页状态 */
interface PaginationState {
    page: number
    pageSize: number
    total: number
}

/**
 * 删除确认文案来源：
 * - string：静态文案，原样展示；
 * - (row)：在 onDelete 打开确认弹窗前求值，允许拼接行内标识（名称/账号等）。
 */
type DeleteConfirmMessage<T> = string | ((row: T) => string)

/** useCrudTable 配置 */
interface UseCrudTableConfig<T = any> {
    api: ApiConfig<T>
    pagination?: Partial<PaginationState>
    immediate?: boolean
    deleteConfirmMessage?: DeleteConfirmMessage<T>
    /** 删除确认文案的行标识候选字段（默认 name > account > title > fileName > nickName > code > jobId > triggerId > id） */
    deleteConfirmLabelFields?: string[]
    deleteSuccessMessage?: string
}

/** useEditDialog 配置 */
interface UseEditDialogConfig<T = any> {
    addApi: ApiFn<T>
    updateApi: ApiFn<T>
    rules?: FormRules
    defaultForm?: () => T
    idCheckMode?: 'game' | 'system'
    cloneStrategy?: 'json' | 'structured' | ((row: any) => any)
    beforeOpen?: (form: T, isEditor: boolean) => T | void | Promise<T | void>
    beforeSubmit?: (form: T, isEditor: boolean) => T | false | Promise<T | false>
    afterSubmit?: (form: T, isEditor: boolean, response?: unknown) => void
}

export type {ApiFn, ApiConfig, PaginationState, UseCrudTableConfig, UseEditDialogConfig, DeleteConfirmMessage}
