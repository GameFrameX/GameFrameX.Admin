import { test, expect } from '@playwright/test';
import { shot, navigateByMenu, expectOverview } from './helpers';

// 复刻 .local-archive/menu-test-20261020/verify-log-pages.js 的验证口径，但按 AGENTS 标准以真实菜单导航
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
        if (route.request().method() === 'POST') apiRequested = true;
        await route.continue();
      });

      await page.goto('/');
      await expectOverview(page);
      await navigateByMenu(page, c.menuItem, '审计');
      await expect(page).toHaveURL(/#\/audit\//);

      // 表体出现账号名 = 分页 API 响应已被应用处理并渲染（仅成功才有数据行）
      await expect(page.locator('.p-datatable-tbody')).toContainText('admin', { timeout: 20_000 });
      if (!apiRequested) {
        throw new Error(`${c.name}: 分页 API 未发出`);
      }

      await shot(page, `log-${c.name}`);
    });
  }
});

// R-20：验证真实菜单触发的浏览器下载；工作簿内容由集成轮 openpyxl 再验。
test('操作日志导出可下载 Excel 工作簿', async ({page}, testInfo) => {
  await page.goto('/');
  await expectOverview(page);
  await navigateByMenu(page, '操作日志', '审计');
  await expect(page.locator('.p-datatable-tbody')).toContainText('admin');
  const response = page.waitForResponse(value => value.url().includes('/api/logOperation/export'));
  const download = page.waitForEvent('download');
  await page.getByRole('button', {name: /导出/}).click();
  expect((await response).ok()).toBe(true);
  const workbook = await download;
  expect(await workbook.failure()).toBeNull();
  const file = testInfo.outputPath('operation-log-export.xlsx');
  await workbook.saveAs(file);
  await testInfo.attach('operation-log-export', {path: file, contentType: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'});
  await shot(page, 'operation-log-export-completed');
});
