import request from '/@/utils/request';

enum Api {
    AddClientAssetPackage = '/api/clientAssetPackage/add',
    DeleteClientAssetPackage = '/api/clientAssetPackage/delete',
    UpdateClientAssetPackage = '/api/clientAssetPackage/update',
    PageClientAssetPackage = '/api/clientAssetPackage/page',
    SelectClientAssetPackage = '/api/clientAssetPackage/Select',
}

// 增加资源包
export const addClientAssetPackage = (params?: any) =>
    request({
        url: Api.AddClientAssetPackage,
        method: 'post',
        data: params,
    });

// 删除资源包
export const deleteClientAssetPackage = (params?: any) =>
    request({
        url: Api.DeleteClientAssetPackage,
        method: 'post',
        data: params,
    });

// 编辑资源包
export const updateClientAssetPackage = (params?: any) =>
    request({
        url: Api.UpdateClientAssetPackage,
        method: 'post',
        data: params,
    });

// 分页查询资源包
export const pageClientAssetPackage = (params?: any) =>
    request({
        url: Api.PageClientAssetPackage,
        method: 'post',
        data: params,
    });

export const selectClientAssetPackage = (params?: any) =>
    request({
        url: Api.SelectClientAssetPackage,
        method: 'post',
        data: params,
    });



