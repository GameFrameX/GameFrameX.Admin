export interface RewardItem {
  id?: number | string
  itemId?: number | string
  count?: number | string
  star?: number | string
  _catalogueId?: number | string
  [key: string]: unknown
}

export const parseRewardItems = (value: unknown): RewardItem[] => {
  if (Array.isArray(value)) return normalizeRewardItems(value)
  if (typeof value !== 'string' || !value) return []
  try {
    const parsed = JSON.parse(value)
    return Array.isArray(parsed) ? normalizeRewardItems(parsed) : []
  } catch {
    return []
  }
}

export const normalizeRewardItems = (items: unknown): RewardItem[] => {
  if (!Array.isArray(items)) return []
  // 双读是转换边界：字符串载荷内部是 PascalCase（后端存储 + 游戏客户端契约），内部模型统一 camelCase
  // C270：id/itemId 是雪花 Id，后端序列化为字符串，保持原值禁止 Number() 回转（超 2^53 丢精度）
  return items.map((item) => ({
    id: item.id ?? item.Id ?? 0,
    count: Number(item.count ?? item.Count ?? 0),
    itemId: item.itemId ?? item.ItemId ?? 0,
    star: Number(item.star ?? item.Star ?? 0),
  }))
}

export const toRewardItemsJson = (value: unknown): string => {
  if (typeof value === 'string') return value
  const items = Array.isArray(value) ? value : []
  // 跨端契约：序列化输出的 JSON key 必须保持 PascalCase（后端存储 + 游戏客户端按 PascalCase 反序列化）
  // C270：Id/ItemId 原值透传禁止 Number() 回转（超 2^53 丢精度）
  return JSON.stringify(items.map((v: RewardItem) => ({
    Id: v.id ?? 0,
    Count: Number(v.count ?? 0),
    ItemId: v.itemId ?? 0,
    Star: Number(v.star ?? 0),
  })))
}
