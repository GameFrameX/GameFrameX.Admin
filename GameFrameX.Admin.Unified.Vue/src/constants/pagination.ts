export const DEFAULT_PAGE_SIZE_OPTIONS = [10, 20, 50, 100]

export const DEFAULT_PAGE_SIZE = DEFAULT_PAGE_SIZE_OPTIONS[0]

export const MAX_PAGE_SIZE = DEFAULT_PAGE_SIZE_OPTIONS[DEFAULT_PAGE_SIZE_OPTIONS.length - 1]

// 选择器下拉最大数量，Select API 默认上限 1000
export const MAX_SELECT_SIZE = 1000
export const DEFAULT_SELECT_SIZE = 500

export const clampPageSize = (pageSize?: number) => {
  if (!pageSize || pageSize < DEFAULT_PAGE_SIZE) return DEFAULT_PAGE_SIZE
  return Math.min(pageSize, MAX_PAGE_SIZE)
}
