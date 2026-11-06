import { test, expect } from '@playwright/test';
import { ADMIN_ROOTS, SUPERADMIN_ROOTS, SUPERADMIN_GROUPS, SUPERADMIN_LEAVES,
  expectOverview, menuRoot, shot, superadminPage } from './helpers';
import { renderedMenuGroups } from './module-coverage';

test.describe('单 shell 双向菜单边界', () => {
  test('普通管理员可见九根但系统不含超管组和超管叶子', async ({ page }) => {
    await page.goto('/');
    await expectOverview(page);
    expect((await renderedMenuGroups(page)).map(group => group.group)).toEqual([...ADMIN_ROOTS]);
    const system = menuRoot(page, '系统');
    await expect(system).toBeVisible();
    // 检查整个 DOM（包括折叠子树），不是只看当前展开状态。
    for (const title of [...SUPERADMIN_GROUPS, ...SUPERADMIN_LEAVES]) {
      await expect(page.locator('.p-panelmenu .menu-label').filter({ hasText: new RegExp(`^${title}$`) })).toHaveCount(0);
    }
    for (const title of ['账号管理', '角色管理', '文件管理', '定时任务']) {
      await expect(system.locator('a.menu-nav-item').filter({ has: page.getByText(title, { exact: true }) })).toHaveCount(1);
    }
    await shot(page, 'boundary-admin-system');
  });

  test('超管仅见总览系统且系统包含全部超管组和叶子', async ({ browser }) => {
    const page = await superadminPage(browser);
    try {
      await page.goto('/');
      await expectOverview(page);
      const groups = await renderedMenuGroups(page);
      expect(groups.map(group => group.group)).toEqual([...SUPERADMIN_ROOTS]);
      const system = menuRoot(page, '系统');
      await expect(system).toBeVisible();
      for (const title of SUPERADMIN_GROUPS) {
        await expect(system.locator('.menu-label').filter({ hasText: new RegExp(`^${title}$`) })).toHaveCount(1);
      }
      expect(groups.find(group => group.group === '系统')?.leaves.sort()).toEqual([...SUPERADMIN_LEAVES].sort());
      await shot(page, 'boundary-superadmin-system');
    } finally {
      await page.context().close();
    }
  });
});
