/**
 * OnlineCursorList 游标分页纯逻辑状态机（C349 / GFX-756，C340 前端半边）。
 *
 * 契约（vault C2 S1.2 定稿 + C340 §3）：Online 域查询分页统一 `Cursor + HasMore`，
 * 禁页码偏移。本模块只做状态搬运：
 *   - append / reset / cursor 透传 / hasMore 终止；
 *   - 不去重、不修补——重复或漏项是服务端 bug，必须原样暴露给 UI 与测试（透传原则）。
 * 无外部依赖：Vue 组件与 node --test 单测（cursorPager.test.mjs）共用本文件。
 */

/** 游标分页响应载荷（Online 查询端点统一形态）。 */
export interface CursorPage<T> {
    items: T[];
    nextCursor?: string | null;
    hasMore: boolean;
    snapshotTime?: string | number | null;
}

/** 游标列表状态（组件持有的唯一数据源）。 */
export interface CursorPagerState<T> {
    items: T[];
    nextCursor: string | null;
    hasMore: boolean;
    snapshotTime: string | number | null;
    loading: boolean;
    /** 至少成功加载过一页（区分「未加载」与「空结果」两种空态）。 */
    loaded: boolean;
}

/** 组装下一次请求参数：仅在已加载、还有更多且拿到游标时携带 cursor（原样透传，不重编码）。 */
export function buildRequestParams<T>(state: CursorPagerState<T>, pageSize?: number): { cursor?: string; pageSize?: number } {
    const params: { cursor?: string; pageSize?: number } = {};
    if (state.loaded && state.hasMore && state.nextCursor) {
        params.cursor = state.nextCursor;
    }
    if (pageSize) {
        params.pageSize = pageSize;
    }
    return params;
}

/** 重建初始状态（reload 的首页语义）。 */
export function createCursorPagerState<T>(): CursorPagerState<T> {
    return {
        items: [],
        nextCursor: null,
        hasMore: false,
        snapshotTime: null,
        loading: false,
        loaded: false,
    };
}

/** 一次加载开始（首页或加载更多共用）。 */
export function beginLoad<T>(state: CursorPagerState<T>): void {
    state.loading = true;
}

/** 应用首页响应：替换 items（reload 后的全新窗口），记录游标/hasMore/快照。 */
export function applyFirstPage<T>(state: CursorPagerState<T>, page: CursorPage<T>): void {
    state.items = page.items ?? [];
    state.nextCursor = page.nextCursor ?? null;
    state.hasMore = page.hasMore;
    state.snapshotTime = page.snapshotTime ?? null;
    state.loading = false;
    state.loaded = true;
}

/** 追加「加载更多」响应：concat 不去重（透传原则），游标/hasMore 滚动更新。 */
export function appendPage<T>(state: CursorPagerState<T>, page: CursorPage<T>): void {
    state.items = state.items.concat(page.items ?? []);
    state.nextCursor = page.nextCursor ?? null;
    state.hasMore = page.hasMore;
    if (page.snapshotTime != null) {
        state.snapshotTime = page.snapshotTime;
    }
    state.loading = false;
    state.loaded = true;
}

/** 加载失败：仅复位 loading，保留已加载数据与游标（用户可重试「加载更多」）。 */
export function failLoad<T>(state: CursorPagerState<T>): void {
    state.loading = false;
}
