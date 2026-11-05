import { test, expect } from '@playwright/test';
import { ADMIN_STATE, anonPage, navigateByMenu, shot, signIn } from './helpers';
import { installApiRecorder, readApiRecords } from './module-coverage';
import { globalServerSelector } from './server-selection';
import { readField } from './admin-menu-fixture';

// 环境准备由集成轮执行：至少三个启用区服，目标 OnlineAdminApiPrefix=online/admin，
// HttpManageUrl 指向 server-merge.mjs；两个不同普通运营账号均授予合服/区服菜单。
// E2E_MERGE_TARGET_NAME / E2E_MERGE_SOURCE_NAMES(JSON 字符串数组) 必填，禁止任取共享库区服。
// E2E_MERGE_REVIEWER_ACCOUNT / E2E_MERGE_REVIEWER_PASSWORD 必填；测试不主动调用授权/准备 API。
test('合服向导：取消计划释放区服并允许同范围再次建计划', async ({ browser }) => {
  const targetName = process.env.E2E_MERGE_TARGET_NAME;
  const sourceNames: string[] = JSON.parse(process.env.E2E_MERGE_SOURCE_NAMES ?? '[]');
  expect(targetName, '请准备隔离目标区服').toBeTruthy();
  expect(sourceNames.length, '请准备至少两个隔离源区服').toBeGreaterThanOrEqual(2);
  const page = await anonPage(browser);
  try {
    await installApiRecorder(page);
    await signIn(page, process.env.E2E_ACCOUNT ?? 'admin', process.env.E2E_PASSWORD ?? '123456');
    await page.context().storageState({ path: ADMIN_STATE });
    await navigateByMenu(page, '合服', '区服');
    for (const [round, cancellationReason] of ['取消错误范围', '再次取消演练'].entries()) {
      await page.getByRole('button', { name: '新建计划', exact: true }).click();
      const mergeDialog = page.locator('.p-dialog:visible').first();
      await mergeDialog.getByRole('combobox').nth(0).click();
      await page.getByRole('option', { name: targetName!, exact: true }).click();
      await page.mouse.click(5, 5);
      await page.waitForTimeout(400);
      await mergeDialog.locator('.p-multiselect').first().click();
      for (const source of sourceNames) {
        const option = page.getByRole('option', { name: source, exact: true });
        await option.waitFor({ state: 'visible', timeout: 10_000 });
        await option.click();
      }
      await page.getByRole('dialog').getByText('源服执行成功后退场。请先确认范围，再生成计划。').click();
      await page.locator('#merge-create-reason').fill('C404 取消后重建演练');
      await page.getByRole('button', { name: '生成计划', exact: true }).click();
      await expect(page.getByTestId('merge-status')).toHaveText('草稿');
      await page.getByRole('button', { name: '取消计划', exact: true }).last().click();
      const cancellationDialog = page.getByRole('dialog').last();
      await cancellationDialog.locator('#merge-cancellation-reason').fill(cancellationReason);
      await cancellationDialog.getByRole('button', { name: '取消计划', exact: true }).click();
      await expect(page.getByText('确认取消此合服计划？取消后参与区服可重新建计划。')).toBeVisible();
      await page.getByRole('button', { name: '确定', exact: true }).click();
      await expect(page.getByTestId('merge-status')).toHaveText('已取消');
      await expect(page.getByText(cancellationReason, { exact: true })).toBeVisible();
      await shot(page, `merge-cancelled-round-${round + 1}`);
      await page.getByRole('dialog').getByRole('button', { name: /close|关闭/i }).click();
    }
    const records = await readApiRecords(page);
    for (const endpoint of ['add', 'cancel']) {
      expect(records.some(record => record.url.includes(`/api/gameAreaMerge/${endpoint}`)), `页面真实请求 ${endpoint}`).toBe(true);
    }
  } finally {
    await page.context().storageState({ path: ADMIN_STATE });
    await page.context().close();
  }
});

