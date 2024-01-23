import request from '/@/utils/request';
enum Api {
  AddClientAppVersion = '/api/clientAppVersion/add',
  DeleteClientAppVersion = '/api/clientAppVersion/delete',
  UpdateClientAppVersion = '/api/clientAppVersion/update',
  PageClientAppVersion = '/api/clientAppVersion/page',
}

// 增加游戏版本管理
export const addClientAppVersion = (params?: any) =>
	request({
		url: Api.AddClientAppVersion,
		method: 'post',
		data: params,
	});

// 删除游戏版本管理
export const deleteClientAppVersion = (params?: any) => 
	request({
			url: Api.DeleteClientAppVersion,
			method: 'post',
			data: params,
		});

// 编辑游戏版本管理
export const updateClientAppVersion = (params?: any) => 
	request({
			url: Api.UpdateClientAppVersion,
			method: 'post',
			data: params,
		});

// 分页查询游戏版本管理
export const pageClientAppVersion = (params?: any) => 
	request({
			url: Api.PageClientAppVersion,
			method: 'post',
			data: params,
		});


