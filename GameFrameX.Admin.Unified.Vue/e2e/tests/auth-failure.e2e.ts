import { test, expect } from '@playwright/test';
import { anonPage } from './helpers';

// 登录失败路径：错误密码 → 停留在登录页，登录按钮 loading 态清除（响应已被应用处理）
const ACCOUNT = process.env.E2E_ACCOUNT ?? 'admin';
const TENANT_ID = process.env.E2E_TENANT_ID ?? '1300000000001';
const WRONG_PASSWORD = process.env.E2E_WRONG_PASSWORD ?? 'wrong-password';

test('错误密码登录失败并停留在登录页', async ({ browser }) => {
  // 登录失败路径需要匿名上下文，不能带 admin 会话态
  const page = await anonPage(browser);
  await page.goto('/#/login');

  await page.locator('#login-account').fill(`${ACCOUNT}@${TENANT_ID}`);
  await page.locator('#login-password').fill(WRONG_PASSWORD);

  let loginPosted = false;
  await page.route(/\/api\/systemAuth\/login(?![A-Za-z])/, async (route) => {
    if (route.request().method() === 'POST') loginPosted = true;
    await route.continue();
  });

  await page.locator('.login-submit').click();

  await expect.poll(() => loginPosted).toBe(true);
  // loading 态（disabled）清除 = 登录响应已被处理（失败路径返回，不导航）
  await expect(page.locator('.login-submit')).toBeEnabled();
  // 仍在登录页且表单可用（未发生成功跳转）
  const url = page.url();
  if (!url.includes('/#/login')) {
    throw new Error(`错误密码不应登录成功，当前 URL: ${url}`);
  }
  await expect(page.locator('#login-account')).toBeVisible();

  await page.close();
});
