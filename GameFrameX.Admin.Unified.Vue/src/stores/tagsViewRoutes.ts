import {defineStore} from 'pinia';
import {prefixedSessionStorage} from './index';

/**
 * TagsView 路由列表
 * @methods setTagsViewRoutes 设置 TagsView 路由列表
 * @methods setCurrenFullscreen 设置开启/关闭全屏时的 boolean 状态
 */
export const useTagsViewRoutesStore = defineStore('tagsViewRoutes', {
    state: (): TagsViewRoutesState => ({
        tagsViewRoutes: [],
        isTagsViewCurrenFull: false,
        tagsViewList: [],
    }),
    actions: {
        async setTagsViewRoutes(data: Array<string>) {
            this.tagsViewRoutes = data;
        },
        setCurrenFullscreen(bool: Boolean) {
            this.isTagsViewCurrenFull = bool;
        },
    },
    persist: {
        key: 'tagsViewRoutes',
        storage: prefixedSessionStorage,
        pick: ['isTagsViewCurrenFull', 'tagsViewList'],
    },
});
