export type IsEnabledValue = boolean | null | undefined | {
    isEnabled?: boolean | null;
};

export function isEnabled(value: IsEnabledValue): boolean {
    if (value && typeof value === 'object') {
        return value.isEnabled !== false;
    }

    return value !== false;
}

export function normalizeIsEnabled<T>(row: T): T {
    if (!row || typeof row !== 'object') return row;

    const record = row as Record<string, unknown>;
    if (!('isEnabled' in record)) return row;

    record.isEnabled = isEnabled(record as IsEnabledValue);
    return row;
}

/**
 * 新增模式下给 IsEnabled 一个安全的默认值 false。
 *
 * 仅当当前值不是布尔（null / undefined / 缺字段 / 其它非布尔）时才写入 `false`；
 * 已显式设置为 `true` / `false` 的原样返回，避免覆盖页面 `defaultForm` 的显式默认。
 *
 * 仅用于 `useEditDialog` 打开「新增」表单时调用：避免缺省值被
 * {@link normalizeIsEnabled} 的展示语义（缺省视为启用）与 {@link isEnabled}
 * 的提交语义误判为 `true`，造成「界面显示关闭、提交却为启用」的不一致。
 *
 * 编辑回显与列表展示不应调用本函数，保持既有「缺省视为启用」的展示语义。
 */
export function applyDefaultIsEnabledForAdd<T>(row: T): T {
    if (!row || typeof row !== 'object') return row;

    const record = row as Record<string, unknown>;
    const current = record.isEnabled;
    if (current === true || current === false) return row;

    record.isEnabled = false;
    return row;
}
