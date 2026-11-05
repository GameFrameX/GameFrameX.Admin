import { test, expect } from '@playwright/test';
import { expectOverview, SUPERADMIN_LEAVES, superadminPage } from '../helpers';
import { expectNoApiAnomaly, installApiRecorder, sweepGroup } from '../module-coverage';
import { selectGlobalServer } from '../server-selection';

const ADMIN_MODULES = [
  { group: '总览', minLeaves: 1, minRequests: 1, timeout: 120_000 },
  { group: '区服', minLeaves: 3, minRequests: 3, timeout: 240_000 },
  { group: '内容运营', minLeaves: 8, minRequests: 6, timeout: 420_000 },
  { group: '在线运维', minLeaves: 13, minRequests: 10, timeout: 900_000 },
  { group: '玩家', minLeaves: 7, minRequests: 7, timeout: 480_000 },
  { group: '商业化', minLeaves: 9, minRequests: 9, timeout: 600_000 },
  { group: 'C 端交付', minLeaves: 18, minRequests: 12, timeout: 900_000 },
  { group: '审计', minLeaves: 4, minRequests: 4, timeout: 240_000 },
  { group: '系统', minLeaves: 5, minRequests: 5, timeout: 480_000 },
] as const;

test.describe('单 shell 普通管理员模块覆盖', () => {
  for (const module of ADMIN_MODULES) {
    test(`${module.group} 全叶可达且交互无异常`, async ({ page }) => {
      test.setTimeout(module.timeout);
      await installApiRecorder(page);
      await page.goto('/');
      await expectOverview(page);
      if (module.group === '在线运维') await selectGlobalServer(page);
      const reports = await sweepGroup(page, module.group, { minLeaves: module.minLeaves, actions: true });
      expect(reports.every(report => !report.is404)).toBe(true);
      await expectNoApiAnomaly(page, module.minRequests);
    });
  }
});

test.describe('单 shell 超管模块覆盖', () => {
  for (const group of ['总览', '系统'] as const) {
    test(`${group} 全叶可达且交互无异常`, async ({ browser }) => {
      test.setTimeout(group === '系统' ? 900_000 : 120_000);
      const page = await superadminPage(browser);
      try {
        await installApiRecorder(page);
        await page.goto('/');
        await expectOverview(page);
        const reports = await sweepGroup(page, group, { minLeaves: group === '系统' ? 15 : 1, actions: true });
        expect(reports.every(report => !report.is404)).toBe(true);
        if (group === '系统') expect(reports.map(report => report.leaf).sort()).toEqual([...SUPERADMIN_LEAVES].sort());
        await expectNoApiAnomaly(page, group === '系统' ? 13 : 1);
      } finally {
        await page.context().close();
      }
    });
  }
});
