import { test, expect } from 'vitest'
import {
  createCursorPagerState,
  beginLoad,
  applyFirstPage,
  appendPage,
  failLoad,
  buildRequestParams,
} from './cursorPager'

// C349 / GFX-756 —— OnlineCursorList 游标分页纯逻辑状态机单测。
// 契约（vault C2 S1.2 + C340 §3）：分页统一 Cursor + HasMore，禁页码偏移；
// 状态机不去重、不修补——重复/漏项是服务端 bug，必须原样暴露（透传原则）。

test('初始状态：未加载、无游标、无更多；请求参数不携带 cursor', () => {
  const state = createCursorPagerState()
  expect(state.loaded).toBe(false)
  expect(state.items.length).toBe(0)
  expect(state.hasMore).toBe(false)
  expect(state.nextCursor).toBe(null)
  expect(buildRequestParams(state)).toEqual({})
  expect(buildRequestParams(state, 20)).toEqual({ pageSize: 20 })
})

test('首页应用：items 替换、游标/hasMore/快照时间记录、loaded 置位', () => {
  const state = createCursorPagerState()
  beginLoad(state)
  expect(state.loading).toBe(true)
  applyFirstPage(state, {
    items: [{ id: 1 }, { id: 2 }],
    nextCursor: 'cursor-1',
    hasMore: true,
    snapshotTime: 1770000000,
  })
  expect(state.loading).toBe(false)
  expect(state.loaded).toBe(true)
  expect(state.items).toEqual([{ id: 1 }, { id: 2 }])
  expect(state.nextCursor).toBe('cursor-1')
  expect(state.hasMore).toBe(true)
  expect(state.snapshotTime).toBe(1770000000)
})

test('游标透传：下一次请求 cursor 与上一响应 nextCursor 全等（不重编码）', () => {
  const state = createCursorPagerState()
  applyFirstPage(state, { items: [], nextCursor: 'b64%2FAbC+==', hasMore: true })
  const params = buildRequestParams(state)
  expect(params.cursor).toBe('b64%2FAbC+==')
  // 追加页的 nextCursor 继续成为第三次请求的 cursor（链式透传）
  appendPage(state, { items: [{ id: 3 }], nextCursor: 'cursor-2', hasMore: true })
  expect(buildRequestParams(state).cursor).toBe('cursor-2')
})

test('hasMore 终止：无更多时请求参数不再携带 cursor', () => {
  const state = createCursorPagerState()
  applyFirstPage(state, { items: [{ id: 1 }], nextCursor: 'cursor-1', hasMore: false })
  expect(buildRequestParams(state)).toEqual({})
})

test('nextCursor 缺省/空串视为无游标：hasMore=true 也不携带 cursor', () => {
  const state = createCursorPagerState()
  applyFirstPage(state, { items: [{ id: 1 }], hasMore: true })
  expect(buildRequestParams(state)).toEqual({})
  appendPage(state, { items: [{ id: 2 }], nextCursor: '', hasMore: true })
  expect(buildRequestParams(state)).toEqual({})
})

test('追加不去重：两页出现同 id 行时都保留（重复即暴露服务端 bug）', () => {
  const state = createCursorPagerState<{ id: number }>()
  applyFirstPage(state, { items: [{ id: 1 }, { id: 2 }], nextCursor: 'c1', hasMore: true })
  appendPage(state, { items: [{ id: 2 }, { id: 3 }], nextCursor: null, hasMore: false })
  expect(state.items.map((i) => i.id)).toEqual([1, 2, 2, 3])
  expect(state.hasMore).toBe(false)
})

test('reload 语义：重建状态后回到首页加载（无 cursor、清空 items）', () => {
  const state = createCursorPagerState()
  applyFirstPage(state, { items: [{ id: 1 }], nextCursor: 'c1', hasMore: true })
  appendPage(state, { items: [{ id: 2 }], nextCursor: 'c2', hasMore: true })
  const reset = createCursorPagerState()
  expect(buildRequestParams(reset)).toEqual({})
  applyFirstPage(reset, { items: [{ id: 9 }], nextCursor: null, hasMore: false })
  expect(reset.items).toEqual([{ id: 9 }])
})

test('失败不清空已加载数据：failLoad 只复位 loading', () => {
  const state = createCursorPagerState()
  applyFirstPage(state, { items: [{ id: 1 }], nextCursor: 'c1', hasMore: true })
  beginLoad(state)
  failLoad(state)
  expect(state.loading).toBe(false)
  expect(state.items).toEqual([{ id: 1 }])
  expect(state.nextCursor).toBe('c1')
  expect(state.hasMore).toBe(true)
})

test('快照时间：追加页缺省不覆盖，提供则更新为最新快照', () => {
  const state = createCursorPagerState()
  applyFirstPage(state, { items: [], hasMore: true, snapshotTime: 1770000000 })
  appendPage(state, { items: [], hasMore: true })
  expect(state.snapshotTime).toBe(1770000000)
  appendPage(state, { items: [], hasMore: false, snapshotTime: 1770009999 })
  expect(state.snapshotTime).toBe(1770009999)
})
