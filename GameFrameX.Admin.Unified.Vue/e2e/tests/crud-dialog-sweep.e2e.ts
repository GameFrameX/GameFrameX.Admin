import { test, expect, TimeoutError, type Locator } from '@playwright/test';
import { enterWorkspace, gotoAppSelector, navigateByMenu, renderedMenuLeaves, shot, sleep } from './helpers';

// 全页面 CRUD 骨架扫描（verify-tenant-admin-menu-contract 的回归化收敛，非破坏性；
// 2026-10-20 起全面真实点击化：叶子清单读自渲染后的侧边栏 DOM，逐个真实点击导航）：
// 遍历租户管理员两个真实表面（应用选择页全局侧边栏 + 应用工作台侧边栏）的叶子菜单，
// 凡提供可用「新增」按钮的页面——打开新增对话框 → 对话框可见 → 取消关闭 → 对话框消失。

test('租户菜单页新增对话框全量可开可关', async ({ page }) => {
  await page.goto('/');
  await page.locator('.p-panelmenu').waitFor({ timeout: 15_000 });
  // 前序用例可能已把会话留在应用工作台；回到应用选择页拿全局侧边栏
  await gotoAppSelector(page);

  const issues: string[] = [];
  let dialogPages = 0;

  const sweepSurface = async (surface: string, leaves: string[]): Promise<void> => {
    for (const title of leaves) {
      await navigateByMenu(page, title);
      // 等 SPA 挂载后再探测「新增」（isVisible 是单次读，不等元素）
      await sleep(350);

      const addButton = page.locator('role=button[name="新增"]').first();
      if (!(await addButton.isVisible()) || !(await addButton.isEnabled())) continue;
      await addButton.click();
      const dialog = page.locator('.p-dialog');
      if (!(await pollVisible(dialog, 2500))) {
        issues.push(`[${surface}] ${title}: 新增未打开对话框`);
        continue;
      }
      dialogPages += 1;

      // dev server 整页 reload（vite 依赖再优化等）会连同对话框一起卸载：点击超时但对话框已消失
      // 视为已关闭继续扫描；对话框仍在则走下方 Escape 兜底并记为异常
      try {
        await page.locator('.p-dialog >> role=button[name="取消"]').click();
      } catch (error) {
        if (!(error instanceof TimeoutError)) throw error;
      }
      if (await pollHidden(dialog, 2500)) continue;
      // 取消通道失效时的兜底：Escape 关闭，避免残留弹窗影响下一页
      await page.keyboard.press('Escape');
      await pollHidden(dialog, 1500);
      issues.push(`[${surface}] ${title}: 取消未关闭对话框`);
    }
  };

  // 表面一：应用选择页的全局侧边栏
  const globalLeaves = await renderedMenuLeaves(page);
  expect(globalLeaves.length, '全局侧边栏叶子数').toBeGreaterThan(5);
  await sweepSurface('全局', globalLeaves);

  // 表面二：真实点击默认应用卡进入工作台，应用域侧边栏
  await enterWorkspace(page);
  const appLeaves = await renderedMenuLeaves(page);
  expect(appLeaves.length, '工作台侧边栏叶子数').toBeGreaterThan(20);
  await sweepSurface('应用', appLeaves);

  // 全程遍历却一个对话框页都没有，说明扫描本身失灵
  expect(dialogPages).toBeGreaterThan(3);
  expect(issues, '新增对话框开/关异常的页面').toEqual([]);
  await shot(page, 'crud-dialog-sweep-done');
});

async function pollVisible(locator: Locator, timeoutMs: number): Promise<boolean> {
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    if (await locator.isVisible()) return true;
    await sleep(100);
  }
  return await locator.isVisible();
}

async function pollHidden(locator: Locator, timeoutMs: number): Promise<boolean> {
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    if (!(await locator.isVisible())) return true;
    await sleep(100);
  }
  return !(await locator.isVisible());
}
