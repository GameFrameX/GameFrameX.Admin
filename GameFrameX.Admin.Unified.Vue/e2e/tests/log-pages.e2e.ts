import { test, expect } from '@playwright/test';
import { shot } from './helpers';

// 复刻 .menu-test/verify-log-pages.js 的验证口径，但按 AGENTS 标准以真实菜单导航
// （动态菜单 hash 随版本漂移：system/log/oplog 与 log/oplog 均已 404），
// route 记录器证明分页 API 真实发出，表体断言渲染出 DB 行，截图留证。
const CASES = [
  {
    name: 'oplog',
    menuItem: '操作日志',
    api: /\/api\/logOperation\/page/,
  },
  {
    name: 'vislog',
    menuItem: '访问日志',
    api: /\/api\/logVisit\/page/,
  },
] as const;

test.describe('日志页面渲染', () => {
  for (const c of CASES) {
    test(`${c.name} 通过真实菜单导航渲染出数据行`, async ({ page }) => {
      let apiRequested = false;
      await page.route(c.api, async (route) => {
        apiRequested = true;
        await route.continue();
      });

      await page.goto('/');
      // 侧边栏“日志管理”组（语义树中外内两层同名 button，取第一个）
      await page.locator('role=button[name="日志管理"] >> nth=0').click();
      await page.locator(`text=${c.menuItem}`).first().click();

      // 表体出现账号名 = 分页 API 响应已被应用处理并渲染（仅成功才有数据行）
      await expect(page.locator('.p-datatable-tbody')).toContainText('admin', { timeout: 20_000 });
      if (!apiRequested) {
        throw new Error(`${c.name}: 分页 API 未发出`);
      }

      await shot(page, `log-${c.name}`);
    });
  }
});
