import { test, expect } from '@playwright/test';
import { shot, expectOverview, userMenuButton, logoutByMenu, signIn, ADMIN_STATE } from './helpers';

// 单 shell 登录后直达总览首页，旧应用选择卡片/应用工作台入口退场。
test('登录落地页渲染总览首页', async ({ page }) => {
  await page.goto('/');
  await expectOverview(page);
  await expect(page.locator('body')).not.toContainText('返回应用列表');
  await shot(page, 'landing-overview');
});

// 回归：登录后顶栏头像必须真实加载（2026-10-21 修复）。
// 根因曾为：store 把 DB 相对对象键直接 '/'+path 渲染，请求打到前端 origin 被 SPA fallback
// 吃掉（200 返回 index.html）→ 裂图；现经 resolveBackendFileUrl 补全后端 origin。
// 有头像走后端静态文件；无头像回退 /favicon.ico——两者都必须 naturalWidth>0。
test('登录后顶栏头像真实加载', async ({ page }) => {
  await page.goto('/');
  await expectOverview(page);

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
  await userMenuButton(page).click();
  await page.locator('.p-menu-overlay').getByText('个人中心').click();
  await page.waitForURL(/#\/system\/userCenter/);

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
  await expectOverview(page);

  await logoutByMenu(page);

  try {
    await expect(page.locator('#login-account')).toBeVisible();
    await shot(page, 'logout-back-to-login');
  } finally {
    // 登出会使保存的 token 失效；恢复真实登录并更新状态文件，后续串行用例才能复用有效会话。
    await signIn(page, process.env.E2E_ACCOUNT ?? 'admin', process.env.E2E_PASSWORD ?? '123456');
    await page.context().storageState({ path: ADMIN_STATE });
  }
});
