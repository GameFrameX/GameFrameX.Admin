import { test, expect } from '@playwright/test';
import { ADMIN_ROOTS, ADMIN_STATE, shot, signIn } from './helpers';
import { renderedMenuGroups } from './module-coverage';
import { ensureMergeReviewer, isolatedAdminOrigin } from './admin-stack';

// C406 / GFX-1232 集成第 2 轮（R6）：admin 账号由 Web.Api 首启种子引导创建
// （GAMEFRAMEX_ADMIN_INITIAL_ACCOUNT / _PASSWORD，取值与 E2E_ACCOUNT / E2E_PASSWORD 一致），
// sys_admin 角色已随引导获得全部非超管菜单——本 setup 不再做 role/grantMenu 主动授权。
test('sign in as admin', async ({ page }) => {
  isolatedAdminOrigin();
  const account = process.env.E2E_ACCOUNT ?? 'admin';
  const password = process.env.E2E_PASSWORD ?? '123456';
  // 新装首启后的第一次表单登录即成功（引导由 HostedService 在启动期同步完成，无竞态）
  await signIn(page, account, password);
  expect((await renderedMenuGroups(page)).map(group => group.group)).toEqual([...ADMIN_ROOTS]);
  await shot(page, 'admin-authorized-nine-roots');
  // 合服复核人走产品路径准备：引导出来的 admin 在「账号管理」真实界面创建并重置口令
  await ensureMergeReviewer(page, process.env.E2E_MERGE_REVIEWER_NAME ?? 'e2e_merge_reviewer', 'E2E 合服复核人');
  await page.context().storageState({ path: ADMIN_STATE });
});
