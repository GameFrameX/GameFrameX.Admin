/**
 * 后端文件 URL 解析（纯函数）。
 *
 * 背景：对象存储 local provider 把文件落盘到后端 wwwroot（如 Upload/Avatar/123.png），
 * 而 TenantUserEntity.Avatar 等 DB 字段只保存相对对象键（无 origin、无协议）。
 * 前端直接 `'/'+path` 渲染时请求打到前端自身（dev 20800 / 静态站），命中 SPA fallback
 * 返回 index.html（HTTP 200 的 HTML），<img> 渲染为裂图——登录后头像不显示即此因。
 * 统一经 window.__env__.VITE_API_URL 补全为指向后端 origin 的可加载绝对地址
 * （与 route.ts 的 `VITE_API_URL + '/schedule'` 同一约定）。
 */

/**
 * 把后端保存的文件路径解析为可加载 URL。
 * - 空值返回空字符串（调用方自行决定占位图）；
 * - data: / http(s):// / 协议相对地址原样返回；
 * - 其余（"Upload/Avatar/x.png" 或 "/Upload/Avatar/x.png"）补全 API origin 前缀。
 */
export function resolveBackendFileUrl(path?: string | null): string {
  if (!path) {
    return '';
  }

  const trimmed = path.trim();
  // data: 内联图与绝对/协议相对地址（云对象存储公共 URL）已是完整可加载形态
  if (!trimmed || /^(https?:)?\/\//i.test(trimmed) || trimmed.startsWith('data:')) {
    return trimmed;
  }

  const apiBaseUrl = (window.__env__?.VITE_API_URL as string | undefined) ?? '';
  const normalizedBase = apiBaseUrl.replace(/\/+$/, '');
  return `${normalizedBase}/${trimmed.replace(/^\/+/, '')}`;
}
