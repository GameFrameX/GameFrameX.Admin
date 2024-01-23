import request from '/@/utils/request';

enum Api {
    AddClientPlatform = '/api/clientPlatform/add',
    DeleteClientPlatform = '/api/clientPlatform/delete',
    UpdateClientPlatform = '/api/clientPlatform/update',
    PageClientPlatform = '/api/clientPlatform/page',
    SelectClientPlatform = '/api/clientPlatform/Select',
}

// 增加平台管理
export const addClientPlatform = (params?: any) =>
    request({
        url: Api.AddClientPlatform,
        method: 'post',
        data: params,
    });

// 删除平台管理
export const deleteClientPlatform = (params?: any) =>
    request({
        url: Api.DeleteClientPlatform,
        method: 'post',
        data: params,
    });

// 编辑平台管理
export const updateClientPlatform = (params?: any) =>
    request({
        url: Api.UpdateClientPlatform,
        method: 'post',
        data: params,
    });

// 分页查询平台管理
export const pageClientPlatform = (params?: any) =>
    request({
        url: Api.PageClientPlatform,
        method: 'post',
        data: params,
    });
export const selectClientPlatform = (params?: any) =>
    request({
        url: Api.SelectClientPlatform,
        method: 'post',
        data: params,
    });


