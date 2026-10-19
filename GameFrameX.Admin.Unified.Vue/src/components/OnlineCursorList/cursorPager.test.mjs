import { test } from 'node:test'
import assert from 'node:assert/strict'
import {
  createCursorPagerState,
  beginLoad,
  applyFirstPage,
  appendPage,
  failLoad,
  buildRequestParams,
} from './cursorPager.ts'

// C349 / GFX-756 —— OnlineCursorList 游标分页纯逻辑状态机单测。
// 契约（vault C2 S1.2 + C340 §3）：分页统一 Cursor + HasMore，禁页码偏移；
// 状态机不去重、不修补——重复/漏项是服务端 bug，必须原样暴露（透传原则）。

test('初始状态：未加载、无游标、无更多；请求参数不携带 cursor', () => {
  const state = createCursorPagerState()
  assert.equal(state.loaded, false)
  assert.equal(state.items.length, 0)
  assert.equal(state.hasMore, false)
  assert.equal(state.nextCursor, null)
  assert.deepEqual(buildRequestParams(state), {})
  assert.deepEqual(buildRequestParams(state, 20), { pageSize: 20 })
})

test('首页应用：items 替换、游标/hasMore/快照时间记录、loaded 置位', () => {
  const state = createCursorPagerState()
  beginLoad(state)
  assert.equal(state.loading, true)
  applyFirstPage(state, {
    items: [{ id: 1 }, { id: 2 }],
    nextCursor: 'cursor-1',
    hasMore: true,
    snapshotTime: 1770000000,
  })
  assert.equal(state.loading, false)
  assert.equal(state.loaded, true)
  assert.deepEqual(state.items, [{ id: 1 }, { id: 2 }])
  assert.equal(state.nextCursor, 'cursor-1')
  assert.equal(state.hasMore, true)
  assert.equal(state.snapshotTime, 1770000000)
})

test('游标透传：下一次请求 cursor 与上一响应 nextCursor 全等（不重编码）', () => {
  const state = createCursorPagerState()
  applyFirstPage(state, { items: [], nextCursor: 'b64%2FAbC+==', hasMore: true })
  const params = buildRequestParams(state)
  assert.equal(params.cursor, 'b64%2FAbC+==')
  // 追加页的 nextCursor 继续成为第三次请求的 cursor（链式透传）
  appendPage(state, { items: [{ id: 3 }], nextCursor: 'cursor-2', hasMore: true })
  assert.equal(buildRequestParams(state).cursor, 'cursor-2')
})

test('hasMore 终止：无更多时请求参数不再携带 cursor', () => {
  const state = createCursorPagerState()
  applyFirstPage(state, { items: [{ id: 1 }], nextCursor: 'cursor-1', hasMore: false })
  assert.deepEqual(buildRequestParams(state), {})
})

test('nextCursor 缺省/空串视为无游标：hasMore=true 也不携带 cursor', () => {
  const state = createCursorPagerState()
  applyFirstPage(state, { items: [{ id: 1 }], hasMore: true })
  assert.deepEqual(buildRequestParams(state), {})
  appendPage(state, { items: [{ id: 2 }], nextCursor: '', hasMore: true })
  assert.deepEqual(buildRequestParams(state), {})
})

test('追加不去重：两页出现同 id 行时都保留（重复即暴露服务端 bug）', () => {
  const state = createCursorPagerState()
  applyFirstPage(state, { items: [{ id: 1 }, { id: 2 }], nextCursor: 'c1', hasMore: true })
  appendPage(state, { items: [{ id: 2 }, { id: 3 }], nextCursor: null, hasMore: false })
  assert.deepEqual(
    state.items.map((i) => i.id),
    [1, 2, 2, 3],
  )
  assert.equal(state.hasMore, false)
})

test('reload 语义：重建状态后回到首页加载（无 cursor、清空 items）', () => {
  const state = createCursorPagerState()
  applyFirstPage(state, { items: [{ id: 1 }], nextCursor: 'c1', hasMore: true })
  appendPage(state, { items: [{ id: 2 }], nextCursor: 'c2', hasMore: true })
  const reset = createCursorPagerState()
  assert.deepEqual(buildRequestParams(reset), {})
  applyFirstPage(reset, { items: [{ id: 9 }], nextCursor: null, hasMore: false })
  assert.deepEqual(reset.items, [{ id: 9 }])
})

test('失败不清空已加载数据：failLoad 只复位 loading', () => {
  const state = createCursorPagerState()
  applyFirstPage(state, { items: [{ id: 1 }], nextCursor: 'c1', hasMore: true })
  beginLoad(state)
  failLoad(state)
  assert.equal(state.loading, false)
  assert.deepEqual(state.items, [{ id: 1 }])
  assert.equal(state.nextCursor, 'c1')
  assert.equal(state.hasMore, true)
})

test('快照时间：追加页缺省不覆盖，提供则更新为最新快照', () => {
  const state = createCursorPagerState()
  applyFirstPage(state, { items: [], hasMore: true, snapshotTime: 1770000000 })
  appendPage(state, { items: [], hasMore: true })
  assert.equal(state.snapshotTime, 1770000000)
  appendPage(state, { items: [], hasMore: false, snapshotTime: 1770009999 })
  assert.equal(state.snapshotTime, 1770009999)
})
