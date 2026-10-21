import { test, expect, type APIRequestContext } from '@playwright/test';

// —— Client.Api ↔ 后台 关联性验证（2026-10-21，C395 姊妹篇）——
// 验证链路：后台（Admin API，20802）写入的配置/密钥 → Client.Api（20808，真实容器）读出一致。
// 全部走真实双主机 + 真实租户库；写侧用 Admin API（与 UI 同一后端契约），读侧带应用凭证头
// （GameFrameX-App-Id/Secret = 后台「应用管理 → 重置密钥」管理的同一份应用密钥）+ X-Proto-Ver 协议头。
// 覆盖配对：①应用密钥轮换（重置 → 新密钥放行/旧密钥失效）②功能开关（写→读→删）
// ③区服/版本/远程配置（读写一致性）。

const ADMIN_API = process.env.E2E_ADMIN_API_URL ?? 'http://127.0.0.1:20802';
const CLIENT_API = process.env.E2E_CLIENT_API_URL ?? 'http://127.0.0.1:20808';

// —— 外部 JSON 的受检读取：不写内联 as 断言，全部经守卫取值 ——
function asRecord(value: unknown): Record<string, unknown> {
  return value && typeof value === 'object' && !Array.isArray(value) ? (value as Record<string, unknown>) : {};
}

function pick(source: unknown, path: string[]): unknown {
  let current: unknown = source;
  for (const key of path) {
    current = asRecord(current)[key];
  }
  return current;
}

function readNumber(source: unknown, path: string[]): number | null {
  const value = pick(source, path);
  return typeof value === 'number' ? value : null;
}

// Client.Api 信封 wire 为小写（code/message/data），Admin 信封 data 内字段为 PascalCase——双形态读取
function readEnvelopeCode(payload: unknown): number | null {
  return readNumber(payload, ['Code']) ?? readNumber(payload, ['code']);
}

function readString(source: unknown, path: string[]): string {
  const value = pick(source, path);
  return typeof value === 'string' ? value : '';
}

function readObjects(source: unknown, path: string[]): Record<string, unknown>[] {
  const value = pick(source, path);
  if (!Array.isArray(value)) return [];
  return value.map(asRecord).filter((row) => Object.keys(row).length > 0);
}

type AdminContext = { token: string; request: APIRequestContext; appId: number };

async function adminPost(ctx: AdminContext, path: string, body: unknown): Promise<Record<string, unknown>> {
  const res = await ctx.request.post(`${ADMIN_API}/api/${path}`, {
    headers: { Authorization: `Bearer ${ctx.token}` },
    data: body as Record<string, unknown>,
  });
  const envelope = asRecord(await res.json());
  const code = readNumber(envelope, ['code']);
  if (code !== 0) {
    throw new Error(`Admin POST ${path} code=${code} message=${readString(envelope, ['message'])}`);
  }
  return asRecord(envelope['data']);
}

async function loginAdmin(request: APIRequestContext): Promise<string> {
  const res = await request.post(`${ADMIN_API}/api/systemAuth/login`, {
    data: {
      account: process.env.E2E_ACCOUNT ?? 'admin',
      password: process.env.E2E_PASSWORD ?? '123456',
      tenantId: process.env.E2E_TENANT_ID ?? '1300000000001',
    },
  });
  const token = readString(asRecord(await res.json()), ['data', 'AccessToken']);
  expect(token, '登录令牌').not.toBe('');
  return token;
}

async function fetchDefaultApp(request: APIRequestContext, token: string): Promise<{ appId: number; packageName: string }> {
  const res = await request.post(`${ADMIN_API}/api/tenantGameAppEntity/page`, {
    headers: { Authorization: `Bearer ${token}` },
    data: { page: 1, pageSize: 10 },
  });
  const rows = readObjects(await res.json(), ['data', 'Items']);
  expect(rows.length, '租户下存在应用').toBeGreaterThan(0);
  // 取有分包的应用（孤儿应用无包，包级端点全部不可达）；优先 e2e fixture 应用
  let appId = 0;
  let packageName = '';
  for (const row of rows) {
    const candidateId = Number(readString(row, ['Id']));
    const pkgRes = await request.post(`${ADMIN_API}/api/tenantGameAppPackage/page`, {
      headers: { Authorization: `Bearer ${token}` },
      data: { appId: candidateId, page: 1, pageSize: 5 },
    });
    const pkgRows = readObjects(await pkgRes.json(), ['data', 'Items']);
    const candidatePackage =
      readString(pkgRows.find((p) => readString(p, ['PackageName']) === 'com.gameframex.e2e.correlation') ?? pkgRows[0] ?? {}, ['PackageName']) ||
      readString(pkgRows[0] ?? {}, ['Name']);
    if (candidatePackage) {
      appId = candidateId;
      packageName = candidatePackage;
      break;
    }
  }
  expect(appId, '存在带分包的应用（关联性 fixture 见 ensureFixturePackage）').toBeGreaterThan(0);
  return { appId, packageName };
}

