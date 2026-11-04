// ponytail: C400 已删除后端端点，此处只为保持 build 可用；C403 连页删除
// 本模块为空桩：所有函数不发请求、不伪造数据，返回空载荷，消费方按既有空态/err 分支渲染。
/* eslint-disable */

export const tenantPositionAddPost = (..._args: unknown[]): Promise<any> => Promise.resolve({ data: { data: null } });
export const tenantPositionDeletePost = (..._args: unknown[]): Promise<any> => Promise.resolve({ data: { data: null } });
export const tenantPositionListGet = (..._args: unknown[]): Promise<any> => Promise.resolve({ data: { data: null } });
export const tenantPositionUpdatePost = (..._args: unknown[]): Promise<any> => Promise.resolve({ data: { data: null } });
