/**
 * jsdom 缺失而测试链路需要的浏览器 API 类型补全，仅在 vitest 期间生效。
 * 运行时（生产 bundle）不会触碰这些字段，类型也仅做属性补全。
 */

interface ResizeObserverStub {
	observe(target: Element): void
	unobserve(target: Element): void
	disconnect(): void
}

interface IntersectionObserverStub {
	observe(target: Element): void
	unobserve(target: Element): void
	disconnect(): void
	takeRecords(): IntersectionObserverEntry[]
	root?: Element | Document | null
	rootMargin?: string
	thresholds?: ReadonlyArray<number>
}

interface Window {
	ResizeObserver: { new (cb: ResizeObserverCallback): ResizeObserverStub }
	IntersectionObserver: { new (
		cb: IntersectionObserverCallback,
		init?: IntersectionObserverInit,
	): IntersectionObserverStub }
}

export {}
