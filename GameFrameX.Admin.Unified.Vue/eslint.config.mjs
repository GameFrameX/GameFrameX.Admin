import tsParser from '@typescript-eslint/parser'
import tsPlugin from '@typescript-eslint/eslint-plugin'
import vueParser from 'vue-eslint-parser'
import vuePlugin from 'eslint-plugin-vue'

const browserGlobals = {
    console: 'readonly',
    window: 'readonly',
    document: 'readonly',
    localStorage: 'readonly',
    sessionStorage: 'readonly',
    FormData: 'readonly',
    Blob: 'readonly',
    File: 'readonly',
    URL: 'readonly',
    setTimeout: 'readonly',
    clearTimeout: 'readonly',
    setInterval: 'readonly',
    clearInterval: 'readonly',
}

// ── TS 标识符命名规则（GFX-700 / C287）───────────────────────────────────────
// 变量/函数 camelCase；type/interface/class/enum PascalCase。
// 已知合理场景的豁免面（在 selector 层放行并在此注释说明）：
// - import：camelCase / PascalCase（组件与类型绑定，如 CrudDataTable、FormField）/ UPPER_CASE（常量）
// - objectLiteralProperty：对象字面量键承载后端契约键/错误码/本地化 key，不做格式约束
//   （后端契约字符串如 'NotifyApple' 属字符串字面量，本就不受本规则约束）
// - typeProperty：TS 类型属性常映射后端契约字段（Id/IsEnabled）与 kebab 事件名（toggle-visible），不在本规则范围
// - variable 放行 UPPER_CASE（模块常量）与单/双首尾下划线（__NEXT_VERSION__ 全局版本注入等）；
//   名字中间的下划线（getEnumXxxData_Index）不放行，存量逐点标注
// - PascalCase 局部变量（defineAsyncComponent 组件占位等）不在此放行，存量逐点 eslint-disable + 理由注释
const namingConventionRule = [
    'error',
    // 默认标识符（含类成员/方法等未单独配置的 selector）：camelCase
    { selector: 'default', format: ['camelCase'], leadingUnderscore: 'allow' },
    // import 绑定：组件/类型 PascalCase 与常量 UPPER_CASE 均合法
    { selector: 'import', format: ['camelCase', 'PascalCase', 'UPPER_CASE'] },
    // 变量：camelCase / UPPER_CASE；PascalCase 变量仅限组件占位等白名单场景（存量逐点标注）
    { selector: 'variable', format: ['camelCase', 'UPPER_CASE'], leadingUnderscore: 'allowSingleOrDouble', trailingUnderscore: 'allowSingleOrDouble' },
    // 函数：严格 camelCase（拦截 C# 风格 PascalCase 函数，含声明式）
    { selector: 'function', format: ['camelCase'], leadingUnderscore: 'allow' },
    // type/interface/class/enum 名：PascalCase
    { selector: 'typeLike', format: ['PascalCase'] },
    // enum 成员：PascalCase / UPPER_CASE
    { selector: 'enumMember', format: ['PascalCase', 'UPPER_CASE'] },
    // 对象字面量键：数据契约面，不做格式约束
    { selector: 'objectLiteralProperty', format: null },
    // TS 类型属性：后端契约字段/kebab 事件名映射，不做格式约束
    { selector: 'typeProperty', format: null },
    // 类/对象属性：camelCase / UPPER_CASE（静态常量）
    { selector: 'property', format: ['camelCase', 'UPPER_CASE'], leadingUnderscore: 'allow' },
    // 函数参数：camelCase
    { selector: 'parameter', format: ['camelCase'], leadingUnderscore: 'allow' },
]

