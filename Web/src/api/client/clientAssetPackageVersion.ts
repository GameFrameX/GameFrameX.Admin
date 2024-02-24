import request from '/@/utils/request';
enum Api {
  AddClientAssetPackageVersion = '/api/clientAssetPackageVersion/add',
  DeleteClientAssetPackageVersion = '/api/clientAssetPackageVersion/delete',
  UpdateClientAssetPackageVersion = '/api/clientAssetPackageVersion/update',
  PageClientAssetPackageVersion = '/api/clientAssetPackageVersion/page',
}

// 增加资源包版本
export const addClientAssetPackageVersion = (params?: any) =>
	request({
		url: Api.AddClientAssetPackageVersion,
		method: 'post',
		data: params,
	});

// 删除资源包版本
export const deleteClientAssetPackageVersion = (params?: any) => 
	request({
			url: Api.DeleteClientAssetPackageVersion,
			method: 'post',
			data: params,
		});

// 编辑资源包版本
export const updateClientAssetPackageVersion = (params?: any) => 
	request({
			url: Api.UpdateClientAssetPackageVersion,
			method: 'post',
			data: params,
		});

// 分页查询资源包版本
export const pageClientAssetPackageVersion = (params?: any) => 
	request({
			url: Api.PageClientAssetPackageVersion,
			method: 'post',
			data: params,
		});


