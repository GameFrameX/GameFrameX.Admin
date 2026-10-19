import type {CrudColumnConfig} from '../components/crud-data-table/types'

type ColumnOverrides = Partial<CrudColumnConfig>

export function nameColumn(overrides: ColumnOverrides = {}): CrudColumnConfig {
  return {field: 'name', header: t('message.common.label.name'), ...overrides}
}

export function descriptionColumn(overrides: ColumnOverrides = {}): CrudColumnConfig {
  return {field: 'description', header: t('message.common.label.description'), ...overrides}
}

export function createdNameColumn(overrides: ColumnOverrides = {}): CrudColumnConfig {
  return {field: 'createdName', header: t('message.common.label.creator'), ...overrides}
}

export function updatedNameColumn(overrides: ColumnOverrides = {}): CrudColumnConfig {
  return {field: 'updatedName', header: t('message.common.label.modifier'), ...overrides}
}

export function nameSearchField(overrides: Record<string, any> = {}) {
  return {
    key: 'name',
    label: t('message.common.label.name'),
    type: 'input' as const,
    placeholder: t('message.common.form.pleaseInput'),
    ...overrides,
  }
}