test('合服向导：真实登录、预演、双人复核、执行与源服退场', async ({ browser }) => {
  const targetName = process.env.E2E_MERGE_TARGET_NAME;
  const sourceNames: string[] = JSON.parse(process.env.E2E_MERGE_SOURCE_NAMES ?? '[]');
  const reviewerAccount = process.env.E2E_MERGE_REVIEWER_ACCOUNT;
  const reviewerPassword = process.env.E2E_MERGE_REVIEWER_PASSWORD;
  expect(targetName, '请准备隔离目标区服').toBeTruthy();
  expect(sourceNames.length, '请准备至少两个隔离源区服').toBeGreaterThanOrEqual(2);
  expect(reviewerAccount, '请准备独立复核账号及菜单权限').toBeTruthy();
  expect(reviewerPassword).toBeTruthy();
  expect(reviewerAccount).not.toBe(process.env.E2E_ACCOUNT ?? 'admin');
  const applicant = await anonPage(browser);
  const reviewer = await anonPage(browser);
  try {
    await installApiRecorder(applicant);
    await signIn(applicant, process.env.E2E_ACCOUNT ?? 'admin', process.env.E2E_PASSWORD ?? '123456');
    await applicant.context().storageState({ path: ADMIN_STATE });
    await navigateByMenu(applicant, '合服', '区服');
    await applicant.getByRole('button', { name: '新建计划', exact: true }).click();
    // 向导下拉的 label 未做 aria 关联（无 accessible name），按向导弹窗内出现的顺序定位。
    const mergeDialog = applicant.locator('.p-dialog:visible').first();
    await mergeDialog.getByRole('combobox').nth(0).click();
    await applicant.getByRole('option', { name: targetName!, exact: true }).click();
    await applicant.mouse.click(5, 5);
    await applicant.waitForTimeout(400);
    // MultiSelect 的 combobox 是隐藏 input，点击可见的多选组件本体。
    await mergeDialog.locator('.p-multiselect').first().click();
    for (const source of sourceNames) {
      const option = applicant.getByRole('option', { name: source, exact: true });
      await option.waitFor({ state: 'visible', timeout: 10_000 });
      await option.click();
    }
    await applicant.getByRole('dialog').getByText('源服执行成功后退场。请先确认范围，再生成计划。').click();
    await applicant.locator('#merge-create-reason').fill('C404 合服浏览器演练');
    const createdResponse = applicant.waitForResponse(response => response.url().includes('/api/gameAreaMerge/add') && response.request().method() === 'POST');
    await applicant.getByRole('button', { name: '生成计划', exact: true }).click();
    const created = readField(await (await createdResponse).json(), 'data');
    const planId = String(readField(created, 'planId'));
    const targetIdentifier = String(readField(created, 'targetServerId'));
    await expect(applicant.getByTestId('merge-status')).toHaveText('草稿');
    await applicant.getByRole('button', { name: '预演', exact: true }).click();
    await expect(applicant.getByTestId('merge-status')).toHaveText('已预演');
    await expect(applicant.getByTestId('merge-report')).toContainText('players');
    await expect(applicant.getByTestId('merge-report')).toContainText(/GlobalDomainRowsTouched\):\s*0/);
    await expect(applicant.getByRole('button', { name: '批准', exact: true })).toBeDisabled();
    await shot(applicant, 'merge-dry-run-self-approval-blocked');

    await installApiRecorder(reviewer);
    await signIn(reviewer, reviewerAccount!, reviewerPassword!);
    await navigateByMenu(reviewer, '合服', '区服');
    await reviewer.getByRole('row').filter({ hasText: planId }).getByRole('button', { name: '详情', exact: true }).click();
    await reviewer.locator('#merge-review-reason').fill('已核查零全局域写入');
    await reviewer.getByRole('button', { name: '批准', exact: true }).click();
    await expect(reviewer.getByTestId('merge-status')).toHaveText('已批准');
    await expect(reviewer.getByRole('button', { name: '执行合服', exact: true })).toBeDisabled();
    await reviewer.locator('#merge-backup').fill(`backup-${Date.now()}`);
    await reviewer.getByLabel('已完成备份', { exact: true }).check();
    await reviewer.getByRole('button', { name: '执行合服', exact: true }).click();
    await expect(reviewer.getByText('确认执行合服？成功后源服将停止对客户端和顶栏选服可见。')).toBeVisible();
    await reviewer.getByRole('button', { name: '确定', exact: true }).click();
    await expect(reviewer.getByTestId('merge-status')).toHaveText('已完成');
    await expect(reviewer.getByTestId('merge-report')).toHaveCount(2);
    await shot(reviewer, 'merge-completed-reports');
    await reviewer.getByRole('dialog').getByRole('button', { name: /close|关闭/i }).click();
    await globalServerSelector(reviewer).getByRole('combobox').click();
    await expect(reviewer.getByRole('option', { name: targetName!, exact: true })).toBeVisible();
    for (const source of sourceNames) await expect(reviewer.getByRole('option', { name: source, exact: true })).toHaveCount(0);
    await reviewer.keyboard.press('Escape');
    await shot(reviewer, 'merge-sources-removed-from-selector');
    await navigateByMenu(reviewer, '游戏区服', '区服');
    // 全量套件中区服行数超过一页，按源服名检索后再断言（管理查询保留禁用行并显示已合服）；
    // 名称输入在「高级」折叠区，先展开。
    const advancedToggle = reviewer.locator('role=button[name=/高级/]').first();
    if (await advancedToggle.isVisible().catch(() => false)) {
      await advancedToggle.click();
      await reviewer.waitForTimeout(400);
    }
    const areaSearch = reviewer.locator('.search-form input:visible').first();
    for (const source of sourceNames) {
      await areaSearch.fill(source);
      await reviewer.getByRole('button', { name: '查询' }).first().click();
      await reviewer.waitForTimeout(1200);
      const row = reviewer.getByRole('row').filter({ hasText: source });
      await expect(row).toContainText('已合服');
      await expect(row).toContainText(targetIdentifier);
    }
    await shot(reviewer, 'merge-management-retains-retired-sources');
    const records = [...await readApiRecords(applicant), ...await readApiRecords(reviewer)];
    for (const endpoint of ['add', 'page', 'detail', 'dryRun', 'approve', 'execute']) {
      expect(records.some(record => record.url.includes(`/api/gameAreaMerge/${endpoint}`)), `页面真实请求 ${endpoint}`).toBe(true);
    }
    expect(records.some(record => record.url.includes('/api/gameArea/page'))).toBe(true);
  } finally {
    await applicant.context().storageState({ path: ADMIN_STATE });
    await reviewer.context().close();
    await applicant.context().close();
  }
});
