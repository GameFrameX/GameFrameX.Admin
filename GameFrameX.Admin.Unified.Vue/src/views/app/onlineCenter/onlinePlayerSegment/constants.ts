/**
 * 玩家分群条件白名单（C343 / GFX-759 前端半边）。
 * 白名单唯一来源 = 后端 OnlineRolloutSemanticMap（Admin 只校验不解释）：
 * field ∈ {activeDays, retentionDays, totalPaymentAmount}（锚定 vault S7.5 事件族）
 * op ∈ {gte, lte, eq}（闭区间与等值三操作符起步）。
 * 联调后白名单扩充须先改后端 SemanticMap，再同步本表——禁散落第二套。
 */

// 条件字段选项（值 = 后端白名单字符串，逐字一致）
export const ONLINE_SEGMENT_FIELD_OPTIONS = [
  { value: 'activeDays', labelKey: 'message.onlineCenter.playerSegment.fieldActiveDays' },
  { value: 'retentionDays', labelKey: 'message.onlineCenter.playerSegment.fieldRetentionDays' },
  { value: 'totalPaymentAmount', labelKey: 'message.onlineCenter.playerSegment.fieldTotalPaymentAmount' },
];

// 条件操作符选项
export const ONLINE_SEGMENT_OP_OPTIONS = [
  { value: 'gte', labelKey: 'message.onlineCenter.playerSegment.opGte' },
  { value: 'lte', labelKey: 'message.onlineCenter.playerSegment.opLte' },
  { value: 'eq', labelKey: 'message.onlineCenter.playerSegment.opEq' },
];
