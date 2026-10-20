import vue from '@vitejs/plugin-vue';
import { resolve } from 'path';
import { defineConfig, loadEnv, ConfigEnv } from 'vite';
import vueSetupExtend from 'vite-plugin-vue-setup-extend-plus';
import viteCompression from 'vite-plugin-compression';
import { BUILD_CONFIG } from './src/utils/build';
import vueJsx from '@vitejs/plugin-vue-jsx';
import { CodeInspectorPlugin } from 'code-inspector-plugin';
import Components from 'unplugin-vue-components/vite';
import AutoImport from 'unplugin-auto-import/vite';
import { PrimeVueResolver } from '@primevue/auto-import-resolver';
import fs from 'fs';

const pathResolve = (dir: string) => {
	return resolve(__dirname, '.', dir);
};

const alias: Record<string, string> = {
	'/@': pathResolve('./src/'),
	'vue-i18n': 'vue-i18n/dist/vue-i18n.cjs.js',
};

const iconNames = [
	'faMagnifyingGlass', 'faPenToSquare', 'faTrash', 'faArrowsRotate', 'faPlus',
	'faMagnifyingGlassPlus', 'faMagnifyingGlassMinus', 'faRotateRight', 'faRotateLeft',
	'faXmark', 'faPlay', 'faPause', 'faCircleInfo', 'faCirclePlus', 'faCircleCheck',
	'faCircleXmark', 'faEllipsisVertical', 'faTrashCan', 'faFileCirclePlus', 'faEye',
	'faArrowUp', 'faArrowDown', 'faArrowLeft', 'faChevronRight', 'faChevronDown',
	'faBars', 'faBell', 'faUser', 'faCheck', 'faLink', 'faCopy', 'faDownload',
	'faPrint', 'faSort', 'faExpand', 'faCompress', 'faImage', 'faGear', 'faBuilding',
	'faFolderOpen', 'faFolderMinus', 'faCoins', 'faFile', 'faPen', 'faCropSimple',
	'faLockOpen', 'faLocationDot', 'faNoteSticky', 'faBolt', 'faToggleOn', 'faPowerOff',
	'faSun', 'faClock', 'faStopwatch', 'faTrophy', 'faGauge', 'faMoneyBill',
	'faMicrophone', 'faMicrochip', 'faKitMedical', 'faArrowPointer', 'faCloudArrowUp',
	'faMugHot', 'faSchool', 'faCartShopping', 'faMinus', 'faMaximize',
	'faCircleQuestion', 'faShop', 'faBullhorn', 'faFlag', 'faChartLine', 'faChartPie',
	'faArrowDownShortWide', 'faTableCells', 'faLightbulb', 'faRightFromBracket',
	'faCircleExclamation', 'faWarning', 'faFont', 'faGlobe', 'faEarthAmericas',
	'faMap', 'faTemperatureHalf', 'faDroplet', 'faSmog', 'faVolumeHigh', 'faQrcode',
	'faPhone', 'faDatabase', 'faLanguage', 'faWaveSquare', 'faShoePrints', 'faSquare',
	'faRectangleList', 'faChartBar', 'faLock', 'faCircle', 'faHandPeace',
	'faFlagCheckered', 'faGithubAlt',
];

const viteConfig = defineConfig((mode: ConfigEnv) => {
	const env = loadEnv(mode.mode, process.cwd());
	fs.writeFileSync('./public/config/config.js', `window.__env__ = ${JSON.stringify(env, null, 2)} `);
	return {
		plugins: [
			CodeInspectorPlugin({
				bundler: 'vite',
				hotKeys: ['shiftKey'],
			}),
			vue(),
			vueJsx(),
			vueSetupExtend(),
			Components({ resolvers: [PrimeVueResolver()] }),
			AutoImport({
				imports: [
					{ [pathResolve('./src/i18n/index')]: ['t', 'locale'] },
					{ [pathResolve('./src/utils/icons')]: iconNames },
					{ [pathResolve('./src/components/form-label')]: ['FormLabel'] },
					{ [pathResolve('./src/components/form-help-text')]: ['FormHelpText'] },
				],
				dts: 'src/auto-imports.d.ts',
				vueTemplate: true,
			}),
			viteCompression(),
			JSON.parse(env.VITE_OPEN_CDN) ? BUILD_CONFIG.cdn() : null,
		],
		root: process.cwd(),
		resolve: { alias },
		base: mode.command === 'serve' ? './' : env.VITE_PUBLIC_PATH,
		optimizeDeps: { exclude: ['vue-demi'] },
		server: {
			host: '0.0.0.0',
			port: env.VITE_PORT as unknown as number,
			open: JSON.parse(env.VITE_OPEN),
			hmr: true,
		},
		build: {
			outDir: 'build/dist',
			chunkSizeWarningLimit: 1500,
			sourcemap: false, // 生产构建禁用 Source Maps
			rollupOptions: {
				output: {
					chunkFileNames: 'assets/js/[name]-[hash].js',
					entryFileNames: 'assets/js/[name]-[hash].js',
					assetFileNames: 'assets/[ext]/[name]-[hash].[ext]',
					manualChunks(id) {
						if (id.includes('node_modules')) {
							return id.toString().match(/\/node_modules\/(?!.pnpm)(?<moduleName>[^\/]*)\//)?.groups!.moduleName ?? 'vender';
						}
					},
				},
				...(JSON.parse(env.VITE_OPEN_CDN) ? { external: BUILD_CONFIG.external } : {}),
			},
		},
		css: { preprocessorOptions: { css: { charset: false }, scss: { additionalData: `@use "/@/theme/z-index" as *;
@use "/@/theme/breakpoints" as *;\n` } } },
		define: {
			__VUE_I18N_LEGACY_API__: JSON.stringify(false),
			__VUE_I18N_FULL_INSTALL__: JSON.stringify(false),
			__INTLIFY_PROD_DEVTOOLS__: JSON.stringify(false),
			__NEXT_VERSION__: JSON.stringify(process.env.npm_package_version),
			__NEXT_NAME__: JSON.stringify(process.env.npm_package_name),
		},
		// Vitest 单一配置源：在 Vite 配置上加 test 块，vitest 自动读取。
		// 单元 = jsdom（支持 Vue 组件与浏览器 API stub）；e2e = Playwright（独立路径，conf 文件分离）。
		test: {
			environment: 'jsdom',
			globals: false,
			include: ['src/**/*.test.ts'],
			exclude: ['src/**/*.test.mjs', 'node_modules', 'build/dist'],
			setupFiles: ['./vitest.setup.ts'],
			coverage: {
				provider: 'v8',
				reporter: ['text', 'html'],
				include: ['src/**/*.{ts,vue}'],
				exclude: ['src/**/*.test.ts', 'src/**/*.d.ts', 'src/auto-imports.d.ts', 'src/components.d.ts'],
			},
		},
	};
});

export default viteConfig;
