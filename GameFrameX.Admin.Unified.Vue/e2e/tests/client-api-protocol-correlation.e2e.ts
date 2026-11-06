import { expect, test, type Page } from '@playwright/test';
import { expectOverview, navigateByMenu, shot } from './helpers';

const CLIENT_API_URL = process.env.E2E_CLIENT_API_URL;
const CLIENT_TENANT_ID = process.env.E2E_CLIENT_TENANT_ID;
const CLIENT_APP_ID = process.env.E2E_CLIENT_APP_ID;
const CLIENT_APP_SECRET = process.env.E2E_CLIENT_APP_SECRET;
const CLIENT_PROTO_VERSION = process.env.E2E_CLIENT_PROTO_VERSION ?? 'cola/1.0.0';

function requireClientEnvironment(): { url: string; tenantId: string; appId: string; appSecret: string } {
  const missing = [
    ['E2E_CLIENT_API_URL', CLIENT_API_URL],
    ['E2E_CLIENT_TENANT_ID', CLIENT_TENANT_ID],
    ['E2E_CLIENT_APP_ID', CLIENT_APP_ID],
    ['E2E_CLIENT_APP_SECRET', CLIENT_APP_SECRET],
  ].filter(([, value]) => !value).map(([name]) => name);
  if (missing.length > 0) {
    throw new Error(`新协议 Client.Api e2e 缺少隔离栈变量：${missing.join(', ')}`);
  }

  const url = new URL(CLIENT_API_URL!);
  if (!['127.0.0.1', 'localhost', '[::1]'].includes(url.hostname) || !url.port
    || ['20800', '20802', '20806', '20808', '5432'].includes(url.port)) {
    throw new Error('E2E_CLIENT_API_URL 必须指向本机隔离 Client.Api 端口，不能使用共享栈端口');
  }
  return {
    url: url.origin,
    tenantId: CLIENT_TENANT_ID!,
    appId: CLIENT_APP_ID!,
    appSecret: CLIENT_APP_SECRET!,
  };
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
        DeviceId: 'c405-protocol-e2e',
      }),
    });
    let body: unknown = null;
    try {
      body = await response.json();
    } catch {
      // The status assertion below still reports protocol middleware failures clearly.
    }
    return { status: response.status, body };
  }, {
    endpoint: `${url}/api/GameRemoteConfig/GetInfo`,
    requestHeaders: headers,
  });
}

test('Client.Api 新协议鉴权头经真实 Admin 菜单关联到启动配置端点', async ({ page }) => {
  let policyPageRequested = false;
  await page.route(/\/api\/clientApiPolicy\/page/, async route => {
    if (route.request().method() === 'POST') policyPageRequested = true;
    await route.continue();
  });

  await page.goto('/');
  await expectOverview(page);
  await navigateByMenu(page, 'Client API 策略', 'C 端交付');
  await expect(page).toHaveURL(/#\/clientDelivery\/tenantClientApiPolicyEntity/);
  await expect(page.locator('.p-datatable')).toBeVisible();
  expect(policyPageRequested, '真实菜单页面应读取 Client.Api 策略').toBe(true);

  const client = requireClientEnvironment();
  const headers = {
    'GameFrameX-Tenant-Id': client.tenantId,
    'GameFrameX-App-Id': client.appId,
    'GameFrameX-App-Secret': client.appSecret,
    'X-Proto-Ver': CLIENT_PROTO_VERSION,
  };
  const accepted = await requestStartupEndpoint(page, client.url, headers);
  // 远程配置可能没有命中种子数据（404），但带完整新协议头不能被版本中间件以 400 拒绝。
  expect(accepted.status).not.toBe(400);
  expect(accepted.body).toBeTruthy();

  const missingProto = await requestStartupEndpoint(page, client.url, {
    'GameFrameX-Tenant-Id': client.tenantId,
    'GameFrameX-App-Id': client.appId,
    'GameFrameX-App-Secret': client.appSecret,
  });
  expect(missingProto.status, '普通 Client.Api 请求缺失 X-Proto-Ver 必须拒绝').toBe(400);
  await shot(page, 'client-api-protocol-correlation');
});
