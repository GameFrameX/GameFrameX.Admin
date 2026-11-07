import { test, expect, type Page } from '@playwright/test';
import { navigateByMenu, shot, sleep, superadminPage } from './helpers';
import { installApiRecorder, readApiRecords } from './module-coverage';

//  集成轮专门断言（调度合一 + 审计留存）：
// 1) 「任务调度」页（超管）能看到审计留存作业（job_auditRetention）与业务定时任务派发作业
//  （job_gameTimeJobDispatch）——内置作业此前从未落库，页面看不到；手动执行留存作业后，
//  触发器运行态（触发次数）从数据库刷新可见（运行态写回）。
// 2) 预置的超期访问 / 操作日志（环境准备 SQL，CreatedTime 早于热数据窗口）在留存作业执行前
//  于「审计 › 访问日志 / 操作日志」（admin 视角，system_* 与业务日志联合查询）可见，
//  手动执行后消失（归档表行数由编排脚本查库留证）。
// 3) 普通 admin 在「系统 › 定时任务」完成增删改（crud-roundtrips 闭环之外的专门截图证据）。
const RETENTION_JOB_ID = 'job_auditRetention';
const DISPATCH_JOB_ID = 'job_gameTimeJobDispatch';
const ARCHIVE_PROBE_MARKER = 'gfx1216-archive-probe';

// 展开作业行并等待触发器子表渲染（展开内容异步挂载，需要显式等待）。
async function expandJobTriggers(page: Page, jobId: string): Promise<void> {
  // 展开按钮在行首列；点击后验证内表行出现，未展开则重试（偶发一次点击未生效，重试幂等：已展开即返回）。
  const row = page.locator('.p-datatable-tbody tr', { hasText: jobId }).first();
  const toggler = row.locator('td').first().locator('button');
  for (let attempt = 0; attempt < 4; attempt += 1) {
    await toggler.click();
    await sleep(1200);
    const expanded = await page.locator('tr.p-datatable-row-expansion table tr:visible', { hasText: 'trigger_auditRetention' }).count();
    if (expanded > 0) {
      await shot(page, `expand-${jobId}`);
      return;
    }
  }
  throw new Error(`作业 ${jobId} 行展开未生效（重试 4 次）`);
}

async function readTriggerRunCount(page: Page, triggerId: string): Promise<number> {
  // 按表头定位「触发次数」列：TimeColumn 无值时不渲染单元格，列号随运行态变化漂移，
  // 固定下标不可靠（展开容器行 tr.p-datatable-row-expansion 的文本含内表，也不能做行定位）。
  const innerTable = page.locator('tr.p-datatable-row-expansion table').first();
  const headerTexts = await innerTable.locator('thead th').allInnerTexts();
  const columnIndex = headerTexts.findIndex(header => header.includes('触发次数'));
  expect(columnIndex, '触发器子表应有「触发次数」列').toBeGreaterThanOrEqual(0);
  const row = innerTable.locator('tbody tr', { hasText: triggerId }).first();
  const text = (await row.locator('td').nth(columnIndex).innerText()).trim();
  const count = Number(text);
  expect(Number.isFinite(count), `触发器 ${triggerId} 运行次数列应为数字，实际 "${text}"`).toBe(true);
  return count;
}

async function runRetentionJob(page: Page): Promise<void> {
  const row = page.locator('.p-datatable-tbody tr', { hasText: RETENTION_JOB_ID }).first();
  await row.locator('button:has(i.fa-circle-check)').first().click();
  // 归档分批执行 + 运行态落库后再刷新。
  await sleep(3000);
}

test('超期访问与操作日志在留存作业执行后从热数据页消失', async ({ browser, page }) => {
  // 阶段一（admin）：按 ActionName 关键词检索（两页 searchKey 服务端过滤 ActionName），
  // 执行前探针行在两个日志页可见。
  const searchByMarker = async (): Promise<void> => {
    await page.locator('.search-form input:visible').first().fill(ARCHIVE_PROBE_MARKER);
    await page.locator('role=button[name="查询"]').first().click();
    await sleep(1500);
  };
  await installApiRecorder(page);
  await page.goto('/');
  await navigateByMenu(page, '访问日志', '审计');
  await searchByMarker();
  await expect(page.locator('.p-datatable-tbody tr', { hasText: ARCHIVE_PROBE_MARKER }).first()).toBeVisible();
  await shot(page, 'retention-visit-before');

  await navigateByMenu(page, '操作日志', '审计');
  await searchByMarker();
  await expect(page.locator('.p-datatable-tbody tr', { hasText: ARCHIVE_PROBE_MARKER }).first()).toBeVisible();
  await shot(page, 'retention-operation-before');

  // 阶段二（超管）：手动执行审计留存作业。
  const supervisor = await superadminPage(browser);
  try {
    await supervisor.goto('/');
    await navigateByMenu(supervisor, '任务调度', '系统');
    await runRetentionJob(supervisor);
    await shot(supervisor, 'retention-job-executed');
  } finally {
    await supervisor.context().close();
  }

  // 阶段三（admin）：重新按关键词查询，探针行从热数据页消失（已入归档表，查库证据由编排脚本留存）。
  await searchByMarker();
  await expect(page.locator('.p-datatable-tbody tr', { hasText: ARCHIVE_PROBE_MARKER })).toHaveCount(0);
  await shot(page, 'retention-operation-archived');

  await navigateByMenu(page, '访问日志', '审计');
  await searchByMarker();
  await expect(page.locator('.p-datatable-tbody tr', { hasText: ARCHIVE_PROBE_MARKER })).toHaveCount(0);
  await shot(page, 'retention-visit-archived');
});

