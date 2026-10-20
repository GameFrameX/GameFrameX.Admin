import { test, expect } from '@playwright/test';
import { shot } from './helpers';

// 登录后落地页 + 登出闭环：应用选择页渲染真实数据（应用卡片），
// 顶栏用户菜单 → 退出登录 → 确认弹窗 → 回到登录页。
test('登录落地页渲染应用选择卡片', async ({ page }) => {
  await page.goto('/');

  // 应用中心 + 默认应用卡片（后端真实数据：应用名与配额）
  await expect(page.locator('body')).toContainText('应用中心');
  await expect(page.locator('body')).toContainText('默认应用');
  await expect(page.locator('body')).toContainText('应用ID:');
  await shot(page, 'landing-app-selector');
});

test('用户菜单退出登录回到登录页', async ({ page }) => {
  await page.goto('/');
  await expect(page.locator('body')).toContainText('应用中心');

  // 顶栏用户菜单触发器（aria-label 含租管），弹出 Menu 后点退出登录
  await page.locator('button[aria-label*="租管"]').click();
  await page.locator('text=退出登录').first().click();

  // showConfirm 弹窗 → 确定
  await page.locator('role=button[name="确定"]').click();

  // 登出清 token 后路由守卫应带回登录页
  await page.waitForURL(/\/#\/login/, { timeout: 20_000 });
  await expect(page.locator('#login-account')).toBeVisible();
  await shot(page, 'logout-back-to-login');
});
