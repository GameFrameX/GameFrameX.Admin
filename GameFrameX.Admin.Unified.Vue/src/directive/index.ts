import type {App} from 'vue';
import {authDirective} from './authDirective';
import {wavesDirective, dragDirective, reclickDirective} from './customDirective';
import {blockLoading} from './blockLoading';

/**
 * 导出指令方法：v-xxx
 * @methods authDirective 用户权限指令，用法：v-auth
 * @methods wavesDirective 按钮波浪指令，用法：v-waves
 * @methods dragDirective 自定义拖动指令，用法：v-drag
 * @methods reclickDirective 重复按钮提交指令，用法：v-reclick
 * @methods blockLoading 加载遮罩指令，用法：v-block-loading
 */
export function directive(app: App) {
    // 用户权限指令
    authDirective(app);
    // 按钮波浪指令
    wavesDirective(app);
    // // 自定义拖动指令
    // dragDirective(app);
    // 重复按钮提交指令
    reclickDirective(app);
    // 加载遮罩指令 (替代 Element Plus v-loading)
    app.directive('block-loading', blockLoading);
}