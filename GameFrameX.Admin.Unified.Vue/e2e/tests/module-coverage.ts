import { expect, test, type Locator, type Page } from '@playwright/test';
import { navigateByMenu, shot, sleep } from './helpers';

// —— 分模块覆盖扫描共用骨架（2026-10-20 套件重组；同日补动作层：搜索规则/详情/编辑回显/删除取消/下拉）——
// 每个模块（侧边栏根面板）一个用例（modules/*.e2e.ts），本文件提供共用能力：
//   installApiRecorder / readApiRecords / apiAnomalies —— 被动 API 记录（含请求体快照）与异常过滤
//     （fetch/XHR 打桩只读不改，证明页面自身发出的请求及结果，符合真实点击化约束）
//   renderedMenuGroups —— 按根面板（模块）枚举渲染后侧边栏的叶子（不读菜单树接口）
//   sweepGroup —— 模块内逐叶子：真实点击导航 → 404 采样 → 搜索/重置/新增对话框交互探测；
//     actions 开启时追加行级动作探针：详情开/关、编辑回显 + 下拉面板开、删除二次确认取消、
//     搜索关键词真实输入 + payload 断言 + 重置清空断言、搜索下拉真实选值。
//     全部非破坏性（不提交编辑、不确认删除）；写入半边（编辑提交/真删）由 crud-roundtrips 承载。
// 覆盖产物写入用例 annotations（leaves 清单 + 每叶交互面），报告可审计。

export type ApiRecord = { url: string; status: number; code: number | null; body?: string };

