import { test, expect } from '@playwright/test';
import { ADMIN_ROOTS, SUPERADMIN_ROOTS, expectOverview, shot, superadminPage } from '../helpers';
import { renderedMenuGroups } from '../module-coverage';

test('普通管理员单 shell 根组清单一致（九组）', async ({ page }) => {
  await page.goto('/');
  await expectOverview(page);
  expect((await renderedMenuGroups(page)).map(entry => entry.group)).toEqual([...ADMIN_ROOTS]);
  await shot(page, 'admin-nine-roots');
});

test('超管单 shell 根组清单一致（两组）', async ({ browser }) => {
  const page = await superadminPage(browser);
  try {
    await page.goto('/');
    await expectOverview(page);
    expect((await renderedMenuGroups(page)).map(entry => entry.group)).toEqual([...SUPERADMIN_ROOTS]);
    await shot(page, 'superadmin-two-roots');
  } finally {
    await page.context().close();
  }
});
