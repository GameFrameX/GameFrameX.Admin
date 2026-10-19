/**
 * VC-1.10 Admin 前端半边断言（C348 / GFX-751）：未知 Online 码回退后端 message。
 *
 * 被测对象是真实 src/utils/http/shared.ts 的 resolveResponseMessage。该模块依赖
 * 浏览器环境（storage/message）与 vite 专属的 import.meta.glob（i18n index），
 * 无法在 node 下直接 import，因此用 module customization hook 把它的五个值导入
 * 替换为 data: URL 桩；其中 t/te 桩在调用时读取 globalThis 上由本文件注入的真实
 * 五语言 errors 码表（键形态 errors.codes.<code>，与 vue-i18n 平表查找一致），
 * 保证断言覆盖的是「真实数据 + 真实解析逻辑」，而非复刻实现。
 *
 * 运行：pnpm run test:unit（node --test，node ≥24 原生 type stripping）。
 */
import {test} from 'node:test';
import assert from 'node:assert/strict';
import {register} from 'node:module';

import zhCn from '../../i18n/pages/errors/zh-cn.ts';
import zhTw from '../../i18n/pages/errors/zh-tw.ts';
import en from '../../i18n/pages/errors/en.ts';
import ja from '../../i18n/pages/errors/ja.ts';
import ko from '../../i18n/pages/errors/ko.ts';

// ── i18n 桩状态：t/te 调用时读取，locale 可切换 ─────────────

const i18nState = {
    locale: 'zh-cn',
    messages: {'zh-cn': zhCn, 'zh-tw': zhTw, en, ja, ko},
};
globalThis.__onlineErrorI18nState = i18nState;

const dataUrl = (source) => `data:text/javascript,${encodeURIComponent(source)}`;

// shared.ts 的五个值导入 → 桩（axios 为 import type，会被 type stripping 擦除）
const stubModules = {
    '../storage': dataUrl(`export const local = {get: () => null, set() {}, remove() {}};
export const session = {clear() {}};`),
    '../formatTime': dataUrl(`export const formatDate = () => '';`),
    '../message': dataUrl(`export const showError = () => {};`),
    '../targetTenant': dataUrl(`export const TARGET_TENANT_ID_HEADER = 'x-target-tenant-id';
export const getTargetTenantId = () => null;
export const clearTargetTenantId = () => {};`),
    '../../i18n/index': dataUrl(`
const lookup = (key) => {
    const state = globalThis.__onlineErrorI18nState;
    let node = state.messages[state.locale];
    for (const part of key.split('.')) {
        if (node === null || typeof node !== 'object') {
            return undefined;
        }
        node = node[part];
    }
    return node;
};
export const te = (key) => typeof lookup(key) === 'string';
export const t = (key) => {
    const value = lookup(key);
    return typeof value === 'string' ? value : key;
};`),
};

register(
    dataUrl(`
export function resolve(specifier, context, nextResolve) {
    const stubs = ${JSON.stringify(stubModules)};
    if (Object.prototype.hasOwnProperty.call(stubs, specifier)) {
        return {url: stubs[specifier], shortCircuit: true};
    }
    return nextResolve(specifier, context);
}`),
    import.meta.url,
);

const {resolveResponseMessage} = await import('./shared.ts');

// ── 断言数据 ──────────────────────────────────────────────

// C339 Online 八段（320～327）镜像 vault 1xxx～8xxx；段号 = 前 3 位
const isOnlineCode = (code) => Math.floor(Number(code) / 100000) >= 320 && Math.floor(Number(code) / 100000) <= 327;
const onlineCodes = Object.keys(zhCn.errors.codes).filter((code) => isOnlineCode(code));
// 未知码样本：Online 段内但不在码表（模拟后端比前端新的场景，如新码未同步）
const unknownOnlineCodes = [32099999, 32512345, 32700099];
const BACKEND_MESSAGE = '后端原文 message（测试哨兵）';

test('已知 Online 码在五语言全部命中本地化文案（C339 同步落地证据）', () => {
    assert.ok(onlineCodes.length >= 20, `Online 码数量异常：${onlineCodes.length}（预期 ≥20，同步是否已执行？）`);
    for (const locale of Object.keys(i18nState.messages)) {
        i18nState.locale = locale;
        const table = i18nState.messages[locale].errors.codes;
        for (const code of onlineCodes) {
            const result = resolveResponseMessage({code: Number(code), message: BACKEND_MESSAGE});
            assert.ok(
                result && result !== BACKEND_MESSAGE,
                `${locale} 的 ${code} 未走本地化路径（返回「${result}」），码表同步疑似缺失`,
            );
            assert.equal(result, table[code], `${locale} 的 ${code} 本地化文案与码表不一致`);
        }
    }
});

test('未知 Online 码五语言键未命中 → resolveResponseMessage 回退后端 message 原文（VC-1.10）', () => {
    for (const locale of Object.keys(i18nState.messages)) {
        i18nState.locale = locale;
        for (const code of unknownOnlineCodes) {
            assert.equal(
                resolveResponseMessage({code, message: BACKEND_MESSAGE}),
                BACKEND_MESSAGE,
                `${locale} 的未知 Online 码 ${code} 应回退后端 message 原文`,
            );
        }
    }
});

test('未知 Online 码 + 空后端 message → 返回空串，不虚构成功文案（不误判成功）', () => {
    for (const locale of Object.keys(i18nState.messages)) {
        i18nState.locale = locale;
        for (const code of unknownOnlineCodes) {
            assert.equal(
                resolveResponseMessage({code, message: ''}),
                '',
                `${locale} 的未知 Online 码 ${code} 不应虚构展示文案`,
            );
        }
    }
});
