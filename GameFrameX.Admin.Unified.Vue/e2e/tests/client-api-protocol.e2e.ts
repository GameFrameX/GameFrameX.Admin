import { expect, test, type Page } from '@playwright/test';
import { expectOverview, navigateByMenu, shot } from './helpers';

// C406 / GFX-1232：替代 C405 的 client-api-protocol-correlation 用例。
// 新单游戏协议只认 X-Proto-Ver；旧三头（GameFrameX-Tenant-Id / GameFrameX-App-Id / GameFrameX-App-Secret）
// 在生产代码已无读取方——本用例证明：附带旧头时结果与不带时完全一致（被忽略）。
const CLIENT_API_URL = process.env.E2E_CLIENT_API_URL;
const CLIENT_PROTO_VERSION = process.env.E2E_CLIENT_PROTO_VERSION ?? 'cola/1.0.0';

function requireClientEnvironment(): string {
  if (!CLIENT_API_URL) {
    throw new Error('新协议 Client.Api e2e 缺少隔离栈变量：E2E_CLIENT_API_URL');
  }
  const url = new URL(CLIENT_API_URL);
  if (!['127.0.0.1', 'localhost', '[::1]'].includes(url.hostname) || !url.port
    || ['20800', '20802', '20806', '20808', '5432'].includes(url.port)) {
    throw new Error('E2E_CLIENT_API_URL 必须指向本机隔离 Client.Api 端口，不能使用共享栈端口');
  }
  return url.origin;
}

async function requestStartupEndpoint(
  page: Page,
  url: string,
  headers: Record<string, string>,
): Promise<{ status: number; body: unknown }> {
  return page.evaluate(async ({ endpoint, requestHeaders }) => {
    const response = await fetch(endpoint, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', ...requestHeaders },
      body: JSON.stringify({
        Platform: 'ios',
        PackageName: 'com.gameframex.e2e',
        AppVersion: '0.0.0',
        Language: 'zh-CN',
        Channel: 'official',
        DeviceId: 'c406-protocol-e2e',
      }),
    });
    let body: unknown = null;
    try {
      body = await response.json();
    } catch {
      // 状态断言足以区分协议中间件的拒绝形态。
    }
    return { status: response.status, body };
  }, {
    endpoint: `${url}/api/GameRemoteConfig/GetInfo`,
    requestHeaders: headers,
  });
}

test('Client.Api 新协议 X-Proto-Ver 关联与旧作用域头被忽略', async ({ page }) => {
  let policyPageRequested = false;
  await page.route(/\/api\/clientApiPolicy\/page/, async route => {
    if (route.request().method() === 'POST') policyPageRequested = true;
    await route.continue();
  });

  await page.goto('/');
  await expectOverview(page);
  await navigateByMenu(page, 'Client API 策略', 'C 端交付');
  await expect(page).toHaveURL(/#\/clientDelivery\/clientApiPolicyEntity/);
  await expect(page.locator('.p-datatable')).toBeVisible();
  expect(policyPageRequested, '真实菜单页面应读取改名后的 Client.Api 策略接口').toBe(true);

  const client = requireClientEnvironment();

  // 1. 带 X-Proto-Ver：业务信封 HTTP 200，业务码 0（成功）或 404（配置未命中）。
  const accepted = await requestStartupEndpoint(page, client, { 'X-Proto-Ver': CLIENT_PROTO_VERSION });
  expect(accepted.status).toBe(200);
  const acceptedBody = accepted.body as { code?: number };
  expect(
    [0, 404],
    '带 X-Proto-Ver 的请求必须命中成功(0)或配置未命中(404)，不得是鉴权/服务器错误',
  ).toContain(acceptedBody?.code);

  // 2. 缺 X-Proto-Ver：协议中间件拒绝 400。
  const missingProto = await requestStartupEndpoint(page, client, {});
  expect(missingProto.status, '缺失 X-Proto-Ver 必须拒绝').toBe(400);

  // 3. 附带旧三头：结果与不带时完全一致（旧头被忽略，不参与解析、不改变结果）。
  const legacyHeaders: Record<string, string> = {
    'GameFrameX-Tenant-Id': '1300000000001',
    'GameFrameX-App-Id': '1081001',
    'GameFrameX-App-Secret': 'legacy-secret-not-read',
  };
  const withLegacy = await requestStartupEndpoint(page, client, legacyHeaders);
  expect(withLegacy.status, '附带旧作用域头不得改变拒绝结果').toBe(missingProto.status);
  const withLegacyAndProto = await requestStartupEndpoint(page, client, {
    'X-Proto-Ver': CLIENT_PROTO_VERSION,
    ...legacyHeaders,
  });
  expect(withLegacyAndProto.status).toBe(accepted.status);
  expect((withLegacyAndProto.body as { code?: number })?.code).toBe(acceptedBody?.code);

  await shot(page, 'client-api-protocol');
});
