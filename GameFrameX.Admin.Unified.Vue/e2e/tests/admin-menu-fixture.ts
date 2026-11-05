import { expect, type Page, type Browser } from '@playwright/test';
import { anonPage, signIn, SUPERADMIN_STATE } from './helpers';

// 仅 setup 的授权准备可读管理菜单清单；导航/可见性断言始终读取真实侧边栏 DOM。
// 不用新的 API 登录挤掉已经建立的 superadmin 会话：完整表单登录后同步重写其 storageState。
// 两个 setup 串行运行，顺序任意，最后一次 superadmin 登录总是对应最新的状态文件。
export function readField(value: unknown, name: string): unknown {
  if (!value || typeof value !== 'object') return undefined;
  const record = value as Record<string, unknown>;
  return record[name] ?? record[name[0].toUpperCase() + name.slice(1)];
}

function rows(value: unknown): unknown[] {
  expect(Array.isArray(value)).toBe(true);
  return value as unknown[];
}

function identifier(value: unknown): string {
  expect(typeof value === 'string' || typeof value === 'number').toBe(true);
  const result = String(value);
  expect(result).toMatch(/^\d+$/);
  return result;
}

export function isolatedAdminOrigin(): string {
  for (const name of ['E2E_APP_URL', 'E2E_ADMIN_API_URL']) {
    const value = process.env[name];
    if (!value) throw new Error(`${name} 必须显式指向 C403 隔离栈`);
    const url = new URL(value);
    if (!['127.0.0.1', 'localhost', '[::1]'].includes(url.hostname)
      || !url.port || ['20800', '20802', '20806', '20808', '5432'].includes(url.port)) {
      throw new Error(`${name} 必须使用本机隔离端口，不能使用共享栈`);
    }
  }
  return new URL(process.env.E2E_ADMIN_API_URL!).origin;
}

export async function grantAdminMenus(browser: Browser, adminPage: Page, adminToken: string): Promise<void> {
  const origin = isolatedAdminOrigin();
  const rolesResponse = await adminPage.request.get(`${origin}/api/tenantRole/list`, {
    headers: { Authorization: `Bearer ${adminToken}` },
  });
  expect(rolesResponse.ok()).toBe(true);
  const rolesEnvelope: unknown = await rolesResponse.json();
  expect(readField(rolesEnvelope, 'code')).toBe(0);
  const roles = rows(readField(rolesEnvelope, 'data'));
  const configuredRole = process.env.E2E_ADMIN_ROLE_ID;
  const candidates = roles.filter(role => configuredRole
    ? identifier(readField(role, 'id')) === configuredRole
    : ['admin', 'adminrole'].includes(String(readField(role, 'code')).toLowerCase()));
  expect(candidates, '应有唯一 admin 角色；种子编码不同则设置 E2E_ADMIN_ROLE_ID').toHaveLength(1);
  const roleId = identifier(readField(candidates[0], 'id'));

  const bootstrapPage = await anonPage(browser);
  try {
    const token = await signIn(bootstrapPage, process.env.E2E_SUPERADMIN_ACCOUNT ?? 'superadmin',
      process.env.E2E_SUPERADMIN_PASSWORD ?? '123456');
    const headers = { Authorization: `Bearer ${token}` };
    const response = await bootstrapPage.request.get(`${origin}/api/globalMenu/list`, { headers });
    expect(response.ok()).toBe(true);
    const envelope: unknown = await response.json();
    expect(readField(envelope, 'code')).toBe(0);
    const menuIdentifiers: string[] = [];
    const rootKeys: string[] = [];
    const visit = (nodes: unknown[]): void => {
      for (const node of nodes) {
        // 保护整个超管子树，不能仅剔除叶子或丢掉普通 system 父根。
        expect(typeof readField(node, 'superAdminOnly')).toBe('boolean');
        if (readField(node, 'superAdminOnly') === true) continue;
        menuIdentifiers.push(identifier(readField(node, 'id')));
        if (String(readField(node, 'pid')) === '0') rootKeys.push(String(readField(node, 'key')));
        const children = readField(node, 'children');
        if (children != null) visit(rows(children));
      }
    };
    visit(rows(readField(envelope, 'data')));
    expect(rootKeys.sort()).toEqual([
      'route:dashboard', 'route:serverCenter', 'route:noticeCenter', 'route:gmCenter', 'route:player',
      'route:commerceCenter', 'route:clientDelivery', 'route:audit', 'route:system',
    ].sort());
    // 调服务清除权限缓存；不直接写 SQL，也不依赖菜单种子的自动授权。
    const grant = await bootstrapPage.request.post(`${origin}/api/tenantRole/grantMenu`, {
      headers, data: { id: roleId, menuIdList: [...new Set(menuIdentifiers)] },
    });
    expect(grant.ok()).toBe(true);
    expect(readField(await grant.json(), 'code')).toBe(0);
    await bootstrapPage.context().storageState({ path: SUPERADMIN_STATE });
  } finally {
    await bootstrapPage.context().close();
  }
}
