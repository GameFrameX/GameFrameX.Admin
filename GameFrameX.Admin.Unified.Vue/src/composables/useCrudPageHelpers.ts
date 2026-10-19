import {watch, type Ref} from 'vue'
import type {RouteLocationNormalizedLoaded} from 'vue-router'
import type {PaginationState} from './types/crud'

export interface PrimeTablePageEvent {
  page: number
  rows: number
}

export function usePrimeTablePage(
  tableParams: Ref<PaginationState>,
  onQuery: () => Promise<void> | void,
) {
  return async (event: PrimeTablePageEvent) => {
    tableParams.value.pageSize = event.rows
    tableParams.value.page = event.page + 1
    await onQuery()
  }
}

export function useRouteAppIdQuerySync(options: {
  route: RouteLocationNormalizedLoaded
  queryParams: Ref<Record<string, unknown>>
  resolveAppId: (queryOrRoute: RouteLocationNormalizedLoaded['query']) => unknown
  onQuery: () => Promise<void> | void
  field?: string
}) {
  const field = options.field ?? 'appId'

  const syncAppId = () => {
    options.queryParams.value[field] = options.resolveAppId(options.route.query)
  }

  syncAppId()

  watch(() => options.route.query.appId, async () => {
    syncAppId()
    await options.onQuery()
  })
}
