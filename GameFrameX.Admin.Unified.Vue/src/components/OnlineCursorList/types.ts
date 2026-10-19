/**
 * OnlineCursorList 组件类型定义（C349 / GFX-756，C340 前端半边）。
 * 分页契约：Cursor + HasMore 透传（vault C2 S1.2），禁页码偏移。
 */
import type { CursorPage } from './cursorPager';

/** 游标列表 request 工厂入参（cursor 由组件透传，调用方只拼业务过滤条件）。 */
export interface OnlineCursorListParams {
    cursor?: string;
    pageSize?: number;
}

/** request 工厂：入参含组件管理的游标，出参为 Online 查询端点的统一载荷形态。 */
export type OnlineCursorRequest = (params: OnlineCursorListParams) => Promise<CursorPage<Record<string, any>>>;

/** 列定义（header 由调用方传入已本地化文案；field 与 Online 响应载荷 camelCase 字段一致）。 */
export interface OnlineColumnConfig {
    field: string;
    header: string;
    width?: string;
    align?: 'left' | 'center' | 'right';
    /** 敏感列：默认遮蔽（******），点击显明——vault C2 S1.6 脱敏口径的展示层落法。 */
    sensitive?: boolean;
}
