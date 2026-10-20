import { test, expect } from '@playwright/test';
import { openHash, shot, sleep } from './helpers';

// .menu-test/run.js 的完整迁移（浏览器层）：
// 1) 菜单全量可达：叶子菜单逐个真实导航，无一渲染 404；
// 2) API 异常扫描：initScript 打桩 fetch/XHR 记录页面触发的 /api/ 响应，
//    断言无 HTTP>=400 且无业务 code!=0（信封成功码为 0；排除登录/菜单树自身）。
// 注意：evaluate/addInitScript 的函数体序列化后丢失模块闭包，工具函数必须函数内定义。
const API_BASE = process.env.E2E_API_BASE ?? 'http://127.0.0.1:20802';

type MenuLeaf = { path: string; title: string };
type ApiRecord = { url: string; status: number; code: number | null };

test('租户管理员菜单全量可达且无 API 异常', async ({ page }) => {
  // 页面级网络记录器：对后续所有加载生效（hash 导航不重载，同页持续记录）
  await page.addInitScript((): void => {
    type ApiEntry = { url: string; status: number; code: number | null };
    const readFieldLocal = (source: object, keys: string[]): unknown => {
      for (const key of keys) {
        if (key in source) return Reflect.get(source, key);
      }
      return undefined;
    };
    const scope = window as unknown as { __e2eApi?: ApiEntry[] }; // 本打桩注入的记录槽
    scope.__e2eApi = [];
    const push = (entry: ApiEntry): void => {
      scope.__e2eApi?.push(entry);
    };
    const businessCode = (body: unknown): number | null => {
      if (!body || typeof body !== 'object') return null;
      const code: unknown = readFieldLocal(body, ['code', 'Code']);
      return typeof code === 'number' ? code : null;
    };
    const originalFetch = window.fetch;
    window.fetch = function patchedFetch(input: RequestInfo | URL, init?: RequestInit) {
      const url = typeof input === 'string' ? input : input instanceof URL ? input.href : input.url;
      return originalFetch.call(this, input, init).then((response) => {
        if (url.includes('/api/')) {
          response
            .clone()
            .json()
            .then((body: unknown) => push({ url, status: response.status, code: businessCode(body) }))
            .catch(() => push({ url, status: response.status, code: null }));
        }
        return response;
      });
    };
    const openOriginal = XMLHttpRequest.prototype.open;
    const sendOriginal = XMLHttpRequest.prototype.send;
    XMLHttpRequest.prototype.open = function patchedOpen(
      this: XMLHttpRequest & { __url?: string },
      method: string,
      url: string | URL,
    ) {
      this.__url = String(url);
      const openArgs = arguments as unknown as Parameters<typeof openOriginal>; // 打桩签名与 lib 类型不一致
      return openOriginal.apply(this, openArgs);
    };
    XMLHttpRequest.prototype.send = function patchedSend(this: XMLHttpRequest & { __url?: string }) {
      this.addEventListener('load', () => {
        if (!(this.__url ?? '').includes('/api/')) return;
        let code: number | null = null;
        try {
          code = businessCode(JSON.parse(this.responseText));
        } catch {
          code = null;
        }
        push({ url: this.__url ?? '', status: this.status, code });
      });
      const sendArgs = arguments as unknown as Parameters<typeof sendOriginal>; // 打桩签名与 lib 类型不一致
      return sendOriginal.apply(this, sendArgs);
    };
  });

  await page.goto('/');
  const leaves = await page.evaluate(
    async (apiBase: string): Promise<MenuLeaf[]> => {
      const readFieldLocal = (source: object, keys: string[]): unknown => {
        for (const key of keys) {
          if (key in source) return Reflect.get(source, key);
        }
        return undefined;
      };
      const rawToken: unknown = JSON.parse(localStorage.getItem('gameframex.admin.vue:access-token') ?? '""');
      const token = typeof rawToken === 'string' ? rawToken : '';
      const res = await fetch(`${apiBase}/api/globalMenu/loginMenuTree`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const body: unknown = await res.json();
      const tree: unknown =
        body && typeof body === 'object' ? readFieldLocal(body, ['result', 'Result', 'data']) : [];
      const out: MenuLeaf[] = [];
      const walk = (nodes: unknown) => {
        if (!Array.isArray(nodes)) return;
        for (const node of nodes) {
          if (!node || typeof node !== 'object') continue;
          const children = readFieldLocal(node, ['children', 'Children']);
          const path = readFieldLocal(node, ['path', 'Path']);
          const title = readFieldLocal(node, ['title', 'Title']);
          const childList = Array.isArray(children) ? children : [];
          if (childList.length === 0 && typeof path === 'string' && path) {
            out.push({ path, title: typeof title === 'string' ? title : path });
          }
          walk(childList);
        }
      };
      walk(tree);
      return out;
    },
    API_BASE,
  );

  // 菜单树为空说明鉴权/接口异常，直接失败
  expect(leaves.length).toBeGreaterThan(5);

  const notFound: string[] = [];
  for (const menu of leaves) {
    await openHash(page, `/#${menu.path.startsWith('/') ? '' : '/'}${menu.path}`);
    const marker = page.locator('text=找不到此页面').first();
    let is404 = false;
    // SPA hash 路由组件在 load 后挂载，三个时间点采样判定 404
    for (const delay of [100, 250, 450]) {
      await sleep(delay);
      if (await marker.isVisible()) {
        is404 = true;
        break;
      }
    }
    if (is404) notFound.push(`${menu.title} ${menu.path}`);
  }

  expect(notFound, '渲染 404 的菜单').toEqual([]);

  const records = await page.evaluate((): ApiRecord[] => {
    const scope = window as unknown as { __e2eApi?: ApiRecord[] }; // initScript 打桩注入的记录槽
    return scope.__e2eApi ?? [];
  });
  expect(records.length).toBeGreaterThan(10);
  const anomalies = records.filter(
    (record) =>
      !record.url.includes('sysAuth') &&
      !record.url.includes('loginMenuTree') &&
      (record.status >= 400 || (record.code !== null && record.code !== 0)),
  );
  expect(anomalies, '页面触发的 API 异常').toEqual([]);
  await shot(page, 'menu-patrol-last-page');
});
