import { test, type Browser, type Page } from '@playwright/test';

// 会话态文件由 setup 项目（auth.setup / superadmin.setup）真实登录后写入。
export const ADMIN_STATE = 'e2e/.e2e/state/admin.json';
export const SUPERADMIN_STATE = 'e2e/.e2e/state/superadmin.json';

const APP_URL = process.env.E2E_APP_URL ?? 'http://127.0.0.1:20800';

// 显式证据截图：落在 outputDir 的用例目录（e2e/.e2e/results/<test>/<label>.png）
export async function shot(page: Page, label: string): Promise<void> {
  await page.screenshot({ path: test.info().outputPath(`${label}.png`) });
}

// 默认 page fixture 即 admin 态；需要 superadmin 或匿名（登录页路径）时开显式 context。
// newContext 不继承 config 的 use.baseURL/locale，需在此重复。
export async function superadminPage(browser: Browser): Promise<Page> {
  const context = await browser.newContext({
    storageState: SUPERADMIN_STATE,
    baseURL: APP_URL,
    locale: 'zh-CN',
  });
  return context.newPage();
}

export async function anonPage(browser: Browser): Promise<Page> {
  // 必须显式清空 storageState：browser.newContext() 会继承 config 项目级的 admin 态，
  // 否则登录页被路由守卫弹回，匿名路径用例全部失效
  const context = await browser.newContext({
    storageState: { cookies: [], origins: [] },
    baseURL: APP_URL,
    locale: 'zh-CN',
  });
  return context.newPage();
}

export const sleep = (ms: number): Promise<void> => {
  const { promise, resolve } = Promise.withResolvers<void>();
  setTimeout(resolve, ms);
  return promise;
};

// vite dev 下连续 hash 跳转偶发 net::ERR_ABORTED（前一个懒加载 chunk 的 load 事件被打断），
// 同一导航重试一次即成；巡查/闭环的全菜单循环里必须用它
export async function openHash(page: Page, path: string): Promise<void> {
  try {
    await page.goto(path);
  } catch (error) {
    if (!String(error).includes('ERR_ABORTED')) throw error;
    await page.goto(path);
  }
}
