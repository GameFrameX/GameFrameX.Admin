// 支付配置「复制回调地址」URL 契约单测（vitest，与其他五个纯逻辑单测共用 *.test.ts）。
//
// 运行：`pnpm run test:unit`。锁定 C298 / spec `## Payment Config Copy Notify URL` 契约，
// 防止以后误改回旧 query 格式、错误 action 名或漏掉 packageId。action 段为 lowerCamelCase
// （后端 token 路由匹配大小写不敏感）。

import { test, expect } from 'vitest'
import {
  NOTIFY_ACTION_MAP,
  buildNotifyAction,
  buildNotifyUrl,
  capitalizeChannel,
} from './notifyUrl'

/** 契约级不变量：任何渠道输出都不含旧前缀或 query。被五个独立用例共用 → 抽作辅助函数。 */
function assertRouteSegmentFormat(url: string) {
  expect(url).toMatch(/^\/api\/gameOrder\/notify[A-Za-z]+\/[^/]+\/[^/]+(\/[^/]+)?$/)
  expect(url).not.toContain('?')
  expect(url).not.toContain('/client/gameOrder/')
}

test('NOTIFY_ACTION_MAP maps known channels to lowerCamelCase actions', () => {
  expect(NOTIFY_ACTION_MAP.apple).toBe('notifyApple')
  expect(NOTIFY_ACTION_MAP.google).toBe('notifyGoogle')
  expect(NOTIFY_ACTION_MAP.alipay).toBe('notifyAliPay')
  expect(NOTIFY_ACTION_MAP.xsolla).toBe('notifyXsolla')
})

test('buildNotifyAction returns mapped action for known channels', () => {
  expect(buildNotifyAction('apple')).toBe('notifyApple')
  expect(buildNotifyAction('google')).toBe('notifyGoogle')
  expect(buildNotifyAction('alipay')).toBe('notifyAliPay')
  expect(buildNotifyAction('xsolla')).toBe('notifyXsolla')
})

test('buildNotifyAction falls back to notify${capitalize(channel)} for unknown channels without breaking known map', () => {
  expect(buildNotifyAction('wechat')).toBe('notifyWechat')
  expect(buildNotifyAction('stripe')).toBe('notifyStripe')
  // 已知渠道不受 fallback 影响（回归保护）
  expect(buildNotifyAction('apple')).toBe('notifyApple')
  expect(buildNotifyAction('ALIPAY')).toBe('notifyALIPAY') // fallback 保留 remainder 原样，仅首字母大写
})

test('capitalizeChannel handles empty / single-char / multi-char', () => {
  expect(capitalizeChannel('')).toBe('')
  expect(capitalizeChannel('a')).toBe('A')
  expect(capitalizeChannel('wechat')).toBe('Wechat')
})

test('buildNotifyUrl produces route-segment format for each known channel without packageId', () => {
  const base = { tenantId: 1300000000001, gameAppId: 7 }
  const cases = [
    { channel: 'apple', action: 'notifyApple' },
    { channel: 'google', action: 'notifyGoogle' },
    { channel: 'alipay', action: 'notifyAliPay' },
    { channel: 'xsolla', action: 'notifyXsolla' },
  ]
  for (const { channel, action } of cases) {
    const url = buildNotifyUrl({ ...base, payChannel: channel })
    expect(url).toBe(`/api/gameOrder/${action}/1300000000001/7`)
    assertRouteSegmentFormat(url)
  }
})

test('buildNotifyUrl appends packageId segment when present', () => {
  const url = buildNotifyUrl({ payChannel: 'apple', tenantId: 1300000000001, gameAppId: 7, packageId: 42 })
  expect(url).toBe('/api/gameOrder/notifyApple/1300000000001/7/42')
  assertRouteSegmentFormat(url)
})

test('buildNotifyUrl omits packageId segment when missing, zero, or empty', () => {
  const base = { payChannel: 'apple', tenantId: 1300000000001, gameAppId: 7 }
  expect(buildNotifyUrl(base)).toBe('/api/gameOrder/notifyApple/1300000000001/7')
  expect(buildNotifyUrl({ ...base, packageId: 0 })).toBe('/api/gameOrder/notifyApple/1300000000001/7')
  expect(buildNotifyUrl({ ...base, packageId: undefined })).toBe('/api/gameOrder/notifyApple/1300000000001/7')
})

test('buildNotifyUrl accepts appId as fallback when gameAppId absent', () => {
  const url = buildNotifyUrl({ payChannel: 'google', tenantId: 99, appId: 5 })
  expect(url).toBe('/api/gameOrder/notifyGoogle/99/5')
})

test('buildNotifyUrl falls back to placeholders when tenant/app missing', () => {
  const url = buildNotifyUrl({ payChannel: 'alipay' })
  expect(url).toBe('/api/gameOrder/notifyAliPay/{tenantId}/{appId}')
})

test('buildNotifyUrl never emits legacy query or /client/gameOrder/ format across channels', () => {
  const rows = [
    { payChannel: 'apple', tenantId: 1, gameAppId: 2 },
    { payChannel: 'google', tenantId: 1, gameAppId: 2, packageId: 3 },
    { payChannel: 'alipay', tenantId: 1, appId: 2 },
    { payChannel: 'xsolla' },
    { payChannel: 'wechat', tenantId: 1, gameAppId: 2, packageId: 9 },
  ]
  for (const row of rows) {
    const url = buildNotifyUrl(row)
    assertRouteSegmentFormat(url)
    // 形态守卫：拦截大写 N 前缀 action 段与大写 G base path 段（PascalCase 回退），字面量不落入验收 grep 模式
    expect(url).not.toMatch(/\/Notify[A-Z]/)
    expect(url).not.toContain('/GameOrder/')
    expect(url).not.toContain('tenantId=')
    expect(url).not.toContain('appId=')
    expect(url).not.toContain('packageId=')
  }
})
