import {locale, t} from '../i18n/index';

// 季度本地化映射（Intl.DateTimeFormat 无标准 quarter 选项）
const quarterLongMap: Record<string, string[]> = {
    'zh-cn': ['第一季度', '第二季度', '第三季度', '第四季度'],
    'zh-tw': ['第一季度', '第二季度', '第三季度', '第四季度'],
    'en': ['Q1', 'Q2', 'Q3', 'Q4'],
    'ja': ['第1四半期', '第2四半期', '第3四半期', '第4四半期'],
    'ko': ['제1분기', '제2분기', '제3분기', '제4분기'],
};
const quarterShortMap: Record<string, string[]> = {
    'zh-cn': ['一', '二', '三', '四'],
    'zh-tw': ['一', '二', '三', '四'],
    'en': ['1', '2', '3', '4'],
    'ja': ['1', '2', '3', '4'],
    'ko': ['1', '2', '3', '4'],
};
// 周数本地化前缀
const weekPrefixMap: Record<string, string> = {
    'zh-cn': '第',
    'zh-tw': '第',
    'en': 'W',
    'ja': '第',
    'ko': '제',
};
const weekSuffixMap: Record<string, string> = {
    'zh-cn': '周',
    'zh-tw': '週',
    'en': '',
    'ja': '週',
    'ko': '주차',
};

/**
 * Unix 秒时间戳格式化（C145：后端时间字段统一 long 秒 + *Time 后缀）。
 * @param seconds Unix 秒；null/undefined/<=0/NaN 视为无值，返回 '-'。
 * @param format 格式，默认 'YYYY-mm-dd HH:MM:SS'。
 */
export function formatUnix(seconds: number | null | undefined, format = 'YYYY-mm-dd HH:MM:SS'): string {
    if (seconds == null || typeof seconds !== 'number' || isNaN(seconds) || seconds <= 0) return '-';
    const d = new Date(seconds * 1000);
    if (isNaN(d.getTime())) return '-';
    return formatDate(d, format);
}

/**
 * 时间日期转换
 * @param date 当前时间，new Date() 格式
 * @param format 需要转换的时间格式字符串
 * @description format 字符串随意，如 `YYYY-mm、YYYY-mm-dd`
 * @description format 季度："YYYY-mm-dd HH:MM:SS QQQQ"
 * @description format 星期："YYYY-mm-dd HH:MM:SS WWW"
 * @description format 几周："YYYY-mm-dd HH:MM:SS ZZZ"
 * @description format 季度 + 星期 + 几周："YYYY-mm-dd HH:MM:SS WWW QQQQ ZZZ"
 * @returns 返回拼接后的时间字符串
 */
export function formatDate(date: Date, format: string): string {
    let z = getWeek(date); // 周
    let qut = Math.floor((date.getMonth() + 3) / 3).toString(); // 季度
    const opt: { [key: string]: string } = {
        'Y+': date.getFullYear().toString(), // 年
        'm+': (date.getMonth() + 1).toString(), // 月(月份从0开始，要+1)
        'd+': date.getDate().toString(), // 日
        'H+': date.getHours().toString(), // 时
        'M+': date.getMinutes().toString(), // 分
        'S+': date.getSeconds().toString(), // 秒
        'q+': qut, // 季度
    };
    // 星期：W / WW / WWW — 使用 Intl.DateTimeFormat weekday
    if (/(W+)/.test(format)) {
        const len = RegExp.$1.length;
        let weekday: string;
        if (len === 1) {
            weekday = new Intl.DateTimeFormat(locale.value, {weekday: 'narrow'}).format(date);
        } else if (len === 2) {
            weekday = new Intl.DateTimeFormat(locale.value, {weekday: 'short'}).format(date);
        } else {
            weekday = new Intl.DateTimeFormat(locale.value, {weekday: 'long'}).format(date);
        }
        format = format.replace(RegExp.$1, weekday);
    }
    // 季度：Q / QQ / QQQ / QQQQ — 本地化映射
    if (/(Q+)/.test(format)) {
        const len = RegExp.$1.length;
        const quarterIdx = Math.floor(date.getMonth() / 3); // 0-3
        const loc = locale.value;
        let quarterStr: string;
        if (len >= 4) {
            quarterStr = quarterLongMap[loc]?.[quarterIdx] ?? `Q${quarterIdx + 1}`;
        } else if (len === 3) {
            quarterStr = `Q${quarterIdx + 1}`;
        } else {
            quarterStr = quarterShortMap[loc]?.[quarterIdx] ?? String(quarterIdx + 1);
        }
        format = format.replace(RegExp.$1, quarterStr);
    }
    // 几周：Z / ZZ / ZZZ — 本地化映射
    if (/(Z+)/.test(format)) {
        const len = RegExp.$1.length;
        const loc = locale.value;
        let weekStr: string;
        if (len >= 3) {
            const prefix = weekPrefixMap[loc] ?? 'W';
            const suffix = weekSuffixMap[loc] ?? '';
            weekStr = `${prefix}${z}${suffix}`;
        } else {
            weekStr = String(z);
        }
        format = format.replace(RegExp.$1, weekStr);
    }
    for (let k in opt) {
        let r = new RegExp('(' + k + ')').exec(format);
        // 若输入的长度不为1，则前面补零
        if (r) format = format.replace(r[1], RegExp.$1.length == 1 ? opt[k] : opt[k].padStart(RegExp.$1.length, '0'));
    }
    return format;
}

