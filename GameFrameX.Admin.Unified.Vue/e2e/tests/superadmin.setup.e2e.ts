import { test, expect } from '@playwright/test';
import { SUPERADMIN_STATE } from './helpers';

// SuperAdmin 会话（run.js 同源的开发种子账号，无租户后缀）。
// 与 auth.setup 的差别：账号不带 @tenantId，落地后侧边栏应多出「全局配置」组。
const ACCOUNT = process.env.E2E_SUPERADMIN_ACCOUNT ?? 'superadmin';
const PASSWORD = process.env.E2E_SUPERADMIN_PASSWORD ?? '123456';
const LOGIN_URL = /\/api\/systemAuth\/login(?![A-Za-z])/;

test('sign in as superadmin', async ({ page }) => {
  await page.goto('/#/login');

  await page.locator('#login-account').fill(ACCOUNT);
  await page.locator('#login-password').fill(PASSWORD);

  let loginPosted = false;
  await page.route(LOGIN_URL, async (route) => {
    if (route.request().method() === 'POST') loginPosted = true;
    await route.continue();
  });

  await page.locator('.login-submit').click();

  await page.waitForURL(/\/#\/(?!login)/, { timeout: 20_000 });
  if (!loginPosted) {
    throw new Error('POST /api/systemAuth/login 未发出');
  }

  // SuperAdmin 是平台级菜单树（平台管理/全局配置/开发工具），无租户级"系统管理"组
  await expect(page.locator('body')).toContainText('全局配置');
  await page.context().storageState({ path: SUPERADMIN_STATE });
});
