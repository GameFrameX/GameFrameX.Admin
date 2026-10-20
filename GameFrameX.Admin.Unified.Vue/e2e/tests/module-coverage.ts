import { expect, test, type Locator, type Page } from '@playwright/test';
import { navigateByMenu, shot, sleep } from './helpers';

// —— 分模块覆盖扫描共用骨架（2026-10-20 套件重组）——
// 每个模块（侧边栏根面板）一个用例（modules/*.e2e.ts），本文件提供共用能力：
//   installApiRecorder / readApiRecords / apiAnomalies —— 被动 API 记录与异常过滤
//     （fetch/XHR 打桩只读不改，证明页面自身发出的请求及结果，符合真实点击化约束）
//   renderedMenuGroups —— 按根面板（模块）枚举渲染后侧边栏的叶子（不读菜单树接口）
//   sweepGroup —— 模块内逐叶子：真实点击导航 → 404 采样 → 搜索/重置/新增对话框交互探测
// 覆盖产物写入用例 annotations（leaves 清单 + 每叶交互面），报告可审计。

export type ApiRecord = { url: string; status: number; code: number | null };

// 页面级网络记录器：addInitScript 必须在首次 goto 前安装（对后续整页加载均生效；
// hash 导航不重载文档，同页持续记录）。evaluate 函数体序列化丢失模块闭包，逻辑必须函数内定义。
export async function installApiRecorder(page: Page): Promise<void> {
  await page.addInitScript((): void => {
    type ApiEntry = { url: string; status: number; code: number | null };
    const readFieldLocal = (source: object, keys: string[]): unknown => {
      for (const key of keys) {
        if (key in source) return Reflect.get(source, key);
      }
      return undefined;
    };
    const scope = window as unknown as { __e2eApi?: ApiEntry[] };
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
      const openArgs = arguments as unknown as Parameters<typeof openOriginal>;
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
      const sendArgs = arguments as unknown as Parameters<typeof sendOriginal>;
      return sendOriginal.apply(this, sendArgs);
    };
  });
}

export async function readApiRecords(page: Page): Promise<ApiRecord[]> {
  return page.evaluate((): { url: string; status: number; code: number | null }[] => {
    const scope = window as unknown as { __e2eApi?: { url: string; status: number; code: number | null }[] };
    return scope.__e2eApi ?? [];
  });
}

// 异常口径与巡查套件一致：HTTP>=400 或业务 code!=0（信封成功码 0）；
// 排除登录/菜单树自身与 OPTIONS 预检（status 0 的打桩噪声不含预检，此处仅按 URL 排除）。
export function apiAnomalies(records: ApiRecord[]): ApiRecord[] {
  return records.filter(
    (record) =>
      !record.url.includes('sysAuth') &&
      !record.url.includes('loginMenuTree') &&
      (record.status >= 400 || (record.code !== null && record.code !== 0)),
  );
}

export type MenuGroup = { group: string; leaves: string[] };

// 读取渲染后侧边栏的根面板（模块）及其叶子（折叠容器内叶子仍在 DOM，去重保序）。
// 根面板标题取 .p-panelmenu-header 内 .menu-label；叶子为面板子树内 a.menu-nav-item。
export async function renderedMenuGroups(page: Page): Promise<MenuGroup[]> {
  return page.evaluate(() => {
    const dedupe = (titles: string[]): string[] => {
      const seen = new Set<string>();
      return titles.filter((title) => {
        if (!title || seen.has(title)) return false;
        seen.add(title);
        return true;
      });
    };
    return Array.from(document.querySelectorAll('.p-panelmenu-panel'))
      .map((panel) => {
        const header = panel.querySelector(':scope > .p-panelmenu-header .menu-label');
        const group = (header?.textContent ?? '').trim();
        const leaves = dedupe(
          Array.from(panel.querySelectorAll('a.menu-nav-item .menu-label')).map((el) =>
            (el.textContent ?? '').trim(),
          ),
        );
        return { group, leaves };
      })
      .filter((entry) => entry.group && entry.leaves.length > 0);
  });
}

export type LeafReport = {
  leaf: string;
  is404: boolean;
  hasSearch: boolean;
  hasReset: boolean;
  hasAdd: boolean;
  addDialogOpened: boolean;
  addDialogClosed: boolean;
};

async function visibleButton(page: Page, nameRe: RegExp): Promise<boolean> {
  const button = page.locator(`role=button[name="${nameRe.source}"]`).first();
  return (await button.isVisible()) && (await button.isEnabled());
}

