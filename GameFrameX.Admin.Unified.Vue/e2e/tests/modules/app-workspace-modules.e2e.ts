import { test, expect } from '@playwright/test';
import { enterWorkspace } from '../helpers';
import { expectNoApiAnomaly, installApiRecorder, sweepGroup } from '../module-coverage';

// 应用工作台表面分模块覆盖（租户最高权限 admin，菜单 ctx=appWorkspace）：
// 十个中心各一个用例，口径同 platform-shell / tenant-global-shell。
// 取代原 menu-patrol 的工作台半场与 crud-dialog-sweep 的「应用」半场。
// 数据级新增→回显→删除闭环仍在 crud-roundtrips.e2e.ts（矩阵推进中）。

const CENTERS = [
  // 组名取侧边栏渲染后的 i18n 文案（与菜单树 Title 既定漂移：信任与安全、在线业务中心）。
  // 权益发货账本 / 受控操作 在种子中 IsHide=true（页内入口，不出侧边栏），故按渲染叶子数计数。
  { group: '应用中心', minLeaves: 20, minRequests: 12, timeoutMs: 600_000 },
  { group: '玩家中心', minLeaves: 3, minRequests: 3, timeoutMs: 240_000 },
  { group: '信任与安全', minLeaves: 5, minRequests: 5, timeoutMs: 300_000 },
  { group: '商品与道具', minLeaves: 3, minRequests: 3, timeoutMs: 200_000 },
  { group: '区服运维', minLeaves: 3, minRequests: 3, timeoutMs: 200_000 },
  { group: '在线业务中心', minLeaves: 13, minRequests: 10, timeoutMs: 480_000 },
  { group: '兑换码', minLeaves: 3, minRequests: 3, timeoutMs: 200_000 },
  { group: 'Web 商城', minLeaves: 2, minRequests: 2, timeoutMs: 180_000 },
  { group: '任务与自动化', minLeaves: 1, minRequests: 1, timeoutMs: 150_000 },
  { group: '内容运营', minLeaves: 8, minRequests: 6, timeoutMs: 360_000 },
] as const;

test.describe('应用工作台分模块覆盖', () => {
  for (const center of CENTERS) {
    test(`${center.group} 全叶可达且交互无异常`, async ({ page }) => {
      test.setTimeout(center.timeoutMs);
      await installApiRecorder(page);
      await page.goto('/');
      await page.locator('.p-panelmenu').waitFor({ timeout: 15_000 });
      await enterWorkspace(page);

      const reports = await sweepGroup(page, center.group, { minLeaves: center.minLeaves });
      expect(reports.some((report) => !report.is404), '模块内至少一个非 404 叶子').toBe(true);

      await expectNoApiAnomaly(page, center.minRequests);
    });
  }
});
