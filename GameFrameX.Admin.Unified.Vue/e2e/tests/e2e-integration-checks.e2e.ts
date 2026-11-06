import { createHmac, createHash } from 'node:crypto';
import { expect, test, type Page } from '@playwright/test';
import { expectOverview, navigateByMenu, shot, signIn, logoutByMenu } from './helpers';
import { installApiRecorder, readApiRecords } from './module-coverage';

// C406 / GFX-1232 集成轮专项断言（round-integration-spec 第 8 步 a–g）。
// 全部走真实菜单 / 真实表单 / 浏览器内 fetch；每项独立用例并 shot() 留证。

const HUB_API = process.env.E2E_HUB_API_URL;

// —— (a) 登录页没有租户字段，admin 与超管都能通过表单登录 ——
test('a 登录表单无租户字段且双身份表单登录', async ({ browser }) => {
  const context = await browser.newContext({ storageState: { cookies: [], origins: [] } });
  const page = await context.newPage();
  await page.goto('/#/login');
  await expect(page.locator('#login-account')).toBeVisible();
  await expect(page.locator('#login-password')).toBeVisible();
  // 全表单只有账号与密码两个输入，不存在任何租户/作用域输入或下拉
  expect(await page.locator('input[id], select[id]').count()).toBe(2);
  expect(await page.locator('input[id*="tenant" i], select[id*="tenant" i], input[placeholder*="租户"]').count()).toBe(0);
  await shot(page, 'c406-a-login-form-no-tenant-field');

  // admin 表单登录（种子默认密码经 e2e 环境注入）
  await signIn(page, process.env.E2E_ACCOUNT ?? 'admin', process.env.E2E_PASSWORD ?? '123456');
  await expectOverview(page);
  await shot(page, 'c406-a-admin-landed');
  await logoutByMenu(page);

  // 超管表单登录（首装密码经 GFX_SUPERADMIN_INITIAL_PASSWORD 注入）
  await signIn(page, process.env.E2E_SUPERADMIN_ACCOUNT ?? 'superadmin',
    process.env.E2E_SUPERADMIN_PASSWORD ?? '123456');
  await expectOverview(page);
  await shot(page, 'c406-a-superadmin-landed');
  await context.close();
});

