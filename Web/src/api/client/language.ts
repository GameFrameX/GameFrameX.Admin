import request from '/@/utils/request';
enum Api {
  AddClientLanguage = '/api/clientLanguage/add',
  DeleteClientLanguage = '/api/clientLanguage/delete',
  UpdateClientLanguage = '/api/clientLanguage/update',
  PageClientLanguage = '/api/clientLanguage/page',
  SelectClientLanguage = '/api/clientLanguage/Select',
}

// 增加语言管理
export const addClientLanguage = (params?: any) =>
	request({
		url: Api.AddClientLanguage,
		method: 'post',
		data: params,
	});

// 删除语言管理
export const deleteClientLanguage = (params?: any) =>
	request({
			url: Api.DeleteClientLanguage,
			method: 'post',
			data: params,
		});

// 编辑语言管理
export const updateClientLanguage = (params?: any) =>
	request({
			url: Api.UpdateClientLanguage,
			method: 'post',
			data: params,
		});

// 分页查询语言管理
export const pageClientLanguage = (params?: any) =>
	request({
			url: Api.PageClientLanguage,
			method: 'post',
			data: params,
		});
export const selectClientLanguage = (params?: any) =>
	request({
		url: Api.SelectClientLanguage,
		method: 'post',
		data: params,
	});


