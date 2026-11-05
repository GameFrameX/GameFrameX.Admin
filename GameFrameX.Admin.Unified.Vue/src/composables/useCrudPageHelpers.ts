import {type Ref} from 'vue'
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
