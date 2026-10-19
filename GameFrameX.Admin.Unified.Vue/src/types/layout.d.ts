// aside
declare type AsideState = {
    menuList: RouteRecordRaw[];
    clientWidth: number;
};

// columnsAside
declare type ColumnsAsideState<T = any> = {
    columnsAsideList: T[];
    liIndex: number;
    liOldIndex: null | number;
    liHoverIndex: null | number;
    liOldPath: null | string;
    difference: number;
    routeSplit: string[];
};

// navigationBars breadcrumb
declare type BreadcrumbState<T = any> = {
    breadcrumbList: T[];
    routeSplit: string[];
    routeSplitFirst: string;
    routeSplitIndex: number;
};

// navigationBars search
declare type SearchState<T = any> = {
    isShowSearch: boolean;
    menuQuery: string;
    tagsViewList: T[];
};

// navigationBars tagsView
declare type TagsViewState<T = any> = {
    routeActive: string | T;
    routePath: string | unknown;
    sortable: T;
    tagsRefsIndex: number;
    tagsViewList: T[];
    tagsViewRoutesList: T[];
};

// navigationBars parent
declare type ParentViewState<T = any> = {
    refreshRouterViewKey: string;
    iframeRefreshKey: string;
    keepAliveNameList: string[];
    iframeList: T[];
};

// navigationBars link
declare type LinkViewState = {
    title: string;
    titleCode: string;
    isLink: string;
};
