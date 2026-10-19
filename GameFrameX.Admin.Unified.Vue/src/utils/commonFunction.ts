// 通用函数
import useClipboard from 'vue-clipboard3';
import {showSuccess, showError} from './message';

export default function () {
    const {toClipboard} = useClipboard();

    // 点击复制文本
    const copyText = (text: string) => {
        return new Promise((resolve, reject) => {
            try {
                //复制
                toClipboard(text);
                //下面可以设置复制成功的提示框等操作
                showSuccess(t('message.layout.copyTextSuccess'));
                resolve(text);
            } catch (e) {
                //复制失败
                showError(t('message.layout.copyTextError'));
                reject(e);
            }
        });
    };
    // 去掉Html标签(取前面5个字符)
    const removeHtmlSub = (value: string) => {
        var str = value.replace(/<[^>]+>/g, '');
        if (str.length > 50) return str.substring(0, 50) + '......';
        else return str;
    };
    // 去掉Html标签
    const removeHtml = (value: string) => {
        return value.replace(/<[^>]+>/g, '');
    };
    // 获取枚举描述
    const getEnumDesc = (key: any, lstEnum: any) => {
        return lstEnum.find((x: any) => x.value == key)?.describe;
    };
    return {
        copyText,
        removeHtmlSub,
        removeHtml,
        getEnumDesc,
    };
}