// 页面级网络记录器：addInitScript 必须在首次 goto 前安装（对后续整页加载均生效；
// hash 导航不重载文档，同页持续记录）。evaluate 函数体序列化丢失模块闭包，逻辑必须函数内定义。
export async function installApiRecorder(page: Page): Promise<void> {
  await page.addInitScript((): void => {
    type ApiEntry = { url: string; status: number; code: number | null; body?: string };
    const readFieldLocal = (source: object, keys: string[]): unknown => {
      for (const key of keys) {
        if (key in source) return Reflect.get(source, key);
      }
      return undefined;
    };
    const scope = window as unknown as { __e2eApi?: ApiEntry[] };
    scope.__e2eApi = [];
    // 请求体快照（截断）：搜索规则探针靠它断言「表单值真实进入请求 payload」
    const bodySnapshot = (raw: unknown): string | undefined => {
      if (raw == null) return undefined;
      const text = typeof raw === 'string' ? raw : String(raw);
      return text.length > 400 ? text.slice(0, 400) : text;
    };
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
      const requestBody = bodySnapshot(init?.body ?? (input instanceof Request ? input.body : undefined));
      return originalFetch.call(this, input, init).then((response) => {
        if (url.includes('/api/')) {
          response
            .clone()
            .json()
            .then((body: unknown) => push({ url, status: response.status, code: businessCode(body), body: requestBody }))
            .catch(() => push({ url, status: response.status, code: null, body: requestBody }));
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
    XMLHttpRequest.prototype.send = function patchedSend(
      this: XMLHttpRequest & { __url?: string; __body?: string },
      payload?: Document | XMLHttpRequestBodyInit | null,
    ) {
      this.__body = bodySnapshot(payload);
      this.addEventListener('load', () => {
        if (!(this.__url ?? '').includes('/api/')) return;
        let code: number | null = null;
        try {
          code = businessCode(JSON.parse(this.responseText));
        } catch {
          code = null;
        }
        push({ url: this.__url ?? '', status: this.status, code, body: this.__body });
      });
      const sendArgs = arguments as unknown as Parameters<typeof sendOriginal>;
      return sendOriginal.apply(this, sendArgs);
    };
  });
}

export async function readApiRecords(page: Page): Promise<ApiRecord[]> {
  return page.evaluate((): { url: string; status: number; code: number | null; body?: string }[] => {
    const scope = window as unknown as {
      __e2eApi?: { url: string; status: number; code: number | null; body?: string }[];
    };
    return scope.__e2eApi ?? [];
  });
}

// 异常口径与巡查套件一致：HTTP>=400 或业务 code!=0（信封成功码 0）；
// 排除登录/菜单树自身。
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
  // —— 表单验证层（2026-10-20 三轮：新增/编辑空表单必须被拦截）——
  addValidationBlocked: boolean;
  addValidationVisible: boolean;
  editValidationBlocked: boolean;
  editValidationVisible: boolean;
  // —— 动作层（actions 开启时填写）——
  hasSearchForm: boolean;
  searchPayloadAsserted: boolean;
  searchResetAsserted: boolean;
  searchSelectInteracted: boolean;
  // —— 检索区按钮层（2026-10-20 二轮：所有按钮功能）——
  advancedToggleAsserted: boolean;
  resetRequeried: boolean;
  columnConfigToggled: boolean;
  exportDownloaded: boolean;
  customButtons: string[];
  hasRows: boolean;
  detailOpened: boolean;
  editEchoed: boolean;
  editSelectPanelOpened: boolean;
  deleteConfirmCancelled: boolean;
};

const emptyReport = (leaf: string): LeafReport => ({
  leaf,
  is404: false,
  hasSearch: false,
  hasReset: false,
  hasAdd: false,
  addDialogOpened: false,
  addDialogClosed: false,
  addValidationBlocked: false,
  addValidationVisible: false,
  editValidationBlocked: false,
  editValidationVisible: false,
  hasSearchForm: false,
  searchPayloadAsserted: false,
  searchResetAsserted: false,
  searchSelectInteracted: false,
  advancedToggleAsserted: false,
  resetRequeried: false,
  columnConfigToggled: false,
  exportDownloaded: false,
  customButtons: [],
  hasRows: false,
  detailOpened: false,
  editEchoed: false,
  editSelectPanelOpened: false,
  deleteConfirmCancelled: false,
});

async function pollState(locator: Locator, wantVisible: boolean, timeoutMs: number): Promise<boolean> {
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    if ((await locator.isVisible()) === wantVisible) return true;
    await sleep(100);
  }
  return (await locator.isVisible()) === wantVisible;
}

// 注意：role 选择器的带引号 name 是字面量匹配，正则必须用不带引号的 name=/…/ 形态
async function visibleButton(page: Page, nameRe: RegExp): Promise<boolean> {
  const button = page.locator(`role=button[name=/${nameRe.source}/]`).first();
  return (await button.isVisible()) && (await button.isEnabled());
}

// 防御：如页面残留模态遮罩（如全局搜索/空结果提示），Escape 收掉再继续后续探针
async function dismissStrayMask(page: Page): Promise<void> {
  const mask = page.locator('.p-dialog-mask:visible, .p-overlay:visible').first();
  if (await mask.isVisible().catch(() => false)) {
    await page.keyboard.press('Escape');
    await sleep(300);
  }
}

// 新增对话框探测：打开 → （有必填契约时）空表单提交必须被拦截 → 取消关闭。
// 拦截 = 无新增/编辑 API 请求 + 弹窗保持打开；BaseEditDialog（FormField 模式）还必须显示可见校验错误。
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

  // —— 表单验证探针：有必填标记（*）才做「空提交必须拦截」断言；先尝试清空全部可编辑输入，
  // 若清空后仍有默认值残留（InputNumber 预填/程序回填，属合法默认提交路径）则跳过拦截断言 ——
  const requiredMarkers = await dialog.locator('.form-label-required').count();
  if (requiredMarkers > 0) {
    const editable = dialog.locator('input:visible:not([type="checkbox"]):not([type="hidden"]), textarea:visible');
    const editableCount = await editable.count();
    for (let i = 0; i < editableCount; i += 1) {
      await editable.nth(i).fill('', { timeout: 1500 }).catch(() => undefined);
    }
    const residualValues = await dialog.evaluate((el) => {
      const boxes = Array.from(el.querySelectorAll('input, textarea')).filter(
        (node) => node.getBoundingClientRect().height > 0 && !(node as HTMLInputElement).disabled && !(node as HTMLInputElement).readOnly,
      );
      return boxes.some((node) => (node as HTMLInputElement).value !== '');
    });
    if (!residualValues) {
      const recordsBefore = (await readApiRecords(page)).length;
      await page.locator('.p-dialog:visible button').filter({ hasText: '确定' }).first().click();
      await sleep(900);
      const newRecords = (await readApiRecords(page)).slice(recordsBefore);
      const writeFired = newRecords.some(
        (record) => /\/(add|update|edit|save)/.test(record.url) && !/page|select|list|detail/.test(record.url),
      );
      const stillOpen = await dialog.isVisible().catch(() => false);
      report.addValidationBlocked = !writeFired && stillOpen;
      if (!report.addValidationBlocked) {
        const accepted = newRecords.some((record) => record.code === 0 && /\/(add|update)/.test(record.url));
        issues.push(
          `${report.leaf}: 新增空表单提交未被拦截（${writeFired ? '发出了写请求' : '弹窗被关闭'}${accepted ? '且后端接受了空数据' : ''}）`,
        );
      } else {
        const isBaseDialog = await dialog.locator('.base-edit-dialog').count();
        if (isBaseDialog) {
          const errorVisible = await dialog
            .locator('.p-message-error, .p-invalid')
            .first()
            .isVisible()
            .catch(() => false);
          report.addValidationVisible = errorVisible;
          if (!errorVisible) issues.push(`${report.leaf}: 新增空表单被拦截但无可见校验提示`);
        }
      }
    }
  }

  await closeDialog(page);
  const closed = await pollState(dialog, false, 2500);
  report.addDialogClosed = closed;
  if (!closed) issues.push(`${report.leaf}: 取消未关闭新增对话框`);
}

