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

// —— 真实菜单导航（2026-10-20 起 e2e 全面真实点击化）——
// 全部基于渲染后的侧边栏 DOM 与真实点击：不读菜单树接口、不 hash 直达。
// 侧边栏是 PrimeVue PanelMenu（src/layout/navigationMenu/VerticalMenu.vue）：
//   根节点 → .p-panelmenu-panel（.p-panelmenu-header 内 span.menu-nav-item[role=button] 开合，
//            折叠时 .p-panelmenu-content-container display:none，子树仍在 DOM）
//   子分组 → li.p-panelmenu-item[aria-expanded]（span.menu-nav-item[role=button] 开合）
//   叶子   → a.menu-nav-item（.menu-label 为标题；router-link href 自带 appId query）

// 读取渲染后侧边栏的全部叶子标题（折叠容器内的叶子也在 DOM 中，去重保序）
export async function renderedMenuLeaves(page: Page): Promise<string[]> {
  return page.evaluate(() => {
    const titles = Array.from(document.querySelectorAll('.p-panelmenu a.menu-nav-item .menu-label'))
      .map((el) => (el.textContent ?? '').trim())
      .filter(Boolean);
    const seen = new Set<string>();
    return titles.filter((title) => {
      if (seen.has(title)) return false;
      seen.add(title);
      return true;
    });
  });
}

// 当前侧边栏是否包含某叶子（含折叠容器内）
async function sidebarHasLeaf(page: Page, title: string): Promise<boolean> {
  return page.evaluate((leafTitle) => {
    return Array.from(document.querySelectorAll('.p-panelmenu a.menu-nav-item .menu-label')).some(
      (el) => (el.textContent ?? '').trim() === leafTitle,
    );
  }, title);
}

// 侧边栏叶子链接的文本定位器（menu-label 精确匹配；Vue 重渲染后自动重新解析，
// 比打 data 标记再点击稳——标记元素会被 PanelMenu 重渲染替换导致 detached）
function menuLeafLink(page: Page, title: string) {
  return page
    .locator('.p-panelmenu a.menu-nav-item')
    .filter({ has: page.getByText(title, { exact: true }) })
    .first();
}

type MenuAction =
  | { kind: 'ready' }
  | { kind: 'expand'; label: string; where: 'header' | 'item' }
  | { kind: 'missing' }
  | { kind: 'stuck' };

// evaluate 侧：定位目标叶子，返回下一步动作——可见则 ready；
// 否则给出最外层折叠祖先的开关（分组名 + 层级），由真实 click 展开后重试。
// 注意：evaluate 函数体序列化后丢失模块闭包，工具逻辑必须函数内定义。
async function nextMenuAction(page: Page, title: string): Promise<MenuAction> {
  return page.evaluate((leafTitle) => {
    const links = Array.from(document.querySelectorAll('.p-panelmenu a.menu-nav-item'));
    const target = links.find((a) => (a.querySelector('.menu-label')?.textContent ?? '').trim() === leafTitle);
    if (!target) return { kind: 'missing' };
    if (target.getBoundingClientRect().height > 0) return { kind: 'ready' };
    // 自内向外收集折叠祖先（根面板容器 + li 子菜单），先展开最外层
    const collapsed: { label: string; where: 'header' | 'item' }[] = [];
    let node: HTMLElement | null = target.parentElement;
    while (node) {
      if (
        node.classList.contains('p-panelmenu-content-container') &&
        getComputedStyle(node).display === 'none'
      ) {
        const toggle = node
          .closest('.p-panelmenu-panel')
          ?.querySelector(':scope > .p-panelmenu-header span.menu-nav-item[role="button"]');
        const label = toggle?.getAttribute('aria-label') ?? toggle?.textContent?.trim();
        if (toggle && label) collapsed.push({ label, where: 'header' });
      }
      if (node.tagName === 'LI' && node.getAttribute('aria-expanded') === 'false') {
        const toggle = node.querySelector(
          ':scope > .p-panelmenu-item-content span.menu-nav-item[role="button"]',
        );
        const label = toggle?.getAttribute('aria-label') ?? toggle?.textContent?.trim();
        if (toggle && label) collapsed.push({ label, where: 'item' });
      }
      node = node.parentElement;
    }
    if (collapsed.length === 0) return { kind: 'stuck' };
    return { kind: 'expand', ...collapsed[collapsed.length - 1] };
  }, title);
}

// 真实点击导航到侧边栏叶子：逐层真实 click 展开折叠祖先，再真实 click 叶子链接。
// 「找不到叶子」不立即判死：dev server 整页 reload（vite 依赖再优化、HMR 等）期间
// PanelMenu 会短暂整体卸载，先有限等待侧边栏重挂载（真缺失超时后仍会失败）。
export async function navigateByMenu(page: Page, leafTitle: string): Promise<void> {
  const missingDeadline = Date.now() + 5_000;
  for (let round = 0; round < 10; round += 1) {
    const action = await nextMenuAction(page, leafTitle);
    if (action.kind === 'missing') {
      if (Date.now() >= missingDeadline) throw new Error(`当前侧边栏未找到菜单「${leafTitle}」`);
      await sleep(300);
      round -= 1; // 等待侧边栏重挂载不消耗展开轮次
      continue;
    }
    if (action.kind === 'stuck') throw new Error(`菜单「${leafTitle}」展开后仍不可见`);
    if (action.kind === 'ready') break;
    // 分组开关同样用文本定位（label 精确匹配），重渲染后可自动重试
    const scope =
      action.where === 'header'
        ? '.p-panelmenu-panel > .p-panelmenu-header span.menu-nav-item[role="button"]'
        : '.p-panelmenu li[aria-expanded] > .p-panelmenu-item-content span.menu-nav-item[role="button"]';
    await page
      .locator(scope)
      .filter({ has: page.getByText(action.label, { exact: true }) })
      .first()
      .click();
    await sleep(250);
  }
  await menuLeafLink(page, leafTitle).click();
  await sleep(250);
}

// 回到应用选择页（真实用户路径组合）：
// - 全局侧边栏页面：点「工作台」叶子——未选应用被守卫导流回 appSelector；已选应用落应用驾驶舱
// - 应用工作台深层页：侧边栏无出口，重载落地应用驾驶舱（守卫真实路径）
// - 应用驾驶舱：点「返回应用列表」正门出口（C333 后唯一出口）
export async function gotoAppSelector(page: Page): Promise<void> {
  if (!page.url().includes('appCenter/appSelector')) {
    if (await sidebarHasLeaf(page, '工作台')) {
      await navigateByMenu(page, '工作台');
      await sleep(600);
    } else {
      await page.goto('/');
      await sleep(900);
    }
  }
  if (!page.url().includes('appCenter/appSelector')) {
    await page.locator('.back-to-app-list').click();
    await sleep(600);
  }
  await page.locator('role=button[name*="默认应用"] >> nth=0').waitFor({ state: 'visible', timeout: 15_000 });
}

// 进入应用工作台（真实点击默认应用卡；菜单链接的 href 自带 appId query）
export async function enterWorkspace(page: Page): Promise<void> {
  await gotoAppSelector(page);
  await page.locator('role=button[name*="默认应用"] >> nth=0').click();
  await page.waitForURL(/\/#\/(?!appCenter\/appSelector)/, { timeout: 20_000 });
  await sleep(900);
}
