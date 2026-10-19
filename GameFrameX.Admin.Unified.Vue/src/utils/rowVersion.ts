export type RowVersionRecord = Record<string, unknown>

export function readRowVersion(row: unknown): unknown {
    if (!row || typeof row !== 'object') return undefined

    const record = row as RowVersionRecord
    return record.rowVersion
}

export function normalizeRowVersion<T>(row: T): T {
    if (!row || typeof row !== 'object') return row

    const value = readRowVersion(row)
    if (value !== undefined && value !== null) {
        (row as RowVersionRecord).rowVersion = value
    }
    return row
}
