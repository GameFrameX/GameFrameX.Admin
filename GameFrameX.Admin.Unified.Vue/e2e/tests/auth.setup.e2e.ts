import { test, expect } from '@playwright/test';
import { ADMIN_ROOTS, ADMIN_STATE, logoutByMenu, signIn, shot } from './helpers';
import { renderedMenuGroups } from './module-coverage';
import { grantAdminMenus, isolatedAdminOrigin } from './admin-menu-fixture';

test('sign in as tenant admin', async ({ page, browser }) => {
  isolatedAdminOrigin();
  const account = process.env.E2E_ACCOUNT ?? 'admin';
  const password = process.env.E2E_PASSWORD ?? '123456';
  const token = await signIn(page, account, password);
  await grantAdminMenus(browser, page, token);
  await logoutByMenu(page);
  await signIn(page, account, password);
  expect((await renderedMenuGroups(page)).map(group => group.group)).toEqual([...ADMIN_ROOTS]);
  await shot(page, 'admin-authorized-nine-roots');
  await page.context().storageState({ path: ADMIN_STATE });
});
