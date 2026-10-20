#!/usr/bin/env node

import { existsSync, readdirSync, readFileSync, writeFileSync } from 'node:fs'
import { basename, dirname, extname, join, relative, sep } from 'node:path'
import { fileURLToPath } from 'node:url'
import { execFileSync } from 'node:child_process'

const __dirname = dirname(fileURLToPath(import.meta.url))
const ROOT = join(__dirname, '..')
const SRC_ROOT = join(ROOT, 'src')
const BACKEND_ROOT = join(ROOT, '..', 'GameFrameX.Admin.Unified.Api')
const MENU_SEED_FILE = join(BACKEND_ROOT, 'GameFrameX.Core', 'Const', 'MenuSeed', 'MenuSeedDefinitions.cs')
const GLOBAL_MENU_SEED_DIR = join(BACKEND_ROOT, 'GameFrameX.Application', 'SeedData', 'System')
const GLOBAL_MENU_SEED_FILE = join(GLOBAL_MENU_SEED_DIR, 'GlobalMenuSeedData.cs')
const I18N_ROOT = join(SRC_ROOT, 'i18n')
const ALLOWLIST_FILE = join(__dirname, 'frontend-standards-allowlist.json')
const I18N_TEXT_BASELINE_FILE = join(__dirname, 'i18n-text-baseline.json')
const LOCALES = ['zh-cn', 'en', 'zh-tw', 'ja', 'ko']
const BASE_LOCALE = 'zh-cn'
const MODE = process.argv[2] ?? 'all'

function readText(file) {
    return readFileSync(file, 'utf8')
}

function walk(dir, predicate = () => true) {
    const results = []
    if (!existsSync(dir)) return results
    for (const entry of readdirSync(dir, { withFileTypes: true })) {
        const fullPath = join(dir, entry.name)
        if (entry.isDirectory()) {
            if (entry.name === 'node_modules' || entry.name === 'dist' || entry.name === 'build') continue
            results.push(...walk(fullPath, predicate))
        } else if (entry.isFile() && predicate(fullPath)) {
            results.push(fullPath)
        }
    }
    return results
}

function rel(file) {
    return relative(ROOT, file).split(sep).join('/')
}

