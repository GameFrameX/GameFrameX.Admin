import { expect, test, type Browser, type Page } from '@playwright/test';

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
  return new Promise(resolve => setTimeout(resolve, ms));
};

// —— DatePicker(showTime) 真实面板演练（2026-10-21 补齐：此前仅 fill+Escape，面板交互零覆盖）——
// 面板陷阱：首行含上月灰格（p-disabled + aria-disabled，且 td 拦截指针事件），点首个格子必卡死；
// showTime 语义下选日期不收面板（hideOnDateTimeSelect=false）且无确认按钮，必须点外部收起；
// 不用 Escape 收起：会冒泡触发 Dialog dismissOnEscape 关掉整个表单对话框。
export async function pickDateTime(page: Page, comboboxIndex: number, dayOrdinal: 'first' | 'last' = 'first'): Promise<void> {
  await page.locator(`.p-dialog >> role=combobox >> nth=${comboboxIndex}`).click();
  const enabledDays = page.locator('.p-datepicker-day-cell:not(.p-datepicker-other-month)');
  await enabledDays.first().waitFor({ timeout: 5_000 });
  // 同表单常见 start/end 日期对且校验 end>start（最小间隔），第二个日期选月末格必然晚于首格
  await (dayOrdinal === 'last' ? enabledDays.last() : enabledDays.first()).click();
  const hourIncrement = page.locator('.p-datepicker-hour-picker button').first();
  if (await hourIncrement.isVisible().catch(() => false)) {
    await hourIncrement.click();
    await page.locator('.p-datepicker-minute-picker button').first().click();
  }
  // 点对话框标题（面板外部）收起；面板 appendTo body，不遮挡标题
  await page.locator('.p-dialog-title').first().click();
  await page.locator('.p-datepicker-panel').waitFor({ state: 'detached', timeout: 5_000 });
}

// —— 真实菜单导航（2026-10-20 起 e2e 全面真实点击化）——
// 全部基于渲染后的侧边栏 DOM 与真实点击：不读菜单树接口、不 hash 直达。
// 侧边栏是 PrimeVue PanelMenu（src/layout/navigationMenu/VerticalMenu.vue）：
//   根节点 → .p-panelmenu-panel（.p-panelmenu-header 内 span.menu-nav-item[role=button] 开合，
//            折叠时 .p-panelmenu-content-container display:none，子树仍在 DOM）
//   子分组 → li.p-panelmenu-item[aria-expanded]（span.menu-nav-item[role=button] 开合）
//   叶子   → a.menu-nav-item（.menu-label 为标题；router-link href 对应单 shell 内路径）

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

// 侧边栏叶子链接的文本定位器（menu-label 精确匹配；Vue 重渲染后自动重新解析，
// 比打 data 标记再点击稳——标记元素会被 PanelMenu 重渲染替换导致 detached）
function menuLeafLink(page: Page, title: string, rootGroup?: string) {
  const scope = rootGroup ? menuRoot(page, rootGroup) : page.locator('.p-panelmenu');
  return scope
    .locator('a.menu-nav-item')
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
async function nextMenuAction(page: Page, title: string, rootGroup?: string): Promise<MenuAction> {
  return page.evaluate(({ leafTitle, rootGroup }) => {
    const panel = rootGroup
      ? Array.from(document.querySelectorAll('.p-panelmenu-panel')).find((element) =>
        element.querySelector(':scope > .p-panelmenu-header .menu-label')?.textContent?.trim() === rootGroup)
      : document;
    const links = Array.from(panel?.querySelectorAll('a.menu-nav-item') ?? []);
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
  }, { leafTitle: title, rootGroup });
}

// 真实点击导航到侧边栏叶子：逐层真实 click 展开折叠祖先，再真实 click 叶子链接。
// 「找不到叶子」不立即判死：dev server 整页 reload（vite 依赖再优化、HMR 等）期间
// PanelMenu 会短暂整体卸载，先有限等待侧边栏重挂载（真缺失超时后仍会失败）。
export async function navigateByMenu(page: Page, leafTitle: string, rootGroup?: string): Promise<void> {
  const missingDeadline = Date.now() + 5_000;
  for (let round = 0; round < 10; round += 1) {
    const action = await nextMenuAction(page, leafTitle, rootGroup);
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
  await menuLeafLink(page, leafTitle, rootGroup).click();
  await sleep(250);
}


export const ADMIN_ROOTS = ['总览', '区服', '内容运营', '在线运维', '玩家', '商业化', 'C 端交付', '审计', '系统'] as const;
export const SUPERADMIN_ROOTS = ['总览', '系统'] as const;
export const SUPERADMIN_GROUPS = ['开发工具', '帮助文档', '全局配置'] as const;
export const SUPERADMIN_LEAVES = [
  '菜单管理', '参数配置', '字典管理', '任务调度', '开放接口身份', '图形验证码配置', '平台系统邮件',
  '库表管理', '后台接口', 'SqlSugar', 'JWT配置', '限流配置', '日志配置', '密码策略配置', '全局渠道定义',
] as const;

export function menuRoot(page: Page, title: string) {
  return page.locator('.p-panelmenu-panel').filter({
    has: page.locator('.p-panelmenu-header').getByText(title, { exact: true }),
  });
}

export async function expectOverview(page: Page): Promise<void> {
  await expect(page).toHaveURL(/#\/dashboard\/home(?:[/?]|$)/);
  await expect(menuRoot(page, '总览')).toBeVisible();
}

export function userMenuButton(page: Page) {
  return page.locator('button[aria-haspopup="menu"]').filter({
    has: page.locator('.layout-navbars-breadcrumb-user-link-photo'),
  });
}

export async function logoutByMenu(page: Page): Promise<void> {
  await userMenuButton(page).click();
  await page.locator('.p-menu-overlay').getByText('退出登录', { exact: true }).click();
  await page.getByRole('button', { name: '确定', exact: true }).click();
  await expect(page).toHaveURL(/#\/login/);
}

// C400 单实例账号不再拼接 @tenantId；身份与菜单必须由真实表单登录建立。
export async function signIn(page: Page, account: string, password: string): Promise<string> {
  await page.goto('/#/login');
  await page.locator('#login-account').fill(account);
  await page.locator('#login-password').fill(password);
  const responsePromise = page.waitForResponse(response =>
    /\/api\/systemAuth\/login(?:\?|$)/.test(response.url()) && response.request().method() === 'POST');
  await page.locator('.login-submit').click();
  const response = await responsePromise;
  expect(response.ok(), '真实登录 POST 成功').toBe(true);
  const envelope = await response.json();
  expect(envelope.code).toBe(0);
  const token: unknown = envelope.data?.accessToken ?? envelope.data?.AccessToken;
  expect(typeof token).toBe('string');
  await expectOverview(page);
  return token as string;
}
