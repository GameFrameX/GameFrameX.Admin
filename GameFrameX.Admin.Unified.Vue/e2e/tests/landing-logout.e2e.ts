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

// 回归：登录后顶栏头像必须真实加载（2026-10-21 修复）。
// 根因曾为：store 把 DB 相对对象键直接 '/'+path 渲染，请求打到前端 origin 被 SPA fallback
// 吃掉（200 返回 index.html）→ 裂图；现经 resolveBackendFileUrl 补全后端 origin。
// 有头像走后端静态文件；无头像回退 /favicon.ico——两者都必须 naturalWidth>0。
test('登录后顶栏头像真实加载', async ({ page }) => {
  await page.goto('/');
  await expect(page.locator('body')).toContainText('应用中心');

  const avatarImage = page.locator('.layout-navbars-breadcrumb-user-link-photo').first();
  await expect(avatarImage).toBeVisible();
  const loaded = await avatarImage.evaluate((img: HTMLImageElement) => img.naturalWidth > 0);
  expect(loaded, `avatar src=${await avatarImage.evaluate((img: HTMLImageElement) => img.src)}`).toBe(true);
  await shot(page, 'landing-avatar-loaded');
});

// 回归：个人中心电子签名上传后必须真实加载（2026-10-21 修复，与头像同族缺陷）。
// 签名 DB 字段同为后端相对对象键（Upload/Signature/xxx.png），历史上传后赋值/store 直出
// 相对路径 → 被前端 SPA fallback 吃成裂图；现统一走 resolveBackendFileUrl。
// 真实路径：用户菜单 → 个人中心 → FileUpload 选择 PNG → 真实 POST uploadSignature。
test('个人中心签名上传后真实加载', async ({ page }) => {
  await page.goto('/');
  await page.locator('button[aria-label*="租管"]').click();
  await page.locator('.p-menu-overlay').getByText('个人中心').click();
  await page.waitForURL(/#\/tenant\/system\/userCenter/);

  const uploadResponse = page.waitForResponse(
    (r) => /\/api\/tenantFile\/uploadSignature/.test(r.url()) && r.request().method() === 'POST',
    { timeout: 30_000 },
  );
  await page.locator('.sys-userCenter-container input[type="file"][accept=".png"]').setInputFiles('e2e/tests/fixtures/signature-source.png');
  await uploadResponse;
  await expect(page.locator('.image-signature img')).toBeVisible();

  const assertSignatureLoaded = async (label: string) => {
    const signatureImage = page.locator('.image-signature img').first();
    const loaded = await signatureImage.evaluate((img: HTMLImageElement) => img.naturalWidth > 0);
    expect(loaded, `${label} signature src=${await signatureImage.evaluate((img: HTMLImageElement) => img.src)}`).toBe(true);
  };
  await assertSignatureLoaded('after-upload');
  await shot(page, 'usercenter-signature-after-upload');

  // 刷新 = 登录 hydrate 路径（store 从 /api/systemAuth/userInfo 重新取签名）
  await page.reload();
  await page.waitForSelector('.image-signature img', { timeout: 30_000 });
  await page.waitForTimeout(1_000);
  await assertSignatureLoaded('after-reload');
  await shot(page, 'usercenter-signature-after-reload');
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