/**
 * 获取当前日期是第几周
 * @param dateTime 当前传入的日期值
 * @returns 返回第几周数字值
 */
export function getWeek(dateTime: Date): number {
    let temptTime = new Date(dateTime.getTime());
    // 周几
    let weekday = temptTime.getDay() || 7;
    // 周1+5天=周六
    temptTime.setDate(temptTime.getDate() - weekday + 1 + 5);
    let firstDay = new Date(temptTime.getFullYear(), 0, 1);
    let dayOfWeek = firstDay.getDay();
    let spendDay = 1;
    if (dayOfWeek != 0) spendDay = 7 - dayOfWeek + 1;
    firstDay = new Date(temptTime.getFullYear(), 0, 1 + spendDay);
    let d = Math.ceil((temptTime.valueOf() - firstDay.valueOf()) / 86400000);
    let result = Math.ceil(d / 7);
    return result;
}

/**
 * 将时间转换为 `几秒前`、`几分钟前`、`几小时前`、`几天前`
 * @param param 当前时间，new Date() 格式或者字符串时间格式
 * @param format 需要转换的时间格式字符串
 * @description param 10秒：  10 * 1000
 * @description param 1分：   60 * 1000
 * @description param 1小时： 60 * 60 * 1000
 * @description param 24小时：60 * 60 * 24 * 1000
 * @description param 3天：   60 * 60* 24 * 1000 * 3
 * @returns 返回拼接后的时间字符串
 */
export function formatPast(param: string | Date, format: string = 'YYYY-mm-dd'): string {
    // 传入格式处理、存储转换值
    let t: any, s: number;
    // 获取js 时间戳
    let time: number = new Date().getTime();
    // 是否是对象
    typeof param === 'string' || 'object' ? (t = new Date(param).getTime()) : (t = param);
    // 当前时间戳 - 传入时间戳
    time = Number.parseInt(`${time - t}`);
    if (time < 10000) {
        // 10秒内
        return t('message.common.relativeTime.justNow');
    } else if (time < 60000 && time >= 10000) {
        // 超过10秒少于1分钟内
        s = Math.floor(time / 1000);
        return t('message.common.relativeTime.secondsAgo', {s} as any);
    } else if (time < 3600000 && time >= 60000) {
        // 超过1分钟少于1小时
        s = Math.floor(time / 60000);
        return t('message.common.relativeTime.minutesAgo', {s} as any);
    } else if (time < 86400000 && time >= 3600000) {
        // 超过1小时少于24小时
        s = Math.floor(time / 3600000);
        return t('message.common.relativeTime.hoursAgo', {s} as any);
    } else if (time < 259200000 && time >= 86400000) {
        // 超过1天少于3天内
        s = Math.floor(time / 86400000);
        return t('message.common.relativeTime.daysAgo', {s} as any);
    } else {
        // 超过3天
        let date = typeof param === 'string' || 'object' ? new Date(param) : param;
        return formatDate(date, format);
    }
}

/**
 * 时间问候语
 * @param param 当前时间，new Date() 格式
 * @description param 调用 `formatAxis(new Date())` 输出 `上午好`
 * @returns 返回拼接后的时间字符串
 */
export function formatAxis(param: Date): string {
    let hour: number = new Date(param).getHours();
    if (hour < 6) return 'dawn';
    else if (hour < 9) return 'morning';
    else if (hour < 12) return 'forenoon';
    else if (hour < 14) return 'noon';
    else if (hour < 17) return 'afternoon';
    else if (hour < 19) return 'dusk';
    else if (hour < 22) return 'evening';
    else return 'lateNight';
}
