import { test, expect } from '@playwright/test';
import { gotoAppSelector } from '../helpers';
import { expectNoApiAnomaly, installApiRecorder, sweepGroup } from '../module-coverage';

// 租户最高权限（admin）全局表面分模块覆盖：工作台 / 应用中心（全局部分）/ 系统管理 / 日志管理。
// 口径同 platform-shell：组存在断言 + 逐叶子真实点击导航 + 404 采样 + 搜索/重置/新增对话框探测 +
// 被动 API 无异常。取代原 menu-patrol 的全局表面半场与 crud-dialog-sweep 的「全局」半场。

test.describe('租户全局表面分模块覆盖', () => {
  for (const [groupName, minLeaves, minRequests, timeoutMs] of [
    ['工作台', 2, 1, 150_000],
    ['应用中心', 1, 1, 150_000],
    ['系统管理', 12, 8, 420_000],
    ['日志管理', 4, 4, 240_000],
  ] as const) {
    test(`${groupName} 全叶可达且交互无异常`, async ({ page }) => {
      test.setTimeout(timeoutMs);
      await installApiRecorder(page);
      await page.goto('/');
      await page.locator('.p-panelmenu').waitFor({ timeout: 15_000 });
      // 前序用例可能已把会话留在应用工作台；回到应用选择页拿全局侧边栏
      await gotoAppSelector(page);

      const reports = await sweepGroup(page, groupName, { minLeaves, actions: true });
      expect(reports.some((report) => !report.is404), '模块内至少一个非 404 叶子').toBe(true);

      await expectNoApiAnomaly(page, minRequests);
    });
  }
});
