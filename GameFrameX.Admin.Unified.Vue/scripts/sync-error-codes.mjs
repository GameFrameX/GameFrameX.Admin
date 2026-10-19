#!/usr/bin/env node

/**
 * 错误码五语言码表同步脚本（C348 / GFX-751，C339 前端半边 S1.3-B1）
 *
 * 读 Api 仓 ErrorCodeContractTests 导出的 error-codes.json，重写五语言
 * src/i18n/pages/errors/{zh-cn,zh-tw,en,ja,ko}.ts 的 codes 段：
 * - zh-cn / en：取 error-codes.json 的 zh-CN / en-US（后端权威）；
 * - zh-tw / ja / ko：取 scripts/error-codes-translations.json 翻译 overlay，
 *   缺失即列出全清单并退出非 0——不静默回退英文，翻译必须显式登记。
 *
 * 输入解析链：--input <path> > 环境变量 GFX_ERROR_CODES_JSON > 仓库根旁邻
 * ../GameFrameX.Admin.Unified.Api/error-codes.json（主 checkout 布局）。
 * error-codes.json 是 Api 仓 gitignored 测试产物：先在 Api 仓跑
 * `dotnet test GameFrameX.Core.Tests --filter ErrorCodeContractTests` 刷新导出，
 * worktree 布局下再用 --input / 环境变量指向它。
 *
 * 段位限定：--segments "320-327"（逗号分隔 3 位段号或闭区间）。限定后仅该段内
 * 前端=后端（新增/更新/删除），段外条目原样保留——用于增量落地不清偿全表滞后；
 * 不带 --segments 即全量同步（既有 12 码滞后归 GFX-672 全量清偿时使用）。
 *
 * 用法：
 *   node scripts/sync-error-codes.mjs                        # 全量同步
 *   node scripts/sync-error-codes.mjs --segments 320-327     # 仅 Online 八段（C339）
 *   node scripts/sync-error-codes.mjs --input /path/error-codes.json
 */

import {readFileSync, writeFileSync} from 'node:fs';
import {join, resolve} from 'node:path';
import {fileURLToPath} from 'node:url';

const __dirname = fileURLToPath(new URL('.', import.meta.url));
const REPO_ROOT = join(__dirname, '..');
const I18N_DIR = join(REPO_ROOT, 'src', 'i18n', 'pages', 'errors');
const OVERLAY_PATH = join(__dirname, 'error-codes-translations.json');
const LOCALES = ['zh-cn', 'zh-tw', 'en', 'ja', 'ko'];
// 后端 json 只有中英双文；繁/日/韩走 overlay
const BACKEND_FIELD = {'zh-cn': 'zh-CN', 'en': 'en-US'};
const OVERLAY_LOCALES = LOCALES.filter((locale) => BACKEND_FIELD[locale] === undefined);

function fail(message) {
    console.error(`[sync-error-codes] FAIL ${message}`);
    process.exit(1);
}

// ── 参数解析 ──────────────────────────────────────────────

function parseArgs(argv) {
    const args = {input: null, segments: null};
    for (let i = 0; i < argv.length; i++) {
        if (argv[i] === '--input') {
            args.input = argv[++i];
            if (!args.input) {
                fail('--input 缺少路径参数');
            }
        } else if (argv[i] === '--segments') {
            args.segments = argv[++i];
            if (!args.segments) {
                fail('--segments 缺少段位参数（示例 320-327）');
            }
        } else {
            fail(`未知参数：${argv[i]}（支持 --input <path> / --segments <spec>）`);
        }
    }
    return args;
}

function parseSegmentSpec(spec) {
    const segments = new Set();
    for (const part of spec.split(',')) {
        const bounds = part.split('-').map((piece) => piece.trim());
        const valid = bounds.every((piece) => /^\d{1,3}$/.test(piece));
        if (bounds.length === 1 && valid) {
            segments.add(Number(bounds[0]));
        } else if (bounds.length === 2 && valid && Number(bounds[0]) <= Number(bounds[1])) {
            for (let segment = Number(bounds[0]); segment <= Number(bounds[1]); segment++) {
                segments.add(segment);
            }
        } else {
            fail(`--segments 段位格式非法：${part}（示例 320-327 或 320,321）`);
        }
    }
    return segments;
}

/** 错误码 = 段号(3 位)×100000 + 5 位序号（U1 规则），段号即前 3 位 */
const segmentOf = (code) => Math.floor(Number(code) / 100000);

// ── 输入解析 ──────────────────────────────────────────────

function resolveInputPath(flagValue) {
    if (flagValue) {
        return resolve(flagValue);
    }
    if (process.env.GFX_ERROR_CODES_JSON) {
        return resolve(process.env.GFX_ERROR_CODES_JSON);
    }
    // 主 checkout 布局：两仓同父目录；worktree 布局探测不到，走显式传参
    return join(REPO_ROOT, '..', 'GameFrameX.Admin.Unified.Api', 'error-codes.json');
}