// 重置应用密钥：明文仅在响应中一次性返回（后台「应用管理 → 重置密钥」同源行为）
async function rotateAppSecret(ctx: AdminContext): Promise<string> {
  const result = await adminPost(ctx, 'tenantGameAppEntity/resetSecret', { id: String(ctx.appId) });
  const secret = readString(result, ['AppSecret']) || readString(result, ['Secret']);
  expect(secret, '重置密钥必须一次性回发明文').not.toBe('');
  return secret;
}

async function clientCall(
  ctx: AdminContext,
  method: 'get' | 'post',
  path: string,
  secret: string,
  body?: unknown,
): Promise<{ status: number; payload: unknown }> {
  // Client.Api 完整请求契约：租户头（TenantResolutionMiddleware）+ 应用凭证头（AppResolutionMiddleware）+ 协议头
  const headers = {
    'GameFrameX-Tenant-Id': process.env.E2E_TENANT_ID ?? '1300000000001',
    'GameFrameX-App-Id': String(ctx.appId),
    'GameFrameX-App-Secret': secret,
    'X-Proto-Ver': 'v1',
  };
  const res =
    method === 'get'
      ? await ctx.request.get(`${CLIENT_API}/api/${path}`, { headers })
      : await ctx.request.post(`${CLIENT_API}/api/${path}`, { headers, data: body as Record<string, unknown> });
  return { status: res.status(), payload: await res.json().catch(() => null) };
}

function featureSwitchRequestBody(appId: number, packageName: string): Record<string, unknown> {
  return {
    language: 'zh-CN',
    packageName,
    platform: 'android',
    channel: 'official',
    appVersion: '1.0.0',
    appId,
  };
}

function readSwitchKeys(payload: unknown): string[] {
  const data = pick(payload, ['Data']) ?? pick(payload, ['data']);
  const switches = readObjects(data, ['Switches']).length > 0 ? readObjects(data, ['Switches']) : readObjects(data, ['Items']);
  return switches.map((row) => readString(row, ['SwitchKey'])).filter((key) => key !== '');
}

const FIXTURE_PACKAGE = 'com.gameframex.e2e.correlation';

// 分包是包级端点的前置（区服/版本/开关按包解析）；无包时自举持久 fixture 包（幂等：已存在则复用）
async function ensureFixturePackage(ctx: AdminContext, packageName: string): Promise<string> {
  if (packageName) return packageName;
  const channels = await adminPost(ctx, 'tenantChannelDefinition/selectString', { pageSize: 5 });
  const channelRows = readObjects(channels, []) === undefined ? [] : (Array.isArray(channels) ? channels.map(asRecord) : []);
  const channelId = channelRows.length > 0 ? Number(readString(channelRows[0], ['Id']) || readNumber(channelRows[0], ['Id']) || 0) : 0;
  const created = await adminPost(ctx, 'tenantGameAppPackage/add', {
    appId: ctx.appId,
    name: 'E2E关联分包',
    packageName: FIXTURE_PACKAGE,
    platformCode: 'Android',
    platformName: 'Android',
    channelId,
    packageVersion: '1.0.0',
    isEnabled: true,
  }).catch(async (error: unknown) => {
    const message = error instanceof Error ? error.message : '';
    if (message.includes('包名已存在')) {
      return asRecord({ Id: '' });
    }
    throw error;
  });
  return readString(created, ['PackageName']) || FIXTURE_PACKAGE;
}

async function setupContext(request: APIRequestContext): Promise<{ ctx: AdminContext; packageName: string; secret: string }> {
  const token = await loginAdmin(request);
  const { appId, packageName } = await fetchDefaultApp(request, token);
  const ctx: AdminContext = { token, request, appId };
  const resolvedPackage = await ensureFixturePackage(ctx, packageName);
  const secret = await rotateAppSecret(ctx);
  return { ctx, packageName: resolvedPackage, secret };
}