// 编辑侧表单验证探针：清空全部可编辑文本框 → 确定 → 必须被拦截（无写请求 + 弹窗仍在 + 可见错误）；
// 若未被拦截（缺陷），用暂存原值恢复提交，避免污染行数据。
async function probeEditFormValidation(page: Page, report: LeafReport, issues: string[]): Promise<boolean> {
  const dialog = page.locator('.p-dialog:visible').first();
  if (!(await dialog.isVisible().catch(() => false))) return false;
  const requiredMarkers = await dialog.locator('.form-label-required').count();
  if (requiredMarkers === 0) return false;
  const originals = await dialog.evaluate((el) => {
    const boxes = Array.from(el.querySelectorAll('input, textarea')).filter(
      (node) =>
        node.getBoundingClientRect().height > 0 &&
        !(node as HTMLInputElement).disabled &&
        !(node as HTMLInputElement).readOnly &&
        (node as HTMLInputElement).type !== 'checkbox' &&
        (node as HTMLInputElement).type !== 'hidden',
    );
    return boxes.map((node) => (node as HTMLInputElement).value);
  });
  if (originals.length === 0) return false;
  const boxes = dialog.locator('input:visible:not([type="checkbox"]):not([type="hidden"]), textarea:visible');
  for (let i = 0; i < originals.length; i += 1) {
    await boxes.nth(i).fill('', { timeout: 2000 }).catch(() => undefined);
  }
  // 清空后仍有默认值残留（InputNumber 预填等）→ 属合法提交路径，跳过拦截断言
  const residualValues = await dialog.evaluate((el) => {
    const nodes = Array.from(el.querySelectorAll('input, textarea')).filter(
      (node) => node.getBoundingClientRect().height > 0 && !(node as HTMLInputElement).disabled && !(node as HTMLInputElement).readOnly,
    );
    return nodes.some((node) => (node as HTMLInputElement).value !== '');
  });
  if (residualValues) return false;
  const recordsBefore = (await readApiRecords(page)).length;
  await page.locator('.p-dialog:visible button').filter({ hasText: '确定' }).first().click();
  await sleep(900);
  const newRecords = (await readApiRecords(page)).slice(recordsBefore);
  const writeFired = newRecords.some(
    (record) => /\/(add|update|edit|save)/.test(record.url) && !/page|select|list|detail/.test(record.url),
  );
  const stillOpen = await dialog.isVisible().catch(() => false);
  report.editValidationBlocked = !writeFired && stillOpen;
  if (report.editValidationBlocked) {
    const errorVisible = await dialog.locator('.p-message-error, .p-invalid').first().isVisible().catch(() => false);
    report.editValidationVisible = errorVisible;
    if (!errorVisible) issues.push(`${report.leaf}: 编辑清空必填被拦截但无可见校验提示`);
    return true;
  }
  issues.push(`${report.leaf}: 编辑清空必填后提交未被拦截（写请求已发出）`);
  // 缺陷兜底：恢复原值再提交一次，尽量还原行数据
  if (writeFired && stillOpen) {
    for (let i = 0; i < originals.length; i += 1) {
      await boxes.nth(i).fill(originals[i], { timeout: 2000 }).catch(() => undefined);
    }
    await page.locator('.p-dialog:visible button').filter({ hasText: '确定' }).first().click();
    await sleep(900);
  }
  return true;
}

