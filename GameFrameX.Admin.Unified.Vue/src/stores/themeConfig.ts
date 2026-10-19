import {defineStore} from 'pinia';
import {themeConfigStorage} from './index';

export const useThemeConfigStore = defineStore('themeConfig', {
    state: (): ThemeConfigState => ({
        themeConfig: {
            /**
             * 全局主题
             */
            // 是否开启深色模式
            isIsDark: false,

            /**
             * 顶栏设置
             */
            // 默认顶栏导航背景颜色
            topBar: '#FFFFFF',
            // 默认顶栏导航字体颜色
            topBarColor: '#606266',
            // 是否开启顶栏背景颜色渐变
            isTopBarColorGradual: false,

            /**
             * 菜单设置
             */
            // 默认菜单导航背景颜色
            menuBar: '#545C64',
            // 默认菜单导航字体颜色
            menuBarColor: '#EAEAEA',
            // 默认菜单高亮背景色
            menuBarActiveColor: 'rgba(0, 0, 0, 0.2)',
            // 是否开启菜单背景颜色渐变
            isMenuBarColorGradual: false,

            /**
             * 界面设置
             */
            // 是否开启菜单水平折叠效果
            isCollapse: false,





            /**
             * 界面显示
             */




            // 水印文案（运行时由 userInfo 自动设置）
            watermarkText: '',
            /**
             * 其它设置
             */

            /**
             * 后端控制路由
             */
            // 是否开启后端控制路由
            isRequestRoutes: true,

            /**
             * 全局网站标题 / 副标题
             */
            // 网站主标题（菜单导航、浏览器当前网页标题）
            globalTitle: 'GameFrameX',
            // 网站副标题（登录页顶部文字）
            globalViceTitle: 'GameFrameX Admin',
            // 网站副标题（登录页顶部文字）
            globalViceTitleMsg: '游戏前后端一站式解决方案、github.com/GameFrameX',
            // 默认初始语言，可选值"<zh-cn|en|zh-tw>"，默认 zh-cn
            globalI18n: 'zh-cn',
            // 默认全局组件大小，可选值"<large|'default'|small>"，默认 'large'
            globalComponentSize: 'large',
        },
    }),
    actions: {
        setThemeConfig(data: ThemeConfigState) {
            this.themeConfig = data.themeConfig;
        },
    },
    persist: {
        key: 'themeConfig',
        storage: themeConfigStorage,
    },
});