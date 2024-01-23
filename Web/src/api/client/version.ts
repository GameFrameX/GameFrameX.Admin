import request from '/@/utils/request';
enum Api {
  AddClientVersion = '/api/clientVersion/add',
  DeleteClientVersion = '/api/clientVersion/delete',
  UpdateClientVersion = '/api/clientVersion/update',
  PageClientVersion = '/api/clientVersion/page',
  SelectClientVersion = '/api/clientVersion/Select',
}

// 增加版本管理
export const addClientVersion = (params?: any) =>
	request({
		url: Api.AddClientVersion,
		method: 'post',
		data: params,
	});

// 删除版本管理
export const deleteClientVersion = (params?: any) => 
	request({
			url: Api.DeleteClientVersion,
			method: 'post',
			data: params,
		});

// 编辑版本管理
export const updateClientVersion = (params?: any) => 
	request({
			url: Api.UpdateClientVersion,
			method: 'post',
			data: params,
		});

// 分页查询版本管理
export const pageClientVersion = (params?: any) => 
	request({
			url: Api.PageClientVersion,
			method: 'post',
			data: params,
		});
export const selectClientVersion = (params?: any) =>
	request({
			url: Api.SelectClientVersion,
			method: 'post',
			data: params,
		});


