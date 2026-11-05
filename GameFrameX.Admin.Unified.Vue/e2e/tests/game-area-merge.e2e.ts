import { test, expect } from '@playwright/test';
import { ADMIN_STATE, anonPage, navigateByMenu, shot, signIn } from './helpers';
import { installApiRecorder, readApiRecords } from './module-coverage';
import { globalServerSelector } from './server-selection';
import { readField } from './admin-menu-fixture';

// 环境准备由集成轮执行：至少三个启用区服，目标 OnlineAdminApiPrefix=online/admin，
// HttpManageUrl 指向 server-merge.mjs；两个不同普通运营账号均授予合服/区服菜单。
// E2E_MERGE_TARGET_NAME / E2E_MERGE_SOURCE_NAMES(JSON 字符串数组) 必填，禁止任取共享库区服。
// E2E_MERGE_REVIEWER_ACCOUNT / E2E_MERGE_REVIEWER_PASSWORD 必填；测试不主动调用授权/准备 API。
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
    await applicant.getByRole('combobox', { name: '目标服', exact: true }).click();
    await applicant.getByRole('option', { name: targetName!, exact: true }).click();
    await applicant.getByRole('combobox', { name: '源服', exact: true }).click();
    for (const source of sourceNames) await applicant.getByRole('option', { name: source, exact: true }).click();
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
    for (const source of sourceNames) {
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
