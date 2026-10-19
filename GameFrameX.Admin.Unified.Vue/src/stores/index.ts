// https://pinia.vuejs.org/
import {createPinia} from 'pinia';
import {createPersistedState} from 'pinia-plugin-persistedstate';
import {local} from '../utils/storage';

// Storage adapter that adds __NEXT_NAME__: prefix to keys
// Uses raw localStorage/sessionStorage (plugin handles JSON serialization)
const prefixedLocalStorage = {
	getItem: (key: string): string | null => window.localStorage.getItem(local.setKey(key)),
	setItem: (key: string, value: string): void => window.localStorage.setItem(local.setKey(key), value),
	removeItem: (key: string): void => window.localStorage.removeItem(local.setKey(key)),
};

const prefixedSessionStorage = {
	getItem: (key: string): string | null => window.sessionStorage.getItem(local.setKey(key)),
	setItem: (key: string, value: string): void => window.sessionStorage.setItem(local.setKey(key), value),
	removeItem: (key: string): void => window.sessionStorage.removeItem(local.setKey(key)),
};

// themeConfig store stores the inner object directly (backward compat with local.get('themeConfig'))
const themeConfigStorage = {
	getItem: (key: string): string | null => {
		const raw = window.localStorage.getItem(local.setKey(key));
		if (!raw) return null;
		try {
			const inner = JSON.parse(raw);
			return JSON.stringify({themeConfig: inner});
		} catch {
			return null;
		}
	},
	setItem: (key: string, value: string): void => {
		try {
			const state = JSON.parse(value);
			window.localStorage.setItem(local.setKey(key), JSON.stringify(state.themeConfig));
		} catch {
			// ignore
		}
	},
	removeItem: (key: string): void => {
		window.localStorage.removeItem(local.setKey(key));
	},
};

export {prefixedLocalStorage, prefixedSessionStorage, themeConfigStorage};

// 创建
const pinia = createPinia();
pinia.use(createPersistedState({}));

// 导出
export default pinia;
