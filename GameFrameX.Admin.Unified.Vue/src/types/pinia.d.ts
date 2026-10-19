/**
 * pinia 类型定义
 */

// 用户信息
declare interface UserInfos<T = any> {
    authBtnList: string[];
    photo: string;
    roles: string[];
    time: number;
    userName: string;

    [key: string]: T;
}

declare interface UserInfosState {
    userInfos: UserInfos;
    constList: unknown[];
    dictList: unknown[];
}

// 路由缓存列表
declare interface KeepAliveNamesState {
    keepAliveNames: string[];
    cachedViews: string[];
}

// TagsView 路由列表
declare interface TagsViewRoutesState<T = any> {
    tagsViewRoutes: T[];
    isTagsViewCurrenFull: Boolean;
    tagsViewList: T[];
}

// 路由列表
declare interface RoutesListState<T = any> {
    routesList: T[];
}

// 布局配置
declare interface ThemeConfigState {
    themeConfig: {
        topBar: string;
        topBarColor: string;
        isTopBarColorGradual: boolean;
        menuBar: string;
        menuBarColor: string;
        menuBarActiveColor: string;
        isMenuBarColorGradual: boolean;
        isCollapse: boolean;

        isIsDark: boolean;
        watermarkText: string;
        isRequestRoutes: boolean;
        globalTitle: string;
        globalViceTitle: string;
        globalViceTitleMsg: string;
        globalI18n: string;
        globalComponentSize: string;
    };
}
