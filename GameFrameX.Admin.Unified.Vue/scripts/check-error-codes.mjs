#!/usr/bin/env node

/**
 * 错误码五语言码表校验脚本（C348 / GFX-751，C339 前端半边 S1.3-B1）
 *
 * 断言口径：
 * - Online 段（320～327，C339 八段镜像 vault 1xxx～8xxx）零差异 = 硬断言：
 *   五语言缺失 / 多余 / 空文案，以及 zh-cn / en 与 error-codes.json 的
 *   zh-CN / en-US 文案漂移，任一命中退出码 1。
 * - 全表差异 = 软报告：仅打印不阻断——前端既有 12 码滞后归 GFX-672 清偿，
 *   退出码不受非 Online 差异影响。
 *
 * 输入解析链同 sync-error-codes.mjs（--input / GFX_ERROR_CODES_JSON / 旁邻默认）。
 *
 * 用法：
 *   node scripts/check-error-codes.mjs
 *   node scripts/check-error-codes.mjs --input /path/error-codes.json
 */

import {readFileSync} from 'node:fs';
import {join, resolve} from 'node:path';
import {fileURLToPath} from 'node:url';

const __dirname = fileURLToPath(new URL('.', import.meta.url));
const REPO_ROOT = join(__dirname, '..');
const I18N_DIR = join(REPO_ROOT, 'src', 'i18n', 'pages', 'errors');
const LOCALES = ['zh-cn', 'zh-tw', 'en', 'ja', 'ko'];
const BACKEND_FIELD = {'zh-cn': 'zh-CN', 'en': 'en-US'};
// C339 段位登记：Online 八子段 320～327（OnlineSystem…OnlineTransient）
const ONLINE_SEGMENTS = new Set([320, 321, 322, 323, 324, 325, 326, 327]);
const segmentOf = (code) => Math.floor(Number(code) / 100000);
const isOnlineCode = (code) => ONLINE_SEGMENTS.has(segmentOf(code));

function resolveInputPath(flagValue) {
    if (flagValue) {
        return resolve(flagValue);
    }
    if (process.env.GFX_ERROR_CODES_JSON) {
        return resolve(process.env.GFX_ERROR_CODES_JSON);
    }
    return join(REPO_ROOT, '..', 'GameFrameX.Admin.Unified.Api', 'error-codes.json');
}

/** 解析单语言 errors 文件的 codes 表（同 sync-error-codes.mjs 的读取口径） */
function parseErrorCodes(locale) {
    const filePath = join(I18N_DIR, `${locale}.ts`);
    let content = readFileSync(filePath, 'utf-8');
    content = content
        .replace(/\/\/.*$/gm, '')
        .replace(/\/\*[\s\S]*?\*\//g, '')
        .replace(/export\s+default\s*/, '');
    const trimmed = content.trim().replace(/;$/, '');
    const parsed = new Function(`return (${trimmed})`)();
    if (!parsed || !parsed.errors || !parsed.errors.codes) {
        console.error(`[check-error-codes] FAIL ${filePath} 缺少 errors.codes 结构`);
        process.exit(1);
    }
    return parsed.errors.codes;
}

// ── 参数与输入 ────────────────────────────────────────────

const inputFlagIndex = process.argv.indexOf('--input');
const inputPath = resolveInputPath(inputFlagIndex !== -1 ? process.argv[inputFlagIndex + 1] : null);
let backend;
try {
    backend = JSON.parse(readFileSync(inputPath, 'utf-8'));
} catch (error) {
    console.error(`[check-error-codes] FAIL 读取 error-codes.json 失败（${inputPath}）：${error.message}`);
    console.error('[check-error-codes] 请先在 Api 仓跑 dotnet test --filter ErrorCodeContractTests 刷新导出，或用 --input 指向有效文件');
    process.exit(1);
}

const tables = {};
for (const locale of LOCALES) {
    tables[locale] = parseErrorCodes(locale);
}

const backendCodes = Object.keys(backend);
const onlineBackend = backendCodes.filter(isOnlineCode);
const hardFindings = [];

// ── 硬断言：Online 段零差异（五语言） ──────────────────────

for (const locale of LOCALES) {
    const codes = tables[locale];
    const onlineFrontend = Object.keys(codes).filter(isOnlineCode);
    const missing = onlineBackend.filter((code) => !Object.prototype.hasOwnProperty.call(codes, code));
    const extra = onlineFrontend.filter((code) => !Object.prototype.hasOwnProperty.call(backend, code));
    const empty = onlineFrontend.filter((code) => !String(codes[code]).trim());

    for (const code of missing) {
        hardFindings.push(`[Online] ${locale} 缺后端码 ${code}（${backend[code]['zh-CN']}）`);
    }
    for (const code of extra) {
        hardFindings.push(`[Online] ${locale} 多出后端不存在的码 ${code}（${codes[code]}）`);
    }
    for (const code of empty) {
        hardFindings.push(`[Online] ${locale} 的 ${code} 文案为空`);
    }

    // zh-cn / en 与后端权威双文逐码比对（繁/日/韩无后端基准，仅查存在与空文案）
    if (BACKEND_FIELD[locale]) {
        const field = BACKEND_FIELD[locale];
        for (const code of onlineBackend) {
            if (Object.prototype.hasOwnProperty.call(codes, code) && codes[code] !== backend[code][field]) {
                hardFindings.push(`[Online] ${locale} 的 ${code} 文案漂移：前端「${codes[code]}」≠ 后端「${backend[code][field]}」`);
            }
        }
    }
}

// ── 软报告：全表差异（不阻断） ────────────────────────────

const softLines = [];
for (const locale of ['zh-cn', 'en']) {
    const field = BACKEND_FIELD[locale];
    const codes = tables[locale];
    const missing = Object.keys(backend).filter((code) => !Object.prototype.hasOwnProperty.call(codes, code));
    const extra = Object.keys(codes).filter((code) => !Object.prototype.hasOwnProperty.call(backend, code));
    const drifted = Object.keys(backend).filter((code) =>
        Object.prototype.hasOwnProperty.call(codes, code) && codes[code] !== backend[code][field]);

    softLines.push(`[全表] ${locale}：缺 ${missing.length} 码 / 多 ${extra.length} 码 / 文案漂移 ${drifted.length} 码`);
    for (const code of missing) {
        softLines.push(`    缺 ${code} ${backend[code][field]}`);
    }
    for (const code of extra) {
        softLines.push(`    多 ${code} ${codes[code]}`);
    }
    for (const code of drifted) {
        softLines.push(`    漂移 ${code}：前端「${codes[code]}」≠ 后端「${backend[code][field]}」`);
    }
}
softLines.push('[全表] 说明：非 Online 差异仅报告不阻断；既有滞后由全量同步（node scripts/sync-error-codes.mjs，需先补齐 overlay 翻译）清偿');

// ── 结论 ─────────────────────────────────────────────────

console.log(`[check-error-codes] 输入=${inputPath}（后端 ${backendCodes.length} 码，其中 Online ${onlineBackend.length} 码）`);
console.log(softLines.join('\n'));

if (hardFindings.length > 0) {
    console.error(`[check-error-codes] FAIL Online 段零差异断言未过（${hardFindings.length} 处）：`);
    for (const finding of hardFindings) {
        console.error(`    ${finding}`);
    }
    process.exit(1);
}

console.log('[check-error-codes] PASS：Online 段（320-327）五语言与后端码表零差异');
