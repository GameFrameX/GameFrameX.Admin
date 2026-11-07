import { expect, type Page } from '@playwright/test';
import { mkdir, readFile, writeFile } from 'node:fs/promises';
import { dirname } from 'node:path';
import { navigateByMenu } from './helpers';

//  / 集成第 2 轮：业务管理员账号与 sys_admin 角色的全部非超管菜单授权
// 已由 Web.Api 首启种子引导（GAMEFRAMEX_ADMIN_INITIAL_ACCOUNT / _PASSWORD）创建，
// 原 admin-menu-fixture 的 role/grantMenu 主动授权例外随之删除；本文件只保留：
// 1. 隔离栈守卫（防止误打共享栈）；
// 2. 信封字段读取工具（供向导用例解析响应）；
// 3. 合服复核人账号的产品路径准备：由引导出来的 admin 在「账号管理」真实界面创建，
//  并通过「重置密码」取得明文初始口令，写入 state 目录供合服用例读取（不入证据目录）。
export function readField(value: unknown, name: string): unknown {
  if (!value || typeof value !== 'object') return undefined;
  const record = value as Record<string, unknown>;
  return record[name] ?? record[name[0].toUpperCase() + name.slice(1)];
}

export function isolatedAdminOrigin(): string {
  for (const name of ['E2E_APP_URL', 'E2E_ADMIN_API_URL']) {
    const value = process.env[name];
    if (!value) throw new Error(`${name} 必须显式指向 C406 隔离栈`);
    const url = new URL(value);
    if (!['127.0.0.1', 'localhost', '[::1]'].includes(url.hostname)
      || !url.port || ['20800', '20802', '20806', '20808', '5432'].includes(url.port)) {
      throw new Error(`${name} 必须使用本机隔离端口，不能使用共享栈`);
    }
  }
  return new URL(process.env.E2E_ADMIN_API_URL!).origin;
}

export const REVIEWER_CREDENTIALS_FILE = 'e2e/.e2e/state/reviewer-credentials.json';

// 「账号管理」真实界面准备合服复核人：不存在则新增（绑定 sys_admin），随后重置密码取回明文口令。
export async function ensureMergeReviewer(page: Page, account: string, realName: string): Promise<void> {
  await navigateByMenu(page, '账号管理', '系统');
  const table = page.locator('.p-datatable-tbody');
  await expect(table).toBeVisible({ timeout: 20_000 });

  const search = page.locator('label:has-text("账号")').locator('..').locator('input');
  await search.fill(account);
  await page.getByRole('button', { name: '查询', exact: true }).click();
  await page.waitForTimeout(600);
  const existingRow = table.locator('tr').filter({ hasText: account });
  if (await existingRow.count() === 0) {
    await page.getByRole('button', { name: '新增', exact: true }).click();
    const dialog = page.locator('.p-dialog:visible').first();
    await expect(dialog).toBeVisible();
    // BaseEditDialog 自带隐藏 id 输入，按占位符定位可见业务输入
    await dialog.getByPlaceholder('账号名称').fill(account);
    await dialog.getByPlaceholder('真实姓名').fill(realName);
    await dialog.getByPlaceholder('手机号码').fill('13800000002');
    // 生日（必填）：DatePicker 直接键入固定日期后失焦（对话框含出生/入职两个日期框，按占位符区分）
    const birthday = dialog.getByRole('combobox', { name: '出生日期' });
    await birthday.fill('1990-01-01');
    await birthday.press('Enter');
    // 角色集合：绑定种子引导出的 sys_admin（全部非超管菜单）
    await dialog.locator('.p-multiselect').click();
    await page.getByRole('option', { name: /sys_admin|系统管理员/ }).first().click();
    await page.keyboard.press('Escape');
    await dialog.getByRole('button', { name: '确定', exact: true }).click();
    await expect(dialog).toBeHidden({ timeout: 20_000 });
    await page.waitForTimeout(600);
  }

  // 重置密码 → toast 回读明文口令（产品路径；口令只落 state 目录，不进证据）
  await search.fill(account);
  await page.getByRole('button', { name: '查询', exact: true }).click();
  await page.waitForTimeout(600);
  const row = table.locator('tr').filter({ hasText: account }).first();
  await expect(row).toBeVisible();
  // 重置密码在行内「更多」弹出菜单（图标按钮无可访问名，是行内第二个按钮）；
  // PrimeVue popup 菜单带过渡动画，偶发开合竞态，允许再点一次按钮重开
  const menuItem = page.getByRole('menuitem', { name: '重置密码' });
  for (let attempt = 0; attempt < 2; attempt += 1) {
    await row.getByRole('button').nth(1).click();
    try {
      await menuItem.click({ timeout: 5_000 });
      break;
    } catch {
      if (attempt === 1) throw new Error('重置密码菜单项无法点击');
      await page.waitForTimeout(400);
    }
  }
  await page.getByRole('button', { name: '确定', exact: true }).click();
  const toast = page.locator('.p-toast-message-text');
  await expect(toast.first()).toBeVisible({ timeout: 15_000 });
  const toastText = (await toast.first().innerText()).trim();
  const match = toastText.match(/[:：]\s*(\S+)\s*$/);
  expect(match, `重置密码 toast 应包含明文口令：${toastText.slice(0, 40)}`).toBeTruthy();
  const password = match![1];

  // 口令可用性即刻验证：复核人真实表单登录（本页上下文先退出由调用方负责）
  await mkdir(dirname(REVIEWER_CREDENTIALS_FILE), { recursive: true });
  await writeFile(REVIEWER_CREDENTIALS_FILE, JSON.stringify({ account, password }, null, 2), 'utf-8');
}

// 合服用例读取复核人凭证：环境变量优先，回落到 setup 写入的 state 文件。
export async function readMergeReviewerCredentials(): Promise<{ account: string; password: string }> {
  const account = process.env.E2E_MERGE_REVIEWER_ACCOUNT;
  const password = process.env.E2E_MERGE_REVIEWER_PASSWORD;
  if (account && password) {
    return { account, password };
  }
  const raw = await readFile(REVIEWER_CREDENTIALS_FILE, 'utf-8');
  const parsed = JSON.parse(raw) as { account: string; password: string };
  expect(parsed.account, '复核人凭证文件缺少账号').toBeTruthy();
  expect(parsed.password, '复核人凭证文件缺少口令').toBeTruthy();
  return parsed;
}