// 关闭最上层对话框/抽屉并验证：页脚 取消/关闭 → 头部关闭按钮（X）→ Escape，最多三轮
async function closeDialog(page: Page): Promise<void> {
  for (let round = 0; round < 4; round += 1) {
    // 打开的 Select/下拉面板会吞掉页脚按钮点击（outside-click 先吃掉 mousedown）——每轮先收起
    await collapseSelectOverlay(page);
    const anySurface = page.locator('.p-dialog:visible, .p-drawer:visible').first();
    if (!(await anySurface.isVisible().catch(() => false))) return;
    const footerButton = page
      .locator('.p-dialog:visible button, .p-drawer:visible button')
      .filter({ hasText: /^(取消|关闭)$/ })
      .last();
    if (await footerButton.isVisible().catch(() => false)) {
      await footerButton.click().catch(() => undefined);
      await sleep(300);
      continue;
    }
    const headerClose = page.locator('.p-dialog:visible .p-dialog-close-button, .p-drawer:visible .p-drawer-close-button').last();
    if (await headerClose.isVisible().catch(() => false)) {
      await headerClose.click().catch(() => undefined);
      await sleep(300);
      continue;
    }
    await page.keyboard.press('Escape');
    await sleep(300);
  }
}

// 收起已展开的 Select/AutoComplete 面板：真实点击视口角落。
// 打开的面板会吞掉页脚按钮的点击（outside-click 先吃掉 mousedown），必须先收起才能关弹窗；
// 视口角落落在遮罩上——PrimeVue Dialog 默认 dismissableMask=false，不会误关宿主弹窗。
async function collapseSelectOverlay(page: Page): Promise<void> {
  await page.mouse.click(5, 5);
  await sleep(250);
}
// —— 搜索规则探针的页面豁免清单（按侧边栏渲染叶子名）——
// Online 游标页（C349/C365）：ServerId 作用域守卫是设计行为（issue #4 + vault C2 S1.7 硬校验），
//   关键词/下拉断言不适用；玩家中心 游戏角色/游戏用户 首个检索字段是数字 ID（long wire），
//   中文关键词会触发绑定 500（COV-P2-03 跟进统一 400），用数字关键词验证同样的接线。
const SEARCH_RULES_SKIP_LEAVES = new Set([
  '在线总览', '消息审计', '玩家分群', '配置灰度发布', '举报处置', '资产流水', '交易详情',
  '资产审计', '匹配队列总览', '匹配失败原因', '异常Ticket查询', '玩家时间线', '统一审计检索', '受控操作',
]);
const NUMERIC_KEYWORD_LEAVES = new Set(['游戏角色', '游戏用户', '传输签名密钥', '会话密钥']);

