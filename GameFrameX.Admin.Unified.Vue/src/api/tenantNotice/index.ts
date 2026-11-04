// ponytail: C400 已删除后端端点，此处只为保持 build 可用；C403 连页删除
// 本模块为空桩：所有函数不发请求、不伪造数据，返回空载荷，消费方按既有空态/err 分支渲染。
/* eslint-disable */

export const tenantNoticeAddPost = (..._args: unknown[]): Promise<any> => Promise.resolve({ data: { data: null } });
export const tenantNoticeDeletePost = (..._args: unknown[]): Promise<any> => Promise.resolve({ data: { data: null } });
export const tenantNoticePagePost = (..._args: unknown[]): Promise<any> => Promise.resolve({ data: { data: null } });
export const tenantNoticePageReceivedGet = (..._args: unknown[]): Promise<any> => Promise.resolve({ data: { data: null } });
export const tenantNoticePublishPost = (..._args: unknown[]): Promise<any> => Promise.resolve({ data: { data: null } });
export const tenantNoticeSetReadPost = (..._args: unknown[]): Promise<any> => Promise.resolve({ data: { data: null } });
export const tenantNoticeUnReadListGet = (..._args: unknown[]): Promise<any> => Promise.resolve({ data: { data: null } });
export const tenantNoticeUpdatePost = (..._args: unknown[]): Promise<any> => Promise.resolve({ data: { data: null } });