test('任务调度页可见内置作业且手动执行后运行态更新', async ({ browser }) => {
  const page = await superadminPage(browser);
  try {
    await installApiRecorder(page);
    await page.goto('/');
    await navigateByMenu(page, '任务调度', '系统');

    await expect(page.locator('.p-datatable-tbody tr', { hasText: RETENTION_JOB_ID }).first()).toBeVisible();
    await expect(page.locator('.p-datatable-tbody tr', { hasText: DISPATCH_JOB_ID }).first()).toBeVisible();
    await shot(page, 'scheduler-jobs-visible');

    await expandJobTriggers(page, RETENTION_JOB_ID);
    await expect(page.locator('tr.p-datatable-row-expansion table tr:visible', { hasText: 'trigger_auditRetention' }).first()).toBeVisible();
    await shot(page, 'scheduler-trigger-expanded');
    const runsBefore = await readTriggerRunCount(page, 'trigger_auditRetention');

    await runRetentionJob(page);
    await page.locator('role=button[name="查询"]').first().click();
    await sleep(1500);
    await expandJobTriggers(page, RETENTION_JOB_ID);
    const runsAfter = await readTriggerRunCount(page, 'trigger_auditRetention');
    expect(runsAfter, '手动执行后触发次数应增加（运行态已写回数据库）').toBeGreaterThan(runsBefore);
    await shot(page, 'scheduler-runtime-updated');

    const records = await readApiRecords(page);
    expect(records.length).toBeGreaterThan(0);
  } finally {
    await page.context().close();
  }
});

test('普通管理员在系统下仍可管理业务定时任务', async ({ page }) => {
  await installApiRecorder(page);
  await page.goto('/');
  await navigateByMenu(page, '定时任务', '系统');

  const marker = `integration-${Date.now() % 100000000}`;
  let created = false;
  let updated = false;
  let deleted = false;
  const onRequest = (record: { url: string; method: string }): void => {
    if (/gameTimeJob\/add/.test(record.url)) created = true;
    if (/gameTimeJob\/update/.test(record.url)) updated = true;
    if (/gameTimeJob\/delete/.test(record.url)) deleted = true;
  };
  page.on('request', request => {
    if (/api\/gameTimeJob/.test(request.url())) {
      onRequest({ url: request.url(), method: request.method() });
    }
  });

  // 新增：Name=GameMailDeliveryJob（唯一真实 IScheduleJob 实现，手动执行路径依赖按名解析）、Cron 远期一次性；表单无 Id 输入（服务端雪花分配）。
  await page.locator('role=button[name="新增"]').first().click();
  const dialog = page.locator('.p-dialog:visible').first();
  await expect(dialog).toBeVisible();
  const inputs = dialog.locator('input:visible:not([type="checkbox"]):not([readonly]):not(:disabled)');
  await inputs.nth(0).fill('GameMailDeliveryJob');
  await inputs.nth(1).fill(marker);
  await inputs.nth(2).fill('0 0 3 1 1 *');
  await page.locator('role=button[name="确定"]').first().click();
  await expect.poll(() => created, '新增请求未发出').toBe(true);
  await sleep(800);
  await expect(page.locator('.p-datatable-tbody')).toContainText(marker);
  await shot(page, 'timejob-created');

  // 编辑：改描述。
  const row = page.locator('.p-datatable-tbody tr', { hasText: marker }).first();
  await row.locator('button').filter({ hasText: '编辑' }).first().click();
  const editDialog = page.locator('.p-dialog:visible').first();
  await expect(editDialog).toBeVisible();
  await editDialog.locator('input:visible:not([type="checkbox"]):not([readonly]):not(:disabled)').nth(1).fill(`${marker}-E2`);
  await page.locator('role=button[name="确定"]').first().click();
  await expect.poll(() => updated, '更新请求未发出').toBe(true);
  await sleep(800);
  await expect(page.locator('.p-datatable-tbody')).toContainText(`${marker}-E2`);
  await shot(page, 'timejob-updated');

  // 删除：二次确认。
  await page.locator('.p-datatable-tbody tr', { hasText: `${marker}-E2` }).first()
       .locator('button').filter({ hasText: '删除' }).first().click();
  await expect(page.locator('.p-dialog:visible, .p-confirm-popup:visible').first()).toBeVisible();
  await page.locator('role=button[name="确定"]').last().click();
  await expect.poll(() => deleted, '删除请求未发出').toBe(true);
  await sleep(800);
  await expect(page.locator('.p-datatable-tbody')).not.toContainText(marker);
  await shot(page, 'timejob-deleted');
});
