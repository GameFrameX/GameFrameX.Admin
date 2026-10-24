import { test, expect } from '@playwright/test';
import { enterWorkspace, gotoAppSelector, navigateByMenu, pickDateTime, shot, sleep, superadminPage } from './helpers';

// L3 功能闭环（数据驱动）：首批 6 个应用域实体的
// 真实新增 → 表格回显 → 删除 → 行消失，全程真实 web 交互、真实数据、结束即清理。
// 路由不硬编码：2026-10-20 起全面真实点击化——菜单标题在渲染后的侧边栏 DOM 中解析，
// 逐层真实展开分组后真实点击叶子链接（不读菜单树接口、不 hash 直达；appId 由菜单链接 href 自带）。
// 通知公告被 is_enabled 缺陷阻塞（Api 仓 issue #284），见 tests-disabled/。
// 全量推进记录（2026-10-18，42 个带 Edit 组件的叶子页盘点后）：
// 已闭环 12 页（本矩阵）；例外 9 页——
//   权限类：游戏角色/三方账号/应用管理的新增按钮 v-auth `<prefix>:add` 对租管不渲染（权限矩阵现实）；
//   字典管理超管按钮不渲染已修复（v-auth 超管短路，已入矩阵）；
//   引擎能力：支付配置 token 锚点落在列表不可见的 remark 列；
//   数据依赖：游戏内横幅公告与邮件的区服下拉当前环境"无选项/联动禁用"（需先存在可用区服数据）；
//     游戏区服自身必填 8 项含 serverVersion/appVersion 联动下拉，闭环三轮校验未过（表单验证失败，
//     疑 select 数据源按 appId 过滤后无匹配），待专项排查数据链；
//     邮件另叠加 Vue#198 的两处漏 import（已修复，待合入）；
//   缺陷阻塞（issue 跟踪）：职位 remark 约束（Api#286）、渠道列表显示软删行（Api#287）、
//     组织新建 NRE（Api#288）、邮件新增前端 ReferenceError（Vue#198）、通知 is_enabled（Api#284）；
//   结构/语义例外：游戏订单（三卡片多表页，锚点歧义）；
//     游戏运营配置（config 必填合法 JSON，引擎文本值语义不匹配）；
//   级联/联动交互（交互器 v2 待建）：组织管理与游戏内商品的级联选择存在
//     "点目录=展开而非选中"语义（'未选择任何项'），需按叶子标记定位；
//     运营邮件 Campaign 的 areaId 下拉联动禁用；活动功能开关多 select 联动必填；
//   自举依赖：邮件模板的必填分类下拉取自模板分类数据（闭环用例建完即删，常态为空；
//     解锁需引擎支持"用例内自举前置数据"）；
//   DatePicker 必填已由 date 填充器解决（游戏公告已闭环）；
//   待专项：角色管理删除走行上下文菜单（⋯）+ 动态权限。
// 其余 21 页含 Password/DatePicker/CascadeSelect/RewardItemsEditor/依赖链 select 等复杂控件，需引擎扩展后分批纳入。

// 2026-10-20 补编辑半边：闭环升级为 新增 → 行回显 → 编辑提交（回显断言 + 改值）→ 行回显新值 → 删除 → 行消失。
// update 路由按约定由 create 路由派生（xxx/add → xxx/update，C18 权限码对齐约定），不逐实体重复登记。
type EntitySpec = {
  name: string;
  menuTitle: string;
  session?: 'admin' | 'superadmin';
  // 行内无「编辑」按钮的页（如兑换码：编辑复用新增弹窗、动作列只挂删除）不跑编辑半边
  rowEdit?: boolean;
  createApi: RegExp;
  deleteApi: RegExp;
};

