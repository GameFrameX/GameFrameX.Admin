import { test, expect } from '@playwright/test';
import { shot, superadminPage } from './helpers';

// AGENTS 产品规则的回归：`全局配置` 是 SuperAdmin-only，
// 租户管理员的菜单树/侧边栏不得出现该组（租户边界，P1 清单遗留项）。
test.describe('全局配置租户边界', () => {
  test('SuperAdmin 可见全局配置菜单', async ({ browser }) => {
    const page = await superadminPage(browser);
    await page.goto('/');
    await expect(page.locator('role=button[name="全局配置"] >> nth=0')).toBeVisible();
    await shot(page, 'boundary-superadmin-global-config');
    await page.close();
  });

  test('租户管理员不可见全局配置菜单', async ({ page }) => {
    await page.goto('/');
    // 先等通用菜单组渲染完成（菜单树异步加载），再断言全局配置缺席
    await expect(page.locator('role=button[name="系统管理"] >> nth=0')).toBeVisible();
    const count = await page.locator('text=全局配置').count();
    expect(count).toBe(0);
    await shot(page, 'boundary-tenant-admin-no-global-config');
  });
});
