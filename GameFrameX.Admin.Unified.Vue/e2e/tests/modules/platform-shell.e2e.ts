import { test, expect } from '@playwright/test';
import { renderedMenuGroups, expectNoApiAnomaly, installApiRecorder, sweepGroup } from '../module-coverage';
import { sleep, superadminPage } from '../helpers';

// SuperAdmin（最高权限）平台表面分模块覆盖：平台管理 / 全局配置 / 开发工具 / 帮助文档 / 工作台。
// 每个模块一个用例：组存在断言（防模块菜单静默丢失）→ 逐叶子真实点击导航 + 404 采样 +
// 搜索/重置/新增对话框交互探测 → 被动 API 记录断言无异常。覆盖清单写入用例 annotations。
// 取代原 superadmin-patrol 巨型单测（失败可按模块归因）。

test.describe('SuperAdmin 平台表面分模块覆盖', () => {
  test('模块清单与种子一致（五组）', async ({ browser }) => {
    const page = await superadminPage(browser);
    await page.goto('/');
    await page.locator('.p-panelmenu').waitFor({ timeout: 15_000 });
    const groups = (await renderedMenuGroups(page)).map((entry) => entry.group);
    expect(new Set(groups), '平台侧边栏根组').toEqual(
      new Set(['工作台', '平台管理', '全局配置', '开发工具', '帮助文档']),
    );
    await page.close();
  });

  for (const [groupName, minLeaves, minRequests, timeoutMs] of [
    ['平台管理', 11, 8, 420_000],
    ['全局配置', 9, 5, 300_000],
    ['开发工具', 1, 1, 120_000],
    ['帮助文档', 2, 0, 120_000],
    ['工作台', 1, 1, 120_000],
  ] as const) {
    test(`${groupName} 全叶可达且交互无异常`, async ({ browser }) => {
      test.setTimeout(timeoutMs);
      const page = await superadminPage(browser);
      await installApiRecorder(page);
      await page.goto('/');
      await page.locator('.p-panelmenu').waitFor({ timeout: 15_000 });

      const reports = await sweepGroup(page, groupName, { minLeaves, actions: true });
      expect(reports.some((report) => !report.is404), '模块内至少一个非 404 叶子').toBe(true);

      await expectNoApiAnomaly(page, minRequests);
      await sleep(200);
      await page.close();
    });
  }
});