test.describe('Client.Api 与后台关联性', () => {
  test.beforeAll(async ({ request }) => {
    for (const [label, url] of [
      ['Admin', ADMIN_API],
      ['Client', CLIENT_API],
    ] as const) {
      const probe = await request.get(url, { failOnStatusCode: false }).catch(() => null);
      if (probe === null) {
        throw new Error(`${label} 主机不可达：${url}`);
      }
    }
  });

  test('应用密钥链：后台重置密钥 → Client.Api 新密钥放行、旧密钥失效', async ({ request }) => {
    const { ctx, secret: firstSecret } = await setupContext(request);
    // 新密钥可过 Client.Api 应用鉴权（PublicKeys 为最小只读端点）
    let call = await clientCall(ctx, 'get', 'GameLoginTicketKey/PublicKeys', firstSecret);
    expect(readEnvelopeCode(call.payload), '重置后的新密钥 Client.Api 应放行').toBe(0);

    // 后台再次重置 → 旧密钥必须立即失效（AppCache 按 UpdateTime 信号刷新，无 TTL 延迟）
    const secondSecret = await rotateAppSecret(ctx);
    call = await clientCall(ctx, 'get', 'GameLoginTicketKey/PublicKeys', firstSecret);
    expect(call.status, '轮换后旧密钥应被拒绝').toBe(401);

    // 新密钥恢复放行
    call = await clientCall(ctx, 'get', 'GameLoginTicketKey/PublicKeys', secondSecret);
    expect(readEnvelopeCode(call.payload), '轮换后的最新密钥应放行').toBe(0);
  });

  test('功能开关：后台新增开关 ↔ Client.Api GetInfo 下发，删除后消失', async ({ request }) => {
    const { ctx, packageName, secret } = await setupContext(request);
    const marker = `E2E_FS_${Date.now().toString(36)}`;

    const created = await adminPost(ctx, 'tenantGameAppFeatureSwitch/add', {
      appId: ctx.appId,
      switchKey: marker,
      displayName: marker,
      description: 'e2e correlation',
      audience: 'both',
      valueJson: '{"v":"corr"}',
      priority: 1,
      rolloutPercent: 100,
      isEnabled: true,
    });

    const call = await clientCall(ctx, 'post', 'GameFeatureSwitch/GetInfo', secret, featureSwitchRequestBody(ctx.appId, packageName));
    expect(readEnvelopeCode(call.payload), `开关查询 code（packageName=${packageName || '空'}）`).toBe(0);
    expect(readSwitchKeys(call.payload), 'Client.Api 必须下发后台刚创建的开关').toContain(marker);
    await adminPost(ctx, 'tenantGameAppFeatureSwitch/delete', {
      id: readString(created, ['Id']),
      rowVersion: readNumber(created, ['RowVersion']) ?? 0,
    });
    const after = await clientCall(ctx, 'post', 'GameFeatureSwitch/GetInfo', secret, featureSwitchRequestBody(ctx.appId, packageName));
    expect(readSwitchKeys(after.payload), '删除后台开关后 Client.Api 不应再下发').not.toContain(marker);
  });

  test('读一致性：区服/版本/远程配置在 Client.Api 与后台同源', async ({ request }) => {
    const { ctx, packageName, secret } = await setupContext(request);

    // 区服（写读关联）：Client.Api 区服按「分类 → 区服」两级下发——后台 seed 分类+区服 → Client.Api 可见 → 清理后消失
    {
      const classify = await adminPost(ctx, 'tenantGameAreaClassifyEntity/add', {
        appId: ctx.appId,
        name: `E2E分类${Date.now() % 1000}`,
        description: 'e2e correlation',
      });
      const area = await adminPost(ctx, 'tenantGameAreaEntity/add', {
        appId: ctx.appId,
        name: `E2E区服${Date.now() % 1000}`,
        description: 'e2e correlation',
        ClassifyId: Number(readString(classify, ['Id'])),  // wire 绑定 PascalCase；Id wire 是字符串
        httpManageUrl: 'https://area.example.test/manage',
        serverVersion: '1.0.0',
        serverType: 'official',
        language: 1,
        serverStatus: 1,
        capacity: 100,
        isEnabled: true,
      });
      const call = await clientCall(ctx, 'post', 'GameArea/GetAreaList', secret, {
        language: 'zh-CN',
        packageName,
        platform: 'android',
        appVersion: '1.0.0',
        channel: 'official',
      });
      expect(readEnvelopeCode(call.payload), '区服列表 code').toBe(0);
      const data = pick(call.payload, ['Data']) ?? pick(call.payload, ['data']);
      // 响应是分类数组，每分类下 GameAreaList 挂区服
      const classifyRows = Array.isArray(data) ? data.map(asRecord).filter((row) => Object.keys(row).length > 0) : [];
      const allAreas = classifyRows.flatMap((row) => readObjects(row, ['GameAreaList']));
      const clientIds = allAreas.map((row) => readString(row, ['Id'])).filter((id) => id !== '');
      expect(clientIds, 'Client.Api 区服列表应包含后台刚 seed 的区服').toContain(readString(area, ['Id']));
      await adminPost(ctx, 'tenantGameAreaEntity/delete', {
        id: readString(area, ['Id']),
        rowVersion: readNumber(area, ['RowVersion']) ?? 0,
      }).catch(() => undefined);
      await adminPost(ctx, 'tenantGameAreaClassifyEntity/delete', {
        id: readString(classify, ['Id']),
        rowVersion: readNumber(classify, ['RowVersion']) ?? 0,
      }).catch(() => undefined);
    }

    // 版本检查（写读关联）：后台 seed 应用版本行 → Client.Api 低版本号命中该行；删除后回 404
    if (packageName) {
      const versionMarker = `9.9.9.${Date.now() % 1000}`;
      // 精确匹配契约：OldVersion == 客户端当前版本(0.0.1)、Platform/Language/Channel/Package 与请求全等
      const createdVersion = await adminPost(ctx, 'tenantGameAppReleaseVersion/add', {
        appId: ctx.appId,
        appVersion: versionMarker,
        platform: 'android',
        channel: 'official',
        package: packageName,
        language: 'zh-CN',
        oldVersion: '0.0.1',
        downloadUrl: 'https://client.example.test/e2e.apk',
        announcement: '',
        description: 'e2e correlation version',
        isForce: false,
        isEnabled: true,
      });
      const call = await clientCall(ctx, 'post', 'GameAppVersion/GetInfo', secret, {
        language: 'zh-CN',
        packageName,
        platform: 'android',
        appVersion: '0.0.1',
        channel: 'official',
      });
      expect(readEnvelopeCode(call.payload), '版本检查 code（后台 seed 后应命中）').toBe(0);
      const versionData = pick(call.payload, ['Data']) ?? pick(call.payload, ['data']);
      const downloadUrl = readString(versionData, ['AppDownloadUrl']) || readString(versionData, ['appDownloadUrl']);
      expect(downloadUrl, '下发 URL 与后台写入一致').toContain('e2e.apk');
      await adminPost(ctx, 'tenantGameAppReleaseVersion/delete', {
        id: readString(createdVersion, ['Id']),
        rowVersion: readNumber(createdVersion, ['RowVersion']) ?? 0,
      });
      const after = await clientCall(ctx, 'post', 'GameAppVersion/GetInfo', secret, {
        language: 'zh-CN',
        packageName,
        platform: 'android',
        appVersion: '0.0.1',
        channel: 'official',
      });
      expect(readEnvelopeCode(after.payload), '删除后台版本行后 Client.Api 应回 404').toBe(404);
    }

    // 远程配置（写读关联）：后台 seed 配置模式 + 配置内容 → Client.Api 命中并带 Md5；删除后 404
    {
      const configKey = `e2e_corr_${Date.now() % 100000}`;
      const markerValue = Date.now().toString(36);
      const key = await adminPost(ctx, 'tenantGameAppRemoteConfigKey/add', {
        keyName: configKey,
        displayName: 'E2E Corr Key',
        valueType: 'string',
        description: 'e2e correlation',
      });
      // 远程配置单行：作用域五元组 + ModeContents（modeCode → 内容键值）与 ActiveModeCode 激活
      const modeCode = `e2e_mode_${Date.now() % 100000}`;
      const config = await adminPost(ctx, 'tenantGameAppRemoteConfig/add', {
        appId: ctx.appId,
        platform: 'android',
        package: packageName,
        language: 'zh-CN',
        channel: 'official',
        appVersion: '1.0.0',
        modeContents: JSON.stringify({ [modeCode]: { [configKey]: markerValue } }),
        activeModeCode: modeCode,
        content: JSON.stringify({ [configKey]: markerValue }),
        checkResourceUrl: '',
        checkAppVersionUrl: '',
        aotCodeList: '',
        description: 'e2e correlation',
        priority: 1,
        isEnabled: true,
      });
      const configCall = await clientCall(ctx, 'post', 'GameRemoteConfig/GetInfo', secret, {
        language: 'zh-CN',
        packageName,
        platform: 'android',
        appVersion: '1.0.0',
        channel: 'official',
      });
      expect(readEnvelopeCode(configCall.payload), '远程配置 code（后台 seed 后应命中）').toBe(0);
      const configData = pick(configCall.payload, ['Data']) ?? pick(configCall.payload, ['data']);
      const content = readString(configData, ['Content']) || readString(configData, ['content']);
      expect(content, '远程配置 Content 必须与后台写入的 ModeContents 一致').toContain(markerValue);
      await adminPost(ctx, 'tenantGameAppRemoteConfig/delete', { id: readString(config, ['Id']), rowVersion: readNumber(config, ['RowVersion']) ?? 0 }).catch(() => undefined);
      await adminPost(ctx, 'tenantGameAppRemoteConfigKey/delete', { id: readString(key, ['Id']), rowVersion: readNumber(key, ['RowVersion']) ?? 0 }).catch(() => undefined);
    }
  });
});