function normalizePathPart(value) {
    return value.replace(/\\/g, '/').replace(/^\.\//, '').replace(/^\/+/, '').replace(/\.(vue|tsx|ts|js)$/, '')
}

function unquote(value) {
    const trimmed = value.trim()
    if (trimmed === 'null') return null
    if (!/^["']/.test(trimmed)) return trimmed
    return trimmed.slice(1, -1).replace(/\\"/g, '"').replace(/\\'/g, "'").replace(/\\\\/g, '\\')
}

function splitArgs(raw) {
    const args = []
    let current = ''
    let quote = null
    let escaped = false
    let depth = 0

    for (const char of raw) {
        if (escaped) {
            current += char
            escaped = false
            continue
        }
        if (char === '\\') {
            current += char
            escaped = true
            continue
        }
        if (quote) {
            current += char
            if (char === quote) quote = null
            continue
        }
        if (char === '"' || char === "'") {
            current += char
            quote = char
            continue
        }
        if (char === '(') depth++
        if (char === ')') depth--
        if (char === ',' && depth === 0) {
            args.push(current.trim())
            current = ''
            continue
        }
        current += char
    }
    if (current.trim()) args.push(current.trim())
    return args
}

function parseMenuSeedDefinitions(content) {
    const marker = 'new MenuSeedDefinition('
    const definitions = []
    let index = 0

    while ((index = content.indexOf(marker, index)) !== -1) {
        let cursor = index + marker.length
        let depth = 1
        let quote = null
        let escaped = false
        let raw = ''

        for (; cursor < content.length; cursor++) {
            const char = content[cursor]
            if (escaped) {
                raw += char
                escaped = false
                continue
            }
            if (char === '\\') {
                raw += char
                escaped = true
                continue
            }
            if (quote) {
                raw += char
                if (char === quote) quote = null
                continue
            }
            if (char === '"' || char === "'") {
                raw += char
                quote = char
                continue
            }
            if (char === '(') depth++
            if (char === ')') {
                depth--
                if (depth === 0) break
            }
            raw += char
        }

        const args = splitArgs(raw)
        definitions.push({
            key: unquote(args[0] ?? ''),
            parentKey: unquote(args[1] ?? ''),
            type: args[2] ?? '',
            name: unquote(args[3] ?? ''),
            path: unquote(args[4] ?? ''),
            component: unquote(args[5] ?? ''),
            permission: unquote(args[7] ?? ''),
            title: unquote(args[8] ?? ''),
            titleCode: unquote(args[9] ?? ''),
        })
        index = cursor + 1
    }

    return definitions
}

function readInitializerValue(block, propertyName) {
    const match = block.match(new RegExp(`\\b${propertyName}\\s*=\\s*([^,\\n]+),`))
    if (!match) return null
    return unquote(match[1])
}

function parseGlobalMenuSeedDefinitions(content) {
    const definitions = []
    const pattern = /new(?:\s+GlobalMenu)?\s*\(\)\s*\{([\s\S]*?)\n\s*\},/g

    for (const match of content.matchAll(pattern)) {
        const block = match[1]
        const key = readInitializerValue(block, 'Key')
        if (!key || !key.includes(':')) continue
        definitions.push({
            key,
            parentKey: null,
            type: readInitializerValue(block, 'Type') ?? '',
            name: readInitializerValue(block, 'Name') ?? '',
            path: readInitializerValue(block, 'Path') ?? '',
            component: readInitializerValue(block, 'Component'),
            permission: readInitializerValue(block, 'Permission'),
            title: readInitializerValue(block, 'Title') ?? '',
            titleCode: readInitializerValue(block, 'TitleCode'),
        })
    }

    definitions.push(...parseGlobalMenuHelperDefinitions(content))
    return definitions
}

function parseGlobalMenuHelperDefinitions(content) {
    const definitions = []
    const pattern = /^\s*(Menu|Button)\s*\(/gm

    for (const match of content.matchAll(pattern)) {
        const functionName = match[1]
        const openParenIndex = content.indexOf('(', match.index)
        const raw = readCallArguments(content, openParenIndex)
        if (!raw) continue

        const args = splitArgs(raw)
        if (functionName === 'Menu') {
            const key = unquote(args[0] ?? '')
            if (!key || !key.includes(':')) continue
            const component = unquote(args[4] ?? '')
            definitions.push({
                key,
                parentKey: unquote(args[1] ?? ''),
                type: component ? 'MenuTypeEnum.Menu' : 'MenuTypeEnum.Dir',
                name: unquote(args[2] ?? ''),
                path: unquote(args[3] ?? ''),
                component,
                permission: null,
                title: unquote(args[5] ?? ''),
                titleCode: unquote(args[6] ?? ''),
            })
        } else {
            const key = unquote(args[0] ?? '')
            if (!key || !key.includes(':')) continue
            definitions.push({
                key,
                parentKey: unquote(args[1] ?? ''),
                type: 'MenuTypeEnum.Btn',
                name: '',
                path: null,
                component: null,
                permission: unquote(args[2] ?? ''),
                title: unquote(args[3] ?? ''),
                titleCode: null,
            })
        }
    }

    return definitions
}

function readCallArguments(content, openParenIndex) {
    let depth = 0
    let quote = null
    let escaped = false
    let raw = ''

    for (let index = openParenIndex; index < content.length; index++) {
        const char = content[index]
        if (escaped) {
            if (depth > 0) raw += char
            escaped = false
            continue
        }
        if (char === '\\') {
            if (depth > 0) raw += char
            escaped = true
            continue
        }
        if (quote) {
            raw += char
            if (char === quote) quote = null
            continue
        }
        if (char === '"' || char === "'") {
            raw += char
            quote = char
            continue
        }
        if (char === '(') {
            if (depth > 0) raw += char
            depth++
            continue
        }
        if (char === ')') {
            depth--
            if (depth === 0) return raw
            raw += char
            continue
        }
        if (depth > 0) raw += char
    }

    return null
}

function globalMenuSeedFiles() {
    if (!existsSync(GLOBAL_MENU_SEED_DIR)) return []
    return readdirSync(GLOBAL_MENU_SEED_DIR)
        .filter((name) => /^GlobalMenuSeedData.*\.cs$/.test(name))
        .sort()
        .map((name) => join(GLOBAL_MENU_SEED_DIR, name))
}

function loadMenuDefinitions() {
    if (existsSync(MENU_SEED_FILE)) return parseMenuSeedDefinitions(readText(MENU_SEED_FILE))
    const seedFiles = globalMenuSeedFiles()
    if (seedFiles.length > 0) return seedFiles.flatMap((file) => parseGlobalMenuSeedDefinitions(readText(file)))
    console.warn(`[routes] warning: backend repo not found at ${BACKEND_ROOT}. Skipping route/permission checks against menu seed.`)
    return null
}

function loadAllowlist() {
    if (!existsSync(ALLOWLIST_FILE)) return {}
    return JSON.parse(readText(ALLOWLIST_FILE))
}

function collectComponentModules() {
    const files = [
        ...walk(join(SRC_ROOT, 'views'), (file) => /\.(vue|tsx)$/.test(file)),
        ...walk(join(SRC_ROOT, 'layout', 'routerView'), (file) => /\.(vue|tsx)$/.test(file)),
    ]
    return files.map((file) => {
        const fromSrc = normalizePathPart(relative(SRC_ROOT, file).split(sep).join('/'))
        const fromViews = fromSrc.startsWith('views/') ? fromSrc.slice('views/'.length) : fromSrc
        return {
            file,
            rel: rel(file),
            keys: new Set([fromSrc, `/${fromSrc}`, fromViews, `/${fromViews}`]),
        }
    })
}

function matchComponent(component, modules) {
    const normalized = normalizePathPart(component)
    const candidates = modules.filter((module) => {
        for (const key of module.keys) {
            const k = normalizePathPart(key)
            if (k === normalized || k.startsWith(`${normalized}/`)) return true
        }
        return false
    })
    return candidates
}

function parseTsObject(filePath) {
    let content = readText(filePath)
    content = content.replace(/\/\/.*$/gm, '')
    content = content.replace(/\/\*[\s\S]*?\*\//g, '')
    content = content.replace(/export\s+default\s*/, '')
    const trimmed = content.trim().replace(/;$/, '')
    if (!trimmed || trimmed === '{}') return {}
    return new Function(`return (${trimmed})`)()
}

function extractKeyPaths(obj, prefix = '') {
    const paths = []
    for (const key of Object.keys(obj)) {
        const fullPath = prefix ? `${prefix}.${key}` : key
        if (obj[key] !== null && typeof obj[key] === 'object' && !Array.isArray(obj[key])) {
            paths.push(...extractKeyPaths(obj[key], fullPath))
        } else {
            paths.push(fullPath)
        }
    }
    return paths
}

function collectI18nKeys() {
    const localeKeys = Object.fromEntries(LOCALES.map((locale) => [locale, new Set()]))
    const files = walk(I18N_ROOT, (file) => file.endsWith('.ts'))
    for (const file of files) {
        const locale = basename(file, '.ts')
        if (!LOCALES.includes(locale)) continue
        for (const key of extractKeyPaths(parseTsObject(file))) {
            localeKeys[locale].add(key)
            localeKeys[locale].add(`message.${key}`)
        }
    }
    return localeKeys
}

function checkRoutes() {
    const definitions = loadMenuDefinitions()
    if (definitions === null) return true
    const modules = collectComponentModules()
    const i18nKeys = collectI18nKeys()[BASE_LOCALE]
    const allowlist = loadAllowlist().routeComponents ?? {}
    const failures = []
    const allowed = []

    for (const definition of definitions) {
        const component = definition.component
        if (!component || component === 'Layout') continue
        if (!definition.type.includes('MenuTypeEnum.Menu') && !definition.type.includes('MenuTypeEnum.Dir')) continue
        const matches = matchComponent(component, modules)
        if (matches.length !== 1) {
            const allowReason = allowlist[definition.key]
            if (allowReason) {
                allowed.push(`${definition.key}: component "${component}" matched ${matches.length} files (${allowReason})`)
            } else {
                failures.push(`${definition.key}: component "${component}" matched ${matches.length} files`)
            }
        }
        if (!definition.titleCode) {
            failures.push(`${definition.key}: missing titleCode`)
        } else if (!i18nKeys.has(definition.titleCode)) {
            failures.push(`${definition.key}: titleCode "${definition.titleCode}" is not in ${BASE_LOCALE} i18n keys`)
        }
    }

    printResult('routes', failures, [
        `checked ${definitions.length} menu seed definitions`,
        `checked ${modules.length} component modules`,
        ...allowed.map((item) => `allowed: ${item}`),
    ])
    return failures.length === 0
}

function extractTagBlocks(content, tagName) {
    const blocks = []
    const pattern = new RegExp(`<${tagName}\\b[\\s\\S]*?(?:/>|</${tagName}>)`, 'g')
    for (const match of content.matchAll(pattern)) blocks.push(match[0])
    return blocks
}

function readAttr(block, names) {
    for (const name of names) {
        const pattern = new RegExp(`\\b${name}=["']([^"']+)["']`)
        const match = block.match(pattern)
        if (match) return match[1]
    }
    return null
}

function hasTrueAttr(block, names) {
    return names.some((name) => new RegExp(`\\b${name}=["']true["']|\\b:${name}=["']true["']`).test(block))
}

function extractActionKeys(content, variableName) {
    if (!variableName) return []
    const pattern = new RegExp(`const\\s+${variableName}\\s*[:\\w\\[\\]<>\\s]*=\\s*\\{([\\s\\S]*?)\\n\\}`, 'm')
    const match = content.match(pattern)
    if (!match) return []
    const body = match[1]
    const keys = []
    if (/\bdetail\s*:/.test(body)) keys.push('detail')
    if (/\bedit\s*:/.test(body)) keys.push('update')
    if (/\bdelete\s*:/.test(body)) keys.push('delete')
    return keys
}

function extractCustomActionKeys(content, variableName) {
    if (!variableName) return []
    const pattern = new RegExp(`const\\s+${variableName}\\s*[:\\w\\[\\]<>\\s]*=\\s*\\[([\\s\\S]*?)\\n\\]`, 'm')
    const match = content.match(pattern)
    if (!match) return []
    const keys = []
    const authPattern = /\bauth:\s*['"]([A-Za-z][A-Za-z0-9_]*)['"]/g
    for (const authMatch of match[1].matchAll(authPattern)) keys.push(authMatch[1])
    return keys
}

function collectPermissionDeclarations() {
    const files = walk(SRC_ROOT, (file) => /\.(vue|ts|tsx)$/.test(file) && !file.includes(`${sep}api-services${sep}`))
    const permissions = new Map()

    function add(permission, file, source) {
        if (!/^[A-Za-z][A-Za-z0-9_]*:[A-Za-z][A-Za-z0-9_]*$/.test(permission)) return
        if (!permissions.has(permission)) permissions.set(permission, [])
        permissions.get(permission).push(`${rel(file)} (${source})`)
    }

    for (const file of files) {
        const content = readText(file)
        for (const line of content.split('\n')) {
            const trimmed = line.trim()
            if (trimmed.startsWith('*') || trimmed.startsWith('//')) continue
            if (!/(v-auth|auth\(|hasAnyAuth\(|queryAuth|addAuth)/.test(line)) continue
            const literalPattern = /['"]([A-Za-z][A-Za-z0-9_]*:[A-Za-z][A-Za-z0-9_]*)['"]/g
            for (const match of line.matchAll(literalPattern)) {
                add(match[1], file, 'literal')
            }
        }

        for (const block of extractTagBlocks(content, 'SearchForm')) {
            const prefix = readAttr(block, ['authPrefix', 'auth-prefix'])
            if (!prefix) continue
            add(`${prefix}:page`, file, 'SearchForm')
            if (hasTrueAttr(block, ['showAdd', 'show-add'])) add(`${prefix}:add`, file, 'SearchForm')
            for (const attr of ['queryAuth', 'query-auth', 'addAuth', 'add-auth']) {
                const permission = readAttr(block, [attr])
                if (permission) add(permission, file, attr)
            }
        }

        for (const block of extractTagBlocks(content, 'CrudDataTable')) {
            const prefix = readAttr(block, ['authPrefix', 'auth-prefix'])
            if (!prefix) continue
            const actionsVariable = readAttr(block, [':actions'])
            for (const action of extractActionKeys(content, actionsVariable)) {
                add(`${prefix}:${action}`, file, 'CrudDataTable.actions')
            }
            const customActionsVariable = readAttr(block, [':custom-actions', ':customActions'])
            for (const action of extractCustomActionKeys(content, customActionsVariable)) {
                add(`${prefix}:${action}`, file, 'CrudDataTable.customActions')
            }
        }

        const explicitAuthPattern = /\b(?:queryAuth|addAuth)=["']([A-Za-z][A-Za-z0-9_]*:[A-Za-z][A-Za-z0-9_]*)["']/g
        for (const match of content.matchAll(explicitAuthPattern)) {
            add(match[1], file, 'explicitAuth')
        }
    }
    return permissions
}

function checkPermissions() {
    const definitions = loadMenuDefinitions()
    if (definitions === null) return true
    const allowlist = loadAllowlist().permissions ?? {}
    const seedPermissions = new Set(
        definitions
            .filter((definition) => definition.type === 'MenuTypeEnum.Btn' || definition.type === 'MenuTypeEnum.Button')
            .map((definition) => definition.key)
            .filter((permission) => permission && !permission.startsWith('route:')),
    )
    const declarations = collectPermissionDeclarations()
    const failures = []
    const allowed = []

    for (const [permission, locations] of [...declarations.entries()].sort(([a], [b]) => a.localeCompare(b))) {
        if (!seedPermissions.has(permission)) {
            const allowReason = allowlist[permission]
            if (allowReason) {
                allowed.push(`${permission}: first usage ${locations[0]} (${allowReason})`)
            } else {
                failures.push(`${permission}: missing from menu seed; first usage ${locations[0]}`)
            }
        }
    }

    printResult('permissions', failures, [
        `checked ${declarations.size} frontend permission declarations`,
        `checked ${seedPermissions.size} seeded permissions`,
        ...allowed.map((item) => `allowed: ${item}`),
    ])
    return failures.length === 0
}

function normalizeApiName(file) {
    return basename(file, extname(file))
        .replace(/-api$/, '')
        .replace(/[^a-zA-Z0-9]/g, '')
        .toLowerCase()
}

function lineNumberAt(content, index) {
    return content.slice(0, index).split('\n').length
}

function readObjectBlock(content, openBraceIndex) {
    let depth = 0
    let quote = null
    let escaped = false

    for (let index = openBraceIndex; index < content.length; index++) {
        const char = content[index]
        if (escaped) {
            escaped = false
            continue
        }
        if (char === '\\') {
            escaped = true
            continue
        }
        if (quote) {
            if (char === quote) quote = null
            continue
        }
        if (char === '"' || char === "'" || char === '`') {
            quote = char
            continue
        }
        if (char === '{') depth++
        if (char === '}') {
            depth--
            if (depth === 0) return content.slice(openBraceIndex, index + 1)
        }
    }
    return null
}

function previousOperationName(content, index) {
    const prefix = content.slice(Math.max(0, index - 240), index)
    const patterns = [
        /export\s+const\s+([A-Za-z][A-Za-z0-9_]*)[\s\S]*?=>\s*$/,
        /const\s+([A-Za-z][A-Za-z0-9_]*)[\s\S]*?=>\s*$/,
        /([A-Za-z][A-Za-z0-9_]*)\s*:\s*\([^)]*\)\s*=>\s*$/,
    ]
    for (const pattern of patterns) {
        const match = prefix.match(pattern)
        if (match) return match[1]
    }
    return 'anonymous'
}

function isQueryOperationName(value) {
    return /(^|[A-Z_])(page|list|query|search|filter)([A-Z_]|$)/.test(value)
        || /^(page|list|query|search|filter)$/i.test(value)
}

function isGeneratedFilterGetCall(callName, args) {
    if (!/^api[A-Za-z0-9]*(Page|Query|Search|Filter)[A-Za-z0-9]*Get$/.test(callName)) {
        if (!/^api[A-Za-z0-9]*List[A-Za-z0-9]*Get$/.test(callName) || !/\bqueryParams\b/.test(args)) return false
    }
    return /\b(queryParams|searchParams|filterParams|tableParams|params)\b/.test(args)
}

function collectQueryGetRequests() {
    const findings = []
    const files = [
        ...walk(join(SRC_ROOT, 'api'), (file) => /\.(ts|tsx)$/.test(file)),
        ...walk(join(SRC_ROOT, 'views'), (file) => /\.(vue|ts|tsx)$/.test(file)),
    ]

    for (const file of files) {
        const content = readText(file)
        const requestPattern = /request\(\s*\{/g
        for (const match of content.matchAll(requestPattern)) {
            const objectStart = match.index + match[0].lastIndexOf('{')
            const block = readObjectBlock(content, objectStart)
            if (!block) continue
            if (!/\bmethod\s*:\s*['"]get['"]/i.test(block)) continue
            if (!/\b(params|data)\s*:/.test(block)) continue
            const operation = previousOperationName(content, match.index)
            const urlLine = block.match(/\burl\s*:\s*([^,\n]+)/)?.[1]?.trim() ?? ''
            if (!isQueryOperationName(operation) && !/\bApi\.(Page|List|Query|Search|Filter)\b/.test(urlLine) && !/\/(page|list|query|search|filter)\b/i.test(urlLine)) continue
            findings.push({
                key: `${rel(file)}:manual:${operation}`,
                location: `${rel(file)}:${lineNumberAt(content, match.index)}`,
                message: `${operation} uses GET with ${/\bdata\s*:/.test(block) ? 'data' : 'params'} for a query-like request`,
            })
        }

        const generatedPattern = /\b(getAPI\([^)]+\)\.)?(api[A-Za-z0-9]*(?:Page|List|Query|Search|Filter)[A-Za-z0-9]*Get)\(([^)\n]*)\)/g
        for (const match of content.matchAll(generatedPattern)) {
            const callName = match[2]
            const args = match[3] ?? ''
            if (!isGeneratedFilterGetCall(callName, args)) continue
            findings.push({
                key: `${rel(file)}:generated:${callName}`,
                location: `${rel(file)}:${lineNumberAt(content, match.index)}`,
                message: `${callName} passes query/filter state through generated GET`,
            })
        }
    }

    return findings
}

function checkQueryGetRequests() {
    const allowlist = loadAllowlist().queryGetRequests ?? {}
    const failures = []
    const allowed = []
    const findings = collectQueryGetRequests()

    for (const finding of findings.sort((a, b) => a.key.localeCompare(b.key))) {
        const allowReason = allowlist[finding.key]
        if (allowReason) {
            allowed.push(`${finding.location}: ${finding.message} (${allowReason})`)
        } else {
            failures.push(`${finding.location}: ${finding.message}`)
        }
    }

    printResult('api-query-get', failures, [
        `checked ${findings.length} query-like GET findings`,
        ...allowed.map((item) => `allowed: ${item}`),
    ])
    return failures.length === 0
}

function checkApiWrappers() {
    const handFiles = walk(join(SRC_ROOT, 'api'), (file) => /\.(ts|tsx)$/.test(file))
    const generatedFiles = walk(join(SRC_ROOT, 'api-services', 'apis'), (file) => file.endsWith('.ts'))
    const generatedNames = new Map(generatedFiles.map((file) => [normalizeApiName(file), rel(file)]))
    const reviewItems = []

    for (const file of handFiles) {
        const normalized = normalizeApiName(file)
        if (generatedNames.has(normalized)) {
            reviewItems.push(`${rel(file)} overlaps generated client ${generatedNames.get(normalized)}`)
        }
    }

    printResult('api', [], [
        `manual review items: ${reviewItems.length}`,
        ...reviewItems.slice(0, 50),
        reviewItems.length > 50 ? `... ${reviewItems.length - 50} more` : null,
    ].filter(Boolean))
    return checkQueryGetRequests()
}

function changedSourceFiles() {
    try {
        const changedOutput = execFileSync('git', ['diff', '--name-only', 'HEAD', '--', 'src'], {
            cwd: ROOT,
            encoding: 'utf8',
            stdio: ['ignore', 'pipe', 'ignore'],
        })
        const untrackedOutput = execFileSync('git', ['ls-files', '--others', '--exclude-standard', '--', 'src'], {
            cwd: ROOT,
            encoding: 'utf8',
            stdio: ['ignore', 'pipe', 'ignore'],
        })
        return [...new Set(`${changedOutput}\n${untrackedOutput}`
            .split('\n')
            .filter(Boolean)
            .map((path) => join(ROOT, path))
            .filter((file) => /\.(vue|ts|tsx)$/.test(file) && existsSync(file)))]
    } catch {
        return []
    }
}

const LOCALE_KEY_LINE = /['"`](zh-cn|zh-tw|en|ja|ko)['"`]\s*:/
const CHINESE_TEXT_PATTERN = />[^<]*[\u4e00-\u9fff][^<]*<|['"`][^'"`\n]*[\u4e00-\u9fff][^'"`\n]*['"`]/g

function globToRegex(pattern) {
    const escaped = pattern.replace(/[.+^${}()|[\]\\]/g, '\\$&')
    return new RegExp('^' + escaped
        .replace(/\*\*/g, '\x00')
        .replace(/\*/g, '[^/]*')
        .replace(/\x00/g, '.*')
        + '$')
}

function isI18nTextPathExcluded(relPath) {
    const exceptions = loadAllowlist().i18nTextExceptions ?? {}
    for (const pattern of Object.keys(exceptions)) {
        if (globToRegex(pattern).test(relPath)) return true
    }
    return false
}

function scanChineseItems(files) {
    const items = []
    for (const file of files) {
        const lines = readText(file).split('\n')
        let inBlockComment = false
        for (let index = 0; index < lines.length; index++) {
            const line = lines[index]
            const trimmed = line.trim()
            if (inBlockComment) {
                if (line.includes('*/')) inBlockComment = false
                continue
            }
            if (trimmed.startsWith('//') || trimmed.startsWith('*') || trimmed.startsWith('<!--')) continue
            const blockStart = line.indexOf('/*')
            if (blockStart !== -1) {
                const blockEnd = line.indexOf('*/', blockStart + 2)
                if (blockEnd === -1) inBlockComment = true
                continue
            }
            if (LOCALE_KEY_LINE.test(line)) continue
            CHINESE_TEXT_PATTERN.lastIndex = 0
            for (const match of line.matchAll(CHINESE_TEXT_PATTERN)) {
                const text = match[0].trim()
                if (!text) continue
                items.push({file: rel(file), line: index + 1, text, fingerprint: `${rel(file)}:${text}`})
            }
        }
    }
    return items
}

function allCheckableSourceFiles() {
    return walk(SRC_ROOT, (file) => /\.(vue|ts|tsx)$/.test(file))
        .filter((file) => !file.includes(join(SRC_ROOT, 'i18n') + sep) && !file.includes(join(SRC_ROOT, 'api-services') + sep))
        .filter((file) => !isI18nTextPathExcluded(rel(file)))
}

function loadTextBaseline() {
    if (!existsSync(I18N_TEXT_BASELINE_FILE)) return new Set()
    try {
        const data = JSON.parse(readText(I18N_TEXT_BASELINE_FILE))
        return new Set(data.fingerprints ?? [])
    } catch {
        return new Set()
    }
}

function checkText() {
    const baseline = loadTextBaseline()
    const allFiles = allCheckableSourceFiles()
    const allItems = scanChineseItems(allFiles)
    const changedFiles = changedSourceFiles()
        .filter((file) => !file.includes(`${sep}i18n${sep}`) && !file.includes(`${sep}api-services${sep}`))
        .filter((file) => !isI18nTextPathExcluded(rel(file)))
    const changedItems = scanChineseItems(changedFiles)
    const newViolations = changedItems.filter((item) => !baseline.has(item.fingerprint))
    const failures = newViolations.map((item) => `${item.file}:${item.line}: ${item.text.slice(0, 80)}`)
    printResult('text', failures, [
        `baseline fingerprints: ${baseline.size}`,
        `current full scan items: ${allItems.length} across ${allFiles.length} files`,
        `checked ${changedFiles.length} changed source files`,
        `new hardcoded Chinese not in baseline: ${newViolations.length}`,
        ...failures.slice(0, 50),
        failures.length > 50 ? `... ${failures.length - 50} more` : null,
    ].filter(Boolean))
    return failures.length === 0
}

function generateTextBaseline() {
    const files = allCheckableSourceFiles()
    const items = scanChineseItems(files)
    const fingerprints = [...new Set(items.map((item) => item.fingerprint))].sort()
    const baseline = {
        description: 'C151 \u5168\u91cf\u7528\u6237\u53ef\u89c1\u4e2d\u6587\u503a\u52a1\u57fa\u7ebf\uff1acheck:text \u5bf9 changed files \u4e2d\u4e0d\u5728\u57fa\u7ebf\u7684\u65b0\u589e\u4e2d\u6587\u786c\u7f16\u7801 fail\uff1b\u57fa\u7ebf\u5185\u6761\u76ee\u4e3a\u5df2\u5206\u7c7b\u5386\u53f2\u503a\u52a1\uff0c\u968f\u8fc1\u79fb\u63a8\u8fdb\u5220\u9664\u3002\u91cd\u65b0\u751f\u6210\uff1anode scripts/check-frontend-standards.mjs text-baseline',
        count: fingerprints.length,
        fingerprints,
    }
    writeFileSync(I18N_TEXT_BASELINE_FILE, JSON.stringify(baseline, null, 2) + '\n')
    console.log(`[text-baseline] wrote ${fingerprints.length} fingerprints to ${rel(I18N_TEXT_BASELINE_FILE)}`)
    return true
}

function printResult(name, failures, details = []) {
    console.log(`\n[${name}]`)
    for (const detail of details) console.log(`  ${detail}`)
    if (failures.length === 0) {
        console.log('  result: pass')
        return
    }
    console.log(`  result: fail (${failures.length})`)
    for (const failure of failures.slice(0, 80)) console.log(`  - ${failure}`)
    if (failures.length > 80) console.log(`  ... ${failures.length - 80} more`)
}

function assert(condition, message) {
    if (!condition) throw new Error(message)
}

function selfTest() {
    const sample = 'new MenuSeedDefinition("route:x", null, MenuTypeEnum.Menu, "x", "/x", "/x/index", null, null, "X", "message.menu.x", "icon", null, false)'
    const parsed = parseMenuSeedDefinitions(sample)
    assert(parsed.length === 1, 'parser should find one definition')
    assert(parsed[0].key === 'route:x', 'parser should read key')
    assert(parsed[0].component === '/x/index', 'parser should read component')
    const globalMenuSample = `new() {
 Key = "route:y",
 Type = MenuTypeEnum.Menu,
 Name = "y",
 Path = "/y",
 Component = "/y/index",
 Permission = null,
 Title = "Y",
 TitleCode = "message.menu.y",
},`
    const parsedGlobalMenu = parseGlobalMenuSeedDefinitions(globalMenuSample)
    assert(parsedGlobalMenu.length === 1, 'global menu seed parser should find one definition')
    assert(parsedGlobalMenu[0].component === '/y/index', 'global menu seed parser should read component')
    assert(splitArgs('"a,b", null, Flag.A | Flag.B').length === 3, 'splitArgs should ignore commas inside strings')
    assert(changedSourceFiles().every((file) => file.startsWith(ROOT)), 'changed source files should be absolute paths')
    assert(isQueryOperationName('pageGameEntity'), 'page operation should be treated as a query')
    assert(isGeneratedFilterGetCall('apiTenantOrgListGet', 'state.queryParams.name'), 'generated list GET with queryParams should be blocked')
    assert(!isGeneratedFilterGetCall('apiSysEnumEnumDataListGet', '"GameLanguage"'), 'plain dictionary GET should not be blocked')
    const requestSample = "export const pageX = (params) => request({\n  url: Api.Page,\n  method: 'get',\n  params,\n})"
    const requestIndex = requestSample.indexOf('request(')
    const requestBlock = readObjectBlock(requestSample, requestSample.indexOf('{', requestIndex))
    assert(requestBlock.includes("method: 'get'"), 'request block reader should preserve method')

    // ponytail: component matching mirrors the current Vite glob prefix behavior; if routing moves to exact IDs,
    // replace this with the same resolver used by src/router/backEnd.ts.
    const matches = matchComponent('/x/index', [{ keys: new Set(['x/index']), rel: 'src/views/x/index.vue' }])
    assert(matches.length === 1, 'component matcher should match normalized view paths')
    assert(globToRegex('src/views/pages/**').test('src/views/pages/workflow/index.vue'), 'glob ** should match nested path')
    assert(!globToRegex('src/views/pages/**').test('src/views/system/user/index.vue'), 'glob should not match unrelated dir')
    assert(isI18nTextPathExcluded('src/views/pages/workflow/index.vue'), 'demo pages path should be excluded by allowlist')
    assert(!isI18nTextPathExcluded('src/views/system/user/index.vue'), 'business page should not be excluded')
    const frozenList = extractFrozenMultiWordFiles()
    assert(Array.isArray(frozenList) && frozenList.length > 100, 'frozen multi-word list should be extracted from eslint.config.mjs')
    assert(frozenList.every((path) => path.startsWith('src/') && path.endsWith('.vue')), 'frozen entries should be src-relative .vue paths')
    const additionDiff = diffFrozenAgainstSnapshot(['src/a/One.vue', 'src/b/Two.vue'], ['src/a/One.vue'])
    assert(additionDiff.additions.length === 1 && additionDiff.additions[0] === 'src/b/Two.vue', 'unregistered frozen entry should be flagged as addition')
    assert(diffFrozenAgainstSnapshot(['src/a/One.vue'], ['src/a/One.vue', 'src/c/Gone.vue']).stale.length === 1, 'shrunk entry should surface as stale snapshot hint, not a failure')
    // GFX-734 守卫：views 顶层白名单 + 静态路由 appScope↔文件首段（先红后绿构造样本）
    assert(viewsTopLevelViolation('tenant') === null, 'audience segment dir should pass views whitelist')
    assert(viewsTopLevelViolation('system') !== null, 'non-audience top-level dir should violate views whitelist')
    assert(staticRouteScopeViolation('app/appCenter/appWorkspace/index.vue', 'app') === null, 'appScope=app with app/ first segment should pass')
    assert(staticRouteScopeViolation('tenant/appCenter/appEntry/index.vue', 'app') !== null, 'appScope=app with tenant/ first segment should violate')
    assert(staticRouteScopeViolation('tenant/appHome/AppHomeBoard.vue', 'tenant') === null, 'appScope=tenant with tenant/ first segment should pass')
    assert(staticRouteScopeViolation('platform/global/platformJwtConfig/index.vue', null) === null, 'scopeless platform page should pass shared whitelist')
    assert(staticRouteScopeViolation('login/index.vue', null) === null, 'scopeless shared page should pass shared whitelist')
    assert(staticRouteScopeViolation('global/platform/index.vue', null) !== null, 'scopeless page outside shared whitelist should violate')
    console.log('[self-test] pass')
    return true
}

// ── views 受众三段布局守护（GFX-734 / C325）──────────────────────────────────
// views/ 顶层仅允许受众三段（platform/tenant/app）与共享目录（login/personal/error/chart）。
// 业务页必须按受众判据归位三段之下，第二级保留业务中心聚合。
const VIEWS_TOP_LEVEL_ALLOWED = new Set(['platform', 'tenant', 'app', 'login', 'personal', 'error', 'chart'])

function viewsTopLevelViolation(name) {
    if (VIEWS_TOP_LEVEL_ALLOWED.has(name)) return null
    return `src/views/${name}: 顶层目录超出受众白名单 [${[...VIEWS_TOP_LEVEL_ALLOWED].join(', ')}]（GFX-734 三段重组后业务页须落于 platform/tenant/app 之下）`
}

function checkViewsLayout() {
    const failures = []
    const viewsRoot = join(SRC_ROOT, 'views')
    const entries = existsSync(viewsRoot) ? readdirSync(viewsRoot, { withFileTypes: true }) : []
    for (const entry of entries) {
        const violation = viewsTopLevelViolation(entry.name)
        if (violation) failures.push(violation)
    }
    printResult('views-layout', failures, [
        `checked ${entries.length} src/views top-level entries against audience whitelist`,
    ])
    return failures.length === 0
}

// ── 静态路由 appScope ↔ 文件首段一致性守护（GFX-734 / C325）────────────────────
// route.ts / appWorkspaceRoutes.ts 中静态路由的 meta.appScope
// 必须与 component 文件路径首段一致：app→app、tenant→tenant、无 appScope⇒platform 或共享目录。
const STATIC_ROUTE_SCOPE_FILES = ['src/router/route.ts', 'src/router/appWorkspaceRoutes.ts']
const SHARED_ROUTE_FIRST_SEGMENTS = new Set(['platform', 'login', 'personal', 'error', 'chart'])

function staticRouteScopeViolation(viewPath, appScope) {
    const firstSegment = viewPath.split('/')[0]
    if (appScope === 'app' || appScope === 'tenant') {
        if (firstSegment !== appScope) {
            return `appScope=${appScope} 但 component 文件首段为 "${firstSegment}"（/@/views/${viewPath}）`
        }
        return null
    }
    if (!SHARED_ROUTE_FIRST_SEGMENTS.has(firstSegment)) {
        return `无 appScope（平台/共享页）但 component 文件首段 "${firstSegment}" 不在共享白名单 [${[...SHARED_ROUTE_FIRST_SEGMENTS].join(', ')}]（/@/views/${viewPath}）`
    }
    return null
}

function checkStaticRouteScopes() {
    const failures = []
    const checked = []
    for (const relFile of STATIC_ROUTE_SCOPE_FILES) {
        const text = readText(join(ROOT, relFile))
        // 每个 route 块从一行 path: 开始，到下一个 path: 行或文件尾结束；块内含其 component 与 meta
        const chunks = text.split(/\n(?=\s*path:)/)
        for (const chunk of chunks) {
            const compMatch = chunk.match(/import\('\/@\/views\/([^']+)'\)/)
            if (!compMatch) continue
            const viewPath = compMatch[1]
            const scopeMatch = chunk.match(/appScope:\s*'([^']+)'/)
            const appScope = scopeMatch ? scopeMatch[1] : null
            const label = `${relFile}: /@/views/${viewPath} (appScope=${appScope ?? 'none'})`
            checked.push(label)
            const violation = staticRouteScopeViolation(viewPath, appScope)
            if (violation) failures.push(`${label} — ${violation}`)
        }
    }
    printResult('static-route-scopes', failures, [
        `checked ${checked.length} static route component imports across ${STATIC_ROUTE_SCOPE_FILES.length} router files`,
    ])
    return failures.length === 0
}

function checkSysAbbreviation() {
    const allowlist = loadAllowlist().sysAbbreviations ?? {}
    const allowedTokens = new Set(Object.keys(allowlist))
    // Each pattern requires an uppercase letter right after `Sys`/`sys`, so legitimate
    // `System`/`system` (Sys followed by lowercase `t`) are never matched — only the
    // `Sys`/`sys` business abbreviation leaks the C179 change removed.
    const patterns = [
        { re: /\bSys[A-Z][a-zA-Z]*/g, kind: 'Sys' },
        { re: /\bsys[A-Z][a-zA-Z]*/g, kind: 'sys' },
        { re: /\/api\/sys[A-Z][a-zA-Z]*/g, kind: '/api/sys' },
        { re: /\broute:sys[A-Z][a-zA-Z]*/g, kind: 'route:sys' },
        { re: /message\.menu\.sys[A-Z][a-zA-Z]*/g, kind: 'message.menu.sys' },
    ]
    const files = walk(SRC_ROOT, (file) => /\.(vue|ts|tsx)$/.test(file))
    const failures = []
    const allowed = []
    const allowedSeen = new Set()
    for (const file of files) {
        const relPath = rel(file)
        if (isI18nTextPathExcluded(relPath)) continue
        const text = readText(file)
        for (const { re, kind } of patterns) {
            for (const match of text.matchAll(re)) {
                const token = match[0]
                if (allowedTokens.has(token)) {
                    const key = `${token}@${relPath}`
                    if (!allowedSeen.has(key)) {
                        allowedSeen.add(key)
                        allowed.push(`${token} in ${relPath} (${allowlist[token]})`)
                    }
                    continue
                }
                const lineNo = text.slice(0, match.index).split('\n').length
                failures.push(`${relPath}:${lineNo}: ${token} (${kind})`)
            }
        }
    }
    printResult('sys-abbreviation', failures, [
        `scanned ${files.length} Admin-own source files for Sys/sys business-abbreviation leaks (C179)`,
        ...allowed.slice(0, 80).map((item) => `allowed: ${item}`),
    ])
    return failures.length === 0
}

function checkPrimeVueSeverity() {
    // PrimeVue Tag/Button severity 合法值是 warn 不是 warning（tag/index.d.ts、button/index.d.ts）。
    // warning 落在所有样式分支之外会静默回退默认主色，警示语义丢失（GFX-649 / C242）。
    // 拦截两类写法：
    // 1. 模板字面量属性 severity="warning"（含 :severity="'warning'"）
    // 2. JS 侧作为 severity 值使用的 'warning'（对象字段 severity: 'warning' / map 值）与 severity 类型联合
    // 豁免：src/utils/message.ts 自有 toast MessageType、showConfirm type 选项（非 PrimeVue severity 语境）。
    const patterns = [
        { re: /severity\s*=\s*["']warning["']/g, kind: 'attr' },
        { re: /severity\s*:\s*["']warning["']/g, kind: 'value' },
        { re: /severity\?\s*:[^\n]*["']warning["']/g, kind: 'type-union' },
    ]
    const files = walk(SRC_ROOT, (file) => /\.(vue|ts|tsx)$/.test(file))
    const failures = []
    for (const file of files) {
        if (file.endsWith(join(SRC_ROOT, 'utils', 'message.ts'))) continue
        const relPath = rel(file)
        const text = readText(file)
        for (const { re, kind } of patterns) {
            for (const match of text.matchAll(re)) {
                const lineNo = text.slice(0, match.index).split('\n').length
                failures.push(`${relPath}:${lineNo}: ${match[0].trim()} (${kind})`)
            }
        }
    }
    printResult('primevue-severity', failures, [
        `scanned ${files.length} source files for illegal PrimeVue severity value 'warning' (C242)`,
    ])
    return failures.length === 0
}

// eslint.config.mjs 的 multi-word 冻结清单只减不增守护（GFX-717 / C305）：
// frozen 新条目未同步扩 allowlist componentNames 快照即失败（防豁免面扩张）；
// 收缩（移除 frozen 条目）永远合法，只提示同步删快照，不再复现 C287 时代的无守护冻结。
const ESLINT_CONFIG_FILE = join(ROOT, 'eslint.config.mjs')

function extractFrozenMultiWordFiles() {
    const text = readText(ESLINT_CONFIG_FILE)
    const match = text.match(/const frozenMultiWordComponentFiles = \[([\s\S]*?)\n\]/)
    if (!match) return null
    return [...match[1].matchAll(/'([^']+)'/g)].map((entry) => entry[1])
}

function diffFrozenAgainstSnapshot(frozen, snapshotKeys) {
    const snapshot = new Set(snapshotKeys)
    const frozenSet = new Set(frozen)
    return {
        additions: frozen.filter((path) => !snapshot.has(path)),
        stale: snapshotKeys.filter((path) => !frozenSet.has(path)),
    }
}

function checkComponentNames() {
    const allowlist = loadAllowlist().componentNames ?? {}
    const snapshotKeys = Object.keys(allowlist)
    const frozen = extractFrozenMultiWordFiles()
    const failures = []
    const details = []

    if (frozen === null) {
        failures.push('eslint.config.mjs frozenMultiWordComponentFiles array not found — extraction regex needs adapting after config reshuffle')
    } else if (frozen.length === 0 && snapshotKeys.length > 0) {
        failures.push('extracted 0 frozen entries while allowlist snapshot is non-empty — array quote style or layout may have changed')
    } else {
        const { additions, stale } = diffFrozenAgainstSnapshot(frozen, snapshotKeys)
        details.push(`frozen multi-word exemptions: ${frozen.length}, allowlist snapshot entries: ${snapshotKeys.length}`)
        for (const path of additions) {
            failures.push(`${path}: new multi-word exemption not registered in frontend-standards-allowlist.json componentNames (shrink-only guard)`)
        }
        for (const path of stale.slice(0, 20)) {
            details.push(`stale snapshot entry already shrunk, remove it: ${path}`)
        }
        if (stale.length > 20) {
            details.push(`... ${stale.length - 20} more stale entries`)
        }
    }

    printResult('componentNames', failures, details)
    return failures.length === 0
}

// 反向守护（2026-09-17，tenantAppSignatureKeyEntity 菜单缺失事故防回潮）：
// checkRoutes 只校验「种子 → 页面文件」方向；本检查补上「页面文件 → 种子/静态路由」方向——
// src/views 下每个目录式页面入口（index.vue）必须被某条菜单种子 Component 引用，或被
// src/router/*.ts 静态路由 import，否则该页面在任何租户的动态菜单里都不可达（孤儿页）。
// 确属有意不做菜单的页面（纯嵌入组件、壳页面）登记 allowlist orphanPages 并写明原因，只增不删需评审。
function staticRouteViewImports() {
    const refs = new Set()
    if (!existsSync(join(SRC_ROOT, 'router'))) return refs
    for (const file of readdirSync(join(SRC_ROOT, 'router')).filter((name) => name.endsWith('.ts'))) {
        const content = readText(join(SRC_ROOT, 'router', file))
        for (const match of content.matchAll(/import\('\/@\/(views\/[^']+)'\)/g)) {
            refs.add(match[1])
        }
    }
    return refs
}

function checkOrphanPages() {
    const definitions = loadMenuDefinitions()
    if (definitions === null) return true
    const allowlist = loadAllowlist().orphanPages ?? {}
    const failures = []
    const details = []

    const pageModules = collectComponentModules().filter((module) => module.rel.startsWith('src/views/') && module.rel.endsWith('/index.vue'))
    const seededComponents = definitions
        .filter((definition) => definition.component && definition.component !== 'Layout')
        .map((definition) => definition.component)
    const staticRefs = staticRouteViewImports()

    for (const module of pageModules) {
        const coveredBySeed = seededComponents.some((component) => matchComponent(component, [module]).length === 1)
        const normalizedStaticRefs = [...staticRefs].map((ref) => normalizePathPart(ref))
        const coveredByStaticRoute = [...module.keys].some((key) => normalizedStaticRefs.includes(key))
        if (coveredBySeed || coveredByStaticRoute) continue
        const reason = allowlist[module.rel]
        if (reason) {
            details.push(`allowed: ${module.rel} (${reason})`)
        } else {
            failures.push(`${module.rel}: directory-style page entry referenced by no GlobalMenuSeedData component and no static route import — unreachable for every tenant; add the menu seed or register an orphanPages allowlist reason`)
        }
    }

    printResult('orphanPages', failures, [
        `checked ${pageModules.length} directory-style page entries (${seededComponents.length} seeded components, ${staticRefs.size} static route imports)`,
        ...details,
    ])
    return failures.length === 0
}

const commands = {
    routes: checkRoutes,
    permissions: checkPermissions,
    api: checkApiWrappers,
    'api-query-get': checkQueryGetRequests,
    text: checkText,
    'sys-abbreviation': checkSysAbbreviation,
    'primevue-severity': checkPrimeVueSeverity,
    'componentNames': checkComponentNames,
    'views-layout': checkViewsLayout,
    'static-route-scopes': checkStaticRouteScopes,
    'orphan-pages': checkOrphanPages,
    'text-baseline': generateTextBaseline,
    'self-test': selfTest,
}
function main() {
    if (MODE === 'all') {
        const ok = [checkRoutes(), checkPermissions(), checkApiWrappers(), checkText(), checkSysAbbreviation(), checkPrimeVueSeverity(), checkComponentNames(), checkViewsLayout(), checkStaticRouteScopes(), checkOrphanPages()].every(Boolean)
        process.exit(ok ? 0 : 1)
    }
    const command = commands[MODE]
    if (!command) {
        console.error(`Unknown mode: ${MODE}`)
        console.error(`Expected one of: all, ${Object.keys(commands).join(', ')}`)
        process.exit(2)
    }
    process.exit(command() ? 0 : 1)
}

main()