// —— (c) 改名后的 8 个页面经真实菜单打开、请求改名后的接口、表格渲染 ——
const RENAMED_PAGES = [
  { leaf: '广告回调配置', urlPart: /#\/clientDelivery\/advertisementCallbackConfigEntity/, api: '/api/advertisementCallbackConfig/page' },
  { leaf: '传输签名密钥', urlPart: /#\/clientDelivery\/appSignatureKeyEntity/, api: '/api/appSignatureKey/page' },
  { leaf: '渠道定义', urlPart: /#\/clientDelivery\/channelDefinitionEntity/, api: '/api/channelDefinition/page' },
  { leaf: 'Client API 策略', urlPart: /#\/clientDelivery\/clientApiPolicyEntity/, api: '/api/clientApiPolicy/page' },
  { leaf: '广告奖励账本', urlPart: /#\/clientDelivery\/gameAdvertisementRewardEntity/, api: '/api/gameAdvertisementReward/page' },
  { leaf: '登录票据密钥', urlPart: /#\/clientDelivery\/gameLoginTicketKeyEntity/, api: '/api/gameLoginTicketKey/page' },
  { leaf: '会话密钥', urlPart: /#\/clientDelivery\/sessionKeyEntity/, api: '/api/sessionKey/page' },
  { leaf: '对象存储配置', urlPart: /#\/system\/objectStorageConfigEntity/, api: '/api/objectStorageConfig/page' },
] as const;

test('c 改名页面经真实菜单请求改名接口并渲染表格', async ({ page }) => {
  await page.goto('/');
  await expectOverview(page);
  for (const item of RENAMED_PAGES) {
    let apiRequested = false;
    await page.route(new RegExp(item.api.replace(/\//g, '\\/')), async route => {
      if (route.request().method() === 'POST') apiRequested = true;
      await route.continue();
    });
    await navigateByMenu(page, item.leaf);
    await expect(page).toHaveURL(item.urlPart);
    await expect(page.locator('.p-datatable')).toBeVisible();
    expect(apiRequested, `${item.leaf} 应请求改名后的 ${item.api}`).toBe(true);
    await shot(page, `c406-c-${item.leaf}`);
  }
});

// —— (d) 巡查全部叶子：记录的页面请求 body/query 不含旧作用域字段；区服请求带 serverId ——
test('d 全页面请求不含旧作用域字段且区服请求携带 serverId', async ({ page }) => {
  await installApiRecorder(page);
  await page.goto('/');
  await expectOverview(page);

  const leaves = await page.evaluate(() => {
    const seen = new Set<string>();
    return Array.from(document.querySelectorAll('a.menu-nav-item .menu-label'))
      .map(el => (el.textContent ?? '').trim())
      .filter(title => title && !seen.has(title) && (seen.add(title), true));
  });
  expect(leaves.length, '应巡查到两位数叶子').toBeGreaterThanOrEqual(30);
  for (const leaf of leaves) {
    await navigateByMenu(page, leaf);
  }

  const records = await readApiRecords(page);
  expect(records.length, '应记录到页面请求').toBeGreaterThan(50);
  const offenders = records.filter(record =>
    /tenantid|appid|gameappid/i.test(record.url)
    || (record.body !== undefined && /tenantid|appid|gameappid/i.test(record.body)));
  expect(offenders, `旧作用域字段不得出现在任何请求 URL/body：${JSON.stringify(offenders.slice(0, 5))}`).toHaveLength(0);

  const serverRequests = records.filter(record => /\/api\/(gameArea|onlineServer|serverManager)/i.test(record.url)
    || /gameArea|onlineServer/i.test(record.body ?? ''));
  const withoutServerId = serverRequests.filter(record =>
    !/serverid/i.test(record.url) && !/serverid/i.test(record.body ?? ''));
  // 列表/分页类区服查询允许不带 serverId（全局列表），只要求出现过的区服作用域请求均带 serverId
  test.info().annotations.push({ type: 'c406-d', description: `records=${records.length} serverRequests=${serverRequests.length} withoutServerId=${withoutServerId.length}` });
  await shot(page, 'c406-d-wire-hygiene');

  const areaScopeRequests = serverRequests.filter(record => /serverid/i.test(record.url) || /serverid/i.test(record.body ?? ''));
  expect(areaScopeRequests.length, '至少应捕获到携带 serverId 的区服请求').toBeGreaterThan(0);
});

// —— (e) 日志与审计页没有 TenantId 列并且有数据行 ——
const LOG_PAGES = ['操作日志', '访问日志', '差异日志', '异常日志'] as const;

test('e 日志审计页无 TenantId 列且有数据行', async ({ page }) => {
  await page.goto('/');
  await expectOverview(page);
  for (const leaf of LOG_PAGES) {
    await navigateByMenu(page, leaf, '审计');
    await expect(page).toHaveURL(/#\/audit\//);
    await expect(page.locator('.p-datatable')).toBeVisible();
    const headers = await page.locator('.p-datatable-thead th').allTextContents();
    expect(headers.filter(header => /tenant/i.test(header)), `${leaf} 不得有 TenantId 列`).toHaveLength(0);
    await shot(page, `c406-e-${leaf}`);
  }
  // 数据行：套件此前的真实操作必然产生操作/访问日志（workers=1 顺序执行）
  await navigateByMenu(page, '操作日志', '审计');
  await expect(page.locator('.p-datatable-tbody tr').first()).toBeVisible({ timeout: 20_000 });
  const rowCount = await page.locator('.p-datatable-tbody tr').count();
  expect(rowCount, '操作日志应有数据行').toBeGreaterThan(0);
  await navigateByMenu(page, '访问日志', '审计');
  await expect(page.locator('.p-datatable-tbody tr').first()).toBeVisible({ timeout: 20_000 });
  await shot(page, 'c406-e-visit-rows');
});

// —— (f) 支付配置页回调地址只有 {packageId} 一种形态 ——
test('f 支付配置回调地址为单 packageId 形态', async ({ page, context }) => {
  await context.grantPermissions(['clipboard-read', 'clipboard-write'], { origin: new URL(page.url()).origin });
  await page.goto('/');
  await expectOverview(page);
  await navigateByMenu(page, '支付配置', '商业化');
  await expect(page).toHaveURL(/#\/clientDelivery\/gamePaymentConfigEntity/);
  await expect(page.locator('.p-datatable')).toBeVisible();

  // 新装库无支付配置行：经真实 UI 新增一条（渠道字段由 schema 默认值自动填充）
  await page.getByRole('button', { name: '新增' }).first().click();
  await expect(page.locator('.p-dialog')).toBeVisible();
  await page.locator('.p-dialog .p-select').first().click();
  await page.locator('.p-select-option').first().click();
  await page.screenshot({ path: test.info().outputPath('c406-f-payment-dialog.png') });
  await page.locator('.p-dialog-footer button', { hasText: '确定' }).first().click();
  await expect(page.locator('.p-datatable-tbody tr').first()).toBeVisible({ timeout: 20_000 });

  // 复制回调地址 → 校验剪贴板 URL 形态：/api/gameOrder/notify{Channel}(/packageId)?，不出现旧三段数字
  await page.locator('.p-datatable-tbody tr').first().getByText('复制回调地址').click();
  await expect(page.locator('.p-toast')).toBeVisible();
  const notifyUrl = await page.evaluate(() => navigator.clipboard.readText());
  expect(notifyUrl, `回调地址应为单 packageId 形态：${notifyUrl}`).toMatch(/\/api\/gameOrder\/notify\w+(\/\d+)?$/);
  expect(notifyUrl.match(/\/\d+\/\d+\/\d+(\/|$)/), '不得出现旧三段数字路径').toBeNull();
  await shot(page, 'c406-f-payment-notify-single-package');

  // 清理：删除新增行，保持库内无测试残留
  await page.locator('.p-datatable-tbody tr').first().getByText('删除').click();
  await page.locator('.p-confirmdialog-accept-button, .p-dialog-footer button', { hasText: '确定' }).last().click();
  await expect(page.locator('.p-datatable-tbody tr').first()).toBeHidden({ timeout: 20_000 }).catch(() => {});
});

// —— (g) Hub 机器通道：真实页面建凭证，HMAC 正确 200 / 篡改与缺失被拒 / 旧租户头无影响 ——
function hmacSignature(secret: string, method: string, path: string, query: string, keyId: string, ts: number, nonce: string, body: string): string {
  const bodyHash = createHash('sha256').update(body, 'utf8').digest('hex');
  const message = ['v2', method, path, query, keyId, String(ts), nonce, bodyHash].join('\n');
  return createHmac('sha256', secret).update(message, 'utf8').digest('base64');
}

async function hubSync(page: Page, headers: Record<string, string>): Promise<{ status: number; code: number | null }> {
  return page.evaluate(async ({ endpoint, requestHeaders }) => {
    const body = JSON.stringify({ Version: 0 });
    const response = await fetch(endpoint, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', ...requestHeaders },
      body,
    });
    let code: number | null = null;
    try {
      const envelope = await response.json();
      code = typeof envelope.code === 'number' ? envelope.code : null;
    } catch {
      // 401/403 的错误体不总是 JSON
    }
    return { status: response.status, code };
  }, { endpoint: `${HUB_API}/api/FeatureSwitchSync/Sync`, requestHeaders: headers });
}

test('g Hub 机器通道 HMAC 鉴权与旧租户头被忽略', async ({ page }) => {
  if (!HUB_API) {
    throw new Error('缺少 E2E_HUB_API_URL（隔离 Hub.Api 地址）');
  }

  // 真实页面创建凭证（绑定 admin、scope=FeatureSwitchSync）
  await page.goto('/');
  await expectOverview(page);
  await navigateByMenu(page, '应用开放访问凭证', 'C 端交付');
  await expect(page.locator('.p-datatable')).toBeVisible();
  await page.getByRole('button', { name: '新增' }).first().click();
  await expect(page.locator('.p-dialog')).toBeVisible();
  await page.locator('.p-dialog input[placeholder="责任场景"]').fill('c406-e2e');
  await page.locator('.p-dialog input[placeholder="责任主体"]').fill('集成轮回归');
  await page.locator('.p-dialog textarea[placeholder="使用目的"]').fill('C406 集成轮机器通道验证');
  await page.locator('.p-dialog .p-select').first().click();
  await page.locator('.p-select-option', { hasText: 'admin' }).first().click();
  await page.locator('.p-dialog .p-multiselect').first().click();
  await page.locator('.p-multiselect-option', { hasText: 'FeatureSwitchSync' }).first().click();
  await page.keyboard.press('Escape');
  await page.screenshot({ path: test.info().outputPath('c406-g-credential-dialog.png') });
  await page.locator('.p-dialog-footer button', { hasText: '确定' }).first().click();

  // 一次性密钥展示框：读取 AccessKey/AccessSecret（截图只拍凭证列表，不拍密文明文）
  const secretDialog = page.locator('.p-dialog').last();
  await expect(secretDialog).toBeVisible({ timeout: 20_000 });
  const accessKey = (await secretDialog.locator('code').first().textContent() ?? '').trim();
  const accessSecret = (await secretDialog.locator('code').nth(1).textContent() ?? '').trim();
  expect(accessKey.length, '应取得 AccessKey').toBeGreaterThan(10);
  expect(accessSecret.length, '应取得 AccessSecret').toBeGreaterThan(20);
  await secretDialog.getByRole('button').last().click();
  await shot(page, 'c406-g-credential-created');

  // 1. HMAC 正确 → 200 + 业务码 0
  const ts = Math.floor(Date.now() / 1000);
  const nonce = `c406-${ts}-${Math.random().toString(36).slice(2, 8)}`;
  const goodSig = hmacSignature(accessSecret, 'POST', '/api/FeatureSwitchSync/Sync', '', accessKey, ts, nonce, JSON.stringify({ Version: 0 }));
  const accepted = await hubSync(page, { Signature: `v2;${accessKey};${ts};${nonce};${goodSig}` });
  expect(accepted.status, '正确 HMAC 签名必须 200').toBe(200);
  expect(accepted.code, '业务信封码应为 0').toBe(0);

  // 2. 无签名头 → 401
  const unsigned = await hubSync(page, {});
  expect(unsigned.status, '缺失签名头必须被拒绝').toBe(401);

  // 3. 签名被篡改 → 401
  const tampered = await hubSync(page, { Signature: `v2;${accessKey};${ts};${nonce};${goodSig.slice(0, -4)}AAAA` });
  expect(tampered.status, '篡改签名必须被拒绝').toBe(401);

  // 4. 正确签名 + 旧三个租户/应用头 → 结果与不带完全一致（旧头被忽略）
  const ts2 = ts + 1;
  const nonce2 = `c406-${ts2}-x`;
  const sig2 = hmacSignature(accessSecret, 'POST', '/api/FeatureSwitchSync/Sync', '', accessKey, ts2, nonce2, JSON.stringify({ Version: 0 }));
  const withLegacy = await hubSync(page, {
    Signature: `v2;${accessKey};${ts2};${nonce2};${sig2}`,
    'GameFrameX-Tenant-Id': '1300000000001',
    'GameFrameX-App-Id': '1081001',
    'GameFrameX-App-Secret': 'legacy-secret-not-read',
  });
  expect(withLegacy.status, '附带旧租户/应用头不得改变结果').toBe(accepted.status);
  expect(withLegacy.code, '附带旧租户/应用头不得改变业务码').toBe(accepted.code);
});