// ── 五语言文件读写 ────────────────────────────────────────

/** 解析单语言 errors 文件为 codes 表；结构超出 errors.codes 单一形态即拒绝改写 */
function parseErrorTable(locale) {
    const filePath = join(I18N_DIR, `${locale}.ts`);
    let content = readFileSync(filePath, 'utf-8');
    content = content
        .replace(/\/\/.*$/gm, '')
        .replace(/\/\*[\s\S]*?\*\//g, '')
        .replace(/export\s+default\s*/, '');
    const trimmed = content.trim().replace(/;$/, '');
    let parsed;
    try {
        parsed = new Function(`return (${trimmed})`)();
    } catch (error) {
        fail(`${filePath} 解析失败：${error.message}`);
    }
    const topKeys = Object.keys(parsed || {});
    const errorsKeys = parsed && parsed.errors ? Object.keys(parsed.errors) : [];
    if (topKeys.length !== 1 || topKeys[0] !== 'errors' || errorsKeys.length !== 1 || errorsKeys[0] !== 'codes') {
        fail(`${filePath} 结构超出 errors.codes 单一形态（顶层键 ${JSON.stringify(topKeys)}），为避免破坏文件结构已中止`);
    }
    return {filePath, codes: parsed.errors.codes};
}

function quote(value) {
    return `'${String(value).replace(/\\/g, '\\\\').replace(/'/g, "\\'")}'`;
}

/** 按既有模板整文件重渲染（prettier：singleQuote / 4 空格 / trailingComma all） */
function renderFile(codes) {
    const entries = Object.keys(codes)
        .sort((a, b) => Number(a) - Number(b))
        .map((code) => `            ${quote(code)}: ${quote(codes[code])},`);
    return `export default {
    errors: {
        codes: {
${entries.join('\n')}
        },
    },
};
`;
}

// ── 主流程 ────────────────────────────────────────────────

const args = parseArgs(process.argv.slice(2));
const inputPath = resolveInputPath(args.input);
let backend;
try {
    backend = JSON.parse(readFileSync(inputPath, 'utf-8'));
} catch (error) {
    fail(`读取 error-codes.json 失败（${inputPath}）：${error.message}。请先在 Api 仓跑 dotnet test --filter ErrorCodeContractTests 刷新导出，或用 --input / GFX_ERROR_CODES_JSON 指向有效文件`);
}
const overlay = JSON.parse(readFileSync(OVERLAY_PATH, 'utf-8'));

const segments = args.segments ? parseSegmentSpec(args.segments) : null;
const scopedCodes = Object.keys(backend).filter((code) => segments === null || segments.has(segmentOf(code)));

// overlay 覆盖检查：将写入的每个码都必须有繁/日/韩翻译，零部分写入
const missingOverlay = [];
for (const code of scopedCodes) {
    for (const locale of OVERLAY_LOCALES) {
        const entry = overlay[code];
        const value = entry ? entry[locale] : undefined;
        if (typeof value !== 'string' || !value.trim()) {
            missingOverlay.push(`${code}.${locale}`);
        }
    }
}
if (missingOverlay.length > 0) {
    fail(`error-codes-translations.json 缺 ${missingOverlay.length} 条翻译：${missingOverlay.join(', ')}。请补齐 overlay 后重跑（不静默回退英文）`);
}

console.log(`[sync-error-codes] 输入=${inputPath}（后端 ${Object.keys(backend).length} 码）`);
console.log(`[sync-error-codes] 段位=${segments ? [...segments].join(',') : '全量'}`);

for (const locale of LOCALES) {
    const {filePath, codes: current} = parseErrorTable(locale);
    const next = {};
    let added = 0;
    let updated = 0;
    let removed = 0;

    // 段内删除：后端已移除的码前端同步移除；段外/全量外的既有条目原样保留
    for (const code of Object.keys(current)) {
        const scoped = segments === null || segments.has(segmentOf(code));
        if (scoped && !Object.prototype.hasOwnProperty.call(backend, code)) {
            removed++;
            continue;
        }
        next[code] = current[code];
    }

    for (const code of scopedCodes) {
        const texts = backend[code];
        const value = BACKEND_FIELD[locale] ? texts[BACKEND_FIELD[locale]] : overlay[code][locale];
        if (typeof value !== 'string' || !value.trim()) {
            fail(`${code} 的 ${locale} 文案为空`);
        }
        if (!Object.prototype.hasOwnProperty.call(next, code)) {
            added++;
        } else if (next[code] !== value) {
            updated++;
        }
        next[code] = value;
    }

    const rendered = renderFile(next);
    if (rendered === readFileSync(filePath, 'utf-8')) {
        console.log(`[sync-error-codes] ${locale}.ts：无变化`);
    } else {
        writeFileSync(filePath, rendered, 'utf-8');
        console.log(`[sync-error-codes] ${locale}.ts：+${added} ~${updated} -${removed}（已写入）`);
    }
}

console.log('[sync-error-codes] 完成');
