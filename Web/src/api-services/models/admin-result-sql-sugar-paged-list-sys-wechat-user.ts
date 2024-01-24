/* tslint:disable */
/* eslint-disable */
/**
 * Admin.NET
 * 让 .NET 开发更简单、更通用、更流行。前后端分离架构(.NET6/Vue3)，开箱即用紧随前沿技术。<br/><a href='https://gitee.com/zuohuaijun/Admin.NET/'>https://gitee.com/zuohuaijun/Admin.NET</a>
 *
 * OpenAPI spec version: 1.0.0
 * Contact: 515096995@qq.com
 *
 * NOTE: This class is auto generated by the swagger code generator program.
 * https://github.com/swagger-api/swagger-codegen.git
 * Do not edit the class manually.
 */
import { SqlSugarPagedListSysWechatUser } from './sql-sugar-paged-list-sys-wechat-user';
/**
 * 全局返回结果
 * @export
 * @interface AdminResultSqlSugarPagedListSysWechatUser
 */
export interface AdminResultSqlSugarPagedListSysWechatUser {
    /**
     * 状态码
     * @type {number}
     * @memberof AdminResultSqlSugarPagedListSysWechatUser
     */
    code?: number;
    /**
     * 类型success、warning、error
     * @type {string}
     * @memberof AdminResultSqlSugarPagedListSysWechatUser
     */
    type?: string | null;
    /**
     * 错误信息
     * @type {string}
     * @memberof AdminResultSqlSugarPagedListSysWechatUser
     */
    message?: string | null;
    /**
     * 
     * @type {SqlSugarPagedListSysWechatUser}
     * @memberof AdminResultSqlSugarPagedListSysWechatUser
     */
    result?: SqlSugarPagedListSysWechatUser;
    /**
     * 附加数据
     * @type {any}
     * @memberof AdminResultSqlSugarPagedListSysWechatUser
     */
    extras?: any | null;
    /**
     * 时间
     * @type {Date}
     * @memberof AdminResultSqlSugarPagedListSysWechatUser
     */
    time?: Date;
}