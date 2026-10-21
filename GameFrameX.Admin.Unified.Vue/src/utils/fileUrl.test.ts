// 后端文件 URL 解析纯函数单测（vitest，与 notifyUrl / cursorPager 等纯逻辑单测共用 *.test.ts 约定）。
//
// 运行：`pnpm run test:unit`。锁定 resolveBackendFileUrl 契约：
// 相对对象键（DB 存储形态）必须补全 API origin，绝对/data 地址必须原样透传，
// 防止回归到 `'/' + path` 直拼（登录后头像裂图的根因）。

import { test, expect, beforeEach, afterEach } from 'vitest';
import { resolveBackendFileUrl } from './fileUrl';

const originalEnv = window.__env__;

beforeEach(() => {
  window.__env__ = { VITE_API_URL: 'http://127.0.0.1:20802' };
});

afterEach(() => {
  window.__env__ = originalEnv;
});

test('resolves tenant avatar object key (no leading slash) against the API origin', () => {
  expect(resolveBackendFileUrl('Upload/Avatar/1300000000111.png')).toBe(
    'http://127.0.0.1:20802/Upload/Avatar/1300000000111.png'
  );
});

test('resolves backend-relative path with leading slash without duplicating separators', () => {
  expect(resolveBackendFileUrl('/Upload/Avatar/1.jpg')).toBe('http://127.0.0.1:20802/Upload/Avatar/1.jpg');
});

test('passes through absolute http(s) URLs (cloud object storage public URLs) untouched', () => {
  const url = 'https://bucket.oss-cn-hangzhou.aliyuncs.com/Upload/Avatar/1.png';
  expect(resolveBackendFileUrl(url)).toBe(url);
});

test('passes through data: inline URLs untouched', () => {
  const dataUrl = 'data:image/png;base64,iVBORw0KGgo=';
  expect(resolveBackendFileUrl(dataUrl)).toBe(dataUrl);
});

test('empty input returns empty string so callers can apply their own placeholder', () => {
  expect(resolveBackendFileUrl(undefined)).toBe('');
  expect(resolveBackendFileUrl(null)).toBe('');
  expect(resolveBackendFileUrl('')).toBe('');
  expect(resolveBackendFileUrl('   ')).toBe('');
});

test('trims base URL trailing slash and path whitespace', () => {
  window.__env__ = { VITE_API_URL: 'http://127.0.0.1:20802/' };
  expect(resolveBackendFileUrl('  Upload/Avatar/1.png  ')).toBe('http://127.0.0.1:20802/Upload/Avatar/1.png');
});

test('same-origin deployment (empty VITE_API_URL) degrades to root-relative path', () => {
  window.__env__ = { VITE_API_URL: '' };
  expect(resolveBackendFileUrl('Upload/Avatar/1.png')).toBe('/Upload/Avatar/1.png');
});