const ENTITIES: EntitySpec[] = [
  {
    name: '版本定义',
    menuTitle: '版本定义',
    fields: ['text', 'text'],
    createApi: /tenantGameAppVersionDefinition\/add/,
    deleteApi: /tenantGameAppVersionDefinition\/delete/,
  },
  {
    name: '资源包定义',
    // menuTitle 必须是侧边栏渲染后的 i18n 标签（用户所见），不是种子菜单树的 Meta.Title：
    // gameAppAssetPackage 的 zh-cn 标签是「资源包」，与树 Title「资源包定义」已漂移
    menuTitle: '资源包',
    fields: ['text', 'text'],
    createApi: /tenantGameAppAssetPackage\/add/,
    deleteApi: /tenantGameAppAssetPackage\/delete/,
  },
  {
    name: '配置字段定义',
    menuTitle: '内容元数据', // configKey 的 zh-cn 渲染标签（树 Title「配置字段定义」已漂移）
    fields: ['text', 'text', 'select'],
    createApi: /tenantGameAppRemoteConfigKey\/add/,
    deleteApi: /tenantGameAppRemoteConfigKey\/delete/,
  },
  {
    name: '配置模式定义',
    menuTitle: '模式配置', // configMode 的 zh-cn 渲染标签（树 Title「配置模式定义」已漂移）
    fields: ['text', 'text'],
    createApi: /tenantGameAppRemoteConfigMode\/add/,
    deleteApi: /tenantGameAppRemoteConfigMode\/delete/,
  },
  {
    name: '设备定义',
    menuTitle: '设备', // tenantDevice 的 zh-cn 渲染标签（树 Title「设备定义」已漂移）
    fields: ['text', 'text', 'text'],
    createApi: /tenantGameAppDevice\/add/,
    deleteApi: /tenantGameAppDevice\/delete/,
  },
  {
    name: '邮件模板分类',
    menuTitle: '模板分类',
    fields: ['text'],
    createApi: /tenantGameMailTemplateCategoryEntity\/add/,
    deleteApi: /tenantGameMailTemplateCategoryEntity\/delete/,
  },
  {
    name: '物品目录',
    menuTitle: '物品目录',
    fields: ['text', 'text'],
    createApi: /tenantGameGoodsCatalogueEntity\/add/,
    deleteApi: /tenantGameGoodsCatalogueEntity\/delete/,
  },
  {
    name: '游区分类',
    menuTitle: '游区分类',
    fields: ['text', 'text'],
    createApi: /tenantGameAreaClassifyEntity\/add/,
    deleteApi: /tenantGameAreaClassifyEntity\/delete/,
  },
  {
    name: '兑换码渠道',
    menuTitle: '兑换码渠道',
    fields: ['text', 'text', 'number'],
    createApi: /tenantGameRedeemCodeChannelEntity\/add/,
    deleteApi: /tenantGameRedeemCodeChannelEntity\/delete/,
  },
  {
    name: '定时任务',
    menuTitle: '定时任务',
    fields: ['text', 'text', 'text'],
    createApi: /tenantGameTimeJobEntity\/add/,
    deleteApi: /tenantGameTimeJobEntity\/delete/,
  },
  {
    name: '奖励模板分组',
    menuTitle: '奖励模板分组',
    fields: ['text'],
    createApi: /tenantGameRewardTemplateGroupEntity\/add/,
    deleteApi: /tenantGameRewardTemplateGroupEntity\/delete/,
  },
  {
    name: '游戏公告',
    menuTitle: '游戏公告',
    fields: ['select', 'select', 'date', 'date', 'number', 'text', 'text'],
    createApi: /tenantGameAnnouncementEntity\/add/,
    deleteApi: /tenantGameAnnouncementEntity\/delete/,
  },
  {
    name: '游戏物品',
    menuTitle: '游戏物品',
    fields: ['unique-number', 'select', 'text', 'text'],
    createApi: /tenantGameGoodsEntity\/add/,
    deleteApi: /tenantGameGoodsEntity\/delete/,
  },
  {
    name: '游戏运营配置',
    menuTitle: '游戏运营配置',
    fields: ['select', 'text', 'json', 'text'],
    createApi: /tenantGameAppServerConfig\/add/,
    deleteApi: /tenantGameAppServerConfig\/delete/,
  },
  {
    name: '分包管理',
    menuTitle: '分包管理',
    fields: ['select', 'text', 'select', 'select', 'text', 'text'],
    createApi: /tenantGameAppPackage\/add/,
    deleteApi: /tenantGameAppPackage\/delete/,
  },
  {
    name: '通知公告',
    menuTitle: '通知公告',
    fields: ['text', 'select', 'editor'],
    createApi: /tenantNotice\/add/,
    deleteApi: /tenantNotice\/delete/,
  },
  {
    name: '职位管理',
    menuTitle: '职位管理',
    fields: ['text', 'text'],
    createApi: /tenantPosition\/add/,
    deleteApi: /tenantPosition\/delete/,
  },
  {
    name: '渠道定义',
    menuTitle: '渠道定义',
    fields: ['text', 'text', 'text', 'select', 'text'],
    createApi: /tenantChannelDefinition\/add/,
    deleteApi: /tenantChannelDefinition\/delete/,
  },
  {
    name: '奖励模板',
    menuTitle: '奖励模板',
    fields: ['text', 'select', 'number', 'reward-items', 'text'],
    createApi: /tenantGameRewardTemplateEntity\/add/,
    deleteApi: /tenantGameRewardTemplateEntity\/delete/,
  },
  {
    name: '兑换码',
    rowEdit: false, // 动作列仅删除（C130 盘点同源）；编辑复用新增弹窗
    menuTitle: '兑换码',
    fields: ['select', 'text', 'date', 'select', 'text', 'number', 'date', 'skip-select', 'reward-items'],
    createApi: /tenantGameRedeemCodeEntity\/add/,
    deleteApi: /tenantGameRedeemCodeEntity\/delete/,
  },
  // 游戏内商品：级联 v2 已验证可用（前置目录数据已 seed），但 add 稳定 500 NRE——
  // Api#291（AddRequest CategoryId:string/Number:long 与前端契约错位 → 绑定失败 input=null）。
  // 组织管理：级联 v2 可选中叶子，但列表树形数据只渲染根行（Vue#200）token 无法回显。
  // 级别 InputNumber fill 的 onInputFocus NRE 已在应用启动层兜底修复（Vue#201），
  // 组织 CRUD 闭环仍受 Vue#200 阻断；奖励模板「级别」改回真实 number fill 作回归。
  {
    name: '平台定义',
    menuTitle: '平台定义',
    session: 'superadmin',
    fields: ['text', 'text'],
    createApi: /globalPlatform\/add/,
    deleteApi: /globalPlatform\/delete/,
  },
  {
    name: '语言本地化定义',
    menuTitle: '语言本地化定义',
    session: 'superadmin',
    fields: ['text', 'text', 'text'],
    createApi: /globalLanguage\/add/,
    deleteApi: /globalLanguage\/delete/,
  },
  {
    name: '字典管理',
    menuTitle: '字典管理',
    session: 'superadmin',
    fields: ['text', 'text'],
    createApi: /globalDictionaryType\/add/,
    deleteApi: /globalDictionaryType\/delete/,
  },
  {
    name: '参数配置',
    menuTitle: '参数配置',
    session: 'superadmin',
    fields: ['text', 'text', 'text', 'text'],
    createApi: /systemConfig\/add/,
    deleteApi: /systemConfig\/delete/,
  },
];

