// FormLabel 组件渲染单测 —— jsdom + @vue/test-utils。
//
// 覆盖三路优先级：props.required 显式 > inject(name+set) > 默认 false。
// 仅 5 个测试是最小可表达契约；后续若新增字段，遵循「按分支一条测试」补全。
//
// 运行：`pnpm run test:unit`。

import { mount, type VueWrapper } from '@vue/test-utils'
import { ref } from 'vue'
import { test, expect } from 'vitest'
import FormLabel from './index.vue'
import { requiredFieldsKey } from './key'

const LABEL_TEXT = 'FieldName'

// 组件契约是 ReadonlySet<string>；这里先把外部数据定为字符串数组，再在契约边界上构 Set，
// 避免调用点直接构造 Set 容器（规则：静态串键查找优先 Record，动态集合才用 Set）。
function renderLabel(
	props: { required?: boolean; name?: string },
	requiredFieldNames: readonly string[] = [],
): string {
	const requiredSet: ReadonlySet<string> = new Set(requiredFieldNames)
	const wrapper: VueWrapper = mount(FormLabel, {
		props,
		slots: { default: LABEL_TEXT },
		global: { provide: { [requiredFieldsKey]: ref(requiredSet) } },
	})
	return wrapper.text()
}

test('props.required=true 显式必填 → 渲染星标', () => {
	const text = renderLabel({ required: true })
	expect(text).toContain(LABEL_TEXT)
	expect(text).toContain('*')
})

test('props.required=false 显式非必填 → 不渲染星标', () => {
	const text = renderLabel({ required: false })
	expect(text).toContain(LABEL_TEXT)
	expect(text).not.toContain('*')
})

test('不传 required、name 在注入必填集合 → 推得必填', () => {
	const text = renderLabel({ name: 'email' }, ['email', 'phone'])
	expect(text).toContain('*')
})

test('不传 required、name 不在注入集合 → 不渲染星标', () => {
	const text = renderLabel({ name: 'note' }, ['email'])
	expect(text).toContain(LABEL_TEXT)
	expect(text).not.toContain('*')
})

test('未传 props → 默认非必填', () => {
	const text = renderLabel({})
	expect(text).toContain(LABEL_TEXT)
	expect(text).not.toContain('*')
})
