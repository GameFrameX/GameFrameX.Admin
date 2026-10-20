import { test, expect } from '@playwright/test';
import { gotoAppSelector, navigateByMenu, shot, sleep } from './helpers';

// 探针（临时）：组织页树表 DOM 定性——tbody class、行数、展开状态、子行是否渲染。
// 2026-10-20 起真实点击化：经应用选择页全局侧边栏真实点击「组织管理」进入。
test('探针：组织页树表 DOM', async ({ page }) => {
  await page.goto('/');
  await page.locator('.p-panelmenu').waitFor({ timeout: 15_000 });
  await gotoAppSelector(page);
  await navigateByMenu(page, '组织管理');
  await expect(page.locator('.p-treetable, .p-datatable')).toBeVisible({ timeout: 20_000 });
  await sleep(1200);

  const info = await page.evaluate(() => {
    const tree = document.querySelector('.p-treetable');
    const tbody = document.querySelector('.p-treetable-tbody') ?? document.querySelector('.p-datatable-tbody');
    const rows = tbody ? Array.from(tbody.querySelectorAll('tr')) : [];
    return {
      hasTreeTable: !!tree,
      tbodyClass: tbody?.className ?? 'none',
      rowCount: rows.length,
      rowTexts: rows.slice(0, 6).map((r) => r.textContent?.trim().slice(0, 50) ?? ''),
    };
  });
  console.log('== 树表 DOM: ' + JSON.stringify(info));
  await shot(page, 'probe-orgtree');
});