// ── vue/multi-word-component-names 惯例豁免块（GFX-700 / C287 建立，C296 / GFX-703 完成存量治理）──
// eslint-plugin-vue 该规则的文件名分支报告位置硬编码为 1:1（Program:exit），早于任何注释位置，
// 行内 eslint-disable 无法压制，故只能在此按精确路径冻结（精确路径而非 ignores 名称白名单：
// 同目录今后新增同名文件仍会被主规则拦截，防增量语义最严）。
// 命名治理已完成（C296 改名 132 个单词名/camelCase 文件），本清单仅剩 119 个约定豁免文件
// （117 个 index.vue 目录式组件 + 401/404 错误页），不再是 TODO；新增非 index.vue 文件必须 multi-word。
// 本清单只减不增（GFX-717 / C305）：与 scripts/frontend-standards-allowlist.json 的 componentNames
// 上限快照比对，新增条目未扩快照会被 pnpm run check:frontend 拦截；收缩后请同步删快照条目。
const frozenMultiWordComponentFiles = [
    'src/components/app-confirm-dialog/index.vue',
    'src/components/base-edit-dialog/index.vue',
    'src/components/config-app-scope-picker/index.vue',
    'src/components/image-cropper/index.vue',
    'src/components/OnlineCursorList/index.vue',
    'src/components/crud-data-table/index.vue',
    'src/components/rich-editor/index.vue',
    'src/components/form-help-text/index.vue',
    'src/components/form-label/index.vue',
    'src/components/icon-selector/index.vue',
    'src/components/narrow-screen-hint/index.vue',
    'src/components/notice-bar/index.vue',
    'src/components/reward-items-editor/index.vue',
    'src/components/search-form/index.vue',
    'src/components/svg-icon/index.vue',
    'src/components/simple-table/index.vue',
    'src/components/time-column/index.vue',
    'src/layout/footer/index.vue',
    'src/layout/index.vue',
    'src/layout/logo/index.vue',
    'src/layout/navigationBars/topBar/index.vue',
    'src/layout/upgrade/index.vue',
    'src/views/clientDelivery/remoteConfig/contentKeyConfig/index.vue',
    'src/views/clientDelivery/remoteConfig/index.vue',
    'src/views/clientDelivery/featureSwitch/index.vue',
    'src/views/clientDelivery/remoteConfig/modeConfig/index.vue',
    'src/views/clientDelivery/device/device/index.vue',
    'src/views/clientDelivery/device/group/index.vue',
    'src/views/clientDelivery/gameAppOpenAccess/index.vue',
    'src/views/clientDelivery/gameAppPackage/index.vue',
    'src/views/clientDelivery/gamePaymentConfigEntity/index.vue',
    'src/views/clientDelivery/release/appReleaseVersion/index.vue',
    'src/views/clientDelivery/release/assetPackageVersion/index.vue',
    'src/views/clientDelivery/release/versionDefinition/index.vue',
    'src/views/clientDelivery/resource/assetPackage/index.vue',
    'src/views/clientDelivery/serverConfig/index.vue',
    'src/views/clientDelivery/tenantAdvertisementCallbackConfigEntity/index.vue',
    'src/views/clientDelivery/tenantAppSignatureKeyEntity/index.vue',
    'src/views/clientDelivery/tenantSessionKeyEntity/index.vue',
    'src/views/clientDelivery/tenantChannelDefinition/index.vue',
    'src/views/clientDelivery/tenantClientApiPolicyEntity/index.vue',
    'src/views/clientDelivery/tenantGameAdvertisementRewardEntity/index.vue',
    'src/views/clientDelivery/tenantGameLoginTicketKeyEntity/index.vue',
    'src/views/system/gameTimeJobEntity/index.vue',
    'src/views/noticeCenter/announcement/gameAnnouncementEntity/index.vue',
    'src/views/noticeCenter/announcement/gameBannerAnnouncementEntity/index.vue',
    'src/views/noticeCenter/mail/gameMailEntity/index.vue',
    'src/views/noticeCenter/mail/gameMailTemplateCategoryEntity/index.vue',
    'src/views/noticeCenter/mail/gameMailTemplateEntity/index.vue',
    'src/views/noticeCenter/mail/mailCampaignEntity/index.vue',
    'src/views/noticeCenter/reward/gameRewardTemplateEntity/index.vue',
    'src/views/noticeCenter/reward/gameRewardTemplateGroupEntity/index.vue',
    'src/views/error/401.vue',
    'src/views/error/404.vue',
    'src/views/system/globalChannelDefinition/index.vue',
    'src/views/system/platformJwtConfig/index.vue',
    'src/views/system/platformLoggingConfig/index.vue',
    'src/views/system/platformPasswordPolicyConfig/index.vue',
    'src/views/system/platformRateLimitConfig/index.vue',
    'src/views/commerceCenter/gameCommodityEntity/index.vue',
    'src/views/commerceCenter/gameGoodsCatalogueEntity/index.vue',
    'src/views/commerceCenter/gameGoodsEntity/index.vue',
    'src/views/login/index.vue',
    'src/views/serverCenter/gameAreaClassifyEntity/index.vue',
    'src/views/serverCenter/gameAreaEntity/index.vue',
    'src/views/serverCenter/gameDockerTagEntity/index.vue',
    'src/views/commerceCenter/gameOrderEntity/index.vue',
    'src/views/commerceCenter/gameOrderFulfillmentEntity/index.vue',
    'src/views/playerCenter/gameRoleEntity/index.vue',
    'src/views/playerCenter/gameUserEntity/index.vue',
    'src/views/commerceCenter/gameRedeemCodeChannelEntity/index.vue',
    'src/views/commerceCenter/gameRedeemCodeEntity/index.vue',
    'src/views/commerceCenter/gameRedeemCodeUseRecordEntity/index.vue',
    'src/views/system/platformCaptchaConfig/index.vue',
    'src/views/system/config/index.vue',
    'src/views/system/database/index.vue',
    'src/views/system/dict/index.vue',
    'src/views/system/file/index.vue',
    'src/views/system/job/index.vue',
    'src/views/audit/logDiff/index.vue',
    'src/views/audit/logException/index.vue',
    'src/views/audit/logOperation/index.vue',
    'src/views/audit/logVisit/index.vue',
    'src/views/system/menu/index.vue',
    'src/views/system/openAccess/index.vue',
    'src/views/system/role/index.vue',
    'src/views/system/systemEmailProvider/index.vue',
    'src/views/system/tenantObjectStorageConfigEntity/index.vue',
    'src/views/system/user/index.vue',
    'src/views/playerCenter/antiCheatEventEntity/index.vue',
    'src/views/playerCenter/gamePenaltyAppealEntity/index.vue',
    'src/views/playerCenter/gamePenaltyEntity/index.vue',
    'src/views/playerCenter/gameReportEntity/index.vue',
    'src/views/playerCenter/penaltyEnforcement/index.vue',
    'src/views/commerceCenter/webShop/index.vue',
    'src/views/commerceCenter/webShopOrder/index.vue',
    'src/views/gmCenter/walletLedger/index.vue',
    'src/views/gmCenter/transactionDetail/index.vue',
    'src/views/gmCenter/assetAudit/index.vue',
    'src/views/gmCenter/onlineDashboard/index.vue',
    'src/views/gmCenter/onlinePlayerTimeline/index.vue',
    'src/views/gmCenter/onlineAuditSearch/index.vue',
    'src/views/gmCenter/controlledOperation/index.vue',
    'src/views/gmCenter/messageAudit/index.vue',
    'src/views/gmCenter/reportCase/index.vue',
    'src/views/gmCenter/matchQueueOverview/index.vue',
    'src/views/gmCenter/matchFailureReason/index.vue',
    'src/views/gmCenter/matchAbnormalTicket/index.vue',
    'src/views/gmCenter/onlinePlayerSegment/index.vue',
    'src/views/gmCenter/onlineConfigRollout/index.vue',
]

