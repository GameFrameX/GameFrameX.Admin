import request from '/src/utils/request';
enum Api {
  AddClientGlobalInfo = '/api/clientGlobalInfo/add',
  DeleteClientGlobalInfo = '/api/clientGlobalInfo/delete',
  UpdateClientGlobalInfo = '/api/clientGlobalInfo/update',
  PageClientGlobalInfo = '/api/clientGlobalInfo/page',
}

// 增加全局信息管理
export const addClientGlobalInfo = (params?: any) =>
	request({
		url: Api.AddClientGlobalInfo,
		method: 'post',
		data: params,
	});

// 删除全局信息管理
export const deleteClientGlobalInfo = (params?: any) => 
	request({
			url: Api.DeleteClientGlobalInfo,
			method: 'post',
			data: params,
		});

// 编辑全局信息管理
export const updateClientGlobalInfo = (params?: any) => 
	request({
			url: Api.UpdateClientGlobalInfo,
			method: 'post',
			data: params,
		});

// 分页查询全局信息管理
export const pageClientGlobalInfo = (params?: any) => 
	request({
			url: Api.PageClientGlobalInfo,
			method: 'post',
			data: params,
		});


