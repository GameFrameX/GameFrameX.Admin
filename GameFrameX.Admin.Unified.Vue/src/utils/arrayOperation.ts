/**
 * 判断两个对象是否相同
 * @param a 要比较的对象一
 * @param b 要比较的对象二
 * @returns 相同返回 true，反之则反
 */
export function isObjectValueEqual<T>(a: T, b: T): boolean {
    if (!a || !b) return false;
    const left = a as Record<string, unknown>;
    const right = b as Record<string, unknown>;
    let aProps = Object.getOwnPropertyNames(a);
    let bProps = Object.getOwnPropertyNames(b);
    if (aProps.length != bProps.length) return false;
    for (let i = 0; i < aProps.length; i++) {
        let propName = aProps[i];
        let propA = left[propName];
        let propB = right[propName];
        if (!Object.prototype.hasOwnProperty.call(b, propName)) return false;
        if (propA instanceof Object) {
            if (!isObjectValueEqual(propA, propB)) return false;
        } else if (propA !== propB) {
            return false;
        }
    }
    return true;
}
