/**
 * mitt 事件类型定义
 *
 * @method getBreadcrumbIndexSetFilterRoutes 布局设置弹窗，开启切割菜单时，菜单数据传入到 navigationMenu 下的菜单中
 * @method layoutMobileResize 浏览器窗口改变时，用于适配移动端界面显示
 * @method onTagsViewRefreshRouterView tagsview 刷新界面
 * @method onCurrentContextmenuClick tagsview 右键菜单每项点击时
 * @method openOrCloseSortable 布局配置界面开启/关闭 tagsView 拖拽
 */
declare type MittType<T = any> = {
    getBreadcrumbIndexSetFilterRoutes?: string;
    layoutMobileResize: T;
    onTagsViewRefreshRouterView?: T;
    onCurrentContextmenuClick?: T;
    openOrCloseSortable?: T;
};

// mitt 参数菜单类型
declare type MittMenu = {
    children: RouteRecordRaw[];
    item?: RouteItem;
};
