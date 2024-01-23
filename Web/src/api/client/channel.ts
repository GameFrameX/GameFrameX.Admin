import request from '/@/utils/request';
enum Api {
  AddClientChannel = '/api/clientChannel/add',
  DeleteClientChannel = '/api/clientChannel/delete',
  UpdateClientChannel = '/api/clientChannel/update',
  PageClientChannel = '/api/clientChannel/page',
  SelectClientChannel = '/api/clientChannel/Select',
}

// 增加渠道管理
export const addClientChannel = (params?: any) =>
	request({
		url: Api.AddClientChannel,
		method: 'post',
		data: params,
	});

// 删除渠道管理
export const deleteClientChannel = (params?: any) => 
	request({
			url: Api.DeleteClientChannel,
			method: 'post',
			data: params,
		});

// 编辑渠道管理
export const updateClientChannel = (params?: any) => 
	request({
			url: Api.UpdateClientChannel,
			method: 'post',
			data: params,
		});

// 分页查询渠道管理
export const pageClientChannel = (params?: any) => 
	request({
			url: Api.PageClientChannel,
			method: 'post',
			data: params,
		});
export const selectClientChannel = (params?: any) =>
	request({
		url: Api.SelectClientChannel,
		method: 'post',
		data: params,
	});


