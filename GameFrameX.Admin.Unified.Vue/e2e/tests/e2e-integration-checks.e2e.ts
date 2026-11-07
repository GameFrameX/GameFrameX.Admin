import { createHmac, createHash } from 'node:crypto';
import { expect, test, type Page } from '@playwright/test';
import { ADMIN_ROOTS, expectOverview, navigateByMenu, shot, signIn, logoutByMenu } from './helpers';
import { renderedMenuGroups } from './module-coverage';

//  / 集成轮专项断言（第 1 轮 a–g + 第 2 轮 h/i/j）。
// 全部走真实菜单 / 真实表单 / 浏览器内 fetch；每项独立用例并 shot() 留证。

const HUB_API = process.env.E2E_HUB_API_URL;
const CLIENT_API = process.env.E2E_CLIENT_API_URL;

// 第 2 轮 (i)/(j) 的分包夹具（evidence-integration-2 环境准备 SQL 预置，is_enabled=true）
const FIXTURE_PACKAGE_A = { id: 9200000000101, label: 'com.e2e.pkg.alpha' };
const FIXTURE_PACKAGE_B = { id: 9200000000102, label: 'com.e2e.pkg.beta' };

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
  { leaf: '广告回调验签', root: 'C 端交付', urlPart: /#\/clientDelivery\/advertisementCallbackConfigEntity/, api: '/api/advertisementCallbackConfig/page' },
  { leaf: '传输签名密钥', root: 'C 端交付', urlPart: /#\/clientDelivery\/appSignatureKeyEntity/, api: '/api/appSignatureKey/page' },
  { leaf: '渠道定义', root: 'C 端交付', urlPart: /#\/clientDelivery\/channelDefinition/, api: '/api/channelDefinition/page' },
  { leaf: 'Client API 策略', root: 'C 端交付', urlPart: /#\/clientDelivery\/clientApiPolicyEntity/, api: '/api/clientApiPolicy/page' },
  { leaf: '登录票据密钥', root: 'C 端交付', urlPart: /#\/clientDelivery\/gameLoginTicketKeyEntity/, api: '/api/gameLoginTicketKey/page' },
  { leaf: '会话密钥', root: 'C 端交付', urlPart: /#\/clientDelivery\/sessionKeyEntity/, api: '/api/sessionKey/page' },
  { leaf: '对象存储配置', root: '系统', urlPart: /#\/system\/objectStorageConfigEntity/, api: '/api/objectStorageConfig/page' },
] as const;
// 广告奖励账本为设计内隐藏页（种子 IsHide=true，与权益发货账本同款先例，侧边栏不露出）——URL 直达验证
const HIDDEN_RENAMED_PAGE = { urlPart: /#\/clientDelivery\/gameAdvertisementRewardEntity/, api: '/api/gameAdvertisementReward/page' };

test('c 改名页面经真实菜单请求改名接口并渲染表格', async ({ page }) => {
  // 所有路由拦截在导航前注册：SPA hash 导航与页面挂载时序不定，后注册可能漏掉首个请求
  const apiHits = new Map<string, boolean>();
  const register = (api: string) => {
    if (apiHits.has(api)) {
      return;
    }
    apiHits.set(api, false);
    void page.route(new RegExp(api.replace(/\//g, '\\/')), async route => {
      if (route.request().method() === 'POST') {
        apiHits.set(api, true);
      }
      await route.continue();
    });
  };
  for (const item of RENAMED_PAGES) {
    register(item.api);
  }
  register(HIDDEN_RENAMED_PAGE.api);

  await page.goto('/');
  await expectOverview(page);
  for (const item of RENAMED_PAGES) {
    try {
      await navigateByMenu(page, item.leaf, item.root);
    } catch {
      await page.waitForTimeout(1_000);
      await navigateByMenu(page, item.leaf, item.root);
    }
    await expect(page).toHaveURL(item.urlPart);
    await expect(page.locator('.p-datatable')).toBeVisible();
    await expect.poll(() => apiHits.get(item.api) === true, { timeout: 15_000 })
      .toBe(true);
    await shot(page, `c406-c-${item.leaf}`);
    await page.waitForTimeout(300);
  }

  // 隐藏页（广告奖励账本，设计内 IsHide）经 URL 直达：同样请求改名接口并渲染表格
  await page.goto('/#/clientDelivery/gameAdvertisementRewardEntity');
  await expect(page).toHaveURL(HIDDEN_RENAMED_PAGE.urlPart);
  await expect(page.locator('.p-datatable')).toBeVisible();
  await expect.poll(() => apiHits.get(HIDDEN_RENAMED_PAGE.api) === true, { timeout: 15_000 })
    .toBe(true);
  await shot(page, 'c406-c-广告奖励账本-隐藏页直达');
});

// —— (d) 策展页面巡查：记录的页面请求 body/query 不含旧作用域字段；区服请求带 serverId ——
const WALK_LEAVES: Array<{ leaf: string; root?: string }> = [
  { leaf: '运营首页', root: '总览' },
  { leaf: '游戏区服', root: '区服' },
  { leaf: '开服向导', root: '区服' },
  { leaf: '游戏公告', root: '内容运营' },
  { leaf: '活动/功能开关', root: 'C 端交付' },
  { leaf: '远程配置', root: 'C 端交付' },
  { leaf: '在线总览', root: '在线运维' },
  { leaf: '统一审计', root: '在线运维' },
  { leaf: '资产审计', root: '在线运维' },
  { leaf: '游戏用户', root: '玩家' },
  { leaf: '订单', root: '商业化' },
  { leaf: '支付配置', root: 'C 端交付' },
  ...RENAMED_PAGES.map(item => ({ leaf: item.leaf, root: item.root })),
  { leaf: '操作日志', root: '审计' },
];

test('d 全页面请求不含旧作用域字段且区服请求携带 serverId', async ({ page }) => {
  test.setTimeout(300_000);
  const captured: Array<{ url: string; body?: string }> = [];
  page.on('request', request => {
    if (/\/api\//.test(request.url())) {
      captured.push({ url: request.url(), body: request.postData() ?? undefined });
    }
  });

  await page.goto('/');
  await expectOverview(page);

  // 顶栏全局区服选择器真实点击（在线运维区服维度查询依赖已选区服）
  const serverSelector = page.locator('.server-selector').first();
  if (await serverSelector.count()) {
    await serverSelector.getByRole('combobox', { name: '选择区服', exact: true }).click();
    const areaOption = page.locator('.p-select-option', { hasText: 'E2E-区服一' }).first();
    await areaOption.waitFor({ state: 'visible', timeout: 10_000 });
    await areaOption.click();
    await page.waitForTimeout(1_000);
  }

  for (const item of WALK_LEAVES) {
    try {
      await navigateByMenu(page, item.leaf, item.root);
    } catch {
      await page.waitForTimeout(1_000);
      await navigateByMenu(page, item.leaf, item.root);
    }
    await page.waitForTimeout(300);
  }

  expect(captured.length, '应捕获到页面 API 请求').toBeGreaterThan(50);
  const offenders = captured.filter(record =>
    /tenantid|appid|gameappid/i.test(record.url)
    || (record.body !== undefined && /tenantid|appid|gameappid/i.test(record.body)));
  expect(offenders, `旧作用域字段不得出现在任何请求 URL/body：${JSON.stringify(offenders.slice(0, 5))}`).toHaveLength(0);

  // 区服作用域契约：Online 区服维度查询显式携带 serverId（不再有租户/应用字段）
  const scopedRequests = captured.filter(record => /"serverId"\s*:/.test(record.body ?? '') || /serverid=/i.test(record.url));
  test.info().annotations.push({ type: 'c406-d', description: `captured=${captured.length} serverScoped=${scopedRequests.length}` });
  await shot(page, 'c406-d-wire-hygiene');

  expect(scopedRequests.length, '区服作用域查询应携带 serverId').toBeGreaterThan(0);
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
  await context.grantPermissions(['clipboard-read', 'clipboard-write'], { origin: new URL(process.env.E2E_APP_URL ?? 'http://127.0.0.1:21140').origin });
  await page.goto('/');
  await expectOverview(page);
  await navigateByMenu(page, '支付配置', 'C 端交付');
  await expect(page).toHaveURL(/#\/clientDelivery\/gamePaymentConfigEntity/);
  await expect(page.locator('.p-datatable')).toBeVisible();

  // 新装库无支付配置行：经真实 UI 新增一条（渠道字段由 schema 默认值自动填充）
  await page.getByRole('button', { name: '新增' }).first().click();
  const paymentDialog = page.locator('.p-dialog:visible').first();
  await expect(paymentDialog).toBeVisible();
  await paymentDialog.getByRole('combobox').first().click();
  const channelOption = page.locator('.p-select-option:visible').first();
  await channelOption.waitFor({ state: 'visible', timeout: 8_000 });
  await channelOption.click();
  await page.waitForTimeout(800);
  // 渠道 schema 的必填字段（各渠道不同）逐个补齐，否则表单校验拦截提交
  const requiredInputs = paymentDialog.locator('input[type="text"]:visible');
  const requiredCount = await requiredInputs.count();
  for (let index = 0; index < requiredCount; index += 1) {
    const input = requiredInputs.nth(index);
    if (!(await input.inputValue())) {
      await input.fill('E2E_PAYMENT_FIXTURE');
    }
  }
  await page.waitForTimeout(300);
  await page.screenshot({ path: test.info().outputPath('c406-f-payment-dialog.png') });
  await page.locator('.p-dialog-footer button', { hasText: '确定' }).first().click();
  await expect(paymentDialog).toBeHidden({ timeout: 20_000 });
  await expect(page.locator('.p-datatable-tbody tr:not(:has-text("暂无数据"))').first()).toBeVisible({ timeout: 20_000 });

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
    const body = JSON.stringify({});
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
  await page.locator('.p-dialog input[placeholder="请输入责任场景编码"]').fill('c406-e2e');
  await page.locator('.p-dialog input[placeholder="请输入责任主体"]').fill('集成轮回归');
  await page.locator('.p-dialog textarea[placeholder="请输入使用目的"]').fill('C406 集成轮机器通道验证');
  await page.locator('.p-dialog .p-select').first().click();
  // 绑定用户选项文本为「账号(真实姓名)」；真实姓名随引导账号不同（新装为「初始业务管理员」），按账号前缀匹配
  await page.locator('.p-select-option', { hasText: `${process.env.E2E_ACCOUNT ?? 'admin'}(` }).first().click();
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
  const goodSig = hmacSignature(accessSecret, 'POST', '/api/FeatureSwitchSync/Sync', '', accessKey, ts, nonce, JSON.stringify({}));
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
  const sig2 = hmacSignature(accessSecret, 'POST', '/api/FeatureSwitchSync/Sync', '', accessKey, ts2, nonce2, JSON.stringify({}));
  const withLegacy = await hubSync(page, {
    Signature: `v2;${accessKey};${ts2};${nonce2};${sig2}`,
    'GameFrameX-Tenant-Id': '1300000000001',
    'GameFrameX-App-Id': '1081001',
    'GameFrameX-App-Secret': 'legacy-secret-not-read',
  });
  expect(withLegacy.status, '附带旧租户/应用头不得改变结果').toBe(accepted.status);
  expect(withLegacy.code, '附带旧租户/应用头不得改变业务码').toBe(accepted.code);
});

// —— (h) 新装首启后，引导出来的业务管理员第一次表单登录即成功，菜单九根 ——
test('h 引导业务管理员首启表单登录成功且九根齐全', async ({ browser }) => {
  const context = await browser.newContext({ storageState: { cookies: [], origins: [] } });
  const page = await context.newPage();
  // 账号由 Web.Api 首启种子引导创建（GAMEFRAMEX_ADMIN_INITIAL_ACCOUNT/_PASSWORD 与 E2E_* 同值），
  // 不经任何写库；第一次表单登录必须直接落地运营首页。
  await signIn(page, process.env.E2E_ACCOUNT ?? 'admin', process.env.E2E_PASSWORD ?? '123456');
  await expectOverview(page);
  expect((await renderedMenuGroups(page)).map(group => group.group)).toEqual([...ADMIN_ROOTS]);
  await shot(page, 'c406-h-bootstrap-admin-first-login-nine-roots');
  await context.close();
});

// 清理夹具分包上遗留的广告回调配置（历史失败运行的残留），保证唯一约束可重跑
async function deleteExistingAdConfigs(page: Page, packagePrefixes: string[]): Promise<void> {
  for (const prefix of packagePrefixes) {
    for (let guard = 0; guard < 5; guard += 1) {
      const row = page.locator('.p-datatable-tbody tr').filter({ hasText: prefix }).first();
      if (await row.count() === 0) {
        break;
      }
      await row.getByText('删除', { exact: true }).click();
      await page.locator('role=button[name="确定"]').first().click();
      await page.waitForTimeout(500);
    }
  }
}

// —— (i) 广告回调配置：真实界面选分包新增、编辑回显；回调地址只有 {packageId} 一种形态 ——
test('i 广告回调配置分包选择回显且回调地址单 packageId 形态', async ({ page, request }) => {
  if (!CLIENT_API) {
    throw new Error('缺少 E2E_CLIENT_API_URL（隔离 Client.Api 地址）');
  }
  const token = `GFX_E2E_ADCI_${Date.now().toString(36)}`;

  await page.goto('/');
  await expectOverview(page);
  await navigateByMenu(page, '广告回调验签', 'C 端交付');
  await expect(page.locator('.p-datatable-tbody')).toBeVisible({ timeout: 20_000 });
  // 前置清理：失败重跑可能留下夹具分包上的配置，(PackageId, Provider) 唯一会令新增被拒
  await deleteExistingAdConfigs(page, [FIXTURE_PACKAGE_A.label, FIXTURE_PACKAGE_B.label]);

  // 新增：真实界面选择分包（分包是配置身份的一部分）
  await page.locator('role=button[name="新增"]').first().click();
  const dialog = page.locator('.p-dialog');
  await expect(dialog).toBeVisible({ timeout: 10_000 });
  await expect(dialog.locator('.parameter-panel')).toBeVisible({ timeout: 10_000 });
  await dialog.locator('role=combobox >> nth=1').click();
  const optionA = page.locator('role=option', { hasText: FIXTURE_PACKAGE_A.label }).first();
  await optionA.waitFor({ state: 'visible', timeout: 8_000 });
  await optionA.click();
  const chosenLabel = (await dialog.locator('role=combobox >> nth=1').evaluate((el) => {
    return (el.closest('.p-select')?.querySelector('.p-select-label')?.textContent ?? '').trim();
  }));
  expect(chosenLabel, '分包选择器未回显所选标签').toContain(FIXTURE_PACKAGE_A.label);
  await dialog.locator('role=textbox >> nth=0').fill(`https://cb.example.test/${token}`);
  await dialog.locator('.p-password input').first().fill('E2eAdSecret-Alpha-001');
  await shot(page, 'c406-i-adconfig-package-selected');
  await dialog.locator('role=button[name="确定"]').first().click();
  await expect(page.locator('.p-datatable-tbody')).toContainText(token, { timeout: 20_000 });

  // 编辑回显：分包标签与新增时一致
  await page.locator(`tr:has-text("${token}") >> text=编辑`).first().click();
  await expect(dialog).toBeVisible({ timeout: 10_000 });
  await expect(dialog.locator('.parameter-panel')).toBeVisible({ timeout: 10_000 });
  const echoedLabel = (await dialog.locator('role=combobox >> nth=1').evaluate((el) => {
    return (el.closest('.p-select')?.querySelector('.p-select-label')?.textContent ?? '').trim();
  }));
  expect(echoedLabel, '编辑弹窗必须回显新增时所选分包').toContain(FIXTURE_PACKAGE_A.label);
  await shot(page, 'c406-i-adconfig-edit-echo-package');
  await dialog.locator('role=button[name="确定"]').first().click();
  await page.waitForTimeout(600);

  // 回调地址只有 {packageId} 一种形态：带分包段命中端点（非 404，验签拒绝为 601），
  // 缺分包段直接 404——不存在旧的多段路由形态。
  const withPackage = await request.get(`${CLIENT_API}/api/GameAdWebHook/TopOnNotify/${FIXTURE_PACKAGE_A.id}`);
  expect(withPackage.status(), '带 packageId 的回调地址必须命中端点').toBe(601);
  // Client.Api 协议头守卫先于路由执行：不带 X-Proto-Ver 的非回调路径先得 400，
  // 带上协议头才走到路由层验证「缺 packageId 段 → 404」。
  const withoutPackage = await request.get(`${CLIENT_API}/api/GameAdWebHook/TopOnNotify`, { headers: { 'X-Proto-Ver': 'cola/1.0.0' } });
  expect(withoutPackage.status(), '缺 packageId 的回调地址必须 404').toBe(404);
  await shot(page, 'c406-i-adconfig-callback-url-form');

  // 清理本轮配置（保持 (PackageId, Provider) 可被后续用例复用）
  await page.locator(`tr:has-text("${token}") >> text=删除`).first().click();
  await page.locator('role=button[name="确定"]').first().click();
  await expect(page.locator('.p-datatable-tbody')).not.toContainText(token, { timeout: 20_000 });
});

// —— (j) 接口级补充证据：同 provider 双分包不同密钥，跨包密钥被拒；缺 packageId 404 ——
function topOnSign(secret: string, transId: string): string {
  const raw = `trans_id=${transId}&placement_id=p-1&adsource_id=a-1&reward_amount=10&reward_name=coin&sec_key=${secret}`;
  return createHash('md5').update(raw, 'utf8').digest('hex');
}

function topOnCallbackUrl(packageId: number, secret: string, transId: string): string {
  const query = new URLSearchParams({
    trans_id: transId,
    placement_id: 'p-1',
    adsource_id: 'a-1',
    reward_amount: '10',
    reward_name: 'coin',
    user_id: '9200000000201',
    is_test: '0',
    sign: topOnSign(secret, transId),
  });
  return `${CLIENT_API}/api/GameAdWebHook/TopOnNotify/${packageId}?${query.toString()}`;
}

test('j 广告回调跨包密钥被拒且缺分包 404', async ({ page, request }) => {
  if (!CLIENT_API) {
    throw new Error('缺少 E2E_CLIENT_API_URL（隔离 Client.Api 地址）');
  }
  const tokenAlpha = `GFX_E2E_ADCJ_A_${Date.now().toString(36)}`;
  const tokenBeta = `GFX_E2E_ADCJ_B_${Date.now().toString(36)}`;
  const secretAlpha = 'E2eAdSecret-Alpha-101';
  const secretBeta = 'E2eAdSecret-Beta-202';

  await page.goto('/');
  await expectOverview(page);
  await navigateByMenu(page, '广告回调验签', 'C 端交付');
  await expect(page.locator('.p-datatable-tbody')).toBeVisible({ timeout: 20_000 });
  await deleteExistingAdConfigs(page, [FIXTURE_PACKAGE_A.label, FIXTURE_PACKAGE_B.label]);

  // 同一 provider（TopOn）下两个分包各配一条不同密钥的配置（真实界面）
  for (const [fixture, urlToken, secret] of [
    [FIXTURE_PACKAGE_A, tokenAlpha, secretAlpha],
    [FIXTURE_PACKAGE_B, tokenBeta, secretBeta],
  ] as const) {
    await page.locator('role=button[name="新增"]').first().click();
    const dialog = page.locator('.p-dialog');
    await expect(dialog).toBeVisible({ timeout: 10_000 });
    await expect(dialog.locator('.parameter-panel')).toBeVisible({ timeout: 10_000 });
    await dialog.locator('role=combobox >> nth=1').click();
    const option = page.locator('role=option', { hasText: fixture.label }).first();
    await option.waitFor({ state: 'visible', timeout: 8_000 });
    await option.click();
    await dialog.locator('role=textbox >> nth=0').fill(`https://cb.example.test/${urlToken}`);
    await dialog.locator('.p-password input').first().fill(secret);
    await dialog.locator('role=button[name="确定"]').first().click();
    await expect(page.locator('.p-datatable-tbody')).toContainText(urlToken, { timeout: 20_000 });
  }
  await shot(page, 'c406-j-two-package-configs');

  // 1. A 包回调用 B 包密钥签名 → 601 拒绝（配置按『分包 + provider』精确命中）
  const crossKey = await request.get(topOnCallbackUrl(FIXTURE_PACKAGE_A.id, secretBeta, 'TX-CROSS-KEY'));
  expect(crossKey.status(), '跨包密钥签名必须被拒绝（601）').toBe(601);

  // 2. A 包回调用 A 包自身密钥签名 → 非 601（签名被接受；业务后续校验失败属预期，无角色夹具）
  const ownKey = await request.get(topOnCallbackUrl(FIXTURE_PACKAGE_A.id, secretAlpha, 'TX-OWN-KEY'));
  expect(ownKey.status(), '本包密钥签名应通过验签（结果不应再是 601）').not.toBe(601);

  // 3. 缺 packageId → 404（路由必填，唯一 URL 形态）
  const missingPackage = await request.get(`${CLIENT_API}/api/GameAdWebHook/TopOnNotify?trans_id=tx`, { headers: { 'X-Proto-Ver': 'cola/1.0.0' } });
  expect(missingPackage.status(), '缺 packageId 必须返回 404').toBe(404);

  // 清理两条配置
  for (const urlToken of [tokenAlpha, tokenBeta]) {
    await page.locator(`tr:has-text("${urlToken}") >> text=删除`).first().click();
    await page.locator('role=button[name="确定"]').first().click();
    await expect(page.locator('.p-datatable-tbody')).not.toContainText(urlToken, { timeout: 20_000 });
  }
});
