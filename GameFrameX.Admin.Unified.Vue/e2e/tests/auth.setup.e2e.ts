import { test, expect } from '@playwright/test';
import { ADMIN_STATE } from './helpers';

// 与 .menu-test/verify-log-pages.js 同源的开发种子账号（env 可覆盖）。
// 密码用普通字符串：种子账号无敏感面，且截图证据是验收要求。
const ACCOUNT = process.env.E2E_ACCOUNT ?? 'admin';
const TENANT_ID = process.env.E2E_TENANT_ID ?? '1300000000001';
const PASSWORD = process.env.E2E_PASSWORD ?? '123456';

// 前端实际调用 POST /api/systemAuth/login（src/api/systemAuth/index.ts）；
// (?![A-Za-z]) 排除页面加载即触发的 GET /api/systemAuth/loginConfig。
const LOGIN_URL = /\/api\/systemAuth\/login(?![A-Za-z])/;

test('sign in as tenant admin', async ({ page }) => {
  await page.goto('/#/login');

  const tenantInput = page.locator('#login-tenant-id');
  if (await tenantInput.count()) {
    await tenantInput.fill(TENANT_ID);
    await page.locator('#login-account').fill(ACCOUNT);
  } else {
    await page.locator('#login-account').fill(`${ACCOUNT}@${TENANT_ID}`);
  }
  await page.locator('#login-password').fill(PASSWORD);

  // 跨域（8888 → 20802）下 OPTIONS 预检与 POST 同 URL，waitForResponse 会误捕预检响应；
  // 改用 route 记录器证明真实 POST 发出，waitForURL 证明登录响应已被应用处理成功。
  let loginPosted = false;
  await page.route(LOGIN_URL, async (route) => {
    if (route.request().method() === 'POST') loginPosted = true;
    await route.continue();
  });

  // 提交按钮现类名 .login-submit（LoginAccount.vue）
  await page.locator('.login-submit').click();

  await page.waitForURL(/\/#\/(?!login)/, { timeout: 20_000 });
  if (!loginPosted) {
    throw new Error('POST /api/systemAuth/login 未发出');
  }

  // 登录后落地应用选择页；断言后端菜单树已渲染（登录态硬证据）
  await expect(page.locator('body')).toContainText('日志管理');

  // 会话态落盘供 admin 项目复用（token 在 localStorage，storageState 可完整携带）
  await page.context().storageState({ path: ADMIN_STATE });
});