// —— 搜索规则探针：关键词真实输入 → 搜索点击 → 断言请求 payload 携带关键词；
// 搜索下拉（如有）真实选值后再查一次；重置后断言输入框清空。 ——
async function probeSearchRules(
  page: Page,
  report: LeafReport,
  issues: string[],
  recordsBefore: number,
): Promise<number> {
  const form = page.locator('.search-form').first();
  if (!(await form.isVisible())) return recordsBefore;
  report.hasSearchForm = true;
  if (SEARCH_RULES_SKIP_LEAVES.has(report.leaf)) {
    // 豁免页仅保留重置/表单存在性观察，不做关键词与下拉断言（设计内守卫）
    return recordsBefore;
  }
  // 查询按钮预检：无 搜索/查询 按钮的表单（纯联动/守卫型）不做关键词探针，避免硬超时
  if (!(await visibleButton(page, /^(搜索|查询)$/))) {
    return recordsBefore;
  }

  // 取第一个「可见且非只读、非 hidden、非 Select 自带过滤输入」输入框
  // （DatePicker 输入框 readonly 天然排除；PrimeVue Select 的可编辑输入框是下拉过滤，不是查询字段）
  const pickInput = () =>
    page.evaluate(() => {
      const inputs = Array.from(document.querySelectorAll('.search-form input'));
      const target = inputs.find(
        (el) =>
          el.getBoundingClientRect().height > 0 &&
          !el.readOnly &&
          el.type !== 'hidden' &&
          el.type !== 'checkbox' &&
          el.closest('.p-select') === null && el.closest('.p-autocomplete') === null,
      );
      if (!target) return null;
      const index = inputs.indexOf(target);
      return { index, placeholder: target.getAttribute('placeholder') ?? '' };
    });
  let inputInfo = await pickInput();
  if (!inputInfo) {
    // 基础面板无文本输入：尝试展开高级查询后重取（advanced 字段默认折叠）
    const advancedToggle = page.locator('role=button[name=/高级/]').first();
    if (await advancedToggle.isVisible().catch(() => false)) {
      await advancedToggle.click();
      await sleep(300);
      inputInfo = await pickInput();
    }
  }
  if (inputInfo) {
    // 数字 ID 检索字段（long wire）用纯数字关键词：中文会触发后端绑定 500（COV-P2-03）
    const keyword = NUMERIC_KEYWORD_LEAVES.has(report.leaf)
      ? `8888${Date.now().toString().slice(-6)}`
      : `E2E检索${Date.now().toString(36).slice(-5)}`;
    const input = page.locator(`.search-form input >> nth=${inputInfo.index}`);
    await input.fill(keyword);
    await page.locator('role=button[name=/^(搜索|查询)$/]').first().click();
    await sleep(800);
    const records = await readApiRecords(page);
    const carried = records
      .slice(recordsBefore)
      .some((record) => (record.body ?? '').includes(keyword) || record.url.includes(encodeURIComponent(keyword)));
    report.searchPayloadAsserted = carried;
    if (!carried) issues.push(`${report.leaf}: 搜索关键词未进入请求 payload`);
  }

  // 搜索下拉：Select 与 AutoComplete 两种形态都真实点开 → 选首项 → 再查一次（断言有新请求）；
  // 与文本输入独立执行——仅含下拉的搜索表单（如按区服/渠道过滤）也要覆盖
  const searchSelect = page.locator('.search-form .p-select').first();
  const searchAutoComplete = page.locator('.search-form .p-autocomplete input').first();
  const dropdownHost = (await searchSelect.isVisible().catch(() => false))
    ? { kind: 'select' as const, locator: searchSelect }
    : (await searchAutoComplete.isVisible().catch(() => false))
      ? { kind: 'autocomplete' as const, locator: searchAutoComplete }
      : null;
  if (dropdownHost) {
    await dropdownHost.locator.click();
    if (dropdownHost.kind === 'autocomplete') {
      // AutoComplete 建议列表按输入过滤触发：键入一个字符才会出选项
      await page.keyboard.type('a', { delay: 60 });
      await sleep(400);
    }
    const optionLocator =
      dropdownHost.kind === 'select'
        ? page.locator('.p-select-overlay:visible .p-select-option >> nth=0')
        : page.locator('.p-autocomplete-overlay:visible li >> nth=0');
    if (await optionLocator.isVisible({ timeout: 1500 }).catch(() => false)) {
      await optionLocator.click();
      const afterSelect = (await readApiRecords(page)).length;
      await page.locator('role=button[name=/^(搜索|查询)$/]').first().click();
      await sleep(600);
      const recordsAfter = await readApiRecords(page);
      report.searchSelectInteracted = recordsAfter.length > afterSelect;
      if (!report.searchSelectInteracted) issues.push(`${report.leaf}: 搜索下拉选值后未触发新查询`);
    } else {
      await collapseSelectOverlay(page);
    }
  }

  // 重置：输入框清空 + 触发无条件重查（仅有下拉时验证按钮存在即可）
  if (inputInfo && (await visibleButton(page, /^重置$/))) {
    await page.locator('role=button[name="重置"]').first().click();
    await sleep(500);
    const input = page.locator(`.search-form input >> nth=${inputInfo.index}`);
    const cleared = await input.evaluate((el) => (el as HTMLInputElement).value === '');
    report.searchResetAsserted = cleared;
    if (!cleared) issues.push(`${report.leaf}: 重置未清空搜索输入`);
  }
  return (await readApiRecords(page)).length;
}

