/**
 * VC-1.10 Admin 前端半边断言（C348 / GFX-751）：未知 Online 码回退后端 message。
 *
 * 被测对象是真实 src/utils/http/shared.ts 的 resolveResponseMessage。该模块依赖
 * vue-i18n 与浏览器 storage / message 工具，但被测断言只关心"已知码 → 命中 i18n / 未知码 → 回退原文"，
 * 因此用 vi.mock 把这些依赖换成内存桩，t/te 按当前 locale 查真实五语言 errors 码表。
 *
 * 运行：`pnpm run test:unit`。
 */

import { vi, test, expect } from 'vitest'
import zhCn from '../../i18n/pages/errors/zh-cn'
import zhTw from '../../i18n/pages/errors/zh-tw'
import en from '../../i18n/pages/errors/en'
import ja from '../../i18n/pages/errors/ja'
import ko from '../../i18n/pages/errors/ko'
import { resolveResponseMessage } from './shared'

// ── i18n 桩状态：t/te 调用时读取，locale 可切换 ─────────────

interface I18nRoot {
	readonly errors?: { readonly codes?: Record<string, string> }
}

interface I18nState {
	locale: string
	messages: Record<string, I18nRoot>
}

const sharedState: I18nState = vi.hoisted(() => ({
	locale: 'zh-cn',
	messages: {} as Record<string, I18nRoot>,
}))

sharedState.messages['zh-cn'] = zhCn as I18nRoot
sharedState.messages['zh-tw'] = zhTw as I18nRoot
sharedState.messages['en'] = en as I18nRoot
sharedState.messages['ja'] = ja as I18nRoot
sharedState.messages['ko'] = ko as I18nRoot

// 单字段读 + 内部按路径遍历的 helper；写在外而不是内联 cast 因为规则禁止 inline-cast 读成员。
// type guard：已知 messages[i18nKey] 是对象且 part 命中 → 收窄为对象，否则中止遍历。
function stepInto(node: unknown, part: string): unknown {
	if (node === null || typeof node !== 'object') return undefined
	if (!(part in node)) return undefined
	return (node as Record<string, unknown>)[part]
}

vi.mock('../../i18n/index', () => {
	function walkPath(key: string): unknown {
		let node: unknown = sharedState.messages[sharedState.locale]
		for (const part of key.split('.')) {
			node = stepInto(node, part)
			if (node === undefined) return undefined
		}
		return node
	}
	return {
		t: (key: string): string => {
			const value = walkPath(key)
			return typeof value === 'string' ? value : key
		},
		te: (key: string): boolean => typeof walkPath(key) === 'string',
	}
})

vi.mock('../storage', () => ({
	local: { get: () => null, set: () => undefined, remove: () => undefined },
	session: { clear: () => undefined },
}))

vi.mock('../formatTime', () => ({
	formatDate: (): string => '',
}))

vi.mock('../message', () => ({
	showError: (): void => undefined,
}))

vi.mock('../targetTenant', () => ({
	TARGET_TENANT_ID_HEADER: 'x-target-tenant-id',
	getTargetTenantId: () => null,
	clearTargetTenantId: () => undefined,
}))

// ── 断言数据 ──────────────────────────────────────────────

// C339 Online 八段（320～327）镜像 vault 1xxx～8xxx；段号 = 前 3 位
const isOnlineCode = (code: string): boolean => {
	const head = Math.floor(Number(code) / 100000)
	return head >= 320 && head <= 327
}
const onlineCodes = Object.keys(zhCn.errors.codes).filter((code) => isOnlineCode(code))
// 未知码样本：Online 段内但不在码表（模拟后端比前端新的场景，如新码未同步）
const unknownOnlineCodes = [32099999, 32512345, 32700099]
const BACKEND_MESSAGE = '后端原文 message（测试哨兵）'

// 用 type guard 而非 inline-cast 读 codes 表。规则：in-process value 用 in/typeof 收窄，
// 仅当不可运行时检查时再 unchecked cast；这里两份都是同仓库 known shape，写一个 helper 已足够。
function readCodesTable(locale: string): Record<string, string> | undefined {
	const root = sharedState.messages[locale]
	const errors = stepInto(root, 'errors')
	if (errors === undefined || errors === null || typeof errors !== 'object') return undefined
	if (!('codes' in errors)) return undefined
	const codes = (errors as Record<string, unknown>).codes
	if (codes === undefined || codes === null || typeof codes !== 'object') return undefined
	return codes as Record<string, string>
}

test('已知 Online 码在五语言全部命中本地化文案（C339 同步落地证据）', () => {
	expect(onlineCodes.length).toBeGreaterThanOrEqual(20)
	for (const locale of Object.keys(sharedState.messages)) {
		sharedState.locale = locale
		const table = readCodesTable(locale)
		if (!table) throw new Error(`${locale} 的 errors.codes 缺失`)
		for (const code of onlineCodes) {
			const result = resolveResponseMessage({ code: Number(code), message: BACKEND_MESSAGE })
			expect(result, `${locale} 的 ${code} 未走本地化路径`).not.toBe(BACKEND_MESSAGE)
			expect(result).toBe(table[code])
		}
	}
})

test('未知 Online 码五语言键未命中 → resolveResponseMessage 回退后端 message 原文（VC-1.10）', () => {
	for (const locale of Object.keys(sharedState.messages)) {
		sharedState.locale = locale
		for (const code of unknownOnlineCodes) {
			expect(
				resolveResponseMessage({ code, message: BACKEND_MESSAGE }),
				`${locale} 的未知 Online 码 ${code} 应回退后端 message 原文`,
			).toBe(BACKEND_MESSAGE)
		}
	}
})

test('未知 Online 码 + 空后端 message → 返回空串，不虚构成功文案（不误判成功）', () => {
	for (const locale of Object.keys(sharedState.messages)) {
		sharedState.locale = locale
		for (const code of unknownOnlineCodes) {
			expect(
				resolveResponseMessage({ code, message: '' }),
				`${locale} 的未知 Online 码 ${code} 不应虚构展示文案`,
			).toBe('')
		}
	}
})
