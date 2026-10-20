import { test, expect } from '@playwright/test';
import { shot } from './helpers';

// 搜索过滤链路（menu-contract 脚本搜索维度的回归化收敛）：
// 操作日志页填入模糊搜索关键词 → 回车触发查询 →
// route 记录器实证分页请求真实发出且请求体携带关键词。
test('操作日志搜索关键词随查询请求发出', async ({ page }) => {
  let pageRequested = false;
  let requestBody = '';
  await page.route(/\/api\/logOperation\/page/, async (route) => {
    pageRequested = true;
    requestBody = route.request().postData() ?? '';
    await route.continue();
  });

  await page.goto('/');
  await page.locator('role=button[name="日志管理"] >> nth=0').click();
  await page.locator('text=操作日志').first().click();
  await expect(page.locator('.p-datatable-tbody')).toContainText('admin', { timeout: 20_000 });

  // 清空捕获（页面加载已触发过一次分页），再由回车触发带关键词的查询
  pageRequested = false;
  requestBody = '';

  // SearchForm 模糊搜索输入（placeholder=模糊搜索关键词），回车即查询
  const searchInput = page.locator('.search-form-control >> role=textbox >> nth=0');
  await searchInput.fill('admin');
  await page.keyboard.press('Enter');

  await expect.poll(() => pageRequested).toBe(true);
  if (!requestBody.toLowerCase().includes('admin')) {
    throw new Error(`查询请求体未携带关键词: ${requestBody}`);
  }
  await shot(page, 'log-search-filter');
});
