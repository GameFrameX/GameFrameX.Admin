/**
 * swagger-typescript-api 程序化生成脚本
 *
 * 用 spec 对象直接传入 generateApi，绕开 stc CLI 13.x 把本地路径当 URL 解析的问题
 * （CLI 的 -p 在 13.x 被映射成 url，沙箱禁网时无法 fetch）。
 *
 * stc 输出中间产物到 .api-gen/（扁平：data-contracts.ts + http-client.ts + 各 <Tag>.ts），
 * 随后由 post-gen.mjs 重组到最终结构 src/api/<tag-camel>/{index,types}.ts + _shared/。
 *
 * 用法：pnpm gen:api
 */
import {readFileSync} from 'node:fs';
import {resolve, dirname} from 'node:path';
import {fileURLToPath} from 'node:url';
import {generateApi} from 'swagger-typescript-api';

const __dirname = dirname(fileURLToPath(import.meta.url));
const projectRoot = resolve(__dirname, '..');

const outputDir = resolve(projectRoot, '.api-gen');
const schemaPath = resolve(projectRoot, 'openapi.json');

const spec = JSON.parse(readFileSync(schemaPath, 'utf8'));

// C201：字段名归一化。后端 System.Text.Json PropertyNamingPolicy=null → JSON 字段为 PascalCase，
// 前端 axios 响应拦截器（src/utils/http/shared.ts 的 normalizeResponseKeys/toCamelCaseKey）
// 会把所有响应键递归转 camelCase 后才交给业务代码。故生成类型必须用 camelCase 才与运行时一致。
// 在 spec 传给 generateApi 之前，递归把每个 JSON Schema 的 properties 键 + required 数组转 camelCase。
normalizeSpecFieldNames(spec);

const result = await generateApi({
    spec,
    output: outputDir,
    // 分文件：HttpClient / data-contracts / 各 tag 模块
    modular: true,
    httpClientType: 'axios',
    moduleNameFirstTag: true,
    extractRequestParams: false,
    extractRequestBody: true,
    extractResponseBody: true,
    singleHttpClient: true,
    unwrapResponseData: false,
    cleanOutput: true,
    silent: false,
});

const files = result.files ?? [];
console.log(`\n[gen-api] stc 已生成 ${files.length} 个文件 -> ${outputDir}`);

// 后处理：split（独占/共享类型）+ restructure（按 tag 分子目录到 src/api/<tag>/ + _shared/）
await import('./post-gen.mjs');

// ── C201：spec 字段名归一化 ──────────────────────────────────

/**
 * 字段名归一化规则：复刻前端 toCamelCaseKey（src/utils/http/shared.ts）。
 * 首字符大写则转小写，其余字符不变，保证生成类型与运行时 normalizeResponseKeys 结果逐字符一致。
 * 对已是 camelCase 的键（如 ApiResult wrapper 的 code/type/result）幂等。
 */
function normalizeFieldName(name) {
    if (typeof name !== 'string' || !name) return name;
    if (name[0] !== name[0].toUpperCase()) return name;
    return name[0].toLowerCase() + name.slice(1);
}

/**
 * 递归把 OpenAPI 规约里所有 JSON Schema 的 properties 键、required 数组与
 * operation parameters[].name 转 camelCase。
 * 不动 $ref（类型引用）、enum 值（运行时数据）、路径模板（本就是 camelCase）。
 * parameters[].name 同样归一化：后端 action 参数导出为 PascalCase（Id/Title/PageSize…），
 * 归一化后 query 内联类型字段为 camelCase，与 properties 口径一致（C304）。
 */
function normalizeSpecFieldNames(node) {
    if (Array.isArray(node)) {
        for (let i = 0; i < node.length; i++) {
            node[i] = normalizeSpecFieldNames(node[i]);
        }
        return node;
    }
    if (node && typeof node === 'object') {
        if (node.properties && typeof node.properties === 'object' && !Array.isArray(node.properties)) {
            const renamed = {};
            for (const [k, v] of Object.entries(node.properties)) {
                renamed[normalizeFieldName(k)] = normalizeSpecFieldNames(v);
            }
            node.properties = renamed;
        }
        if (Array.isArray(node.required)) {
            node.required = node.required.map((r) => normalizeFieldName(r));
        }
        if (Array.isArray(node.parameters)) {
            for (const param of node.parameters) {
                if (param && typeof param === 'object' && typeof param.name === 'string') {
                    param.name = normalizeFieldName(param.name);
                }
            }
        }
        for (const key of Object.keys(node)) {
            if (key === 'properties' || key === 'required' || key === 'parameters') continue;
            node[key] = normalizeSpecFieldNames(node[key]);
        }
        return node;
    }
    return node;
}
