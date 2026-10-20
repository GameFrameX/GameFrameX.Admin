import { test, expect } from '@playwright/test';
import { enterWorkspace, gotoAppSelector, navigateByMenu, renderedMenuLeaves, shot, sleep } from './helpers';

// .local-archive/menu-test-20261020/run.js 的完整迁移（浏览器层，2026-10-20 起全面真实点击化）：
// 1) 菜单全量可达：叶子清单读自渲染后的侧边栏 DOM（不读菜单树接口），
//    逐个真实点击导航（不 hash 直达），无一渲染 404；
//    租户侧覆盖两个真实表面——应用选择页的全局侧边栏 + 应用工作台的应用域侧边栏；
// 已修复（2026-10-20，GitHub #3/#4）：#3 ApplyAppIdFilter 接口成员绑定致 PG 裸列名 appid 500——
//   改调用方选择器重载（u => u.AppId）组合等值谓词；#4 在线中心页面按 Online 作用域守卫（ServerId 未定不发请求）、
//   灰度页 immediate 首查改挂载后守卫发起。
// 2) API 异常扫描：initScript 打桩 fetch/XHR 被动记录页面自己触发的 /api/ 响应，
//    断言无 HTTP>=400 且无业务 code!=0（信封成功码为 0；排除登录/菜单树自身）。
// 注意：evaluate/addInitScript 的函数体序列化后丢失模块闭包，工具函数必须函数内定义。

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
  await page.locator('.p-panelmenu').waitFor({ timeout: 15_000 });
  // 前序用例可能已把会话留在应用工作台；回到应用选择页拿全局侧边栏
  await gotoAppSelector(page);

  const notFound: string[] = [];
  const sampleNotFound = async (surface: string, title: string): Promise<void> => {
    // SPA hash 路由组件在 click 后挂载，三个时间点采样判定 404
    const marker = page.locator('text=找不到此页面').first();
    for (const delay of [100, 250, 450]) {
      await sleep(delay);
      if (await marker.isVisible()) {
        notFound.push(`[${surface}] ${title}`);
        return;
      }
    }
  };

  // 表面一：应用选择页的全局侧边栏（登录后的默认落地表面）
  const globalLeaves = await renderedMenuLeaves(page);
  // 菜单为空说明鉴权/渲染异常，直接失败
  expect(globalLeaves.length, '全局侧边栏叶子数').toBeGreaterThan(5);

  for (const title of globalLeaves) {
    await navigateByMenu(page, title);
    await sampleNotFound('全局', title);
  }

  // 表面二：真实点击默认应用卡进入工作台，拿应用域侧边栏
  await enterWorkspace(page);
  const appLeaves = await renderedMenuLeaves(page);
  expect(appLeaves.length, '工作台侧边栏叶子数').toBeGreaterThan(20);

  for (const title of appLeaves) {
    await navigateByMenu(page, title);
    await sampleNotFound('应用', title);
  }

  expect(notFound, '渲染 404 的菜单').toEqual([]);

  const records = await page.evaluate((): { url: string; status: number; code: number | null }[] => {
    // 打桩 initScript 写入的记录槽（见上方 addInitScript）
    const scope = window as unknown as { __e2eApi?: { url: string; status: number; code: number | null }[] };
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
