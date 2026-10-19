import {defineStore} from 'pinia';
import Watermark from '../utils/watermark';
import {useThemeConfigStore} from './themeConfig';
import {prefixedSessionStorage} from './index';

import {getAPI} from '../utils/axiosUtils';
import {SystemAuth} from '../api/systemAuth';
import {SystemConst} from '../api/systemConst';
import {GlobalDictionaryType} from '../api/globalDictionaryType';
import {clearTargetTenantId} from '../utils/targetTenant';

/**
 * 用户信息
 * @methods setUserInfos 设置用户信息
 */
export const useUserInfoStore = defineStore('userInfo', {
    state: (): UserInfosState => ({
        userInfos: {} as UserInfos,
        constList: [],
        dictList: [],
    }),
    getters: {
        // // 获取系统常量列表
        // async getSysConstList(): Promise<any[]> {
        // 	var res = await getAPI(SystemConst).systemConstListGet();
        // 	this.constList = res.data.data ?? [];
        // 	return this.constList;
        // },
        // 超管判定单一事实来源（C335/GFX-744）：accountType=999 为平台超管。
        // 路由守卫 / 顶栏用户菜单 / 应用驾驶舱等调用方一律读此 getter，不得散写 999 字面判定；
        // 唯一例外：getApiUserInfo 内 hydrate 前读 API payload 的判定（赋值前 getter 不可用）。
        isSuperAdmin: (state) => Number(state.userInfos?.accountType) === 999,
    },
    actions: {
        async setUserInfos() {
            // persistedstate auto-hydrates constList/dictList from sessionStorage
            this.userInfos = <UserInfos>await this.getApiUserInfo();
        },
        async setConstList() {
            if (this.constList && this.constList.length > 0) return;
            this.constList = await this.getSysConstList();
        },
        async setDictList() {
            if (this.dictList && this.dictList.length > 0) return;
            this.dictList = await this.getAllDictList();
        },
        // 获取当前用户信息
        async getApiUserInfo() {
            const res = await getAPI(SystemAuth).systemAuthUserInfoGet();
            if (res.data.data == null) return {};

            const d = res.data.data;
            if (Number(d.accountType) !== 999) {
                clearTargetTenantId();
            }

            const userInfos = {
                id: d.id,
                account: d.account,
                realName: d.realName,
                accountType: d.accountType,
                avatar: d.avatar ? '/' + d.avatar : '/favicon.ico',
                address: d.address,
                signature: d.signature,
                orgId: d.organizationId,
                orgName: d.organizationName,
                posName: d.posName,
                roles: [],
                authBtnList: d.buttons,
                time: new Date().getTime(),
            };
            // vue-next-admin 提交Id：225bce7 提交消息：admin-23.03.26:发布v2.4.32版本
            // 增加了下面代码，引起当前会话的用户信息不会刷新，如：重新提交的头像不更新，需要新开一个页面才能正确显示

            // 水印配置
            const configRes = await getAPI(SystemAuth).systemAuthWatermarkConfigGet();
            if (configRes.data.data == null) return userInfos;

            const storesThemeConfig = useThemeConfigStore();
            const watermarkText = `${userInfos.id} ${userInfos.realName}`;
            storesThemeConfig.themeConfig.watermarkText = watermarkText;
            Watermark.set(watermarkText);

            return userInfos;
        },
        // 获取常量集合
        async getSysConstList() {
            const res = await getAPI(SystemConst).systemConstListGet();
            return res.data.data ?? [];
        },
        // 获取字典集合
        async getAllDictList() {
            const res = await getAPI(GlobalDictionaryType).globalDictionaryTypeAllDictionaryListGet();
            return res.data.data ?? [];
        },
    },
    persist: {
        key: 'userInfo',
        storage: prefixedSessionStorage,
        pick: ['constList', 'dictList'],
    },
});
