export interface AppWorkspaceOption {
  id?: number | string | null
  name?: string | null
  appName?: string | null
  isEnabled?: boolean | null
}

export type AppWorkspaceId = number | string
const toId = (value: unknown): AppWorkspaceId | undefined => {
  if (value === undefined || value === null || value === '') {
    return undefined
  }
  const text = String(value).trim()
  if (!/^[1-9]\d*$/.test(text)) {
    return undefined
  }
  const id = Number(text)
  return Number.isSafeInteger(id) ? id : text
}

export function normalizeAppId(value: unknown): AppWorkspaceId | undefined {
  return toId(Array.isArray(value) ? value[0] : value)
}

export function normalizeRouteAppId(value: unknown): AppWorkspaceId | undefined {
  const source = value && typeof value === 'object' && 'query' in value
    ? (value as { query?: Record<string, unknown> }).query
    : value
  return normalizeAppId((source as Record<string, unknown> | undefined)?.appId)
}

export function visibleApps(apps: AppWorkspaceOption[] = []): AppWorkspaceOption[] {
  return apps.filter((app) => app && app.isEnabled !== false && toId(app.id) !== undefined)
}