// —— 行级动作探针：详情开/关、编辑回显 + 下拉面板、删除二次确认取消 ——
async function probeRowActions(page: Page, report: LeafReport, issues: string[]): Promise<void> {
  const row = page.locator('.p-datatable-tbody tr:visible').nth(0);
  if (!(await row.isVisible().catch(() => false))) return;
  // 空态行（colspan 占位的「暂无数据」）不是数据行，行级动作无从探测
  const isEmptyRow = await row.locator('.crud-data-table-empty, .p-datatable-emptymessage').first().isVisible().catch(() => false);
  if (isEmptyRow) return;
  report.hasRows = true;

  // 详情：行内「详情」按钮 → 弹窗/抽屉可见且内容非空 → 关闭
  const detailButton = row.locator('button').filter({ hasText: '详情' }).first();
  if (await detailButton.isVisible().catch(() => false)) {
    await detailButton.click();
    const surface = page.locator('.p-dialog:visible, .p-drawer:visible').first();
    const opened = await pollState(surface, true, 2500);
    if (opened) {
      const textLength = await surface.evaluate((el) => (el.textContent ?? '').trim().length);
      report.detailOpened = textLength > 10;
      if (!report.detailOpened) issues.push(`${report.leaf}: 详情弹窗内容为空`);
      await closeDialog(page);
      await pollState(surface, false, 2000);
    } else {
      issues.push(`${report.leaf}: 详情按钮未打开弹窗`);
    }
  }

  await dismissStrayMask(page);
  // 编辑：打开回显 → 断言表单至少一个控件带值 → 首个下拉真实点开面板 → 关闭不保存
  const editButton = row.locator('button').filter({ hasText: '编辑' }).first();
  if (await editButton.isVisible().catch(() => false)) {
    await editButton.click();
    const dialog = page.locator('.p-dialog:visible').first();
    const opened = await pollState(dialog, true, 2500);
    if (opened) {
      const echoed = await dialog.evaluate((el) => {
        const inputs = Array.from(el.querySelectorAll('input, textarea')).filter(
          (node) => node.getBoundingClientRect().height > 0,
        );
        const hasValue = inputs.some((node) => (node as HTMLInputElement).value !== '');
        const hasSelected = Array.from(el.querySelectorAll('.p-select')).some(
          (node) => (node.textContent ?? '').trim() !== '',
        );
        return hasValue || hasSelected;
      });
      report.editEchoed = echoed;
      if (!echoed) issues.push(`${report.leaf}: 编辑弹窗未回显行数据`);

      // 首个下拉：真实点开面板 → 有选项则选首项（选中即自然收起，改动随取消丢弃不落库）；
      // 禁用态下拉（编辑态锁定字段）跳过
      const firstSelect = dialog.locator('.p-select >> nth=0');
      // PrimeVue 禁用态靠 p-disabled 类/data-p-disabled 表达（原生 disabled/aria 不全），双查
      const selectInteractable = await firstSelect
        .evaluate((el) => !el.classList.contains('p-disabled') && el.getAttribute('data-p-disabled') !== 'true')
        .catch(() => false);
      if ((await firstSelect.isVisible().catch(() => false)) && selectInteractable) {
        await firstSelect.click();
        const overlay = page.locator('.p-select-overlay:visible');
        const panelOpened = await pollState(overlay, true, 1500);
        report.editSelectPanelOpened = panelOpened;
        if (panelOpened) {
          const firstOption = overlay.locator('.p-select-option').first();
          if (await firstOption.isVisible().catch(() => false)) {
            await firstOption.click();
          } else {
            await collapseSelectOverlay(page);
          }
          await sleep(200);
        }
      }
      // 编辑侧表单验证：清空必填提交必须被拦截（内部自带原值恢复兜底）
      await probeEditFormValidation(page, report, issues);
      await closeDialog(page);
      const closed = await pollState(page.locator('.p-dialog:visible'), false, 2500);
      if (!closed) issues.push(`${report.leaf}: 编辑弹窗取消未关闭`);
    } else {
      issues.push(`${report.leaf}: 编辑按钮未打开弹窗`);
    }
  }

  await dismissStrayMask(page);
  // 删除：真实点行删除 → 二次确认弹窗出现 → 点取消 → 断言行仍在（未删除）
  const deleteButton = row.locator('button').filter({ hasText: '删除' }).first();
  if (await deleteButton.isVisible().catch(() => false)) {
    const rowCountBefore = await page.locator('.p-datatable-tbody tr:visible').count();
    await deleteButton.click();
    const confirmDialog = page.locator('.p-confirmdialog:visible');
    const confirmed = await pollState(confirmDialog, true, 2500);
    if (confirmed) {
      const cancelButton = confirmDialog.locator('button').filter({ hasText: '取消' }).first();
      await cancelButton.click();
      await pollState(confirmDialog, false, 2000);
      await sleep(400);
      const rowCountAfter = await page.locator('.p-datatable-tbody tr:visible').count();
      report.deleteConfirmCancelled = rowCountAfter === rowCountBefore;
      if (!report.deleteConfirmCancelled) {
        issues.push(`${report.leaf}: 删除取消后行数变化（取消路径疑似执行了删除）`);
      }
    } else {
      // 部分页面删除直接执行（无二次确认）：立即恢复检查，行未消失则视为「无确认直接删」告警
      await sleep(400);
      const rowCountAfter = await page.locator('.p-datatable-tbody tr:visible').count();
      if (rowCountAfter === rowCountBefore) {
        issues.push(`${report.leaf}: 删除无二次确认弹窗`);
      } else {
        issues.push(`${report.leaf}: 删除无二次确认且直接删除了行（数据破坏风险）`);
      }
    }
  }
}