export default [
    {
        ignores: [
            'dist/**',
            'build/**',
            'node_modules/**',
            '.api-gen/**',
            'src/api-services/**',
            'src/auto-imports.d.ts',
            'src/components.d.ts',
        ],
    },
    {
        files: ['src/**/*.{js,jsx,ts,tsx}'],
        languageOptions: {
            parser: tsParser,
            ecmaVersion: 'latest',
            sourceType: 'module',
            globals: browserGlobals,
        },
        plugins: {
            '@typescript-eslint': tsPlugin,
        },
        rules: {
            'no-debugger': 'warn',
            'no-empty': ['warn', { allowEmptyCatch: true }],
            'no-unused-vars': 'off',
            // 禁止 handle 前缀 handler 声明（GFX-714）：统一 onXxx；存量已清零，此规则防增量
            'no-restricted-syntax': [
                'error',
                { selector: 'VariableDeclarator[id.name=/^handle[A-Z]/]', message: '事件 handler 统一 onXxx 前缀，禁止 handleXxx（GFX-714 / D5）' },
                { selector: 'FunctionDeclaration[id.name=/^handle[A-Z]/]', message: '事件 handler 统一 onXxx 前缀，禁止 handleXxx（GFX-714 / D5）' },
            ],
            '@typescript-eslint/no-unused-vars': ['warn', { argsIgnorePattern: '^_', varsIgnorePattern: '^_' }],
            '@typescript-eslint/naming-convention': namingConventionRule,
        },
    },
    {
        files: ['src/**/*.vue'],
        languageOptions: {
            parser: vueParser,
            parserOptions: {
                parser: tsParser,
                ecmaVersion: 'latest',
                sourceType: 'module',
                extraFileExtensions: ['.vue'],
            },
            globals: browserGlobals,
        },
        plugins: {
            '@typescript-eslint': tsPlugin,
            vue: vuePlugin,
        },
        rules: {
            'no-debugger': 'warn',
            'no-empty': ['warn', { allowEmptyCatch: true }],
            'no-unused-vars': 'off',
            '@typescript-eslint/no-unused-vars': ['warn', { argsIgnorePattern: '^_', varsIgnorePattern: '^_' }],
            // 组件名必须多词（GFX-700）：文件名分支报告位置硬编码 1:1，行内 eslint-disable 无法压制，
            // 存量违规由 frozenMultiWordComponentFiles 精确路径冻结；新文件一律受检。
            'vue/multi-word-component-names': 'error',
            // prop 传递统一 camelCase（GFX-714 / D6）：自定义组件属性禁止连字符；
            // 规则内置豁免 data-/aria-（HTML 标准 attribute）与 SVG 驼峰特例
            'vue/attribute-hyphenation': ['error', 'never'],
            'vue/no-mutating-props': 'warn',
            'vue/no-unused-vars': 'warn',
            '@typescript-eslint/naming-convention': namingConventionRule,
        },
    },
    {
        files: frozenMultiWordComponentFiles,
        rules: {
            'vue/multi-word-component-names': 'off',
        },
    },
]
