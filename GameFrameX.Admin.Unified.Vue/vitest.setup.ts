/**
 * Vitest 全局 setup —— jsdom 缺失而组件渲染 / PrimeVue 需要的最小浏览器 API 桩。
 *
 * 组件私有依赖由各测试文件按需 vi.mock，不下沉到这里，避免污染全局副作用。
 * 类型补全见 vitest.globals.d.ts（仅在此 setup 中实质使用这些字段）。
 */

function makeMatchMediaStub(query: string): MediaQueryList {
	return {
		matches: false,
		media: query,
		onchange: null,
		addListener: () => undefined,
		removeListener: () => undefined,
		addEventListener: () => undefined,
		removeEventListener: () => undefined,
		dispatchEvent: () => false,
	}
}

if (typeof window !== 'undefined') {
	if (typeof window.matchMedia !== 'function') {
		Object.defineProperty(window, 'matchMedia', {
			writable: true,
			value: (query: string) => makeMatchMediaStub(query),
		})
	}
	// Vue 编译器在测试中发现 vendor-style 名字空间问题时会触发 ResizeObserver；
	// jsdom 默认不存在这两类观察者，必须以空实现兜底。
	if (typeof window.ResizeObserver !== 'function') {
		window.ResizeObserver = class {
			observe(): void {}
			unobserve(): void {}
			disconnect(): void {}
		} as unknown as typeof ResizeObserver
	}
	if (typeof window.IntersectionObserver !== 'function') {
		window.IntersectionObserver = class {
			observe(): void {}
			unobserve(): void {}
			disconnect(): void {}
			takeRecords(): IntersectionObserverEntry[] {
				return []
			}
		} as unknown as typeof IntersectionObserver
	}
}
