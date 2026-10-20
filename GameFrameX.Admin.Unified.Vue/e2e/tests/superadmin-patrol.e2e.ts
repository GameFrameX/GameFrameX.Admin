import { test, expect } from '@playwright/test';
import { navigateByMenu, renderedMenuLeaves, shot, sleep, superadminPage } from './helpers';

// SuperAdmin 平台菜单巡查（menu-patrol 的平台侧镜像，2026-10-20 起全面真实点击化）：
// 平台管理/全局配置/开发工具/帮助文档 等平台页首次纳入回归——
// 叶子清单读自渲染后的平台侧边栏 DOM（不读菜单树接口），逐个真实点击导航（不 hash 直达）无 404，
// initScript 网络记录器被动观察页面自己触发的 API，断言无异常。
// 与租户巡查分文件保持（会话/排除规则可能分化，第三个人物出现时再抽公共）。

test('SuperAdmin 平台菜单全量可达且无 API 异常', async ({ browser }) => {
  const page = await superadminPage(browser);

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
  await page.locator('.p-panelmenu').waitFor({ timeout: 15_000 });

  const leaves = await renderedMenuLeaves(page);
  expect(leaves.length, '平台侧边栏叶子数').toBeGreaterThan(3);

  const notFound: string[] = [];
  for (const title of leaves) {
    await navigateByMenu(page, title);
    // SPA hash 路由组件在 click 后挂载，三个时间点采样判定 404
    const marker = page.locator('text=找不到此页面').first();
    let is404 = false;
    for (const delay of [100, 250, 450]) {
      await sleep(delay);
      if (await marker.isVisible()) {
        is404 = true;
        break;
      }
    }
    if (is404) notFound.push(title);
  }

  expect(notFound, '渲染 404 的菜单').toEqual([]);

  const records = await page.evaluate((): { url: string; status: number; code: number | null }[] => {
    // 打桩 initScript 写入的记录槽（见上方 addInitScript）
    const scope = window as unknown as { __e2eApi?: { url: string; status: number; code: number | null }[] };
    return scope.__e2eApi ?? [];
  });
  expect(records.length).toBeGreaterThan(5);
  const anomalies = records.filter(
    (record) =>
      !record.url.includes('sysAuth') &&
      !record.url.includes('loginMenuTree') &&
      (record.status >= 400 || (record.code !== null && record.code !== 0)),
  );
  expect(anomalies, '平台页触发的 API 异常').toEqual([]);
  await shot(page, 'superadmin-patrol-last-page');
  await page.close();
});
