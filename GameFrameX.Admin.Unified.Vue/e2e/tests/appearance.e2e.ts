import { test, expect, type Page } from '@playwright/test';
import { anonPage, sleep } from './helpers';

// 外观功能回归：深色模式切换（html.dark 类翻转）+ 登录页语言切换（i18n 重渲染）。
test('顶栏深色模式开关翻转 html.dark', async ({ page }) => {
  await page.goto('/');
  await expect(page.locator('role=button[name="系统管理"] >> nth=0')).toBeVisible();

  const darkBefore = await readDarkClass(page);
  await page.locator('role=button[name="深色模式"]').click();
  const darkAfterOn = await pollDarkClass(page, !darkBefore);
  await page.locator('role=button[name="深色模式"]').click();
  const darkAfterOff = await pollDarkClass(page, darkBefore);

  expect(darkAfterOn).toBe(!darkBefore);
  expect(darkAfterOff).toBe(darkBefore);
});

test('登录页语言切换渲染英文表单', async ({ browser }) => {
  // 登录页路径：匿名上下文，不注入 admin 会话态
  const page = await anonPage(browser);
  await page.goto('/#/login');
  await expect(page.locator('#login-account')).toBeVisible();

  // 切 English（下拉选项为显示名）：表单标签变 Account/Password
  await page.locator('role=combobox >> nth=0').click();
  await page.locator('role=option[name="English"]').click();
  await expect(page.locator('role=textbox[name="Account"]')).toBeVisible({ timeout: 10_000 });

  // 还原简体中文，保持环境干净
  await page.locator('role=combobox >> nth=0').click();
  await page.locator('role=option[name="简体中文"]').click();
  await expect(page.locator('role=textbox[name="账号"]')).toBeVisible({ timeout: 10_000 });

  await page.close();
});

async function readDarkClass(page: Page): Promise<boolean> {
  return (await page.evaluate(() => document.documentElement.classList.contains('dark'))) === true;
}

async function pollDarkClass(page: Page, expected: boolean): Promise<boolean> {
  const deadline = Date.now() + 5000;
  while (Date.now() < deadline) {
    if ((await readDarkClass(page)) === expected) return expected;
    await sleep(250);
  }
  return await readDarkClass(page);
}