// —— 检索区按钮全量探针（2026-10-20 二轮）：高级/隐藏切换、重置重查、列显示、导出下载、
// 自定义 slot 按钮（破坏类必须二次确认→取消；弹窗类开/关；导出类捕获下载） ——
const KNOWN_SEARCH_BUTTONS = /^(搜索|查询|重置|新增|高级|隐藏)$/;
const DESTRUCTIVE_HINT = /(清空|清除|删除|启动全部|启动所有|暂停所有|唤醒|持久化|批量)/;
const EXPORT_HINT = /导出|export/i;

async function probeSearchAreaButtons(page: Page, report: LeafReport, issues: string[]): Promise<void> {
  // 高级/隐藏切换：展开 → 有新增可见输入 → 收起 → 隐藏
  const advancedToggle = page.locator('role=button[name=/高级/]').first();
  if (await advancedToggle.isVisible().catch(() => false)) {
    const visibleBefore = await page.locator('.search-form input:visible, .search-form .p-select:visible').count();
    await advancedToggle.click();
    await sleep(400);
    const visibleAfterExpand = await page.locator('.search-form input:visible, .search-form .p-select:visible').count();
    const hideToggle = page.locator('role=button[name=/隐藏/]').first();
    if (await hideToggle.isVisible().catch(() => false)) {
      await hideToggle.click();
      await sleep(300);
    }
    report.advancedToggleAsserted = visibleAfterExpand > visibleBefore;
  }

  // 列显示（CrudDataTable 工具栏）：开面板 → 勾/去勾首列 → 表头数变化 → 还原
  const columnButton = page.locator('role=button[name="列显示"]').first();
  if (await columnButton.isVisible().catch(() => false)) {
    await columnButton.click();
    const panel = page.locator('.column-config-popover');
    if (await pollState(panel, true, 2000)) {
      const firstRow = panel.locator('.panel-row').first();
      if (await firstRow.isVisible().catch(() => false)) {
        const headersBefore = await page.locator('.p-datatable-thead th:visible').count();
        await firstRow.locator('.p-checkbox, label').first().click();
        await sleep(400);
        const headersAfter = await page.locator('.p-datatable-thead th:visible').count();
        report.columnConfigToggled = headersAfter !== headersBefore;
        await firstRow.locator('.p-checkbox, label').first().click();
        await sleep(300);
      }
      await page.mouse.click(5, 5);
      await sleep(250);
    }
  }

  // 导出（工具栏 aria-label 导出 + slot 导出日志类）：点击 → 捕获下载事件与文件名
  const exportButtons = page.locator('role=button[name=/导出/]');
  const exportCount = await exportButtons.count();
  for (let i = 0; i < exportCount; i += 1) {
    const button = exportButtons.nth(i);
    if (!(await button.isVisible().catch(() => false))) continue;
    const downloadPromise = page.waitForEvent('download', { timeout: 8000 }).catch(() => null);
    await button.click().catch(() => undefined);
    const download = await downloadPromise;
    if (download) {
      report.exportDownloaded = (download.suggestedFilename() ?? '').length > 0;
      await download.cancel().catch(() => undefined);
    } else {
      issues.push(`${report.leaf}: 导出按钮未产生下载文件`);
    }
    await sleep(300);
  }

  // 自定义 slot 按钮（检索区内除已知标准按钮外的全部按钮，含图标钮）：
  // 破坏类（文案含 清空/启动所有/暂停所有/唤醒/持久化/批量）必须出二次确认 → 取消；
  // 其余：弹窗/抽屉打开则关闭，确认框出现则取消
  const customButtons = page.locator('.search-form-actions button');
  const total = await customButtons.count();
  for (let i = 0; i < total; i += 1) {
    const button = customButtons.nth(i);
    const text = ((await button.textContent().catch(() => '')) ?? '').trim();
    if (KNOWN_SEARCH_BUTTONS.test(text) || EXPORT_HINT.test(text)) continue;
    const label = text || (await button.getAttribute('aria-label').catch(() => null)) || '图标按钮';
    report.customButtons.push(label);
    const recordsBefore = (await readApiRecords(page)).length;
    await button.click({ timeout: 4000 }).catch(() => undefined);
    await sleep(700);
    const confirmDialog = page.locator('.p-confirmdialog:visible');
    const anyDialog = page.locator('.p-dialog:visible, .p-drawer:visible');
    if (await confirmDialog.isVisible().catch(() => false)) {
      await confirmDialog.locator('button').filter({ hasText: '取消' }).first().click();
      await pollState(confirmDialog, false, 2000);
      await sleep(200);
    } else if (DESTRUCTIVE_HINT.test(label)) {
      issues.push(`${report.leaf}: 破坏类按钮「${label}」无二次确认`);
    } else if (await anyDialog.isVisible().catch(() => false)) {
      await closeDialog(page);
      await pollState(page.locator('.p-dialog:visible, .p-drawer:visible'), false, 2000);
    }
    // 弹窗类按钮不应触发业务写请求；触发了且无弹窗的记为「直接执行」
    const recordsAfter = (await readApiRecords(page)).length;
    if (
      recordsAfter > recordsBefore &&
      !(await anyDialog.isVisible().catch(() => false)) &&
      DESTRUCTIVE_HINT.test(label)
    ) {
      issues.push(`${report.leaf}: 破坏类按钮「${label}」点击直接发出了请求（无确认）`);
    }
  }
}