// 新增对话框开/关探测（沿 crud-dialog-sweep 的口径）：
// 打开 → .p-dialog 可见；取消（Escape 兜底）→ 隐藏。dev server 整页 reload 连同对话框卸载视为已关。
async function probeAddDialog(page: Page, report: LeafReport, issues: string[]): Promise<void> {
  const addButton = page.locator('role=button[name="新增"]').first();
  if (!(await addButton.isVisible()) || !(await addButton.isEnabled())) return;
  report.hasAdd = true;
  await addButton.click();
  const dialog = page.locator('.p-dialog');
  const opened = await pollState(dialog, true, 2500);
  report.addDialogOpened = opened;
  if (!opened) {
    issues.push(`${report.leaf}: 新增未打开对话框`);
    return;
  }
  try {
    await page.locator('.p-dialog >> role=button[name="取消"]').click();
  } catch {
    // 点击超时但对话框可能已随整页 reload 卸载：下方按最终可见性判定
  }
  let closed = await pollState(dialog, false, 2500);
  if (!closed) {
    await page.keyboard.press('Escape');
    closed = await pollState(dialog, false, 1500);
  }
  report.addDialogClosed = closed;
  if (!closed) issues.push(`${report.leaf}: 取消未关闭新增对话框`);
}

async function pollState(locator: Locator, wantVisible: boolean, timeoutMs: number): Promise<boolean> {
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    if ((await locator.isVisible()) === wantVisible) return true;
    await sleep(100);
  }
  return (await locator.isVisible()) === wantVisible;
}

// 单叶子交互探测：404 三点采样（挂载时序）+ 搜索/重置按钮真实点击 + 新增对话框开/关。
// 只做非破坏性交互（不提交表单、不确认删除）；数据级闭环由 crud-roundtrips 承载。
async function sweepLeaf(page: Page, leaf: string, issues: string[]): Promise<LeafReport> {
  const report: LeafReport = {
    leaf,
    is404: false,
    hasSearch: false,
    hasReset: false,
    hasAdd: false,
    addDialogOpened: false,
    addDialogClosed: false,
  };
  await navigateByMenu(page, leaf);

  const marker = page.locator('text=找不到此页面').first();
  for (const delay of [100, 250, 450]) {
    await sleep(delay);
    if (await marker.isVisible()) {
      report.is404 = true;
      issues.push(`${leaf}: 渲染 404`);
      return report;
    }
  }

  if (await visibleButton(page, /^(搜索|查询)$/)) {
    report.hasSearch = true;
    await page.locator('role=button[name=/^(搜索|查询)$/]').first().click();
    await sleep(400);
  }
  if (await visibleButton(page, /^重置$/)) {
    report.hasReset = true;
    await page.locator('role=button[name="重置"]').first().click();
    await sleep(300);
  }
  await probeAddDialog(page, report, issues);
  return report;
}
export type GroupSweepOptions = {
  // 模块用例对叶子数的下限断言（防止模块菜单整体丢失后被静默跳过）
  minLeaves?: number;
  // 交互探测后允许截图标签（默认组名）
  shotLabel?: string;
};

// 模块（根面板）级覆盖扫描：断言组存在且叶子数达下限，逐叶子导航 + 交互探测，
// 汇总 issues 并把覆盖清单写入用例 annotations。返回叶子报告（调用方追加断言）。
export async function sweepGroup(
  page: Page,
  groupName: string,
  options: GroupSweepOptions = {},
): Promise<LeafReport[]> {
  const groups = await renderedMenuGroups(page);
  const entry = groups.find((item) => item.group === groupName);
  if (!entry) {
    throw new Error(`当前侧边栏未找到模块组「${groupName}」；现有组：${groups.map((g) => g.group).join('、')}`);
  }
  const { minLeaves = 1, shotLabel = groupName } = options;
  expect(entry.leaves.length, `模块「${groupName}」叶子数`).toBeGreaterThanOrEqual(minLeaves);

  const issues: string[] = [];
  const reports: LeafReport[] = [];
  for (const leaf of entry.leaves) {
    reports.push(await sweepLeaf(page, leaf, issues));
  }

  test.info().annotations.push({
    type: `模块覆盖:${groupName}`,
    description: JSON.stringify(reports),
  });
  expect(issues, `模块「${groupName}」交互异常`).toEqual([]);
  await shot(page, `module-${shotLabel}`);
  return reports;
}

// 模块用例收尾断言：本模块扫描期间页面自身触发的 API 无异常且确有请求发出。
export async function expectNoApiAnomaly(page: Page, minRequests: number): Promise<void> {
  const records = await readApiRecords(page);
  expect(records.length, '模块扫描期间页面触发的 API 请求数').toBeGreaterThanOrEqual(minRequests);
  expect(apiAnomalies(records), '页面触发的 API 异常').toEqual([]);
}
