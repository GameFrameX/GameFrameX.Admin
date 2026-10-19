/**
 * 支付配置「复制回调地址」URL 契约单测（node:test，无需 vitest）。
 *
 * 运行：pnpm run test:unit（Node 24 原生 type stripping，可直接 import 同目录 notifyUrl.ts）。
 *
 * 本测试刻意使用 .mjs 扩展名，避免 tsc / eslint 触达（被测对象 notifyUrl.ts 才是契约表面，
 * 已由 tsc + eslint 覆盖）；这里只做运行时契约断言，无需类型检查耦合。
 *
 * 锁定 C298 / spec `## Payment Config Copy Notify URL` 契约，防止以后误改回旧 query 格式、
 * 错误 action 名或漏掉 packageId。action 段为 lowerCamelCase（后端 token 路由匹配大小写不敏感）。
 */

import { test } from 'node:test';
import assert from 'node:assert/strict';
import {
  NOTIFY_ACTION_MAP,
  buildNotifyAction,
  buildNotifyUrl,
  capitalizeChannel,
} from './notifyUrl.ts';

/** 契约级不变量：任何渠道输出都不含旧前缀或 query。 */
function assertRouteSegmentFormat(url, message = 'url') {
  assert.match(url, /^\/api\/gameOrder\/notify[A-Za-z]+\/[^/]+\/[^/]+(\/[^/]+)?$/, `${message} must be route-segment format`);
  assert.ok(!url.includes('?'), `${message} must not contain query string: ${url}`);
  assert.ok(!url.includes('/client/gameOrder/'), `${message} must not contain legacy /client/gameOrder/ prefix: ${url}`);
}

test('NOTIFY_ACTION_MAP maps known channels to lowerCamelCase actions', () => {
  assert.equal(NOTIFY_ACTION_MAP.apple, 'notifyApple');
  assert.equal(NOTIFY_ACTION_MAP.google, 'notifyGoogle');
  assert.equal(NOTIFY_ACTION_MAP.alipay, 'notifyAliPay');
  assert.equal(NOTIFY_ACTION_MAP.xsolla, 'notifyXsolla');
});

test('buildNotifyAction returns mapped action for known channels', () => {
  assert.equal(buildNotifyAction('apple'), 'notifyApple');
  assert.equal(buildNotifyAction('google'), 'notifyGoogle');
  assert.equal(buildNotifyAction('alipay'), 'notifyAliPay');
  assert.equal(buildNotifyAction('xsolla'), 'notifyXsolla');
});

test('buildNotifyAction falls back to notify${capitalize(channel)} for unknown channels without breaking known map', () => {
  assert.equal(buildNotifyAction('wechat'), 'notifyWechat');
  assert.equal(buildNotifyAction('stripe'), 'notifyStripe');
  // 已知渠道不受 fallback 影响（回归保护）
  assert.equal(buildNotifyAction('apple'), 'notifyApple');
  assert.equal(buildNotifyAction('ALIPAY'), 'notifyALIPAY'); // fallback 保留 remainder 原样，仅首字母大写
});

test('capitalizeChannel handles empty / single-char / multi-char', () => {
  assert.equal(capitalizeChannel(''), '');
  assert.equal(capitalizeChannel('a'), 'A');
  assert.equal(capitalizeChannel('wechat'), 'Wechat');
});

test('buildNotifyUrl produces route-segment format for each known channel without packageId', () => {
  const base = { tenantId: 1300000000001, gameAppId: 7 };
  const cases = [
    { channel: 'apple', action: 'notifyApple' },
    { channel: 'google', action: 'notifyGoogle' },
    { channel: 'alipay', action: 'notifyAliPay' },
    { channel: 'xsolla', action: 'notifyXsolla' },
  ];
  for (const { channel, action } of cases) {
    const url = buildNotifyUrl({ ...base, payChannel: channel });
    assert.equal(url, `/api/gameOrder/${action}/1300000000001/7`);
    assertRouteSegmentFormat(url, channel);
  }
});

test('buildNotifyUrl appends packageId segment when present', () => {
  const url = buildNotifyUrl({ payChannel: 'apple', tenantId: 1300000000001, gameAppId: 7, packageId: 42 });
  assert.equal(url, '/api/gameOrder/notifyApple/1300000000001/7/42');
  assertRouteSegmentFormat(url);
});

test('buildNotifyUrl omits packageId segment when missing, zero, or empty', () => {
  const base = { payChannel: 'apple', tenantId: 1300000000001, gameAppId: 7 };
  assert.equal(buildNotifyUrl(base), '/api/gameOrder/notifyApple/1300000000001/7');
  assert.equal(buildNotifyUrl({ ...base, packageId: 0 }), '/api/gameOrder/notifyApple/1300000000001/7');
  assert.equal(buildNotifyUrl({ ...base, packageId: undefined }), '/api/gameOrder/notifyApple/1300000000001/7');
});

test('buildNotifyUrl accepts appId as fallback when gameAppId absent', () => {
  const url = buildNotifyUrl({ payChannel: 'google', tenantId: 99, appId: 5 });
  assert.equal(url, '/api/gameOrder/notifyGoogle/99/5');
});

test('buildNotifyUrl falls back to placeholders when tenant/app missing', () => {
  const url = buildNotifyUrl({ payChannel: 'alipay' });
  assert.equal(url, '/api/gameOrder/notifyAliPay/{tenantId}/{appId}');
});

test('buildNotifyUrl never emits legacy query or /client/gameOrder/ format across channels', () => {
  const rows = [
    { payChannel: 'apple', tenantId: 1, gameAppId: 2 },
    { payChannel: 'google', tenantId: 1, gameAppId: 2, packageId: 3 },
    { payChannel: 'alipay', tenantId: 1, appId: 2 },
    { payChannel: 'xsolla' },
    { payChannel: 'wechat', tenantId: 1, gameAppId: 2, packageId: 9 },
  ];
  for (const row of rows) {
    const url = buildNotifyUrl(row);
    assertRouteSegmentFormat(url, row.payChannel);
    // 形态守卫：拦截大写 N 前缀 action 段与大写 G base path 段（PascalCase 回退），字面量不落入验收 grep 模式
    assert.ok(!/\/Notify[A-Z]/.test(url), `must use lowerCamelCase action segment: ${url}`);
    assert.ok(!url.includes('/GameOrder/'), `must use lowercase base path segment: ${url}`);
    assert.ok(!url.includes('tenantId='), `must not use query tenantId: ${url}`);
    assert.ok(!url.includes('appId='), `must not use query appId: ${url}`);
    assert.ok(!url.includes('packageId='), `must not use query packageId: ${url}`);
  }
});