test.describe('CRUD 功能闭环（首批）', () => {
  for (const entity of ENTITIES) {
    test(`${entity.name} 新增回显删除闭环`, async ({ page: adminPage, browser }) => {
      // 默认 page fixture 是 admin 态；平台域实体用 superadmin 显式上下文
      const ownContext = entity.session === 'superadmin';
      const page = ownContext ? await superadminPage(browser) : adminPage;

      const token = `GFX_E2E_${Date.now().toString(36)}`;

      let created = false;
      let updated = false;
      let deleted = false;
      await page.route(entity.createApi, async (route) => {
        created = true;
        await route.continue();
      });
      await page.route(new RegExp(entity.createApi.source.replace('add', 'update')), async (route) => {
        updated = true;
        await route.continue();
      });
      await page.route(entity.deleteApi, async (route) => {
        deleted = true;
        await route.continue();
      });

      // 真实菜单导航：等守卫弹回落地后，先在当前侧边栏找目标菜单；
      // 租户域菜单分布在两个真实表面——应用选择页全局侧边栏与应用工作台侧边栏，
      // 当前表面没有时切换到另一个表面（全部真实点击）再试
      await page.goto('/');
      await sleep(900);
      await page.locator('.p-panelmenu').waitFor({ timeout: 15_000 });
      try {
        await navigateByMenu(page, entity.menuTitle);
      } catch {
        if (entity.session === 'superadmin') throw new Error(`平台侧边栏未找到「${entity.menuTitle}」`);
        if (page.url().includes('appSelector')) {
          await enterWorkspace(page);
        } else {
          await gotoAppSelector(page);
        }
        await navigateByMenu(page, entity.menuTitle);
      }
      await expect(page.locator('.p-datatable-tbody')).toBeVisible({ timeout: 20_000 });

      await page.locator('role=button[name="新增"]').first().click();
      await expect(page.locator('.p-dialog')).toBeVisible({ timeout: 10_000 });

      // 按对话框内控件顺序索引填充（label 不关联可访问名，按名匹配不可靠）；
      // 每个文本字段值唯一化（避免唯一约束撞历史残留），首字段承载回显锚点 token
      let textIndex = 0;
      let selectIndex = 0;
      let numberIndex = 0;
      let cascadeIndex = 0;
      let dateFieldIndex = 0;
      for (const field of entity.fields) {
        if (field === 'text') {
          const value = textIndex === 0 ? token : `${token}-${textIndex}`;
          await page.locator(`.p-dialog >> role=textbox >> nth=${textIndex}`).fill(value);
          textIndex += 1;
        } else if (field === 'cascade') {
          // 级联选择 v2：物理 click 在 PrimeVue 4 CascadeSelect 上不生效（onOptionClick 不触发），
          // 用程序化 dispatchEvent。策略：每次在当前最深子列里优先点叶子（group=false），
          // 无叶子则点首个目录展开下一层；点中叶子即选中收起。封顶 6 层。
          await page.locator(`.p-dialog >> .p-cascadeselect-dropdown >> nth=${cascadeIndex}`).click();
          for (let depth = 0; depth < 6; depth += 1) {
            const step = await page.evaluate(() => {
              const overlays = Array.from(document.querySelectorAll('.p-cascadeselect-overlay')).filter(
                (el) => el.getBoundingClientRect().height > 0,
              );
              const overlay = overlays[0];
              if (!overlay) return 'closed';
              const lists = Array.from(overlay.querySelectorAll(':scope ul, :scope ul ul')).filter(
                (el) => el.getBoundingClientRect().width > 0,
              );
              const deepest = lists[lists.length - 1];
              if (!deepest) return 'no-list';
              const items = Array.from(deepest.children) as HTMLElement[];
              const leaf = items.find((li) => li.dataset.pOptionGroup === 'false' || li.getAttribute('data-p-option-group') === 'false');
              const target = (leaf ?? items[0])?.querySelector('.p-cascadeselect-option-content');
              if (!target) return 'no-item';
              target.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true }));
              return leaf ? 'leaf-clicked' : 'group-clicked';
            });
            if (step === 'closed' || step === 'leaf-clicked') break;
            await sleep(350);
          }
          // 不用 keyboard Escape：会直达 PrimeVue Dialog 的 dismissOnEscape 关掉整个对话框；
          // 面板未自动收起时点 body 外部关闭
          await page.evaluate(() => {
            const overlay = Array.from(document.querySelectorAll('.p-cascadeselect-overlay')).find(
              (el) => el.getBoundingClientRect().height > 0,
            );
            if (overlay) document.body.dispatchEvent(new MouseEvent('mousedown', { bubbles: true }));
          });
          cascadeIndex += 1;
          selectIndex += 1;
        } else if (field === 'json') {
          // 占 textbox 序的 JSON 语义字段（如 serverConfig.config），填合法空对象
          await page.locator(`.p-dialog >> role=textbox >> nth=${textIndex}`).fill('{}');
          textIndex += 1;
        } else if (field === 'editor') {
          // wangeditor 富文本：contenteditable 区点击后键入（不占 textbox/combobox 序）
          await page.locator('.w-e-text-container [contenteditable="true"]').click();
          await page.keyboard.type('E2E 内容');
        } else if (field === 'reward-items') {
          // RewardItemsEditor：点"添加"按钮增行，行内 catalogue/goods 两个 Select 各选首项。
          // 与 cascade 同理用 dispatchEvent（物理 click 对 PrimeVue 下拉不触发）。
          await page.locator('.p-dialog >> .reward-items-editor .add-button').click();
          await sleep(300);
          for (const sel of ['.catalogue-select', '.goods-select']) {
            await page.locator(`.p-dialog >> .reward-items-editor-row ${sel}`).click();
            const firstOption = page.locator('role=option >> nth=0');
            if (!(await firstOption.isVisible())) {
              await page.locator(`.p-dialog >> .reward-items-editor-row ${sel} ~ * , .p-dialog >> .reward-items-editor-row ${sel}`).first().click();
            }
            await firstOption.first().click();
            // 选项懒加载（goods 按 catalogue 过滤），等数据与面板收起
            await sleep(600);
          }
        } else if (field === 'skip-text') {
          textIndex += 1;
        } else if (field === 'skip-number') {
          // 占位：有默认值的数字字段（如 orderNo），仅递增 spinbutton 序号
          numberIndex += 1;
        } else if (field === 'skip-select') {
          selectIndex += 1;
        } else if (field === 'skip') {
          // 占位：物理存在但非必填的 combobox（如级联上级），仅递增序号
          selectIndex += 1;
        } else if (field === 'select') {
          await page.locator(`.p-dialog >> role=combobox >> nth=${selectIndex}`).click();
          const firstOption = page.locator('role=option >> nth=0');
          if (!(await firstOption.isVisible())) {
            // editable/只读 Select 点输入框不开面板：兜底点专用下拉钮
            await page.locator(`.p-dialog >> .p-select-dropdown >> nth=${selectIndex}`).click();
          }
          await firstOption.first().click();
          selectIndex += 1;
        } else if (field === 'unique-number') {
          // 唯一编号字段（如游戏物品 Number）：预置数据可能已占 1，用时间派生值避免唯一约束冲突
          await page.locator(`.p-dialog >> role=spinbutton >> nth=${numberIndex}`).fill(String((Date.now() % 899) + 2));
          numberIndex += 1;
        } else if (field === 'number') {
          await page.locator(`.p-dialog >> role=spinbutton >> nth=${numberIndex}`).fill('1');
          numberIndex += 1;
        } else {
          // DatePicker(showTime) 真实面板交互：点开 → 选可点日期格 → 调时分 → 点外部收起（helpers.pickDateTime）；
          // 同表单第 2+ 个日期选月末格，满足 end>start 及最小间隔校验（如兑换码失效时间）
          await pickDateTime(page, selectIndex, dateFieldIndex === 0 ? 'first' : 'last');
          dateFieldIndex += 1;
          selectIndex += 1;
        }
      }

      await page.locator('role=button[name="确定"]').first().click();
      await expect.poll(() => created, `${entity.name}: 新增请求未发出`).toBe(true);
      await expect(page.locator('.p-datatable-tbody')).toContainText(token, { timeout: 20_000 });
      // —— 编辑闭环（rowEdit=false 的页跳过：动作列未挂编辑按钮，如兑换码）：
      // 真实打开行编辑弹窗 → 回显断言（任意文本框含 token；编辑态字段顺序/禁用可与新增态不同）
      // → 改首个「可编辑」文本框（业务键编辑态锁定是常态；数字框用数字值）→ 提交 → 更新真实发出且行仍在 ——
      if (entity.rowEdit !== false) {
        await page.locator(`tr:has-text("${token}") >> text=编辑`).first().click();
        await expect(page.locator('.p-dialog')).toBeVisible({ timeout: 10_000 });
        const editState = await page.evaluate((tk) => {
          const boxes = Array.from(document.querySelectorAll('.p-dialog input, .p-dialog textarea')).filter(
            (el) => el.getBoundingClientRect().height > 0,
          );
          return {
            echoed: boxes.some((el) => (el as HTMLInputElement).value.includes(tk)),
            editableIndex: boxes.findIndex(
              (el) => !(el as HTMLInputElement).disabled && !(el as HTMLInputElement).readOnly,
            ),
          };
        }, token);
        expect(editState.echoed, `${entity.name}: 编辑弹窗未回显新增值`).toBe(true);
        if (editState.editableIndex >= 0) {
          const editableBox = page.locator('.p-dialog input:visible, .p-dialog textarea:visible').nth(editState.editableIndex);
          const isNumeric = await editableBox.evaluate(
            (el) => (el as HTMLInputElement).type === 'number' || (el as HTMLInputElement).getAttribute('inputmode') === 'numeric',
          );
          // 数字框填数字值（非数字会被校验拦下，编辑请求发不出）
          await editableBox.fill(isNumeric ? String((Date.now() % 899) + 2) : `${token}-E2`, { timeout: 5000 });
          await page.locator('role=button[name="确定"]').first().click();
          await expect.poll(() => updated, `${entity.name}: 编辑请求未发出`).toBe(true);
          await sleep(800);
          await expect(page.locator('.p-datatable-tbody')).toContainText(token, { timeout: 20_000 });
        } else {
          // 全字段锁定：仅验证编辑弹窗可开且回显
          await page.keyboard.press('Escape');
        }
      }

      await page.locator(`tr:has-text("${token}") >> text=删除`).first().click();
      await page.locator('role=button[name="确定"]').first().click();
      await expect.poll(() => deleted, `${entity.name}: 删除请求未发出`).toBe(true);
      await expect(page.locator('.p-datatable-tbody')).not.toContainText(token, { timeout: 20_000 });

      await shot(page, `crud-roundtrip-${entity.name}`);
      if (ownContext) await page.close();
    });
  }
});