export type GroupSweepOptions = {
  // 模块用例对叶子数的下限断言（防止模块菜单整体丢失后被静默跳过）
  minLeaves?: number;
  // 交互探测后允许截图标签（默认组名）
  shotLabel?: string;
  // 开启行级动作层（搜索规则 payload + 详情/编辑回显/删除取消 + 下拉交互）
  actions?: boolean;
};

// 单叶子交互探测：404 三点采样（挂载时序）+ 搜索/重置按钮真实点击 + 新增对话框开/关；
// actions 开启时追加搜索规则 payload 断言与行级动作探针。
async function sweepLeaf(
  page: Page,
  leaf: string,
  issues: string[],
  options: { actions: boolean },
): Promise<LeafReport> {
  const report = emptyReport(leaf);
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

  let recordsMark = (await readApiRecords(page)).length;
  if (options.actions) {
    // 检索区按钮全量探针先跑（新表单态）；其中破坏类按钮会被取消，不改变数据
    await probeSearchAreaButtons(page, report, issues);
    await dismissStrayMask(page);
    recordsMark = await probeSearchRules(page, report, issues, recordsMark);
    // 重置重查断言：重置点击后应发出新的列表请求
    if (report.hasSearchForm && (await visibleButton(page, /^重置$/))) {
      const beforeReset = (await readApiRecords(page)).length;
      await page.locator('role=button[name="重置"]').first().click();
      await sleep(700);
      const afterReset = (await readApiRecords(page)).length;
      report.resetRequeried = afterReset > beforeReset;
    }
  }
  if (await visibleButton(page, /^(搜索|查询)$/)) {
    report.hasSearch = true;
    if (!options.actions) {
      await page.locator('role=button[name=/^(搜索|查询)$/]').first().click();
      await sleep(400);
    }
  }
  if (!options.actions && (await visibleButton(page, /^重置$/))) {
    report.hasReset = true;
    await page.locator('role=button[name="重置"]').first().click();
    await sleep(300);
  }
  await dismissStrayMask(page);
  await probeAddDialog(page, report, issues);
  if (options.actions) {
    await dismissStrayMask(page);
    await probeRowActions(page, report, issues);
  }
  return report;
}

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
  const { minLeaves = 1, shotLabel = groupName, actions = false } = options;
  expect(entry.leaves.length, `模块「${groupName}」叶子数`).toBeGreaterThanOrEqual(minLeaves);

  const issues: string[] = [];
  const reports: LeafReport[] = [];
  for (const leaf of entry.leaves) {
    reports.push(await sweepLeaf(page, leaf, issues, { actions }));
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
