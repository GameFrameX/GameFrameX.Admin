import { test, expect, type Locator } from '@playwright/test';
import { openHash, shot, sleep } from './helpers';

// 全页面 CRUD 骨架扫描（verify-tenant-admin-menu-contract 的回归化收敛，非破坏性）：
// 遍历租户管理员叶子菜单，凡提供可用「新增」按钮的页面——
// 打开新增对话框 → 对话框可见 → 取消关闭 → 对话框消失。
const API_BASE = process.env.E2E_API_BASE ?? 'http://127.0.0.1:20802';

type MenuLeaf = { path: string; title: string };

test('租户菜单页新增对话框全量可开可关', async ({ page }) => {
  await page.goto('/');
  const leaves = await page.evaluate(
    async (apiBase: string): Promise<MenuLeaf[]> => {
      const readFieldLocal = (source: object, keys: string[]): unknown => {
        for (const key of keys) {
          if (key in source) return Reflect.get(source, key);
        }
        return undefined;
      };
      const rawToken: unknown = JSON.parse(localStorage.getItem('gameframex.admin.vue:access-token') ?? '""');
      const token = typeof rawToken === 'string' ? rawToken : '';
      const res = await fetch(`${apiBase}/api/globalMenu/loginMenuTree`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const body: unknown = await res.json();
      const tree: unknown =
        body && typeof body === 'object' ? readFieldLocal(body, ['result', 'Result', 'data']) : [];
      const out: MenuLeaf[] = [];
      const walk = (nodes: unknown) => {
        if (!Array.isArray(nodes)) return;
        for (const node of nodes) {
          if (!node || typeof node !== 'object') continue;
          const children = readFieldLocal(node, ['children', 'Children']);
          const path = readFieldLocal(node, ['path', 'Path']);
          const meta = readFieldLocal(node, ['meta', 'Meta']);
          const title =
            meta && typeof meta === 'object' ? readFieldLocal(meta, ['title', 'Title']) : undefined;
          const childList = Array.isArray(children) ? children : [];
          if (childList.length === 0 && typeof path === 'string' && path) {
            out.push({ path, title: typeof title === 'string' ? title : path });
          }
          walk(childList);
        }
      };
      walk(tree);
      return out;
    },
    API_BASE,
  );
  expect(leaves.length).toBeGreaterThan(5);

  const issues: string[] = [];
  let dialogPages = 0;
  for (const menu of leaves) {
    await openHash(page, `/#${menu.path.startsWith('/') ? '' : '/'}${menu.path}`);
    // 等 SPA 挂载后再探测「新增」（isVisible 是单次读，不等元素）
    await sleep(350);

    const addButton = page.locator('role=button[name="新增"]').first();
    if (!(await addButton.isVisible()) || !(await addButton.isEnabled())) continue;
    await addButton.click();
    const dialog = page.locator('.p-dialog');
    if (!(await pollVisible(dialog, 2500))) {
      issues.push(`${menu.title} ${menu.path}: 新增未打开对话框`);
      continue;
    }
    dialogPages += 1;

    await page.locator('.p-dialog >> role=button[name="取消"]').click();
    if (await pollHidden(dialog, 2500)) continue;
    // 取消通道失效时的兜底：Escape 关闭，避免残留弹窗影响下一页
    await page.keyboard.press('Escape');
    await pollHidden(dialog, 1500);
    issues.push(`${menu.title} ${menu.path}: 取消未关闭对话框`);
  }

  // 全程遍历却一个对话框页都没有，说明扫描本身失灵
  expect(dialogPages).toBeGreaterThan(3);
  expect(issues, '新增对话框开/关异常的页面').toEqual([]);
  await shot(page, 'crud-dialog-sweep-done');
});

async function pollVisible(locator: Locator, timeoutMs: number): Promise<boolean> {
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    if (await locator.isVisible()) return true;
    await sleep(250);
  }
  return await locator.isVisible();
}

async function pollHidden(locator: Locator, timeoutMs: number): Promise<boolean> {
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    if (!(await locator.isVisible())) return true;
    await sleep(250);
  }
  return !(await locator.isVisible());
}
