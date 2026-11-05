import { test, expect } from '@playwright/test';
import { SUPERADMIN_ROOTS, SUPERADMIN_STATE, signIn, shot } from './helpers';
import { renderedMenuGroups } from './module-coverage';

test('sign in as superadmin', async ({ page }) => {
  await signIn(page, process.env.E2E_SUPERADMIN_ACCOUNT ?? 'superadmin', process.env.E2E_SUPERADMIN_PASSWORD ?? '123456');
  expect((await renderedMenuGroups(page)).map(group => group.group)).toEqual([...SUPERADMIN_ROOTS]);
  await shot(page, 'superadmin-system-overview');
  await page.context().storageState({ path: SUPERADMIN_STATE });
});
