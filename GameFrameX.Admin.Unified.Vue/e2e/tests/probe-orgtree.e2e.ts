import { test, expect } from '@playwright/test';
import { shot, sleep } from './helpers';

// 探针（临时）：组织页树表 DOM 定性——tbody class、行数、展开状态、子行是否渲染
test('探针：组织页树表 DOM', async ({ page }) => {
  await page.goto('/');
  // 等守卫弹回完成后再判定落地；appSelector 则点默认应用卡进工作台
  await sleep(900);
  let appId = '';
  if (page.url().includes('appSelector')) {
    const cardLabel = await page.locator('role=button[name*="默认应用"] >> nth=0').textContent();
    appId = /应用ID:\s*(\d+)/.exec(cardLabel ?? '')?.[1] ?? '';
    await page.locator('role=button[name*="默认应用"] >> nth=0').click();
    await page.waitForURL(/\/#\/(?!appCenter\/appSelector)/, { timeout: 20_000 });
  }
  await page.goto(`/#/tenant/system/org${appId ? `?appId=${appId}` : ''}`);
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
